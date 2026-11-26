using System.Collections.Generic;
using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// WP2: Smoothly slides a 3D archive cabinet drawer open/closed when clicked.
    /// - Automatically disables inner envelope collider while closed so it NEVER blocks clicking the drawer!
    /// - Closes any other open drawer so upper drawers never block lower drawers.
    /// - Smoothly elevates & tilts the camera to look right down INSIDE whichever drawer you open!
    /// </summary>
    public class SlidingDrawer : MonoBehaviour
    {
        private static readonly List<SlidingDrawer> AllDrawers = new List<SlidingDrawer>();

        [Tooltip("Drawer label shown in status feedback (e.g. 'Drawer B-01 (Old Receipts)').")]
        public string drawerTitle = "Archive Drawer";

        [Tooltip("Local position offset when slid open.")]
        public Vector3 openLocalOffset = new Vector3(0.46f, 0f, 0f);

        [Tooltip("Optional message shown when opening an empty/decoy drawer.")]
        public string emptyMessage = "Only old archived tax receipts here. Check the Mail Delivery Log on your desk!";

        [Tooltip("If true, this drawer holds Alex's hidden letter.")]
        public bool containsAlexLetter = false;

        [SerializeField] private float slideSpeed = 7f;

        [Header("Open-Drawer Camera (Uses VP_Drawer1/2/3 if present, otherwise offset!)")]
        [Tooltip("Optional Viewpoint ID (e.g. 'Drawer1', 'Drawer2', 'Drawer3') so you can visually position each drawer's camera in Scene View!")]
        public string openDrawerViewpointId = "";
        [Tooltip("Fallback camera offset if no dedicated VP_Drawer viewpoint exists.")]
        public Vector3 camOffsetWhenOpen = new Vector3(0.48f, 0.92f, 0f);
        [Tooltip("Fallback look target offset inside the open drawer tray.")]
        public Vector3 lookOffsetWhenOpen = new Vector3(0.06f, -0.10f, 0f);

        private Vector3 closedLocalPos;
        private bool isOpen;
        private Collider innerItemCollider;
        private BoxCollider drawerFrontCollider;

        public bool IsOpen => isOpen;

        private void OnEnable()
        {
            if (!AllDrawers.Contains(this)) AllDrawers.Add(this);
        }

        private void OnDisable()
        {
            AllDrawers.Remove(this);
        }

        private void Awake()
        {
            closedLocalPos = transform.localPosition;
            drawerFrontCollider = GetComponent<BoxCollider>();
            // Find any child interactable (e.g. AlexHiddenEnvelope) and keep its collider disabled while closed!
            InteractableObject[] children = GetComponentsInChildren<InteractableObject>(true);
            foreach (InteractableObject child in children)
            {
                if (child.gameObject != gameObject)
                {
                    innerItemCollider = child.GetComponent<Collider>();
                    if (innerItemCollider != null) innerItemCollider.enabled = false;
                    break;
                }
            }
        }

        private void Update()
        {
            Vector3 target = isOpen ? closedLocalPos + openLocalOffset : closedLocalPos;
            transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * slideSpeed);
        }

        public void ForceClose()
        {
            isOpen = false;
            if (innerItemCollider != null) innerItemCollider.enabled = false;
        }

        public void ToggleDrawer()
        {
            bool willOpen = !isOpen;

            if (willOpen)
            {
                // Close any other open drawer so it never blocks looking inside this one
                foreach (SlidingDrawer other in AllDrawers)
                {
                    if (other != null && other != this && other.isOpen)
                    {
                        other.ForceClose();
                    }
                }
            }

            isOpen = willOpen;
            if (innerItemCollider != null) innerItemCollider.enabled = isOpen;
            SoundManager.Instance?.Play("drawer_slide", 0.34f);

            // Elevate & angle camera steeply right above the open drawer (prefers editable VP_Drawer1/2/3 viewpoint!)
            if (CameraViewpointController.Instance != null)
            {
                if (isOpen)
                {
                    CameraViewpoint customVp = null;
                    if (!string.IsNullOrEmpty(openDrawerViewpointId))
                    {
                        foreach (var vp in CameraViewpointController.Instance.Viewpoints)
                        {
                            if (vp != null && vp.viewpointId == openDrawerViewpointId)
                            {
                                customVp = vp;
                                break;
                            }
                        }
                    }

                    if (customVp != null)
                    {
                        CameraViewpointController.Instance.SetTemporaryOverridePose(customVp.transform.position, customVp.transform.rotation);
                    }
                    else
                    {
                        Vector3 openWorldCenter = transform.parent != null
                            ? transform.parent.TransformPoint(closedLocalPos + openLocalOffset)
                            : transform.position + openLocalOffset;

                        Vector3 camPos = openWorldCenter + camOffsetWhenOpen;
                        Vector3 lookAt = openWorldCenter + lookOffsetWhenOpen;
                        CameraViewpointController.Instance.SetTemporaryOverridePose(camPos, Quaternion.LookRotation(lookAt - camPos));
                    }
                }
                else
                {
                    CameraViewpointController.Instance.ClearTemporaryOverridePose();
                }
            }

            if (isOpen && MissionManager.Instance != null)
            {
                if (containsAlexLetter)
                {
                    if (!EvidenceManager.Instance.HasEvidence("letter_alex"))
                    {
                        MissionManager.Instance.NotifyStatus($"{drawerTitle} opened! You spot a yellow envelope from A. Moretti inside — click the envelope!");
                    }
                    else
                    {
                        MissionManager.Instance.NotifyStatus($"{drawerTitle} opened (you already collected Alex's letter).");
                    }
                }
                else
                {
                    MissionManager.Instance.NotifyStatus($"{drawerTitle}: {emptyMessage}");
                }
            }
        }
    }
}
