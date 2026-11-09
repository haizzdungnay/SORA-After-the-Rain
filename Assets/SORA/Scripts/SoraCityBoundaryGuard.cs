using System.Collections.Generic;
using UnityEngine;

namespace Sora.AfterTheRain.Demo
{
    /// <summary>
    /// Actual runtime safety net for both characters in the nine-block map.
    /// Physics colliders remain primary; this restores a last valid position if an
    /// actor escapes through a gap. Does not change save schema, cash or inventory.
    /// </summary>
    public sealed class SoraCityBoundaryGuard : MonoBehaviour
    {
        private SoraDemoCore owner;
        private Vector3 minimum;
        private Vector3 maximum;
        private readonly Dictionary<Transform, Vector3> lastSafe = new Dictionary<Transform, Vector3>();
        private const float FloorRecoveryY = -4f;

        public void Initialize(SoraDemoCore game, Vector3 minCorner, Vector3 maxCorner)
        {
            owner = game;
            minimum = minCorner;
            maximum = maxCorner;
        }

        private bool IsSafe(Vector3 p)
        {
            return p.x >= minimum.x && p.x <= maximum.x &&
                   p.z >= minimum.z && p.z <= maximum.z && p.y >= FloorRecoveryY;
        }

        private void LateUpdate()
        {
            if (owner == null || owner.Characters == null) return;
            foreach (SoraDemoCharacter twin in owner.Characters)
            {
                if (twin == null) continue;
                Transform actor = twin.transform;
                Vector3 location = actor.position;
                if (IsSafe(location))
                {
                    lastSafe[actor] = location;
                    continue;
                }

                Vector3 recovery;
                if (!lastSafe.TryGetValue(actor, out recovery) || !IsSafe(recovery))
                    recovery = new Vector3(0f, 0f, -2.5f); // Old SORA MART interior.

                CharacterController controller = twin.GetComponent<CharacterController>();
                bool wasEnabled = controller != null && controller.enabled;
                if (wasEnabled) controller.enabled = false;
                actor.position = recovery;
                if (wasEnabled) controller.enabled = true;
                lastSafe[actor] = recovery;
                Debug.LogWarning("SORA CITY: restored a twin who crossed the nine-block world boundary.");
            }
        }
    }
}
