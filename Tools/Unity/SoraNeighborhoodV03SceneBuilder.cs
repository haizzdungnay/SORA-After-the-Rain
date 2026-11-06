// Unity 6000.3.7f1 / URP 17.3.0
// Owner-private V02 GLB/FBX + public CC0-only V03 FBX overlay.
// Copy this script to Assets/SORA/Scripts/Editor/ on the Unity workstation.
// It builds a SEPARATE art-only staging scene; existing gameplay bootstrap is not altered.
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
    public static class SoraNeighborhoodV03SceneBuilder
    {
        const string PrivateDir = "Assets/SORA/Environment/StreetExterior/PrivateGenerated";
        const string V02Base = PrivateDir + "/SORA_Neighborhood_V02_Grid.fbx";
        const string V03Overlay = PrivateDir + "/SORA_V03_CC0_Overlay.fbx";
        const string SceneFile = "Assets/SORA/Scenes/SORA_Neighborhood_V03_Staging.unity";

        [MenuItem("SORA/Street Exterior/V03 - Create art staging scene")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("Stop Play Mode before creating a staged V03 scene.");
                return;
            }
            GameObject overlay = AssetDatabase.LoadAssetAtPath<GameObject>(V03Overlay);
            GameObject baseModel = AssetDatabase.LoadAssetAtPath<GameObject>(V02Base);
            if (overlay == null)
            {
                EditorUtility.DisplayDialog("Missing SORA V03 FBX",
                    "Convert SORA_V03_CC0_Overlay.glb with Blender to FBX and import its textures into "
                    + PrivateDir + ". The original SORA_Prototype.unity has not been modified.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("SORA Neighborhood V03 - independent art staging");

            if (baseModel != null)
                Instance(baseModel, "Private V02 base - do NOT push derived AFX model to public repo", root.transform);
            else
                Debug.LogWarning("V02 base missing. Only CC0 overlay will be visible; roads/old shops are absent.");

            Instance(overlay, "V03 CC0 facade, infill, prop and road overlay", root.transform);
            CreateCamera(root.transform);
            CreatePreviewLights(root.transform);
            CreateCollision(root.transform);
            CreateAnchors(root.transform);
            CreatePreviewRain(root.transform);

            Directory.CreateDirectory("Assets/SORA/Scenes");
            if (!EditorSceneManager.SaveScene(scene, SceneFile))
                throw new IOException("Could not save scene " + SceneFile);
            AssetDatabase.Refresh();
            Selection.activeGameObject = root;
            Debug.Log("Created V03 art-only staging scene. No SoraDemoCore, POS or legacy world spawners were instantiated. "
                      + "TEST imported URP materials, transforms, visible backs, camera clipping and gameplay integration separately.");
        }

        static void Instance(GameObject asset, string name, Transform root)
        {
            GameObject copy = (GameObject)PrefabUtility.InstantiatePrefab(asset, root.gameObject.scene);
            copy.transform.SetParent(root, false);
            copy.transform.localPosition = Vector3.zero;
            copy.transform.localRotation = Quaternion.identity;
            copy.transform.localScale = Vector3.one;
            copy.name = name;
        }

        static void CreateCamera(Transform root)
        {
            GameObject go = new GameObject("V03 overview camera");
            go.tag = "MainCamera";
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(28f, 23f, -40f);
            go.transform.LookAt(new Vector3(17f, 1.8f, 8f));
            Camera cam = go.AddComponent<Camera>();
            cam.fieldOfView = 58f;
            cam.nearClipPlane = .08f;
            cam.farClipPlane = 260f;
            go.AddComponent<AudioListener>();
        }

        static void CreatePreviewLights(Transform root)
        {
            GameObject moon = new GameObject("V03 moon / debug directional light");
            moon.transform.SetParent(root, false);
            moon.transform.rotation = Quaternion.Euler(38f, -30f, 0f);
            Light directional = moon.AddComponent<Light>();
            directional.type = LightType.Directional;
            directional.intensity = .85f;
            directional.color = new Color(.65f, .77f, 1f);
            directional.shadows = LightShadows.Soft;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.25f, .32f, .44f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = .006f;
            RenderSettings.fogColor = new Color(.07f, .11f, .17f);

            // Eight limited-range lights, not one real-time light per storefront.
            foreach (float x in new float[] { -45f, 0f, 45f })
                foreach (float z in new float[] { -22.5f, 22.5f })
                    AddPoint(root, "Street lamp preview", new Vector3(x + 6f, 4.9f, z), 8f, 10f);
            AddPoint(root, "SORA MART entrance preview", new Vector3(17f, 3.1f, 5.8f), 5f, 9f);
            AddPoint(root, "SORA MART inner preview", new Vector3(17f, 3f, 10f), 3f, 8f);
        }

        static void AddPoint(Transform root, string name, Vector3 position, float range, float intensity)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.transform.localPosition = position;
            Light light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = range;
            light.intensity = intensity;
            light.color = new Color(1f, .83f, .61f);
            light.shadows = LightShadows.None;
        }

        static void Box(Transform parent, string label, Vector3 at, Vector3 dimensions)
        {
            GameObject go = new GameObject(label);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = at;
            BoxCollider box = go.AddComponent<BoxCollider>();
            box.size = dimensions;
        }

        static void CreateCollision(Transform root)
        {
            GameObject boundary = new GameObject("VS03 rough navigation collision - QA required");
            boundary.transform.SetParent(root, false);
            foreach (float c in new float[] { -45f, 0f, 45f })
            {
                Box(boundary.transform, "EW road surface", new Vector3(0f, -.18f, c), new Vector3(126f, .20f, 9f));
                Box(boundary.transform, "NS road surface", new Vector3(c, -.18f, 0f), new Vector3(9f, .20f, 126f));
            }
            foreach (float x in new float[] { -22.5f, 22.5f })
                foreach (float z in new float[] { -22.5f, 22.5f })
                    Box(boundary.transform, "Block walkable surface", new Vector3(x, -.075f, z),
                        new Vector3(36f, .16f, 36f));
            // SORA MART shell-side guards only: DO NOT block central entrance.
            Box(boundary.transform, "SORA MART rear", new Vector3(17f, 2.7f, 17.55f),
                new Vector3(14f, 5.4f, .3f));
            Box(boundary.transform, "SORA MART east wall", new Vector3(23.9f, 2.7f, 12.1f),
                new Vector3(.25f, 5.4f, 10.8f));
            Box(boundary.transform, "SORA MART west wall", new Vector3(10.1f, 2.7f, 12.1f),
                new Vector3(.25f, 5.4f, 10.8f));
        }

        static void CreateAnchors(Transform root)
        {
            GameObject anchors = new GameObject("Reference anchors - NOT gameplay objects");
            anchors.transform.SetParent(root, false);
            GameObject mart = new GameObject("F-SORA-001 entrance reference");
            mart.transform.SetParent(anchors.transform, false);
            mart.transform.localPosition = new Vector3(17f, .1f, 6.75f);
            // Both twins, POS and persisted player positions are still owned by SoraDemoCore.
        }

        static void CreatePreviewRain(Transform root)
        {
            GameObject go = new GameObject("Wide-area preview drizzle - not roof-aware or final");
            go.transform.SetParent(root, false);
            go.transform.localPosition = new Vector3(0, 22f, 0);
            go.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            ParticleSystem particles = go.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = true;
            main.startLifetime = 1.8f;
            main.startSpeed = 13f;
            main.startSize = .035f;
            main.maxParticles = 1800;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = particles.emission;
            emission.rateOverTime = 600f;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(126, 126, .2f);
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            // Renderer/material still needs URP tuning. This is not EX-09 acceptance.
        }
    }
}
#endif
