using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Sora.AfterTheRain.Demo.Tests
{
    public sealed class CharacterMotionTests
    {
        [Test]
        public void MotionUpgradePreservesLegacySourceBlendTreeAndClipBindings()
        {
            var controller = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(
                "Assets/SORA/Characters/SweaterGirl/TeiAkiko.controller");
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.layers[0].stateMachine.defaultState.motion, Is.TypeOf<UnityEditor.Animations.BlendTree>());
            Assert.That(controller.animationClips.Select(c => c.name).Distinct(), Is.EquivalentTo(new[] { "Idle", "Walk", "Run" }));
        }

        [TestCase("Locomotion")]
        [TestCase("Land")]
        [TestCase("TurnLeft")]
        [TestCase("TurnRight")]
        [TestCase("IdleLookAround")]
        [TestCase("IdleStretch")]
        [TestCase("MoveStart")]
        [TestCase("StopLeft")]
        [TestCase("StopRight")]
        public void PhysicsApprovedJumpInterruptsGroundedActionsImmediately(string initialState)
        {
            var prefab = Resources.Load<GameObject>(SoraCharacterModels.ResourceFolder + "Zhongli");
            Assert.That(prefab, Is.Not.Null);
            var go = Object.Instantiate(prefab);
            try
            {
                var animator = go.GetComponent<Animator>();
                animator.SetInteger("IdleVariant", initialState == "IdleStretch" ? 2 : 1);
                animator.Play(initialState, 0, .1f);
                animator.Update(0);
                // Move already reports airborne on the launch frame: physics is the jump authority.
                animator.SetBool("IsGrounded", false);
                animator.SetFloat("VerticalSpeed", 4.2f);
                animator.SetTrigger("Jump");
                for (int i = 0; i < 6; i++) animator.Update(.02f);
                Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"), Is.True, initialState);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [TestCase("Walk")]
        [TestCase("Jog")]
        [TestCase("Run")]
        public void GaitClipsCarryTwoAlternatingFootContacts(string name)
        {
            var animator = Resources.Load<GameObject>(SoraCharacterModels.ResourceFolder + "Zhongli").GetComponent<Animator>();
            var clip = animator.runtimeAnimatorController.animationClips.First(c => c.name == name);
            var contacts = clip.events.Where(e => e.functionName == "Footstep").OrderBy(e => e.time).ToArray();
            Assert.That(contacts.Select(e => e.intParameter), Is.EqualTo(new[] { 0, 1 }));
            Assert.That(contacts[0].time / clip.length, Is.EqualTo(.25f).Within(.015f));
            Assert.That(contacts[1].time / clip.length, Is.EqualTo(.75f).Within(.015f));
        }

        [TestCase("MoveStart", 0)]
        [TestCase("StopLeft", 1)]
        [TestCase("StopRight", 0)]
        public void StartAndClosingStepsCarryOneContact(string name, int foot)
        {
            var animator = Resources.Load<GameObject>(SoraCharacterModels.ResourceFolder + "Zhongli").GetComponent<Animator>();
            var clip = animator.runtimeAnimatorController.animationClips.First(c => c.name == name);
            var contact = clip.events.Single(e => e.functionName == "Footstep");
            Assert.That(contact.intParameter, Is.EqualTo(foot));
            Assert.That(contact.time / clip.length, Is.EqualTo(.8f).Within(.015f));
        }

        [Test]
        public void IdleVariationCancelsOnMovementAndMenuActivity()
        {
            var root = new GameObject("motion idle test");
            try
            {
                root.AddComponent<CharacterController>();
                var character = root.AddComponent<SoraDemoCharacter>();
                character.InitializeAppearance(0);
                Assert.That(character.TrySetModel("Zhongli"), Is.True);
                var motion = root.GetComponentInChildren<SoraCharacterMotion>();
                var animator = root.GetComponentInChildren<Animator>();
                animator.Rebind();
                animator.Update(0);
                motion.Tick(0, true, -2, false, 0, false, 8.1f);
                for (int i = 0; i < 15; i++) animator.Update(.02f);
                Assert.That(motion.IdleCount, Is.EqualTo(1));
                Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion"), Is.False);
                motion.Tick(0, true, -2, false, 0, true, .02f);
                for (int i = 0; i < 8; i++) animator.Update(.02f);
                Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion"), Is.True);
                motion.Tick(0, true, -2, false, 0, true, 20);
                Assert.That(motion.IdleCount, Is.EqualTo(1), "Menus/movement must not schedule another idle.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        [TestCase(.02f)]
        [TestCase(.1f)]
        public void JumpRejectsAirborneSecondPressAndLandsOnce(float deltaTime)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var root = new GameObject("motion physics test");
            try
            {
                floor.transform.position = new Vector3(0, -.5f, 0);
                floor.transform.localScale = new Vector3(20, 1, 20);
                var cc = root.AddComponent<CharacterController>();
                cc.height = 1.8f;
                cc.center = Vector3.up * .9f;
                cc.radius = .25f;
                var character = root.AddComponent<SoraDemoCharacter>();
                character.InitializeAppearance(0);
                Physics.SyncTransforms();
                for (int i = 0; i < 5; i++) character.Move(Vector3.zero, 0, false, 0, .02f);
                Assert.That(cc.isGrounded, Is.True);
                float peak = 0;
                for (int i = 0; i < Mathf.CeilToInt(1.6f / deltaTime); i++)
                {
                    character.Move(Vector3.zero, 0, i == 0 || i == Mathf.RoundToInt(.16f / deltaTime), 0, deltaTime);
                    peak = Mathf.Max(peak, root.transform.position.y);
                }
                Assert.That(peak, Is.InRange(.65f, 1.05f), "A second airborne press must not add jump height.");
                Assert.That(cc.isGrounded, Is.True);
                Assert.That(root.transform.position.y, Is.EqualTo(0).Within(.08f));
                Vector3 before = root.transform.position;
                character.Move(new Vector3(float.NaN, 0, 0), 5, false, 0, .02f);
                Assert.That(root.transform.position, Is.EqualTo(before), "Invalid motion must not corrupt transforms.");
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(floor); }
        }

        [TestCase("Chisa")]
        [TestCase("Zhongli")]
        public void UmbrellaLayerRaisesArmWithoutChangingLegsOrRoot(string model)
        {
            var go = Object.Instantiate(Resources.Load<GameObject>(SoraCharacterModels.ResourceFolder + model));
            try
            {
                var animator = go.GetComponent<Animator>();
                var controller = (UnityEditor.Animations.AnimatorController)animator.runtimeAnimatorController;
                var upper = controller.layers[1];
                Assert.That(upper.name, Is.EqualTo("Umbrella"));
                Assert.That(upper.iKPass, Is.True);
                Assert.That(upper.avatarMask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.RightHandIK), Is.True);
                foreach (var part in new[] { AvatarMaskBodyPart.Root, AvatarMaskBodyPart.Body, AvatarMaskBodyPart.LeftLeg, AvatarMaskBodyPart.RightLeg, AvatarMaskBodyPart.LeftArm })
                    Assert.That(upper.avatarMask.GetHumanoidBodyPartActive(part), Is.False, part.ToString());
                animator.SetFloat("Speed", 1.25f);
                animator.SetFloat("GaitRate", 1);
                animator.Play("Locomotion", 0, .3f);
                animator.Update(0);
                var left = animator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
                var right = animator.GetBoneTransform(HumanBodyBones.RightFoot).position;
                var hips = animator.GetBoneTransform(HumanBodyBones.Hips).position;
                var hand = animator.GetBoneTransform(HumanBodyBones.RightHand).position;
                animator.SetLayerWeight(1, 1);
                animator.Play("UmbrellaHold", 1, 0);
                animator.Update(0);
                Assert.That(Vector3.Distance(left, animator.GetBoneTransform(HumanBodyBones.LeftFoot).position), Is.LessThan(.003f));
                Assert.That(Vector3.Distance(right, animator.GetBoneTransform(HumanBodyBones.RightFoot).position), Is.LessThan(.003f));
                Assert.That(Vector3.Distance(hips, animator.GetBoneTransform(HumanBodyBones.Hips).position), Is.LessThan(.003f));
                Assert.That(Vector3.Distance(hand, animator.GetBoneTransform(HumanBodyBones.RightHand).position), Is.GreaterThan(.08f));
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void UmbrellaDebouncesShelterAndResetsWithoutDuplicateProps()
        {
            var root = new GameObject("umbrella lifecycle test");
            try
            {
                root.AddComponent<CharacterController>();
                var character = root.AddComponent<SoraDemoCharacter>();
                character.InitializeAppearance(0);
                Assert.That(character.TrySetModel("Zhongli"), Is.True);
                var umbrella = root.GetComponentInChildren<SoraCharacterUmbrella>();
                Assert.That(umbrella.IsReady, Is.True);
                umbrella.AdvanceContext(.65f, false, true, .3f);
                umbrella.AdvanceContext(.65f, false, true, .6f);
                Assert.That(umbrella.WantsOpen && umbrella.OpenAmount > .99f, Is.True);
                umbrella.AdvanceContext(.65f, true, true, .08f);
                Assert.That(umbrella.WantsOpen, Is.True, "Door-edge flicker must not close the umbrella.");
                umbrella.AdvanceContext(.65f, false, true, .08f);
                umbrella.AdvanceContext(.65f, true, true, .3f);
                umbrella.AdvanceContext(.65f, true, true, .6f);
                Assert.That(umbrella.WantsOpen || umbrella.OpenAmount > .01f, Is.False);
                Assert.That(umbrella.Prop.gameObject.activeSelf, Is.True, "Folded umbrella stays in hand.");
                Assert.That(umbrella.Prop.GetComponentsInChildren<Collider>(), Is.Empty);
                var first = umbrella.Prop;
                Assert.That(character.TrySetModel("Zhongli"), Is.True);
                character.ResetMotion();
                Assert.That(root.GetComponentsInChildren<SoraCharacterUmbrella>().Length, Is.EqualTo(1));
                Assert.That(umbrella.Prop, Is.SameAs(first));
                Assert.That(umbrella.OpenAmount, Is.Zero);
                umbrella.AdvanceContext(.65f, false, true, 1);
                umbrella.enabled = false;
                float disabledLift = umbrella.Lift;
                umbrella.AdvanceContext(.65f, false, true, 1);
                Assert.That(umbrella.Lift, Is.EqualTo(disabledLift), "Disabled visuals must not advance; actual OnDisable reset is covered in Play Mode.");
                character.ResetMotion();
                Assert.That(root.GetComponentInChildren<Animator>().GetLayerWeight(1), Is.Zero, "Reset must not revive a disabled arm override.");
                Assert.That(root.GetComponentInChildren<Animator>().GetLayerWeight(2), Is.Zero);
                umbrella.enabled = true;
                Assert.That(character.TrySetModel("MaleRover"), Is.True);
                Assert.That(root.GetComponentsInChildren<SoraCharacterUmbrella>().Length, Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void ShelterProbeFindsNonDefaultLayerRoofAndIgnoresCharacters()
        {
            var rain = new GameObject("shelter test").AddComponent<SoraWeather>();
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                roof.layer = 8;
                roof.transform.position = new Vector3(0, 3, 0);
                roof.transform.localScale = new Vector3(3, .1f, 3);
                Physics.SyncTransforms();
                Assert.That(rain.IsSheltered(Vector3.up), Is.True);
                roof.SetActive(false);
                Physics.SyncTransforms();
                Assert.That(rain.IsSheltered(Vector3.up), Is.False);
            }
            finally { Object.DestroyImmediate(roof); Object.DestroyImmediate(rain.gameObject); }
        }

        [TestCase("StopLeft")]
        [TestCase("StopRight")]
        public void StopKeepsClosingFootMotionAndResumesOnIntent(string state)
        {
            var go = Object.Instantiate(Resources.Load<GameObject>(SoraCharacterModels.ResourceFolder + "Zhongli"));
            try
            {
                var animator = go.GetComponent<Animator>();
                animator.SetBool("IsGrounded", true);
                animator.Play(state, 0, .05f);
                animator.Update(0);
                var foot = animator.GetBoneTransform(state == "StopLeft" ? HumanBodyBones.RightFoot : HumanBodyBones.LeftFoot);
                Vector3 first = foot.position;
                for (int i = 0; i < 10; i++) animator.Update(.02f);
                Assert.That(Vector3.Distance(first, foot.position), Is.GreaterThan(.015f), "Closing foot must keep moving after root speed reaches zero.");
                animator.SetBool("Moving", true);
                for (int i = 0; i < 5; i++) animator.Update(.02f);
                Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion"), Is.True);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [TestCase("MoveStart")]
        [TestCase("StopLeft")]
        [TestCase("StopRight")]
        public void MenuInterruptsGroundedMotionDuringCrossfade(string state)
        {
            var go = Object.Instantiate(Resources.Load<GameObject>(SoraCharacterModels.ResourceFolder + "Zhongli"));
            try
            {
                var animator = go.GetComponent<Animator>();
                animator.SetBool("IsGrounded", true);
                animator.SetBool("Moving", state == "MoveStart");
                animator.Play("Locomotion", 0, .2f); animator.Update(0);
                animator.CrossFadeInFixedTime(state, .15f); animator.Update(.02f);
                animator.SetBool("MotionBlocked", true);
                for (int i = 0; i < 15; i++) animator.Update(.02f);
                Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion"), Is.True);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [TestCase("Chisa")]
        [TestCase("Citlali")]
        [TestCase("Denia")]
        [TestCase("Lynae")]
        [TestCase("Zhongli")]
        public void ExistingFingerChainsCloseWithNativeGripLayer(string model)
        {
            var go = Object.Instantiate(Resources.Load<GameObject>(SoraCharacterModels.ResourceFolder + model));
            try
            {
                var animator = go.GetComponent<Animator>();
                var distal = animator.GetBoneTransform(HumanBodyBones.RightIndexDistal);
                Assert.That(distal, Is.Not.Null, model + " existing fingers must be mapped");
                animator.Play("Locomotion", 0, 0); animator.Update(0);
                var first = distal.position;
                var foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
                animator.SetLayerWeight(2, 1);
                animator.Play("UmbrellaGrip", 2, 0); animator.Update(0);
                Assert.That(Vector3.Distance(first, distal.position), Is.GreaterThan(.003f), model + " finger muscle must close the hand");
                Assert.That(Vector3.Distance(foot, animator.GetBoneTransform(HumanBodyBones.LeftFoot).position), Is.LessThan(.003f), "Finger mask must preserve leg pose.");
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
