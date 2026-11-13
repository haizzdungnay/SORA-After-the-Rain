#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sora.AfterTheRain.Demo.Editor
{
    public sealed class SoraCharacterMotionReview
    {
        const string Folder = "Artifacts/CharacterMotion/PlayMode";
        static readonly Vector3 Stage = new Vector3(500, 20, 500);
        private static SoraCharacterMotionReview instance;
        private readonly Stack<IEnumerator> tasks = new Stack<IEnumerator>();
        private float resumeAt;
        private int lastFrame = -1;
        private SoraDemoCore core;
        private SoraDemoCharacter focus;
        private Vector3 direction;
        private float speed, pivot, startedAt;
        private bool jump, running;
        private bool captureVideo;
        private int videoFrames;
        private readonly List<string> checks = new List<string>();
        private readonly List<ContactSample> contacts = new List<ContactSample>();
        private readonly List<string> errors = new List<string>();

        [InitializeOnLoadMethod]
        static void Bootstrap()
        {
            if (!Environment.GetCommandLineArgs().Contains("-soraMotionReview")) return;
            EditorApplication.update += StartReview;
        }

        static void StartReview()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (!EditorApplication.isPlaying)
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    EditorSceneManager.OpenScene(SoraDevAutomation.DemoScene);
                    EditorApplication.EnterPlaymode();
                }
                return;
            }
            var core = SoraDemoCore.Instance;
            if (core == null || core.Characters.Count != 2) return;
            EditorApplication.update -= StartReview;
            instance = new SoraCharacterMotionReview();
            instance.tasks.Push(instance.Start());
            EditorApplication.update += RunTasks;
        }

        private static void RunTasks()
        {
            if (!EditorApplication.isPlaying || instance.lastFrame == Time.frameCount) return;
            instance.lastFrame = Time.frameCount;
            try
            {
                instance.Update();
                if (instance.captureVideo && instance.running && instance.focus != null && instance.focus.ModelId == "Chisa" && Time.frameCount % 2 == 0)
                    instance.Capture("Frames/" + (instance.videoFrames++).ToString("D4"));
                if (Time.time < instance.resumeAt) return;
                while (instance.tasks.Count > 0)
                {
                    var task = instance.tasks.Peek();
                    if (!task.MoveNext()) { instance.tasks.Pop(); continue; }
                    if (task.Current is IEnumerator nested) { instance.tasks.Push(nested); continue; }
                    instance.resumeAt = Time.time + (task.Current is float seconds ? seconds : 0);
                    return;
                }
            }
            catch (Exception error)
            {
                File.WriteAllText(Folder + "/failure.txt", error.ToString());
                Debug.LogException(error);
                instance.WriteReport();
                EditorApplication.Exit(1);
            }
        }

        private IEnumerator Start()
        {
            Directory.CreateDirectory(Folder);
            captureVideo = Environment.GetCommandLineArgs().Contains("-soraMotionVideo");
            if (captureVideo) Directory.CreateDirectory(Folder + "/Frames");
            Application.logMessageReceived += Log;
            startedAt = Time.realtimeSinceStartup;
            if (File.Exists(Folder + "/failure.txt")) File.Delete(Folder + "/failure.txt");
            core = SoraDemoCore.Instance;
            core.SetModelSelectionOpen(false);
            var boundary = Object.FindFirstObjectByType<SoraCityBoundaryGuard>();
            if (boundary != null) boundary.enabled = false;
            Time.captureDeltaTime = 1f / 30;
            foreach (var twin in core.Characters) twin.enabled = false;
            Camera.main.GetComponent<SoraThirdPersonCamera>().enabled = false;
            Box("Motion review floor", Stage + Vector3.down * .5f, new Vector3(30, 1, 30));
            var ramp = Box("Motion review ramp", Stage + new Vector3(4, .4f, 0), new Vector3(3, .2f, 4));
            ramp.transform.rotation = Quaternion.Euler(10, 0, 0);
            Box("Motion review step", Stage + new Vector3(-4, .08f, 0), new Vector3(2, .16f, 3));
            var key = new GameObject("Motion review key").AddComponent<Light>();
            key.type = LightType.Directional;
            key.intensity = 2.5f;
            key.shadows = LightShadows.Soft;
            key.transform.rotation = Quaternion.Euler(40, -35, 0);
            key.cullingMask = 1 << 30;
            Camera.main.cullingMask = 1 << 30;
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = new Color(.085f, .12f, .145f);
            Camera.main.farClipPlane = 25;
            RenderSettings.fog = false;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.55f, .6f, .65f);
            running = true;
            yield return null;
            foreach (var model in SoraCharacterModels.All)
                yield return ReviewModel(model);
            direction = Vector3.zero; speed = pivot = 0;
            yield return WetContacts();
            CheckStoreState();
            WriteReport();
            EditorApplication.Exit(errors.Count == 0 ? 0 : 1);
        }

        private IEnumerator ReviewModel(SoraCharacterModels.Model model)
        {
            Require(core.SelectCharacterModel(model.TwinIndex, model.Id), model.Id + " model choice");
            focus = core.Characters[model.TwinIndex];
            foreach (var twin in core.Characters)
            {
                Place(twin, Stage + new Vector3(twin == focus ? 0 : -10, 0, -6));
                foreach (var item in twin.GetComponentsInChildren<Transform>()) item.gameObject.layer = 30;
            }
            direction = Vector3.zero; speed = pivot = 0;
            yield return .4f;
            var animator = focus.GetComponentInChildren<Animator>();
            var motion = focus.GetComponentInChildren<SoraCharacterMotion>();
            Require(motion.IsReady && focus.GetComponent<CharacterController>().isGrounded, model.Id + " grounded initialized motion");
            Capture(model.Id + "-idle");
            foreach (var gait in new[] { ("walk", 1.25f), ("jog", 3.05f), ("sprint", 5f) })
            {
                direction = Vector3.forward; speed = gait.Item2;
                int before = motion.FootstepCount;
                yield return 1.1f;
                Require(Mathf.Abs(animator.GetFloat("Speed") - gait.Item2) < .2f,
                    model.Id + " " + gait.Item1 + " actual speed " + animator.GetFloat("Speed") + " dt " + Time.deltaTime);
                Require(motion.FootstepCount > before, model.Id + " " + gait.Item1 + " foot contact events");
                Capture(model.Id + "-" + gait.Item1);
            }
            direction = Vector3.zero; speed = 0;
            yield return .4f;
            Require(animator.GetFloat("Speed") < .08f, model.Id + " stop");
            pivot = 1;
            yield return .25f;
            Require(State(animator, "TurnRight"), model.Id + " stationary right pivot");
            pivot = 0; jump = true;
            float groundY = focus.transform.position.y;
            yield return .14f;
            Require(State(animator, "Jump") && focus.transform.position.y > groundY + .2f, model.Id + " jump interrupts pivot");
            Capture(model.Id + "-jump");
            jump = true;
            yield return .39f;
            Require(State(animator, "Fall"), model.Id + " physical descent drives fall");
            Require(focus.transform.position.y < groundY + 1.1f, model.Id + " no airborne double jump");
            float deadline = Time.time + 2;
            while (!focus.GetComponent<CharacterController>().isGrounded && Time.time < deadline) yield return null;
            Require(focus.GetComponent<CharacterController>().isGrounded, model.Id + " lands");
            jump = true;
            yield return .13f;
            Require(State(animator, "Jump"), model.Id + " immediate jump on landing");
            yield return .9f;
            Place(focus, Stage);
            yield return .3f;
            motion.Tick(0, true, -2, false, 0, false, 16);
            yield return .7f;
            Require(State(animator, "IdleLookAround") || State(animator, "IdleStretch"), model.Id + " idle variation");
            Capture(model.Id + "-idle-variation");
            direction = Vector3.forward; speed = 1.25f;
            yield return .15f;
            Require(State(animator, "Locomotion"), model.Id + " movement cancels idle");
            direction = Vector3.zero; speed = 0;
            Place(focus, Stage + new Vector3(4, .6f, 0));
            yield return .6f;
            SampleFeet(model.Id, "ramp", animator);
            Place(focus, Stage + new Vector3(-3.05f, .3f, 0));
            yield return .6f;
            SampleFeet(model.Id, "step", animator);
            Place(focus, Stage + new Vector3(14.9f, 0, 6));
            direction = Vector3.right; speed = 3.05f;
            yield return .65f;
            Require(State(animator, "Fall"), model.Id + " walking off edge enters fall");
            direction = Vector3.zero; speed = 0;
            Place(focus, Stage);
            yield return .25f;
        }

        private IEnumerator WetContacts()
        {
            var weather = Object.FindFirstObjectByType<SoraWeather>();
            Require(weather != null, "Weather integration present");
            weather.Restore(new SoraWeatherState { kind = SoraWeatherKind.Clear, automaticCycle = false, rain = 0, wetness = 1, puddles = 1 });
            Vector3 wetPoint = default;
            bool found = false;
            for (int x = -5; x <= 5 && !found; x++)
                for (int z = -16; z <= -10 && !found; z++)
                {
                    var p = new Vector3(x, 0, z);
                    if (weather.Surfaces.IsPuddle(p) && weather.Surfaces.IsPuddle(p + Vector3.left * .2f) &&
                        weather.Surfaces.IsPuddle(p + Vector3.right * .2f)) { wetPoint = p; found = true; }
                }
            Require(found, "After-rain puddle contact point found");
            Place(focus, wetPoint);
            direction = Vector3.zero; speed = 0;
            yield return .3f;
            int splashes = weather.Surfaces.FootstepSplashes;
            int before = focus.GetComponentInChildren<SoraCharacterMotion>().FootstepCount;
            direction = Vector3.forward; speed = 1.25f;
            yield return .7f;
            Require(focus.GetComponentInChildren<SoraCharacterMotion>().FootstepCount > before, "Street animation emits foot contacts");
            Require(weather.Surfaces.FootstepSplashes > splashes, "Animation contact splashes after-rain puddle");
            Require(focus.GetComponents<AudioSource>().Any(a => a.spatialBlend == 1 && a.volume > 0), "Spatial footstep audio configured");
            direction = Vector3.zero; speed = 0;
        }

        private void Update()
        {
            if (!running) return;
            if (Time.realtimeSinceStartup - startedAt > 600)
            { File.WriteAllText(Folder + "/failure.txt", "Timed out"); EditorApplication.Exit(1); return; }
            foreach (var twin in core.Characters)
                twin.Move(twin == focus ? direction : Vector3.zero, twin == focus ? speed : 0,
                    twin == focus && jump, twin == focus ? pivot : 0, Time.deltaTime);
            jump = false;
        }

        private void Require(bool value, string message)
        {
            if (!value)
            {
                errors.Add(message);
                File.WriteAllText(Folder + "/failure.txt", string.Join("\n", errors));
                Debug.LogError("SORA MOTION REVIEW: " + message);
                WriteReport();
                EditorApplication.Exit(1);
                throw new InvalidOperationException(message);
            }
            checks.Add(message);
        }

        private static bool State(Animator animator, string name) => animator.GetCurrentAnimatorStateInfo(0).IsName(name) ||
            animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).IsName(name);

        private static GameObject Box(string name, Vector3 at, Vector3 size)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name; box.layer = 30;
            box.transform.position = at; box.transform.localScale = size;
            box.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                { color = new Color(.23f, .29f, .3f) };
            return box;
        }

        private static void Place(SoraDemoCharacter twin, Vector3 at)
        {
            var cc = twin.GetComponent<CharacterController>();
            cc.enabled = false; twin.transform.position = at; twin.transform.rotation = Quaternion.identity;
            cc.enabled = true; twin.ResetMotion(); Physics.SyncTransforms();
        }

        private void SampleFeet(string model, string terrain, Animator animator)
        {
            foreach (var pair in new[] { (HumanBodyBones.LeftFoot, animator.leftFeetBottomHeight), (HumanBodyBones.RightFoot, animator.rightFeetBottomHeight) })
            {
                Vector3 foot = animator.GetBoneTransform(pair.Item1).position;
                var hit = Physics.RaycastAll(foot + Vector3.up * .4f, Vector3.down, 1, 1 << 30, QueryTriggerInteraction.Ignore)
                    .Where(h => !(h.collider is CharacterController)).OrderBy(h => h.distance).First();
                float error = foot.y - pair.Item2 - hit.point.y;
                contacts.Add(new ContactSample { model = model, terrain = terrain, foot = pair.Item1.ToString(), clearance = error });
                Require(error > -.065f && error < .15f, model + " " + terrain + " " + pair.Item1 + " bounded foot placement");
            }
        }

        private void CheckStoreState()
        {
            var register = core.Interactables.Single(t => t.kind == InteractKind.Register);
            var call = typeof(SoraDemoCore).GetMethod("Interact", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var swap = typeof(SoraDemoCore).GetMethod("SwitchCharacter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            int cash = core.State.storeCash, stock = core.State.shelfStock, sales = core.State.sales;
            call.Invoke(core, new object[] { register }); swap.Invoke(core, null);
            Require(!core.HasPendingSale && core.State.storeCash == cash, "POS draft cancels on switch");
            call.Invoke(core, new object[] { register }); call.Invoke(core, new object[] { register });
            for (int i = 0; i < 100; i++) swap.Invoke(core, null);
            Require(core.State.storeCash == cash + 380 && core.State.shelfStock == stock - 1 && core.State.sales == sales + 1,
                "100 switches preserve a single POS commit");
        }

        private void Capture(string name)
        {
            var camera = Camera.main;
            var at = focus.transform.position;
            camera.transform.position = at + new Vector3(2.4f, 1.3f, 4.7f);
            camera.transform.LookAt(at + Vector3.up * .9f);
            camera.fieldOfView = 29;
            var target = new RenderTexture(1280, 960, 24);
            var image = new Texture2D(1280, 960, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            var previousTarget = camera.targetTexture;
            try
            {
                RenderSettings.fog = false;
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1280, 960), 0, 0); image.Apply();
                File.WriteAllBytes(Folder + "/" + name + ".png", image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget; RenderTexture.active = previous;
                Object.DestroyImmediate(target); Object.DestroyImmediate(image);
            }
        }

        private void WriteReport() => File.WriteAllText(Folder + "/report.json", JsonUtility.ToJson(new Report
        { unity = Application.unityVersion, checks = checks, contacts = contacts, errors = errors, videoFrames = videoFrames, fixedDeltaTime = 1f / 30 }, true));
        public static void BuildWindows()
        {
            var report = UnityEditor.BuildPipeline.BuildPlayer(new[] { SoraDevAutomation.DemoScene },
                "Builds/Windows/SORA_After_the_Rain.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new InvalidOperationException("Motion Windows build failed.");
            Debug.Log("SORA MOTION: Windows build succeeded using existing prototype scene.");
        }
        private void Log(string text, string stack, LogType type)
        { if (type == LogType.Exception || type == LogType.Error) errors.Add(text); }
        [Serializable] private class ContactSample { public string model, terrain, foot; public float clearance; }
        [Serializable] private class Report { public string unity; public List<string> checks, errors; public List<ContactSample> contacts; public int videoFrames; public float fixedDeltaTime; }
    }
}
#endif
