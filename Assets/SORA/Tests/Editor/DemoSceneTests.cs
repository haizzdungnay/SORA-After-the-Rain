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
                var oldShelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                oldShelf.name = "Grocery shelf base";
                oldShelf.transform.SetParent(root.transform, false);
                var lamp = new GameObject("Warm store lamp 0").AddComponent<Light>();
                lamp.transform.SetParent(root.transform, false);
                Assert.That(SoraMartVisuals.TryBuild(root.transform), Is.True);
                Assert.That(lamp.isActiveAndEnabled && lamp.range > lamp.transform.localPosition.y, Is.True,
                    "Ceiling fixture must reach the store floor.");
                Assert.That(lamp.transform.localPosition.y, Is.LessThan(3.0f), "Store light must be below the acoustic ceiling.");
                Assert.That(oldShelf.GetComponent<Collider>().enabled, Is.False, "Old wide shelf collider must be disabled.");
                Assert.That(marker.GetComponent<Renderer>().enabled, Is.False);
                Assert.That(marker.GetComponent<Collider>().enabled, Is.True);
                Assert.That(root.GetComponentsInChildren<SoraInteractable>(), Has.Length.EqualTo(1));
                var store = root.transform.Find(SoraMartVisuals.RootName);
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
                var renderers = store.GetComponentsInChildren<MeshRenderer>();
                foreach (string source in new[] { "gondola", "endcap", "checkout", "back_fixture", "pos", "stock_shelf", "goods",
                    "chilled_food", "bakery_rack", "magazine_rack", "basket_stack", "hot_food", "microwave",
                    "coffee_brewer", "card_reader", "receipt_printer", "scanner", "register_console", "cash_tray", "packaged_goods" })
                    Assert.That(renderers.Any(r => r.name.StartsWith("IMPORTED | " + source + " | ")), Is.True,
                        "Imported source missing from the actual runtime GLB: " + source);
                var packaged = renderers.Where(r => r.name.StartsWith("IMPORTED | packaged_goods | "))
                    .SelectMany(r => r.sharedMaterials).Distinct().ToArray();
                Assert.That(packaged.Length, Is.InRange(10, 24), "Shared retail atlases must not create one material per product.");
                Assert.That(packaged.All(m => m.GetTexturePropertyNames().Any(property =>
                    m.GetTexture(property) != null && m.GetTexture(property).width > 4)), Is.True,
                    "Retail packaging must retain actual source textures.");
                Assert.That(store.GetComponentsInChildren<MeshCollider>().Length, Is.GreaterThan(12));
                Physics.SyncTransforms();
                Assert.That(Physics.SphereCast(new Vector3(0, 1.75f, -2), .18f, Vector3.up, out var ceilingHit, 3), Is.True);
                Assert.That(ceilingHit.collider.name, Is.EqualTo("Interior ceiling shelter"), "Camera must stop below the visible ceiling.");
                for (int z = -5; z <= 4; z++)
                    Assert.That(Physics.OverlapCapsule(new Vector3(0, .32f, z), new Vector3(0, 1.55f, z), .28f)
                        .Any(c => c.transform.IsChildOf(store)), Is.False, "Blocked main aisle at z=" + z);
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
