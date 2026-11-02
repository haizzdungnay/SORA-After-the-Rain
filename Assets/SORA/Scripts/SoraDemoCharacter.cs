using UnityEngine;

namespace Sora.AfterTheRain.Demo
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class SoraDemoCharacter : MonoBehaviour
    {
        public string DisplayName;
        private CharacterController controller;
        private bool isPlayer;
        private Vector3 aiTarget;
        private float aiRetargetAt;
        private Vector3 fallVelocity;

        private static readonly Vector3[] WorkPoints =
        {
            new Vector3(-3.4f, 0, 1.6f), new Vector3(4.7f, 0, 2.3f),
            new Vector3(1.2f, 0, -0.7f), new Vector3(-1.8f, 0, -2.2f)
        };

        private void Awake() { controller = GetComponent<CharacterController>(); }
        public void SetPlayerControlled(bool enabled) { isPlayer = enabled; aiRetargetAt = 0; }

        private void Update()
        {
            if (controller == null || SoraDemoCore.Instance == null) return;
            Vector3 intended = Vector3.zero;
            float speed = 0f;
            if (isPlayer)
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
        }
    }

    public sealed class SoraThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 6.5f;
        private float yaw = 0f;
        private float pitch = 20f;
        private Vector3 velocity;

        public void SetTarget(Transform focus)
        {
            target = focus;
            velocity = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (target == null) return;
            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * 3.1f;
                pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * 2.3f, 5f, 65f);
            }
            distance = Mathf.Clamp(distance - Input.mouseScrollDelta.y * 0.6f, 3.5f, 9f);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 focus = target.position + Vector3.up * 1.45f;
            Vector3 wanted = focus + rotation * new Vector3(0f, 0f, -distance);
            // Prevent the camera from clipping through storefront walls and shelving.
            Vector3 towards = wanted - focus;
            if (Physics.SphereCast(focus, 0.18f, towards.normalized, out RaycastHit hit, towards.magnitude, ~0, QueryTriggerInteraction.Ignore))
            {
                if (!hit.transform.IsChildOf(target)) wanted = focus + towards.normalized * Mathf.Max(0.7f, hit.distance - 0.20f);
            }
            transform.position = Vector3.SmoothDamp(transform.position, wanted, ref velocity, 0.09f);
            transform.rotation = Quaternion.LookRotation(focus - transform.position, Vector3.up);
        }
    }
}
