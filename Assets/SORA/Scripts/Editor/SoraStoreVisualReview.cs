#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sora.AfterTheRain.Demo.Editor
{
    // Fixed review cameras, not gameplay-camera acceptance. Never reads or writes player saves.
    public static class SoraStoreVisualReview
    {
        private const string Key = "SoraStoreVisualReview";
        private static readonly string[] Args = Environment.GetCommandLineArgs();
        private static readonly string Folder = OutputFolder();
        private static readonly Vector3[] Positions = {
            new Vector3(0, 1.65f, -13.5f), new Vector3(.2f, 1.7f, -4.6f),
            new Vector3(-1.8f, 1.7f, -4.6f), new Vector3(-.5f, 1.1f, -12) };
        private static readonly Vector3[] Targets = {
            new Vector3(0, 1.65f, -5.5f), new Vector3(.1f, 1.45f, 2.8f),
            new Vector3(-4.85f, 1.3f, -2.5f), new Vector3(.8f, 1.1f, -7.3f) };
        private static readonly float[] Fovs = { 58, 60, 50, 60 };
        private static int shot = -1, nextFrame, lastFrame = -1;
        private static double started;
        private static SoraDemoCore core;
        private static SoraWeather weather;
        private static bool ownsOutput;

        private static string OutputFolder()
        {
            int index = Array.IndexOf(Args, "-soraVisualOutput");
            return index >= 0 && index + 1 < Args.Length ? Path.GetFullPath(Args[index + 1]) :
                "Artifacts/VisualStyleAcceptance/capture";
        }

        [InitializeOnLoadMethod]
        private static void Bootstrap()
        {
            if (!Args.Contains("-soraVisualReview")) return;
            ownsOutput = SessionState.GetBool(Key, false);
            started = EditorApplication.timeSinceStartup;
            Application.logMessageReceived += Log;
            EditorApplication.update += Tick;
        }

        private static void Log(string message, string stack, LogType type)
        {
            // Unity 6000.3 Search startup can throw in a fresh worktree; retain evidence separately.
            if (stack.Contains("UnityEditor.Search.SearchDatabase") && !stack.Contains("Sora.AfterTheRain.Demo"))
            {
                if (ownsOutput)
                    File.AppendAllText(Path.Combine(Folder, "editor-search.txt"), message + "\n" + stack + "\n");
                return;
            }
            if (EditorApplication.isPlaying && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert))
                Fail(new InvalidOperationException(message + "\n" + stack));
        }

        private static void Tick()
        {
            try
            {
                if (EditorApplication.timeSinceStartup - started > 240) throw new TimeoutException("Visual capture timed out.");
                if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
                if (!EditorApplication.isPlaying)
                {
                    if (SessionState.GetBool(Key, false)) return;
                    Directory.CreateDirectory(Folder);
                    if (Directory.EnumerateFiles(Folder).Any(path => Path.GetExtension(path) == ".png" || Path.GetExtension(path) == ".json" || Path.GetFileName(path) == "failure.txt"))
                        throw new IOException("Choose a fresh capture folder; existing evidence must not be overwritten.");
                    ownsOutput = true;
                    if (Args.Contains("-soraVisualPrepare")) SoraWeatherSetup.Prepare();
                    EditorSceneManager.OpenScene(SoraDevAutomation.DemoScene);
                    SessionState.SetBool(Key, true);
                    EditorApplication.EnterPlaymode();
                    return;
                }
                if (lastFrame == Time.frameCount) return;
                lastFrame = Time.frameCount;
                Time.captureDeltaTime = 1f / 30;
                if (shot >= 0)
                    foreach (var twin in core.Characters)
                        twin.GetComponentInChildren<SoraCharacterMotion>().Tick(0, true, 0, false, 0, true, 1f / 30);
                if (Time.frameCount < 180 || Time.frameCount < nextFrame) return;
                if (shot < 0)
                {
                    core = SoraDemoCore.Instance;
                    weather = UnityEngine.Object.FindFirstObjectByType<SoraWeather>();
                    if (core == null || core.Characters.Count != 2 || weather == null || Camera.main == null)
                        throw new InvalidOperationException("Store, weather, camera and both twins required.");
                    core.SetModelSelectionOpen(false);
                    if (core.Characters[0].ModelId != "Zhongli" || core.Characters[1].ModelId != "Chisa")
                        throw new InvalidOperationException("Chisa/Zhongli baseline models required.");
                    core.enabled = false;
                    foreach (var twin in core.Characters)
                    {
                        twin.enabled = false;
                        twin.ResetMotion();
                        var animator = twin.GetComponentInChildren<Animator>();
                        animator.Play("Locomotion", 0, .25f);
                        animator.Update(0);
                        animator.speed = 0;
                    }
                    var boundary = UnityEngine.Object.FindFirstObjectByType<SoraCityBoundaryGuard>();
                    if (boundary != null) boundary.enabled = false;
                    Camera.main.GetComponent<SoraThirdPersonCamera>().enabled = false;
                    if (Args.Contains("-soraVisualFxaa"))
                        Camera.main.GetUniversalAdditionalCameraData().antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                    Time.captureDeltaTime = 1f / 30;
                    weather.Rain.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    weather.Rain.useAutoRandomSeed = false;
                    weather.Rain.randomSeed = 16112026;
                    weather.Rain.Play();
                    shot = 0;
                    SetShot();
                    return;
                }
                string id = "C" + (shot + 1).ToString("D2");
                SoraWeatherReview.Capture(id, 1920, 1080, Folder);
                WriteMetadata(id);
                if (++shot < Positions.Length) SetShot();
                else
                {
                    Debug.Log("SORA VISUAL REVIEW: four 1920x1080 camera captures with metadata complete.");
                    Finish(0);
                }
            }
            catch (Exception error) { Fail(error); }
        }

        private static void SetShot()
        {
            var camera = Camera.main;
            camera.fieldOfView = Fovs[shot];
            camera.transform.position = Positions[shot];
            camera.transform.LookAt(Targets[shot]);
            // Same environment and poses for every A/B. Only C04 explicitly carries an umbrella.
            weather.Restore(new SoraWeatherState { kind = SoraWeatherKind.HeavyRain, rain = 1,
                wetness = 1, puddles = .9f, automaticCycle = false });
            bool inside = shot == 1 || shot == 2;
            Place(core.Characters[0], shot == 1 ? new Vector3(-.8f, 0, -1) : inside ? new Vector3(-4.85f, 0, -1.35f) : new Vector3(-2.4f, 0, -6.9f), shot == 1 ? 0 : 180);
            Place(core.Characters[1], shot == 1 ? new Vector3(.7f, 0, .2f) : inside ? new Vector3(-3.25f, 0, -3.65f) : new Vector3(.8f, 0, -8.2f), shot == 1 ? 0 : inside ? 330 : 210);
            core.Characters[1].SetUmbrellaCarried(shot == 3);
            if (shot == 3)
            {
                var animator = core.Characters[1].GetComponentInChildren<Animator>();
                animator.Play("UmbrellaHold", animator.GetLayerIndex("Umbrella"), 0);
            }
            nextFrame = Time.frameCount + 90;
        }

        private static void Place(SoraDemoCharacter twin, Vector3 position, float yaw)
        {
            var controller = twin.GetComponent<CharacterController>();
            controller.enabled = false;
            twin.transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static void WriteMetadata(string id)
        {
            var profile = weather.GetComponent<Volume>().sharedProfile;
            profile.TryGet(out ColorAdjustments color);
            profile.TryGet(out Bloom bloom);
            profile.TryGet(out Tonemapping tone);
            var camera = Camera.main;
            var metadata = new CaptureMetadata {
                id = id, unity = Application.unityVersion, gpu = SystemInfo.graphicsDeviceName,
                graphicsApi = SystemInfo.graphicsDeviceType.ToString(), colorSpace = QualitySettings.activeColorSpace.ToString(),
                antiAliasing = camera.GetUniversalAdditionalCameraData().antialiasing.ToString(),
                position = camera.transform.position, rotation = camera.transform.eulerAngles, fov = camera.fieldOfView,
                width = 1920, height = 1080, frame = Time.frameCount, weather = weather.State.Copy(),
                ambientMode = RenderSettings.ambientMode.ToString(), sky = RenderSettings.ambientSkyColor,
                equator = RenderSettings.ambientEquatorColor, ground = RenderSettings.ambientGroundColor,
                fog = RenderSettings.fogColor, fogDensity = RenderSettings.fogDensity,
                exposure = color.postExposure.value, contrast = color.contrast.value, saturation = color.saturation.value,
                bloom = bloom.intensity.value, bloomThreshold = bloom.threshold.value, tonemapping = tone.mode.value.ToString(),
                privateCity = Resources.Load<GameObject>(SoraCityRuntimeWorld.PrivateV03Path) != null,
                models = core.Characters.Select(c => c.ModelId).ToArray(),
                characters = core.Characters.Select(c => c.transform.position).ToArray(),
                umbrellas = core.Characters.Select(c => c.HasUmbrella).ToArray(),
                lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Select(l => new Lamp {
                    name = l.name, position = l.transform.position, color = l.color, intensity = l.intensity, range = l.range }).ToArray()
            };
            File.WriteAllText(Path.Combine(Folder, id + ".json"), JsonUtility.ToJson(metadata, true));
        }

        private static void Fail(Exception error)
        {
            if (ownsOutput)
            {
                Directory.CreateDirectory(Folder);
                File.WriteAllText(Path.Combine(Folder, "failure.txt"), error.ToString());
            }
            Application.logMessageReceived -= Log;
            Debug.LogException(error);
            Finish(1);
        }

        private static void Finish(int code)
        {
            Time.captureDeltaTime = 0;
            SessionState.EraseBool(Key);
            EditorApplication.Exit(code);
        }

        [Serializable] private class Lamp { public string name; public Vector3 position; public Color color; public float intensity, range; }
        [Serializable] private class CaptureMetadata
        {
            public string id, unity, gpu, graphicsApi, colorSpace, antiAliasing, ambientMode, tonemapping;
            public string[] models;
            public Vector3 position, rotation;
            public Vector3[] characters;
            public bool[] umbrellas;
            public Color sky, equator, ground, fog;
            public float fov, fogDensity, exposure, contrast, saturation, bloom, bloomThreshold;
            public int width, height, frame;
            public bool privateCity;
            public SoraWeatherState weather;
            public Lamp[] lights;
        }
    }
}
#endif
