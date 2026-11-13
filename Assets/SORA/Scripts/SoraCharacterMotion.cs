using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Sora.AfterTheRain.Demo
{
    [RequireComponent(typeof(Animator))]
    public sealed class SoraCharacterMotion : MonoBehaviour
    {
        private static readonly int Speed = Animator.StringToHash("Speed"), Grounded = Animator.StringToHash("IsGrounded"),
            Vertical = Animator.StringToHash("VerticalSpeed"), Jump = Animator.StringToHash("Jump"), Turn = Animator.StringToHash("Turn"),
            Idle = Animator.StringToHash("IdleAction"), Variant = Animator.StringToHash("IdleVariant"), GaitRate = Animator.StringToHash("GaitRate");
        private static readonly RaycastHit[] hits = new RaycastHit[16];
        // Anonymous Chisa roots verified against weighted hair vertices in Blender, not inferred from screenshots.
        private static readonly string[] ChisaHairRoots = { "n214", "n220", "n241", "n298", "n321", "n329", "n356" };
        private Animator animator;
        private SoraDemoCharacter owner;
        private SoraWetSurfaces wetSurfaces;
        private SoraCharacterUmbrella umbrella;
        private float speed, turn, stillTime, nextIdle = 8, airborneTime, lastStepAt = -10, pelvisOffset, balanceShift;
        private bool grounded, ready;
        private bool hadMoveIntent;
        private int lastFoot;
        private Vector2 lean, secondarySway;
        private Transform[] secondary = Array.Empty<Transform>();
        private Quaternion[] secondaryRest = Array.Empty<Quaternion>();
        private readonly Vector3[] lockedFeet = new Vector3[2];
        private readonly bool[] planted = new bool[2];
        private readonly Vector3[] footTargets = new Vector3[2], groundNormals = new Vector3[2];
        private readonly float[] footWeights = new float[2];
        public int FootstepCount { get; private set; }
        public Vector3 LastFootstepPosition { get; private set; }
        public bool LastFootstepPhysicalGrounded { get; private set; }
        public int IdleCount { get; private set; }
        public bool GroundedNow => grounded;
        public bool IsReady => ready;
        public int SecondaryBoneCount => secondary.Length;

        public void Initialize(SoraDemoCharacter character)
        {
            owner = character;
            animator = GetComponent<Animator>();
            ready = animator.isHuman && animator.runtimeAnimatorController != null;
            if (!ready) return;
            foreach (var parameter in animator.parameters)
                if (parameter.nameHash == GaitRate)
                {
                    var source = Resources.Load<GameObject>(SoraDemoWorld.AkikoModelResource);
                    var reference = source != null ? source.GetComponent<Animator>() : null;
                    animator.SetFloat(GaitRate, reference != null ? reference.humanScale / animator.humanScale : 1);
                    umbrella = GetComponent<SoraCharacterUmbrella>() ?? gameObject.AddComponent<SoraCharacterUmbrella>();
                    umbrella.Initialize(owner, animator);
                    CacheSecondaryBones();
                    ResetMotion();
                    return;
                }
            ready = false;
            Debug.LogWarning("SORA: rebuild character motion with SORA/Characters/5. Upgrade Motion Library.", this);
        }

        public void CancelIdle()
        {
            stillTime = 0;
            if (ready) { animator.ResetTrigger(Idle); animator.SetInteger(Variant, 0); }
        }

        public void ResetMotion()
        {
            CancelIdle();
            planted[0] = planted[1] = false;
            airborneTime = 0;
            pelvisOffset = 0;
            balanceShift = 0;
            hadMoveIntent = false;
            speed = turn = 0;
            lean = secondarySway = Vector2.zero;
            RestoreSecondary();
            umbrella?.ResetMotion();
            if (!ready) return;
            animator.ResetTrigger(Jump);
            animator.SetFloat(Speed, 0);
            animator.SetFloat(Turn, 0);
            animator.ResetTrigger("MoveStart");
            animator.ResetTrigger("MoveStop");
            animator.SetBool("Moving", false);
            animator.SetBool("MotionBlocked", false);
            animator.Play("Locomotion", 0, 0);
        }

        public void Tick(float horizontalSpeed, bool isGrounded, float verticalSpeed, bool jumped, float turnRate,
            bool activeInput, float deltaTime, float steeringRate = 0, bool moveIntent = false, bool blocked = false)
        {
            if (!ready) return;
            float previousSpeed = speed;
            speed = horizontalSpeed;
            turn = turnRate;
            bool landed = !grounded && isGrounded && airborneTime > .15f;
            grounded = isGrounded;
            airborneTime = grounded ? 0 : airborneTime + deltaTime;
            animator.SetFloat(Speed, speed, .05f, deltaTime);
            animator.SetBool(Grounded, grounded);
            animator.SetFloat(Vertical, verticalSpeed);
            animator.SetFloat(Turn, turn);
            animator.SetBool("Moving", moveIntent);
            animator.SetBool("MotionBlocked", blocked);
            // Never let a trigger queued during a crossfade outlive the intent that created it.
            if (moveIntent) animator.ResetTrigger("MoveStop"); else animator.ResetTrigger("MoveStart");
            if (jumped || blocked || !grounded)
            {
                animator.ResetTrigger("MoveStart");
                animator.ResetTrigger("MoveStop");
            }
            else if (!moveIntent && hadMoveIntent && previousSpeed > .25f)
            {
                animator.ResetTrigger("MoveStart");
                animator.SetInteger("StopFoot", lastFoot);
                animator.SetTrigger("MoveStop");
            }
            else if (moveIntent && !hadMoveIntent && previousSpeed < .15f)
            {
                animator.ResetTrigger("MoveStop");
                animator.SetTrigger("MoveStart");
            }
            hadMoveIntent = moveIntent;
            float settle = 1 - Mathf.Exp(-10 * deltaTime);
            float acceleration = (speed - previousSpeed) / Mathf.Max(.01f, deltaTime);
            lean = Vector2.Lerp(lean, grounded ? new Vector2(Mathf.Clamp(-acceleration * .16f, -3, 3),
                Mathf.Clamp(-steeringRate * .018f, -5, 5) * Mathf.Clamp01(speed)) : Vector2.zero, settle);
            secondarySway = Vector2.Lerp(secondarySway, new Vector2(Mathf.Clamp(acceleration * .25f, -4, 4),
                Mathf.Clamp(steeringRate * .025f, -6, 6)), settle * .55f);
            umbrella?.Tick(grounded, blocked, deltaTime);
            if (jumped) { animator.ResetTrigger(Jump); animator.SetTrigger(Jump); }
            if (activeInput || !grounded || speed > .08f || Mathf.Abs(turn) > 5)
                CancelIdle();
            else if ((stillTime += deltaTime) >= nextIdle && animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion"))
            {
                stillTime = 0;
                nextIdle = Random.Range(9f, 15f);
                animator.SetInteger(Variant, umbrella != null && umbrella.IsReady ? 1 : Random.Range(1, 3));
                animator.SetTrigger(Idle);
                umbrella?.AdjustGrip();
                IdleCount++;
            }
            if (landed) Contact(-1, 1.25f);
            if (!grounded) planted[0] = planted[1] = false;
        }

        // Imported clips send one event per planted foot; blended clips are filtered to avoid double hits.
        public void Footstep(AnimationEvent contact)
        {
            bool closingStep = contact.animatorStateInfo.IsName("StopLeft") || contact.animatorStateInfo.IsName("StopRight");
            if (!ready || !grounded || speed < .1f && !closingStep || contact.animatorClipInfo.weight < .35f) return;
            lastFoot = contact.intParameter;
            Contact(contact.intParameter, Mathf.Lerp(.65f, 1, speed / 5));
        }

        private void Contact(int foot, float volume)
        {
            if (owner == null || Time.time - lastStepAt < .12f) return;
            lastStepAt = Time.time;
            if (wetSurfaces == null) wetSurfaces = FindFirstObjectByType<SoraWetSurfaces>();
            var bone = foot == 0 ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot;
            Vector3 position = foot < 0 ? owner.transform.position : animator.GetBoneTransform(bone).position;
            LastFootstepPosition = position;
            LastFootstepPhysicalGrounded = owner.GetComponent<CharacterController>().isGrounded;
            wetSurfaces?.PlayFootstep(owner, position, volume);
            FootstepCount++;
        }

        private void OnAnimatorIK(int layer)
        {
            if (!ready || owner == null) return;
            if (layer == 1) { umbrella?.ApplyHandIK(); return; }
            if (layer != 0) return;
            var chest = animator.GetBoneTransform(HumanBodyBones.Chest);
            if (chest != null)
            {
                var offset = Quaternion.AngleAxis(lean.x, owner.transform.right) * Quaternion.AngleAxis(lean.y, owner.transform.forward);
                animator.SetBoneLocalRotation(HumanBodyBones.Chest,
                    Quaternion.Inverse(chest.parent.rotation) * offset * chest.rotation);
            }
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
            if (!grounded)
            {
                balanceShift = 0;
                pelvisOffset = Mathf.MoveTowards(pelvisOffset, 0, Time.deltaTime * 2);
                animator.bodyPosition += Vector3.up * pelvisOffset;
                return;
            }
            float lowerBody = 0;
            var state = animator.GetCurrentAnimatorStateInfo(0);
            var next = animator.GetNextAnimatorStateInfo(0);
            bool settling = state.IsName("MoveStart") || state.IsName("StopLeft") || state.IsName("StopRight") ||
                animator.IsInTransition(0) && (next.IsName("MoveStart") || next.IsName("StopLeft") || next.IsName("StopRight"));
            for (int i = 0; i < 2; i++)
            {
                footWeights[i] = 0;
                var goal = i == 0 ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;
                Vector3 foot = animator.GetIKPosition(goal);
                float sole = i == 0 ? animator.leftFeetBottomHeight : animator.rightFeetBottomHeight;
                int count = Physics.RaycastNonAlloc(foot + Vector3.up * .35f, Vector3.down, hits, .75f, ~0, QueryTriggerInteraction.Ignore);
                float nearest = float.MaxValue;
                RaycastHit ground = default;
                if (count == hits.Length) { planted[i] = false; continue; }
                for (int h = 0; h < count; h++)
                    if (!(hits[h].collider is CharacterController) && hits[h].normal.y > .65f && hits[h].distance < nearest)
                    { ground = hits[h]; nearest = ground.distance; }
                if (ground.collider == null) { planted[i] = false; continue; }
                Vector3 target = ground.point + ground.normal * sole;
                float clearance = foot.y - target.y;
                float weight = speed < .1f && !settling ? 1 : Mathf.InverseLerp(.14f, .025f, clearance);
                if (speed < .1f && !settling && Mathf.Abs(turn) < 45) lowerBody = Mathf.Min(lowerBody, -clearance);
                bool stance = weight > .8f && Mathf.Abs(turn) < 45;
                if (stance && (!planted[i] || Vector3.Distance(lockedFeet[i], target) > .3f * owner.Height / 1.65f))
                    lockedFeet[i] = target;
                planted[i] = stance;
                if (stance) { target.x = lockedFeet[i].x; target.z = lockedFeet[i].z; }
                target.y = Mathf.Clamp(target.y, foot.y - .24f, foot.y + .2f);
                footTargets[i] = target;
                groundNormals[i] = ground.normal;
                footWeights[i] = weight;
            }
            // Lower only the visual pelvis on split-height support; collision and actor root stay unchanged.
            pelvisOffset = Mathf.MoveTowards(pelvisOffset, Mathf.Clamp(lowerBody, -.22f, 0), Time.deltaTime * 1.5f);
            float balance = speed < .08f && !settling && Mathf.Abs(turn) < 5 ? Mathf.Sin(Time.time * .8f) * .01f * owner.Height : 0;
            balanceShift = Mathf.MoveTowards(balanceShift, balance, Time.deltaTime * .08f);
            animator.bodyPosition += Vector3.up * pelvisOffset + owner.transform.right * balanceShift;
            for (int i = 0; i < 2; i++)
            {
                var goal = i == 0 ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;
                float weight = footWeights[i];
                animator.SetIKPositionWeight(goal, weight);
                animator.SetIKRotationWeight(goal, weight);
                if (weight <= 0) continue;
                animator.SetIKPosition(goal, footTargets[i]);
                animator.SetIKRotation(goal, Quaternion.FromToRotation(Vector3.up, groundNormals[i]) * animator.GetIKRotation(goal));
            }
        }

        private void CacheSecondaryBones()
        {
            RestoreSecondary();
            var human = new HashSet<Transform>();
            for (int i = 0; i < (int)HumanBodyBones.LastBone; i++) human.Add(animator.GetBoneTransform((HumanBodyBones)i));
            bool Garment(Transform bone) => bone != null && (bone.name.IndexOf("hair", StringComparison.OrdinalIgnoreCase) >= 0 ||
                bone.name.IndexOf("skirt", StringComparison.OrdinalIgnoreCase) >= 0 || bone.name.IndexOf("coat", StringComparison.OrdinalIgnoreCase) >= 0 ||
                owner.ModelId == "Chisa" && Array.IndexOf(ChisaHairRoots, bone.name) >= 0);
            var bones = new List<Transform>();
            var skinnedBones = new HashSet<Transform>();
            foreach (var skin in GetComponentsInChildren<SkinnedMeshRenderer>()) foreach (var bone in skin.bones) if (bone != null) skinnedBones.Add(bone);
            foreach (var bone in skinnedBones)
                if (Garment(bone) && !Garment(bone.parent) && !human.Contains(bone) && bone.childCount > 0 &&
                    bone.GetComponent<Renderer>() == null && bones.Count < 32) bones.Add(bone);
            secondary = bones.ToArray();
            secondaryRest = Array.ConvertAll(secondary, bone => bone.localRotation);
        }

        private void RestoreSecondary()
        {
            for (int i = 0; i < secondary.Length; i++) if (secondary[i] != null) secondary[i].localRotation = secondaryRest[i];
        }

        private void Update() => RestoreSecondary();

        private void LateUpdate()
        {
            if (!ready || owner == null) return;
            // ponytail: existing dedicated garment roots only; authored per-rig constraints replace this bounded sway after art QA.
            var offset = Quaternion.AngleAxis(secondarySway.x, owner.transform.right) * Quaternion.AngleAxis(secondarySway.y, owner.transform.forward);
            foreach (var bone in secondary) if (bone != null) bone.rotation = offset * bone.rotation;
        }

        private void OnDisable() { RestoreSecondary(); if (umbrella != null) umbrella.enabled = false; }
        private void OnEnable() { if (umbrella != null) umbrella.enabled = true; }
    }
}
