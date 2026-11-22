using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// WP2: Marks a fixed first-person camera position and rotation inside the Investigation Office.
    /// Attach this to an empty GameObject placed where the player's eyes should be for a given station
    /// (e.g., Desk, Computer, Cabinet, Bookshelf, Printer, EvidenceBoard).
    /// </summary>
    public class CameraViewpoint : MonoBehaviour
    {
        [Tooltip("Unique viewpoint ID used by buttons and scripts (e.g. Desk, Computer, Cabinet).")]
        public string viewpointId = "Desk";

        [Tooltip("Human-readable label shown on the HUD navigation bar.")]
        public string displayName = "Desk";

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.12f);
            Gizmos.DrawRay(transform.position, transform.forward * 0.8f);
        }
    }
}
