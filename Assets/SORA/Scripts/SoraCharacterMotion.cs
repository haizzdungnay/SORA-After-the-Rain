using UnityEngine;

namespace Sora.AfterTheRain.Demo
{
    [RequireComponent(typeof(Animator))]
    public sealed class SoraCharacterMotion : MonoBehaviour
    {
        private static readonly int Speed = Animator.StringToHash("Speed"), Grounded = Animator.StringToHash("IsGrounded"),
            Vertical = Animator.StringToHash("VerticalSpeed"), Jump = Animator.StringToHash("Jump"), Turn = Animator.StringToHash("Turn"),
            Idle = Animator.StringToHash("IdleAction"), Variant = Animator.StringToHash("IdleVariant"), GaitRate = Animator.StringToHash("GaitRate");
        private static readonly RaycastHit[] hits = new RaycastHit[16];
        private Animator animator;
        private SoraDemoCharacter owner;
        private SoraWetSurfaces wetSurfaces;
        private float speed, turn, stillTime, nextIdle = 8, airborneTime, lastStepAt = -10, pelvisOffset;
        private bool grounded, ready;
        private readonly Vector3[] lockedFeet = new Vector3[2];
        private readonly bool[] planted = new bool[2];
        private readonly Vector3[] footTargets = new Vector3[2], groundNormals = new Vector3[2];
        private readonly float[] footWeights = new float[2];
        public int FootstepCount { get; private set; }
        public int IdleCount { get; private set; }
        public bool GroundedNow => grounded;
        public bool IsReady => ready;

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
            if (!ready) return;
            animator.ResetTrigger(Jump);
            animator.SetFloat(Speed, 0);
            animator.SetFloat(Turn, 0);
        }

        public void Tick(float horizontalSpeed, bool isGrounded, float verticalSpeed, bool jumped, float turnRate,
            bool activeInput, float deltaTime)
        {
            if (!ready) return;
            speed = horizontalSpeed;
            turn = turnRate;
            bool landed = !grounded && isGrounded && airborneTime > .15f;
            grounded = isGrounded;
            airborneTime = grounded ? 0 : airborneTime + deltaTime;
            animator.SetFloat(Speed, speed, .05f, deltaTime);
            animator.SetBool(Grounded, grounded);
            animator.SetFloat(Vertical, verticalSpeed);
            animator.SetFloat(Turn, turn);
            if (jumped) { animator.ResetTrigger(Jump); animator.SetTrigger(Jump); }
            if (activeInput || !grounded || speed > .08f || Mathf.Abs(turn) > 5)
                CancelIdle();
            else if ((stillTime += deltaTime) >= nextIdle && animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion"))
            {
                stillTime = 0;
                nextIdle = Random.Range(9f, 15f);
                animator.SetInteger(Variant, Random.Range(1, 3));
                animator.SetTrigger(Idle);
                IdleCount++;
            }
            if (landed) Contact(-1, 1.25f);
            if (!grounded) planted[0] = planted[1] = false;
        }

        // Imported clips send one event per planted foot; blended clips are filtered to avoid double hits.
        public void Footstep(AnimationEvent contact)
        {
            if (!ready || !grounded || speed < .1f || contact.animatorClipInfo.weight < .35f) return;
            Contact(contact.intParameter, Mathf.Lerp(.65f, 1, speed / 5));
        }

        private void Contact(int foot, float volume)
        {
            if (owner == null || Time.time - lastStepAt < .12f) return;
            lastStepAt = Time.time;
            if (wetSurfaces == null) wetSurfaces = FindFirstObjectByType<SoraWetSurfaces>();
            var bone = foot == 0 ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot;
            Vector3 position = foot < 0 ? owner.transform.position : animator.GetBoneTransform(bone).position;
            wetSurfaces?.PlayFootstep(owner, position, volume);
            FootstepCount++;
        }

        private void OnAnimatorIK(int layer)
        {
            if (!ready || owner == null || layer != 0) return;
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
            if (!grounded)
            {
                pelvisOffset = Mathf.MoveTowards(pelvisOffset, 0, Time.deltaTime * 2);
                animator.bodyPosition += Vector3.up * pelvisOffset;
                return;
            }
            float lowerBody = 0;
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
                float weight = speed < .1f ? 1 : Mathf.InverseLerp(.14f, .025f, clearance);
                if (speed < .1f && Mathf.Abs(turn) < 45) lowerBody = Mathf.Min(lowerBody, -clearance);
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
            animator.bodyPosition += Vector3.up * pelvisOffset;
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
    }
}
