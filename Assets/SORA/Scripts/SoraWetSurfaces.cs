using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sora.AfterTheRain.Demo
{
    public sealed class SoraWetSurfaces : MonoBehaviour
    {
        private const int Side = 23;
        private const float Cell = 1.6f;
        private static readonly RaycastHit[] groundHits = new RaycastHit[32];
        public int GroundTiles { get; private set; }
        public ParticleSystem Splash { get; private set; }
        private SoraDemoCore core;
        private SoraWeather weather;
        private Mesh mesh;
        private Material surfaceMaterial, splashMaterial;
        private ReflectionProbe probe;
        private readonly List<Vector3> vertices = new List<Vector3>(Side * Side * 4);
        private readonly List<int> triangles = new List<int>(Side * Side * 6);
        private readonly List<Vector3> normals = new List<Vector3>(Side * Side * 4);
        private Vector3 anchor = new Vector3(float.MaxValue, 0, 0), buildingAt;
        private int cursor = Side * Side;
        private bool captured;
        private float nextProbe;
        private Texture2D noise;
        private AudioClip[] waterSteps, groundSteps, metalSteps;
        private readonly AudioSource[] feet = new AudioSource[2];
        private readonly Vector3[] lastStep = new Vector3[2];
        private readonly float[] stride = new float[2];

        public void Initialize(SoraDemoCore owner, SoraWeather controller)
        {
            core = owner;
            weather = controller;
            gameObject.layer = 4;
            mesh = new Mesh { name = "SORA outdoor wet surface patches" };
            mesh.MarkDynamic();
            gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = gameObject.AddComponent<MeshRenderer>();
            var material = Resources.Load<Material>("Weather/Puddle");
            if (material != null) renderer.sharedMaterial = surfaceMaterial = new Material(material);
            else { renderer.enabled = false; Debug.LogError("SORA WEATHER: Weather/Puddle material missing; run weather setup."); }
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
            noise = Resources.Load<Texture2D>("Weather/detail");
            waterSteps = Clips("step_water", 3);
            groundSteps = Clips("step_concrete", 4);
            metalSteps = Clips("step_metal", 3);
            for (int i = 0; i < owner.Characters.Count && i < feet.Length; i++)
            {
                feet[i] = owner.Characters[i].gameObject.AddComponent<AudioSource>();
                feet[i].playOnAwake = false;
                feet[i].spatialBlend = 1;
                feet[i].minDistance = 1;
                feet[i].maxDistance = 14;
                feet[i].dopplerLevel = 0;
                feet[i].volume = .18f;
                lastStep[i] = owner.Characters[i].transform.position;
            }
            var probeObject = new GameObject("SORA Storefront Weather Reflection");
            probeObject.transform.SetParent(transform, false);
            probeObject.transform.position = new Vector3(0, 1.8f, -8);
            probe = probeObject.AddComponent<ReflectionProbe>();
            probe.mode = ReflectionProbeMode.Realtime;
            probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
            probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
            probe.resolution = 128;
            probe.size = new Vector3(45, 18, 45);
            probe.boxProjection = true;
            probe.cullingMask = ~(1 << 4);
            probe.farClipPlane = 70;
            probe.clearFlags = ReflectionProbeClearFlags.SolidColor;
            probe.backgroundColor = new Color(.035f, .055f, .09f);
            var splashObject = new GameObject("SORA Rain Ground Splashes");
            splashObject.transform.SetParent(transform, false);
            Splash = splashObject.AddComponent<ParticleSystem>();
            Splash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = Splash.main;
            main.loop = false;
            main.playOnAwake = false;
            main.maxParticles = 128;
            main.startLifetime = .28f;
            main.startSpeed = .35f;
            main.startSize = new ParticleSystem.MinMaxCurve(.06f, .13f);
            main.startColor = new Color(.68f, .78f, .9f, .5f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = .15f;
            var fade = Splash.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) });
            fade.color = gradient;
            var emission = Splash.emission;
            emission.enabled = false;
            var shape = Splash.shape;
            shape.enabled = false;
            var splashRenderer = splashObject.GetComponent<ParticleSystemRenderer>();
            var splashSource = Resources.Load<Material>("Weather/Splash");
            if (splashSource != null) splashRenderer.sharedMaterial = splashMaterial = new Material(splashSource);
            splashRenderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        private static AudioClip[] Clips(string name, int count)
        {
            var clips = new AudioClip[count];
            for (int i = 0; i < count; i++) clips[i] = Resources.Load<AudioClip>("Weather/" + name + "-v" + i);
            return clips;
        }

        public bool IsPuddle(Vector3 position)
        {
            if (noise == null || !noise.isReadable || weather.State.puddles < .02f || !TryGround(position, out _)) return false;
            float sample = noise.GetPixelBilinear(position.x * .065f, position.z * .065f).r * .7f +
                noise.GetPixelBilinear(position.x * .19f + .37f, position.z * .19f + .37f).r * .3f;
            return sample > .67f - weather.State.puddles * .18f;
        }

        private void Footsteps()
        {
            // ponytail: distance-based contacts; use Animator Events when every avatar exposes foot markers.
            for (int i = 0; i < core.Characters.Count && i < feet.Length; i++)
            {
                var twin = core.Characters[i];
                Vector3 position = twin.transform.position;
                Vector3 delta = position - lastStep[i];
                lastStep[i] = position;
                delta.y = 0;
                if (delta.sqrMagnitude > 25) { stride[i] = 0; continue; }
                if (!twin.GetComponent<CharacterController>().isGrounded) continue;
                stride[i] += delta.magnitude;
                if (stride[i] < 1.15f) continue;
                stride[i] %= 1.15f;
                bool water = IsPuddle(position);
                bool metal = Physics.Raycast(position + Vector3.up * .3f, Vector3.down, out var hit, 1, 1,
                    QueryTriggerInteraction.Ignore) && hit.collider.name.IndexOf("metal", StringComparison.OrdinalIgnoreCase) >= 0;
                var clips = water ? waterSteps : metal ? metalSteps : groundSteps;
                var clip = clips[UnityEngine.Random.Range(0, clips.Length)];
                if (clip != null && feet[i] != null) feet[i].PlayOneShot(clip);
                if (water) EmitSplash(position);
            }
        }

        public static bool TryGround(Vector3 near, out RaycastHit hit)
        {
            hit = default;
            int count = Physics.RaycastNonAlloc(new Vector3(near.x, near.y + 40, near.z), Vector3.down,
                groundHits, 44, 1, QueryTriggerInteraction.Ignore);
            if (count == groundHits.Length) return false;
            float closest = float.MaxValue;
            for (int i = 0; i < count; i++)
                if (!(groundHits[i].collider is CharacterController) && groundHits[i].distance < closest)
                {
                    hit = groundHits[i];
                    closest = hit.distance;
                }
            if (hit.collider == null || hit.normal.y < .98f || Mathf.Abs(hit.point.y - near.y) > .8f) return false;
            string name = hit.collider.name;
            return name.StartsWith("V03/ROAD_PBR/", StringComparison.Ordinal) || name.StartsWith("V03/SIDEWALK", StringComparison.Ordinal) ||
                name.StartsWith("ROAD/", StringComparison.Ordinal) || name.StartsWith("SIDEWALK/", StringComparison.Ordinal) ||
                name.StartsWith("V04/GROUND/", StringComparison.Ordinal) || name.StartsWith("V04/ENV/", StringComparison.Ordinal) ||
                name.StartsWith("OUTER_PATH/", StringComparison.Ordinal) || name.StartsWith("BLOCK_LAND/", StringComparison.Ordinal) ||
                name.IndexOf("asphalt", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("sidewalk", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void LateUpdate()
        {
            if (core == null || core.ActiveCharacter == null) return;
            Footsteps();
            Vector3 focus = core.ActiveCharacter.transform.position;
            Vector3 next = new Vector3(Mathf.Floor(focus.x / (Cell * 4)) * Cell * 4, focus.y,
                Mathf.Floor(focus.z / (Cell * 4)) * Cell * 4);
            Shader.SetGlobalVector("_SoraWetCenter", new Vector4(focus.x, focus.z, 16, 0));
            if (cursor >= Side * Side && (next - anchor).sqrMagnitude > Cell * Cell)
            {
                anchor = buildingAt = next;
                vertices.Clear(); triangles.Clear(); normals.Clear();
                cursor = 0;
            }
            if (cursor < Side * Side)
            {
                int end = Mathf.Min(cursor + 16, Side * Side);
                for (; cursor < end; cursor++) AddTile(cursor);
                if (cursor == Side * Side)
                {
                    mesh.Clear(); mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetTriangles(triangles, 0);
                    mesh.RecalculateBounds();
                    GroundTiles = vertices.Count / 4;
                }
            }
            if (Application.isPlaying && cursor == Side * Side && (!captured || Time.time > nextProbe &&
                (probe.transform.position - focus).sqrMagnitude > 225 && !weather.IsSheltered(focus + Vector3.up)))
            {
                if (captured) probe.transform.position = focus + Vector3.up * 1.8f;
                probe.RenderProbe();
                captured = true;
                nextProbe = Time.time + 15;
            }
        }

        private void AddTile(int index)
        {
            Vector3 at = buildingAt + new Vector3((index % Side - Side / 2) * Cell, 0, (index / Side - Side / 2) * Cell);
            if (!TryGround(at, out var hit)) return;
            float half = Cell * .5f;
            Vector3 a = at + new Vector3(-half, 0, -half), b = at + new Vector3(-half, 0, half);
            Vector3 c = at + new Vector3(half, 0, half), d = at + new Vector3(half, 0, -half);
            if (!TryGround(a, out var ha) || !TryGround(b, out var hb) || !TryGround(c, out var hc) || !TryGround(d, out var hd) ||
                Mathf.Abs(ha.point.y - hit.point.y) > .06f || Mathf.Abs(hb.point.y - hit.point.y) > .06f ||
                Mathf.Abs(hc.point.y - hit.point.y) > .06f || Mathf.Abs(hd.point.y - hit.point.y) > .06f) return;
            int start = vertices.Count;
            vertices.Add(ha.point + Vector3.up * .012f); vertices.Add(hb.point + Vector3.up * .012f);
            vertices.Add(hc.point + Vector3.up * .012f); vertices.Add(hd.point + Vector3.up * .012f);
            for (int i = 0; i < 4; i++) normals.Add(Vector3.up);
            triangles.Add(start); triangles.Add(start + 1); triangles.Add(start + 2);
            triangles.Add(start); triangles.Add(start + 2); triangles.Add(start + 3);
        }

        public bool EmitSplash(Vector3 position)
        {
            if (Splash == null || weather == null || weather.State.wetness < .15f || !TryGround(position, out var hit)) return false;
            if (!Splash.isPlaying) Splash.Play();
            Splash.Emit(new ParticleSystem.EmitParams { position = hit.point + Vector3.up * .035f,
                velocity = Vector3.up * .3f, startLifetime = .25f }, 1);
            return true;
        }

        private void OnDestroy()
        {
            if (mesh != null) Destroy(mesh);
            if (surfaceMaterial != null) Destroy(surfaceMaterial);
            if (splashMaterial != null) Destroy(splashMaterial);
        }
    }
}
