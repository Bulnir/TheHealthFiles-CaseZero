using System;
using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// WP2: Attach to any clickable 3D object in the Investigation Office (Phone, Computer, Cabinet,
    /// Sliding Drawers, Mail Log, Bookshelf, Printer, Evidence Board, etc.).
    /// Highlights on mouse hover and notifies WP3's MissionManager using its unique interactableId.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InteractableObject : MonoBehaviour
    {
        [Header("WP2 -> WP3 Contract")]
        [Tooltip("Frozen ID consumed by WP3 MissionManager (e.g. desk_phone, bookshelf_manual, cabinet_alex_letter).")]
        public string interactableId = "object_id";

        [Tooltip("Label displayed on screen when hovering over this object.")]
        public string displayLabel = "Inspect Object";

        [Tooltip("Optional: Automatically move camera to this viewpoint ID when clicked.")]
        public string focusViewpointId = "";

        [Tooltip("Optional: Only allow hovering/clicking when the camera is at this viewpoint ID (e.g. 'Desk', 'Bookshelf', 'Cabinet'). Leave empty for any.")]
        public string requiredViewpointId = "";

        [Header("Visual Hover Feedback")]
        [SerializeField] private Color hoverTint = new Color(1f, 0.85f, 0.35f, 1f);

        private Renderer[] cachedRenderers;
        private Color[] originalColors;
        private bool isHighlighted;

        public static event Action<InteractableObject> OnAnyObjectClicked;

        private void Awake()
        {
            CacheRenderers();
        }

        private void CacheRenderers()
        {
            cachedRenderers = GetComponentsInChildren<Renderer>();
            originalColors = new Color[cachedRenderers.Length];
            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                if (cachedRenderers[i] != null && cachedRenderers[i].material != null)
                {
                    originalColors[i] = GetMaterialColor(cachedRenderers[i].material);
                }
            }
        }

        public void SetHighlighted(bool highlighted)
        {
            if (isHighlighted == highlighted || cachedRenderers == null)
                return;

            isHighlighted = highlighted;
            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                if (cachedRenderers[i] == null || cachedRenderers[i].material == null)
                    continue;

                Color targetColor = highlighted
                    ? Color.Lerp(originalColors[i], hoverTint, 0.45f)
                    : originalColors[i];

                SetMaterialColor(cachedRenderers[i].material, targetColor);
            }
        }

        public void Interact()
        {
            if (!string.IsNullOrEmpty(focusViewpointId) && CameraViewpointController.Instance != null)
            {
                CameraViewpointController.Instance.MoveToViewpoint(focusViewpointId);
            }

            // If this interactable is a physical 3D sliding drawer, slide it open/closed!
            SlidingDrawer drawer = GetComponent<SlidingDrawer>();
            if (drawer != null)
            {
                drawer.ToggleDrawer();
            }
            else
            {
                SoundManager.Instance?.Play("ui_click", 0.55f);
            }

            Debug.Log($"[WP2] Clicked interactable: {interactableId} ({displayLabel})");
            OnAnyObjectClicked?.Invoke(this);

            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.HandleInteractableClicked(interactableId);
            }
        }

        private static Color GetMaterialColor(Material mat)
        {
            if (mat.HasProperty("_BaseColor")) return mat.GetColor("_BaseColor");
            if (mat.HasProperty("_Color")) return mat.color;
            return Color.white;
        }

        private static void SetMaterialColor(Material mat, Color color)
        {
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.color = color;
        }
    }
}
