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
