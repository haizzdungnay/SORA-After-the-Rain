#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Sora.AfterTheRain.Demo.Editor
{
    public static class SoraLocalCharacterImporter
    {
        public const string Root = "Assets/SORA/Characters/LocalImported";
        static readonly string[] Models = { "Chisa", "Citlali", "Denia", "Lynae", "MaleRover", "Zhongli" };

        [Serializable] sealed class Settings { public MaterialSettings[] materials = Array.Empty<MaterialSettings>(); }
        [Serializable] sealed class MaterialSettings
        {
            public string name = "", texture = "", alphaMode = "OPAQUE";
            public float[] color = { 1, 1, 1, 1 };
            public float alphaCutoff = 0.35f;
        }
        [Serializable] sealed class Report { public List<ModelReport> models = new List<ModelReport>(); }
        [Serializable] sealed class ModelReport
        {
            public string id;
            public int meshes, vertices, triangles, skinnedRenderers, weightedBones, blendShapes, materials, texturedMaterials;
            public float height;
            public string[] clips;
        }

        [MenuItem("SORA/Characters/1. Build Local Model Library")]
        public static void Import()
        {
            Require(Directory.Exists(Root), "Prepare the owner models with Tools/Blender/prepare_character_models.py first.");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            Require(shader != null, "URP Lit shader missing.");
            foreach (string id in Models)
            {
                string dir = Root + "/" + id;
                string fbx = dir + "/" + id + ".fbx";
                Require(File.Exists(fbx) && File.Exists(dir + "/import-settings.json"), "Missing prepared model: " + id);
                var settings = JsonUtility.FromJson<Settings>(File.ReadAllText(dir + "/import-settings.json"));
                var importer = AssetImporter.GetAtPath(fbx) as ModelImporter;
                Require(importer != null, "FBX importer missing: " + id);
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
                // ponytail: Generic rigs and base-color URP materials; add Humanoid retargeting/toon shaders for production characters.
                importer.animationType = ModelImporterAnimationType.Generic;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.importAnimation = true;
                importer.importBlendShapes = true;
                importer.optimizeGameObjects = false;
                importer.isReadable = true;
                importer.SaveAndReimport();

                var sources = AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<Material>().ToDictionary(m => m.name);
                foreach (var remap in importer.GetExternalObjectMap())
                    if (remap.Key.type == typeof(Material) && remap.Value is Material material)
                        sources[remap.Key.name] = material;
                Directory.CreateDirectory(dir + "/Materials");
                int index = 0;
                foreach (var pair in sources.OrderBy(p => p.Key, StringComparer.Ordinal))
                {
                    var spec = settings.materials.SingleOrDefault(m => m.name == pair.Key);
                    var texture = spec != null && !string.IsNullOrEmpty(spec.texture)
                        ? AssetDatabase.LoadAssetAtPath<Texture2D>(dir + "/" + spec.texture)
                        : pair.Value.mainTexture;
                    Require(spec == null || string.IsNullOrEmpty(spec.texture) || texture != null, "Missing texture: " + id + "/" + pair.Key);
                    var color = spec != null ? new Color(spec.color[0], spec.color[1], spec.color[2], spec.color[3])
                        : texture != null ? Color.white : pair.Value.color;
                    string path = dir + "/Materials/M_" + (index++).ToString("D2") + ".mat";
                    var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
                    mat.name = Path.GetFileNameWithoutExtension(path);
                    mat.shader = shader;
                    mat.SetTexture("_BaseMap", texture);
                    mat.SetColor("_BaseColor", color);
                    mat.SetFloat("_Smoothness", 0.1f);
                    mat.SetFloat("_Cull", (float)CullMode.Off);
                    bool blend = spec?.alphaMode == "BLEND";
                    bool cutout = spec?.alphaMode == "MASK";
                    mat.SetFloat("_Surface", blend ? 1 : 0);
                    mat.SetFloat("_AlphaClip", cutout ? 1 : 0);
                    mat.SetFloat("_Cutoff", spec?.alphaCutoff ?? 0.35f);
                    mat.SetFloat("_SrcBlend", (float)(blend ? BlendMode.SrcAlpha : BlendMode.One));
                    mat.SetFloat("_DstBlend", (float)(blend ? BlendMode.OneMinusSrcAlpha : BlendMode.Zero));
                    mat.SetFloat("_ZWrite", blend ? 0 : 1);
                    if (cutout) mat.EnableKeyword("_ALPHATEST_ON"); else mat.DisableKeyword("_ALPHATEST_ON");
                    if (blend) mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); else mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.SetOverrideTag("RenderType", blend ? "Transparent" : cutout ? "TransparentCutout" : "Opaque");
                    mat.renderQueue = (int)(blend ? RenderQueue.Transparent : cutout ? RenderQueue.AlphaTest : RenderQueue.Geometry);
                    EditorUtility.SetDirty(mat);
                    importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), pair.Key), mat);
                }
                AssetDatabase.SaveAssets();
                importer.SaveAndReimport();
                var root = new GameObject(id);
                try
                {
                    var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(fbx));
                    model.transform.SetParent(root.transform, false);
                    var bounds = MeshBounds(root);
                    Require(bounds.size.y > 0.001f, "Invalid model height: " + id);
                    float scale = SoraCharacterModels.Find(id).Height / bounds.size.y;
                    model.transform.localPosition = (model.transform.localPosition - new Vector3(bounds.center.x, bounds.min.y, bounds.center.z)) * scale;
                    model.transform.localScale *= scale;
                    foreach (var animator in root.GetComponentsInChildren<Animator>(true)) animator.applyRootMotion = false;
                    PrefabUtility.SaveAsPrefabAsset(root, dir + "/" + id + ".prefab");
                }
                finally { Object.DestroyImmediate(root); }
                Debug.Log("SORA: local character prefab imported: " + id);
            }
            AssetDatabase.SaveAssets();
            Validate();
        }

        [MenuItem("SORA/Characters/2. Validate Local Model Library")]
        public static void Validate()
        {
            var report = new Report();
            foreach (string id in Models)
            {
                string dir = Root + "/" + id;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(dir + "/" + id + ".prefab");
                Require(prefab != null, "Missing prefab: " + id);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                try
                {
                    var renderers = instance.GetComponentsInChildren<Renderer>(true);
                    Require(renderers.Length > 0, "No renderers: " + id);
                    var materials = renderers.SelectMany(r => r.sharedMaterials).Distinct().ToArray();
                    Require(materials.All(m => m != null && m.shader.name == "Universal Render Pipeline/Lit"), "Invalid URP materials: " + id);
                    Require(materials.Any(m => m.GetTexture("_BaseMap") != null), "No base-color textures: " + id);
                    var bounds = MeshBounds(instance);
                    Require(Mathf.Abs(bounds.min.y) < 0.005f && Mathf.Abs(bounds.size.y - SoraCharacterModels.Find(id).Height) < 0.005f, "Feet/height normalization failed: " + id + ": " + bounds);
                    Require(instance.GetComponentsInChildren<Collider>(true).Length == 0, "Unexpected model collider: " + id);
                    var skinned = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    var meshes = skinned.Select(r => r.sharedMesh).Concat(instance.GetComponentsInChildren<MeshFilter>(true).Select(f => f.sharedMesh)).Distinct().ToArray();
                    Require(meshes.All(m => m != null), "Missing mesh: " + id);
                    var clips = AssetDatabase.LoadAllAssetsAtPath(dir + "/" + id + ".fbx").OfType<AnimationClip>()
                        .Where(c => !c.name.StartsWith("__preview__", StringComparison.Ordinal)).ToArray();
                    report.models.Add(new ModelReport
                    {
                        id = id, meshes = meshes.Length, vertices = meshes.Sum(m => m.vertexCount),
                        triangles = meshes.Sum(m => m.triangles.Length / 3), skinnedRenderers = skinned.Length,
                        weightedBones = skinned.SelectMany(s => s.bones).Where(b => b != null).Distinct().Count(),
                        blendShapes = meshes.Sum(m => m.blendShapeCount), materials = materials.Length,
                        texturedMaterials = materials.Count(m => m.GetTexture("_BaseMap") != null), height = bounds.size.y,
                        clips = clips.Select(c => c.name + " (" + c.length.ToString("F3", System.Globalization.CultureInfo.InvariantCulture) + "s)").ToArray()
                    });
                }
                finally { Object.DestroyImmediate(instance); }
            }
            Directory.CreateDirectory("Artifacts/CharacterIntake");
            File.WriteAllText("Artifacts/CharacterIntake/unity-import-report.json", JsonUtility.ToJson(report, true));
            Debug.Log("SORA: PASS: 6 local character prefabs, URP materials, textures, feet pivots and requested heights. Report: Artifacts/CharacterIntake/unity-import-report.json");
        }

        internal static Bounds MeshBounds(GameObject root)
        {
            var points = new List<Vector3>();
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var skinned = renderer as SkinnedMeshRenderer;
                var filter = renderer.GetComponent<MeshFilter>();
                if (skinned == null && filter == null) continue;
                var mesh = skinned != null ? new Mesh() : filter.sharedMesh;
                try
                {
                    // Compensate baked scale before TransformPoint applies the renderer transform.
                    if (skinned != null) skinned.BakeMesh(mesh, true);
                    points.AddRange(mesh.vertices.Select(v => renderer.transform.TransformPoint(v)));
                }
                finally { if (skinned != null) Object.DestroyImmediate(mesh); }
            }
            Require(points.Count > 0, "No mesh vertices.");
            var bounds = new Bounds(points[0], Vector3.zero);
            foreach (var point in points) bounds.Encapsulate(point);
            return bounds;
        }

        [MenuItem("SORA/Characters/3. Capture Local Model Preview")]
        public static void CapturePreview()
        {
            Validate();
            var scene = EditorSceneManager.NewPreviewScene();
            RenderTexture target = null;
            Texture2D image = null;
            try
            {
                for (int i = 0; i < Models.Length; i++)
                {
                    var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/" + Models[i] + "/" + Models[i] + ".prefab"), scene);
                    model.transform.position = new Vector3((2.5f - i) * 1.3f, 0, 0);
                }
                var camera = new GameObject("Character Library Camera").AddComponent<Camera>();
                SceneManager.MoveGameObjectToScene(camera.gameObject, scene);
                camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(scene);
                camera.transform.position = new Vector3(0, 1.15f, 9);
                camera.transform.LookAt(new Vector3(0, 0.85f, 0));
                camera.orthographic = true;
                camera.orthographicSize = 1.15f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.20f, 0.24f, 0.30f);
                foreach (var angles in new[] { new Vector3(30, 150, 0), new Vector3(20, -35, 0) })
                {
                    var light = new GameObject("Preview Light").AddComponent<Light>();
                    SceneManager.MoveGameObjectToScene(light.gameObject, scene);
                    light.type = LightType.Directional;
                    light.intensity = 1.5f;
                    light.transform.rotation = Quaternion.Euler(angles);
                }
                target = new RenderTexture(1920, 540, 24);
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image = new Texture2D(1920, 540, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 1920, 540), 0, 0);
                image.Apply();
                File.WriteAllBytes("Artifacts/CharacterIntake/unity-character-library.png", image.EncodeToPNG());
                camera.targetTexture = null;
                Debug.Log("SORA: Unity Editor preview captured: Chisa, Citlali, Denia, Lynae, MaleRover, Zhongli (left to right). Not a gameplay capture.");
            }
            finally
            {
                RenderTexture.active = null;
                if (target != null) { target.Release(); Object.DestroyImmediate(target); }
                if (image != null) Object.DestroyImmediate(image);
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("SORA: " + message);
        }
    }
}
#endif
