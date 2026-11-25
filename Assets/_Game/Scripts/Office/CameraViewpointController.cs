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
    /// WP2: Smoothly moves the Main Camera between fixed first-person viewpoints around the office,
    /// AND allows natural free-look around the room by holding Right-Mouse-Button + dragging
    /// (or pressing A/D / Arrow keys). Left-clicking any 3D object still interacts smoothly.
    /// </summary>
    public class CameraViewpointController : MonoBehaviour
    {
        public static CameraViewpointController Instance { get; private set; }

        [Header("Camera & Viewpoints")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private List<CameraViewpoint> viewpoints = new List<CameraViewpoint>();
        [SerializeField] private string defaultViewpointId = "Desk";

        [Header("Transition & Free-Look")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotateSpeed = 7f;
        [SerializeField] private float mouseLookSensitivity = 0.22f;
        [SerializeField] private float keyLookSpeed = 85f;
        [SerializeField] private float maxYawOffset = 80f;
        [SerializeField] private float maxPitchOffset = 32f;

        private CameraViewpoint currentTarget;
        private float lookYawOffset;
        private float lookPitchOffset;

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
            HandleFreeLookInput();

            if (currentTarget == null || cameraTransform == null)
                return;

            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                currentTarget.transform.position,
                Time.deltaTime * moveSpeed
            );

            // Apply natural free-look yaw (world up) + pitch (local right) on top of the current viewpoint's base rotation
            Quaternion desiredRot =
                Quaternion.AngleAxis(lookYawOffset, Vector3.up) *
                currentTarget.transform.rotation *
                Quaternion.AngleAxis(lookPitchOffset, Vector3.right);

            cameraTransform.rotation = Quaternion.Slerp(
                cameraTransform.rotation,
                desiredRot,
                Time.deltaTime * rotateSpeed
            );
        }

        private void HandleFreeLookInput()
        {
            // Skip free-look while hovering an open modal UI panel
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null && (mouse.rightButton.isPressed || mouse.middleButton.isPressed))
            {
                Vector2 delta = mouse.delta.ReadValue();
                lookYawOffset = Mathf.Clamp(lookYawOffset + delta.x * mouseLookSensitivity, -maxYawOffset, maxYawOffset);
                lookPitchOffset = Mathf.Clamp(lookPitchOffset - delta.y * mouseLookSensitivity, -maxPitchOffset, maxPitchOffset);
            }

            var kb = Keyboard.current;
            if (kb != null)
            {
                float h = 0f;
                float v = 0f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v -= 1f;

                if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
                {
                    lookYawOffset = Mathf.Clamp(lookYawOffset + h * keyLookSpeed * Time.deltaTime, -maxYawOffset, maxYawOffset);
                    lookPitchOffset = Mathf.Clamp(lookPitchOffset - v * keyLookSpeed * 0.65f * Time.deltaTime, -maxPitchOffset, maxPitchOffset);
                }
            }
#else
            if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
            {
                lookYawOffset = Mathf.Clamp(lookYawOffset + Input.GetAxis("Mouse X") * 3.5f, -maxYawOffset, maxYawOffset);
                lookPitchOffset = Mathf.Clamp(lookPitchOffset - Input.GetAxis("Mouse Y") * 3.5f, -maxPitchOffset, maxPitchOffset);
            }
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
            {
                lookYawOffset = Mathf.Clamp(lookYawOffset + h * keyLookSpeed * Time.deltaTime, -maxYawOffset, maxYawOffset);
                lookPitchOffset = Mathf.Clamp(lookPitchOffset - v * keyLookSpeed * 0.65f * Time.deltaTime, -maxPitchOffset, maxPitchOffset);
            }
#endif
        }

        private void HandleKeyboardShortcuts()
        {
            if (viewpoints == null || viewpoints.Count == 0)
                return;

#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.escapeKey.wasPressedThisFrame) { MoveToViewpoint(defaultViewpointId); return; }
            if (kb.digit1Key.wasPressedThisFrame && viewpoints.Count > 0) MoveToViewpoint(viewpoints[0].viewpointId);
            if (kb.digit2Key.wasPressedThisFrame && viewpoints.Count > 1) MoveToViewpoint(viewpoints[1].viewpointId);
            if (kb.digit3Key.wasPressedThisFrame && viewpoints.Count > 2) MoveToViewpoint(viewpoints[2].viewpointId);
            if (kb.digit4Key.wasPressedThisFrame && viewpoints.Count > 3) MoveToViewpoint(viewpoints[3].viewpointId);
            if (kb.digit5Key.wasPressedThisFrame && viewpoints.Count > 4) MoveToViewpoint(viewpoints[4].viewpointId);
            if (kb.digit6Key.wasPressedThisFrame && viewpoints.Count > 5) MoveToViewpoint(viewpoints[5].viewpointId);
#else
            if (Input.GetKeyDown(KeyCode.Escape)) { MoveToViewpoint(defaultViewpointId); return; }
            for (int i = 0; i < Mathf.Min(6, viewpoints.Count); i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    MoveToViewpoint(viewpoints[i].viewpointId);
                }
            }
#endif
        }

        public void SetViewpoints(List<CameraViewpoint> newViewpoints, Transform cam = null)
        {
            viewpoints = newViewpoints ?? new List<CameraViewpoint>();
            if (cam != null) cameraTransform = cam;
        }

        public void MoveToViewpoint(string viewpointId)
        {
            CameraViewpoint found = viewpoints.Find(v => v != null && v.viewpointId == viewpointId);
            if (found == null)
            {
                Debug.LogWarning($"[WP2] Viewpoint not found: {viewpointId}");
                return;
            }

            currentTarget = found;
            lookYawOffset = 0f;
            lookPitchOffset = 0f;
            OnViewpointChanged?.Invoke(found);
        }

        public void JumpToViewpointImmediate(string viewpointId)
        {
            CameraViewpoint found = viewpoints.Find(v => v != null && v.viewpointId == viewpointId);
            if (found == null) return;

            currentTarget = found;
            lookYawOffset = 0f;
            lookPitchOffset = 0f;
            if (cameraTransform != null)
            {
                cameraTransform.position = found.transform.position;
                cameraTransform.rotation = found.transform.rotation;
            }
            OnViewpointChanged?.Invoke(found);
        }
    }
}
