#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Sora.AfterTheRain.Demo.Editor
{
    // Run a separate Editor with -batchmode -force-d3d11 -soraMartReview (without -quit).
    [InitializeOnLoad]
    public static class SoraMartReview
    {
        private const string Key = "SoraMartReview20261111Konbini";
        private const string Folder = "Artifacts/SoraMartKonbini";
        private static double began;

        static SoraMartReview()
        {
            if (Environment.GetCommandLineArgs().Contains("-soraMartReview")) EditorApplication.update += Tick;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void Call(SoraDemoCore core, string name, params object[] args)
        {
            typeof(SoraDemoCore).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(core, args);
        }

        private static void Tick()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            try
            {
                int step = SessionState.GetInt(Key, 0);
                if (step == 0)
                {
                    Directory.CreateDirectory(Folder);
                    var importer = AssetImporter.GetAtPath("Assets/Resources/SoraStore/SoraMart.glb");
                    var settings = new SerializedObject(importer);
                    settings.FindProperty("reportItems").ClearArray();
                    settings.ApplyModifiedPropertiesWithoutUndo();
                    importer.SaveAndReimport();
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
                }
                if (step == 2 && EditorApplication.timeSinceStartup - began > 6)
                {
                    CheckStore();
                    Camera.main.GetComponent<SoraThirdPersonCamera>().enabled = false;
                    Camera.main.fieldOfView = 65;
                    Camera.main.transform.position = new Vector3(11, 3.3f, -18);
                    Camera.main.transform.LookAt(new Vector3(0, 2.2f, -2));
                    began = EditorApplication.timeSinceStartup;
                    SessionState.SetInt(Key, 3);
                }
                else if (step == 3 && EditorApplication.timeSinceStartup - began > 2)
                {
                    Capture("storefront");
                    Camera.main.fieldOfView = 78;
                    Camera.main.transform.position = new Vector3(.2f, 2.3f, -4.6f);
                    Camera.main.transform.LookAt(new Vector3(.1f, 1.5f, 2.8f));
                    began = EditorApplication.timeSinceStartup;
                    SessionState.SetInt(Key, 4);
                }
                else if (step == 4 && EditorApplication.timeSinceStartup - began > 2)
                {
                    Capture("interior");
                    Camera.main.fieldOfView = 65;
                    Camera.main.transform.position = new Vector3(.4f, 2.15f, 4.5f);
                    Camera.main.transform.LookAt(new Vector3(-3.5f, 1.5f, -3.6f));
                    began = EditorApplication.timeSinceStartup;
                    SessionState.SetInt(Key, 5);
                }
                else if (step == 5 && EditorApplication.timeSinceStartup - began > 2)
                {
                    Capture("checkout");
                    Camera.main.fieldOfView = 60;
                    Camera.main.transform.position = new Vector3(-3.2f, 1.9f, -.5f);
                    Camera.main.transform.LookAt(new Vector3(-4.85f, 1.2f, -2.5f));
                    began = EditorApplication.timeSinceStartup;
                    SessionState.SetInt(Key, 6);
                }
                else if (step == 6 && EditorApplication.timeSinceStartup - began > 2)
                {
                    Capture("pos-closeup");
                    Camera.main.fieldOfView = 70;
                    Camera.main.transform.position = new Vector3(0, 1.7f, 2.9f);
                    Camera.main.transform.LookAt(new Vector3(0, 1.0f, 4.8f));
                    began = EditorApplication.timeSinceStartup;
                    SessionState.SetInt(Key, 7);
                }
                else if (step == 7 && EditorApplication.timeSinceStartup - began > 2)
                {
                    Capture("fresh-food");
                    Camera.main.fieldOfView = 65;
                    Camera.main.transform.position = new Vector3(-2.6f, 1.7f, -3.2f);
                    Camera.main.transform.LookAt(new Vector3(-2.6f, .9f, -1.1f));
                    began = EditorApplication.timeSinceStartup;
                    SessionState.SetInt(Key, 8);
                }
                else if (step == 8 && EditorApplication.timeSinceStartup - began > 2)
                {
                    Capture("daily-care");
                    Camera.main.fieldOfView = 70;
                    Camera.main.transform.position = new Vector3(-4.1f, 2.0f, .7f);
                    Camera.main.transform.LookAt(new Vector3(-6.2f, 1.3f, 1.8f));
                    began = EditorApplication.timeSinceStartup;
                    SessionState.SetInt(Key, 9);
                }
                else if (step == 9 && EditorApplication.timeSinceStartup - began > 2)
                {
                    Capture("coffee-bakery");
                    SessionState.EraseInt(Key);
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                Directory.CreateDirectory(Folder);
                File.WriteAllText(Folder + "/failure.txt", error.ToString());
                SessionState.EraseInt(Key);
                EditorApplication.Exit(1);
            }
        }

        private static void Place(SoraDemoCharacter twin, Vector3 position)
        {
            var controller = twin.GetComponent<CharacterController>();
            controller.enabled = false;
            twin.transform.position = position;
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static void CheckStore()
        {
            var core = SoraDemoCore.Instance;
            var store = GameObject.Find(SoraMartVisuals.RootName);
            Require(core != null && core.Characters.Count == 2 && store != null, "Authored store/twins absent");
            core.SetModelSelectionOpen(false);
            foreach (var twin in core.Characters) twin.enabled = false;
            Require(core.Interactables.Count(t => t.kind == InteractKind.Register) == 1, "Duplicate POS");
            Require(!GameObject.Find("Store tiled floor").GetComponent<Renderer>().enabled, "Prototype store art duplicated");
            Require(GameObject.Find("SORA_CITY_V05_REAL_ASSETS - 9 blocks - 42 Kenney vehicles") != null, "City regression");
            var overlaps = GameObject.Find("SORA_CITY_V05_REAL_ASSETS - 9 blocks - 42 Kenney vehicles")
                .GetComponentsInChildren<MeshRenderer>().Where(r => r.enabled && r.bounds.size.y > 1 && r.bounds.Intersects(SoraMartVisuals.Interior)).ToArray();
            File.WriteAllLines(Folder + "/city-store-overlaps.txt", overlaps.Select(r => r.name + " | center=" + r.bounds.center + " size=" + r.bounds.size));
            Require(overlaps.Length == 0, "Imported city wall cuts through store; inspect city-store-overlaps.txt");
            var materials = store.GetComponentsInChildren<MeshRenderer>().SelectMany(r => r.sharedMaterials).Distinct().ToArray();
            foreach (string source in new[] { "gondola", "endcap", "checkout", "back_fixture", "pos", "stock_shelf", "goods",
                "chilled_food", "bakery_rack", "magazine_rack", "basket_stack", "hot_food", "microwave",
                "coffee_brewer", "card_reader", "receipt_printer", "scanner" })
                Require(store.GetComponentsInChildren<MeshRenderer>().Any(r => r.name.StartsWith("IMPORTED | " + source + " | ")),
                    "Imported model source missing: " + source);
            Require(materials.All(m => m != null && m.shader.isSupported && m.shader.name != "Hidden/InternalErrorShader"), "Bad store material");
            Require(materials.Any(m => m.name.Contains("clear storefront glass") && m.renderQueue >= 3000), "Opaque storefront glass");

            int walks = 0;
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
                                             new Vector2(ends[route].x, ends[route].z)) < .25f, "Blocked store route " + route + " for " + twin.name);
                    walks++;
                }
                Place(twin, new Vector3(twin == core.Characters[0] ? -1 : 1.2f, 0, -2.5f));
            }

            var register = core.Interactables.Single(t => t.kind == InteractKind.Register);
            int cash = core.State.storeCash, stock = core.State.shelfStock, sales = core.State.sales;
            Call(core, "Interact", register);
            Call(core, "SwitchCharacter");
            Require(!core.HasPendingSale && core.State.storeCash == cash, "Switch charged draft");
            Call(core, "Interact", register);
            Call(core, "Interact", register);
            for (int i = 0; i < 100; i++) Call(core, "SwitchCharacter");
            Require(core.State.storeCash == cash + 380 && core.State.shelfStock == stock - 1 && core.State.sales == sales + 1, "POS/state regression");
            Call(core, "Interact", core.Interactables.Single(t => t.kind == InteractKind.Delivery));
            Call(core, "Interact", core.Interactables.Single(t => t.kind == InteractKind.Shelf));
            Require(core.State.backStock == 3 && core.State.shelfStock == stock + 2, "Delivery/restock regression");
            byte[] oldSave = File.Exists(core.SavePath) ? File.ReadAllBytes(core.SavePath) : null;
            try
            {
                core.Save();
                core.State.storeCash = 0;
                core.Load();
                Require(core.State.storeCash == cash + 380, "Save/load regression");
            }
            finally
            {
                if (oldSave == null) File.Delete(core.SavePath);
                else File.WriteAllBytes(core.SavePath, oldSave);
            }

            var rain = GameObject.Find("Low-cost Rain Particle System").GetComponent<ParticleSystem>();
            Require(rain.collision.enabled && rain.collision.lifetimeLoss.constant == 1, "Rain shelter collision absent");
            var emission = rain.emission;
            emission.enabled = false;
            var probes = new[] { new Vector3(0, 6, 0), new Vector3(0, 5, -6.3f), new Vector3(0, 6, -9) };
            for (int repeat = 0; repeat < 3; repeat++)
            {
                for (int probe = 0; probe < probes.Length; probe++)
                {
                    if (probe < 2) Require(Physics.Raycast(probes[probe], Vector3.down, out var hit, 3) &&
                        hit.collider.name.StartsWith("Store "), "Roof/canopy collider absent at " + probes[probe]);
                    rain.Clear();
                    rain.SetParticles(new[] { new ParticleSystem.Particle { position = probes[probe],
                        velocity = Vector3.down * 10, startLifetime = 2, remainingLifetime = 2, startSize = .03f } }, 1);
                    rain.Simulate(.4f, false, false, true);
                    Require(rain.particleCount == (probe == 2 ? 1 : 0), "Rain probe " + probe + " repeat " + repeat + " failed");
                }
            }
            rain.Clear();
            emission.enabled = true;
            rain.Play();
            var report = new Report { unity = Application.unityVersion, materials = materials.Length,
                storeRenderers = store.GetComponentsInChildren<MeshRenderer>().Length, walkthroughs = walks,
                switches = 100, posStockSaveLoad = true, roofRainCollision = true,
                privateCity = Resources.Load<GameObject>(SoraCityRuntimeWorld.PrivateV03Path) != null,
                triangles = store.GetComponentsInChildren<MeshFilter>().Sum(filter =>
                    Enumerable.Range(0, filter.sharedMesh.subMeshCount).Sum(i => (long)filter.sharedMesh.GetIndexCount(i) / 3)) };
            File.WriteAllText(Folder + "/report.json", JsonUtility.ToJson(report, true));
            Debug.Log("SORA MART ACCEPTANCE: " + JsonUtility.ToJson(report));
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
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
                image.Apply();
                File.WriteAllBytes(Folder + "/" + name + ".png", image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previous;
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(image);
            }
        }

        [Serializable]
        private class Report
        {
            public string unity;
            public int materials, storeRenderers, walkthroughs, switches;
            public long triangles;
            public bool posStockSaveLoad, roofRainCollision, privateCity;
        }
    }
}
#endif
