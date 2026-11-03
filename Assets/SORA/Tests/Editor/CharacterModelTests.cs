using NUnit.Framework;
using UnityEngine;

namespace Sora.AfterTheRain.Demo.Tests
{
    public sealed class CharacterModelTests
    {
        [Test]
        public void AkikoModelLoadsAndFitsHerCharacterController()
        {
            var prefab = Resources.Load<GameObject>(SoraDemoWorld.AkikoModelResource);
            Assert.That(prefab, Is.Not.Null, "Run SORA/5. Import Tei Akiko Model.");
            var go = Object.Instantiate(prefab);
            try
            {
                var renderers = go.GetComponentsInChildren<Renderer>();
                Assert.That(renderers, Is.Not.Empty);
                Bounds b = renderers[0].bounds;
                foreach (var r in renderers)
                {
                    b.Encapsulate(r.bounds);
                    foreach (var m in r.sharedMaterials) Assert.That(m, Is.Not.Null, r.name + " has a missing material.");
                }
                // Feet on the root pivot; height below Akiko's 1.72 m controller.
                Assert.That(b.min.y, Is.EqualTo(0f).Within(0.03f));
                Assert.That(b.size.y, Is.InRange(1.45f, 1.72f));
                Assert.That(go.GetComponentsInChildren<Collider>(), Is.Empty, "Model colliders would fight the CharacterController.");
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
