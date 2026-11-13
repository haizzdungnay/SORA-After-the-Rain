using UnityEngine;

namespace Sora.AfterTheRain.Demo
{
    [DefaultExecutionOrder(50)]
    public sealed class SoraCharacterUmbrella : MonoBehaviour
    {
        private Animator animator;
        private SoraDemoCharacter owner;
        private SoraWeather weather;
        private GameObject prop;
        private SkinnedMeshRenderer canopy;
        private Transform hand, middle;
        private int layer = -1;
        private int gripLayer = -1;
        private float candidateTime, probeAt, weatherRetryAt;
        private bool rain, candidate, sheltered, grounded;
        public bool IsReady => layer >= 0 && prop != null;
        public bool WantsOpen { get; private set; }
        public float Lift { get; private set; }
        public float OpenAmount => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.45f, .95f, Lift));
        public Transform Prop => prop != null ? prop.transform : null;
        public Vector3 GripPosition => owner.transform.TransformPoint(Vector3.Lerp(
            new Vector3(.17f, .59f, .025f), new Vector3(.17f, .76f, .10f), Mathf.SmoothStep(0, 1, Lift)) * owner.Height);

        public void Initialize(SoraDemoCharacter character, Animator rig)
        {
            owner = character; animator = rig;
            layer = animator.GetLayerIndex("Umbrella");
            gripLayer = animator.GetLayerIndex("UmbrellaGrip");
            if (layer < 0) return;
            hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            middle = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
            if (prop == null)
            {
                var source = Resources.Load<GameObject>("Weather/SoraUmbrella");
                if (source == null) { layer = -1; return; }
                prop = Instantiate(source, transform, false);
                canopy = prop.GetComponentInChildren<SkinnedMeshRenderer>();
                foreach (var child in prop.GetComponentsInChildren<Transform>()) child.gameObject.layer = gameObject.layer;
            }
            prop.transform.localScale = Vector3.one * (owner.Height / 1.65f);
            ResetMotion();
        }

        public void ResetMotion()
        {
            WantsOpen = candidate = rain = false;
            Lift = candidateTime = probeAt = 0;
            if (!IsReady) return;
            animator.SetBool("UmbrellaOpen", false);
            animator.ResetTrigger("UmbrellaAdjust");
            animator.SetBool("UmbrellaBusy", false);
            animator.Play("UmbrellaCarry", layer, 0);
            animator.SetLayerWeight(layer, isActiveAndEnabled ? 1 : 0);
            if (gripLayer >= 0) animator.SetLayerWeight(gripLayer, isActiveAndEnabled ? 1 : 0);
            prop.SetActive(isActiveAndEnabled);
            PoseProp();
        }

        public void Tick(bool isGrounded, bool blocked, float deltaTime)
        {
            if (!IsReady || !isActiveAndEnabled) return;
            if (weather == null && Time.time >= weatherRetryAt)
            {
                weather = FindFirstObjectByType<SoraWeather>();
                weatherRetryAt = Time.time + 1;
            }
            if (weather != null && Time.time >= probeAt)
            {
                sheltered = weather.IsSheltered(owner.transform.position + Vector3.up * owner.Height * .9f);
                probeAt = Time.time + .08f;
            }
            AdvanceContext(weather != null ? weather.State.rain : 0, sheltered, isGrounded, deltaTime);
            animator.SetBool("UmbrellaBusy", blocked || !isGrounded || animator.GetFloat("Speed") > .08f);
            if (animator.GetBool("UmbrellaBusy") || !WantsOpen) animator.ResetTrigger("UmbrellaAdjust");
        }

        // Per-character roof state: the inactive twin must not borrow the active twin's shelter value.
        public void AdvanceContext(float rainfall, bool underRoof, bool isGrounded, float deltaTime)
        {
            if (!IsReady || !isActiveAndEnabled || !float.IsFinite(rainfall) || !float.IsFinite(deltaTime) || deltaTime <= 0) return;
            grounded = isGrounded;
            rain = rainfall >= (rain ? .04f : .12f);
            bool next = rain && !underRoof;
            if (candidate != next) { candidate = next; candidateTime = 0; }
            candidateTime += deltaTime;
            if (candidateTime >= .22f) WantsOpen = candidate;
            animator.SetBool("UmbrellaOpen", WantsOpen);
            // Keep the prop secure during jumps; the base jump/fall/land states retain control of both legs.
            Lift = Mathf.MoveTowards(Lift, WantsOpen ? 1 : 0, deltaTime / .53f);
        }

        public void AdjustGrip()
        {
            if (IsReady && isActiveAndEnabled && WantsOpen && grounded && !animator.GetBool("UmbrellaBusy")) animator.SetTrigger("UmbrellaAdjust");
        }

        public void ApplyHandIK()
        {
            if (!IsReady || !isActiveAndEnabled) return;
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKPosition(AvatarIKGoal.RightHand, GripPosition);
            animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, .6f);
            animator.SetIKHintPosition(AvatarIKHint.RightElbow, owner.transform.TransformPoint(new Vector3(.25f, .66f, -.02f) * owner.Height));
        }

        private void LateUpdate() { if (IsReady) PoseProp(); }

        private void PoseProp()
        {
            prop.transform.position = hand != null && middle != null ? Vector3.Lerp(hand.position, middle.position, .55f) : GripPosition;
            // ponytail: bounded visual sway, not wind/cloth simulation; add authored rig physics after art QA.
            float sway = Mathf.Sin(Time.time * 1.7f) * 1.2f * OpenAmount;
            prop.transform.rotation = owner.transform.rotation * Quaternion.Euler(180 * (1 - Mathf.SmoothStep(0, 1, Lift)) + sway, 0, 0);
            canopy.SetBlendShapeWeight(0, OpenAmount * 100);
        }

        private void OnEnable()
        {
            if (IsReady) ResetMotion();
        }
        private void OnDisable()
        {
            WantsOpen = candidate = rain = false;
            Lift = candidateTime = probeAt = 0;
            if (prop != null) prop.SetActive(false);
            if (animator != null && layer >= 0 && animator.isInitialized)
            {
                animator.SetBool("UmbrellaOpen", false);
                animator.ResetTrigger("UmbrellaAdjust");
                animator.SetLayerWeight(layer, 0);
                if (gripLayer >= 0) animator.SetLayerWeight(gripLayer, 0);
            }
        }
    }
}
