using UnityEngine;

namespace Sora.AfterTheRain.Demo
{
    public static class SoraDemoWorld
    {
        private static SoraDemoCore game;
        private static Transform world;
        private static Material tiles, walls, metal, warmWhite, darkGlass, road, pavement, blue, coral, pink, green, gold, skin, hair;

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
        }

        private static Material Mat(Color c, float smoothness = 0.5f, bool emissive = false)
        {
            // A minimal project may start with the Built-in pipeline even when the
            // URP package is installed. Select a shader compatible with the active
            // pipeline; otherwise every object can show up pink in the Editor.
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
            skin = Mat(new Color(.95f, .74f, .59f));
            hair = Mat(new Color(.12f, .095f, .14f));
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
            RenderSettings.ambientLight = new Color(.32f, .38f, .51f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(.055f, .09f, .15f);
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = .013f;
            Camera.main?.gameObject.SetActive(false);
            Cube("Store tiled floor", new Vector3(0, -.18f, 0), new Vector3(14, .30f, 11f), tiles);
            Cube("Back wall", new Vector3(0, 2.25f, 5.45f), new Vector3(14, 4.5f, .35f), walls);
            Cube("Left wall", new Vector3(-7f, 2.25f, .1f), new Vector3(.36f, 4.5f, 10.7f), walls);
            Cube("Right wall", new Vector3(7f, 2.25f, .1f), new Vector3(.36f, 4.5f, 10.7f), walls);
            Cube("Left storefront frame", new Vector3(-5.6f, 2.1f, -5.25f), new Vector3(2.8f, 4.2f, .25f), darkGlass);
            Cube("Right storefront frame", new Vector3(5.6f, 2.1f, -5.25f), new Vector3(2.8f, 4.2f, .25f), darkGlass);
            Cube("Top storefront lintel", new Vector3(0f, 3.9f, -5.25f), new Vector3(8.6f, .45f, .25f), metal);
            Cube("Bottom window border", new Vector3(0f, .3f, -5.25f), new Vector3(8.6f, .25f, .25f), metal);
            Cube("Front glass - left (stylized)", new Vector3(-2.5f, 2f, -5.25f), new Vector3(2.7f, 3.5f, .04f), darkGlass, null, false);
            Cube("Front glass - right (stylized)", new Vector3(2.5f, 2f, -5.25f), new Vector3(2.7f, 3.5f, .04f), darkGlass, null, false);
            Cube("Sliding entrance frame", new Vector3(0f, 1.85f, -5.3f), new Vector3(2.1f, 3.7f, .16f), metal, null, false);
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
            // Aisles run from front to back; central aisle stays traversable.
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
            Cube("Rainy asphalt road", new Vector3(0f, -.2f, -14.3f), new Vector3(40f, .28f, 15f), road);
            Cube("Pavement in front of store", new Vector3(0f, -.10f, -7f), new Vector3(40f, .25f, 3.9f), pavement);
            for (int i = -5; i <= 5; i++)
            {
                Cube("Road line", new Vector3(i * 3.75f, -.043f, -14f), new Vector3(1.9f, .02f, .15f), warmWhite, null, false);
                Cube("Wet road reflected highlight", new Vector3(i * 3.5f, -.040f, -9.0f), new Vector3(.11f, .009f, 2.5f), i % 2 == 0 ? coral : blue, null, false);
            }
            // Simple Tokyo neighborhood silhouettes and lit shop windows.
            for (int i = 0; i < 5; i++)
            {
                float x = -17f + i * 8.5f;
                float height = 7.5f + (i % 3) * 2.8f;
                Cube("Across street building", new Vector3(x, height / 2f, -24.5f), new Vector3(7.6f, height, 4f), metal);
                for (int floor = 1; floor < height / 2.2f; floor++)
                for (int col = 0; col < 3; col++)
                    Cube("Lit Tokyo window", new Vector3(x - 2.3f + col * 2.3f, floor * 2.15f, -22.4f), new Vector3(.9f, .94f, .09f), (col + floor + i) % 3 == 0 ? warmWhite : darkGlass, null, false);
            }
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * 9.2f;
                Cube("Street lamp pole", new Vector3(x, 2.75f, -8f), new Vector3(.17f, 5.5f, .17f), metal);
                Cube("Street lamp head", new Vector3(x, 5.4f, -8f), new Vector3(.88f, .18f, .48f), warmWhite, null, false);
                LightAt("Street lamp", new Vector3(x, 5.15f, -8f), new Color(1f, .74f, .41f), 4f, 11f);
            }
            GameObject exitDoor = Cube("Exit door step", new Vector3(0, -.02f, -5.7f), new Vector3(1.8f, .06f, .5f), gold, null, false);
            TagInteract(exitDoor, InteractKind.ExitDoor, "Ra pho Tokyo");
        }

        private static void SetupTwins()
        {
            SpawnTwin("Tei Toshiharu", new Vector3(-.5f, 0, -2.7f), blue, false);
            SpawnTwin("Tei Akiko", new Vector3(1.4f, 0, -2.7f), pink, true);
        }

        private static void SpawnTwin(string name, Vector3 pos, Material clothing, bool longHair)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(world, false);
            root.transform.position = pos;
            CharacterController cc = root.AddComponent<CharacterController>();
            cc.height = 1.88f;
            cc.radius = .30f;
            cc.center = new Vector3(0, .96f, 0);
            cc.stepOffset = .23f;
            cc.skinWidth = .05f;
            Sphere("Torso", pos + Vector3.up * 1.09f, new Vector3(.55f, .75f, .3f), clothing, root.transform);
            Sphere("Head", pos + Vector3.up * 1.66f, new Vector3(.38f, .43f, .34f), skin, root.transform);
            Sphere("Hair", pos + Vector3.up * 1.89f, new Vector3(.43f, longHair ? .31f : .22f, .38f), hair, root.transform);
            for (int s = -1; s <= 1; s += 2)
            {
                Sphere("Arm", pos + new Vector3(s * .38f, 1.10f, 0), new Vector3(.18f, .64f, .19f), clothing, root.transform);
                Sphere("Leg", pos + new Vector3(s * .15f, .42f, 0), new Vector3(.21f, .85f, .23f), metal, root.transform);
            }
            var character = root.AddComponent<SoraDemoCharacter>();
            character.DisplayName = name;
            game.RegisterCharacter(character);
        }

        private static void SetupCustomers()
        {
            // All three prototype customers are adults; their social routes are placeholders only.
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
            cameraGo.transform.position = new Vector3(0, 5.3f, -9f);
            Camera camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.05f, .08f, .12f);
            camera.fieldOfView = 65f;
            camera.nearClipPlane = .06f;
            cameraGo.AddComponent<AudioListener>();
            cameraGo.AddComponent<SoraThirdPersonCamera>();
            GameObject sun = new GameObject("Moonlight");
            sun.transform.SetParent(world, false);
            sun.transform.rotation = Quaternion.Euler(45f, -35f, 0);
            Light directional = sun.AddComponent<Light>();
            directional.type = LightType.Directional;
            directional.color = new Color(.43f, .65f, 1f);
            directional.intensity = .25f;
        }

        private static void SetupRain()
        {
            GameObject go = new GameObject("Low-cost Rain Particle System");
            go.transform.SetParent(world, false);
            go.transform.position = new Vector3(0f, 8f, -15f);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = true;
            main.duration = 8f;
            main.startLifetime = 1.3f;
            main.startSpeed = 0f;
            main.gravityModifier = 1.2f;
            main.startSize = .035f;
            main.maxParticles = 1800;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 550f;
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(35f, 1f, 15f);
            // Gravity drops rain vertically over the street; no rotation needed.
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            ps.Play();
            if (shader != null)
            {
                Material rainMaterial = new Material(shader);
                rainMaterial.color = new Color(.65f, .79f, 1f, .5f);
                renderer.sharedMaterial = rainMaterial;
            }
            ps.Play();
        }
    }
}
