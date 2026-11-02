#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sora.AfterTheRain.Demo.Editor
{
    public static class SoraDemoSceneGenerator
    {
        private const string PathToScene = "Assets/SORA/Scenes/SORA_Prototype.unity";

        [MenuItem("SORA/1. Generate Demo Scene")]
        public static void Generate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("SORA", "Stop Play Mode before generating the demo scene.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            SoraProjectSetup.Configure();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("SORA GAME - Press Play to spawn prototype environment");
            root.AddComponent<SoraDemoCore>();
            Directory.CreateDirectory("Assets/SORA/Scenes");
            EditorSceneManager.SaveScene(scene, PathToScene);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(PathToScene, true) };
            AssetDatabase.Refresh();
            Selection.activeGameObject = root;
            EditorUtility.DisplayDialog("SORA Demo Ready",
                "Scene created! Open Assets/SORA/Scenes/SORA_Prototype.unity and click Play.\n\nIf Input errors appear, choose Edit > Project Settings > Player > Active Input Handling: Both.", "OK");
        }

        [MenuItem("SORA/2. Open Demo Scene")]
        public static void Open()
        {
            if (!File.Exists(PathToScene)) { Generate(); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(PathToScene);
        }
    }
}
#endif
