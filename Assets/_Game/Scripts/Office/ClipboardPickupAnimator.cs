using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// WP2: Smoothly lifts the 3D Mail Delivery Log clipboard off the desk toward the player's hands
    /// when inspected, and sets it gently back down on the desk when closed!
    /// </summary>
    public class ClipboardPickupAnimator : MonoBehaviour
    {
        public static ClipboardPickupAnimator Instance { get; private set; }

        private Vector3 deskPos;
        private Quaternion deskRot;
        private bool isHeld;

        private void Awake()
        {
            Instance = this;
            deskPos = transform.position;
            deskRot = transform.rotation;
        }

        public void SetHeld(bool held)
        {
            isHeld = held;
        }

        private void Update()
        {
            Vector3 targetPos = deskPos;
            Quaternion targetRot = deskRot;

            if (isHeld && Camera.main != null)
            {
                Transform cam = Camera.main.transform;
                // Lift up and slightly forward/below camera center as if holding it in hand
                targetPos = cam.position + cam.forward * 0.42f - cam.up * 0.14f;
                targetRot = Quaternion.LookRotation(cam.up, -cam.forward) * Quaternion.Euler(0f, 0f, 4f);
            }

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 9f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 9f);
        }
    }
}
