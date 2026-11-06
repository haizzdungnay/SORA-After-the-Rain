// Unity 6000.3.7f1 / URP 17.3; Editor-only, no gameplay bootstrap duplication.
// Copy under Assets/SORA/Scripts/Editor/ (let Unity generate .meta).
// V03 private FBX and V04 CC0/private FBX MUST be aligned at (0,0,0), scale 1.
#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Sora.Demo.Editor
{
    public static class SoraNeighborhoodV04Staging
    {
        const string LocalAssets = "Assets/SORA/Environment/StreetExterior/PrivateGenerated";
        const string V03Model = LocalAssets + "/SORA_Neighborhood_V03_Integrated.fbx";
        const string V04Model = LocalAssets + "/SORA_V04_Dense_9Block_Overlay.fbx";
        const string PublicColliderJson = "ArtSource/Generated/V04_CC0/colliders.json";
        const string LocalColliderJson = LocalAssets + "/V04_COLLIDERS.json";
        const string StagingPath = "Assets/SORA/Scenes/SORA_Neighborhood_V04_Staging.unity";

        [Serializable] class Proxy
        {
            public string id;
            public string group;
            public float cx,cy,cz,sx,sy,sz;
        }
        [Serializable] class ProxyList { public Proxy[] items; }

        // The private V04 JSON uses vector arrays. Prefer the public flat-array
        // collider manifest created by the verified CC0 model build.
        [MenuItem("SORA/Street Exterior/V04 - Build 9-block staging scene")]
        public static void BuildScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("Stop Play Mode first.");
                return;
            }
            GameObject baseModel = AssetDatabase.LoadAssetAtPath<GameObject>(V03Model);
            GameObject overlay = AssetDatabase.LoadAssetAtPath<GameObject>(V04Model);
            string manifest = Path.Combine(Directory.GetCurrentDirectory(),PublicColliderJson);
            if (baseModel == null || overlay == null || !File.Exists(manifest))
            {
                EditorUtility.DisplayDialog("SORA V04 imports missing",
                    "Required before staging:\n" + V03Model + "\n" + V04Model
                    + "\n" + PublicColliderJson
                    + "\n\nConvert both owner-private GLBs to FBX with Blender, and check out the V04 feature branch including LFS models. No existing scene was changed.",
                    "OK");
                return;
            }

            ProxyList data = JsonUtility.FromJson<ProxyList>(File.ReadAllText(manifest));
            if (data == null || data.items == null || data.items.Length != 122)
                throw new InvalidDataException("Unexpected V04 collider count; expected 60 buildings, 42 cars, 4 borders, 16 road ends.");

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            GameObject root = new GameObject("SORA V04 Nine-Block Visual Staging");
            Place(baseModel,root.transform,"Owner-private V03 base / keep AFX off public repo");
            Place(overlay,root.transform,"V04 5-block extension, parked vehicles and art barriers");

            GameObject boundary = new GameObject("SORA V04 Physical World Limits");
            boundary.transform.SetParent(root.transform,false);
            foreach (Proxy part in data.items)
            {
                GameObject go = new GameObject(part.group + " - " + part.id);
                go.transform.SetParent(boundary.transform,false);
                go.transform.localPosition = new Vector3(part.cx,part.cy,part.cz);
                BoxCollider collider = go.AddComponent<BoxCollider>();
                collider.size = new Vector3(part.sx,part.sy,part.sz);
                collider.isTrigger = false;
            }
            // Continuous floor safety net, extending outside the protected perimeter.
            GameObject floor = new GameObject("Safety ground - no fall through open terrain");
            floor.transform.SetParent(boundary.transform,false);
            floor.transform.localPosition = new Vector3(22.5f,-.20f,22.5f);
            floor.AddComponent<BoxCollider>().size = new Vector3(185f,.32f,185f);
            PreviewCamera(root.transform);
            PreviewLighting(root.transform);
            GameObject marker = new GameObject("SORA MART existing entrance anchor - visual only");
            marker.transform.SetParent(root.transform,false);
            marker.transform.localPosition = new Vector3(17f,.1f,6.75f);

            Directory.CreateDirectory("Assets/SORA/Scenes");
            if (!EditorSceneManager.SaveScene(scene,StagingPath))
                throw new IOException("Could not save V04 staging scene");
            Selection.activeGameObject = root;
            Debug.Log("SORA V04 staging created with " + data.items.Length +
                " independent building/car/road-end/continuous-perimeter collision proxies. " +
                "This is NOT playable gameplay integration and is NOT a Unity collision QA PASS. " +
                "The old SORA_Prototype scene and two-character/POS save state are untouched.");
        }

        static void Place(GameObject asset,Transform root,string label)
        {
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(asset,root.gameObject.scene);
            go.transform.SetParent(root,false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            go.name = label;
        }

        static void PreviewCamera(Transform root)
        {
            GameObject go = new GameObject("V04 eye-level inspector camera");
            go.transform.SetParent(root,false);
            go.tag = "MainCamera";
            go.transform.localPosition = new Vector3(45f,2.6f,-32f);
            go.transform.LookAt(new Vector3(45f,2.2f,60f));
            Camera camera = go.AddComponent<Camera>();
            camera.fieldOfView = 58f;
            camera.nearClipPlane = .075f;
            camera.farClipPlane = 320f;
            go.AddComponent<AudioListener>();
        }

        static void PreviewLighting(Transform root)
        {
            GameObject lightRoot = new GameObject("Temporary night lighting - not final rain");
            lightRoot.transform.SetParent(root,false);
            lightRoot.transform.rotation = Quaternion.Euler(39f,-37f,0);
            Light moon = lightRoot.AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.intensity = .58f;
            moon.color = new Color(.59f,.68f,.87f);
            moon.shadows = LightShadows.Soft;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.21f,.25f,.34f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = .004f;
            RenderSettings.fogColor = new Color(.09f,.12f,.18f);
        }
    }
}
#endif
