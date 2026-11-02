using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sora.AfterTheRain.Demo
{
    /// <summary>
    /// Deterministic, dependency-free neighborhood blockout around the existing SORA MART.
    /// This is NOT a production art pass: replace facade primitives with licensed prefabs
    /// and authored materials without changing the traversable street layout.
    /// </summary>
    public static class SoraNeighborhoodDistrict
    {
        public const int LayoutVersion = 5;
        public const float EastWestExtent = 84f;
        public const float NorthExtent = 52f;
        public const float SouthExtent = -91f;

        private static Transform root;
        private static Material asphalt, sidewalk, kerb, markings, yellow, glass, darkMetal;
        private static Material brick, plaster, charcoal, teal, wood, roof, litWindow, darkWindow;
        private static Material redNeon, blueNeon, amberNeon, greenNeon, wetHighlight, foliage;
        private static Material[] facades;
        private static Material[] signs;
        private static Texture2D roadTexture;

        public static void Build(Transform parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (parent.Find("SORA Neighborhood v0.5 - Connected Blocks") != null) return;
            root = new GameObject("SORA Neighborhood v0.5 - Connected Blocks").transform;
            root.SetParent(parent, false);

            CreatePalette();
            MatchOriginalStreetMaterials(parent);
            CreateStreetGrid();
            CreateBuildingBlocks();
            DetailExistingCentralFacades();
            CreateGuardrails();
            CreateStreetFurniture();
            CreateWetDetails();
            CreateStoreMerchandising();
            Debug.Log("SORA: neighborhood blockout v0.5 generated. All assets are procedural placeholders.");
        }

        private static Material Make(string label, Color color, float smoothness = .45f, bool emissive = false)
        {
            Shader shader = Shader.Find(GraphicsSettings.currentRenderPipeline != null
                ? "Universal Render Pipeline/Lit" : "Standard");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) throw new InvalidOperationException("SORA: no compatible shader found.");
            Material mat = new Material(shader);
            mat.name = "SORA Neighborhood / " + label;
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (emissive)
            {
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", color * 1.65f);
            }
            return mat;
        }

        private static void CreatePalette()
        {
            asphalt = Make("Asphalt, wet blue black", new Color(.092f, .116f, .16f), .87f);
            sidewalk = Make("Tiled sidewalk", new Color(.36f, .40f, .44f), .66f);
            kerb = Make("Concrete kerbs", new Color(.61f, .64f, .66f), .35f);
            markings = Make("Road paint white", new Color(.88f, .91f, .91f), .48f);
            yellow = Make("Tactile paving", new Color(.92f, .72f, .16f), .34f);
            glass = Make("Storefront glass blockout", new Color(.16f, .27f, .32f), .94f);
            darkMetal = Make("Blue black metal", new Color(.12f, .17f, .22f), .54f);
            brick = Make("Brick red", new Color(.38f, .20f, .22f), .25f);
            plaster = Make("Warm cream", new Color(.67f, .59f, .46f), .26f);
            charcoal = Make("Charcoal cladding", new Color(.20f, .25f, .32f), .48f);
            teal = Make("Retro pale teal", new Color(.30f, .57f, .55f), .45f);
            wood = Make("Warm storefront timber", new Color(.41f, .29f, .21f), .25f);
            roof = Make("Roof slate", new Color(.14f, .18f, .23f), .36f);
            litWindow = Make("Warm window", new Color(.98f, .74f, .39f), .70f, true);
            darkWindow = Make("Dark window", new Color(.09f, .15f, .21f), .85f);
            redNeon = Make("Red signage", new Color(.94f, .18f, .27f), .60f, true);
            blueNeon = Make("Blue signage", new Color(.17f, .72f, .94f), .60f, true);
            amberNeon = Make("Amber signage", new Color(1f, .58f, .19f), .60f, true);
            greenNeon = Make("Green signage", new Color(.26f, .82f, .54f), .60f, true);
            wetHighlight = Make("Wet puddle neutral sheen", new Color(.16f, .22f, .30f), .98f);
            foliage = Make("Planter leaves", new Color(.12f, .34f, .29f), .38f);
            facades = new[] { brick, plaster, charcoal, teal, wood };
            signs = new[] { redNeon, blueNeon, amberNeon, greenNeon };

            // Cheap deterministic asphalt variation: no external textures, HTTP, or asset rights.
            roadTexture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            roadTexture.name = "SORA procedural asphalt variation";
            roadTexture.wrapMode = TextureWrapMode.Repeat;
            roadTexture.filterMode = FilterMode.Bilinear;
            Color32[] pixels = new Color32[128 * 128];
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    float noise = Mathf.PerlinNoise(x * .14f + 2.6f, y * .14f + 17.2f);
                    float grain = Mathf.PerlinNoise(x * .85f + 8.4f, y * .85f + 3.5f);
                    byte v = (byte)Mathf.Clamp(Mathf.RoundToInt(125f + 43f * noise + 18f * grain), 0, 255);
                    pixels[y * 128 + x] = new Color32(v, v, v, 255);
                }
            }
            roadTexture.SetPixels32(pixels);
            roadTexture.Apply(false, true);
            asphalt.mainTexture = roadTexture;
            asphalt.mainTextureScale = new Vector2(9f, 3f);
        }

        private static GameObject Box(string name, Vector3 position, Vector3 scale, Material material,
            bool hasCollider = true, Transform parent = null)
        {
            GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.name = name;
            item.transform.SetParent(parent == null ? root : parent, false);
            item.transform.localPosition = position;
            item.transform.localScale = scale;
            item.GetComponent<Renderer>().sharedMaterial = material;
            if (!hasCollider) UnityEngine.Object.Destroy(item.GetComponent<Collider>());
            return item;
        }

        private static void LightAt(string name, Vector3 position, Color color, float intensity, float range)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(root, false);
            obj.transform.localPosition = position;
            Light light = obj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }

        private static void MatchOriginalStreetMaterials(Transform parent)
        {
            // The central road/sidewalk must not visibly change texture at x=+/-40.
            // Preserve the source geometry and colliders; only unify blockout surfaces.
            foreach (string surface in new[] { "Rainy asphalt road", "Front sidewalk", "Across sidewalk" })
            {
                Transform existing = parent.Find(surface);
                if (existing == null) continue;
                Renderer renderer = existing.GetComponent<Renderer>();
                if (renderer != null) renderer.sharedMaterial =
                    surface == "Rainy asphalt road" ? asphalt : sidewalk;
            }
        }

        private static void CreateStreetGrid()
        {
            // Existing central SORA MART facade/street occupies x=-40..40, z=-32..5.
            // Extensions touch its road edges and add a connected neighborhood loop.
            Box("Terrain safety slab", new Vector3(0f, -.52f, -20f),
                new Vector3(185f, .54f, 147f), charcoal);

            foreach (int side in new[] { -1, 1 })
            {
                Box("Main avenue extension " + side,
                    new Vector3(side * 62f, -.20f, -15.5f), new Vector3(44f, .28f, 14f), asphalt);
                Box("North south connector road " + side,
                    new Vector3(side * 47f, -.20f, -16.5f), new Vector3(13f, .28f, 105f), asphalt);
            }
            Box("North residential cross street", new Vector3(0f, -.20f, 26f),
                new Vector3(168f, .28f, 13f), asphalt);
            Box("South market cross street", new Vector3(0f, -.20f, -57f),
                new Vector3(168f, .28f, 13f), asphalt);

            // Four pavement runs at each side street, interrupted at crossings.
            float[] centers = { -70f, -37f, 5f, 40f };
            float[] lengths = { 13f, 25f, 25f, 14f };
            foreach (int side in new[] { -1, 1 })
            {
                for (int i = 0; i < centers.Length; i++)
                {
                    Box("Inner lane pavement " + side + " section " + i,
                        new Vector3(side * 37.7f, -.08f, centers[i]),
                        new Vector3(4.0f, .24f, lengths[i]), sidewalk);
                    Box("Outer lane pavement " + side + " section " + i,
                        new Vector3(side * 56.3f, -.08f, centers[i]),
                        new Vector3(4.0f, .24f, lengths[i]), sidewalk);
                }
                // Original avenue pavement only reaches x=40. Extend its edges.
                Box("Outer main street pavement front " + side,
                    new Vector3(side * 62f, -.08f, -7f),
                    new Vector3(44f, .24f, 3.7f), sidewalk);
                Box("Outer main street pavement rear " + side,
                    new Vector3(side * 62f, -.08f, -24f),
                    new Vector3(44f, .24f, 3.7f), sidewalk);
            }

            foreach (float z in new[] { 17.6f, 34.4f, -48.6f, -65.4f })
            {
                Box("West district sidewalk " + z, new Vector3(-69.2f, -.08f, z),
                    new Vector3(28.5f, .24f, 3.8f), sidewalk);
                Box("Central district sidewalk " + z, new Vector3(0f, -.08f, z),
                    new Vector3(74f, .24f, 3.8f), sidewalk);
                Box("East district sidewalk " + z, new Vector3(69.2f, -.08f, z),
                    new Vector3(28.5f, .24f, 3.8f), sidewalk);
            }

            // Walkable lanes between existing central buildings and outer blocks.
            foreach (int side in new[] { -1, 1 })
            {
                Box("Arcade passage " + side, new Vector3(side * 31f, -.08f, 8.5f),
                    new Vector3(3.4f, .24f, 20f), sidewalk);
                Box("Shop-side north access " + side, new Vector3(side * 31f, -.08f, 15.3f),
                    new Vector3(14f, .24f, 3.5f), sidewalk);
            }

            // Small pocket plaza behind the current opposite-side row.
            Box("Backstreet courtyard", new Vector3(0f, -.08f, -42f),
                new Vector3(29f, .24f, 12f), sidewalk);
            Box("Backstreet footpath west", new Vector3(-24f, -.08f, -41.8f),
                new Vector3(20f, .24f, 3.2f), sidewalk);
            Box("Backstreet footpath east", new Vector3(24f, -.08f, -41.8f),
                new Vector3(20f, .24f, 3.2f), sidewalk);

            // Painted crossings preserve ground collision: all markings are decorative.
            foreach (float x in new[] { -47f, 47f })
            {
                foreach (float z in new[] { -57f, -15.5f, 26f })
                {
                    for (int stripe = -5; stripe <= 5; stripe++)
                        Box("Crossing east-west " + x + " " + z + " " + stripe,
                            new Vector3(x, -.039f, z + stripe * .88f),
                            new Vector3(12.4f, .016f, .50f), markings, false);
                }
            }
            foreach (float z in new[] { -57f, 26f })
            {
                for (int stripe = -4; stripe <= 4; stripe++)
                    Box("North south pedestrian crossing " + z + " " + stripe,
                        new Vector3(stripe * 1.15f, -.039f, z),
                        new Vector3(.61f, .016f, 12.1f), markings, false);
            }
            for (int i = -18; i <= 18; i++)
            {
                float x = i * 4.3f;
                if (Mathf.Abs(Mathf.Abs(x) - 47f) < 8f) continue;
                foreach (float z in new[] { -57f, 26f })
                    Box("Lane center dash " + z + " " + i, new Vector3(x, -.039f, z),
                        new Vector3(1.7f, .012f, .10f), yellow, false);
            }
            foreach (int side in new[] { -1, 1 })
            {
                for (int i = -6; i <= 7; i++)
                {
                    float z = i * 6.4f - 15.5f;
                    if (Mathf.Abs(z + 57f) < 8f || Mathf.Abs(z + 15.5f) < 8f
                        || Mathf.Abs(z - 26f) < 8f) continue;
                    Box("Side street lane divider " + side + " " + i,
                        new Vector3(side * 47f, -.039f, z),
                        new Vector3(.10f, .012f, 2.3f), yellow, false);
                }
            }

            // Tactile paving on safe sidewalk stretches, not over road carriageways.
            foreach (int side in new[] { -1, 1 })
            {
                foreach (float z in new[] { -69f, -36f, 5f, 40f })
                    Box("Tactile paving side path " + side + " " + z,
                        new Vector3(side * 37.7f, .049f, z),
                        new Vector3(.4f, .018f, 6f), yellow, false);
            }
        }

        private static void Facade(string name, Vector3 at, float width, float depth,
            int stories, float yRotation, int colorIndex, int signIndex, string shopName)
        {
            Transform lot = new GameObject(name).transform;
            lot.SetParent(root, false);
            lot.localPosition = at;
            lot.localRotation = Quaternion.Euler(0f, yRotation, 0f);

            float height = stories * 3.0f + .2f;
            Material wall = facades[colorIndex % facades.Length];
            Material sign = signs[signIndex % signs.Length];
            Box("Structural shell", new Vector3(0f, height / 2f, 0f),
                new Vector3(width, height, depth), wall, true, lot);
            Box("Parapet", new Vector3(0f, height + .25f, 0f),
                new Vector3(width + .45f, .45f, depth + .45f), roof, false, lot);

            float front = -depth / 2f - .09f;
            Box("Ground floor glazing", new Vector3(0f, 1.55f, front),
                new Vector3(width - 1.3f, 2.45f, .07f), glass, false, lot);
            Box("Glazed shop door", new Vector3(-width * .27f, 1.15f, front - .05f),
                new Vector3(1.2f, 2.2f, .08f), darkWindow, false, lot);
            Box("Door handle", new Vector3(-width * .27f + .38f, 1.08f, front - .13f),
                new Vector3(.04f, .48f, .04f), kerb, false, lot);

            Box("Lit shop fascia", new Vector3(0f, 3.03f, front - .04f),
                new Vector3(width - .6f, .78f, .15f), sign, false, lot);
            Box("Awning", new Vector3(0f, 2.88f, front - .49f),
                new Vector3(width - .4f, .12f, 1.05f), roof, false, lot);
            // Short ASCII labels are placeholders; Japanese signage needs licensed font art.
            GameObject labelObject = new GameObject("Sign label " + shopName);
            labelObject.transform.SetParent(lot, false);
            labelObject.transform.localPosition = new Vector3(0f, 2.95f, front - .16f);
            labelObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            TextMesh text = labelObject.AddComponent<TextMesh>();
            text.text = shopName;
            text.fontSize = 48;
            text.characterSize = .08f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = Color.white;

            int windowsAcross = Mathf.Max(2, Mathf.FloorToInt((width - 1.0f) / 2.5f));
            for (int floor = 1; floor < stories; floor++)
            {
                float yy = floor * 3f + 1.57f;
                for (int column = 0; column < windowsAcross; column++)
                {
                    float xx = (column - (windowsAcross - 1) * .5f) * 2.25f;
                    Material window = (floor * 7 + column * 3 + colorIndex) % 5 == 0
                        ? litWindow : darkWindow;
                    Box("Apartment window " + floor + "-" + column,
                        new Vector3(xx, yy, front), new Vector3(1.18f, 1.42f, .07f),
                        window, false, lot);
                    Box("Window sill " + floor + "-" + column,
                        new Vector3(xx, yy - .76f, front - .09f),
                        new Vector3(1.3f, .09f, .24f), kerb, false, lot);
                }
                Box("Facade story band " + floor, new Vector3(0f, floor * 3f, front),
                    new Vector3(width + .12f, .12f, .12f), darkMetal, false, lot);
            }
            if (stories >= 4)
            {
                Box("Roof access stairwell", new Vector3(width * .19f, height + .65f, depth * .13f),
                    new Vector3(2.4f, 1.3f, 2.1f), roof, false, lot);
                Box("AC mechanical unit", new Vector3(-width * .2f, height + .32f, 0f),
                    new Vector3(1.4f, .52f, .9f), darkMetal, false, lot);
            }
        }

        private static void CreateBuildingBlocks()
        {
            // Oriented facade modules facing the public footpaths.
            // The center is intentionally left open for the existing SORA storefront.
            Facade("North - Station Hotel", new Vector3(-72f, 0f, 43f), 18f, 12f, 6, 0f, 2, 1, "HOTEL");
            Facade("North - Retro Records", new Vector3(-28f, 0f, 43f), 16f, 12f, 4, 0f, 0, 2, "RECORDS");
            Facade("North - Corner Bakery", new Vector3(-9f, 0f, 43f), 16f, 12f, 3, 0f, 1, 2, "BAKERY");
            Facade("North - Clinic", new Vector3(10f, 0f, 43f), 16f, 12f, 4, 0f, 3, 3, "CLINIC");
            Facade("North - Night Laundry", new Vector3(29f, 0f, 43f), 16f, 12f, 3, 0f, 2, 1, "LAUNDRY");
            Facade("North - Small Apartment", new Vector3(72f, 0f, 43f), 18f, 12f, 5, 0f, 3, 1, "APARTMENTS");

            Facade("North inner - Residence West", new Vector3(-29f, 0f, 9f), 12f, 11f, 3, 180f, 1, 3, "RESIDENCE");
            Facade("North inner - Residence East", new Vector3(29f, 0f, 9f), 12f, 11f, 3, 180f, 4, 2, "RESIDENCE");

            Facade("South - Arcade", new Vector3(-72f, 0f, -79f), 17f, 13f, 4, 180f, 1, 1, "ARCADE");
            Facade("South - Flower Market", new Vector3(-34f, 0f, -79f), 17f, 13f, 3, 180f, 3, 3, "FLOWERS");
            Facade("South - Bento Counter", new Vector3(-13f, 0f, -79f), 17f, 13f, 3, 180f, 4, 2, "BENTO");
            Facade("South - Cinema", new Vector3(8f, 0f, -79f), 17f, 13f, 5, 180f, 2, 0, "CINEMA");
            Facade("South - Pharmacy", new Vector3(29f, 0f, -79f), 17f, 13f, 3, 180f, 1, 3, "DRUGSTORE");
            Facade("South - Night Office", new Vector3(72f, 0f, -79f), 17f, 13f, 5, 180f, 0, 1, "OFFICES");

            Facade("West edge - Coffee", new Vector3(-72f, 0f, -41f), 16f, 12f, 3, -90f, 4, 2, "COFFEE");
            Facade("West edge - Game Center", new Vector3(-72f, 0f, -13f), 16f, 12f, 4, -90f, 2, 0, "GAME");
            Facade("West edge - Bookstore", new Vector3(-72f, 0f, 8f), 16f, 12f, 3, -90f, 0, 1, "BOOKS");
            Facade("East edge - Noodle Shop", new Vector3(72f, 0f, -41f), 16f, 12f, 3, 90f, 4, 2, "NOODLES");
            Facade("East edge - Karaoke", new Vector3(72f, 0f, -13f), 16f, 12f, 5, 90f, 2, 0, "KARAOKE");
            Facade("East edge - Music Store", new Vector3(72f, 0f, 8f), 16f, 12f, 3, 90f, 3, 1, "MUSIC");
        }

        private static void DetailExistingCentralFacades()
        {
            // The original five opposite-side buildings are the first sightline
            // after exiting SORA MART: layer their fronts before adding distant bulk.
            float[] xs = { -28f, -14f, 0f, 14f, 28f };
            float[] heights = { 13f, 19f, 15f, 22f, 14f };
            for (int i = 0; i < xs.Length; i++)
            {
                float x = xs[i];
                Material accent = signs[i % signs.Length];
                Box("Existing frontage projecting awning " + i,
                    new Vector3(x, 3.12f, -24.75f),
                    new Vector3(8.2f, .15f, 1.10f), roof, false);
                Box("Existing frontage illuminated vertical blade " + i,
                    new Vector3(x + 5.1f, 5.35f, -24.65f),
                    new Vector3(.45f, 2.3f, .32f), accent, false);
                Box("Existing frontage side pilaster left " + i,
                    new Vector3(x - 5.8f, 2.4f, -24.25f),
                    new Vector3(.25f, 4.7f, .33f), kerb, false);
                Box("Existing frontage side pilaster right " + i,
                    new Vector3(x + 5.8f, 2.4f, -24.25f),
                    new Vector3(.25f, 4.7f, .33f), kerb, false);
                for (float yy = 7.0f; yy < heights[i] - .4f; yy += 3.2f)
                {
                    Box("Existing facade balcony slab " + i + " " + yy,
                        new Vector3(x, yy, -24.53f),
                        new Vector3(9.0f, .11f, .70f), kerb, false);
                    for (int rail = -3; rail <= 3; rail++)
                        Box("Existing facade rail " + i + " " + yy + " " + rail,
                            new Vector3(x + rail * 1.2f, yy + .54f, -24.83f),
                            new Vector3(.055f, 1.04f, .055f), darkMetal, false);
                    Box("Existing facade balcony upper bar " + i + " " + yy,
                        new Vector3(x, yy + 1.06f, -24.83f),
                        new Vector3(8.8f, .06f, .07f), darkMetal, false);
                }
            }
        }

        private static void CreateGuardrails()
        {
            // Match the video's rail-protected konbini sidewalk while preserving
            // zebra-crossing openings at the side-road intersections.
            foreach (int side in new[] { -1, 1 })
            {
                foreach (float xMagnitude in new[] { 59f, 66f, 73f, 80f })
                {
                    float x = side * xMagnitude;
                    foreach (float z in new[] { -8.7f, -22.3f })
                    {
                        Box("Tokyo footpath guardrail upper " + x + " " + z,
                            new Vector3(x, .87f, z), new Vector3(4.6f, .075f, .08f), kerb);
                        Box("Tokyo footpath guardrail lower " + x + " " + z,
                            new Vector3(x, .40f, z), new Vector3(4.6f, .06f, .07f), kerb);
                        foreach (float dx in new[] { -2.1f, 2.1f })
                            Box("Tokyo footpath guardrail post " + x + " " + z + " " + dx,
                                new Vector3(x + dx, .48f, z),
                                new Vector3(.10f, .95f, .11f), kerb);
                    }
                }
            }
        }

        private static void CreateStreetFurniture()
        {
            foreach (int side in new[] { -1, 1 })
            {
                foreach (float z in new[] { -73f, -37f, 6f, 39f })
                {
                    float x = side * 59.2f;
                    Box("Street lamp post " + side + " " + z, new Vector3(x, 2.75f, z),
                        new Vector3(.15f, 5.5f, .15f), darkMetal);
                    Box("Street lamp arm", new Vector3(x - side * .6f, 5.35f, z),
                        new Vector3(1.35f, .12f, .12f), darkMetal, false);
                    Box("Street lamp head", new Vector3(x - side * 1.12f, 5.22f, z),
                        new Vector3(.70f, .16f, .40f), litWindow, false);
                    if (Mathf.Abs(z) < 10f || Mathf.Abs(z + 37f) < 3f)
                        LightAt("Night pool light " + side + " " + z,
                            new Vector3(x - side * 1.1f, 5.15f, z),
                            new Color(1f, .77f, .46f), 1.45f, 9f);

                    Box("Corner recycling bin " + side + " " + z,
                        new Vector3(side * 60.6f, .53f, z + 2.6f),
                        new Vector3(.57f, 1.05f, .57f), teal);
                }
            }

            // Street furniture clustered where the player actually walks.
            VendingPair(new Vector3(28f, 0f, 34.8f));
            VendingPair(new Vector3(-24f, 0f, -65.2f));
            Bicycle(new Vector3(33.7f, 0f, 35.5f));
            Bicycle(new Vector3(-33.2f, 0f, -48.4f));
            Bicycle(new Vector3(22f, 0f, -65.3f));
            foreach (float x in new[] { -10f, -4f, 4f, 10f })
            {
                Planter(new Vector3(x, 0f, -43.5f));
            }
            Box("Courtyard seating stone", new Vector3(0f, .24f, -44f),
                new Vector3(5.7f, .45f, 1.0f), kerb);
            Box("Courtyard art light", new Vector3(0f, 1.7f, -40.5f),
                new Vector3(.24f, 3.2f, .24f), darkMetal);
            LightAt("Courtyard amber ambient", new Vector3(0f, 3.3f, -40.5f),
                new Color(1f, .67f, .37f), 1.2f, 9f);

            // Hanging telecom lines lend Tokyo's compact residential street identity.
            for (int i = 0; i < 3; i++)
            {
                float z = -39f + i * 19f;
                OverheadLine(new Vector3(-58f, 6.4f, z), new Vector3(-36f, 6.0f, z + 1.5f), "West crossing line " + i);
                OverheadLine(new Vector3(36f, 6.0f, z), new Vector3(58f, 6.4f, z + 1.5f), "East crossing line " + i);
            }
        }

        private static void VendingPair(Vector3 position)
        {
            for (int i = 0; i < 2; i++)
            {
                float x = position.x + i * 1.18f;
                Material face = i == 0 ? redNeon : blueNeon;
                Box("Side street vending machine", new Vector3(x, 1.1f, position.z),
                    new Vector3(.95f, 2.1f, .72f), face);
                Box("Vending display", new Vector3(x, 1.45f, position.z - .39f),
                    new Vector3(.75f, .92f, .035f), litWindow, false);
                Box("Vending payment keypad", new Vector3(x + .27f, .65f, position.z - .40f),
                    new Vector3(.17f, .28f, .035f), darkMetal, false);
            }
        }

        private static void Bicycle(Vector3 at)
        {
            for (int i = -1; i <= 1; i += 2)
            {
                GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel.name = "Parked bicycle wheel";
                wheel.transform.SetParent(root, false);
                wheel.transform.localPosition = at + new Vector3(i * .46f, .36f, 0f);
                wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                wheel.transform.localScale = new Vector3(.34f, .045f, .34f);
                wheel.GetComponent<Renderer>().sharedMaterial = darkMetal;
                UnityEngine.Object.Destroy(wheel.GetComponent<Collider>());
            }
            Box("Parked bicycle frame", at + new Vector3(0f, .69f, 0f),
                new Vector3(1.0f, .07f, .08f), kerb, false);
            Box("Parked bicycle saddle", at + new Vector3(.10f, .95f, 0f),
                new Vector3(.30f, .06f, .18f), roof, false);
        }

        private static void Planter(Vector3 at)
        {
            Box("Planter trough", at + new Vector3(0f, .25f, 0f),
                new Vector3(.85f, .46f, .85f), brick);
            GameObject shrub = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shrub.name = "Planter foliage";
            shrub.transform.SetParent(root, false);
            shrub.transform.localPosition = at + new Vector3(0f, .82f, 0f);
            shrub.transform.localScale = new Vector3(.78f, .88f, .78f);
            shrub.GetComponent<Renderer>().sharedMaterial = foliage;
            UnityEngine.Object.Destroy(shrub.GetComponent<Collider>());
        }

        private static void OverheadLine(Vector3 from, Vector3 to, string name)
        {
            GameObject wire = new GameObject(name);
            wire.transform.SetParent(root, false);
            LineRenderer line = wire.AddComponent<LineRenderer>();
            line.positionCount = 3;
            line.useWorldSpace = true;
            line.sharedMaterial = darkMetal;
            line.startWidth = .025f;
            line.endWidth = .025f;
            line.SetPosition(0, from);
            line.SetPosition(1, (from + to) * .5f + Vector3.down * .30f);
            line.SetPosition(2, to);
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
        }

        private static void CreateWetDetails()
        {
            // Subtle highlights, not the original brightly colored fake reflection bars.
            // Real reflections, decals and wetness normal maps belong to the art pass.
            for (int i = 0; i < 54; i++)
            {
                float sample = Mathf.PerlinNoise(i * .67f, i * .32f);
                float x = Mathf.Lerp(-78f, 78f, Mathf.Repeat(i * .6180339f + .17f, 1f));
                float z = i % 3 == 0 ? -57f : (i % 3 == 1 ? 26f : -15.5f);
                if (Mathf.Abs(Mathf.Abs(x) - 47f) < 9f || Mathf.Abs(x) < 6f) continue;
                float dz = Mathf.Lerp(-4.7f, 4.7f, sample);
                Box("Wet asphalt puddle patch " + i,
                    new Vector3(x, -.047f, z + dz),
                    new Vector3(1.2f + sample * 3.0f, .008f, .55f + sample),
                    wetHighlight, false);
            }
        }

        private static void CreateStoreMerchandising()
        {
            // Fill existing wall fridge/shelf sightlines without blocking interactions.
            // Props remain noninteractive until the authored store-art pass.
            Box("Backwall fridge lighting", new Vector3(-3.5f, 1.55f, 5.12f),
                new Vector3(6.0f, 2.6f, .08f), glass, false);
            Material[] goods = { redNeon, blueNeon, amberNeon, greenNeon, litWindow };
            for (int shelf = 0; shelf < 3; shelf++)
            {
                float y = .7f + shelf * .69f;
                Box("Refrigerator white shelf " + shelf, new Vector3(-3.5f, y - .20f, 5.00f),
                    new Vector3(6.1f, .07f, .28f), kerb, false);
                for (int product = 0; product < 25; product++)
                {
                    float x = -6.38f + product * .24f;
                    Box("Beverage product " + shelf + " " + product,
                        new Vector3(x, y + .05f, 4.85f),
                        new Vector3(.13f, .30f, .12f),
                        goods[(shelf * 3 + product) % goods.Length], false);
                }
            }
            Box("Promo island display", new Vector3(.05f, .45f, -2.3f),
                new Vector3(.75f, .88f, .75f), wood, false);
            for (int i = 0; i < 4; i++)
                Box("Promo pack " + i, new Vector3(-.23f + (i % 2) * .38f,
                    1.04f, -2.52f + (i / 2) * .38f),
                    new Vector3(.27f, .31f, .25f), goods[i], false);
        }
    }

    /// <summary>Keep the existing rain volume centered near the active twin outdoors.</summary>
    public sealed class SoraDistrictRainFollower : MonoBehaviour
    {
        private void LateUpdate()
        {
            SoraDemoCore core = SoraDemoCore.Instance;
            if (core == null || core.ActiveCharacter == null) return;
            Vector3 position = core.ActiveCharacter.transform.position;
            transform.position = new Vector3(position.x, 12f, position.z);
        }
    }
}
