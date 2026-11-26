using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HealthFiles
{
    /// <summary>
    /// WP2: Effortless Point-and-Click Detective Office Camera Controller.
    /// - Gentle, automatic Mouse Parallax (no buttons held! Moving the mouse subtly tilts/pans your view).
    /// - Clean Left / Right chair-turn navigation (screen arrows or Q/E keys) cycling naturally around the room:
    ///   Printer <-> EvidenceBoard <-> Cabinet <-> Desk <-> Bookshelf.
    /// - Clicking any 3D object zooms cleanly into it; pressing Esc or clicking 'Back to Desk' returns to Desk.
    /// - Parallax automatically pauses while zoomed into the Computer OS or reading modals.
    /// </summary>
    public class CameraViewpointController : MonoBehaviour
    {
        public static CameraViewpointController Instance { get; private set; }

        [Header("Camera & Viewpoints")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private List<CameraViewpoint> viewpoints = new List<CameraViewpoint>();
        [SerializeField] private string defaultViewpointId = "Desk";

        [Header("Smooth Transition & Gentle Mouse Parallax")]
        [SerializeField] private float moveSpeed = 6.5f;
        [SerializeField] private float rotateSpeed = 7.5f;
        [SerializeField] private float parallaxMaxYaw = 16f;
        [SerializeField] private float parallaxMaxPitch = 7f;
        [SerializeField] private float parallaxSmooth = 5f;

        // Natural spatial order around the room from Left to Right
        private static readonly string[] RoomTurnOrder =
        {
            "Printer",
            "EvidenceBoard",
            "Cabinet",
            "Desk",
            "Bookshelf"
        };

        private CameraViewpoint currentTarget;
        private float currentParallaxYaw;
        private float currentParallaxPitch;
        private bool hasOverridePose;
        private Vector3 overridePos;
        private Quaternion overrideRot;

        /// <summary>
        /// Set true while a fullscreen modal (PC Desktop, Phone, Document) is open so camera stays rock-steady.
        /// </summary>
        public bool IsCameraLockedByModal { get; set; }

        public event Action<CameraViewpoint> OnViewpointChanged;
        public IReadOnlyList<CameraViewpoint> Viewpoints => viewpoints;
        public string CurrentViewpointId => currentTarget != null ? currentTarget.viewpointId : "";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Start()
        {
            if (viewpoints.Count == 0)
            {
                viewpoints.AddRange(FindObjectsByType<CameraViewpoint>(FindObjectsSortMode.None));
            }

            if (!string.IsNullOrEmpty(defaultViewpointId))
            {
                JumpToViewpointImmediate(defaultViewpointId);
            }
            else if (viewpoints.Count > 0)
            {
                JumpToViewpointImmediate(viewpoints[0].viewpointId);
            }
        }

        private void Update()
        {
            HandleKeyboardShortcuts();

            if (currentTarget == null || cameraTransform == null)
                return;

            Vector3 desiredPos = hasOverridePose ? overridePos : currentTarget.transform.position;
            Quaternion baseRot = hasOverridePose ? overrideRot : currentTarget.transform.rotation;

            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                desiredPos,
                Time.deltaTime * moveSpeed
            );

            // Compute gentle mouse-position parallax (zero buttons held!) when not locked by a modal or zoomed into PC
            float targetYaw = 0f;
            float targetPitch = 0f;
            bool allowParallax = !IsCameraLockedByModal && CurrentViewpointId != "Computer";

            if (allowParallax && Screen.width > 0 && Screen.height > 0)
            {
                Vector2 mPos = GetMousePosition();
                float nx = Mathf.Clamp((mPos.x / Screen.width) * 2f - 1f, -1f, 1f);
                float ny = Mathf.Clamp((mPos.y / Screen.height) * 2f - 1f, -1f, 1f);
                float maxYaw = (currentTarget.customParallaxMaxYaw > 0.1f) ? currentTarget.customParallaxMaxYaw : parallaxMaxYaw;
                float scale = hasOverridePose ? 0.35f : 1f;
                targetYaw = nx * maxYaw * scale;
                targetPitch = -ny * parallaxMaxPitch * scale;
            }

            currentParallaxYaw = Mathf.Lerp(currentParallaxYaw, targetYaw, Time.deltaTime * parallaxSmooth);
            currentParallaxPitch = Mathf.Lerp(currentParallaxPitch, targetPitch, Time.deltaTime * parallaxSmooth);

            Quaternion desiredRot =
                Quaternion.AngleAxis(currentParallaxYaw, Vector3.up) *
                baseRot *
                Quaternion.AngleAxis(currentParallaxPitch, Vector3.right);

            cameraTransform.rotation = Quaternion.Slerp(
                cameraTransform.rotation,
                desiredRot,
                Time.deltaTime * rotateSpeed
            );
        }

        public void TurnLeft()
        {
            if (IsCameraLockedByModal) return;
            if (CurrentViewpointId == "Computer")
            {
                MoveToViewpoint("Cabinet");
                return;
            }

            int idx = Array.IndexOf(RoomTurnOrder, CurrentViewpointId);
            if (idx < 0) idx = 3; // default Desk
            int nextIdx = Mathf.Max(0, idx - 1);
            MoveToViewpoint(RoomTurnOrder[nextIdx]);
        }

        public void TurnRight()
        {
            if (IsCameraLockedByModal) return;
            if (CurrentViewpointId == "Computer")
            {
                MoveToViewpoint("Bookshelf");
                return;
            }

            int idx = Array.IndexOf(RoomTurnOrder, CurrentViewpointId);
            if (idx < 0) idx = 3; // default Desk
            int nextIdx = Mathf.Min(RoomTurnOrder.Length - 1, idx + 1);
            MoveToViewpoint(RoomTurnOrder[nextIdx]);
        }

        public bool CanTurnLeft()
        {
            int idx = Array.IndexOf(RoomTurnOrder, CurrentViewpointId);
            return idx > 0 || CurrentViewpointId == "Computer";
        }

        public bool CanTurnRight()
        {
            int idx = Array.IndexOf(RoomTurnOrder, CurrentViewpointId);
            return (idx >= 0 && idx < RoomTurnOrder.Length - 1) || CurrentViewpointId == "Computer";
        }

        private void HandleKeyboardShortcuts()
        {
            if (viewpoints == null || viewpoints.Count == 0 || IsCameraLockedByModal)
                return;

#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.escapeKey.wasPressedThisFrame) { MoveToViewpoint(defaultViewpointId); return; }
            if (kb.qKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame) { TurnLeft(); return; }
            if (kb.eKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame) { TurnRight(); return; }
            if (kb.digit1Key.wasPressedThisFrame && viewpoints.Count > 0) MoveToViewpoint(viewpoints[0].viewpointId);
            if (kb.digit2Key.wasPressedThisFrame && viewpoints.Count > 1) MoveToViewpoint(viewpoints[1].viewpointId);
            if (kb.digit3Key.wasPressedThisFrame && viewpoints.Count > 2) MoveToViewpoint(viewpoints[2].viewpointId);
            if (kb.digit4Key.wasPressedThisFrame && viewpoints.Count > 3) MoveToViewpoint(viewpoints[3].viewpointId);
            if (kb.digit5Key.wasPressedThisFrame && viewpoints.Count > 4) MoveToViewpoint(viewpoints[4].viewpointId);
            if (kb.digit6Key.wasPressedThisFrame && viewpoints.Count > 5) MoveToViewpoint(viewpoints[5].viewpointId);
#else
            if (Input.GetKeyDown(KeyCode.Escape)) { MoveToViewpoint(defaultViewpointId); return; }
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.LeftArrow)) { TurnLeft(); return; }
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.RightArrow)) { TurnRight(); return; }
            for (int i = 0; i < Mathf.Min(6, viewpoints.Count); i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i)) MoveToViewpoint(viewpoints[i].viewpointId);
            }
#endif
        }

        private static Vector2 GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.position.ReadValue() : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
#else
            return Input.mousePosition;
#endif
        }

        public void SetViewpoints(List<CameraViewpoint> newViewpoints, Transform cam = null)
        {
            viewpoints = newViewpoints ?? new List<CameraViewpoint>();
            if (cam != null) cameraTransform = cam;
        }

        public void SetTemporaryOverridePose(Vector3 pos, Quaternion rot)
        {
            hasOverridePose = true;
            overridePos = pos;
            overrideRot = rot;
        }

        public void ClearTemporaryOverridePose()
        {
            hasOverridePose = false;
        }

        public void MoveToViewpoint(string viewpointId)
        {
            CameraViewpoint found = viewpoints.Find(v => v != null && v.viewpointId == viewpointId);
            if (found == null)
            {
                Debug.LogWarning($"[WP2] Viewpoint not found: {viewpointId}");
                return;
            }

            hasOverridePose = false;
            currentTarget = found;
            OnViewpointChanged?.Invoke(found);
        }

        public void JumpToViewpointImmediate(string viewpointId)
        {
            CameraViewpoint found = viewpoints.Find(v => v != null && v.viewpointId == viewpointId);
            if (found == null) return;

            currentTarget = found;
            if (cameraTransform != null)
            {
                cameraTransform.position = found.transform.position;
                cameraTransform.rotation = found.transform.rotation;
            }
            OnViewpointChanged?.Invoke(found);
        }
    }
}
