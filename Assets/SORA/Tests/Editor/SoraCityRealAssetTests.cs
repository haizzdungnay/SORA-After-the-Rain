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
        public void RuntimeVehiclesAlignWithTheirCollisionProxies()
        {
            var ownerObject = new GameObject("City alignment test owner");
            var world = new GameObject("City alignment test world");
            try
            {
                var owner = ownerObject.AddComponent<SoraDemoCore>();
                Assert.That(SoraCityRuntimeWorld.TryBuild(world.transform, owner), Is.True);
                var renderers = world.GetComponentsInChildren<MeshRenderer>();
                int checkedVehicles = 0;
                foreach (var collider in world.GetComponentsInChildren<BoxCollider>())
                {
                    if (!collider.name.StartsWith("VEHICLE - ")) continue;
                    string id = collider.name.Substring("VEHICLE - ".Length);
                    Bounds? bounds = null;
                    foreach (var renderer in renderers)
                    {
                        if (!renderer.name.StartsWith("V05/KENNEY_CAR/" + id + "/")) continue;
                        if (bounds.HasValue)
                        {
                            Bounds combined = bounds.Value;
                            combined.Encapsulate(renderer.bounds);
                            bounds = combined;
                        }
                        else bounds = renderer.bounds;
                    }
                    Assert.That(bounds.HasValue, Is.True, "Missing vehicle geometry: " + id);
                    Vector3 expected = collider.transform.TransformPoint(collider.center);
                    Assert.That(bounds.Value.center.x, Is.EqualTo(expected.x).Within(.08f), id + " X alignment");
                    Assert.That(bounds.Value.center.z, Is.EqualTo(expected.z).Within(.08f), id + " Z alignment");
                    Assert.That(bounds.Value.size.x, Is.LessThanOrEqualTo(collider.size.x + .05f), id + " X coverage");
                    Assert.That(bounds.Value.size.z, Is.LessThanOrEqualTo(collider.size.z + .05f), id + " Z coverage");
                    checkedVehicles++;
                }
                Assert.That(checkedVehicles, Is.EqualTo(42));
                Assert.That(world.transform.Find("SORA_CITY_V05_REAL_ASSETS - 9 blocks - 42 Kenney vehicles/V02 original building collisions").childCount,
                    Is.EqualTo(15), "Original V03 shops also need collision; exclude the interactive SORA MART.");
            }
            finally
            {
                Object.DestroyImmediate(world);
                Object.DestroyImmediate(ownerObject);
            }
        }

        [Test]
        public void BoundaryRecoveryDoesNotRememberFallingPositions()
        {
            var ownerObject = new GameObject("Boundary recovery test owner");
            try
            {
                var owner = ownerObject.AddComponent<SoraDemoCore>();
                var guard = ownerObject.AddComponent<SoraCityBoundaryGuard>();
                guard.Initialize(owner, new Vector3(-100, -5, -100), new Vector3(100, 40, 100));
                for (int i = 0; i < 2; i++)
                {
                    var twin = new GameObject("Boundary test twin " + i);
                    twin.transform.SetParent(ownerObject.transform);
                    owner.Characters.Add(twin.AddComponent<SoraDemoCharacter>());
                    twin.GetComponent<CharacterController>().enabled = false;
                    twin.transform.position = new Vector3(i, -3, 0);
                }
                var update = typeof(SoraCityBoundaryGuard).GetMethod("LateUpdate",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                update.Invoke(guard, null);
                foreach (var twin in owner.Characters) twin.transform.position = new Vector3(0, -10, 0);
                update.Invoke(guard, null);
                foreach (var twin in owner.Characters)
                    Assert.That(twin.transform.position.y, Is.GreaterThanOrEqualTo(0),
                        "Recovery must not restore an airborne point below the floor.");
            }
            finally { Object.DestroyImmediate(ownerObject); }
        }

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

        [Test]
        public void StreetPropSpotsStayOnMapAndAvoidRoadSurface()
        {
            Assert.That(SoraCityStreetProps.SignalSpots().Count, Is.EqualTo(64));
            Assert.That(SoraCityStreetProps.SignSpots().Count, Is.EqualTo(128));
            Assert.That(SoraCityStreetProps.ConeSpots().Count, Is.EqualTo(32));
            foreach (var s in SoraCityStreetProps.LampSpots())
            {
                Assert.That(s.Xz.x, Is.InRange(-70f, 115f));
                Assert.That(s.Xz.y, Is.InRange(-70f, 115f));
            }
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
