using UnityEngine;

namespace Sora.AfterTheRain.Demo
{
    public static class SoraDemoWorld
    {
        private static SoraDemoCore game;
        private static Transform world;
        private static Material tiles, walls, metal, warmWhite, darkGlass, road, pavement, blue, coral, pink, green, gold, skin, hair;
        private static Material neonCyan, neonRed, neonAmber, neonGreen, wood, darkCloth, whiteCloth, yellowBlind, redMailbox, eyeBlack;

        public static void Build(SoraDemoCore owner)
        {
            game = owner;
            world = new GameObject("SORA - Generated Tokyo Night Blockout").transform;
            SetupMaterials();
            SetupEnvironment();
            SetupShop();
            SetupStreet();
            SetupTwins();
            SetupCustomers();
            SetupCamera();
            SetupRain();
            // Connected blockout, kept separate from the existing interactive core.
            SoraNeighborhoodDistrict.Build(world);
        }

        private static Material Mat(Color c, float smoothness = 0.5f, bool emissive = false)
        {
            bool usingSRP = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null;
            Shader shader = Shader.Find(usingSRP ? "Universal Render Pipeline/Lit" : "Standard");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var material = new Material(shader);
            material.color = c;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", c);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            if (emissive)
            {
                material.EnableKeyword("_EMISSION");
                if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", c * 2.5f);
            }
            return material;
        }

        private static void SetupMaterials()
        {
            tiles = Mat(new Color(0.78f, 0.83f, 0.84f), 0.82f);
            walls = Mat(new Color(0.86f, 0.91f, 0.87f));
            metal = Mat(new Color(0.29f, 0.39f, 0.48f), .75f);
            warmWhite = Mat(new Color(1f, .95f, .68f), .3f, true);
            darkGlass = Mat(new Color(.08f, .18f, .23f), .92f);
            road = Mat(new Color(.035f, .055f, .08f), .95f);
            pavement = Mat(new Color(.22f, .25f, .31f), .85f);
            blue = Mat(new Color(.14f, .55f, .89f), .55f, true);
            coral = Mat(new Color(.98f, .30f, .35f), .36f);
            pink = Mat(new Color(.95f, .56f, .72f), .45f);
            green = Mat(new Color(.28f, .75f, .48f));
            gold = Mat(new Color(.93f, .68f, .28f), .6f);
            skin = Mat(new Color(.96f, .77f, .64f));
            hair = Mat(new Color(.10f, .08f, .12f));

            neonCyan = Mat(new Color(0.12f, 0.92f, 1f), 0.2f, true);
            neonRed = Mat(new Color(1f, 0.18f, 0.22f), 0.2f, true);
            neonAmber = Mat(new Color(1f, 0.72f, 0.15f), 0.2f, true);
            neonGreen = Mat(new Color(0.2f, 1f, 0.45f), 0.2f, true);
            wood = Mat(new Color(0.38f, 0.24f, 0.15f), 0.4f);
            darkCloth = Mat(new Color(0.11f, 0.13f, 0.18f), 0.3f);
            whiteCloth = Mat(new Color(0.95f, 0.95f, 0.97f), 0.4f);
            yellowBlind = Mat(new Color(0.96f, 0.82f, 0.12f), 0.7f);
            redMailbox = Mat(new Color(0.88f, 0.12f, 0.10f), 0.8f);
            eyeBlack = Mat(new Color(0.05f, 0.05f, 0.07f), 0.9f);
        }

        private static GameObject Cube(string name, Vector3 at, Vector3 scale, Material material, Transform parent = null, bool collider = true)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent == null ? world : parent, false);
            go.transform.position = at;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider) Object.Destroy(go.GetComponent<Collider>());
            return go;
        }

        private static GameObject Sphere(string name, Vector3 at, Vector3 scale, Material material, Transform parent = null)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent == null ? world : parent, false);
            go.transform.position = at;
            go.transform.localScale = scale;
            Object.Destroy(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        private static void LightAt(string name, Vector3 pos, Color color, float intensity, float range)
        {
            GameObject g = new GameObject(name);
            g.transform.SetParent(world, false);
            g.transform.position = pos;
            var light = g.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
        }

        private static void TagInteract(GameObject target, InteractKind kind, string prompt, int index = 0)
        {
            var component = target.AddComponent<SoraInteractable>();
            component.kind = kind;
            component.title = prompt;
            component.customerIndex = index;
            game.Register(component);
        }

        private static void AddText(string text, Vector3 at, Vector3 size, Color tint, Transform parent = null)
        {
            GameObject go = new GameObject("Text_" + text);
            go.transform.SetParent(parent == null ? world : parent, false);
            go.transform.position = at;
            go.transform.localScale = size;
            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.fontSize = 58;
            mesh.characterSize = 0.12f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = tint;
            if (text.StartsWith("SORA MART")) go.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        private static void SetupEnvironment()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.24f, .28f, .38f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(.045f, .075f, .13f);
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = .012f;
            Camera.main?.gameObject.SetActive(false);

            // Store Interior Architecture
            Cube("Store tiled floor", new Vector3(0, -.18f, 0), new Vector3(14, .30f, 11f), tiles);
            Cube("Back wall", new Vector3(0, 2.25f, 5.45f), new Vector3(14, 4.5f, .35f), walls);
            Cube("Left wall", new Vector3(-7f, 2.25f, .1f), new Vector3(.36f, 4.5f, 10.7f), walls);
            Cube("Right wall", new Vector3(7f, 2.25f, .1f), new Vector3(.36f, 4.5f, 10.7f), walls);

            // Storefront with open 2.4m walk-through entrance
            Cube("Left storefront frame", new Vector3(-4.8f, 2.1f, -5.25f), new Vector3(4.4f, 4.2f, .25f), darkGlass);
            Cube("Right storefront frame", new Vector3(4.8f, 2.1f, -5.25f), new Vector3(4.4f, 4.2f, .25f), darkGlass);
            Cube("Top storefront lintel", new Vector3(0f, 3.9f, -5.25f), new Vector3(14.4f, .45f, .25f), metal);
            Cube("Bottom window border - left", new Vector3(-4.8f, .3f, -5.25f), new Vector3(4.4f, .25f, .25f), metal);
            Cube("Bottom window border - right", new Vector3(4.8f, .3f, -5.25f), new Vector3(4.4f, .25f, .25f), metal);

            // Open automatic sliding doors (left & right open panels, empty middle for seamless walk)
            Cube("Sliding door panel left", new Vector3(-1.8f, 1.85f, -5.28f), new Vector3(1.2f, 3.5f, .06f), darkGlass, null, false);
            Cube("Sliding door panel right", new Vector3(1.8f, 1.85f, -5.28f), new Vector3(1.2f, 3.5f, .06f), darkGlass, null, false);
            Cube("Entrance welcome mat", new Vector3(0f, -.02f, -5.25f), new Vector3(2.4f, .04f, 1.4f), darkCloth, null, false);

            Cube("Roof eave", new Vector3(0f, 4.55f, -5.6f), new Vector3(14.4f, .48f, 2.6f), metal, null, false);
            Cube("SORA sign backboard", new Vector3(0f, 4.9f, -5.6f), new Vector3(10f, .8f, .21f), blue, null, false);
            AddText("SORA MART - OPEN 24H", new Vector3(0f, 4.9f, -5.73f), new Vector3(.38f, .38f, .38f), Color.white);

            for (int i = 0; i < 6; i++)
            {
                float x = -5.6f + i * 2.25f;
                Cube("LED ceiling fixture", new Vector3(x, 4.19f, 0f), new Vector3(.6f, .08f, 8.8f), warmWhite, null, false);
                LightAt("Warm store lamp " + i, new Vector3(x, 3.72f, 0), new Color(1f, .92f, .75f), 1.85f, 6.2f);
            }
            LightAt("Front neon", new Vector3(0, 4f, -6.2f), new Color(.26f, .72f, 1f), 3.0f, 10f);
        }

        private static void SetupShop()
        {
            // Central aisles
            for (int row = 0; row < 2; row++)
            {
                float x = row == 0 ? -2.6f : 2.5f;
                for (int column = 0; column < 2; column++)
                {
                    float z = column == 0 ? .8f : 3.2f;
                    Cube("Grocery shelf base", new Vector3(x, .75f, z), new Vector3(1.45f, 1.50f, 1.40f), metal);
                    for (int level = 0; level < 3; level++)
                    {
                        Cube("Grocery shelf rail", new Vector3(x, .45f + level * .46f, z - .71f), new Vector3(1.45f, .08f, .07f), gold, null, false);
                        for (int item = 0; item < 4; item++)
                        {
                            Material itemMat = (item + level) % 3 == 0 ? coral : ((item + level) % 3 == 1 ? green : warmWhite);
                            Cube("Product mockup", new Vector3(x - .47f + item * .30f, .65f + level * .42f, z - .78f), new Vector3(.17f, .28f, .1f), itemMat, null, false);
                        }
                    }
                }
            }
            GameObject shelf = Cube("Restock point", new Vector3(-2.6f, 1.65f, -0.32f), new Vector3(1.20f, .16f, .12f), coral, null, false);
            TagInteract(shelf, InteractKind.Shelf, "Sap them hang len ke");
            Cube("POS checkout counter", new Vector3(-4.85f, .6f, -2.5f), new Vector3(2.65f, 1.2f, 1.2f), metal);
            GameObject till = Cube("POS terminal", new Vector3(-4.85f, 1.3f, -2.54f), new Vector3(.7f, .3f, .42f), blue);
            TagInteract(till, InteractKind.Register, "POS: quet hang / thanh toan");
            Cube("Countertop", new Vector3(-4.85f, 1.21f, -2.5f), new Vector3(2.75f, .13f, 1.35f), warmWhite, null, false);
            Cube("Backroom partition", new Vector3(4.85f, 1.9f, 4.85f), new Vector3(3.6f, 3.8f, .25f), metal);
            GameObject delivery = Cube("Delivery box", new Vector3(5.0f, .45f, 3.15f), new Vector3(1.15f, .9f, 1.1f), gold);
            TagInteract(delivery, InteractKind.Delivery, "Nhan thung hang (+6 kho)");
            GameObject stock = Cube("Inventory tablet", new Vector3(6.3f, 1.3f, 1.6f), new Vector3(.24f, .62f, .8f), blue);
            TagInteract(stock, InteractKind.StockCount, "Kiem ke hang ton");
            GameObject clock = Cube("End shift clock", new Vector3(-6.7f, 1.6f, 3.8f), new Vector3(.15f, .7f, .7f), coral, null, false);
            TagInteract(clock, InteractKind.ShiftClock, "Xem bao cao cuoi ca");
            LightAt("Stockroom lamp", new Vector3(5f, 3.6f, 3.3f), new Color(1f, .8f, .64f), 2.2f, 5f);
        }

        private static void SetupStreet()
        {
            // Broad wet asphalt main road (width 80m, depth 14m)
            Cube("Rainy asphalt road", new Vector3(0f, -.20f, -15.5f), new Vector3(80f, .28f, 14f), road);
            // Sidewalk in front of store & neighbors
            Cube("Front sidewalk", new Vector3(0f, -.10f, -7.0f), new Vector3(80f, .25f, 3.6f), pavement);
            // Across-the-street sidewalk
            Cube("Across sidewalk", new Vector3(0f, -.10f, -24.0f), new Vector3(80f, .25f, 3.6f), pavement);

            // Tactile yellow guide tiles (gạch xúc giác màu vàng dẫn đường)
            Cube("Tactile paving line", new Vector3(0f, -.03f, -7.6f), new Vector3(76f, .02f, .38f), yellowBlind, null, false);

            // Road markings: yellow center dashed lines and white outer solid lines
            for (int i = -10; i <= 10; i++)
            {
                Cube("Center road divider", new Vector3(i * 3.8f, -.043f, -15.5f), new Vector3(2.0f, .02f, .16f), yellowBlind, null, false);
                // Neon reflection strips removed: district uses restrained puddle blockout.
            }

            // Pedestrian Zebra Crosswalk in front of Sora Mart
            for (int w = -4; w <= 4; w++)
            {
                Cube("Zebra stripe " + w, new Vector3(w * .95f, -.042f, -15.5f), new Vector3(.55f, .02f, 13f), whiteCloth, null, false);
            }

            // Sidewalk pedestrian guard rails (white Tokyo curb fences with break at crosswalk)
            for (int r = -6; r <= 6; r++)
            {
                if (Mathf.Abs(r) <= 1) continue; // Opening for crosswalk
                float rx = r * 4.5f;
                Cube("Sidewalk guard rail " + r, new Vector3(rx, .45f, -8.6f), new Vector3(4.2f, .75f, .08f), whiteCloth);
            }

            // Japanese Vending Machine Station beside Sora Mart
            SetupVendingStation(new Vector3(-5.8f, 0f, -6.2f));

            // Japanese Red Mailbox (Hộp thư bưu điện đỏ đặc trưng)
            GameObject mailbox = Cube("Japanese Red Postbox", new Vector3(4.8f, .72f, -7.5f), new Vector3(.65f, 1.25f, .55f), redMailbox);
            Cube("Postbox slot", new Vector3(4.8f, 1.12f, -7.78f), new Vector3(.45f, .08f, .04f), darkCloth, mailbox.transform, false);

            // Left Neighbor: Tokyo Ramen Bar (Mì Ramen đêm)
            SetupRamenShop(new Vector3(-17f, 0f, -0.5f));

            // Right Neighbor: Izakaya & Night Cafe (Quán nhậu & Cafe đèn vàng)
            SetupIzakayaShop(new Vector3(17f, 0f, -0.5f));

            // Across-the-street Tokyo Cityscape (5 multi-story buildings)
            SetupAcrossStreetBuildings();

            // Street lighting & utility poles
            for (int p = -2; p <= 2; p++)
            {
                float px = p * 18f;
                SetupUtilityPole(new Vector3(px, 0f, -8.5f));
            }

            // Bus Stop Shelter across the street
            GameObject busStop = Cube("Bus stop shelter", new Vector3(8.5f, 1.4f, -23.5f), new Vector3(4.5f, 2.8f, 1.8f), darkGlass, null, false);
            Cube("Bus stop roof", new Vector3(8.5f, 2.85f, -23.5f), new Vector3(5.0f, .18f, 2.4f), metal, null, false);
            Cube("Bus bench", new Vector3(8.5f, .45f, -23.3f), new Vector3(3.2f, .12f, .45f), wood, null, false);
            AddText("TOKYO METRO BUS", new Vector3(8.5f, 2.65f, -22.55f), new Vector3(.24f, .24f, .24f), Color.white);
            TagInteract(busStop, InteractKind.ExitDoor, "Tram xe buyt Tokyo");
        }

        private static void SetupVendingStation(Vector3 pos)
        {
            // Red Hot/Cold Drink Machine
            GameObject vmRed = Cube("Red Beverage Vending Machine", pos, new Vector3(1.15f, 2.15f, .85f), redMailbox);
            Cube("VM Red Display", pos + new Vector3(0f, .45f, -.44f), new Vector3(.95f, .85f, .05f), warmWhite, vmRed.transform, false);
            Cube("VM Red Slot", pos + new Vector3(0f, -.65f, -.44f), new Vector3(.75f, .30f, .05f), darkCloth, vmRed.transform, false);
            LightAt("VM Red Glow", pos + new Vector3(0f, .45f, -.85f), new Color(1f, .5f, .4f), 1.8f, 3.5f);

            // Blue Cold Drink Machine
            Vector3 posBlue = pos + new Vector3(-1.35f, 0f, 0f);
            GameObject vmBlue = Cube("Blue Beverage Vending Machine", posBlue, new Vector3(1.15f, 2.15f, .85f), blue);
            Cube("VM Blue Display", posBlue + new Vector3(0f, .45f, -.44f), new Vector3(.95f, .85f, .05f), neonCyan, vmBlue.transform, false);
            Cube("VM Blue Slot", posBlue + new Vector3(0f, -.65f, -.44f), new Vector3(.75f, .30f, .05f), darkCloth, vmBlue.transform, false);
            LightAt("VM Blue Glow", posBlue + new Vector3(0f, .45f, -.85f), new Color(.3f, .8f, 1f), 1.8f, 3.5f);

            // Recycling Bin for cans & plastic bottles
            Cube("Recycling Box", pos + new Vector3(1.0f, -.15f, 0f), new Vector3(.65f, 1.1f, .65f), whiteCloth);
        }

        private static void SetupRamenShop(Vector3 center)
        {
            // Building block
            Cube("Ramen Shop Structure", center + new Vector3(0f, 2.75f, 0f), new Vector3(10f, 5.5f, 11f), wood);
            Cube("Ramen Tiled Eaves", center + new Vector3(0f, 4.3f, -5.8f), new Vector3(10.6f, .55f, 2.4f), darkCloth, null, false);
            // Glowing Japanese Red Lanterns (Chochin 提灯)
            for (int l = -1; l <= 1; l++)
            {
                float lx = center.x + l * 2.2f;
                GameObject lantern = Sphere("Ramen Chochin " + l, new Vector3(lx, 3.5f, center.z - 5.6f), new Vector3(.48f, .75f, .48f), neonRed);
                LightAt("Lantern light " + l, new Vector3(lx, 3.4f, center.z - 5.8f), new Color(1f, .25f, .15f), 2.5f, 5.5f);
            }
            // Traditional Noren curtain banner
            Cube("Ramen Noren Curtain", center + new Vector3(0f, 3.1f, -5.55f), new Vector3(4.2f, .85f, .05f), whiteCloth, null, false);
            AddText("RAMEN - MENYA SORA", center + new Vector3(0f, 4.6f, -5.9f), new Vector3(.32f, .32f, .32f), new Color(1f, .85f, .4f));
        }

        private static void SetupIzakayaShop(Vector3 center)
        {
            // Building block
            Cube("Izakaya Structure", center + new Vector3(0f, 2.75f, 0f), new Vector3(10f, 5.5f, 11f), metal);
            Cube("Izakaya Eaves", center + new Vector3(0f, 4.3f, -5.8f), new Vector3(10.6f, .55f, 2.4f), wood, null, false);
            // Amber glowing signs
            Cube("Izakaya Signboard", center + new Vector3(0f, 4.7f, -5.85f), new Vector3(6.5f, .95f, .15f), neonAmber, null, false);
            AddText("IZAKAYA - NIGHT CAFE", center + new Vector3(0f, 4.7f, -5.98f), new Vector3(.30f, .30f, .30f), Color.white);
            LightAt("Izakaya Amber Glow", center + new Vector3(0f, 3.8f, -6.5f), new Color(1f, .7f, .2f), 3.0f, 7.5f);

            // Commuter bicycle parked against the wall
            GameObject bike = new GameObject("Parked Bicycle");
            bike.transform.SetParent(world, false);
            bike.transform.position = center + new Vector3(3.2f, 0f, -6.2f);
            bike.transform.rotation = Quaternion.Euler(0f, 15f, 0f);
            Cube("Bike Frame", new Vector3(0f, .55f, 0f), new Vector3(1.2f, .06f, .06f), coral, bike.transform, false);
            Cylinder("Bike Wheel Front", new Vector3(.65f, .35f, 0f), new Vector3(.7f, .05f, .7f), metal, bike.transform);
            Cylinder("Bike Wheel Back", new Vector3(-.65f, .35f, 0f), new Vector3(.7f, .05f, .7f), metal, bike.transform);
            Cube("Bike Handlebars", new Vector3(.58f, .85f, 0f), new Vector3(.06f, .06f, .55f), metal, bike.transform, false);
        }

        private static void SetupAcrossStreetBuildings()
        {
            // 5 Tokyo urban buildings across the street
            float[] xs = { -28f, -14f, 0f, 14f, 28f };
            float[] heights = { 13f, 19f, 15f, 22f, 14f };
            string[] signs = { "24H PHARMACY", "KARAOKE 7F", "TOKYO RESIDENCE", "COFFEE & BOOK", "HOTEL SHINJUKU" };
            Material[] signMats = { neonGreen, neonRed, neonCyan, neonAmber, neonCyan };

            for (int i = 0; i < xs.Length; i++)
            {
                float x = xs[i];
                float h = heights[i];
                // Main building tower
                Cube("Tokyo Tower " + i, new Vector3(x, h / 2f, -28f), new Vector3(12.5f, h, 7.5f), metal);

                // Lit windows grid
                for (int fl = 1; fl < (int)(h / 2.6f); fl++)
                {
                    for (int c = -2; c <= 2; c++)
                    {
                        Material winMat = (fl * 3 + c + i) % 3 == 0 ? warmWhite : darkGlass;
                        Cube("Window " + i + "_" + fl + "_" + c, new Vector3(x + c * 2.1f, fl * 2.4f, -24.15f), new Vector3(1.1f, 1.2f, .08f), winMat, null, false);
                    }
                }

                // Vertical / horizontal neon commercial sign
                Cube("Building Neon Signboard " + i, new Vector3(x, 4.5f, -24.18f), new Vector3(7.2f, 1.0f, .18f), signMats[i], null, false);
                AddText(signs[i], new Vector3(x, 4.5f, -24.3f), new Vector3(.26f, .26f, .26f), Color.white);
                LightAt("Sign Light " + i, new Vector3(x, 4.5f, -24.8f), signMats[i].color, 2.8f, 7f);

                // Rooftop water tower
                Cylinder("Rooftop Water Tank " + i, new Vector3(x + 2.5f, h + 1.4f, -27.5f), new Vector3(1.8f, 1.4f, 1.8f), metal);
            }
        }

        private static void SetupUtilityPole(Vector3 pos)
        {
            // Concrete utility pole
            Cylinder("Utility Pole Concrete", pos + new Vector3(0f, 4.5f, 0f), new Vector3(.32f, 4.5f, .32f), metal);
            // Crossarms for electrical lines
            Cube("Crossarm Upper", pos + new Vector3(0f, 8.2f, 0f), new Vector3(2.4f, .12f, .12f), metal, null, false);
            Cube("Crossarm Lower", pos + new Vector3(0f, 7.3f, 0f), new Vector3(2.0f, .12f, .12f), metal, null, false);
            // Gray Transformer can
            Cylinder("Transformer Unit", pos + new Vector3(.45f, 6.8f, 0f), new Vector3(.55f, .75f, .55f), metal);
            // Street light lamp
            Cube("Street Lamp Arm", pos + new Vector3(0f, 5.6f, -.55f), new Vector3(.12f, .12f, 1.1f), metal, null, false);
            Cube("Street Lamp Head", pos + new Vector3(0f, 5.5f, -1.15f), new Vector3(.65f, .16f, .38f), warmWhite, null, false);
            LightAt("Streetlight Glow", pos + new Vector3(0f, 5.2f, -1.2f), new Color(1f, .84f, .55f), 4.2f, 12f);
        }

        private static GameObject Cylinder(string name, Vector3 at, Vector3 scale, Material material, Transform parent = null)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent == null ? world : parent, false);
            go.transform.position = at;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        private static void SetupTwins()
        {
            // Toshiharu: 18-year-old male twin, blue shirt, dark denim jeans, sneakers, short spiky hair
            SpawnToshiharu(new Vector3(-1.0f, 0f, -2.5f));

            // Akiko: 18-year-old female twin, pink cardigan, cute pleated skirt, twin-tail hair ribbons, school loafers
            SpawnAkiko(new Vector3(1.2f, 0f, -2.5f));
        }

        private static void SpawnToshiharu(Vector3 pos)
        {
            GameObject root = new GameObject("Tei Toshiharu");
            root.transform.SetParent(world, false);
            root.transform.position = pos;

            CharacterController cc = root.AddComponent<CharacterController>();
            cc.height = 1.86f;
            cc.radius = .30f;
            cc.center = new Vector3(0, .93f, 0);
            cc.stepOffset = .25f;
            cc.skinWidth = .04f;

            // --- Head & Face ---
            Sphere("Boy Head", pos + new Vector3(0f, 1.58f, 0f), new Vector3(.32f, .34f, .32f), skin, root.transform);
            // Anime Boy Eyes (dark sharp anime eyes)
            Sphere("Boy Eye L", pos + new Vector3(-.075f, 1.60f, .155f), new Vector3(.045f, .055f, .025f), eyeBlack, root.transform);
            Sphere("Boy Eye R", pos + new Vector3(.075f, 1.60f, .155f), new Vector3(.045f, .055f, .025f), eyeBlack, root.transform);
            // Anime Boy Hair (layered dark hair with bangs)
            Sphere("Boy Hair Crown", pos + new Vector3(0f, 1.74f, -.02f), new Vector3(.36f, .26f, .36f), hair, root.transform);
            Cube("Boy Bangs Front", pos + new Vector3(0f, 1.70f, .14f), new Vector3(.24f, .12f, .08f), hair, root.transform, false);
            Cube("Boy Sideburn L", pos + new Vector3(-.17f, 1.56f, .04f), new Vector3(.05f, .18f, .14f), hair, root.transform, false);
            Cube("Boy Sideburn R", pos + new Vector3(.17f, 1.56f, .04f), new Vector3(.05f, .18f, .14f), hair, root.transform, false);

            // --- Upper Body: Male broad chest & casual blue shirt ---
            Cube("Boy Chest", pos + new Vector3(0f, 1.25f, 0f), new Vector3(.46f, .38f, .24f), blue, root.transform, false);
            Cube("Boy Shirt Collar", pos + new Vector3(0f, 1.43f, .06f), new Vector3(.22f, .06f, .14f), whiteCloth, root.transform, false);
            Cube("Boy Waist", pos + new Vector3(0f, .97f, 0f), new Vector3(.38f, .26f, .22f), blue, root.transform, false);

            // --- Arms ---
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * .28f;
                Cube("Boy Sleeve " + side, pos + new Vector3(x, 1.22f, 0f), new Vector3(.13f, .32f, .14f), blue, root.transform, false);
                Cube("Boy Arm " + side, pos + new Vector3(x, .92f, 0f), new Vector3(.10f, .30f, .11f), skin, root.transform, false);
                Sphere("Boy Hand " + side, pos + new Vector3(x, .74f, 0f), new Vector3(.09f, .09f, .09f), skin, root.transform);
            }

            // --- Lower Body: Male straight dark jeans & sneakers ---
            Cube("Boy Belt", pos + new Vector3(0f, .82f, 0f), new Vector3(.39f, .08f, .23f), darkCloth, root.transform, false);
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * .12f;
                Cube("Boy Pants " + side, pos + new Vector3(x, .46f, 0f), new Vector3(.16f, .68f, .18f), darkCloth, root.transform, false);
                // Sneakers with white sole
                Cube("Boy Sneaker " + side, pos + new Vector3(x, .08f, .04f), new Vector3(.16f, .12f, .27f), metal, root.transform, false);
                Cube("Boy Sneaker Sole " + side, pos + new Vector3(x, .02f, .04f), new Vector3(.17f, .04f, .28f), whiteCloth, root.transform, false);
            }

            var character = root.AddComponent<SoraDemoCharacter>();
            character.DisplayName = "Tei Toshiharu";
            game.RegisterCharacter(character);
        }

        private static void SpawnAkiko(Vector3 pos)
        {
            GameObject root = new GameObject("Tei Akiko");
            root.transform.SetParent(world, false);
            root.transform.position = pos;

            CharacterController cc = root.AddComponent<CharacterController>();
            cc.height = 1.72f;
            cc.radius = .27f;
            cc.center = new Vector3(0, .86f, 0);
            cc.stepOffset = .25f;
            cc.skinWidth = .04f;

            // --- Head & Face ---
            Sphere("Girl Head", pos + new Vector3(0f, 1.50f, 0f), new Vector3(.29f, .31f, .29f), skin, root.transform);
            // Cute Anime Eyes with sparkle highlights
            Sphere("Girl Eye L", pos + new Vector3(-.07f, 1.52f, .14f), new Vector3(.05f, .065f, .025f), eyeBlack, root.transform);
            Sphere("Girl Eye R", pos + new Vector3(.07f, 1.52f, .14f), new Vector3(.05f, .065f, .025f), eyeBlack, root.transform);
            Sphere("Girl Highlight L", pos + new Vector3(-.06f, 1.54f, .15f), new Vector3(.018f, .022f, .015f), whiteCloth, root.transform);
            Sphere("Girl Highlight R", pos + new Vector3(.08f, 1.54f, .15f), new Vector3(.018f, .022f, .015f), whiteCloth, root.transform);

            // --- Anime Twin-tail Hair with Pink Ribbons ---
            Sphere("Girl Hair Crown", pos + new Vector3(0f, 1.65f, -.02f), new Vector3(.34f, .26f, .34f), hair, root.transform);
            Cube("Girl Neat Bangs", pos + new Vector3(0f, 1.62f, .13f), new Vector3(.24f, .10f, .06f), hair, root.transform, false);
            // Two cute pink ribbon hair ties
            Cube("Girl Ribbon L", pos + new Vector3(-.19f, 1.66f, -.04f), new Vector3(.10f, .08f, .08f), coral, root.transform, false);
            Cube("Girl Ribbon R", pos + new Vector3(.19f, 1.66f, -.04f), new Vector3(.10f, .08f, .08f), coral, root.transform, false);
            // Long cascading twin tails hanging down past shoulders
            Cube("Girl TwinTail L", pos + new Vector3(-.22f, 1.34f, -.04f), new Vector3(.09f, .58f, .11f), hair, root.transform, false);
            Cube("Girl TwinTail R", pos + new Vector3(.22f, 1.34f, -.04f), new Vector3(.09f, .58f, .11f), hair, root.transform, false);

            // --- Upper Body: Cute pink cardigan / blouse & school collar ---
            Cube("Girl Cardigan", pos + new Vector3(0f, 1.20f, 0f), new Vector3(.35f, .32f, .21f), pink, root.transform, false);
            Cube("Girl Sailor Collar", pos + new Vector3(0f, 1.35f, .06f), new Vector3(.24f, .06f, .14f), whiteCloth, root.transform, false);
            Cube("Girl Slender Waist", pos + new Vector3(0f, .96f, 0f), new Vector3(.28f, .20f, .18f), pink, root.transform, false);

            // --- Arms ---
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * .22f;
                Cube("Girl Sleeve " + side, pos + new Vector3(x, 1.18f, 0f), new Vector3(.10f, .30f, .11f), pink, root.transform, false);
                Cube("Girl Forearm " + side, pos + new Vector3(x, .90f, 0f), new Vector3(.08f, .28f, .09f), skin, root.transform, false);
                Sphere("Girl Hand " + side, pos + new Vector3(x, .72f, 0f), new Vector3(.075f, .075f, .075f), skin, root.transform);
            }

            // --- Lower Body: Cute flared pleated school skirt & knee-high socks ---
            Cube("Girl Pleated Skirt", pos + new Vector3(0f, .80f, 0f), new Vector3(.44f, .26f, .36f), darkCloth, root.transform, false);
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * .10f;
                // Thighs (skin)
                Cube("Girl Thigh " + side, pos + new Vector3(x, .62f, 0f), new Vector3(.10f, .16f, .11f), skin, root.transform, false);
                // Knee-high dark socks
                Cube("Girl Sock " + side, pos + new Vector3(x, .35f, 0f), new Vector3(.11f, .42f, .12f), darkCloth, root.transform, false);
                // School Loafers
                Cube("Girl Loafer " + side, pos + new Vector3(x, .07f, .03f), new Vector3(.12f, .10f, .22f), wood, root.transform, false);
            }

            var character = root.AddComponent<SoraDemoCharacter>();
            character.DisplayName = "Tei Akiko";
            game.RegisterCharacter(character);
        }

        private static void SetupCustomers()
        {
            SpawnCustomer("Rei", new Vector3(-.5f, 0, 1.0f), gold, 0);
            SpawnCustomer("Miyu", new Vector3(4.7f, 0, -2.0f), coral, 1);
            SpawnCustomer("Kazuki", new Vector3(-4f, 0, 2.8f), green, 2);
        }

        private static void SpawnCustomer(string name, Vector3 pos, Material clothes, int ix)
        {
            GameObject root = new GameObject("NPC Customer " + name);
            root.transform.SetParent(world, false);
            root.transform.position = pos;
            Sphere(name + " body", pos + Vector3.up * 1.02f, new Vector3(.65f, 1.05f, .42f), clothes, root.transform);
            Sphere(name + " face", pos + Vector3.up * 1.73f, new Vector3(.38f, .4f, .36f), skin, root.transform);
            Sphere(name + " hair", pos + Vector3.up * 1.91f, new Vector3(.42f, .22f, .41f), hair, root.transform);
            GameObject prompt = Cube("NPC Interaction marker", pos + Vector3.up * 2.28f, new Vector3(.32f, .32f, .32f), gold, root.transform, false);
            TagInteract(prompt, InteractKind.Customer, "Noi chuyen voi " + name, ix);
        }

        private static void SetupCamera()
        {
            GameObject cameraGo = new GameObject("SORA Main Camera");
            cameraGo.tag = "MainCamera";
            cameraGo.transform.SetParent(world, false);
            cameraGo.transform.position = new Vector3(0, 3.8f, -6f);
            var cam = cameraGo.AddComponent<Camera>();
            cam.fieldOfView = 60f;
            cam.nearClipPlane = .15f;
            cam.farClipPlane = 250f;
            cam.clearFlags = CameraClearFlags.Skybox;
            cameraGo.AddComponent<AudioListener>();
            cameraGo.AddComponent<SoraThirdPersonCamera>();
        }

        private static void SetupRain()
        {
            GameObject go = new GameObject("Low-cost Rain Particle System");
            go.transform.SetParent(world, false);
            go.transform.position = new Vector3(0f, 12f, -14f);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = true;
            main.duration = 8f;
            main.startLifetime = 1.6f;
            main.startSpeed = 0f;
            main.gravityModifier = 1.5f;
            main.startSize = .038f;
            main.maxParticles = 3500;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 1200f;
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(85f, 1f, 35f);
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            ps.Play();
            // Move rain with the active twin so the larger district stays rainy.
            go.AddComponent<SoraDistrictRainFollower>();
            if (shader != null)
            {
                Material rainMaterial = new Material(shader);
                rainMaterial.color = new Color(.78f, .88f, 1f, .45f);
                if (rainMaterial.HasProperty("_BaseColor")) rainMaterial.SetColor("_BaseColor", rainMaterial.color);
                renderer.sharedMaterial = rainMaterial;
            }
        }
    }
}
