using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sora.AfterTheRain.Demo
{
    public enum SoraWeatherKind { Clear, Drizzle, Rain, HeavyRain }

    [Serializable]
    public sealed class SoraWeatherState
    {
        public SoraWeatherKind kind = SoraWeatherKind.Rain;
        public float rain = .65f, wetness = .7f, puddles = .55f, remaining = 180;
        public bool automaticCycle = true;

        private static bool Range(float value, float max) => !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0 && value <= max;
        public bool IsValid() => Enum.IsDefined(typeof(SoraWeatherKind), kind) && Range(rain, 1) &&
            Range(wetness, 1) && Range(puddles, 1) && Range(remaining, 3600);

        public SoraWeatherState Copy() => (SoraWeatherState)MemberwiseClone();

        public void SetWeather(SoraWeatherKind next)
        {
            if (!Enum.IsDefined(typeof(SoraWeatherKind), next)) throw new ArgumentOutOfRangeException(nameof(next));
            kind = next;
            remaining = next == SoraWeatherKind.HeavyRain ? 75 : next == SoraWeatherKind.Rain ? 180 : 120;
        }

        public void Advance(float seconds, bool cycle = true)
        {
            if (!Range(seconds, 3600)) throw new ArgumentOutOfRangeException(nameof(seconds));
            // ponytail: four-stage visual cycle; use authored forecasts when shift time owns weather.
            if (cycle && automaticCycle && (remaining -= seconds) <= 0) SetWeather((SoraWeatherKind)(((int)kind + 1) % 4));
            float target = kind == SoraWeatherKind.Clear ? 0 : kind == SoraWeatherKind.Drizzle ? .22f : kind == SoraWeatherKind.Rain ? .65f : 1;
            rain = Mathf.MoveTowards(rain, target, seconds / 20);
            wetness = Mathf.MoveTowards(wetness, rain > .01f ? Mathf.Clamp01(.25f + rain) : 0, seconds * (rain > .01f ? .018f : .0024f));
            puddles = Mathf.Clamp01(puddles + seconds * (rain > .1f && wetness > .45f ? rain * .006f : -.0016f));
        }
    }

    public sealed class SoraWeather : MonoBehaviour
    {
        public SoraWeatherState State { get; private set; } = new SoraWeatherState();
        public float Shelter { get; private set; }
        public SoraWetSurfaces Surfaces { get; private set; }
        public ParticleSystem Rain { get; private set; }
        private SoraDemoCore core;
        private AudioSource outside, inside, wind;
        private AudioClip[] thunder;
        private readonly List<ParticleCollisionEvent> collisions = new List<ParticleCollisionEvent>(128);
        private Material rainMaterial;
        private float shelterAt, thunderAt = 18;
        private Vector3 lastFocus;
        private bool showControls;
        private int splashFrame, splashes;
        private float oldFogDensity;

        public static SoraWeather Create(Transform world, SoraDemoCore core, ParticleSystem rain)
        {
            var existing = world.GetComponentInChildren<SoraWeather>();
            if (existing != null) return existing;
            var weather = rain.gameObject.AddComponent<SoraWeather>();
            weather.core = core;
            weather.Rain = rain;
            weather.oldFogDensity = RenderSettings.fogDensity;
            var main = rain.main;
            main.startLifetime = 1.6f;
            main.startSpeed = 12;
            main.gravityModifier = .1f;
            main.startSize = .025f;
            main.startColor = new Color(.78f, .86f, 1, .5f);
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            var shape = rain.shape;
            shape.rotation = new Vector3(90, 0, 0);
            shape.scale = new Vector3(32, 32, .5f);
            var collision = rain.collision;
            collision.sendCollisionMessages = true;
            var renderer = rain.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 2;
            renderer.velocityScale = .018f;
            renderer.cameraVelocityScale = 0;
            var source = Resources.Load<Material>("Weather/Rain");
            if (source != null)
            {
                var previous = renderer.sharedMaterial;
                renderer.sharedMaterial = weather.rainMaterial = new Material(source);
                if (previous != null) Destroy(previous);
            }
            var surfaces = new GameObject("SORA Wet Ground and Puddles");
            surfaces.transform.SetParent(world, false);
            weather.Surfaces = surfaces.AddComponent<SoraWetSurfaces>();
            weather.Surfaces.Initialize(core, weather);
            weather.outside = weather.Loop("rain");
            weather.inside = weather.Loop("rain_interior");
            weather.wind = weather.Loop("wind");
            weather.thunder = new[] { Resources.Load<AudioClip>("Weather/thunder-v0"), Resources.Load<AudioClip>("Weather/thunder-v1") };
            var profile = Resources.Load<VolumeProfile>("Weather/NightVolume");
            if (profile != null && Camera.main != null)
            {
                var volume = weather.gameObject.AddComponent<Volume>();
                volume.isGlobal = true;
                volume.priority = 1;
                volume.sharedProfile = profile;
                Camera.main.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            }
            Debug.Log("SORA WEATHER: shared rain, wet ground, puddles and sheltered audio initialized.");
            return weather;
        }

        private AudioSource Loop(string name)
        {
            var audio = gameObject.AddComponent<AudioSource>();
            audio.clip = Resources.Load<AudioClip>("Weather/" + name + "-v0");
            audio.loop = true;
            audio.playOnAwake = false;
            audio.volume = 0;
            audio.spatialBlend = 0;
            if (audio.clip != null) audio.Play();
            return audio;
        }

        public void Restore(SoraWeatherState snapshot)
        {
            if (snapshot == null) return;
            if (!snapshot.IsValid()) throw new ArgumentException("Invalid weather snapshot", nameof(snapshot));
            State = snapshot.Copy();
        }

        public bool IsSheltered(Vector3 position) => Physics.Raycast(position + Vector3.up * .25f,
            Vector3.up, 40, 1, QueryTriggerInteraction.Ignore);

        private void Update()
        {
            if (core == null || Rain == null) return;
            if (Input.GetKeyDown(KeyCode.F8)) showControls = !showControls;
            State.Advance(Mathf.Min(Time.deltaTime, .25f));
            var emission = Rain.emission;
            emission.rateOverTime = State.rain * 1800;
            Shader.SetGlobalFloat("_SoraRain", State.rain);
            Shader.SetGlobalFloat("_SoraWetness", State.wetness);
            Shader.SetGlobalFloat("_SoraPuddles", State.puddles);
            RenderSettings.fogDensity = Mathf.Lerp(.006f, .013f, State.rain);
            if (core.ActiveCharacter == null) return;
            Vector3 focus = core.ActiveCharacter.transform.position;
            if ((focus - lastFocus).sqrMagnitude > 144) Rain.Clear();
            lastFocus = focus;
            if (Time.time >= shelterAt)
            {
                float target = IsSheltered(focus + Vector3.up) ? 1 : 0;
                Shelter = Mathf.MoveTowards(Shelter, target, .12f);
                shelterAt = Time.time + .08f;
            }
            outside.volume = .3f * State.rain * Mathf.Sqrt(1 - Shelter);
            inside.volume = .22f * State.rain * Mathf.Sqrt(Shelter);
            wind.volume = .055f * Mathf.Lerp(.25f, 1, State.rain) * Mathf.Lerp(1, .15f, Shelter);
            if (Time.time >= thunderAt)
            {
                thunderAt = Time.time + UnityEngine.Random.Range(16f, 34f);
                var clip = thunder[UnityEngine.Random.Range(0, thunder.Length)];
                if (State.kind == SoraWeatherKind.HeavyRain && clip != null) (Shelter > .5f ? inside : outside).PlayOneShot(clip, .55f);
            }
        }

        private void OnParticleCollision(GameObject other)
        {
            if (Surfaces == null || core == null || core.ActiveCharacter == null || State.rain < .03f) return;
            if (splashFrame != Time.frameCount) { splashFrame = Time.frameCount; splashes = 0; }
            if (splashes >= 4) return;
            int count = Rain.GetCollisionEvents(other, collisions);
            for (int i = 0; i < count && splashes < 4; i++)
                if ((collisions[i].intersection - core.ActiveCharacter.transform.position).sqrMagnitude < 144 &&
                    Surfaces.EmitSplash(collisions[i].intersection)) splashes++;
        }

        private void OnGUI()
        {
            if (!showControls || core == null) return;
            GUILayout.BeginArea(new Rect(Mathf.Max(8, Screen.width - 278), 12, 266, 240), GUI.skin.box);
            GUILayout.Label("THOI TIET [F8]  |  " + State.kind);
            foreach (SoraWeatherKind kind in Enum.GetValues(typeof(SoraWeatherKind)))
                if (GUILayout.Button(kind.ToString())) { State.automaticCycle = false; State.SetWeather(kind); }
            State.automaticCycle = GUILayout.Toggle(State.automaticCycle, "Tu dong doi thoi tiet");
            GUILayout.Label("Mua " + State.rain.ToString("P0") + " | Am " + State.wetness.ToString("P0") + " | Vung " + State.puddles.ToString("P0"));
            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            if (rainMaterial != null) Destroy(rainMaterial);
            Shader.SetGlobalFloat("_SoraRain", 0);
            Shader.SetGlobalFloat("_SoraWetness", 0);
            Shader.SetGlobalFloat("_SoraPuddles", 0);
            if (core != null) RenderSettings.fogDensity = oldFogDensity;
        }
    }
}
