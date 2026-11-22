using System;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HealthFiles
{
    /// <summary>
    /// WP2: Casts a ray from the Main Camera through the mouse cursor to hover-highlight
    /// and click InteractableObjects inside the 3D office. Automatically ignores clicks on UI panels.
    /// </summary>
    public class OfficeRaycaster : MonoBehaviour
    {
        public static OfficeRaycaster Instance { get; private set; }

        [SerializeField] private float maxInteractDistance = 25f;

        private Camera mainCam;
        private InteractableObject currentHovered;

        public event Action<string> OnHoverLabelChanged;

        private void Awake()
        {
            Instance = this;
            mainCam = GetComponent<Camera>();
            if (mainCam == null) mainCam = Camera.main;
        }

        private void Update()
        {
            if (mainCam == null) return;

            // Do not raycast through open UI panels (Phone, Computer, Inventory, etc.)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                ClearHover();
                return;
            }

            Vector2 mousePos = GetMousePosition();
            Ray ray = mainCam.ScreenPointToRay(mousePos);

            InteractableObject hitObj = null;
            if (Physics.Raycast(ray, out RaycastHit hit, maxInteractDistance))
            {
                hitObj = hit.collider.GetComponentInParent<InteractableObject>();
            }

            if (hitObj != currentHovered)
            {
                if (currentHovered != null)
                {
                    currentHovered.SetHighlighted(false);
                }

                currentHovered = hitObj;

                if (currentHovered != null)
                {
                    currentHovered.SetHighlighted(true);
                    OnHoverLabelChanged?.Invoke(currentHovered.displayLabel);
                }
                else
                {
                    OnHoverLabelChanged?.Invoke(string.Empty);
                }
            }

            if (currentHovered != null && WasLeftMouseClicked())
            {
                currentHovered.Interact();
            }
        }

        private void ClearHover()
        {
            if (currentHovered != null)
            {
                currentHovered.SetHighlighted(false);
                currentHovered = null;
                OnHoverLabelChanged?.Invoke(string.Empty);
            }
        }

        private static Vector2 GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
            return Input.mousePosition;
#endif
        }

        private static bool WasLeftMouseClicked()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }
    }
}
