using UnityEngine;

namespace Sora.AfterTheRain.Demo
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class SoraDemoCharacter : MonoBehaviour
    {
        public string DisplayName;
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private CharacterController controller;
        private Animator animator;
        private bool isPlayer;
        private Vector3 aiTarget;
        private float aiRetargetAt;
        private Vector3 fallVelocity;
        private int twinIndex;
        private GameObject[] defaultVisuals;
        private GameObject selectedVisual;
        private float defaultHeight;
        public string ModelId { get; private set; }
        public float Height => controller != null ? controller.height : 1.72f;

        private static readonly Vector3[] WorkPoints =
        {
            new Vector3(-3.4f, 0, 1.6f), new Vector3(4.7f, 0, 2.3f),
            new Vector3(1.2f, 0, -0.7f), new Vector3(-1.8f, 0, -2.2f)
        };

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponentInChildren<Animator>(); // rigged models only; blockouts have none
        }
        public void SetPlayerControlled(bool enabled) { isPlayer = enabled; aiRetargetAt = 0; }

        public void InitializeAppearance(int index)
        {
            controller = GetComponent<CharacterController>();
            twinIndex = index;
            defaultHeight = controller.height;
            defaultVisuals = new GameObject[transform.childCount];
            for (int i = 0; i < defaultVisuals.Length; i++) defaultVisuals[i] = transform.GetChild(i).gameObject;
        }

        public bool TrySetModel(string id)
        {
            if (defaultVisuals == null || !SoraCharacterModels.IsAllowed(twinIndex, id)) return false;
            if (ModelId == id) return true;
            var definition = SoraCharacterModels.Find(id);
            var prefab = definition.Prefab;
            if (prefab == null) return false;
            var visual = Instantiate(prefab, transform, false);
            var nextAnimator = visual.GetComponentInChildren<Animator>();
            if (nextAnimator == null || nextAnimator.avatar == null || !nextAnimator.avatar.isHuman || nextAnimator.runtimeAnimatorController == null)
            {
                ReleaseVisual(visual);
                return false;
            }
            ReleaseVisual(selectedVisual);
            foreach (var child in defaultVisuals) child.SetActive(false);
            selectedVisual = visual;
            animator = nextAnimator;
            animator.applyRootMotion = false;
            controller.height = definition.Height;
            controller.center = Vector3.up * (definition.Height * 0.5f);
            ModelId = id;
            return true;
        }

        public void RestoreDefaultAppearance()
        {
            if (defaultVisuals == null) return;
            ReleaseVisual(selectedVisual);
            selectedVisual = null;
            foreach (var child in defaultVisuals) child.SetActive(true);
            animator = GetComponentInChildren<Animator>();
            controller.height = defaultHeight;
            controller.center = Vector3.up * (defaultHeight * 0.5f);
            ModelId = null;
        }

        private static void ReleaseVisual(GameObject visual)
        {
            if (visual == null) return;
            visual.SetActive(false);
            if (Application.isPlaying) Destroy(visual); else DestroyImmediate(visual);
        }

        private void Update()
        {
            if (controller == null || SoraDemoCore.Instance == null) return;
            Vector3 intended = Vector3.zero;
            float speed = 0f;
            if (SoraDemoCore.Instance.IsChoosingModels) { aiRetargetAt = 0; }
            else if (isPlayer)
            {
                float x = Input.GetAxisRaw("Horizontal");
                float z = Input.GetAxisRaw("Vertical");
                Camera cam = Camera.main;
                Vector3 forward = cam != null ? cam.transform.forward : Vector3.forward;
                Vector3 right = cam != null ? cam.transform.right : Vector3.right;
                forward.y = 0; right.y = 0;
                forward.Normalize(); right.Normalize();
                intended = (right * x + forward * z).normalized;
                speed = Input.GetKey(KeyCode.LeftShift) ? 5.0f : 3.05f;
            }
            else
            {
                if (Time.time >= aiRetargetAt || Vector3.Distance(transform.position, aiTarget) < 0.35f)
                {
                    aiTarget = WorkPoints[Random.Range(0, WorkPoints.Length)];
                    aiRetargetAt = Time.time + Random.Range(3f, 8f);
                }
                Vector3 delta = aiTarget - transform.position;
                delta.y = 0;
                intended = delta.sqrMagnitude > 0.15f ? delta.normalized : Vector3.zero;
                speed = 1.25f;
            }
            if (intended.sqrMagnitude > 0.05f)
            {
                Quaternion desired = Quaternion.LookRotation(intended, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desired, Time.deltaTime * 10f);
            }
            if (controller.isGrounded && fallVelocity.y < 0) fallVelocity.y = -1.2f;
            fallVelocity.y += Physics.gravity.y * Time.deltaTime;
            controller.Move((intended * speed + fallVelocity) * Time.deltaTime);
            if (animator != null)
            {
                Vector3 moved = controller.velocity; // actual motion: no running on the spot against a wall
                moved.y = 0;
                animator.SetFloat(SpeedHash, moved.magnitude, 0.1f, Time.deltaTime);
            }
        }
    }

    public sealed class SoraThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 3.6f;
        public float defaultPitch = 15f;
        public float recenterSpeed = 5.0f;
        private float yaw = 0f;
        private float pitch = 15f;
        private Vector3 velocity;

        public void SetTarget(Transform focus)
        {
            target = focus;
            if (focus != null)
            {
                yaw = focus.eulerAngles.y;
                pitch = defaultPitch;
            }
            velocity = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Instant snap camera behind character with Middle Click or C key
            bool choosingModels = SoraDemoCore.Instance != null && SoraDemoCore.Instance.IsChoosingModels;
            if (!choosingModels && (Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.C)))
            {
                yaw = target.eulerAngles.y;
                pitch = defaultPitch;
            }

            if (!choosingModels && Input.GetMouseButton(1))
            {
                // Free look while holding Right Mouse Button
                yaw += Input.GetAxis("Mouse X") * 3.1f;
                pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * 2.3f, 5f, 65f);
            }
            else
            {
                // Smoothly return behind character when Right Mouse Button is released
                float targetYaw = target.eulerAngles.y;
                yaw = Mathf.LerpAngle(yaw, targetYaw, Time.deltaTime * recenterSpeed);
                pitch = Mathf.Lerp(pitch, defaultPitch, Time.deltaTime * recenterSpeed);
            }

            if (!choosingModels) distance = Mathf.Clamp(distance - Input.mouseScrollDelta.y * 0.5f, 2.0f, 6.5f);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            var character = target.GetComponent<SoraDemoCharacter>();
            Vector3 focus = target.position + Vector3.up * (character != null ? character.Height * 0.82f : 1.45f);
            Vector3 wanted = focus + rotation * new Vector3(0f, 0f, -distance);

            // Prevent camera from clipping through storefront walls and shelving
            Vector3 towards = wanted - focus;
            if (Physics.SphereCast(focus, 0.18f, towards.normalized, out RaycastHit hit, towards.magnitude, ~0, QueryTriggerInteraction.Ignore))
            {
                if (!hit.transform.IsChildOf(target)) wanted = focus + towards.normalized * Mathf.Max(0.7f, hit.distance - 0.20f);
            }
            transform.position = Vector3.SmoothDamp(transform.position, wanted, ref velocity, 0.08f);
            transform.rotation = Quaternion.LookRotation(focus - transform.position, Vector3.up);
        }
    }
}
