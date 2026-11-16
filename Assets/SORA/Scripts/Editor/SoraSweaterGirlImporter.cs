#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sora.AfterTheRain.Demo.Editor
{
    /// <summary>
    /// Imports Tei Akiko's procedural Blender model (Tools/Blender/build_sweater_girl.py):
    /// builds URP Lit materials, remaps them onto the FBX, sets up the Humanoid rig with looping Idle/Walk/Run and a
    /// Speed-driven locomotion controller, then saves the Resources prefab SoraDemoWorld loads.
    /// Batch: -executeMethod Sora.AfterTheRain.Demo.Editor.SoraSweaterGirlImporter.Import
    /// </summary>
    public static class SoraSweaterGirlImporter
    {
        const string Dir = "Assets/SORA/Characters/SweaterGirl";
        const string Fbx = Dir + "/sweater_girl.fbx";
        const string Controller = Dir + "/TeiAkiko.controller";
        const string Prefab = "Assets/SORA/Characters/Resources/" + SoraDemoWorld.AkikoModelResource + ".prefab";

        // Colours mirror make_materials() in the Blender script (sRGB hex); textured entries use a map.
        static readonly Dictionary<string, string> Colors = new Dictionary<string, string>
        {
            ["SG_Face"] = "#F6D3CE", ["SG_Skin"] = "#FCE9E0", ["SG_Sweater"] = "#F4ECDA", ["SG_Skirt"] = "#2C3448",
            ["SG_SkirtStripe"] = "#47B3B8", ["SG_Stocking"] = "#34343E", ["SG_StockingBand"] = "#7F8FB2",
            ["SG_Boot"] = "#2A2A30", ["SG_Sole"] = "#1B1B1F", ["SG_Lace"] = "#5C5C68", ["SG_Hair"] = "#FFFFFF",
            ["SG_BlackBand"] = "#24232A", ["SG_Ribbon"] = "#A9BEE3", ["SG_Clip"] = "#B5C8E8"
        };

        static readonly Dictionary<string, string> Maps = new Dictionary<string, string>
        {
            ["SG_Face"] = Dir + "/Textures/sg_face.png", ["SG_Hair"] = Dir + "/Textures/sg_hair_gradient.png"
        };

        [MenuItem("SORA/5. Import Tei Akiko Model")]
        public static void Import()
        {
            AssetDatabase.ImportAsset(Fbx, ImportAssetOptions.ForceSynchronousImport);
            var importer = (ModelImporter)AssetImporter.GetAtPath(Fbx)
                ?? throw new Exception("SORA: FBX missing: " + Fbx);
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.bakeAxisConversion = false; // true turns her 180° (FBX front axis is -Z from Blender)
            importer.animationType = ModelImporterAnimationType.Human;
            importer.importAnimation = true;
            // Drop any saved description first: its skeleton rest pose would outlive FBX/axis changes and mirror the retarget.
            importer.humanDescription = new HumanDescription();
            importer.SaveAndReimport();
            // Bones already carry Humanoid names; map them explicitly (auto-mapping skips Chest and drops its motion).
            var hd = importer.humanDescription;
            if (hd.skeleton == null || hd.skeleton.Length == 0) throw new Exception("SORA: Humanoid import produced no skeleton.");
            var bones = new HashSet<string>(hd.skeleton.Select(s => s.name));
            hd.human = HumanTrait.BoneName.Where(bones.Contains).Select(n => new HumanBone
                { boneName = n, humanName = n, limit = new HumanLimit { useDefaultValues = true } }).ToArray();
            importer.humanDescription = hd;
            SoraAnimationControllerBuilder.ConfigureClips();

            var shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? throw new Exception("SORA: URP Lit shader not found; is URP installed?");
            Directory.CreateDirectory(Dir + "/Materials");
            // Re-runs: remapped materials leave the embedded list, so also read the existing remaps.
            var names = AssetDatabase.LoadAllAssetsAtPath(Fbx).OfType<Material>().Select(m => m.name)
                .Concat(importer.GetExternalObjectMap().Keys.Where(k => k.type == typeof(Material)).Select(k => k.name))
                .Distinct().ToList();
            foreach (string name in names)
            {
                string path = Dir + "/Materials/" + name + ".mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
                mat.shader = shader;
                ColorUtility.TryParseHtmlString(Colors.TryGetValue(name, out string hex) ? hex : "#FFFFFF", out Color c);
                mat.SetColor("_BaseColor", c);
                mat.SetFloat("_Smoothness", 0.15f);
                mat.SetFloat("_Metallic", 0f);
                if (Maps.TryGetValue(name, out string tex))
                {
                    var t = AssetDatabase.LoadAssetAtPath<Texture2D>(tex);
                    if (t != null) mat.SetTexture("_BaseMap", t);
                }
                EditorUtility.SetDirty(mat);
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), name), mat);
            }
            AssetDatabase.SaveAssets();
            importer.SaveAndReimport();

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Fbx);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            instance.name = "TeiAkiko";
            var animator = instance.GetComponent<Animator>() ?? throw new Exception("SORA: Humanoid import added no Animator.");
            if (animator.avatar == null || !animator.avatar.isHuman) throw new Exception("SORA: Humanoid avatar invalid; check bone names.");
            animator.runtimeAnimatorController = SoraAnimationControllerBuilder.Build();
            animator.applyRootMotion = false;
            if (instance.GetComponent<SoraCharacterMotion>() == null) instance.AddComponent<SoraCharacterMotion>();
            Directory.CreateDirectory(Path.GetDirectoryName(Prefab));
            PrefabUtility.SaveAsPrefabAsset(instance, Prefab);
            UnityEngine.Object.DestroyImmediate(instance);
            AssetDatabase.Refresh();
            Debug.Log("SORA: Tei Akiko model imported: " + names.Count + " materials, prefab " + Prefab);
        }

        /// <summary>Renders front/back previews to Artifacts/Character/unity_preview_*.png (needs a GPU; no -nographics).</summary>
        public static void CapturePreview()
        {
            Import();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var camGo = new GameObject("PreviewCam");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.85f, 0.87f, 0.92f);
            cam.fieldOfView = 24f;
            var lightGo = new GameObject("PreviewLight");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            lightGo.transform.rotation = Quaternion.Euler(35f, 150f, 0f);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.6f, 0.6f, 0.65f);

            // The imported model faces +Z (Unity forward).
            foreach (var view in new[] { ("front", new Vector3(0, 0.82f, 4.6f)), ("back", new Vector3(0, 0.82f, -4.6f)) })
            {
                camGo.transform.position = view.Item2;
                camGo.transform.LookAt(new Vector3(0, 0.82f, 0));
                Shot(cam, 700, 1100, "unity_preview_" + view.Item1);
            }
            UnityEngine.Object.DestroyImmediate(go);
            UnityEngine.Object.DestroyImmediate(camGo);
            UnityEngine.Object.DestroyImmediate(lightGo);
            Debug.Log("SORA: preview captured.");
        }

        /// <summary>
        /// Enters Play Mode in the demo scene and renders Akiko in the generated world to Artifacts/Character/unity_ingame_*.png.
        /// Batch WITHOUT -quit (exits itself) and without -nographics.
        /// </summary>
        public static void CaptureInGame()
        {
            EditorSceneManager.OpenScene(SoraDevAutomation.DemoScene);
            SessionState.SetFloat(CaptureFlag, (float)EditorApplication.timeSinceStartup);
            EditorApplication.EnterPlaymode();
        }

        const string CaptureFlag = "SORA.CaptureInGame";

        // Entering Play Mode reloads the domain, so the capture resumes from SessionState.
        [InitializeOnLoadMethod]
        static void ResumeCapture()
        {
            float startedAt = SessionState.GetFloat(CaptureFlag, -1f);
            if (startedAt < 0f) return;
            int frames = 0;
            EditorApplication.update += Tick;

            void Tick()
            {
                if (EditorApplication.timeSinceStartup - startedAt > 180)
                {
                    SessionState.EraseFloat(CaptureFlag);
                    Debug.LogError("SORA: in-game capture timed out.");
                    EditorApplication.Exit(1);
                }
                if (!EditorApplication.isPlaying || ++frames < 90) return;
                EditorApplication.update -= Tick;
                SessionState.EraseFloat(CaptureFlag);
                int code = 0;
                try
                {
                    var akiko = GameObject.Find("Tei Akiko") ?? throw new Exception("SORA: Tei Akiko not spawned.");
                    if (akiko.transform.Find("Tei Akiko Model") == null) throw new Exception("SORA: Akiko spawned without model.");
                    var cam = new GameObject("Capture Cam").AddComponent<Camera>();
                    cam.fieldOfView = 30f;
                    Transform t = akiko.transform;
                    Vector3 focus = t.position + Vector3.up * 0.85f;
                    foreach (var view in new[] { ("front", t.forward * 3.4f), ("side", (t.forward + t.right).normalized * 3.4f) })
                    {
                        cam.transform.position = focus + view.Item2 + Vector3.up * 0.25f;
                        cam.transform.LookAt(focus);
                        Shot(cam, 900, 1100, "unity_ingame_" + view.Item1);
                    }
                    // Mid-stride proof: hold the controller still and pin the blend tree to AI walk / player jog speed.
                    akiko.GetComponent<SoraDemoCharacter>().enabled = false;
                    var animator = akiko.GetComponentInChildren<Animator>();
                    // Skinning normally runs once per player-loop frame, not per cam.Render().
                    foreach (var smr in akiko.GetComponentsInChildren<SkinnedMeshRenderer>()) smr.forceMatrixRecalculationPerRender = true;
                    cam.transform.position = focus + t.right * 3.4f + Vector3.up * 0.25f;
                    cam.transform.LookAt(focus);
                    foreach (var gait in new[] { ("walk", 1.25f), ("run", 3.05f) })
                    {
                        animator.SetFloat("Speed", gait.Item2);
                        animator.Play("Locomotion", 0, 0.25f); // left leg at full forward swing
                        animator.Update(0f);
                        Shot(cam, 900, 1100, "unity_ingame_" + gait.Item1);
                    }
                    Debug.Log("SORA: in-game capture done.");
                }
                catch (Exception e) { Debug.LogException(e); code = 1; }
                EditorApplication.Exit(code);
            }
        }

        static void Shot(Camera cam, int w, int h, string name)
        {
            Directory.CreateDirectory("Artifacts/Character");
            var rt = new RenderTexture(w, h, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            File.WriteAllBytes("Artifacts/Character/" + name + ".png", tex.EncodeToPNG());
            RenderTexture.active = null;
            cam.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(tex);
            rt.Release();
        }
    }
}
#endif
