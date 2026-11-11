using System.Linq;
using UnityEngine;

namespace Sora.AfterTheRain.Demo
{
    public static class SoraMartVisuals
    {
        public const string ResourcePath = "SoraStore/SoraMart";
        public const string RootName = "SORA MART - licensed model store";
        public static readonly Bounds Interior = new Bounds(new Vector3(0, 2, .1f), new Vector3(13.6f, 3.6f, 10.6f));

        public static bool TryBuild(Transform world)
        {
            var asset = Resources.Load<GameObject>(ResourcePath);
            if (world == null || asset == null || asset.GetComponentsInChildren<MeshRenderer>().Length == 0)
            {
                Debug.LogWarning("SORA MART: authored model unavailable; keeping prototype store visuals.");
                return false;
            }

            // Keep interaction markers and lights; match fixture colliders to the new art.
            var previous = world.GetComponentsInChildren<Renderer>();
            var store = Object.Instantiate(asset, world, false);
            store.name = RootName;
            store.transform.localPosition = Vector3.zero;
            store.transform.localRotation = Quaternion.identity;
            store.transform.localScale = Vector3.one;
            foreach (var renderer in previous)
            {
                renderer.enabled = false;
                if (renderer.name == "Grocery shelf base" || renderer.name == "POS checkout counter")
                    renderer.GetComponent<Collider>().enabled = false;
            }

            // Static fixture meshes keep collision aligned when the source layout changes.
            var solids = new[] { "gondola", "endcap", "checkout", "back_fixture", "stock_shelf",
                "chilled_food", "bakery_rack", "magazine_rack", "basket_stack", "umbrella_stand",
                "hot_food", "delivery_boxes" };
            foreach (var filter in store.GetComponentsInChildren<MeshFilter>())
                if (solids.Any(key => filter.name.StartsWith("IMPORTED | " + key + " | ")))
                    filter.gameObject.AddComponent<MeshCollider>().sharedMesh = filter.sharedMesh;

            Solid(store.transform, "Store roof shelter", new Vector3(0, 4.4f, .1f), new Vector3(14.45f, .2f, 11.1f));
            Solid(store.transform, "Store canopy shelter", new Vector3(0, 3.83f, -5.95f), new Vector3(14.5f, .2f, 1.95f));
            Solid(store.transform, "Chilled drinks cabinet", new Vector3(6.35f, 1.45f, -.85f), new Vector3(.85f, 2.9f, 5.95f));
            Solid(store.transform, "Coffee service counter", new Vector3(-6.25f, .55f, 1.6f), new Vector3(1.05f, 1.1f, 2.6f));
            Solid(store.transform, "Vending machine", new Vector3(-6.15f, .95f, -6.05f), new Vector3(1.02f, 1.9f, .85f));
            Solid(store.transform, "Recycling bins", new Vector3(5.15f, .45f, -6.25f), new Vector3(.95f, .9f, .6f));

            foreach (var lamp in world.GetComponentsInChildren<Light>())
            {
                if (lamp.name.StartsWith("Warm store lamp "))
                {
                    lamp.type = LightType.Spot;
                    lamp.transform.rotation = Quaternion.Euler(90, 0, 0);
                    lamp.spotAngle = 120;
                    lamp.innerSpotAngle = 100;
                    lamp.intensity = 4;
                    lamp.range = 7;
                }
                else if (lamp.name == "Front neon")
                {
                    lamp.color = new Color(.83f, 1f, .92f);
                    lamp.intensity = 1.5f;
                }
            }
            // ponytail: decorative stock; bind individual displays when inventory gains per-SKU state.
            Debug.Log("SORA MART: authored exterior/interior loaded; existing POS, stock and save anchors retained.");
            return true;
        }

        public static void FitCityNeighbors(GameObject v03, GameObject v05)
        {
            var bay = v03.GetComponentsInChildren<MeshRenderer>().Where(r =>
                r.name.StartsWith("V03/CC0_FACADE/NE-02/0/")).ToArray();
            if (bay.Length > 0)
            {
                Bounds bounds = bay[0].bounds;
                foreach (var renderer in bay) bounds.Encapsulate(renderer.bounds);
                if (bounds.Intersects(Interior))
                {
                    // NE-02 is centered at -15.3 m; its first 4.85 m bay belongs beside the store.
                    Vector3 offset = new Vector3(-12.875f - bounds.center.x, 0, -5.25f - bounds.center.z);
                    foreach (var renderer in bay) renderer.transform.position += offset;
                }
            }
            foreach (var renderer in v05.GetComponentsInChildren<MeshRenderer>())
            {
                bool courtyard = renderer.name.StartsWith("V04/CLOSE_GAPS/OLD-04/courtyard_") ||
                                 renderer.name.StartsWith("V04/COURTYARD/B05/");
                if (!courtyard || renderer.bounds.size.y < 2 || !renderer.bounds.Intersects(Interior)) continue;
                bool end = renderer.bounds.size.x > renderer.bounds.size.z;
                Vector3 min = renderer.bounds.min, max = renderer.bounds.max;
                if (end) max.x = Mathf.Min(max.x, -7.25f);
                else min.z = Mathf.Max(min.z, 5.7f);
                renderer.enabled = false;
                if (min.x >= max.x || min.z >= max.z) continue;
                var remainder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                remainder.name = "Store boundary remainder - " + (end ? "west" : "rear");
                remainder.transform.SetParent(v05.transform, true);
                remainder.transform.position = (min + max) * .5f;
                remainder.transform.localScale = max - min;
                remainder.GetComponent<MeshRenderer>().sharedMaterial = renderer.sharedMaterial;
                var collider = remainder.GetComponent<Collider>();
                collider.enabled = false;
                if (Application.isPlaying) Object.Destroy(collider); else Object.DestroyImmediate(collider);
            }
        }

        private static void Solid(Transform root, string name, Vector3 center, Vector3 size)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(root, false);
            obj.transform.localPosition = center;
            obj.AddComponent<BoxCollider>().size = size;
        }
    }
}
