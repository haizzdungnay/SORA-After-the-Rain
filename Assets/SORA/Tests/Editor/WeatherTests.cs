using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Sora.AfterTheRain.Demo.Tests
{
    public sealed class WeatherTests
    {
        [Test]
        public void WetRoadRespondsToLocalStoreLight()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                Assert.Ignore("Run this rendering regression with a graphics device, without -nographics.");
            var surface = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var cameraObject = new GameObject("Wet-road light regression camera");
            var lampObject = new GameObject("Wet-road light regression lamp");
            var material = new Material(Resources.Load<Material>("Weather/Puddle"));
            var target = RenderTexture.GetTemporary(96, 96, 24);
            var image = new Texture2D(96, 96, TextureFormat.RGB24, false);
            var previousTarget = RenderTexture.active;
            var previousCenter = Shader.GetGlobalVector("_SoraWetCenter");
            float previousRain = Shader.GetGlobalFloat("_SoraRain"), previousWet = Shader.GetGlobalFloat("_SoraWetness"),
                previousPuddles = Shader.GetGlobalFloat("_SoraPuddles");
            try
            {
                surface.layer = 31;
                surface.transform.SetPositionAndRotation(new Vector3(1000, 0, 1000), Quaternion.Euler(90, 0, 0));
                surface.transform.localScale = Vector3.one * 4;
                surface.GetComponent<Renderer>().sharedMaterial = material;
                var camera = cameraObject.AddComponent<Camera>();
                camera.transform.position = new Vector3(1000, 2, 998);
                camera.transform.LookAt(surface.transform);
                camera.cullingMask = 1 << 31;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                camera.orthographic = true;
                camera.orthographicSize = 1.4f;
                camera.targetTexture = target;
                var lamp = lampObject.AddComponent<Light>();
                lamp.transform.position = new Vector3(1000, 2, 1000);
                lamp.type = LightType.Point;
                lamp.color = Color.white;
                lamp.range = 6;
                lamp.intensity = 20;
                lamp.cullingMask = 1 << 31;
                Shader.SetGlobalVector("_SoraWetCenter", new Vector4(1000, 1000, 16, 0));
                Shader.SetGlobalFloat("_SoraRain", 0);
                Shader.SetGlobalFloat("_SoraWetness", 1);
                Shader.SetGlobalFloat("_SoraPuddles", 0);
                float Brightness(bool enabled)
                {
                    lamp.enabled = enabled;
                    camera.Render();
                    RenderTexture.active = target;
                    image.ReadPixels(new Rect(0, 0, 96, 96), 0, 0);
                    image.Apply();
                    return image.GetPixels().Average(c => c.grayscale);
                }
                float unlit = Brightness(false), lit = Brightness(true);
                Assert.That(lit, Is.GreaterThan(unlit + .01f),
                    "A local store light must illuminate the wet film, not only the road below it.");
            }
            finally
            {
                RenderTexture.active = previousTarget;
                Shader.SetGlobalVector("_SoraWetCenter", previousCenter);
                Shader.SetGlobalFloat("_SoraRain", previousRain);
                Shader.SetGlobalFloat("_SoraWetness", previousWet);
                Shader.SetGlobalFloat("_SoraPuddles", previousPuddles);
                UnityEngine.Object.DestroyImmediate(cameraObject);
                UnityEngine.Object.DestroyImmediate(lampObject);
                UnityEngine.Object.DestroyImmediate(surface);
                UnityEngine.Object.DestroyImmediate(material);
                UnityEngine.Object.DestroyImmediate(image);
                RenderTexture.ReleaseTemporary(target);
            }
        }

        [Test]
        public void RainAccumulatesWaterAndClearWeatherDriesGradually()
        {
            var state = new SoraWeatherState { kind = SoraWeatherKind.HeavyRain, rain = 1, wetness = .1f, puddles = .1f };
            for (int i = 0; i < 1200; i++) state.Advance(.1f, false);
            Assert.That(state.wetness, Is.InRange(.9f, 1f));
            Assert.That(state.puddles, Is.GreaterThan(.6f));
            state.SetWeather(SoraWeatherKind.Clear);
            float before = state.puddles;
            state.Advance(1, false);
            Assert.That(state.puddles, Is.GreaterThan(before - .01f), "Stopping rain must not erase puddles");
            for (int i = 0; i < 12000; i++) state.Advance(.1f, false);
            Assert.That(state.rain, Is.Zero);
            Assert.That(state.puddles, Is.Zero);
            Assert.That(state.wetness, Is.Zero);
        }

        [Test]
        public void ModeChangesSmoothlyAndAutomaticCycleAdvances()
        {
            var state = new SoraWeatherState { kind = SoraWeatherKind.Clear, rain = 0, remaining = .05f };
            state.Advance(.1f);
            Assert.That(state.kind, Is.EqualTo(SoraWeatherKind.Drizzle));
            Assert.That(state.rain, Is.InRange(0, .03f));
            Assert.That(state.remaining, Is.GreaterThan(60));
            state.SetWeather(SoraWeatherKind.HeavyRain);
            state.Advance(.1f, false);
            Assert.That(state.rain, Is.LessThan(.1f));
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(-1f)]
        public void InvalidElapsedTimeCannotPoisonWeather(float seconds)
        {
            var state = new SoraWeatherState();
            Assert.Throws<ArgumentOutOfRangeException>(() => state.Advance(seconds));
            Assert.That(state.IsValid(), Is.True);
        }

        [Test]
        public void SnapshotIsIndependentAndRejectsMalformedSaveValues()
        {
            var state = new SoraWeatherState();
            var snapshot = state.Copy();
            snapshot.puddles = .9f;
            Assert.That(state.puddles, Is.Not.EqualTo(snapshot.puddles));
            snapshot.wetness = float.NaN;
            Assert.That(snapshot.IsValid(), Is.False);
            snapshot.wetness = 1.01f;
            Assert.That(snapshot.IsValid(), Is.False);
            snapshot.wetness = .5f;
            snapshot.kind = (SoraWeatherKind)99;
            Assert.That(snapshot.IsValid(), Is.False);
            var legacy = JsonUtility.FromJson<SoraSaveData>("{\"schema\":1,\"storeCash\":380}");
            Assert.That(legacy.hasWeatherSnapshot, Is.False, "JsonUtility fabricates absent inline objects; old saves must not apply one");
            Assert.That(legacy.storeCash, Is.EqualTo(380));
        }

        [Test]
        public void RoofBlocksPuddlePlacementButOpenGroundDoesNot()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                ground.name = "Rainy asphalt road";
                ground.transform.position = new Vector3(500, -.2f, 500);
                ground.transform.localScale = new Vector3(12, .2f, 12);
                roof.name = "Test shelter";
                roof.transform.position = new Vector3(500, 3, 500);
                roof.transform.localScale = new Vector3(3, .2f, 3);
                Physics.SyncTransforms();
                Assert.That(SoraWetSurfaces.TryGround(new Vector3(500, 0, 500), out _), Is.False);
                Assert.That(SoraWetSurfaces.TryGround(new Vector3(504, 0, 500), out var hit), Is.True);
                Assert.That(hit.point.y, Is.EqualTo(-.1f).Within(.01f));
                ground.name = "Store tiled floor";
                Assert.That(SoraWetSurfaces.TryGround(new Vector3(504, 0, 500), out _), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(roof);
                UnityEngine.Object.DestroyImmediate(ground);
            }
        }

        [Test]
        public void PublicPbrRoadHasRealGroundSupportAndCharacterDoesNotPunchDryHole()
        {
            var model = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(SoraCityRuntimeWorld.PublicV03Path));
            var character = new GameObject("Weather ground-query character");
            try
            {
                model.transform.position = new Vector3(600, 0, 600);
                typeof(SoraCityRuntimeWorld).GetMethod("AddGroundColliders", BindingFlags.NonPublic | BindingFlags.Static)
                    .Invoke(null, new object[] { model });
                var road = model.GetComponentsInChildren<MeshFilter>().First(m => m.name.StartsWith("V03/ROAD_PBR/"));
                Assert.That(road.GetComponent<MeshCollider>(), Is.Not.Null);
                Vector3 point = road.GetComponent<Renderer>().bounds.center;
                character.transform.position = point;
                var capsule = character.AddComponent<CharacterController>();
                capsule.center = Vector3.up;
                Physics.SyncTransforms();
                Assert.That(SoraWetSurfaces.TryGround(point, out var hit), Is.True);
                Assert.That(hit.collider, Is.Not.InstanceOf<CharacterController>());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(character);
                UnityEngine.Object.DestroyImmediate(model);
            }
        }
    }
}
