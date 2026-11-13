using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Sora.AfterTheRain.Demo.Tests
{
    public sealed class CharacterModelTests
    {
        /// <summary>Every clip authored in Tools/Blender/sweater_girl_rig.py, and what each one is allowed to do.</summary>
        /// <remarks>
        /// Loops mirrors the loopTime flag in sweater_girl.fbx.meta: a one-shot left looping plays forever.
        /// FeetDown is false only where she is deliberately airborne, so the floor-contact bound is skipped there.
        /// MinMove is the displacement that proves the clip is bound at all; a gait has to carry a whole stride,
        /// while Idle and Fall are near-stationary loops that only have to breathe.
        /// </remarks>
        static readonly (string Name, bool Loops, bool FeetDown, float MinMove)[] Clips =
        {
            ("Fall",      true,  false, .001f),
            ("Idle",      true,  true,  .001f),
            ("IdleLookAround", false, true, .01f),
            ("IdleStretch", false, true, .01f),
            ("Jog",       true, true, .08f),
            ("Jump",      false, false, .04f),
            ("Land",      false, true,  .04f),
            ("Run",       true,  true,  .08f),
            ("TurnLeft",  false, true,  .04f),
            ("TurnRight", false, true,  .04f),
            ("Walk",      true,  true,  .08f),
        };

        /// <summary>Only these two swing one foot clear of the other; the rest pose or pivot in place.</summary>
        static readonly string[] GaitClips = { "Run", "Jog", "Walk" };

        static string[] ClipNames => Clips.Select(c => c.Name).ToArray();
        static string[] LoopingClipNames => Clips.Where(c => c.Loops).Select(c => c.Name).ToArray();
        static (string Name, bool Loops, bool FeetDown, float MinMove) Expected(string name) => Clips.Single(c => c.Name == name);

        [TestCase("Chisa", 1.65f)]
        [TestCase("Citlali", 1.52f)]
        [TestCase("Denia", 1.58f)]
        [TestCase("Lynae", 1.65f)]
        [TestCase("MaleRover", 1.80f)]
        [TestCase("Zhongli", 1.80f)]
        public void LocalModelHasRequestedHeightAndDeformsWithLocomotion(string id, float height)
        {
            var prefab = Resources.Load<GameObject>(SoraCharacterModels.ResourceFolder + id);
            if (prefab == null) Assert.Ignore("Local owner assets absent; run SORA/Characters/4. Build Playable Model Library.");
            var go = Object.Instantiate(prefab);
            try
            {
                Assert.That(go.transform.localScale, Is.EqualTo(Vector3.one));
                Assert.That(go.GetComponentsInChildren<Collider>(), Is.Empty);
                var skins = go.GetComponentsInChildren<SkinnedMeshRenderer>();
                Assert.That(skins, Is.Not.Empty, "Every model, including Rover, needs skinned geometry.");
                var rest = BoundsOf(go);
                Assert.That(rest.min.y, Is.EqualTo(0).Within(.015f), id + " feet pivot.");
                Assert.That(rest.size.y, Is.EqualTo(height).Within(.015f), id + " requested height.");
                var animator = go.GetComponent<Animator>();
                Assert.That(animator != null && animator.avatar != null && animator.avatar.isValid && animator.avatar.isHuman, Is.True);
                Assert.That(animator.applyRootMotion, Is.False);
                Assert.That(animator.GetBoneTransform(HumanBodyBones.LeftToes).position.z,
                    Is.GreaterThan(animator.GetBoneTransform(HumanBodyBones.LeftFoot).position.z), "Forward must be +Z.");
                var clips = animator.runtimeAnimatorController.animationClips.Distinct().ToArray();
                Assert.That(clips.Select(c => c.name), Is.EquivalentTo(ClipNames));
                Assert.That(clips.Where(c => c.isLooping).Select(c => c.name), Is.EquivalentTo(LoopingClipNames),
                    "loopTime in sweater_girl.fbx.meta disagrees with the authored clip table.");
                var controller = (UnityEditor.Animations.AnimatorController)animator.runtimeAnimatorController;
                Assert.That(controller.layers[0].stateMachine.defaultState.iKOnFeet, Is.True);
                // SoraDemoCharacter writes all five every frame; a rebuild that drops one silently kills jump or turn.
                Assert.That(controller.parameters.Select(p => p.name),
                    Is.EquivalentTo(new[] { "Speed", "IsGrounded", "VerticalSpeed", "Jump", "Turn", "GaitRate", "IdleAction", "IdleVariant" }));
                Assert.That(controller.layers[0].stateMachine.states.Select(s => s.state.name),
                    Is.EquivalentTo(new[] { "Locomotion", "Jump", "Fall", "Land", "TurnLeft", "TurnRight", "IdleLookAround", "IdleStretch" }));
                if (id == "Denia") Assert.That(skins.Sum(s => s.sharedMesh.blendShapeCount), Is.EqualTo(187));
                AnimationMode.StartAnimationMode();
                try
                {
                    foreach (var clip in clips)
                    {
                        var expected = Expected(clip.name);
                        Sample(clip, 0);
                        var first = skins.SelectMany(WorldVertices).ToArray();
                        var head = animator.GetBoneTransform(HumanBodyBones.Head);
                        var initialHead = head.rotation;
                        Sample(clip, .25f);
                        var posed = skins.SelectMany(WorldVertices).ToArray();
                        float displacement = first.Zip(posed, Vector3.Distance).Max();
                        Assert.That(displacement, Is.GreaterThan(expected.MinMove), id + "/" + clip.name + " must deform actual geometry.");
                        if (clip.name == "IdleLookAround")
                        {
                            var change = head.rotation * Quaternion.Inverse(initialHead);
                            Assert.That(Mathf.Abs((change * Vector3.forward).x), Is.GreaterThan(.25f), id + " idle look must change heading.");
                            Assert.That(Vector3.Dot(change * Vector3.up, Vector3.up), Is.GreaterThan(.94f), id + " idle look must keep the head upright.");
                        }
                        for (int frame = 0; frame < 8; frame++)
                        {
                            Sample(clip, frame / 8f);
                            var bounds = BoundsOf(go);
                            // Raw clip sampling excludes controller foot IK; these bounds only catch gross deformation.
                            if (expected.FeetDown)
                                Assert.That(bounds.min.y, Is.GreaterThan(-.04f), id + "/" + clip.name + " retarget bounds.");
                            Assert.That(bounds.max.y, Is.LessThan(height + .15f), id + "/" + clip.name + " rig explosion.");
                            Assert.That(bounds.size.x, Is.LessThan(height), id + "/" + clip.name + " rig width.");
                            Vector3 hipRight = (animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).position
                                - animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position).normalized;
                            Vector3 footSpan = animator.GetBoneTransform(HumanBodyBones.RightFoot).position
                                - animator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
                            Assert.That(Vector3.Dot(footSpan, hipRight), Is.GreaterThan(.04f), id + "/" + clip.name + " crossed legs at " + frame + "/8.");
                        }
                    }
                    void Sample(AnimationClip clip, float normalizedTime)
                    {
                        AnimationMode.BeginSampling();
                        AnimationMode.SampleAnimationClip(go, clip, clip.length * normalizedTime);
                        AnimationMode.EndSampling();
                    }
                }
                finally { AnimationMode.StopAnimationMode(); }
            }
            finally { Object.DestroyImmediate(go); }
        }

        static Bounds BoundsOf(GameObject go)
        {
            var points = go.GetComponentsInChildren<Renderer>().SelectMany(WorldVertices).ToArray();
            var bounds = new Bounds(points[0], Vector3.zero);
            foreach (var point in points) bounds.Encapsulate(point);
            return bounds;
        }

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
                Assert.That(controller.animationClips.Select(c => c.name).Distinct(), Is.EquivalentTo(ClipNames));
                Assert.That(controller.animationClips.Where(c => c.isLooping).Select(c => c.name).Distinct(), Is.EquivalentTo(LoopingClipNames));
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
                        if (!GaitClips.Contains(clip.name)) continue; // only Walk/Run swing a foot clear
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
            if (skinned != null) skinned.BakeMesh(mesh, true);
            var m = r.transform.localToWorldMatrix;
            var v = mesh.vertices.Select(m.MultiplyPoint3x4).ToArray();
            if (skinned != null) Object.DestroyImmediate(mesh);
            return v;
        }
    }
}
