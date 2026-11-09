using NUnit.Framework;
using UnityEngine;

namespace Sora.AfterTheRain.Demo.Tests
{
    /// <summary>
    /// These are REAL Unity Editor tests, unlike the Python-only repository checks.
    /// They will be executed only by a configured, licensed Unity Editor runner.
    /// </summary>
    public sealed class SoraCityRealAssetTests
    {
        [Test]
        public void PublicV03V05AndV06ExistAsImportedUnityGameObjects()
        {
            GameObject v03 = Resources.Load<GameObject>(SoraCityRuntimeWorld.PublicV03Path);
            GameObject v05 = Resources.Load<GameObject>(SoraCityRuntimeWorld.PublicV05Path);
            GameObject v06 = Resources.Load<GameObject>(SoraCityRuntimeWorld.PublicV06Path);
            Assert.That(v03, Is.Not.Null, "glTFast must import the V03 CC0 GLB as a Unity GameObject.");
            Assert.That(v05, Is.Not.Null, "glTFast must import the Kenney V05 GLB as a Unity GameObject.");
            Assert.That(v06, Is.Not.Null, "glTFast must import the V06 60-storefront-detail GLB.");

            Assert.That(v03.GetComponentsInChildren<MeshRenderer>(true).Length, Is.GreaterThan(100),
                "V03 GLB should contain hundreds of actual building/prop render meshes.");
            Assert.That(v05.GetComponentsInChildren<MeshRenderer>(true).Length, Is.GreaterThan(100),
                "V05 GLB should contain real buildings, roads and Kenney cars.");
            Assert.That(v06.GetComponentsInChildren<MeshRenderer>(true).Length, Is.GreaterThan(100),
                "V06 GLB must import thousands of authored shopfront detail meshes.");
        }

        [Test]
        public void RuntimeCollisionManifestIncludesParkedVehiclesAndClosedPerimeter()
        {
            TextAsset asset = Resources.Load<TextAsset>("SoraCity/V05_COLLIDERS");
            Assert.That(asset, Is.Not.Null);
            var manifest = JsonUtility.FromJson<CollisionManifest>(asset.text);
            Assert.That(manifest, Is.Not.Null);
            Assert.That(manifest.items, Has.Length.EqualTo(122));

            int vehicles = 0, walls = 0, roadEnds = 0, buildings = 0;
            foreach (CollisionBox box in manifest.items)
            {
                Assert.That(box.sx, Is.GreaterThan(0));
                Assert.That(box.sy, Is.GreaterThan(0));
                Assert.That(box.sz, Is.GreaterThan(0));
                switch (box.group)
                {
                    case "VEHICLE": vehicles++; break;
                    case "HARD_BOUNDARY": walls++; break;
                    case "ROAD_BLOCK": roadEnds++; break;
                    case "BUILDING": buildings++; break;
                }
            }

            Assert.That(vehicles, Is.EqualTo(42));
            Assert.That(buildings, Is.EqualTo(60));
            Assert.That(walls, Is.EqualTo(4));
            Assert.That(roadEnds, Is.EqualTo(16));
        }

        [System.Serializable]
        private class CollisionManifest { public CollisionBox[] items; }

        [System.Serializable]
        private class CollisionBox
        {
            public string id, group;
            public float cx, cy, cz, sx, sy, sz;
        }
    }
}
