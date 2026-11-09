#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Sora.AfterTheRain.Demo.Editor
{
    // Isolated ART QA, not a playable SoraDemoCore scene. Do not publish owner-private V03 FBX.
    public static class SoraV06FacadeStaging
    {
        const string Root = "Assets/SORA/Environment/StreetExterior/";
        const string V03 = Root + "PrivateGenerated/SORA_Neighborhood_V03_Integrated.fbx";
        const string V05Public = Root + "PublicGenerated/SORA_V05_Kenney_9Block_Overlay.fbx";
        const string V05Private = Root + "PrivateGenerated/SORA_V05_Kenney_9Block_Overlay.fbx";
        const string V06 = Root + "PublicGenerated/SORA_V06_9Block_Facade_Details.fbx";
        const string ScenePath = "Assets/SORA/Scenes/SORA_Neighborhood_V06_ArtQA.unity";

        [Serializable] sealed class Proxy
        {
            public string id, group;
            public float cx, cy, cz, sx, sy, sz;
        }
        [Serializable] sealed class ProxyList { public Proxy[] items; }
        [Serializable] sealed class V05Manifest { public string glb_sha256; public int kenney_car_count; }
        [Serializable] sealed class V06Manifest
        {
            public string target_v05_sha256;
            public int new_v04_v05_facades_detailed, new_colliders;
        }

        [MenuItem("SORA/Street Exterior/V06 - Build detailed art QA scene")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("Stop Play Mode first.");
                return;
            }
            GameObject a = AssetDatabase.LoadAssetAtPath<GameObject>(V03);
            GameObject b = AssetDatabase.LoadAssetAtPath<GameObject>(V05Public);
            if (b == null) b = AssetDatabase.LoadAssetAtPath<GameObject>(V05Private);
            GameObject c = AssetDatabase.LoadAssetAtPath<GameObject>(V06);
            if (a == null || b == null || c == null)
            {
                EditorUtility.DisplayDialog("V06 assets missing",
                    "Import owner-held V03, V05 replacement and V06 additive FBXs first.\n" +
                    V03 + "\n" + V05Public + " or " + V05Private + "\n" + V06 +
                    "\n\nConvert GLB to FBX using Tools/StreetExterior/convert_glb_to_fbx_blender.py.",
                    "OK");
                return;
            }
            string baseDir = Directory.GetCurrentDirectory();
            string[] files = {
                Path.Combine(baseDir, "ArtSource/Generated/V05_CC0/colliders.json"),
                Path.Combine(baseDir, "ArtSource/Generated/V05_CC0/manifest.json"),
                Path.Combine(baseDir, "ArtSource/Generated/V06_CC0/manifest.json") };
            foreach (string file in files)
                if (!File.Exists(file)) { Debug.LogError("Missing " + file); return; }
            var proxies = JsonUtility.FromJson<ProxyList>(File.ReadAllText(files[0]));
            var prior = JsonUtility.FromJson<V05Manifest>(File.ReadAllText(files[1]));
            var current = JsonUtility.FromJson<V06Manifest>(File.ReadAllText(files[2]));
            if (proxies == null || proxies.items == null || proxies.items.Length != 122 ||
                prior == null || prior.kenney_car_count != 42 || current == null ||
                current.new_v04_v05_facades_detailed != 60 || current.new_colliders != 0 ||
                current.target_v05_sha256 != prior.glb_sha256)
            {
                Debug.LogError("V05/V06 manifest mismatch. Refusing partial/incorrect staging.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("SORA V06 ART QA (no game logic)");
            Place(a, root.transform, "V03 private base");
            Place(b, root.transform, "V05 full replacement with 42 Kenney vehicles");
            Place(c, root.transform, "V06 additive details, 60 facades");
            var physical = new GameObject("V05 physical collision proxies, manual QA pending");
            physical.transform.SetParent(root.transform, false);
            foreach (Proxy p in proxies.items)
            {
                var go = new GameObject(p.group + " - " + p.id);
                go.transform.SetParent(physical.transform, false);
                go.transform.localPosition = new Vector3(p.cx,p.cy,p.cz);
                var collider = go.AddComponent<BoxCollider>();
                collider.size = new Vector3(p.sx,p.sy,p.sz);
                collider.isTrigger = false;
            }
            var floor = new GameObject("Safety continuous ground");
            floor.transform.SetParent(physical.transform, false);
            floor.transform.localPosition = new Vector3(22.5f,-.20f,22.5f);
            floor.AddComponent<BoxCollider>().size = new Vector3(185f,.32f,185f);
            var eye = new GameObject("Walking height inspection camera");
            eye.transform.SetParent(root.transform, false);
            eye.tag = "MainCamera";
            eye.transform.localPosition = new Vector3(-23f,2.2f,45f);
            eye.transform.LookAt(new Vector3(-22.5f,3f,57f));
            Camera camera = eye.AddComponent<Camera>();
            camera.fieldOfView=60f;
            camera.nearClipPlane=.08f;
            camera.farClipPlane=300f;
            eye.AddComponent<AudioListener>();
            var moon = new GameObject("Temporary art review directional light");
            moon.transform.SetParent(root.transform,false);
            moon.transform.rotation=Quaternion.Euler(36f,-42f,0f);
            var light = moon.AddComponent<Light>();
            light.type=LightType.Directional;
            light.color=new Color(.62f,.71f,.90f);
            light.intensity=.65f;
            light.shadows=LightShadows.Soft;
            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.27f,.30f,.38f);
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Exponential;
            RenderSettings.fogColor=new Color(.08f,.11f,.16f);
            RenderSettings.fogDensity=.004f;
            Directory.CreateDirectory("Assets/SORA/Scenes");
            if (!EditorSceneManager.SaveScene(scene,ScenePath))
                throw new IOException("Could not save " + ScenePath);
            Selection.activeGameObject=root;
            Debug.Log("V06 art QA scene saved. NOT gameplay; Unity traversal/camera/URP QA pending.");
        }
        static void Place(GameObject prefab, Transform root, string label)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab, root.gameObject.scene);
            go.transform.SetParent(root,false);
            go.transform.localPosition=Vector3.zero;
            go.transform.localRotation=Quaternion.identity;
            go.transform.localScale=Vector3.one;
            go.name=label;
        }
    }
}
#endif
