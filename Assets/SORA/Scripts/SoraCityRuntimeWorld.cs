using System;
using UnityEngine;

namespace Sora.AfterTheRain.Demo
{
    /// <summary>
    /// ACTUAL playable-world art bootstrap for the owner-supplied Japanese city.
    ///
    /// glTFast imports the committed .glb files in Assets/Resources/SoraCity/Models
    /// into Unity GameObject assets. Resources.Load then instantiates those REAL meshes
    /// in the SAME world as the existing POS, twins, camera, customer and save state.
    ///
    /// V05 is a full REPLACEMENT for V04. NEVER instantiate V04 and V05 together.
    /// Optional owner-private V03_Integrated_NoMart.glb replaces the public CC0 V03
    /// art and contains the rights-restricted AFX-derived architecture. Not in Git.
    /// </summary>
    public static class SoraCityRuntimeWorld
    {
        public const string PublicV03Path = "SoraCity/Models/V03_CC0_Overlay";
        public const string PublicV05Path = "SoraCity/Models/V05_Kenney_9Block_Overlay";
        public const string PublicV06Path = "SoraCity/Models/V06_Facade_Details";
        public const string PrivateV03Path = "SoraCity/Private/V03_Integrated_NoMart";
        public const string PrivateV05Path = "SoraCity/Private/V05_Kenney_9Block_Overlay";

        // Offset the glTFast storefront (-17, 0, 6.75) onto the interactive entrance (0, 0, -5.25).
        public static readonly Vector3 CityToGameOffset = new Vector3(17f, 0f, -12f);

        private static Vector3 AuthorToUnity(Vector3 point) => new Vector3(-point.x, point.y, point.z);

        [Serializable]
        private sealed class CityColliderManifest { public CityCollider[] items; }

        [Serializable]
        private sealed class CityCollider
        {
            public string id;
            public string group;
            public float cx, cy, cz, sx, sy, sz;
        }

        [Serializable]
        private sealed class V02Layout { public V02Shop[] shops; }

        [Serializable]
        private sealed class V02Shop
        {
            public string id;
            public float[] front_xyz;
            public float yaw_degrees;
            public float[] nominal_footprint_m;
            public float nominal_height_m;
        }

        public static bool TryBuild(Transform world, SoraDemoCore owner)
        {
            if (world == null || owner == null) return false;

            // A hard missing-asset fallback keeps the old playable game functional.
            GameObject privateV03 = Resources.Load<GameObject>(PrivateV03Path);
            GameObject privateV05 = privateV03 != null ? Resources.Load<GameObject>(PrivateV05Path) : null;
            GameObject v05Asset = privateV05 != null ? privateV05 : Resources.Load<GameObject>(PublicV05Path);
            GameObject v06Asset = privateV05 == null ? Resources.Load<GameObject>(PublicV06Path) : null;
            GameObject v03Asset = privateV03 != null ? privateV03 :
                Resources.Load<GameObject>(PublicV03Path);
            TextAsset collisionFile = Resources.Load<TextAsset>(privateV05 != null ?
                "SoraCity/Private/V05_COLLIDERS" : "SoraCity/V05_COLLIDERS");
            TextAsset layoutFile = Resources.Load<TextAsset>("SoraCity/V02_GRID_LAYOUT");
            if (v05Asset == null || v03Asset == null || collisionFile == null || layoutFile == null)
            {
                Debug.LogWarning("SORA CITY: 3D assets/layout unavailable. " +
                    "Keeping the previous interactive procedural neighborhood. " +
                    "Check Git LFS, glTFast and Assets/Resources/SoraCity.");
                return false;
            }

            CityColliderManifest proxyData;
            V02Layout layout;
            try
            {
                proxyData = JsonUtility.FromJson<CityColliderManifest>(collisionFile.text);
                layout = JsonUtility.FromJson<V02Layout>(layoutFile.text);
                if (proxyData == null || proxyData.items == null || proxyData.items.Length != 122 ||
                    layout == null || layout.shops == null || layout.shops.Length != 16)
                    throw new InvalidOperationException("Bad V05 collider or V02 parcel catalog");
            }
            catch (Exception e)
            {
                Debug.LogError("SORA CITY: refusing partially configured world: " + e.Message);
                return false;
            }

            Transform city = null;
            try
            {
                city = new GameObject("SORA_CITY_V05_REAL_ASSETS - 9 blocks - 42 Kenney vehicles").transform;
                city.SetParent(world, false);
                city.localPosition = CityToGameOffset;
                city.localRotation = Quaternion.identity;
                city.localScale = Vector3.one;

                GameObject v03 = UnityEngine.Object.Instantiate(v03Asset, city, false);
                v03.name = privateV03 != null ?
                    "V03 AFX + CC0 city buildings [OWNER PRIVATE REAL GLB]" :
                    "V03 CC0 real shopfronts / props / asphalt [GIT LFS GLB]";
                v03.transform.localPosition = Vector3.zero;
                v03.transform.localRotation = Quaternion.identity;
                v03.transform.localScale = Vector3.one;

                GameObject v05 = UnityEngine.Object.Instantiate(v05Asset, city, false);
                v05.name = privateV05 != null ? "V05 owner ZIP + matching collisions [LOCAL GLB]" :
                    "V05 real Kenney Car Kit + 60 building shells + roads [GIT LFS GLB]";
                v05.transform.localPosition = Vector3.zero;
                v05.transform.localRotation = Quaternion.identity;
                v05.transform.localScale = Vector3.one;
                if (world.Find("SORA MART - authored CC0 store") != null)
                    SoraMartVisuals.FitCityNeighbors(v03, v05);

                // V06 is additive ART DETAIL ONLY: 60 storefronts / 3,301 nodes.
                // It must load once, at the same transform as V05, without collider duplication.
                int v06Renderers = 0;
                // V06 targets the PUBLIC V05 hash; the private ZIP has different facades/heights.
                if (v06Asset != null && privateV05 == null)
                {
                    GameObject detail = UnityEngine.Object.Instantiate(v06Asset, city, false);
                    detail.name = "V06 REAL facade details / signage / awnings [GIT LFS GLB]";
                    detail.transform.localPosition = Vector3.zero;
                    detail.transform.localRotation = Quaternion.identity;
                    detail.transform.localScale = Vector3.one;
                    v06Renderers = detail.GetComponentsInChildren<MeshRenderer>(true).Length;
                }
                else if (privateV05 == null) Debug.LogWarning("SORA CITY: V06 addon model missing; check GLB import.");

                // Without the rights-restricted source we still need the 15 original
                // building bodies; the public V03 GLB provides their TRUE CC0 facades.
                if (privateV03 == null) BuildMissingV02Shells(city, layout);
                BuildOriginalBuildingCollisions(city, layout);
                AddGroundColliders(v03);
                AddGroundColliders(v05);
                RenderSettings.skybox = null;
                var moon = new GameObject("City night fill");
                moon.transform.SetParent(city, false);
                moon.transform.localRotation = Quaternion.Euler(48f, -30f, 0f);
                var light = moon.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(.58f, .70f, 1f);
                light.intensity = .7f;

                // Imported GLBs are render meshes, NOT collision geometry.
                BuildCollisionProxies(city, proxyData);
                int streetProps = SoraCityStreetProps.Build(city);
                int importedRenderers = v03.GetComponentsInChildren<MeshRenderer>(true).Length +
                                        v05.GetComponentsInChildren<MeshRenderer>(true).Length + v06Renderers;
                if (importedRenderers < 50)
                    throw new InvalidOperationException("glTF files did not instantiate usable MeshRenderers (" +
                                                        importedRenderers + "). Check glTFast import.");

                // Both existing character controllers remain owned by SoraDemoCore;
                // the guard is a defensive fallback, not a replacement state system.
                var guard = world.gameObject.AddComponent<SoraCityBoundaryGuard>();
                guard.Initialize(owner, CityToGameOffset + new Vector3(-107f, -5f, -62f),
                                        CityToGameOffset + new Vector3(62f, 40f, 107f));

                Debug.Log("SORA CITY: REAL V03 + V05" + (privateV05 == null ? " + V06" : " owner ZIP pair") +
                    " GLB meshes instantiated in the playable world. " +
                    "Imported renderer count=" + importedRenderers +
                    ", street props=" + streetProps + ", collision proxies=" + proxyData.items.Length +
                    ", private AFX=" + (privateV03 != null) +
                    ". POS / twins / save remain owned by SoraDemoCore.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                if (city != null)
                {
                    city.gameObject.SetActive(false);
                    UnityEngine.Object.Destroy(city.gameObject);
                }
                Debug.LogError("SORA CITY: asset instantiation failed; using legacy world fallback.");
                return false;
            }
        }

        private static Material MakeMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var m = new Material(shader) { name = name, color = color };
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            return m;
        }

        // The first 15 source facades are backed by CC0 mesh nodes in V03 GLB.
        // These authored plain side/back/roof shells close their otherwise empty lots.
        private static void BuildMissingV02Shells(Transform city, V02Layout layout)
        {
            Material facadeWall = MakeMaterial("SORA facade sidewall filler", new Color(.42f, .48f, .50f));
            Material facadeRoof = MakeMaterial("SORA simple roof filler", new Color(.12f, .16f, .22f));
            Transform shells = new GameObject("V02 15 building shells behind imported CC0 shopfronts").transform;
            shells.SetParent(city, false);
            foreach (V02Shop shop in layout.shops)
            {
                if (shop.id == "NE-01") continue; // Existing SORA MART is owned by SetupEnvironment.
                if (shop.front_xyz == null || shop.front_xyz.Length < 3 ||
                    shop.nominal_footprint_m == null || shop.nominal_footprint_m.Length < 2)
                    continue;
                float width = shop.nominal_footprint_m[0];
                float depth = shop.nominal_footprint_m[1];
                float height = shop.nominal_height_m;
                Transform building = new GameObject("CC0 storefront underlay " + shop.id).transform;
                building.SetParent(shells, false);
                building.localPosition = AuthorToUnity(new Vector3(shop.front_xyz[0], 0f, shop.front_xyz[2]));
                building.localRotation = Quaternion.Euler(0f, -shop.yaw_degrees, 0f);
                CreateSolid(building, "rear", new Vector3(0f, height * .5f, depth),
                            new Vector3(width, height, .26f), facadeWall);
                CreateSolid(building, "left side", new Vector3(-width * .5f, height * .5f, depth * .5f),
                            new Vector3(.24f, height, depth), facadeWall);
                CreateSolid(building, "right side", new Vector3(width * .5f, height * .5f, depth * .5f),
                            new Vector3(.24f, height, depth), facadeWall);
                CreateSolid(building, "upper front", new Vector3(0, (3.6f + height) * .5f, .08f),
                            new Vector3(width, Mathf.Max(.2f, height - 3.6f), .22f), facadeWall);
                CreateSolid(building, "roof", new Vector3(0, height + .13f, depth * .5f),
                            new Vector3(width + .22f, .26f, depth + .25f), facadeRoof);
            }
        }

        private static void CreateSolid(Transform root, string name, Vector3 at, Vector3 size, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(root, false);
            go.transform.localPosition = at;
            go.transform.localScale = size;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static void BuildOriginalBuildingCollisions(Transform city, V02Layout layout)
        {
            Transform root = new GameObject("V02 original building collisions").transform;
            root.SetParent(city, false);
            foreach (V02Shop shop in layout.shops)
            {
                if (shop.id == "NE-01") continue;
                var obj = new GameObject(shop.id);
                obj.transform.SetParent(root, false);
                obj.transform.localPosition = AuthorToUnity(new Vector3(shop.front_xyz[0], 0f, shop.front_xyz[2]));
                obj.transform.localRotation = Quaternion.Euler(0f, -shop.yaw_degrees, 0f);
                var collider = obj.AddComponent<BoxCollider>();
                float depth = shop.nominal_footprint_m[1];
                collider.center = new Vector3(0f, shop.nominal_height_m * .5f, depth * .5f);
                collider.size = new Vector3(shop.nominal_footprint_m[0], shop.nominal_height_m, depth);
            }
        }

        private static void AddGroundColliders(GameObject model)
        {
            foreach (var mesh in model.GetComponentsInChildren<MeshFilter>())
            {
                string name = mesh.name;
                if (name.StartsWith("ROAD/") || name.StartsWith("SIDEWALK/") ||
                    name.StartsWith("CURB/") || name.StartsWith("BLOCK_LAND/") ||
                    name.StartsWith("OUTER_PATH/") || name.StartsWith("V04/ENV/") ||
                    name.StartsWith("V04/GROUND/"))
                    mesh.gameObject.AddComponent<MeshCollider>().sharedMesh = mesh.sharedMesh;
            }
        }

        private static void BuildCollisionProxies(Transform city, CityColliderManifest data)
        {
            Transform root = new GameObject("REAL CITY COLLISIONS - 60 buildings / 42 cars / 4 walls / 16 ends").transform;
            root.SetParent(city, false);
            foreach (CityCollider c in data.items)
            {
                if (c.sx <= 0 || c.sy <= 0 || c.sz <= 0) continue;
                GameObject obj = new GameObject(c.group + " - " + c.id);
                obj.transform.SetParent(root, false);
                obj.transform.localPosition = AuthorToUnity(new Vector3(c.cx, c.cy, c.cz));
                BoxCollider collider = obj.AddComponent<BoxCollider>();
                collider.isTrigger = false;
                collider.size = new Vector3(c.sx, c.sy, c.sz);
            }

            // Continuous safety surface; NEVER rely on the car models to stop a fall.
            GameObject ground = new GameObject("Continuous safety floor under 171m map");
            ground.transform.SetParent(root, false);
            ground.transform.localPosition = AuthorToUnity(new Vector3(22.5f, -.50f, 22.5f));
            ground.AddComponent<BoxCollider>().size = new Vector3(185f, .8f, 185f);
        }
    }
}
