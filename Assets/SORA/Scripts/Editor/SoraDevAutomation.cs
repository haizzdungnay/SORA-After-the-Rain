#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sora.AfterTheRain.Demo.Editor
{
    /// <summary>
    /// Editor-only entrypoints for unattended scene generation and Windows builds.
    /// Usage: Unity.exe -batchmode -nographics -quit -projectPath ...
    ///        -executeMethod Sora.AfterTheRain.Demo.Editor.SoraDevAutomation.BuildWindows
    /// </summary>
    public static class SoraDevAutomation
    {
        public const string DemoScene = "Assets/SORA/Scenes/SORA_Prototype.unity";

        [MenuItem("SORA/3. Create Scene (Noninteractive)")]
        public static void GenerateScene()
        {
            // Unlike the interactive generator, this method does not open dialogs.
            SoraProjectSetup.Configure();
            Directory.CreateDirectory("Assets/SORA/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("SORA GAME - Press Play to spawn prototype environment");
            root.AddComponent<SoraDemoCore>();
            if (!EditorSceneManager.SaveScene(scene, DemoScene))
                throw new Exception("SORA: could not save generated demo scene.");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(DemoScene, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("SORA: generated scene " + DemoScene);
        }

        [MenuItem("SORA/4. Build Windows x64")]
        public static void BuildWindows()
        {
            GenerateScene();
            var options = new BuildPlayerOptions
            {
                scenes = new[] { DemoScene },
                locationPathName = "Builds/Windows/SORA_After_the_Rain.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            Directory.CreateDirectory("Builds/Windows");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report == null || report.summary.result != BuildResult.Succeeded)
                throw new Exception("SORA: Windows build FAILED; inspect Editor.log.");
            Debug.Log("SORA: Windows build complete: " + options.locationPathName);
        }

        public static void CompileSmokeCheck()
        {
            GenerateScene();
            Debug.Log("SORA: smoke check passed (Editor loaded and scene generated); NOT a Play Mode test.");
        }
    }
}
#endif
