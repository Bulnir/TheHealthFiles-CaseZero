using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HealthFiles
{
    /// <summary>
    /// WP2: Smoothly moves the Main Camera between fixed first-person viewpoints around the office.
    /// Supports UI button clicks as well as quick keyboard shortcuts (keys 1-6, Esc to return to Desk).
    /// </summary>
    public class CameraViewpointController : MonoBehaviour
    {
        public static CameraViewpointController Instance { get; private set; }

        [Header("Camera & Viewpoints")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private List<CameraViewpoint> viewpoints = new List<CameraViewpoint>();
        [SerializeField] private string defaultViewpointId = "Desk";

        [Header("Transition")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotateSpeed = 6f;

        private CameraViewpoint currentTarget;

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

            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                currentTarget.transform.position,
                Time.deltaTime * moveSpeed
            );

            cameraTransform.rotation = Quaternion.Slerp(
                cameraTransform.rotation,
                currentTarget.transform.rotation,
                Time.deltaTime * rotateSpeed
            );
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
