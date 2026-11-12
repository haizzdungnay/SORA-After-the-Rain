#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;

namespace Sora.AfterTheRain.Demo.Editor
{
    [InitializeOnLoad]
    public static class SoraWeatherReview
    {
        private const string Key = "SoraWeatherReview20261112";
        private const string Folder = "Artifacts/SaltlineWeather";
        private static double began;
        private static Report report;

        static SoraWeatherReview()
        {
            if (Environment.GetCommandLineArgs().Contains("-soraWeatherReview")) EditorApplication.update += Tick;
        }

        private static void Require(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
        }

        private static object Call(SoraDemoCore core, string method, params object[] args) =>
            typeof(SoraDemoCore).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(core, args);

        private static void Tick()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            try
            {
                int step = SessionState.GetInt(Key, 0);
                if (step == 0)
                {
                    Directory.CreateDirectory(Folder);
                    EditorSceneManager.OpenScene(SoraDevAutomation.DemoScene);
                    SessionState.SetInt(Key, 1);
                    EditorApplication.EnterPlaymode();
                    return;
                }
                if (!EditorApplication.isPlaying) return;
                if (step == 1)
                {
                    began = EditorApplication.timeSinceStartup;
                    SessionState.SetInt(Key, 2);
                    return;
                }
                var core = SoraDemoCore.Instance;
                var weather = UnityEngine.Object.FindFirstObjectByType<SoraWeather>();
                if (step == 2)
                {
                    if (EditorApplication.timeSinceStartup - began < 8) return;
                    Require(core != null && weather != null && Camera.main != null, "Playable weather world absent");
                    core.SetModelSelectionOpen(false);
                    foreach (var twin in core.Characters) twin.enabled = false;
                    Check(core, weather);
                    Camera.main.GetComponent<SoraThirdPersonCamera>().enabled = false;
                    Place(core.ActiveCharacter, new Vector3(-2.4f, 0, -10.5f));
                    Camera.main.transform.position = new Vector3(3.7f, 1.35f, -16.5f);
                    Camera.main.transform.LookAt(new Vector3(-.5f, 1.25f, -4.3f));
                    Camera.main.fieldOfView = 65;
                    Set(weather, 0, 0, 0);
                    Next(3);
                }
                else if (step >= 3 && EditorApplication.timeSinceStartup - began > 5)
                {
                    if (step == 3)
                    {
                        Capture("dry-front");
                        Set(weather, 1, 1, .95f);
                        Next(4);
                    }
                    else if (step == 4)
                    {
                        Require(weather.Surfaces.GroundTiles > 0, "No outdoor wet-ground tiles");
                        int wetPoints = 0;
                        for (int x = -6; x <= 6; x++)
                            for (int z = -16; z <= -9; z++)
                                if (weather.Surfaces.IsPuddle(new Vector3(x, 0, z))) wetPoints++;
                        Require(wetPoints > 0, "Wet shader/footstep query has no puddles on the street");
                        report.puddleQueryPoints = wetPoints;
                        Require(!ShaderUtil.ShaderHasError(Resources.Load<Material>("Weather/Puddle").shader), "Puddle shader failed");
                        Capture("rain-front");
                        Camera.main.transform.position = new Vector3(-.5f, .85f, -13.5f);
                        Camera.main.transform.LookAt(new Vector3(.8f, .5f, -6.3f));
                        Next(5);
                    }
                    else if (step == 5)
                    {
                        Capture("puddle-closeup");
                        Camera.main.transform.position = new Vector3(1.3f, 1.9f, -.6f);
                        Camera.main.transform.LookAt(new Vector3(0, 1.25f, -8.2f));
                        Place(core.ActiveCharacter, new Vector3(0, 0, -.5f));
                        Next(6);
                    }
                    else if (step == 6)
                    {
                        Require(weather.Shelter > .9f, "Interior rain audio did not become sheltered");
                        Require(!SoraWetSurfaces.TryGround(new Vector3(0, 0, 0), out _), "Interior received wet surface");
                        Capture("sheltered-interior");
                        Camera.main.transform.position = new Vector3(3.7f, 1.35f, -16.5f);
                        Camera.main.transform.LookAt(new Vector3(-.5f, 1.25f, -4.3f));
                        Place(core.ActiveCharacter, new Vector3(-2.4f, 0, -10.5f));
                        Set(weather, 0, .9f, .9f);
                        Next(7);
                    }
                    else
                    {
                        Require(weather.State.puddles > .85f && weather.State.rain == 0, "After-rain puddles vanished");
                        Capture("after-rain");
                        report.groundTiles = weather.Surfaces.GroundTiles;
                        report.rainParticles = weather.Rain.particleCount;
                        report.splashBudget = weather.Surfaces.Splash.main.maxParticles;
                        report.drawCalls = UnityStats.drawCalls;
                        report.triangles = UnityStats.triangles;
                        report.allocatedMemoryBytes = Profiler.GetTotalAllocatedMemoryLong();
                        File.WriteAllText(Folder + "/report.json", JsonUtility.ToJson(report, true));
                        Debug.Log("SORA WEATHER ACCEPTANCE: " + JsonUtility.ToJson(report));
                        SessionState.EraseInt(Key);
                        EditorApplication.Exit(0);
                    }
                }
            }
            catch (Exception error)
            {
                Directory.CreateDirectory(Folder);
                File.WriteAllText(Folder + "/failure.txt", error.ToString());
                Debug.LogException(error);
                SessionState.EraseInt(Key);
                EditorApplication.Exit(1);
            }
        }

        private static void Next(int step)
        {
            began = EditorApplication.timeSinceStartup;
            SessionState.SetInt(Key, step);
        }

        private static void Set(SoraWeather weather, float rain, float wet, float puddles)
        {
            weather.Restore(new SoraWeatherState { kind = rain > .5f ? SoraWeatherKind.HeavyRain : SoraWeatherKind.Clear,
                rain = rain, wetness = wet, puddles = puddles, automaticCycle = false });
        }

        private static void Place(SoraDemoCharacter twin, Vector3 position)
        {
            var controller = twin.GetComponent<CharacterController>();
            controller.enabled = false;
            twin.transform.position = position;
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static void Check(SoraDemoCore core, SoraWeather weather)
        {
            Require(core.Characters.Count == 2 && UnityEngine.Object.FindObjectsByType<SoraWeather>(FindObjectsSortMode.None).Length == 1,
                "Duplicate/missing shared weather");
            Require(UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length == 1, "Duplicate listener");
            Require(weather.GetComponents<AudioSource>().Length == 3 && weather.GetComponents<AudioSource>().All(a => a.clip != null),
                "Weather audio clips/loop sources missing");
            var starts = new[] { new Vector3(0, 0, -7.6f), new Vector3(0, 0, -3),
                new Vector3(-4.2f, 0, -.6f), new Vector3(3.9f, 0, -3) };
            var ends = new[] { new Vector3(0, 0, -3), new Vector3(0, 0, 4.3f),
                new Vector3(-4.2f, 0, 4.2f), new Vector3(3.9f, 0, 2.3f) };
            foreach (var twin in core.Characters)
            {
                for (int route = 0; route < starts.Length; route++)
                {
                    Place(twin, starts[route]);
                    var controller = twin.GetComponent<CharacterController>();
                    Vector3 delta = (ends[route] - starts[route]) / 100;
                    for (int i = 0; i < 100; i++) controller.Move(delta + Vector3.down * .015f);
                    Require(Vector2.Distance(new Vector2(twin.transform.position.x, twin.transform.position.z),
                        new Vector2(ends[route].x, ends[route].z)) < .25f, "Wet surfaces blocked route " + route);
                }
                Place(twin, new Vector3(twin == core.Characters[0] ? -1 : 1.2f, 0, -2.5f));
            }
            var register = core.Interactables.Single(t => t.kind == InteractKind.Register);
            int cash = core.State.storeCash, stock = core.State.shelfStock, sales = core.State.sales;
            var state = weather.State;
            Call(core, "Interact", register);
            Call(core, "SwitchCharacter");
            Require(!core.HasPendingSale && core.State.storeCash == cash, "Draft charged on swap");
            Call(core, "Interact", register); Call(core, "Interact", register);
            for (int i = 0; i < 100; i++) Call(core, "SwitchCharacter");
            Require(ReferenceEquals(state, weather.State), "Twin swap replaced shared weather");
            Require(core.State.storeCash == cash + 380 && core.State.shelfStock == stock - 1 && core.State.sales == sales + 1, "POS regression");
            Call(core, "Interact", core.Interactables.Single(t => t.kind == InteractKind.Delivery));
            Call(core, "Interact", core.Interactables.Single(t => t.kind == InteractKind.Shelf));
            Require(core.State.backStock == 3 && core.State.shelfStock == stock + 2, "Stock regression");
            var save = File.Exists(core.SavePath) ? File.ReadAllBytes(core.SavePath) : null;
            try
            {
                weather.State.puddles = .83f;
                core.Save();
                weather.State.puddles = .05f; core.State.storeCash = 0;
                core.Load();
                Require(Mathf.Abs(weather.State.puddles - .83f) < .001f && core.State.storeCash == cash + 380, "Weather/store save-load failed");
                var malformed = JsonUtility.FromJson<SoraSaveData>(JsonUtility.ToJson(core.State));
                malformed.weather.rain = float.NaN;
                var before = core.State;
                Require(!(bool)Call(core, "RestoreSave", malformed) && ReferenceEquals(before, core.State), "Invalid weather replaced store state");
                var legacy = JsonUtility.FromJson<SoraSaveData>(JsonUtility.ToJson(core.State));
                legacy.weather = null;
                legacy.hasWeatherSnapshot = false;
                Require((bool)Call(core, "RestoreSave", legacy) && core.State.storeCash == cash + 380, "Legacy schema-1 save failed");
            }
            finally
            {
                if (save == null) File.Delete(core.SavePath); else File.WriteAllBytes(core.SavePath, save);
            }
            var rain = weather.Rain;
            Require(rain.collision.enabled && rain.collision.lifetimeLoss.constant == 1, "Rain roof collisions disabled");
            Require(weather.IsSheltered(new Vector3(0, 1, 0)) && weather.IsSheltered(new Vector3(0, 1, -6.3f)), "Roof/canopy shelter query failed");
            Require(!weather.IsSheltered(new Vector3(0, 1, -10)), "Street considered indoors");
            var emission = rain.emission; emission.enabled = false;
            var probes = new[] { new Vector3(0, 6, 0), new Vector3(0, 5, -6.3f), new Vector3(0, 6, -9) };
            for (int repeat = 0; repeat < 3; repeat++)
                for (int p = 0; p < probes.Length; p++)
                {
                    rain.Clear();
                    rain.SetParticles(new[] { new ParticleSystem.Particle { position = probes[p], velocity = Vector3.down * 10,
                        startLifetime = 2, remainingLifetime = 2, startSize = .03f } }, 1);
                    rain.Simulate(.4f, false, false, true);
                    Require(rain.particleCount == (p == 2 ? 1 : 0), "Roof rain probe failed: " + p);
                }
            rain.Clear(); emission.enabled = true; rain.Play();
            report = new Report { unity = Application.unityVersion, switches = 100, roofProbes = 9, walkthroughs = 8,
                weatherSaveLoad = true, oldSaveAccepted = true, malformedSaveRejected = true,
                privateCity = GameObject.Find("V03 AFX + CC0 city buildings [OWNER PRIVATE REAL GLB]") != null };
        }

        private static void Capture(string name)
        {
            var camera = Camera.main;
            var target = new RenderTexture(1600, 900, 24);
            var image = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            var previousTarget = camera.targetTexture;
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); image.Apply();
                File.WriteAllBytes(Folder + "/" + name + ".png", image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget; RenderTexture.active = previous;
                UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(image);
            }
        }

        [Serializable]
        private class Report
        {
            public string unity;
            public int switches, roofProbes, groundTiles, rainParticles, splashBudget, drawCalls, triangles, walkthroughs, puddleQueryPoints;
            public long allocatedMemoryBytes;
            public bool weatherSaveLoad, oldSaveAccepted, malformedSaveRejected, privateCity;
        }
    }
}
#endif
