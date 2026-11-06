// Unity 6000.3.7f1 / URP 17.3.0 — local-only staging tool.
// Place this file under Assets/SORA/Scripts/Editor/; Unity creates its .meta GUID.
// Requires the private FBX to be imported first. NO gameplay bootstrap/scene replacement.
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Sora.AfterTheRain.Demo.Editor
{
    public static class SoraStreetVSliceSceneBuilder
    {
        private const string PrivateRoot = "Assets/SORA/Environment/StreetExterior/PrivateGenerated";
        private const string ModelPath = PrivateRoot + "/SORA_Street_VSlice_VS01.fbx";
        private const string ScenePath = PrivateRoot + "/SORA_Street_VSlice_Staging.unity";

        [MenuItem("SORA/Street Exterior/VS01 - Build staging scene")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("SORA VS01: Stop Play Mode first.");
                return;
            }
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (source == null)
            {
                EditorUtility.DisplayDialog("SORA VS01 - model not imported",
                    "Convert the local/private GLB to FBX with Blender, then copy FBX + Textures into:\n"
                    + PrivateRoot + "\n\nThe working SORA_Prototype.unity was not modified.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("SORA - Street Visual Slice VS01");
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(source, scene);
            instance.transform.SetParent(root.transform, false);
            instance.name = "Kitbashed architecture & SORA MART (visual prototype)";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            AddCamera(root.transform);
            AddMoon(root.transform);
            AddStagingColliders(root.transform);
            GameObject anchor = new GameObject("SORA MART ENTRY - anchor only");
            anchor.transform.SetParent(root.transform, false);
            anchor.transform.localPosition = new Vector3(0f, .1f, -.42f);
            Directory.CreateDirectory(PrivateRoot);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new IOException("Could not save local VS01 staging scene.");
            Selection.activeGameObject = root;
            Debug.Log("SORA VS01 visual staging saved: " + ScenePath
               + ". No SoraDemoCore/world generator was spawned. Validate import shaders and colliders manually.");
        }

        private static void AddCamera(Transform root)
        {
            GameObject go = new GameObject("VS01 Camera");
            go.transform.SetParent(root, false);
            go.tag = "MainCamera";
            go.transform.position = new Vector3(1.5f, 2.6f, -24f);
            go.transform.LookAt(new Vector3(0, 2.6f, 3.6f));
            Camera cam = go.AddComponent<Camera>();
            cam.fieldOfView = 62f;
            cam.nearClipPlane = .08f;
            cam.farClipPlane = 170f;
            go.AddComponent<AudioListener>();
        }

        private static void AddMoon(Transform root)
        {
            GameObject go = new GameObject("Moonlight - preview");
            go.transform.SetParent(root, false);
            go.transform.rotation = Quaternion.Euler(34f, -32f, 0f);
            Light light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(.54f, .65f, .87f);
            light.intensity = .48f;
            light.shadows = LightShadows.Soft;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.15f, .20f, .30f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(.095f, .13f, .19f);
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = .009f;
        }

        private static void Box(Transform root, string name, Vector3 center, Vector3 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.transform.position = center;
            BoxCollider collider = go.AddComponent<BoxCollider>();
            collider.size = size;
        }

        private static void AddStagingColliders(Transform root)
        {
            GameObject colliders = new GameObject("Temporary staging colliders - replace with authored collision");
            colliders.transform.SetParent(root, false);
            Box(colliders.transform, "road", new Vector3(0,-.31f,-8.5f), new Vector3(48,.2f,8));
            Box(colliders.transform, "north pavement", new Vector3(0,-.13f,-2.25f), new Vector3(48,.3f,4.5f));
            Box(colliders.transform, "south pavement", new Vector3(0,-.13f,-13.75f), new Vector3(48,.3f,2.5f));
            Box(colliders.transform, "SORA MART floor", new Vector3(0,-.1f,5.5f), new Vector3(14,.3f,11));
            Box(colliders.transform, "SORA MART rear wall", new Vector3(0,2.75f,10.84f), new Vector3(14,5.5f,.32f));
            Box(colliders.transform, "SORA MART left wall", new Vector3(-6.9f,2.75f,5.5f), new Vector3(.25f,5.5f,10.8f));
            Box(colliders.transform, "SORA MART right wall", new Vector3(6.9f,2.75f,5.5f), new Vector3(.25f,5.5f,10.8f));
            Box(colliders.transform, "Midori test facade", new Vector3(-11.8f,3.1f,3), new Vector3(6,6.2f,6));
            Box(colliders.transform, "Hoshi test facade", new Vector3(11.6f,3.1f,3.2f), new Vector3(6.2f,6.2f,6.5f));
        }
    }
}
#endif
