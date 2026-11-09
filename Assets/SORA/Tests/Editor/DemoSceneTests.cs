using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Sora.AfterTheRain.Demo.Tests
{
    public sealed class DemoSceneTests
    {
        [Test]
        public void PlayableSceneContainsOneEnabledBootstrapAndIsInBuild()
        {
            const string path = "Assets/SORA/Scenes/SORA_Prototype.unity";
            Assert.That(EditorBuildSettings.scenes.Any(s => s.enabled && s.path == path), Is.True);
            Scene scene = SceneManager.GetSceneByPath(path);
            bool openedForTest = !scene.isLoaded;
            if (openedForTest) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                var cores = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<SoraDemoCore>(true)).ToArray();
                Assert.That(cores.Length, Is.EqualTo(1));
                Assert.That(cores[0].isActiveAndEnabled, Is.True);
            }
            finally
            {
                if (openedForTest) EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
