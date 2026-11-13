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
    }
}
