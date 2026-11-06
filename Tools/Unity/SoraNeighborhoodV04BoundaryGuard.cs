// Optional defence in depth for SORA V04 gameplay integration. Attach it ONLY
// when the existing two character transforms are bound and integration is tested.
// Physics colliders in the V04 staging scene remain the primary containment.
// Do not add a second SoraDemoCore, POS, save handler or world-spawn routine.
using System.Collections.Generic;
using UnityEngine;

namespace Sora.Demo
{
    public sealed class SoraNeighborhoodV04BoundaryGuard : MonoBehaviour
    {
        [SerializeField] private Transform[] protectedCharacters;
        [SerializeField] private float minX = -62f;
        [SerializeField] private float maxX = 107f;
        [SerializeField] private float minZ = -62f;
        [SerializeField] private float maxZ = 107f;
        [SerializeField] private float minimumY = -4f;
        [SerializeField] private Vector3 fallbackPosition = new Vector3(17f, 1f, 0f);
        private readonly Dictionary<Transform,Vector3> lastSafe = new Dictionary<Transform,Vector3>();
        private readonly HashSet<Transform> rescuedThisFrame = new HashSet<Transform>();

        private bool Inside(Vector3 p)
        {
            return p.x >= minX && p.x <= maxX &&
                   p.z >= minZ && p.z <= maxZ && p.y >= minimumY;
        }

        private void LateUpdate()
        {
            if (protectedCharacters == null || protectedCharacters.Length == 0) return;
            rescuedThisFrame.Clear();
            foreach (Transform actor in protectedCharacters)
            {
                if (actor == null || !rescuedThisFrame.Add(actor)) continue;
                Vector3 position = actor.position;
                if (Inside(position))
                {
                    lastSafe[actor] = position;
                    continue;
                }
                Vector3 destination = fallbackPosition;
                if (lastSafe.TryGetValue(actor,out Vector3 previous) && Inside(previous))
                    destination = previous;
                // CharacterController.Move can retain an old collision state across
                // direct teleport; briefly disable only the controlled component.
                CharacterController controller = actor.GetComponent<CharacterController>();
                bool wasEnabled = controller != null && controller.enabled;
                if (wasEnabled) controller.enabled = false;
                actor.position = destination;
                if (wasEnabled) controller.enabled = true;
                lastSafe[actor] = destination;
                Debug.LogWarning("SORA V04: character escaped map limits, restored safe position.");
            }
        }

        // Do not serialize a new game state or modify the existing save format.
        // Both existing twins must be assigned by the reviewed gameplay adapter.
        public void Bind(Transform firstTwin, Transform secondTwin)
        {
            protectedCharacters = new[] { firstTwin, secondTwin };
            lastSafe.Clear();
        }
    }
}
