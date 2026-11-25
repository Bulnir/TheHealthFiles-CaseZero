using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// WP2: Smoothly slides a 3D archive cabinet drawer open/closed when clicked.
    /// Used in Case 01 (Alex) so the player checks the desk Mail Log (Drawer B-02),
    /// physically opens the drawers, and clicks Alex's envelope inside.
    /// </summary>
    public class SlidingDrawer : MonoBehaviour
    {
        [Tooltip("Drawer label shown in status feedback (e.g. 'Drawer B-01 (Old Receipts)').")]
        public string drawerTitle = "Archive Drawer";

        [Tooltip("Local position offset when slid open.")]
        public Vector3 openLocalOffset = new Vector3(0.45f, 0f, 0f);

        [Tooltip("Optional message shown when opening an empty/decoy drawer.")]
        public string emptyMessage = "Only old archived tax receipts here. Check the Mail Delivery Log on your desk!";

        [Tooltip("If true, this drawer holds Alex's hidden letter.")]
        public bool containsAlexLetter = false;

        [SerializeField] private float slideSpeed = 6f;

        private Vector3 closedLocalPos;
        private bool isOpen;

        private void Awake()
        {
            closedLocalPos = transform.localPosition;
        }

        private void Update()
        {
            Vector3 target = isOpen ? closedLocalPos + openLocalOffset : closedLocalPos;
            transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * slideSpeed);
        }

        public void ToggleDrawer()
        {
            isOpen = !isOpen;
            SoundManager.Instance?.Play("drawer_slide", 0.8f);

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
