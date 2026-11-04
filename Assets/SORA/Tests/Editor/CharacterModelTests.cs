using System.Linq;
using NUnit.Framework;
using UnityEditor;
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
                // Posed geometry: skinned renderer.bounds is padded to cover every animation frame.
                var points = renderers.SelectMany(WorldVertices).ToArray();
                var b = new Bounds(points[0], Vector3.zero);
                foreach (var p in points) b.Encapsulate(p);
                foreach (var r in renderers)
                    foreach (var m in r.sharedMaterials) Assert.That(m, Is.Not.Null, r.name + " has a missing material.");
                // Feet on the root pivot; height below Akiko's 1.72 m controller.
                Assert.That(b.min.y, Is.EqualTo(0f).Within(0.03f));
                Assert.That(b.size.y, Is.InRange(1.45f, 1.72f));
                Assert.That(go.GetComponentsInChildren<Collider>(), Is.Empty, "Model colliders would fight the CharacterController.");

                var animator = go.GetComponent<Animator>();
                Assert.That(animator, Is.Not.Null);
                Assert.That(animator.avatar != null && animator.avatar.isHuman, "Humanoid avatar missing.");
                Assert.That(animator.applyRootMotion, Is.False, "SoraDemoCharacter moves the CharacterController.");
                Assert.That(animator.GetBoneTransform(HumanBodyBones.LeftToes).position.z,
                    Is.GreaterThan(animator.GetBoneTransform(HumanBodyBones.LeftFoot).position.z), "Model must face +Z.");
                var controller = animator.runtimeAnimatorController;
                Assert.That(controller, Is.Not.Null);
                Assert.That(controller.animationClips.Select(c => c.name).Distinct(), Is.EquivalentTo(new[] { "Idle", "Walk", "Run" }));
                Assert.That(controller.animationClips.All(c => c.isLooping), "Locomotion clips must loop.");
                // A stale Humanoid rest pose mirrors the retarget: her left leg crosses to +X.
                AnimationMode.StartAnimationMode();
                try
                {
                    foreach (var clip in controller.animationClips.Distinct())
                    {
                        AnimationMode.BeginSampling();
                        AnimationMode.SampleAnimationClip(go, clip, 0f);
                        AnimationMode.EndSampling();
                        Assert.That(animator.GetBoneTransform(HumanBodyBones.LeftFoot).position.x, Is.LessThan(0f), clip.name + " mirrors the legs.");
                        if (clip.name == "Idle") continue;
                        // Quarter cycle = full stride; a lossy retarget leaves the feet together.
                        AnimationMode.BeginSampling();
                        AnimationMode.SampleAnimationClip(go, clip, clip.length * 0.25f);
                        AnimationMode.EndSampling();
                        float stride = animator.GetBoneTransform(HumanBodyBones.LeftFoot).position.z
                                     - animator.GetBoneTransform(HumanBodyBones.RightFoot).position.z;
                        Assert.That(stride, Is.GreaterThan(0.35f), clip.name + " stride (m).");
                    }
                }
                finally { AnimationMode.StopAnimationMode(); }
            }
            finally { Object.DestroyImmediate(go); }
        }

        static Vector3[] WorldVertices(Renderer r)
        {
            var skinned = r as SkinnedMeshRenderer;
            var mesh = skinned != null ? new Mesh() : r.GetComponent<MeshFilter>().sharedMesh;
            if (skinned != null) skinned.BakeMesh(mesh);
            var m = r.transform.localToWorldMatrix;
            var v = mesh.vertices.Select(m.MultiplyPoint3x4).ToArray();
            if (skinned != null) Object.DestroyImmediate(mesh);
            return v;
        }
    }
}
