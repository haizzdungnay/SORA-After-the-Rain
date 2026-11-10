using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sora.AfterTheRain.Demo.Tests
{
    public sealed class DemoSceneTests
    {
        [Test]
        public void AuthoredStoreRetainsInteractionColliderAndHasTransparentGlassAndRoofShelter()
        {
            var root = new GameObject("Store art regression");
            try
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.transform.SetParent(root.transform, false);
                var register = marker.AddComponent<SoraInteractable>();
                register.kind = InteractKind.Register;
                Assert.That(SoraMartVisuals.TryBuild(root.transform), Is.True);
                Assert.That(marker.GetComponent<Renderer>().enabled, Is.False);
                Assert.That(marker.GetComponent<Collider>().enabled, Is.True);
                Assert.That(root.GetComponentsInChildren<SoraInteractable>(), Has.Length.EqualTo(1));
                var store = root.transform.Find("SORA MART - authored CC0 store");
                Assert.That(store, Is.Not.Null);
                Assert.That(store.localPosition, Is.EqualTo(Vector3.zero));
                Assert.That(store.localScale, Is.EqualTo(Vector3.one));
                var materials = store.GetComponentsInChildren<MeshRenderer>().SelectMany(r => r.sharedMaterials).ToArray();
                Assert.That(materials.All(m => m != null && m.shader.isSupported), Is.True);
                Assert.That(materials.Any(m => m.name.Contains("clear storefront glass") && m.renderQueue >= 3000), Is.True);
                Assert.That(store.Find("Store roof shelter").GetComponent<BoxCollider>().bounds.Contains(new Vector3(0, 4.4f, 0)), Is.True);
                Assert.That(store.Find("Store canopy shelter").GetComponent<BoxCollider>().bounds.Contains(new Vector3(0, 3.83f, -6)), Is.True);
                Assert.That(store.GetComponentsInChildren<BoxCollider>().Any(c =>
                    c.bounds.Contains(new Vector3(0, 1, -5.25f))), Is.False, "Doorway must remain clear.");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

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
