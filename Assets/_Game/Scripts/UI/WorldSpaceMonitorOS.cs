using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HealthFiles
{
    /// <summary>
    /// WP3 + WP2: Realistic World-Space PC Desktop ('HealthOS') mounted directly onto the 3D Monitor!
    /// Features:
    /// - Smooth gradient desktop wallpaper + transparent hoverable desktop icons with procedurally drawn graphics:
    ///   This PC, Browser, Case Files (Explorer), Mail, PrivacyPolicy.pdf, Notes.txt, Recycle Bin
    /// - Working Start Menu popup, real Windows-style Taskbar with pinned shortcuts, system tray (SFX/Clock/Date)
    /// - Full interactive apps: Browser (News + Case 01 Blog vs Policy Inspector with Guided/Independent DHLI banner),
    ///   File Explorer, Office Mail Inbox, This PC drive viewer, PDF Reader, Notepad, and interactive Recycle Bin!
    /// </summary>
    public class WorldSpaceMonitorOS : MonoBehaviour
    {
        public static WorldSpaceMonitorOS Instance { get; private set; }

        [Header("3D Monitor Binding")]
        [SerializeField] private BoxCollider outerMonitorCollider;
        [SerializeField] private Vector2 screenWorldSize = new Vector2(0.56f, 0.33f);

        private Font uiFont;
        private Font monoFont;
        private Sprite roundedSprite;
        private Canvas worldCanvas;
        private GraphicRaycaster worldRaycaster;

        // OS UI elements
        private GameObject osWindowFrame;
        private GameObject startMenuPopup;
        private Text osWindowTitle;
        private Text osTaskbarActiveApp;
        private Text osClockText;
        private RawImage osTrayMuteIcon;
        private Texture2D volOnIconTex;
        private Texture2D volMutedIconTex;

        // App content panels
        private GameObject appBrowserObj;
        private GameObject browserNewsTab;
        private GameObject browserAuditTab;
        private GameObject browserLockedBox;
        private GameObject browserGameBox;
        private Text browserUrlText;
        private Text browserModeHintText;
        private Text computerFeedbackText;

        private GameObject appExplorerObj;
        private GameObject appMailObj;
        private Text mailBodyText;
        private GameObject appThisPcObj;
        private GameObject appPolicyObj;
        private GameObject appNotesObj;
        private GameObject appRecycleObj;
        private Text recycleListText;
        private Button emptyRecycleBtn;
        private bool recycleBinEmptied;

        // Papers, Please style 3-File Triage Queue + 4-Document Multi-Desk Discrepancy Linker
        // Queue: 0 = VitaPulse Official FAQ (VERIFIED, 0 lies), 1 = Viral Influencer Post (MISLEADING AD, 2 lies), 2 = Alex's Viral Blog (UNVERIFIED, 5 lies)
        private int activeQueueFileIndex = 0;
        private readonly bool[] queueFileCompleted = new bool[3];
        private int selectedLeftIndex = -1;
        private readonly bool[] pairSolved = new bool[7]; // 0..4 = Alex Blog (5 lies), 5..6 = Influencer Post (2 lies)
        private readonly Button[] leftClaimButtons = new Button[6];
        private readonly Image[] leftClaimBgs = new Image[6];
        private readonly Text[] leftClaimLabels = new Text[6];
        private readonly Button[] rightClueButtons = new Button[7];
        private readonly Image[] rightClueBgs = new Image[7];
        private readonly Text[] rightClueLabels = new Text[7];
        private GameObject stampBarObj;
        private GameObject bigStampOverlayObj;
        private Text bigStampOverlayText;
        private Image bigStampOverlayBg;
        private Text stampStatusText;
        private Text queueProgressText;
        private Text leftDocHeaderText;
        private Text leftMastheadText;
        private Image leftMastheadBg;
        private RawImage leftHeroBannerImg;
        private Texture2D heroBannerFaqTex;
        private Texture2D heroBannerSocialTex;
        private Texture2D heroBannerBlogTex;
        private Text leftFooterEngagementText;
        private Text stepGuideText;
        private Button refTabPolicyBtn;
        private Button refTabRegistryBtn;
        private Button refTabRulesBtn;
        private int activeRefTabIndex = 0; // 0=Policy, 1=Registry, 2=Rulebook
        private Button stampBtnA;
        private Button stampBtnB;
        private Button stampBtnC;
        private GameObject bookcaseStickyObj;
        private Text bookcaseStickyText;
        private RectTransform inspectionDeskRect;
        private RectTransform redThreadRect;
        private float redThreadTimer;
        private RectTransform blogDocRect;
        private RectTransform policyDocRect;
        private RectTransform registryDocRect;
        private RectTransform rulesDocRect;

        private struct LeftClaimItem
        {
            public string label;
            public int pairIndex; // 0..6, or -1 if factually true / distractor
            public string wrongHint;
        }

        private struct RightClueItem
        {
            public int docGroup;  // 0=Policy v4.2, 1=WHOIS & EU Registry, 2=Rulebook
            public string label;
            public int pairIndex; // 0..6, or -1 if distractor
            public string wrongHint;
        }

        // FILE 1/3: Official VitaPulse FAQ (Authentic Website Layout — 0 contradictions!)
        private readonly LeftClaimItem[] file0FaqClaims =
        {
            new LeftClaimItem {
                label = "<b>VitaPulse Health S.r.l. — Official Patient FAQ (Milan)</b>",
                pairIndex = -1,
                wrongHint = "Matches official registry! No contradiction here — if every line checks out, stamp VERIFIED below."
            },
            new LeftClaimItem {
                label = "<i>Current Privacy Policy v4.2 — Effective October 2026</i>",
                pairIndex = -1,
                wrongHint = "Matches Policy v4.2 header! No contradiction here — check the remaining lines."
            },
            new LeftClaimItem {
                label = "EU Health Registry: Listed as an ACTIVE wellness app (routine audit).",
                pairIndex = -1,
                wrongHint = "Matches the EU Health Registry! Everything in this FAQ is consistent — try stamping VERIFIED."
            },
            new LeftClaimItem {
                label = "Your Data Rights: Export or delete your health diary anytime (Sec. 5.1).",
                pairIndex = -1,
                wrongHint = "Matches Policy Section 5.1! Zero lies in File 1 — stamp VERIFIED to move to File 2!"
            }
        };

        // FILE 2/3: Viral Influencer Reel (Authentic Social Post Layout — 2 contradictions!)
        private readonly LeftClaimItem[] file1AdClaims =
        {
            new LeftClaimItem {
                label = "<b>\"Clinically PROVEN to prevent 99% of heart attacks!!\"</b>",
                pairIndex = 5,
                wrongHint = "Check the Rulebook in DHIO Terminal — miracle health claims require traceable clinical studies!"
            },
            new LeftClaimItem {
                label = "100% unpaid honest review — not sponsored! Use my code <b>VITA20</b> for 20% off!",
                pairIndex = 6,
                wrongHint = "Check the Rulebook in DHIO Terminal — promo codes without ad disclosure = hidden sponsorship!"
            },
            new LeftClaimItem {
                label = "<i>Posted as a 30-second vertical health reel (4.2M views in 48 hours).</i>",
                pairIndex = -1,
                wrongHint = "High view count is real, not a policy contradiction — inspect the miracle claim or promo code instead!"
            },
            new LeftClaimItem {
                label = "<i>Reviewing the VitaPulse sleep &amp; daily symptom tracker app.</i>",
                pairIndex = -1,
                wrongHint = "Basic background fact — no contradiction here!"
            }
        };

        // FILE 3/3: Alex's Viral Blog (Authentic News/Conspiracy Blog Layout — 5 contradictions!)
        private readonly LeftClaimItem[] file2BlogClaims =
        {
            new LeftClaimItem {
                label = "<b>VitaPulse Secretly Sells ALL Medical Records to Brokers!</b>",
                pairIndex = 0,
                wrongHint = "Check Privacy Policy v4.2 (Section 4.2) — does it actually prove selling ALL records?"
            },
            new LeftClaimItem {
                label = "<i>By HealthTruth99  •  Independent Whistleblower</i>",
                pairIndex = 1,
                wrongHint = "Check the WHOIS & EU DB tab in DHIO Terminal to see who really owns this blog domain!"
            },
            new LeftClaimItem {
                label = "<i>Evidence leaked from VitaPulse Policy v1.0 (2022)</i>",
                pairIndex = 2,
                wrongHint = "Compare that 2022 v1.0 date against the current Privacy Policy v4.2 header!"
            },
            new LeftClaimItem {
                label = "<color=#8B1E1E><b>BREAKING:</b> Banned by the EU Health Ministry last month!</color>",
                pairIndex = 3,
                wrongHint = "Check the EU Health Registry status in the WHOIS & EU DB tab!"
            },
            new LeftClaimItem {
                label = "VitaPulse is a widely downloaded mobile sleep &amp; symptom diary app.",
                pairIndex = -1,
                wrongHint = "No contradiction here — that's factually accurate background info. Inspect a suspicious claim instead!"
            },
            new LeftClaimItem {
                label = "<i>Sources: \"No links needed — everyone on Telegram already knows!\"</i>",
                pairIndex = 4,
                wrongHint = "Check the Rulebook tab in DHIO Terminal for what valid evidence requires!"
            }
        };

        private LeftClaimItem[] ActiveLeftClaims => activeQueueFileIndex == 0 ? file0FaqClaims
            : (activeQueueFileIndex == 1 ? file1AdClaims : file2BlogClaims);

        private readonly RightClueItem[] rightClues =
        {
            // Doc 0: Official Privacy Policy v4.2
            new RightClueItem {
                docGroup = 0,
                label = "<b>HEADER:</b>  Official Privacy Policy <b>v4.2</b> — Effective Oct 2026 (replaces 2022 v1.0).",
                pairIndex = 2,
                wrongHint = "This contradicts an outdated version/date claim!"
            },
            new RightClueItem {
                docGroup = 0,
                label = "<b>SEC. 4.2:</b>  \"Some anonymized data may be shared with third-party partners\" (vague, NOT selling all records).",
                pairIndex = 0,
                wrongHint = "This contradicts an extreme headline claiming ALL medical records are sold!"
            },
            new RightClueItem {
                docGroup = 0,
                label = "<b>SEC. 5.1:</b>  \"Users may export or delete diary entries at any time.\"",
                pairIndex = -1,
                wrongHint = "Section 5.1 is a valid user-rights clause — no contradiction."
            },
            // Doc 1: WHOIS & EU Health Registry
            new RightClueItem {
                docGroup = 1,
                label = "<b>WHOIS LOG:</b>  Blog domain registered 4 days ago via anonymous proxy — no medical credentials or author ID.",
                pairIndex = 1,
                wrongHint = "This exposes an anonymous 'whistleblower' author!"
            },
            new RightClueItem {
                docGroup = 1,
                label = "<b>EU REGISTRY:</b>  VitaPulse status: <b>ACTIVE</b> (Routine Audit) — Zero bans or suspension orders issued.",
                pairIndex = 3,
                wrongHint = "This disproves any claim of an EU Ministry ban!"
            },
            // Doc 2: Health Literacy Rulebook
            new RightClueItem {
                docGroup = 2,
                label = "<b>RULE 1 — EVIDENCE:</b>  Medical accusations & miracle claims require traceable studies — social rumors/hype are invalid.",
                pairIndex = 4, // Also matches pairIndex 5 (Influencer miracle claim) in handler!
                wrongHint = "This contradicts unverified rumors or unsupported miracle health claims!"
            },
            new RightClueItem {
                docGroup = 2,
                label = "<b>RULE 2 — AD DISCLOSURE:</b>  Promo codes (e.g. VITA20) & affiliate links without disclosure = Hidden Sponsored Ad.",
                pairIndex = 6,
                wrongHint = "This exposes hidden sponsored influencer posts pretending to be unpaid reviews!"
            }
        };

        public void Configure(BoxCollider monitorCollider, Vector2 worldSize)
        {
            outerMonitorCollider = monitorCollider;
            screenWorldSize = worldSize;
        }

        private void Awake()
        {
            Instance = this;
            Font fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            uiFont = LoadBestFont(new[] { "Segoe UI", "Calibri", "Helvetica Neue", "Arial" }, 14, fallback);
            monoFont = LoadBestFont(new[] { "Consolas", "Courier New", "Menlo" }, 12, uiFont);
            roundedSprite = CreateRoundedSprite(64, 12);
            BuildWorldSpaceDesktop();
        }

        private static Font LoadBestFont(string[] candidates, int size, Font fallback)
        {
            try
            {
                Font f = Font.CreateDynamicFontFromOSFont(candidates, size);
                if (f != null) return f;
            }
            catch { }
            return fallback;
        }

        private void Start()
        {
            if (worldCanvas != null && Camera.main != null)
            {
                worldCanvas.worldCamera = Camera.main;
            }

            if (CameraViewpointController.Instance != null)
            {
                CameraViewpointController.Instance.OnViewpointChanged += HandleViewpointChanged;
                HandleViewpointChanged(null);
            }

            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnAlexComputerOpened += FocusComputerScreen;
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.OnAudioSettingsChanged += RefreshTrayAudio;
                RefreshTrayAudio();
            }

            // Show the clean, realistic PC desktop right on the 3D monitor!
            osWindowFrame.SetActive(false);
            if (startMenuPopup != null) startMenuPopup.SetActive(false);
            osTaskbarActiveApp.text = "Desktop";

            OptimizeComputerCameraZoom();
        }

        private void OptimizeComputerCameraZoom()
        {
            var ctrl = CameraViewpointController.Instance;
            if (ctrl == null) return;
            foreach (CameraViewpoint vp in ctrl.Viewpoints)
            {
                if (vp != null && vp.viewpointId == "Computer")
                {
                    Vector3 toCam = vp.transform.position - transform.position;
                    if (toCam.sqrMagnitude > 0.001f)
                    {
                        vp.transform.position = transform.position + toCam.normalized * 0.32f;
                        vp.transform.LookAt(transform.position, Vector3.up);
                    }
                    break;
                }
            }
        }

        private void OnDestroy()
        {
            if (CameraViewpointController.Instance != null)
                CameraViewpointController.Instance.OnViewpointChanged -= HandleViewpointChanged;
            if (MissionManager.Instance != null)
                MissionManager.Instance.OnAlexComputerOpened -= FocusComputerScreen;
            if (SoundManager.Instance != null)
                SoundManager.Instance.OnAudioSettingsChanged -= RefreshTrayAudio;
        }

        private void Update()
        {
            if (worldCanvas != null && worldCanvas.worldCamera == null && Camera.main != null)
            {
                worldCanvas.worldCamera = Camera.main;
            }
            if (osClockText != null)
            {
                osClockText.text = DateTime.Now.ToString("HH:mm") + "\n" + DateTime.Now.ToString("dd/MM/yyyy");
            }
            if (redThreadTimer > 0f && redThreadRect != null)
            {
                redThreadTimer -= Time.unscaledDeltaTime;
                if (redThreadTimer <= 0f) redThreadRect.gameObject.SetActive(false);
            }
        }

        private void RefreshTrayAudio()
        {
            if (osTrayMuteIcon == null) return;
            bool muted = SoundManager.Instance != null && SoundManager.Instance.IsMuted;
            osTrayMuteIcon.texture = muted ? volMutedIconTex : volOnIconTex;
        }

        private void HandleViewpointChanged(CameraViewpoint vp)
        {
            bool zoomedOnPC = CameraViewpointController.Instance != null &&
                              CameraViewpointController.Instance.CurrentViewpointId == "Computer";

            if (outerMonitorCollider != null) outerMonitorCollider.enabled = !zoomedOnPC;
            if (worldRaycaster != null) worldRaycaster.enabled = zoomedOnPC;
        }

        public void FocusComputerScreen()
        {
            CameraViewpointController.Instance?.MoveToViewpoint("Computer");

            // If player already collected Alex's letter and is on the blog inspection step, open Browser directly!
            if (MissionManager.Instance != null && MissionManager.Instance.CurrentState == MissionState.Alex_InspectBlogAndPolicy)
            {
                OpenOSApp("browser_audit", playSound: false);
            }
        }

        private void OpenOSApp(string appId, bool playSound = true)
        {
            if (playSound) SoundManager.Instance?.Play("ui_click", 0.5f);
            if (startMenuPopup != null) startMenuPopup.SetActive(false);

            osWindowFrame.SetActive(true);
            appBrowserObj.SetActive(false);
            appExplorerObj.SetActive(false);
            appMailObj.SetActive(false);
            appThisPcObj.SetActive(false);
            appPolicyObj.SetActive(false);
            appNotesObj.SetActive(false);
            appRecycleObj.SetActive(false);

            switch (appId)
            {
                case "browser":
                case "browser_news":
                    osWindowTitle.text = "Web Browser — Trending Health News";
                    osTaskbarActiveApp.text = "Browser";
                    appBrowserObj.SetActive(true);
                    ShowBrowserTab(auditTab: false);
                    break;

                case "browser_audit":
                    osWindowTitle.text = "Web Browser — Blog & Policy Audit";
                    osTaskbarActiveApp.text = "Browser";
                    appBrowserObj.SetActive(true);
                    ShowBrowserTab(auditTab: true);
                    break;

                case "files":
                    osWindowTitle.text = "File Explorer — Case Files";
                    osTaskbarActiveApp.text = "Case Files";
                    appExplorerObj.SetActive(true);
                    break;

                case "mail":
                    osWindowTitle.text = "Office Mail — Inbox (3)";
                    osTaskbarActiveApp.text = "Mail";
                    appMailObj.SetActive(true);
                    break;

                case "thispc":
                    osWindowTitle.text = "This PC — DHIO Workstation";
                    osTaskbarActiveApp.text = "This PC";
                    appThisPcObj.SetActive(true);
                    break;

                case "policy":
                    osWindowTitle.text = "PDF Reader — PrivacyPolicy_v4.2.pdf";
                    osTaskbarActiveApp.text = "PrivacyPolicy.pdf";
                    appPolicyObj.SetActive(true);
                    break;

                case "notes":
                    osWindowTitle.text = "Notepad — CaseNotes.txt";
                    osTaskbarActiveApp.text = "Notes.txt";
                    appNotesObj.SetActive(true);
                    break;

                case "recycle":
                    osWindowTitle.text = "Recycle Bin";
                    osTaskbarActiveApp.text = "Recycle Bin";
                    appRecycleObj.SetActive(true);
                    RefreshRecycleBinUI();
                    break;
            }
        }

        private void ShowBrowserTab(bool auditTab)
        {
            browserNewsTab.SetActive(!auditTab);
            browserAuditTab.SetActive(auditTab);

            if (!auditTab)
            {
                browserUrlText.text = "  https://news.health-wire.local/trending/vitapulse";
                return;
            }

            bool hasLetter = EvidenceManager.Instance != null && EvidenceManager.Instance.HasEvidence("letter_alex");
            browserUrlText.text = hasLetter
                ? "  [Not Secure]  http://truth-health-watch.blog/vitapulse-exposed"
                : "  about:blank  —  Enter a blog URL from Case 01 evidence";
            browserLockedBox.SetActive(!hasLetter);
            browserGameBox.SetActive(hasLetter);
            if (hasLetter) UpdateBlogInspectionState();
        }

        public void UnlockBookcaseBonusNote(string bookKey)
        {
            if (bookcaseStickyObj == null || bookcaseStickyText == null) return;
            Image bg = bookcaseStickyObj.GetComponent<Image>();
            if (bg != null) bg.color = new Color(0.98f, 0.91f, 0.48f, 0.96f);
            bookcaseStickyText.color = new Color(0.22f, 0.17f, 0.06f);
            bookcaseStickyText.text = bookKey == "myths"
                ? "<b>★ BOOKCASE TIP (Myths):</b>\nPromo codes = hidden ad! Check WHOIS & EU Registry for fake bans."
                : "<b>★ BOOKCASE TIP (Privacy):</b>\nWatch Policy v4.2 dates & Section 4.2 — vague sharing ≠ proven sale!";
        }

        private void SelectReferenceTab(int tabIdx)
        {
            activeRefTabIndex = tabIdx;
            if (policyDocRect != null) policyDocRect.gameObject.SetActive(tabIdx == 0);
            if (registryDocRect != null) registryDocRect.gameObject.SetActive(tabIdx == 1);
            if (rulesDocRect != null) rulesDocRect.gameObject.SetActive(tabIdx == 2);

            Color activeCol = new Color(0.945f, 0.918f, 0.865f, 1f); // Warm manila folder active tab!
            Color idleCol   = new Color(0.22f, 0.20f, 0.18f, 1f);    // Warm espresso idle tab
            Color activeTxt = new Color(0.16f, 0.14f, 0.12f, 1f);
            Color idleTxt   = new Color(0.80f, 0.76f, 0.70f, 1f);
            if (refTabPolicyBtn != null)
            {
                refTabPolicyBtn.GetComponent<Image>().color = tabIdx == 0 ? activeCol : idleCol;
                refTabPolicyBtn.GetComponentInChildren<Text>().color = tabIdx == 0 ? activeTxt : idleTxt;
            }
            if (refTabRegistryBtn != null)
            {
                refTabRegistryBtn.GetComponent<Image>().color = tabIdx == 1 ? activeCol : idleCol;
                refTabRegistryBtn.GetComponentInChildren<Text>().color = tabIdx == 1 ? activeTxt : idleTxt;
            }
            if (refTabRulesBtn != null)
            {
                refTabRulesBtn.GetComponent<Image>().color = tabIdx == 2 ? activeCol : idleCol;
                refTabRulesBtn.GetComponentInChildren<Text>().color = tabIdx == 2 ? activeTxt : idleTxt;
            }
        }

        private void ShowRedThreadBetween(RectTransform fromRt, RectTransform toRt)
        {
            if (redThreadRect == null || inspectionDeskRect == null || fromRt == null || toRt == null) return;
            Vector3 pA = inspectionDeskRect.InverseTransformPoint(fromRt.TransformPoint(fromRt.rect.center));
            Vector3 pB = inspectionDeskRect.InverseTransformPoint(toRt.TransformPoint(toRt.rect.center));
            Vector2 delta = new Vector2(pB.x - pA.x, pB.y - pA.y);
            float dist = delta.magnitude;
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

            redThreadRect.anchoredPosition = new Vector2((pA.x + pB.x) * 0.5f, (pA.y + pB.y) * 0.5f);
            redThreadRect.sizeDelta = new Vector2(dist, 3.5f);
            redThreadRect.localRotation = Quaternion.Euler(0f, 0f, angle);
            redThreadRect.SetAsLastSibling();
            redThreadRect.gameObject.SetActive(true);
            redThreadTimer = 1.6f;
        }

        private int GetActiveFileRequiredCount()
        {
            return activeQueueFileIndex == 0 ? 0 : (activeQueueFileIndex == 1 ? 2 : 5);
        }

        private int CountSolvedPairsForActiveFile()
        {
            if (activeQueueFileIndex == 0) return 0;
            if (activeQueueFileIndex == 1) return (pairSolved[5] ? 1 : 0) + (pairSolved[6] ? 1 : 0);
            int c = 0;
            for (int i = 0; i < 5; i++) if (pairSolved[i]) c++;
            return c;
        }

        private void OnLeftClaimClicked(int leftIdx)
        {
            bool alreadyCollected = EvidenceManager.Instance != null && EvidenceManager.Instance.HasEvidence("blog_unverified");
            if (alreadyCollected) return;

            LeftClaimItem[] claims = ActiveLeftClaims;
            if (leftIdx < 0 || leftIdx >= claims.Length) return;

            LeftClaimItem item = claims[leftIdx];
            if (item.pairIndex == -1)
            {
                selectedLeftIndex = -1;
                if (activeQueueFileIndex == 0)
                {
                    SoundManager.Instance?.Play("ui_click", 0.45f);
                    computerFeedbackText.color = new Color(0.16f, 0.44f, 0.32f);
                    computerFeedbackText.text = item.wrongHint;
                }
                else
                {
                    MissionManager.Instance?.RecordWrongBlogSelection();
                    computerFeedbackText.color = new Color(0.68f, 0.20f, 0.18f);
                    computerFeedbackText.text = item.wrongHint;
                }
                UpdateBlogInspectionState();
                return;
            }

            if (pairSolved[item.pairIndex]) return;

            SoundManager.Instance?.Play("ui_click", 0.5f);
            selectedLeftIndex = leftIdx;

            bool guided = PersonalizationManager.Instance != null && PersonalizationManager.Instance.IsGuidedMode(DhliDomain.Reliability);
            computerFeedbackText.color = new Color(0.14f, 0.36f, 0.56f);
            computerFeedbackText.text = guided
                ? $"Selected! Guided Tip: {item.wrongHint}"
                : "Claim selected! Drag/open Policy v4.2, WHOIS Registry, or Rulebook and click the contradicting fact.";
            UpdateBlogInspectionState();
        }

        private void OnRightClueClicked(int rightIdx)
        {
            bool alreadyCollected = EvidenceManager.Instance != null && EvidenceManager.Instance.HasEvidence("blog_unverified");
            if (alreadyCollected) return;

            if (activeQueueFileIndex == 0)
            {
                SoundManager.Instance?.Play("ui_click", 0.4f);
                computerFeedbackText.color = new Color(0.16f, 0.44f, 0.32f);
                computerFeedbackText.text = "File 1 (Official FAQ) matches all reference records! Stamp VERIFIED below to proceed to File 2.";
                return;
            }

            RightClueItem clue = rightClues[rightIdx];

            if (selectedLeftIndex < 0)
            {
                SoundManager.Instance?.Play("ui_click", 0.4f);
                computerFeedbackText.color = new Color(0.55f, 0.38f, 0.12f);
                computerFeedbackText.text = "First click a suspicious claim on DOC 1 (left), then match it to its contradicting proof!";
                return;
            }

            if (clue.pairIndex == -1)
            {
                MissionManager.Instance?.RecordWrongBlogSelection();
                computerFeedbackText.color = new Color(0.68f, 0.20f, 0.18f);
                computerFeedbackText.text = clue.wrongHint;
                UpdateBlogInspectionState();
                return;
            }

            LeftClaimItem chosen = ActiveLeftClaims[selectedLeftIndex];
            // Rule 1 (Evidence, rightIdx==5) disproves BOTH Blog Sources (pairIndex 4) AND Influencer Miracle Claim (pairIndex 5)
            bool isMatch = (chosen.pairIndex == clue.pairIndex) || (rightIdx == 5 && chosen.pairIndex == 5);
            if (isMatch)
            {
                ShowRedThreadBetween(
                    leftClaimButtons[selectedLeftIndex].GetComponent<RectTransform>(),
                    rightClueButtons[rightIdx].GetComponent<RectTransform>());
                pairSolved[chosen.pairIndex] = true;
                selectedLeftIndex = -1;
                SoundManager.Instance?.Play("evidence_unlock", 0.65f);
                int solvedCount = CountSolvedPairsForActiveFile();
                int req = GetActiveFileRequiredCount();
                computerFeedbackText.color = new Color(0.16f, 0.48f, 0.32f);
                computerFeedbackText.text = solvedCount == req
                    ? $"ALL {req} DISCREPANCIES EXPOSED! Now slam the official VERDICT STAMP below!"
                    : $"DISCREPANCY #{solvedCount} LOGGED ({solvedCount}/{req})! Check the remaining documents.";
            }
            else
            {
                MissionManager.Instance?.RecordWrongBlogSelection();
                computerFeedbackText.color = new Color(0.68f, 0.20f, 0.18f);
                computerFeedbackText.text = "No contradiction between those two! " + chosen.wrongHint;
            }
            UpdateBlogInspectionState();
        }

        private void OnVerdictStampClicked(string stampType)
        {
            bool alreadyCollected = EvidenceManager.Instance != null && EvidenceManager.Instance.HasEvidence("blog_unverified");
            if (alreadyCollected) return;

            int solved = CountSolvedPairsForActiveFile();
            int req = GetActiveFileRequiredCount();
            if (solved < req) return;

            if (activeQueueFileIndex == 0)
            {
                if (stampType == "verified")
                {
                    queueFileCompleted[0] = true;
                    activeQueueFileIndex = 1;
                    selectedLeftIndex = -1;
                    SoundManager.Instance?.Play("evidence_unlock", 0.7f);
                    PersonalizationManager.Instance?.RecordCorrectAction(DhliDomain.Reliability, 5);
                    computerFeedbackText.color = new Color(0.16f, 0.48f, 0.32f);
                    computerFeedbackText.text = "FILE 1 STAMPED [VERIFIED] ✓ (+5 Reliability)! Next up: File 2/3 — Viral Influencer Post!";
                    SelectReferenceTab(2); // Switch right binder cleanly to Rulebook for File 2!
                }
                else
                {
                    MissionManager.Instance?.RecordWrongBlogSelection();
                    computerFeedbackText.color = new Color(0.68f, 0.20f, 0.18f);
                    computerFeedbackText.text = "Careful! Every fact in VitaPulse's Official FAQ matched our records — stamp VERIFIED!";
                }
            }
            else if (activeQueueFileIndex == 1)
            {
                if (stampType == "misleading")
                {
                    queueFileCompleted[1] = true;
                    activeQueueFileIndex = 2;
                    selectedLeftIndex = -1;
                    SoundManager.Instance?.Play("evidence_unlock", 0.75f);
                    PersonalizationManager.Instance?.RecordCorrectAction(DhliDomain.Reliability, 5);
                    computerFeedbackText.color = new Color(0.16f, 0.48f, 0.32f);
                    computerFeedbackText.text = "FILE 2 STAMPED [MISLEADING AD] ✓ (+5 Reliability)! Final Boss File 3/3: Alex's Viral Blog!";
                    SelectReferenceTab(0); // Switch right binder cleanly to Policy v4.2 for File 3!
                }
                else
                {
                    MissionManager.Instance?.RecordWrongBlogSelection();
                    computerFeedbackText.color = new Color(0.68f, 0.20f, 0.18f);
                    computerFeedbackText.text = "Look at the promo code (VITA20) + fake 99% cure claim — that's a MISLEADING AD!";
                }
            }
            else
            {
                if (stampType == "unverified")
                {
                    queueFileCompleted[2] = true;
                    SoundManager.Instance?.Play("objection_sting", 0.75f);
                    MissionManager.Instance?.CompleteAlexBlogInspection();
                    computerFeedbackText.color = new Color(0.16f, 0.48f, 0.32f);
                    computerFeedbackText.text = "ALL 3 FILES AUDITED! [UNVERIFIED] stamped — Step Back & call Alex for OBJECTION!";
                }
                else if (stampType == "scam")
                {
                    MissionManager.Instance?.RecordWrongBlogSelection();
                    computerFeedbackText.color = new Color(0.68f, 0.20f, 0.18f);
                    computerFeedbackText.text = "Too fast! We don't have proof it's a scam — the blog itself is unverified!";
                }
                else
                {
                    MissionManager.Instance?.RecordWrongBlogSelection();
                    computerFeedbackText.color = new Color(0.68f, 0.20f, 0.18f);
                    computerFeedbackText.text = "Not so fast! Section 4.2 still has vague third-party sharing — don't stamp it 100% safe!";
                }
            }
            UpdateBlogInspectionState();
        }

        private void UpdateBlogInspectionState()
        {
            int solved = CountSolvedPairsForActiveFile();
            int req = GetActiveFileRequiredCount();
            bool collected = EvidenceManager.Instance != null && EvidenceManager.Instance.HasEvidence("blog_unverified");
            if (collected) activeQueueFileIndex = 2;

            // Queue progress header
            if (queueProgressText != null)
            {
                string f1 = queueFileCompleted[0] ? "<color=#69F0AE>[1] FAQ ✓</color>" : (activeQueueFileIndex == 0 ? "<color=#FFD54F><b>▶ [1] Official FAQ</b></color>" : "[1] FAQ");
                string f2 = queueFileCompleted[1] ? "<color=#69F0AE>[2] Influencer Ad ✓</color>" : (activeQueueFileIndex == 1 ? "<color=#FFD54F><b>▶ [2] Influencer Ad</b></color>" : "[2] Influencer Ad");
                string f3 = collected ? "<color=#69F0AE>[3] Alex's Blog ✓</color>" : (activeQueueFileIndex == 2 ? "<color=#FF8A80><b>▶ [3] Alex's Blog (Boss)</b></color>" : "[3] Alex's Blog");
                queueProgressText.text = $"DESK QUEUE:   {f1}   →   {f2}   →   {f3}";
            }

            if (leftDocHeaderText != null)
            {
                leftDocHeaderText.text = activeQueueFileIndex == 0
                    ? "<b>VITAPULSE HEALTH  •  Official Support Portal</b>"
                    : (activeQueueFileIndex == 1
                        ? "<b>HealthGram  •  Viral Health Reels</b>"
                        : "<b>TRUTH HEALTH WATCH  •  Independent Blog</b>");
            }

            if (leftHeroBannerImg != null)
            {
                leftHeroBannerImg.texture = activeQueueFileIndex == 0 ? heroBannerFaqTex
                    : (activeQueueFileIndex == 1 ? heroBannerSocialTex : heroBannerBlogTex);
            }

            if (leftFooterEngagementText != null)
            {
                leftFooterEngagementText.text = activeQueueFileIndex == 0
                    ? "✓ Verified Medical Organization  •  Last Reviewed: Oct 2026  •  Milan, IT"
                    : (activeQueueFileIndex == 1
                        ? "♥ 4.2M Likes   •   💬 18.4K Comments   •   ↗ 92K Shares   •   #ad?"
                        : "🔥 1,248 Shares  •  Top Comment: \"OMG deleting VitaPulse right now!!\"");
            }

            if (leftMastheadText != null && leftMastheadBg != null)
            {
                if (activeQueueFileIndex == 0)
                {
                    leftMastheadBg.color = new Color(0.11f, 0.34f, 0.32f, 0.16f);
                    leftMastheadText.color = new Color(0.10f, 0.34f, 0.30f, 1f);
                    leftMastheadText.text = "<b>🔒 vitapulse.com/faq</b>    •    Verified Medical Portal";
                }
                else if (activeQueueFileIndex == 1)
                {
                    leftMastheadBg.color = new Color(0.32f, 0.18f, 0.42f, 0.14f);
                    leftMastheadText.color = new Color(0.30f, 0.14f, 0.38f, 1f);
                    leftMastheadText.text = "<b>● @DrMiracleHealth</b>    •    Verified? NO   •   #VitaPulse #VITA20";
                }
                else
                {
                    leftMastheadBg.color = new Color(0.58f, 0.12f, 0.14f, 0.16f);
                    leftMastheadText.color = new Color(0.58f, 0.12f, 0.14f, 1f);
                    leftMastheadText.text = "<b>⚠ NOT SECURE</b>  •  truth-health-watch.blog   |   HOME  •  LEAKS  •  FORUM";
                }
            }

            LeftClaimItem[] claims = ActiveLeftClaims;
            for (int i = 0; i < leftClaimButtons.Length; i++)
            {
                if (i >= claims.Length)
                {
                    leftClaimButtons[i].gameObject.SetActive(false);
                    continue;
                }
                leftClaimButtons[i].gameObject.SetActive(true);

                // Editorial typography: Headline (row 0) is bigger like a real article title!
                leftClaimLabels[i].fontSize = (i == 0) ? 13 : 10;

                int pIdx = claims[i].pairIndex;
                bool isSolved = pIdx >= 0 && pairSolved[pIdx];
                bool isSelected = (selectedLeftIndex == i);

                if (isSolved)
                {
                    leftClaimLabels[i].text = "<color=#9E1C1C><b>✕</b></color>  " + claims[i].label;
                    leftClaimLabels[i].color = new Color(0.36f, 0.24f, 0.24f);
                    leftClaimBgs[i].color = new Color(0.62f, 0.14f, 0.16f, 0.14f);
                }
                else if (isSelected)
                {
                    leftClaimLabels[i].text = "<color=#8B1E1E><b>▶</b></color>  " + claims[i].label;
                    leftClaimLabels[i].color = new Color(0.10f, 0.10f, 0.10f);
                    leftClaimBgs[i].color = new Color(0.96f, 0.85f, 0.44f, 0.65f);
                }
                else
                {
                    leftClaimLabels[i].text = claims[i].label;
                    leftClaimLabels[i].color = new Color(0.12f, 0.13f, 0.15f);
                    leftClaimBgs[i].color = new Color(1f, 1f, 1f, 0.01f);
                }
            }

            for (int r = 0; r < rightClues.Length; r++)
            {
                int pIdx = rightClues[r].pairIndex;
                bool isSolvedForThisFile = activeQueueFileIndex == 1
                    ? ((r == 5 && pairSolved[5]) || (r == 6 && pairSolved[6]))
                    : (activeQueueFileIndex == 2 && pIdx >= 0 && pIdx < 5 && pairSolved[pIdx]);

                if (isSolvedForThisFile)
                {
                    rightClueLabels[r].text = "<color=#1F5E42><b>✓ VERIFIED</b></color>  " + rightClues[r].label;
                    rightClueLabels[r].color = new Color(0.15f, 0.28f, 0.21f);
                    rightClueBgs[r].color = new Color(0.16f, 0.42f, 0.31f, 0.16f);
                }
                else
                {
                    rightClueLabels[r].text = rightClues[r].label;
                    rightClueLabels[r].color = new Color(0.16f, 0.14f, 0.12f);
                    rightClueBgs[r].color = (selectedLeftIndex >= 0)
                        ? new Color(0.94f, 0.82f, 0.44f, 0.28f) // Soft warm highlighter invite!
                        : new Color(0.88f, 0.84f, 0.76f, 0.32f); // Subtle warm linen card on cream binder!
                }
            }

            // Update Physical Rubber Verdict Stamps dynamically per queue file!
            bool showStamps = (solved >= req) && !collected;
            if (stampBarObj != null) stampBarObj.SetActive(showStamps);
            if (showStamps && stampBtnA != null && stampBtnB != null && stampBtnC != null)
            {
                Color pineInk = new Color(0.14f, 0.36f, 0.27f, 1f);
                Color amberInk = new Color(0.58f, 0.35f, 0.10f, 1f);
                Color burgundyInk = new Color(0.56f, 0.14f, 0.15f, 1f);
                Color slateInk = new Color(0.24f, 0.27f, 0.32f, 1f);

                if (activeQueueFileIndex == 0)
                {
                    SetStampButton(stampBtnA, "[  VERIFIED  ]", pineInk, "verified");
                    SetStampButton(stampBtnB, "[  MISLEADING AD  ]", amberInk, "misleading");
                    SetStampButton(stampBtnC, "[  UNVERIFIED  ]", burgundyInk, "unverified");
                }
                else if (activeQueueFileIndex == 1)
                {
                    SetStampButton(stampBtnA, "[  VERIFIED  ]", slateInk, "verified");
                    SetStampButton(stampBtnB, "[  MISLEADING AD  ]", amberInk, "misleading");
                    SetStampButton(stampBtnC, "[  PROVEN SCAM  ]", burgundyInk, "scam");
                }
                else
                {
                    SetStampButton(stampBtnA, "[  PROVEN SCAM  ]", burgundyInk, "scam");
                    SetStampButton(stampBtnB, "[  UNVERIFIED — AUDIT 4.2  ]", pineInk, "unverified");
                    SetStampButton(stampBtnC, "[  100% SAFE  ]", slateInk, "safe");
                }
            }

            if (bigStampOverlayObj != null) bigStampOverlayObj.SetActive(collected);
            if (stepGuideText != null)
            {
                if (collected)
                    stepGuideText.text = "<color=#69F0AE><b>✓ ALL 3 FILES AUDITED!</b>  Click '↓ Step Back' and call Alex on your Office Phone!</color>";
                else if (activeQueueFileIndex == 0)
                    stepGuideText.text = "<color=#FFD54F><b>HOW TO PLAY (FILE 1/3):</b></color>  Compare LEFT vs RIGHT (switch tabs). All 4 lines are TRUE → click <b>STAMP: VERIFIED ✓</b> below!";
                else if (solved >= req)
                    stepGuideText.text = "<color=#69F0AE><b>STEP 3:</b></color>  All lies caught! Click the correct <b>VERDICT STAMP</b> at the bottom!";
                else if (selectedLeftIndex < 0)
                    stepGuideText.text = $"<color=#FFD54F><b>STEP 1:</b></color>  Click a <b>suspicious claim on the LEFT</b>  →  then find its proof on the RIGHT ({solved}/{req} caught)";
                else
                    stepGuideText.text = $"<color=#80D8FF><b>STEP 2:</b></color>  Claim selected! Now switch RIGHT tabs (<b>Policy / Registry / Rulebook</b>) &amp; click the <b>contradicting fact</b>!";
            }

            if (stampStatusText != null)
            {
                if (collected)
                    stampStatusText.text = "<color=#1F5E42><b>QUEUE COMPLETE (3/3): UNVERIFIED BLOG STAMPED — Step Back & Call Alex!</b></color>";
                else if (activeQueueFileIndex == 0)
                    stampStatusText.text = "<b>FILE 1/3 (WARM-UP):</b>  Check facts against Policy & Registry — if clean, stamp VERIFIED!";
                else if (solved >= req)
                    stampStatusText.text = $"<b>FILE {activeQueueFileIndex + 1}/3 READY — SLAM THE OFFICIAL VERDICT STAMP:</b>";
                else
                    stampStatusText.text = $"<b>FILE {activeQueueFileIndex + 1}/3 AUDIT:</b>  Expose all contradictions across the 4 papers  (<b>{solved} / {req}</b>)";
            }

            if (browserModeHintText != null && PersonalizationManager.Instance != null)
            {
                var pm = PersonalizationManager.Instance;
                if (pm.IsGuidedMode(DhliDomain.Reliability))
                {
                    browserModeHintText.color = new Color(0.58f, 0.18f, 0.16f);
                    browserModeHintText.text = activeQueueFileIndex == 0
                        ? "<b>[GUIDED]</b> File 1 has zero lies — verify it against Policy v4.2 & click STAMP: VERIFIED!"
                        : (activeQueueFileIndex == 1
                            ? "<b>[GUIDED]</b> File 2: Match Miracle Claim ↔ Rule 1, and Code VITA20 ↔ Rule 2 (Rulebook)."
                            : "<b>[GUIDED]</b> File 3: Cross-check Blog against Policy v4.2, WHOIS & EU Registry, and Rulebook (5 pairs).");
                }
                else
                {
                    browserModeHintText.color = new Color(0.24f, 0.28f, 0.34f);
                    browserModeHintText.text = activeQueueFileIndex == 0
                        ? "<b>[CASE QUEUE 1/3]</b> Not every file is fake! Verify the Official FAQ before stamping."
                        : (activeQueueFileIndex == 1
                            ? "<b>[CASE QUEUE 2/3]</b> Inspect @DrMiracleHealth for hidden ads & unsupported miracle claims (2 lies)."
                            : "<b>[CASE QUEUE 3/3]</b> Final Boss: Expose all 5 lies in Alex's viral blog across the 4 papers!");
                }
            }
        }

        private void SetStampButton(Button btn, string label, Color inkCol, string stampKey)
        {
            Text txt = btn.GetComponentInChildren<Text>();
            txt.text = label;
            txt.color = inkCol;
            txt.fontStyle = FontStyle.Bold;
            txt.fontSize = 12;
            btn.GetComponent<Image>().color = new Color(0.95f, 0.92f, 0.85f, 1f); // Aged stamp paper fill
            Outline outl = btn.GetComponent<Outline>();
            if (outl == null) outl = btn.gameObject.AddComponent<Outline>();
            outl.effectColor = inkCol;
            outl.effectDistance = new Vector2(2f, -2f);
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnVerdictStampClicked(stampKey));
        }

        private void RefreshRecycleBinUI()
        {
            if (recycleListText == null) return;
            if (recycleBinEmptied)
            {
                recycleListText.text =
                    "<b>Recycle Bin is empty.</b>\n\n" +
                    "<i>All suspicious spam drafts and clickbait ads have been permanently shredded.</i>";
                if (emptyRecycleBtn != null)
                {
                    emptyRecycleBtn.interactable = false;
                    emptyRecycleBtn.GetComponentInChildren<Text>().text = "Recycle Bin Empty";
                }
            }
            else
            {
                recycleListText.text =
                    "<b>Deleted Items (3):</b>\n\n" +
                    "•  <b>miracle_lemon_cure_FINAL.pdf</b>   <i>(420 KB — Deleted Yesterday)</i>\n" +
                    "•  <b>vitapulse_popup_ad_banner.tmp</b>   <i>(1.2 MB — Blocked by Firewall)</i>\n" +
                    "•  <b>old_password_1234.txt</b>   <i>(1 KB — Security Risk!)</i>\n\n" +
                    "<i>Tip: Keeping your desktop clean is good digital hygiene.</i>";
                if (emptyRecycleBtn != null)
                {
                    emptyRecycleBtn.interactable = true;
                    emptyRecycleBtn.GetComponentInChildren<Text>().text = "Empty Recycle Bin";
                }
            }
        }

        private void BuildWorldSpaceDesktop()
        {
            const float resW = 1024f;
            const float resH = 600f;

            GameObject canvasGo = new GameObject("WorldSpace_HealthOS_Canvas");
            canvasGo.transform.SetParent(transform, false);
            canvasGo.transform.localPosition = Vector3.zero;
            canvasGo.transform.localRotation = Quaternion.identity;

            worldCanvas = canvasGo.AddComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.WorldSpace;
            worldCanvas.worldCamera = Camera.main;
            worldRaycaster = canvasGo.AddComponent<GraphicRaycaster>();
            worldRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
            worldRaycaster.enabled = false;

            RectTransform crt = canvasGo.GetComponent<RectTransform>();
            crt.sizeDelta = new Vector2(resW, resH);
            crt.localScale = new Vector3(screenWorldSize.x / resW, screenWorldSize.y / resH, 1f);

            // 1. Realistic High-Res Office Desktop Wallpaper (loads Resources/Textures/os_wallpaper, fallback to gradient)
            GameObject wallGo = new GameObject("OS_Wallpaper");
            wallGo.transform.SetParent(canvasGo.transform, false);
            RectTransform wall = wallGo.AddComponent<RectTransform>();
            SetFill(wall, 0, 0);
            RawImage wallImg = wallGo.AddComponent<RawImage>();
            Texture2D realWall = Resources.Load<Texture2D>("Textures/os_wallpaper");
            wallImg.texture = realWall != null ? realWall : CreateDesktopWallpaperTexture();
            wallImg.raycastTarget = false;

            // Subtle Desktop Widget (Top-Right: Today's Office Status & Date — makes desktop feel alive!)
            RectTransform widget = CreatePanel(wall, "DesktopWidget", new Color(0.08f, 0.11f, 0.16f, 0.58f), raycast: false, rounded: true);
            widget.anchorMin = new Vector2(1f, 1f); widget.anchorMax = new Vector2(1f, 1f); widget.pivot = new Vector2(1f, 1f);
            widget.sizeDelta = new Vector2(235f, 82f);
            widget.anchoredPosition = new Vector2(-18f, -18f);
            Text widTxt = CreateText(widget, "WidTxt",
                "<b>DHIO WORKSTATION</b>\n" +
                "<color=#A8DADC>Network: Secured (PoliMi)</color>\n" +
                "<i>3 viral VitaPulse alerts in Browser</i>",
                12, new Color(0.94f, 0.96f, 0.98f), TextAnchor.MiddleLeft);
            SetFill(widTxt.GetComponent<RectTransform>(), 14f, 8f);

            // 2. Authentic Desktop Icons (2 clean left columns — transparent hover, real graphic icons!)
            BuildDesktopIcon(wall, "This PC",           new Vector2(18f,  -18f),  CreateMonitorIcon(), () => OpenOSApp("thispc"));
            BuildDesktopIcon(wall, "Browser",           new Vector2(18f,  -118f), CreateBrowserIcon(), () => OpenOSApp("browser"));
            BuildDesktopIcon(wall, "Case Files",        new Vector2(18f,  -218f), CreateFolderIcon(),  () => OpenOSApp("files"));
            BuildDesktopIcon(wall, "Mail",              new Vector2(18f,  -318f), CreateMailIcon(),    () => OpenOSApp("mail"));
            BuildDesktopIcon(wall, "Recycle Bin",       new Vector2(18f,  -418f), CreateTrashIcon(),   () => OpenOSApp("recycle"));

            BuildDesktopIcon(wall, "PrivacyPolicy.pdf", new Vector2(118f, -18f),  CreatePdfIcon(),     () => OpenOSApp("policy"));
            BuildDesktopIcon(wall, "CaseNotes.txt",     new Vector2(118f, -118f), CreateNotesIcon(),   () => OpenOSApp("notes"));

            // 3. Application Window Frame (Warm Charcoal Editorial OS Window — zero cold navy!)
            RectTransform win = CreatePanel(wall, "OS_AppWindow", new Color(0.145f, 0.135f, 0.125f, 0.99f), raycast: true, rounded: true);
            win.anchorMin = new Vector2(0.01f, 0.074f);
            win.anchorMax = new Vector2(0.99f, 0.99f);
            win.offsetMin = Vector2.zero; win.offsetMax = Vector2.zero;
            Shadow winShadow = win.gameObject.AddComponent<Shadow>();
            winShadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
            winShadow.effectDistance = new Vector2(6f, -8f);
            Outline winBorder = win.gameObject.AddComponent<Outline>();
            winBorder.effectColor = new Color(0.18f, 0.22f, 0.28f, 0.55f);
            winBorder.effectDistance = new Vector2(1f, -1f);

            RectTransform titleBar = CreatePanel(win, "WinTitleBar", new Color(0.14f, 0.17f, 0.22f, 1f), raycast: false, rounded: false);
            titleBar.anchorMin = new Vector2(0f, 1f); titleBar.anchorMax = new Vector2(1f, 1f); titleBar.pivot = new Vector2(0.5f, 1f);
            titleBar.sizeDelta = new Vector2(0f, 34f); titleBar.anchoredPosition = Vector2.zero;

            osWindowTitle = CreateText(titleBar, "WinTitle", "Window", 13, Color.white, TextAnchor.MiddleLeft);
            osWindowTitle.fontStyle = FontStyle.Bold;
            SetFill(osWindowTitle.GetComponent<RectTransform>(), 14, 2);

            // Minimize (—) & Close (X) window buttons
            Button winMin = CreateButton(titleBar, "WinMin", "—", new Color(0.24f, 0.28f, 0.35f, 1f), 24f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.45f);
                osWindowFrame.SetActive(false);
                osTaskbarActiveApp.text = "Desktop";
            });
            RectTransform wmRect = winMin.GetComponent<RectTransform>();
            wmRect.anchorMin = new Vector2(1f, 0.5f); wmRect.anchorMax = new Vector2(1f, 0.5f); wmRect.pivot = new Vector2(1f, 0.5f);
            wmRect.sizeDelta = new Vector2(30f, 24f); wmRect.anchoredPosition = new Vector2(-42f, 0f);

            Button winClose = CreateButton(titleBar, "WinCloseX", "✕", new Color(0.74f, 0.20f, 0.20f, 1f), 24f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                osWindowFrame.SetActive(false);
                osTaskbarActiveApp.text = "Desktop";
            });
            RectTransform wcRect = winClose.GetComponent<RectTransform>();
            wcRect.anchorMin = new Vector2(1f, 0.5f); wcRect.anchorMax = new Vector2(1f, 0.5f); wcRect.pivot = new Vector2(1f, 0.5f);
            wcRect.sizeDelta = new Vector2(32f, 24f); wcRect.anchoredPosition = new Vector2(-6f, 0f);

            RectTransform winBody = CreatePanel(win, "WinBody", new Color(0, 0, 0, 0), raycast: false, rounded: false);
            winBody.anchorMin = Vector2.zero; winBody.anchorMax = Vector2.one;
            winBody.offsetMin = new Vector2(10, 10); winBody.offsetMax = new Vector2(-10, -40);

            BuildBrowserApp(winBody);
            BuildExplorerApp(winBody);
            BuildMailApp(winBody);
            BuildThisPcApp(winBody);
            BuildPolicyApp(winBody);
            BuildNotesApp(winBody);
            BuildRecycleBinApp(winBody);

            osWindowFrame = win.gameObject;

            // 4. Floating Start Menu Popup
            BuildStartMenuPopup(wall);

            // 5. Realistic Windows-style Taskbar
            BuildTaskbar(wall);
        }

        private void BuildDesktopIcon(Transform parent, string label, Vector2 pos, Texture2D iconTex, UnityEngine.Events.UnityAction onClick)
        {
            GameObject go = new GameObject("Icon_" + label);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(0f, 1f); rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(88f, 88f);
            rt.anchoredPosition = pos;

            Image bg = go.AddComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 0f); // Transparent at rest!
            if (roundedSprite != null) { bg.sprite = roundedSprite; bg.type = Image.Type.Sliced; }

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(1f, 1f, 1f, 0f);
            cb.highlightedColor = new Color(1f, 1f, 1f, 0.18f);
            cb.pressedColor = new Color(0.75f, 0.88f, 1f, 0.30f);
            cb.fadeDuration = 0.08f;
            btn.colors = cb;
            btn.onClick.AddListener(onClick);

            GameObject imgGo = new GameObject("Glyph");
            imgGo.transform.SetParent(go.transform, false);
            RectTransform imgRt = imgGo.AddComponent<RectTransform>();
            imgRt.anchorMin = new Vector2(0.5f, 1f); imgRt.anchorMax = new Vector2(0.5f, 1f); imgRt.pivot = new Vector2(0.5f, 1f);
            imgRt.sizeDelta = new Vector2(46f, 46f);
            imgRt.anchoredPosition = new Vector2(0f, -6f);
            RawImage raw = imgGo.AddComponent<RawImage>();
            raw.texture = iconTex;
            raw.raycastTarget = false;

            Text lbl = CreateText(go.transform, "Lbl", label, 11, Color.white, TextAnchor.UpperCenter);
            RectTransform lRt = lbl.GetComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0f, 0f); lRt.anchorMax = new Vector2(1f, 0f); lRt.pivot = new Vector2(0.5f, 0f);
            lRt.sizeDelta = new Vector2(0f, 30f); lRt.anchoredPosition = new Vector2(0f, 2f);
            Shadow sh = lbl.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.85f);
            sh.effectDistance = new Vector2(1f, -1f);
        }

        private void BuildBrowserApp(Transform parent)
        {
            GameObject root = new GameObject("App_Browser");
            root.transform.SetParent(parent, false);
            SetFill(root.AddComponent<RectTransform>(), 0, 0);
            VerticalLayoutGroup vLay = root.AddComponent<VerticalLayoutGroup>();
            vLay.spacing = 6; vLay.childControlHeight = false; vLay.childForceExpandHeight = false;

            // Sleek Chrome-Style Browser Tab Strip (neutral dark/light tabs — zero clashing colors!)
            GameObject tabBar = new GameObject("BrowserTabs");
            tabBar.transform.SetParent(root.transform, false);
            tabBar.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 26f);
            HorizontalLayoutGroup tLay = tabBar.AddComponent<HorizontalLayoutGroup>();
            tLay.spacing = 4; tLay.childControlWidth = true; tLay.childForceExpandWidth = true;

            CreateButton(tabBar.transform, "TabNews", "HealthWire News", new Color(0.18f, 0.21f, 0.25f, 1f), 24f, () => ShowBrowserTab(false), new Color(0.76f, 0.80f, 0.85f));
            CreateButton(tabBar.transform, "TabAudit", "Case 01: Fact-Check Workspace", new Color(0.24f, 0.29f, 0.35f, 1f), 24f, () => ShowBrowserTab(true), Color.white);
            CreateButton(tabBar.transform, "TabPol", "PrivacyPolicy_v4.2.pdf", new Color(0.18f, 0.21f, 0.25f, 1f), 24f, () => OpenOSApp("policy"), new Color(0.76f, 0.80f, 0.85f));

            // Compact Dark Address Bar with Back/Forward/Reload
            GameObject navRow = new GameObject("BrowserNavRow");
            navRow.transform.SetParent(root.transform, false);
            navRow.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 24f);
            HorizontalLayoutGroup navLay = navRow.AddComponent<HorizontalLayoutGroup>();
            navLay.spacing = 5; navLay.childControlWidth = true; navLay.childForceExpandWidth = false;
            navLay.childControlHeight = true; navLay.childForceExpandHeight = true;

            Button backNav = CreateButton(navRow.transform, "NavBack", "←", new Color(0.20f, 0.23f, 0.28f, 1f), 22f, () => ShowBrowserTab(false), new Color(0.84f, 0.88f, 0.92f));
            backNav.GetComponent<LayoutElement>().minWidth = 26f; backNav.GetComponent<LayoutElement>().preferredWidth = 26f;

            Button fwdNav = CreateButton(navRow.transform, "NavFwd", "→", new Color(0.20f, 0.23f, 0.28f, 1f), 22f, () => ShowBrowserTab(true), new Color(0.84f, 0.88f, 0.92f));
            fwdNav.GetComponent<LayoutElement>().minWidth = 26f; fwdNav.GetComponent<LayoutElement>().preferredWidth = 26f;

            Button relNav = CreateButton(navRow.transform, "NavReload", "↻", new Color(0.20f, 0.23f, 0.28f, 1f), 22f, () => SoundManager.Instance?.Play("ui_click", 0.35f), new Color(0.84f, 0.88f, 0.92f));
            relNav.GetComponent<LayoutElement>().minWidth = 26f; relNav.GetComponent<LayoutElement>().preferredWidth = 26f;

            RectTransform urlBar = CreatePanel(navRow.transform, "UrlBar", new Color(0.09f, 0.11f, 0.14f, 1f), raycast: false, rounded: true);
            LayoutElement urlLe = urlBar.gameObject.AddComponent<LayoutElement>();
            urlLe.minWidth = 240f; urlLe.flexibleWidth = 1f;
            Outline urlOut = urlBar.gameObject.AddComponent<Outline>();
            urlOut.effectColor = new Color(0.24f, 0.28f, 0.34f, 1f);
            urlOut.effectDistance = new Vector2(1f, -1f);
            browserUrlText = CreateText(urlBar, "UrlTxt", "  https://news.health-wire.local/trending/vitapulse", 11, new Color(0.82f, 0.87f, 0.92f), TextAnchor.MiddleLeft);
            if (monoFont != null) browserUrlText.font = monoFont;
            SetFill(browserUrlText.GetComponent<RectTransform>(), 8, 1);

            // Viewport container (generous height now that redundant stacked headers are gone!)
            GameObject viewport = new GameObject("BrowserViewport");
            viewport.transform.SetParent(root.transform, false);
            viewport.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 448f);

            // Tab 1: Realistic Editorial News Website ("HEALTHWIRE DAILY")
            RectTransform newsPage = CreatePanel(viewport.transform, "NewsPage", new Color(0.955f, 0.945f, 0.925f, 1f), raycast: false, rounded: false);
            SetFill(newsPage, 0, 0);
            VerticalLayoutGroup npLay = newsPage.gameObject.AddComponent<VerticalLayoutGroup>();
            npLay.padding = new RectOffset(0, 0, 0, 8); npLay.spacing = 6;
            npLay.childControlHeight = false; npLay.childForceExpandHeight = false;

            // Website Top Masthead (NYT / BBC Health editorial look!)
            RectTransform mastBar = CreatePanel(newsPage, "NewsMasthead", new Color(0.11f, 0.13f, 0.17f, 1f), raycast: false, rounded: false);
            mastBar.sizeDelta = new Vector2(0f, 44f);
            Text brandTxt = CreateText(mastBar, "Brand", "<b>HEALTHWIRE DAILY</b>", 18, new Color(0.98f, 0.96f, 0.92f), TextAnchor.MiddleLeft);
            SetFill(brandTxt.GetComponent<RectTransform>(), 16, 4);
            Text navLinks = CreateText(mastBar, "NavLinks", "WORLD   •   <b>DIGITAL HEALTH</b>   •   PRIVACY   •   FACT-CHECK   |   MILAN, NOV 26", 10, new Color(0.72f, 0.78f, 0.84f), TextAnchor.MiddleRight);
            SetFill(navLinks.GetComponent<RectTransform>(), 16, 4);

            // Live Breaking News Ticker
            RectTransform tickerBar = CreatePanel(newsPage, "BreakingTicker", new Color(0.56f, 0.14f, 0.15f, 1f), raycast: false, rounded: false);
            tickerBar.sizeDelta = new Vector2(0f, 22f);
            Text tickTxt = CreateText(tickerBar, "TickTxt", "<b>● LIVE</b>   VitaPulse surpasses 2.1M European downloads in 48 hours as online privacy rumors spark public debate", 10, Color.white, TextAnchor.MiddleLeft);
            SetFill(tickTxt.GetComponent<RectTransform>(), 16, 2);

            // 2-Column Editorial News Grid
            GameObject newsCols = new GameObject("NewsGrid");
            newsCols.transform.SetParent(newsPage, false);
            newsCols.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 360f);
            HorizontalLayoutGroup ncLay = newsCols.AddComponent<HorizontalLayoutGroup>();
            ncLay.padding = new RectOffset(14, 14, 4, 4); ncLay.spacing = 14;
            ncLay.childControlWidth = true; ncLay.childForceExpandWidth = false;
            ncLay.childControlHeight = true; ncLay.childForceExpandHeight = true;

            // LEFT COLUMN (62%): Featured Lead Article
            RectTransform leadCol = CreatePanel(newsCols.transform, "LeadArticle", new Color(0.99f, 0.985f, 0.97f, 1f), raycast: false, rounded: true);
            Outline leadOut = leadCol.gameObject.AddComponent<Outline>();
            leadOut.effectColor = new Color(0.80f, 0.78f, 0.74f, 1f); leadOut.effectDistance = new Vector2(1f, -1f);
            LayoutElement leadLe = leadCol.gameObject.AddComponent<LayoutElement>();
            leadLe.minWidth = 420f; leadLe.flexibleWidth = 1.55f;
            VerticalLayoutGroup lColLay = leadCol.gameObject.AddComponent<VerticalLayoutGroup>();
            lColLay.padding = new RectOffset(14, 14, 12, 12); lColLay.spacing = 8;
            lColLay.childControlHeight = false; lColLay.childForceExpandHeight = false;

            GameObject newsHeroGo = new GameObject("NewsHeroImg");
            newsHeroGo.transform.SetParent(leadCol, false);
            newsHeroGo.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 76f);
            RawImage newsHeroRaw = newsHeroGo.AddComponent<RawImage>();
            newsHeroRaw.texture = CreateHeroBannerTexture(0);
            newsHeroRaw.raycastTarget = false;

            Text leadCat = CreateText(leadCol, "LeadCat", "<b><color=#7D2629>FEATURED REPORT  •  DIGITAL HEALTH</color></b>", 10, new Color(0.49f, 0.15f, 0.16f), TextAnchor.MiddleLeft);
            leadCat.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 16f);

            Text leadTitle = CreateText(leadCol, "LeadTitle", "<b>VitaPulse Mania: Miracle Health Companion or Privacy Blindspot?</b>", 18, new Color(0.11f, 0.12f, 0.14f), TextAnchor.UpperLeft);
            leadTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 48f);

            Text leadMeta = CreateText(leadCol, "LeadMeta", "<i>By Dr. Elena Conti  •  Digital Health Desk  •  Updated 12 mins ago</i>", 10, new Color(0.42f, 0.40f, 0.38f), TextAnchor.MiddleLeft);
            leadMeta.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 16f);

            Text leadBody = CreateText(leadCol, "LeadBody",
                "Millions of citizens have downloaded the symptom and sleep diary app <b>VitaPulse</b> this week following viral social reels. " +
                "Meanwhile, an anonymous whistleblower blog is accusing the company of selling private medical records.\n\n" +
                "Health literacy experts urge users to verify claims against official documentation before panicking — or trusting miracle ads.",
                12, new Color(0.18f, 0.19f, 0.21f), TextAnchor.UpperLeft);
            leadBody.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 108f);

            Button openAuditBtn = CreateButton(leadCol, "OpenCase01FromNews", "Open Case 01: Fact-Check Workspace  →", new Color(0.16f, 0.34f, 0.38f, 1f), 34f, () => ShowBrowserTab(true), Color.white);
            openAuditBtn.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;

            // RIGHT COLUMN (38%): Trending Sidebar Cards
            GameObject sideCol = new GameObject("TrendingSidebar");
            sideCol.transform.SetParent(newsCols.transform, false);
            sideCol.AddComponent<RectTransform>();
            LayoutElement sideLe = sideCol.AddComponent<LayoutElement>();
            sideLe.minWidth = 260f; sideLe.flexibleWidth = 1f;
            VerticalLayoutGroup sColLay = sideCol.AddComponent<VerticalLayoutGroup>();
            sColLay.spacing = 8; sColLay.childControlHeight = false; sColLay.childForceExpandHeight = false;

            Text sideHdr = CreateText(sideCol.transform, "SideHdr", "<b>TRENDING STORIES</b>", 12, new Color(0.49f, 0.15f, 0.16f), TextAnchor.MiddleLeft);
            sideHdr.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 20f);

            CreateNewsSidebarCard(sideCol.transform, "VIRAL REEL  •  4.2M VIEWS",
                "Influencer @DrMiracleHealth claims VitaPulse 'prevents 99% of heart attacks' — critics spot promo code VITA20.",
                new Color(0.32f, 0.18f, 0.42f));

            CreateNewsSidebarCard(sideCol.transform, "UNVERIFIED BLOG  •  RUMOR",
                "Anonymous site 'TruthHealthWatch' alleges VitaPulse was banned by the EU Health Ministry.",
                new Color(0.58f, 0.14f, 0.15f));

            CreateNewsSidebarCard(sideCol.transform, "INVESTIGATOR BRIEFING",
                "Collect Alex Moretti's yellow envelope from Cabinet Drawer B-02 to unlock the full Case 01 Fact-Check Workspace!",
                new Color(0.14f, 0.36f, 0.28f));

            browserNewsTab = newsPage.gameObject;

            // Tab 2: Blog Audit
            GameObject auditRoot = new GameObject("AuditTab");
            auditRoot.transform.SetParent(viewport.transform, false);
            SetFill(auditRoot.AddComponent<RectTransform>(), 0, 0);

            RectTransform lockBox = CreatePanel(auditRoot.transform, "BrowserLocked", new Color(0.93f, 0.91f, 0.86f, 1f), raycast: false, rounded: true);
            SetFill(lockBox, 24, 55);
            Text lTxt = CreateText(lockBox, "LT",
                "<b><color=#7D2629>NO BLOG URL UNLOCKED YET</color></b>\n\n" +
                "1. Answer the Office Phone (Supervisor → Manual → Alex).\n" +
                "2. Check the Mail Delivery Log on your desk.\n" +
                "3. Open Cabinet Drawer B-02 and collect Alex's Yellow Envelope!",
                15, new Color(0.15f, 0.16f, 0.18f), TextAnchor.MiddleCenter);
            SetFill(lTxt.GetComponent<RectTransform>(), 18, 14);
            browserLockedBox = lockBox.gameObject;

            GameObject gameBox = new GameObject("BrowserGame");
            gameBox.transform.SetParent(auditRoot.transform, false);
            SetFill(gameBox.AddComponent<RectTransform>(), 0, 0);
            VerticalLayoutGroup gLay = gameBox.AddComponent<VerticalLayoutGroup>();
            gLay.spacing = 6; gLay.childControlHeight = false; gLay.childForceExpandHeight = false;

            // Single Sleek Top Queue + Step Bar (replaces the two stacked black pills!)
            RectTransform topStrip = CreatePanel(gameBox.transform, "QueueRibbon", new Color(0.16f, 0.18f, 0.21f, 1f), raycast: false, rounded: false);
            topStrip.sizeDelta = new Vector2(0f, 24f);
            HorizontalLayoutGroup tsLay = topStrip.gameObject.AddComponent<HorizontalLayoutGroup>();
            tsLay.padding = new RectOffset(10, 10, 0, 0); tsLay.spacing = 12;
            tsLay.childControlWidth = true; tsLay.childForceExpandWidth = false;
            tsLay.childControlHeight = true; tsLay.childForceExpandHeight = true;

            queueProgressText = CreateText(topStrip, "QueueTxt", "QUEUE: [1] FAQ → [2] Ad → [3] Blog", 10, new Color(0.92f, 0.94f, 0.96f), TextAnchor.MiddleLeft);
            LayoutElement qLe = queueProgressText.gameObject.AddComponent<LayoutElement>();
            qLe.minWidth = 340f; qLe.preferredWidth = 370f;

            stepGuideText = CreateText(topStrip, "StepTxt", "STEP 1: Highlight a claim on the LEFT → STEP 2: Click proof in DHIO Terminal!", 10, new Color(0.98f, 0.90f, 0.62f), TextAnchor.MiddleRight);
            LayoutElement sgLe = stepGuideText.gameObject.AddComponent<LayoutElement>();
            sgLe.minWidth = 300f; sgLe.flexibleWidth = 1f;

            // Split-Screen Warm Walnut Detective Workspace: LEFT = Illustrated Real Website, RIGHT = Warm Manila Case Binder!
            RectTransform deskArea = CreatePanel(gameBox.transform, "InspectionDeskArea", new Color(0.135f, 0.120f, 0.110f, 1f), raycast: false, rounded: false);
            deskArea.sizeDelta = new Vector2(0f, 344f);
            inspectionDeskRect = deskArea;

            // Generate 3 crisp procedural Hero Banner graphics (Medical Portal, Social Reel, Conspiracy Blog!)
            heroBannerFaqTex    = CreateHeroBannerTexture(0);
            heroBannerSocialTex = CreateHeroBannerTexture(1);
            heroBannerBlogTex   = CreateHeroBannerTexture(2);

            // Right-Side Warm Archival Case Binder Tab Strip ("OFFICIAL DOSSIER" with cream folder divider tabs!)
            RectTransform rightAppBar = CreatePanel(deskArea, "RightAppTitleBar", new Color(0.18f, 0.16f, 0.145f, 1f), raycast: false, rounded: false);
            rightAppBar.anchorMin = new Vector2(0.505f, 1f); rightAppBar.anchorMax = new Vector2(0.992f, 1f); rightAppBar.pivot = new Vector2(0.5f, 1f);
            rightAppBar.sizeDelta = new Vector2(0f, 26f); rightAppBar.anchoredPosition = new Vector2(0f, -4f);

            Text termLbl = CreateText(rightAppBar, "TermLbl", "<b>CASE DOSSIER</b>", 10, new Color(0.88f, 0.80f, 0.66f), TextAnchor.MiddleLeft);
            RectTransform tlRt = termLbl.GetComponent<RectTransform>();
            tlRt.anchorMin = new Vector2(0f, 0f); tlRt.anchorMax = new Vector2(0.30f, 1f); tlRt.offsetMin = new Vector2(8f, 0f); tlRt.offsetMax = Vector2.zero;

            GameObject refTabBar = new GameObject("RightRefTabs");
            refTabBar.transform.SetParent(rightAppBar, false);
            RectTransform rtbRt = refTabBar.AddComponent<RectTransform>();
            rtbRt.anchorMin = new Vector2(0.30f, 0.08f); rtbRt.anchorMax = new Vector2(0.99f, 0.94f);
            rtbRt.offsetMin = Vector2.zero; rtbRt.offsetMax = Vector2.zero;
            HorizontalLayoutGroup rtLay = refTabBar.AddComponent<HorizontalLayoutGroup>();
            rtLay.spacing = 3; rtLay.childControlWidth = true; rtLay.childForceExpandWidth = true;

            refTabPolicyBtn = CreateButton(refTabBar.transform, "TabPol", "Policy v4.2", new Color(0.945f, 0.918f, 0.865f, 1f), 20f, () => { SoundManager.Instance?.Play("paper_rustle", 0.5f); SelectReferenceTab(0); }, new Color(0.16f, 0.14f, 0.12f));
            refTabPolicyBtn.GetComponentInChildren<Text>().fontSize = 10;
            refTabRegistryBtn = CreateButton(refTabBar.transform, "TabReg", "WHOIS & EU", new Color(0.22f, 0.20f, 0.18f, 1f), 20f, () => { SoundManager.Instance?.Play("paper_rustle", 0.5f); SelectReferenceTab(1); }, new Color(0.80f, 0.76f, 0.70f));
            refTabRegistryBtn.GetComponentInChildren<Text>().fontSize = 10;
            refTabRulesBtn = CreateButton(refTabBar.transform, "TabRul", "Rulebook", new Color(0.22f, 0.20f, 0.18f, 1f), 20f, () => { SoundManager.Instance?.Play("paper_rustle", 0.5f); SelectReferenceTab(2); }, new Color(0.80f, 0.76f, 0.70f));
            refTabRulesBtn.GetComponentInChildren<Text>().fontSize = 10;
            Button resetPosBtn = CreateButton(refTabBar.transform, "ResetPos", "↺", new Color(0.26f, 0.23f, 0.20f, 1f), 20f, () =>
            {
                SoundManager.Instance?.Play("paper_rustle", 0.45f);
                if (blogDocRect != null) blogDocRect.anchoredPosition = new Vector2(-246f, 6f);
                if (policyDocRect != null) policyDocRect.anchoredPosition = new Vector2(246f, 0f);
                if (registryDocRect != null) registryDocRect.anchoredPosition = new Vector2(246f, 0f);
                if (rulesDocRect != null) rulesDocRect.anchoredPosition = new Vector2(246f, 0f);
            });
            resetPosBtn.GetComponentInChildren<Text>().fontSize = 11;
            resetPosBtn.GetComponent<LayoutElement>().flexibleWidth = 0.35f;

            // Adaptive DHLI hint ribbon along bottom-left (warm espresso)
            RectTransform modeStrip = CreatePanel(deskArea, "ModeHint", new Color(0.18f, 0.16f, 0.145f, 1f), raycast: false, rounded: false);
            modeStrip.anchorMin = new Vector2(0.008f, 0f); modeStrip.anchorMax = new Vector2(0.70f, 0f); modeStrip.pivot = new Vector2(0f, 0f);
            modeStrip.sizeDelta = new Vector2(0f, 22f); modeStrip.anchoredPosition = new Vector2(0f, 3f);
            browserModeHintText = CreateText(modeStrip, "MHT", "", 10, new Color(0.88f, 0.84f, 0.76f), TextAnchor.MiddleLeft);
            SetFill(browserModeHintText.GetComponent<RectTransform>(), 8, 1);

            // LEFT WINDOW: ILLUSTRATED REAL WEBSITE / SOCIAL POST / CONSPIRACY BLOG (warm editorial paper!)
            blogDocRect = CreateDraggableDeskPaper(deskArea, "Doc_Blog", new Vector2(-246f, 6f), new Vector2(478f, 304f),
                new Color(0.965f, 0.955f, 0.935f, 1f), new Color(0.32f, 0.36f, 0.42f, 1f),
                new Color(0.14f, 0.16f, 0.20f, 1f), "VITAPULSE HEALTH  •  Official Support Portal");
            leftDocHeaderText = blogDocRect.Find("HdrBar/HdrTxt")?.GetComponent<Text>();

            // Realistic Page Masthead Strip
            RectTransform masthead = CreatePanel(blogDocRect, "PageMasthead", new Color(0.11f, 0.34f, 0.32f, 0.16f), raycast: false, rounded: false);
            masthead.sizeDelta = new Vector2(0f, 18f);
            leftMastheadBg = masthead.GetComponent<Image>();
            leftMastheadText = CreateText(masthead, "MastTxt", "<b>🔒 vitapulse.com/faq</b>", 9, new Color(0.10f, 0.34f, 0.30f), TextAnchor.MiddleLeft);
            SetFill(leftMastheadText.GetComponent<RectTransform>(), 8, 1);

            // Visual Hero Banner Illustration (Medical Portal / Social Reel / Conspiracy Blog Graphic!)
            GameObject heroGo = new GameObject("HeroBanner");
            heroGo.transform.SetParent(blogDocRect, false);
            heroGo.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 46f);
            leftHeroBannerImg = heroGo.AddComponent<RawImage>();
            leftHeroBannerImg.texture = heroBannerFaqTex;
            leftHeroBannerImg.raycastTarget = false;

            for (int i = 0; i < leftClaimButtons.Length; i++)
            {
                int idx = i;
                float rowH = (i == 0) ? 30f : 26f;
                Button pBtn = CreateButton(blogDocRect, $"Claim_{i}", "", new Color(1f, 1f, 1f, 0.01f), rowH, () => OnLeftClaimClicked(idx), new Color(0.12f, 0.13f, 0.15f));
                ColorBlock cb = pBtn.colors;
                cb.normalColor = Color.white;
                cb.highlightedColor = new Color(0.98f, 0.89f, 0.50f, 0.55f);
                cb.pressedColor = new Color(0.95f, 0.82f, 0.36f, 0.80f);
                pBtn.colors = cb;
                Text t = pBtn.GetComponentInChildren<Text>();
                t.alignment = TextAnchor.MiddleLeft;
                t.fontSize = (i == 0) ? 13 : 10;
                leftClaimButtons[i] = pBtn;
                leftClaimLabels[i] = t;
                leftClaimBgs[i] = pBtn.GetComponent<Image>();
            }

            // Realistic Website/Blog Footer Engagement & Comments Strip
            RectTransform footBar = CreatePanel(blogDocRect, "PageFooter", new Color(0.88f, 0.86f, 0.82f, 0.85f), raycast: false, rounded: false);
            footBar.sizeDelta = new Vector2(0f, 18f);
            leftFooterEngagementText = CreateText(footBar, "FootTxt", "", 9, new Color(0.32f, 0.30f, 0.28f), TextAnchor.MiddleLeft);
            SetFill(leftFooterEngagementText.GetComponent<RectTransform>(), 8, 1);

            // Big tilted "UNVERIFIED" ink stamp slammed onto Blog once stamped!
            RectTransform stampOverlay = CreatePanel(blogDocRect, "BigStampOverlay", new Color(0.62f, 0.14f, 0.15f, 0.90f), raycast: false, rounded: true);
            LayoutElement stIgnore = stampOverlay.gameObject.AddComponent<LayoutElement>();
            stIgnore.ignoreLayout = true;
            stampOverlay.anchorMin = new Vector2(0.5f, 0.5f); stampOverlay.anchorMax = new Vector2(0.5f, 0.5f); stampOverlay.pivot = new Vector2(0.5f, 0.5f);
            stampOverlay.sizeDelta = new Vector2(236f, 54f);
            stampOverlay.anchoredPosition = Vector2.zero;
            stampOverlay.localRotation = Quaternion.Euler(0f, 0f, -12f);
            bigStampOverlayBg = stampOverlay.GetComponent<Image>();
            bigStampOverlayText = CreateText(stampOverlay, "StTxt", "<b>UNVERIFIED</b>", 24, new Color(0.99f, 0.96f, 0.88f), TextAnchor.MiddleCenter);
            SetFill(bigStampOverlayText.GetComponent<RectTransform>(), 6, 4);
            bigStampOverlayObj = stampOverlay.gameObject;
            bigStampOverlayObj.SetActive(false);

            // RIGHT WINDOW: WARM EDITORIAL ARCHIVAL CASE BINDER (matches office parchment, legal pad & manila dossier!)
            Color binderPaper  = new Color(0.945f, 0.918f, 0.865f, 1f);
            Color binderBorder = new Color(0.62f, 0.55f, 0.44f, 1f);
            Color binderHdr    = new Color(0.49f, 0.15f, 0.16f, 1f); // Refined burgundy dossier header

            rulesDocRect = CreateDraggableDeskPaper(deskArea, "Doc_Rules", new Vector2(246f, 0f), new Vector2(478f, 250f),
                binderPaper, binderBorder, new Color(0.36f, 0.27f, 0.16f, 1f), "OFFICIAL RULEBOOK  •  VERIFICATION STANDARDS");

            registryDocRect = CreateDraggableDeskPaper(deskArea, "Doc_Registry", new Vector2(246f, 0f), new Vector2(478f, 250f),
                binderPaper, binderBorder, binderHdr, "WHOIS &amp; EU HEALTH REGISTRY  •  LIVE RECORD");

            policyDocRect = CreateDraggableDeskPaper(deskArea, "Doc_Policy", new Vector2(246f, 0f), new Vector2(478f, 250f),
                binderPaper, binderBorder, new Color(0.16f, 0.34f, 0.27f, 1f), "VITAPULSE PRIVACY POLICY v4.2  •  LEGAL BRIEF");

            for (int r = 0; r < rightClues.Length; r++)
            {
                int rIdx = r;
                RectTransform targetDoc = rightClues[r].docGroup == 0 ? policyDocRect
                    : (rightClues[r].docGroup == 1 ? registryDocRect : rulesDocRect);
                float btnH = rightClues[r].docGroup == 0 ? 64f : 84f;
                Button rBtn = CreateButton(targetDoc, $"Clue_{r}", rightClues[r].label, new Color(0.88f, 0.84f, 0.76f, 0.35f), btnH, () => OnRightClueClicked(rIdx), new Color(0.16f, 0.14f, 0.12f));
                ColorBlock rcb = rBtn.colors;
                rcb.normalColor = Color.white;
                rcb.highlightedColor = new Color(0.96f, 0.86f, 0.48f, 0.65f);
                rcb.pressedColor = new Color(0.92f, 0.78f, 0.34f, 0.85f);
                rBtn.colors = rcb;
                Text t = rBtn.GetComponentInChildren<Text>();
                t.alignment = TextAnchor.MiddleLeft;
                t.fontSize = 11;
                rightClueButtons[r] = rBtn;
                rightClueLabels[r] = t;
                rightClueBgs[r] = rBtn.GetComponent<Image>();
            }

            // Red Detective Thread Line (connects Left Mismatch <-> Right Proof on match!)
            RectTransform threadRt = CreatePanel(deskArea, "RedThreadLine", new Color(0.92f, 0.24f, 0.24f, 0.95f), raycast: false, rounded: false);
            threadRt.anchorMin = new Vector2(0.5f, 0.5f); threadRt.anchorMax = new Vector2(0.5f, 0.5f); threadRt.pivot = new Vector2(0.5f, 0.5f);
            threadRt.sizeDelta = new Vector2(100f, 3.5f);
            redThreadRect = threadRt;
            redThreadRect.gameObject.SetActive(false);

            SelectReferenceTab(0);

            // Yellow Detective Sticky Note neatly tucked in bottom-right corner
            RectTransform stickyNote = CreatePanel(deskArea, "BookcaseStickyNote", new Color(0.14f, 0.18f, 0.22f, 0.96f), raycast: false, rounded: true);
            Outline stOut = stickyNote.gameObject.AddComponent<Outline>();
            stOut.effectColor = new Color(0.85f, 0.74f, 0.44f, 0.65f);
            stOut.effectDistance = new Vector2(1f, -1f);
            stickyNote.anchorMin = new Vector2(0.71f, 0f); stickyNote.anchorMax = new Vector2(0.992f, 0f); stickyNote.pivot = new Vector2(1f, 0f);
            stickyNote.sizeDelta = new Vector2(0f, 44f); stickyNote.anchoredPosition = new Vector2(0f, 3f);
            bookcaseStickyText = CreateText(stickyNote, "StickyTxt",
                "<b>[BOOKCASE TIP]</b> Read <i>Myths</i> or <i>Privacy 101</i> (Right Wall ►) for +5 DHLI!",
                9, new Color(0.92f, 0.90f, 0.84f), TextAnchor.MiddleLeft);
            SetFill(bookcaseStickyText.GetComponent<RectTransform>(), 7, 3);
            bookcaseStickyObj = stickyNote.gameObject;
            bookcaseStickyObj.SetActive(true);

            // Single clean live feedback line (bright readable on dark OS window!)
            computerFeedbackText = CreateText(gameBox.transform, "PCFb", "Inspect File 1/3 against DHIO Terminal — if everything checks out, stamp VERIFIED!", 11, new Color(0.86f, 0.92f, 0.96f), TextAnchor.MiddleLeft);
            computerFeedbackText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 18f);
            stampStatusText = null;

            // PAPERS, PLEASE PHYSICAL STAMP BAR (labels dynamically update for File 1 / File 2 / File 3!)
            GameObject stampBar = new GameObject("VerdictStampBar");
            stampBar.transform.SetParent(gameBox.transform, false);
            stampBar.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 32f);
            HorizontalLayoutGroup stLay = stampBar.AddComponent<HorizontalLayoutGroup>();
            stLay.spacing = 8; stLay.childControlWidth = true; stLay.childForceExpandWidth = true;

            stampBtnA = CreateButton(stampBar.transform, "StampA", "STAMP A", new Color(0.18f, 0.48f, 0.34f, 1f), 30f, () => { });
            stampBtnB = CreateButton(stampBar.transform, "StampB", "STAMP B", new Color(0.68f, 0.40f, 0.14f, 1f), 30f, () => { });
            stampBtnC = CreateButton(stampBar.transform, "StampC", "STAMP C", new Color(0.62f, 0.18f, 0.18f, 1f), 30f, () => { });

            stampBarObj = stampBar;
            stampBarObj.SetActive(false);

            browserGameBox = gameBox;
            browserAuditTab = auditRoot;
            appBrowserObj = root;
        }

        private RectTransform CreateDraggableDeskPaper(Transform deskArea, string name, Vector2 startPos, Vector2 size, Color paperColor, Color borderColor, Color headerColor, string headerTitle)
        {
            RectTransform doc = CreatePanel(deskArea, name, paperColor, raycast: true, rounded: true);
            doc.anchorMin = new Vector2(0.5f, 0.5f); doc.anchorMax = new Vector2(0.5f, 0.5f); doc.pivot = new Vector2(0.5f, 0.5f);
            doc.sizeDelta = size;
            doc.anchoredPosition = startPos;

            Shadow sh = doc.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.34f); sh.effectDistance = new Vector2(4f, -5f);
            Outline outline = doc.gameObject.AddComponent<Outline>();
            outline.effectColor = borderColor; outline.effectDistance = new Vector2(1f, -1f);

            DraggableDocWindow drag = doc.gameObject.AddComponent<DraggableDocWindow>();
            drag.Init(doc, deskArea as RectTransform);

            VerticalLayoutGroup vLay = doc.gameObject.AddComponent<VerticalLayoutGroup>();
            vLay.padding = new RectOffset(8, 8, 6, 6); vLay.spacing = 4;
            vLay.childControlHeight = false; vLay.childForceExpandHeight = false;

            RectTransform hdrBar = CreatePanel(doc, "HdrBar", headerColor, raycast: false, rounded: true);
            hdrBar.sizeDelta = new Vector2(0f, 22f);
            Text hdrTxt = CreateText(hdrBar, "HdrTxt", $"<b>{headerTitle}</b>", 10, new Color(0.98f, 0.97f, 0.94f), TextAnchor.MiddleLeft);
            SetFill(hdrTxt.GetComponent<RectTransform>(), 8, 1);

            return doc;
        }

        private void CreateNewsSidebarCard(Transform parent, string tag, string headline, Color accentCol)
        {
            RectTransform card = CreatePanel(parent, "NewsCard", new Color(0.99f, 0.985f, 0.97f, 1f), raycast: false, rounded: true);
            card.sizeDelta = new Vector2(0f, 102f);
            Outline cardOut = card.gameObject.AddComponent<Outline>();
            cardOut.effectColor = new Color(0.80f, 0.78f, 0.74f, 1f);
            cardOut.effectDistance = new Vector2(1f, -1f);

            RectTransform bar = CreatePanel(card, "Accent", accentCol, raycast: false, rounded: false);
            bar.anchorMin = new Vector2(0f, 0f); bar.anchorMax = new Vector2(0f, 1f); bar.pivot = new Vector2(0f, 0.5f);
            bar.sizeDelta = new Vector2(4f, 0f); bar.anchoredPosition = Vector2.zero;

            Text t = CreateText(card, "Txt", $"<b><color=#{ColorUtility.ToHtmlStringRGB(accentCol)}>{tag}</color></b>\n\n{headline}", 11, new Color(0.16f, 0.17f, 0.19f), TextAnchor.UpperLeft);
            RectTransform tRt = t.GetComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero; tRt.anchorMax = Vector2.one;
            tRt.offsetMin = new Vector2(12f, 8f); tRt.offsetMax = new Vector2(-10f, -8f);
        }

        private static Texture2D CreateHeroBannerTexture(int kind)
        {
            const int w = 384, h = 48;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < h; y++)
            {
                float fy = y / (float)(h - 1);
                for (int x = 0; x < w; x++)
                {
                    float fx = x / (float)(w - 1);
                    Color c;
                    if (kind == 0)
                    {
                        // Official Medical Portal: calm teal-slate clinical gradient + clean white cross emblem on left
                        c = Color.Lerp(new Color(0.08f, 0.27f, 0.28f, 1f), new Color(0.16f, 0.44f, 0.42f, 1f), fx * 0.7f + fy * 0.3f);
                        bool crossV = x >= 22 && x <= 28 && y >= 11 && y <= 37;
                        bool crossH = x >= 12 && x <= 38 && y >= 21 && y <= 27;
                        if (crossV || crossH) c = new Color(0.95f, 0.99f, 0.98f, 1f);
                        if (y < 3) c = new Color(0.45f, 0.82f, 0.75f, 1f);
                    }
                    else if (kind == 1)
                    {
                        // Viral HealthGram Reel: rich violet-to-coral social gradient + play triangle badge
                        c = Color.Lerp(new Color(0.28f, 0.14f, 0.42f, 1f), new Color(0.78f, 0.30f, 0.34f, 1f), fx);
                        float dx = x - 26f, dy = y - 24f;
                        if (dx * dx + dy * dy < 225f) c = new Color(1f, 1f, 1f, 0.92f);
                        // Subtle reel progress bar at bottom
                        if (y >= 2 && y <= 4) c = fx < 0.62f ? new Color(1f, 0.94f, 0.72f, 1f) : new Color(1f, 1f, 1f, 0.25f);
                    }
                    else
                    {
                        // Conspiracy Blog: dramatic dark crimson/black tabloid banner + diagonal hazard caution stripe!
                        c = Color.Lerp(new Color(0.14f, 0.04f, 0.05f, 1f), new Color(0.48f, 0.08f, 0.10f, 1f), (1f - fx) * 0.65f + fy * 0.35f);
                        if (y < 6)
                        {
                            bool stripe = ((x + y) / 8) % 2 == 0;
                            c = stripe ? new Color(0.90f, 0.70f, 0.16f, 1f) : new Color(0.10f, 0.09f, 0.09f, 1f);
                        }
                        // Warning diamond accent on left
                        float adx = Mathf.Abs(x - 26f), ady = Mathf.Abs(y - 26f);
                        if (adx + ady < 14f) c = new Color(0.95f, 0.78f, 0.24f, 1f);
                        if (x >= 25 && x <= 27 && y >= 23 && y <= 32) c = new Color(0.14f, 0.04f, 0.05f, 1f);
                        if (x >= 25 && x <= 27 && y >= 18 && y <= 20) c = new Color(0.14f, 0.04f, 0.05f, 1f);
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        private void BuildExplorerApp(Transform parent)
        {
            GameObject root = new GameObject("App_Explorer");
            root.transform.SetParent(parent, false);
            SetFill(root.AddComponent<RectTransform>(), 0, 0);
            HorizontalLayoutGroup hLay = root.AddComponent<HorizontalLayoutGroup>();
            hLay.spacing = 10; hLay.childControlWidth = true; hLay.childForceExpandWidth = false;

            // Left Explorer Sidebar
            RectTransform side = CreatePanel(root.transform, "ExpSide", new Color(0.89f, 0.90f, 0.92f, 1f), raycast: false, rounded: true);
            LayoutElement sLe = side.gameObject.AddComponent<LayoutElement>();
            sLe.minWidth = 175f; sLe.preferredWidth = 175f; sLe.flexibleWidth = 0f;
            Text sTxt = CreateText(side, "SideTxt",
                "<b>QUICK ACCESS</b>\n\n" +
                "•  Desktop\n" +
                "•  <b>Case Files</b>\n" +
                "•  Documents\n" +
                "•  Downloads\n" +
                "•  This PC",
                13, new Color(0.18f, 0.20f, 0.24f), TextAnchor.UpperLeft);
            SetFill(sTxt.GetComponent<RectTransform>(), 14, 14);

            // Right File List
            RectTransform main = CreatePanel(root.transform, "ExpMain", Color.white, raycast: false, rounded: true);
            LayoutElement mLe = main.gameObject.AddComponent<LayoutElement>();
            mLe.minWidth = 300f; mLe.flexibleWidth = 1f;
            VerticalLayoutGroup mLay = main.gameObject.AddComponent<VerticalLayoutGroup>();
            mLay.padding = new RectOffset(14, 14, 12, 12); mLay.spacing = 8;
            mLay.childControlHeight = false; mLay.childForceExpandHeight = false;

            Text hdr = CreateText(main, "ExpHdr", "<b>This PC  ›  Desktop  ›  Case Files</b>", 13, new Color(0.32f, 0.35f, 0.40f), TextAnchor.MiddleLeft);
            hdr.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 22f);

            CreateButton(main, "File_Blog",   "🌐  Case_01_Alex_Blog_Audit.url   (Open in Browser)", new Color(0.92f, 0.94f, 0.97f, 1f), 38f, () => OpenOSApp("browser_audit"), new Color(0.13f, 0.15f, 0.18f));
            CreateButton(main, "File_Policy", "📄  VitaPulse_Privacy_Policy_v4.2.pdf", new Color(0.92f, 0.94f, 0.97f, 1f), 38f, () => OpenOSApp("policy"), new Color(0.13f, 0.15f, 0.18f));
            CreateButton(main, "File_Notes",  "📝  CaseNotes.txt", new Color(0.92f, 0.94f, 0.97f, 1f), 38f, () => OpenOSApp("notes"), new Color(0.13f, 0.15f, 0.18f));
            CreateButton(main, "File_Sofia",  "📁  Case_02_Sofia_Romano  (WP4 — Pending Call)", new Color(0.95f, 0.94f, 0.91f, 1f), 38f, () => ShowDesktopToast("Case 02 (Sofia) unlocks after Case 01!"), new Color(0.35f, 0.35f, 0.38f));
            CreateButton(main, "File_Daniel", "📁  Case_03_Daniel_Conti  (WP5 — Locked)", new Color(0.95f, 0.94f, 0.91f, 1f), 38f, () => ShowDesktopToast("Case 03 (Daniel) unlocks after Case 02!"), new Color(0.35f, 0.35f, 0.38f));

            appExplorerObj = root;
        }

        private void BuildMailApp(Transform parent)
        {
            GameObject root = new GameObject("App_Mail");
            root.transform.SetParent(parent, false);
            SetFill(root.AddComponent<RectTransform>(), 0, 0);
            HorizontalLayoutGroup hLay = root.AddComponent<HorizontalLayoutGroup>();
            hLay.spacing = 10; hLay.childControlWidth = true; hLay.childForceExpandWidth = false;

            RectTransform listCol = CreatePanel(root.transform, "InboxList", new Color(0.90f, 0.91f, 0.93f, 1f), raycast: false, rounded: true);
            LayoutElement lLe = listCol.gameObject.AddComponent<LayoutElement>();
            lLe.minWidth = 240f; lLe.preferredWidth = 240f; lLe.flexibleWidth = 0f;
            VerticalLayoutGroup lLay = listCol.gameObject.AddComponent<VerticalLayoutGroup>();
            lLay.padding = new RectOffset(8, 8, 10, 10); lLay.spacing = 6;
            lLay.childControlHeight = false; lLay.childForceExpandHeight = false;

            Text inHdr = CreateText(listCol, "InHdr", "<b>INBOX (3)</b>", 12, new Color(0.30f, 0.34f, 0.40f), TextAnchor.MiddleLeft);
            inHdr.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 20f);

            CreateButton(listCol, "Mail1", "Office Supervisor\nWelcome to Day 1", Color.white, 48f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.45f);
                mailBodyText.text = "<b>From:</b> Office Supervisor\n<b>Subject:</b> Welcome to Day 1 — VitaPulse Investigation\n\nWelcome aboard, Investigator!\nThree citizens have contacted us about the viral health app VitaPulse. Grab the Investigation Manual from the bookshelf and check every claim using Source, Evidence, and Context.";
            }, new Color(0.13f, 0.15f, 0.18f));

            CreateButton(listCol, "Mail2", "VitaPulse Press Alert\nViral Headlines Today", Color.white, 48f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.45f);
                mailBodyText.text = "<b>From:</b> HealthWire Alerts\n<b>Subject:</b> VitaPulse Trending Everywhere\n\n• \"VitaPulse Can Detect Serious Diseases Early!\"\n• \"Millions Download the New Health App!\"\n• \"Is VitaPulse Secretly Selling Medical Records?\"\n\nOpen the Web Browser to inspect active claims.";
            }, new Color(0.13f, 0.15f, 0.18f));

            CreateButton(listCol, "Mail3", "IT Support\nOffice Printer Warning", Color.white, 48f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.45f);
                mailBodyText.text = "<b>From:</b> IT Support\n<b>Subject:</b> Printer Tray B Jammed Again\n\nSomeone tried to photocopy something strange on the office printer and jammed Tray B (Error E-09). Please check the printer when you have a moment.";
            }, new Color(0.13f, 0.15f, 0.18f));

            RectTransform bodyCol = CreatePanel(root.transform, "MailBody", Color.white, raycast: false, rounded: true);
            LayoutElement bLe = bodyCol.gameObject.AddComponent<LayoutElement>();
            bLe.minWidth = 300f; bLe.flexibleWidth = 1f;
            mailBodyText = CreateText(bodyCol, "MailTxt",
                "<b>From:</b> Office Supervisor\n<b>Subject:</b> Welcome to Day 1 — VitaPulse Investigation\n\nWelcome aboard, Investigator!\nThree citizens have contacted us about the viral health app VitaPulse. Grab the Investigation Manual from the bookshelf and check every claim using Source, Evidence, and Context.",
                14, new Color(0.14f, 0.16f, 0.19f), TextAnchor.UpperLeft);
            SetFill(mailBodyText.GetComponent<RectTransform>(), 18, 16);

            appMailObj = root;
        }

        private void BuildThisPcApp(Transform parent)
        {
            RectTransform panel = CreatePanel(parent, "App_ThisPC", Color.white, raycast: false, rounded: true);
            SetFill(panel, 0, 0);
            Text txt = CreateText(panel, "PCTxt",
                "<b>THIS PC  —  DHIO WORKSTATION-12</b>\n\n" +
                "<b>Devices and Drives:</b>\n" +
                "•  <b>Local Disk (C:)</b> — 155 GB free of 464 GB  [Healthy]\n" +
                "•  <b>Evidence Archive (D:)</b> — 294 GB free of 931 GB  [Encrypted]\n\n" +
                "<b>System Information:</b>\n" +
                "•  Operating System: HealthOS 26.2 Workstation (64-bit)\n" +
                "•  Privacy Firewall: <b>ACTIVE</b>\n" +
                "•  Local Audit Mode: <b>READY</b>",
                15, new Color(0.14f, 0.16f, 0.19f), TextAnchor.UpperLeft);
            SetFill(txt.GetComponent<RectTransform>(), 22, 18);
            appThisPcObj = panel.gameObject;
        }

        private void BuildPolicyApp(Transform parent)
        {
            RectTransform panel = CreatePanel(parent, "App_Policy", new Color(0.98f, 0.97f, 0.94f, 1f), raycast: false, rounded: true);
            SetFill(panel, 0, 0);
            Text txt = CreateText(panel, "PT",
                "<b>VITAPULSE — OFFICIAL PRIVACY POLICY (v4.2)</b>\n\n" +
                "<b>Section 4.2 — Third-Party Data Sharing:</b>\n" +
                "\"Some user information may be shared with third-party partners to operate, analyze, and improve app services.\"\n\n" +
                "<b>Investigator Legal Note:</b>\n" +
                "•  Does NOT specify which third-party partners receive data (legitimate privacy concern).\n" +
                "•  Does NOT establish a secret sale of all private medical records.",
                15, new Color(0.13f, 0.15f, 0.18f), TextAnchor.UpperLeft);
            SetFill(txt.GetComponent<RectTransform>(), 22, 18);
            appPolicyObj = panel.gameObject;
        }

        private void BuildNotesApp(Transform parent)
        {
            RectTransform panel = CreatePanel(parent, "App_Notes", new Color(0.99f, 0.98f, 0.93f, 1f), raycast: false, rounded: true);
            SetFill(panel, 0, 0);
            Text txt = CreateText(panel, "CT",
                "<b>CaseNotes.txt — Digital Health Investigation Office</b>\n" +
                "──────────────────────────────────────────\n\n" +
                "•  Case 01 — Alex Moretti: Blog claim vs Privacy Policy Section 4.2\n" +
                "•  Case 02 — Sofia Romano (WP4): Viral video & App permissions\n" +
                "•  Case 03 — Daniel Conti (WP5): Verified portal & Altered screenshot\n" +
                "•  Final Case (WP5): Connect all clues on the Cork Board & publish report.\n\n" +
                "<i>Golden Rule: Source — Evidence — Context.</i>",
                14, new Color(0.14f, 0.15f, 0.17f), TextAnchor.UpperLeft);
            if (monoFont != null) txt.font = monoFont;
            SetFill(txt.GetComponent<RectTransform>(), 20, 18);
            appNotesObj = panel.gameObject;
        }

        private void BuildRecycleBinApp(Transform parent)
        {
            RectTransform panel = CreatePanel(parent, "App_Recycle", Color.white, raycast: false, rounded: true);
            SetFill(panel, 0, 0);
            VerticalLayoutGroup lay = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(22, 22, 18, 18); lay.spacing = 14;
            lay.childControlHeight = false; lay.childForceExpandHeight = false;

            recycleListText = CreateText(panel, "RecTxt", "", 14, new Color(0.14f, 0.16f, 0.19f), TextAnchor.UpperLeft);
            recycleListText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 240f);

            emptyRecycleBtn = CreateButton(panel, "EmptyBinBtn", "Empty Recycle Bin", new Color(0.68f, 0.22f, 0.20f, 1f), 38f, () =>
            {
                SoundManager.Instance?.Play("paper_rustle", 0.75f);
                recycleBinEmptied = true;
                RefreshRecycleBinUI();
                ShowDesktopToast("Recycle Bin emptied!");
            });

            appRecycleObj = panel.gameObject;
        }

        private void BuildStartMenuPopup(Transform parent)
        {
            // Realistic Windows 11 Start Menu (dark frosted charcoal + Search + Pinned icon grid + Recommended + User/Power footer)
            RectTransform menu = CreatePanel(parent, "StartMenuPopup", new Color(0.12f, 0.125f, 0.14f, 0.96f), raycast: true, rounded: true);
            menu.anchorMin = new Vector2(0f, 0f); menu.anchorMax = new Vector2(0f, 0f); menu.pivot = new Vector2(0f, 0f);
            menu.sizeDelta = new Vector2(430f, 455f);
            menu.anchoredPosition = new Vector2(10f, 48f);
            Shadow sh = menu.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.55f);
            sh.effectDistance = new Vector2(6f, -8f);
            Outline outl = menu.gameObject.AddComponent<Outline>();
            outl.effectColor = new Color(1f, 1f, 1f, 0.14f);
            outl.effectDistance = new Vector2(1f, -1f);

            // Top Search Bar Pill
            RectTransform searchPill = CreatePanel(menu, "StartSearch", new Color(0.16f, 0.17f, 0.19f, 1f), raycast: false, rounded: true);
            searchPill.anchorMin = new Vector2(0f, 1f); searchPill.anchorMax = new Vector2(1f, 1f); searchPill.pivot = new Vector2(0.5f, 1f);
            searchPill.sizeDelta = new Vector2(-36f, 32f); searchPill.anchoredPosition = new Vector2(0f, -16f);
            Outline sOut = searchPill.gameObject.AddComponent<Outline>();
            sOut.effectColor = new Color(1f, 1f, 1f, 0.10f); sOut.effectDistance = new Vector2(1f, -1f);
            Text sTxt = CreateText(searchPill, "SearchTxt", "   Search apps, settings, and case files...", 12, new Color(0.68f, 0.70f, 0.74f), TextAnchor.MiddleLeft);
            SetFill(sTxt.GetComponent<RectTransform>(), 12f, 2f);

            // Section 1: "Pinned" Header
            Text pinHdr = CreateText(menu, "PinHdr", "<b>Pinned</b>", 12, Color.white, TextAnchor.MiddleLeft);
            RectTransform phRt = pinHdr.GetComponent<RectTransform>();
            phRt.anchorMin = new Vector2(0f, 1f); phRt.anchorMax = new Vector2(1f, 1f); phRt.pivot = new Vector2(0.5f, 1f);
            phRt.sizeDelta = new Vector2(-40f, 20f); phRt.anchoredPosition = new Vector2(0f, -60f);

            // Pinned 2x4 Icon Grid (pure graphic icon + clean label, just like Windows 11!)
            GameObject gridGo = new GameObject("PinnedGrid");
            gridGo.transform.SetParent(menu, false);
            RectTransform gridRt = gridGo.AddComponent<RectTransform>();
            gridRt.anchorMin = new Vector2(0f, 1f); gridRt.anchorMax = new Vector2(1f, 1f); gridRt.pivot = new Vector2(0.5f, 1f);
            gridRt.sizeDelta = new Vector2(-32f, 152f); gridRt.anchoredPosition = new Vector2(0f, -84f);
            GridLayoutGroup grid = gridGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(92f, 70f);
            grid.spacing = new Vector2(6f, 8f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
            grid.childAlignment = TextAnchor.UpperCenter;

            CreateStartAppTile(gridGo.transform, "Browser",     CreateBrowserIcon(), () => OpenOSApp("browser"));
            CreateStartAppTile(gridGo.transform, "Mail",        CreateMailIcon(),    () => OpenOSApp("mail"));
            CreateStartAppTile(gridGo.transform, "Case Files",  CreateFolderIcon(),  () => OpenOSApp("files"));
            CreateStartAppTile(gridGo.transform, "This PC",     CreateMonitorIcon(), () => OpenOSApp("thispc"));
            CreateStartAppTile(gridGo.transform, "Policy PDF",  CreatePdfIcon(),     () => OpenOSApp("policy"));
            CreateStartAppTile(gridGo.transform, "Notes",       CreateNotesIcon(),   () => OpenOSApp("notes"));
            CreateStartAppTile(gridGo.transform, "Recycle Bin", CreateTrashIcon(),   () => OpenOSApp("recycle"));
            CreateStartAppTile(gridGo.transform, "Blog Audit",  CreateBrowserIcon(), () => OpenOSApp("browser_audit"));

            // Section 2: "Recommended" Header
            Text recHdr = CreateText(menu, "RecHdr", "<b>Recommended</b>", 12, Color.white, TextAnchor.MiddleLeft);
            RectTransform rhRt = recHdr.GetComponent<RectTransform>();
            rhRt.anchorMin = new Vector2(0f, 1f); rhRt.anchorMax = new Vector2(1f, 1f); rhRt.pivot = new Vector2(0.5f, 1f);
            rhRt.sizeDelta = new Vector2(-40f, 20f); rhRt.anchoredPosition = new Vector2(0f, -248f);

            // Recommended 2x2 Recent Files Grid
            GameObject recGo = new GameObject("RecommendedGrid");
            recGo.transform.SetParent(menu, false);
            RectTransform recRt = recGo.AddComponent<RectTransform>();
            recRt.anchorMin = new Vector2(0f, 1f); recRt.anchorMax = new Vector2(1f, 1f); recRt.pivot = new Vector2(0.5f, 1f);
            recRt.sizeDelta = new Vector2(-36f, 112f); recRt.anchoredPosition = new Vector2(0f, -274f);
            GridLayoutGroup recGrid = recGo.AddComponent<GridLayoutGroup>();
            recGrid.cellSize = new Vector2(190f, 48f);
            recGrid.spacing = new Vector2(12f, 8f);
            recGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            recGrid.constraintCount = 2;

            CreateStartRecentItem(recGo.transform, "PrivacyPolicy_v4.2.pdf", "Recently opened", CreatePdfIcon(),     () => OpenOSApp("policy"));
            CreateStartRecentItem(recGo.transform, "CaseNotes.txt",          "Today, 09:15",    CreateNotesIcon(),   () => OpenOSApp("notes"));
            CreateStartRecentItem(recGo.transform, "VitaPulse Alerts",       "Trending now",    CreateBrowserIcon(), () => OpenOSApp("browser"));
            CreateStartRecentItem(recGo.transform, "Inbox (3 unread)",       "Office Mail",     CreateMailIcon(),    () => OpenOSApp("mail"));

            // Bottom User & Power Footer Bar (matches Windows 11 footer!)
            RectTransform footer = CreatePanel(menu, "StartFooter", new Color(0.09f, 0.095f, 0.11f, 1f), raycast: false, rounded: false);
            footer.anchorMin = new Vector2(0f, 0f); footer.anchorMax = new Vector2(1f, 0f); footer.pivot = new Vector2(0.5f, 0f);
            footer.sizeDelta = new Vector2(0f, 52f); footer.anchoredPosition = Vector2.zero;

            RectTransform footDiv = CreatePanel(footer, "FootDiv", new Color(1f, 1f, 1f, 0.08f), raycast: false, rounded: false);
            footDiv.anchorMin = new Vector2(0f, 1f); footDiv.anchorMax = new Vector2(1f, 1f); footDiv.pivot = new Vector2(0.5f, 1f);
            footDiv.sizeDelta = new Vector2(0f, 1f); footDiv.anchoredPosition = Vector2.zero;

            // User avatar circle + Investigator name
            GameObject avGo = new GameObject("UserAvatar");
            avGo.transform.SetParent(footer, false);
            RectTransform avRt = avGo.AddComponent<RectTransform>();
            avRt.anchorMin = new Vector2(0f, 0.5f); avRt.anchorMax = new Vector2(0f, 0.5f); avRt.pivot = new Vector2(0f, 0.5f);
            avRt.sizeDelta = new Vector2(28f, 28f); avRt.anchoredPosition = new Vector2(20f, 0f);
            RawImage avRaw = avGo.AddComponent<RawImage>();
            avRaw.texture = CreateUserAvatarIcon();
            avRaw.raycastTarget = false;

            Text userTxt = CreateText(footer, "UserName", "Investigator (Group 12)", 12, new Color(0.92f, 0.94f, 0.96f), TextAnchor.MiddleLeft);
            RectTransform uRt = userTxt.GetComponent<RectTransform>();
            uRt.anchorMin = new Vector2(0f, 0.5f); uRt.anchorMax = new Vector2(0f, 0.5f); uRt.pivot = new Vector2(0f, 0.5f);
            uRt.sizeDelta = new Vector2(200f, 26f); uRt.anchoredPosition = new Vector2(58f, 0f);

            // Power button (steps back to Desk cleanly!)
            Button powerBtn = CreateButton(footer, "PowerStepBack", "", new Color(1f, 1f, 1f, 0.07f), 32f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                startMenuPopup.SetActive(false);
                CameraViewpointController.Instance?.MoveToViewpoint("Desk");
            });
            RectTransform pwrRt = powerBtn.GetComponent<RectTransform>();
            pwrRt.anchorMin = new Vector2(1f, 0.5f); pwrRt.anchorMax = new Vector2(1f, 0.5f); pwrRt.pivot = new Vector2(1f, 0.5f);
            pwrRt.sizeDelta = new Vector2(34f, 32f); pwrRt.anchoredPosition = new Vector2(-18f, 0f);
            GameObject pwrGlyph = new GameObject("PwrGlyph");
            pwrGlyph.transform.SetParent(powerBtn.transform, false);
            RectTransform pgRt = pwrGlyph.AddComponent<RectTransform>();
            pgRt.anchorMin = new Vector2(0.5f, 0.5f); pgRt.anchorMax = new Vector2(0.5f, 0.5f); pgRt.pivot = new Vector2(0.5f, 0.5f);
            pgRt.sizeDelta = new Vector2(18f, 18f); pgRt.anchoredPosition = Vector2.zero;
            RawImage pgRaw = pwrGlyph.AddComponent<RawImage>();
            pgRaw.texture = CreatePowerIcon();
            pgRaw.raycastTarget = false;

            startMenuPopup = menu.gameObject;
            startMenuPopup.SetActive(false);
        }

        private void CreateStartAppTile(Transform parent, string label, Texture2D iconTex, UnityEngine.Events.UnityAction onClick)
        {
            GameObject go = new GameObject("Tile_" + label);
            go.transform.SetParent(parent, false);
            Image bg = go.AddComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 0f);
            if (roundedSprite != null) { bg.sprite = roundedSprite; bg.type = Image.Type.Sliced; }

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(1f, 1f, 1f, 0f);
            cb.highlightedColor = new Color(1f, 1f, 1f, 0.12f);
            cb.pressedColor = new Color(1f, 1f, 1f, 0.20f);
            cb.fadeDuration = 0.08f;
            btn.colors = cb;
            btn.onClick.AddListener(onClick);

            GameObject ig = new GameObject("Icon");
            ig.transform.SetParent(go.transform, false);
            RectTransform iRt = ig.AddComponent<RectTransform>();
            iRt.anchorMin = new Vector2(0.5f, 1f); iRt.anchorMax = new Vector2(0.5f, 1f); iRt.pivot = new Vector2(0.5f, 1f);
            iRt.sizeDelta = new Vector2(36f, 36f); iRt.anchoredPosition = new Vector2(0f, -6f);
            RawImage raw = ig.AddComponent<RawImage>();
            raw.texture = iconTex;
            raw.raycastTarget = false;

            Text lbl = CreateText(go.transform, "Lbl", label, 11, new Color(0.92f, 0.94f, 0.96f), TextAnchor.UpperCenter);
            RectTransform lRt = lbl.GetComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0f, 0f); lRt.anchorMax = new Vector2(1f, 0f); lRt.pivot = new Vector2(0.5f, 0f);
            lRt.sizeDelta = new Vector2(0f, 22f); lRt.anchoredPosition = new Vector2(0f, 2f);
        }

        private void CreateStartRecentItem(Transform parent, string title, string subtitle, Texture2D iconTex, UnityEngine.Events.UnityAction onClick)
        {
            GameObject go = new GameObject("Rec_" + title);
            go.transform.SetParent(parent, false);
            Image bg = go.AddComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 0f);
            if (roundedSprite != null) { bg.sprite = roundedSprite; bg.type = Image.Type.Sliced; }

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(1f, 1f, 1f, 0f);
            cb.highlightedColor = new Color(1f, 1f, 1f, 0.10f);
            cb.pressedColor = new Color(1f, 1f, 1f, 0.18f);
            cb.fadeDuration = 0.08f;
            btn.colors = cb;
            btn.onClick.AddListener(onClick);

            GameObject ig = new GameObject("Icon");
            ig.transform.SetParent(go.transform, false);
            RectTransform iRt = ig.AddComponent<RectTransform>();
            iRt.anchorMin = new Vector2(0f, 0.5f); iRt.anchorMax = new Vector2(0f, 0.5f); iRt.pivot = new Vector2(0f, 0.5f);
            iRt.sizeDelta = new Vector2(28f, 28f); iRt.anchoredPosition = new Vector2(8f, 0f);
            RawImage raw = ig.AddComponent<RawImage>();
            raw.texture = iconTex;
            raw.raycastTarget = false;

            Text lbl = CreateText(go.transform, "Lbl", $"<b>{title}</b>\n<color=#9BA3AE>{subtitle}</color>", 10, new Color(0.92f, 0.94f, 0.96f), TextAnchor.MiddleLeft);
            RectTransform lRt = lbl.GetComponent<RectTransform>();
            lRt.anchorMin = Vector2.zero; lRt.anchorMax = Vector2.one;
            lRt.offsetMin = new Vector2(44f, 2f); lRt.offsetMax = new Vector2(-6f, -2f);
        }

        private static Texture2D CreateUserAvatarIcon()
        {
            Texture2D tex = NewIconCanvas(64);
            DrawCircleAA(tex, 32, 32, 30, new Color(0.24f, 0.52f, 0.76f, 1f), new Color(0.14f, 0.34f, 0.54f, 1f));
            DrawCircleAA(tex, 32, 41, 10, new Color(0.94f, 0.96f, 0.98f, 1f), new Color(0.84f, 0.88f, 0.92f, 1f));
            DrawCircleAA(tex, 32, 14, 17, new Color(0.94f, 0.96f, 0.98f, 1f), new Color(0.84f, 0.88f, 0.92f, 1f));
            tex.Apply();
            return tex;
        }

        private static Texture2D CreatePowerIcon()
        {
            Texture2D tex = NewIconCanvas(64);
            Color white = new Color(0.94f, 0.96f, 0.98f, 1f);
            DrawArcAA(tex, 32, 30, 17, 4.5f, -180f, 45f, white);
            DrawArcAA(tex, 32, 30, 17, 4.5f, 135f, 180f, white);
            DrawLineAA(tex, 32, 32, 32, 52, 4.5f, white);
            tex.Apply();
            return tex;
        }

        private void BuildTaskbar(Transform parent)
        {
            // Frosted-glass Windows 11 style translucent taskbar with subtle top glass highlight
            RectTransform taskbar = CreatePanel(parent, "OS_Taskbar", new Color(0.08f, 0.10f, 0.14f, 0.82f), raycast: true, rounded: false);
            taskbar.anchorMin = new Vector2(0f, 0f); taskbar.anchorMax = new Vector2(1f, 0f); taskbar.pivot = new Vector2(0.5f, 0f);
            taskbar.sizeDelta = new Vector2(0f, 42f); taskbar.anchoredPosition = Vector2.zero;

            RectTransform glassTop = CreatePanel(taskbar, "GlassHighlight", new Color(1f, 1f, 1f, 0.14f), raycast: false, rounded: false);
            glassTop.anchorMin = new Vector2(0f, 1f); glassTop.anchorMax = new Vector2(1f, 1f); glassTop.pivot = new Vector2(0.5f, 1f);
            glassTop.sizeDelta = new Vector2(0f, 1f); glassTop.anchoredPosition = Vector2.zero;

            // Pure Icon-Only Start Button (real Windows 11 taskbar feel!)
            CreateTaskbarIconButton(taskbar, "StartBtn", CreateStartLogoIcon(), 12f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.45f);
                startMenuPopup.SetActive(!startMenuPopup.activeSelf);
            });

            // Pure Icon-Only Pinned Taskbar Apps (Browser, File Explorer, Mail — NO text!)
            CreateTaskbarIconButton(taskbar, "PinBrowser", CreateBrowserIcon(), 54f,  () => OpenOSApp("browser"));
            CreateTaskbarIconButton(taskbar, "PinFiles",   CreateFolderIcon(),  96f,  () => OpenOSApp("files"));
            CreateTaskbarIconButton(taskbar, "PinMail",    CreateMailIcon(),    138f, () => OpenOSApp("mail"));

            osTaskbarActiveApp = CreateText(taskbar, "ActiveApp", "Desktop", 12, new Color(0.80f, 0.88f, 0.96f), TextAnchor.MiddleLeft);
            RectTransform aaRect = osTaskbarActiveApp.GetComponent<RectTransform>();
            aaRect.anchorMin = new Vector2(0f, 0.5f); aaRect.anchorMax = new Vector2(0f, 0.5f); aaRect.pivot = new Vector2(0f, 0.5f);
            aaRect.sizeDelta = new Vector2(180f, 28f); aaRect.anchoredPosition = new Vector2(186f, 0f);

            // Step Back button
            Button stepBack = CreateButton(taskbar, "StepBackPC", "↓ Step Back", new Color(0.56f, 0.20f, 0.20f, 0.95f), 28f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                if (startMenuPopup != null) startMenuPopup.SetActive(false);
                CameraViewpointController.Instance?.MoveToViewpoint("Desk");
            });
            RectTransform stRect = stepBack.GetComponent<RectTransform>();
            stRect.anchorMin = new Vector2(1f, 0.5f); stRect.anchorMax = new Vector2(1f, 0.5f); stRect.pivot = new Vector2(1f, 0.5f);
            stRect.sizeDelta = new Vector2(106f, 28f); stRect.anchoredPosition = new Vector2(-156f, 0f);

            // System Tray: Real Speaker Icon (clickable mute!) + Real Wi-Fi Icon + Live Clock/Date
            volOnIconTex = CreateVolumeIcon(muted: false);
            volMutedIconTex = CreateVolumeIcon(muted: true);

            Button trayVol = CreateButton(taskbar, "TrayVol", "", new Color(1f, 1f, 1f, 0.08f), 28f, () =>
            {
                SoundManager.Instance?.ToggleMute();
            });
            RectTransform tvRect = trayVol.GetComponent<RectTransform>();
            tvRect.anchorMin = new Vector2(1f, 0.5f); tvRect.anchorMax = new Vector2(1f, 0.5f); tvRect.pivot = new Vector2(1f, 0.5f);
            tvRect.sizeDelta = new Vector2(30f, 28f); tvRect.anchoredPosition = new Vector2(-118f, 0f);
            GameObject volImgGo = new GameObject("VolGlyph");
            volImgGo.transform.SetParent(trayVol.transform, false);
            RectTransform viRt = volImgGo.AddComponent<RectTransform>();
            viRt.anchorMin = new Vector2(0.5f, 0.5f); viRt.anchorMax = new Vector2(0.5f, 0.5f); viRt.pivot = new Vector2(0.5f, 0.5f);
            viRt.sizeDelta = new Vector2(20f, 20f); viRt.anchoredPosition = Vector2.zero;
            osTrayMuteIcon = volImgGo.AddComponent<RawImage>();
            osTrayMuteIcon.texture = volOnIconTex;
            osTrayMuteIcon.raycastTarget = false;

            GameObject wifiGo = new GameObject("TrayWifiIcon");
            wifiGo.transform.SetParent(taskbar, false);
            RectTransform wfRect = wifiGo.AddComponent<RectTransform>();
            wfRect.anchorMin = new Vector2(1f, 0.5f); wfRect.anchorMax = new Vector2(1f, 0.5f); wfRect.pivot = new Vector2(1f, 0.5f);
            wfRect.sizeDelta = new Vector2(20f, 20f); wfRect.anchoredPosition = new Vector2(-88f, 0f);
            RawImage wfRaw = wifiGo.AddComponent<RawImage>();
            wfRaw.texture = CreateWifiIcon();
            wfRaw.raycastTarget = false;

            osClockText = CreateText(taskbar, "Clock", "11:20\n26/11/2026", 10, Color.white, TextAnchor.MiddleRight);
            RectTransform clRect = osClockText.GetComponent<RectTransform>();
            clRect.anchorMin = new Vector2(1f, 0.5f); clRect.anchorMax = new Vector2(1f, 0.5f); clRect.pivot = new Vector2(1f, 0.5f);
            clRect.sizeDelta = new Vector2(72f, 34f); clRect.anchoredPosition = new Vector2(-8f, 0f);
        }

        private void CreateTaskbarIconButton(Transform taskbar, string name, Texture2D iconTex, float xPos, UnityEngine.Events.UnityAction onClick)
        {
            Button btn = CreateButton(taskbar, name, "", new Color(1f, 1f, 1f, 0.07f), 32f, onClick);
            RectTransform rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.5f); rt.anchorMax = new Vector2(0f, 0.5f); rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(34f, 32f);
            rt.anchoredPosition = new Vector2(xPos, 0f);

            GameObject imgGo = new GameObject("Glyph");
            imgGo.transform.SetParent(btn.transform, false);
            RectTransform gRt = imgGo.AddComponent<RectTransform>();
            gRt.anchorMin = new Vector2(0.5f, 0.5f); gRt.anchorMax = new Vector2(0.5f, 0.5f); gRt.pivot = new Vector2(0.5f, 0.5f);
            gRt.sizeDelta = new Vector2(22f, 22f); gRt.anchoredPosition = Vector2.zero;
            RawImage raw = imgGo.AddComponent<RawImage>();
            raw.texture = iconTex;
            raw.raycastTarget = false;
        }

        private void ShowDesktopToast(string msg)
        {
            MissionManager.Instance?.NotifyStatus(msg);
        }

        // === PROCEDURAL CRISP WALLPAPER & DESKTOP ICONS (NO EXTERNAL ASSETS NEEDED!) ===
        private static Texture2D CreateDesktopWallpaperTexture()
        {
            const int w = 256, h = 150;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            Color topCol = new Color(0.10f, 0.19f, 0.29f, 1f);
            Color midCol = new Color(0.15f, 0.30f, 0.42f, 1f);
            Color botCol = new Color(0.08f, 0.14f, 0.22f, 1f);

            Color[] px = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                float v = (float)y / (h - 1);
                for (int x = 0; x < w; x++)
                {
                    float u = (float)x / (w - 1);
                    Color c = v > 0.45f ? Color.Lerp(midCol, topCol, (v - 0.45f) / 0.55f) : Color.Lerp(botCol, midCol, v / 0.45f);
                    // Soft subtle glow in upper-right
                    float dist = Vector2.Distance(new Vector2(u, v), new Vector2(0.75f, 0.68f));
                    float glow = Mathf.Clamp01(1f - dist * 1.45f) * 0.16f;
                    c += new Color(glow * 0.6f, glow * 0.9f, glow, 0f);
                    px[y * w + x] = c;
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }

        // === HIGH-RES 128x128 ANTI-ALIASED SHADED DESKTOP & SYSTEM TRAY ICONS ===
        private static Texture2D CreateFolderIcon()
        {
            Texture2D tex = NewIconCanvas(128);
            // Soft shadow
            DrawRoundedRect(tex, 12, 16, 104, 72, 10, new Color(0f, 0f, 0f, 0.25f), new Color(0f, 0f, 0f, 0.25f));
            // Folder back + tab
            DrawRoundedRect(tex, 10, 26, 108, 76, 10, new Color(0.84f, 0.60f, 0.14f, 1f), new Color(0.72f, 0.48f, 0.08f, 1f));
            DrawRoundedRect(tex, 10, 82, 48, 28, 8, new Color(0.86f, 0.63f, 0.16f, 1f), new Color(0.76f, 0.53f, 0.10f, 1f));
            // Crisp white paper peek inside folder
            DrawRoundedRect(tex, 18, 56, 92, 34, 4, new Color(0.97f, 0.96f, 0.92f, 1f), new Color(0.90f, 0.89f, 0.84f, 1f));
            // Glossy front flap with warm vertical gradient
            DrawRoundedRect(tex, 10, 20, 108, 66, 10, new Color(0.99f, 0.83f, 0.34f, 1f), new Color(0.90f, 0.66f, 0.16f, 1f));
            // Subtle top highlight line on front flap
            DrawRoundedRect(tex, 16, 78, 96, 4, 2, new Color(1f, 0.95f, 0.72f, 0.7f), new Color(1f, 0.95f, 0.72f, 0.2f));
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateBrowserIcon()
        {
            Texture2D tex = NewIconCanvas(128);
            // Soft shadow
            DrawCircleAA(tex, 64, 60, 50, new Color(0f, 0f, 0f, 0.25f), new Color(0f, 0f, 0f, 0.25f));
            // Outer glossy blue-teal globe sphere
            DrawCircleAA(tex, 64, 64, 50, new Color(0.16f, 0.64f, 0.94f, 1f), new Color(0.08f, 0.34f, 0.72f, 1f));
            // Subtle emerald wave crescent on lower-left
            DrawCircleAA(tex, 50, 50, 32, new Color(0.18f, 0.78f, 0.58f, 0.72f), new Color(0.10f, 0.54f, 0.44f, 0.72f));
            // Crisp white ring + inner blue core (Chrome/Edge feel)
            DrawCircleAA(tex, 64, 64, 26, new Color(0.97f, 0.99f, 1f, 1f), new Color(0.86f, 0.92f, 0.98f, 1f));
            DrawCircleAA(tex, 64, 64, 18, new Color(0.22f, 0.60f, 0.95f, 1f), new Color(0.10f, 0.38f, 0.78f, 1f));
            // Glossy top-left specular highlight
            DrawCircleAA(tex, 54, 84, 14, new Color(1f, 1f, 1f, 0.32f), new Color(1f, 1f, 1f, 0.0f));
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateMonitorIcon()
        {
            Texture2D tex = NewIconCanvas(128);
            // Stand & base
            DrawRoundedRect(tex, 52, 20, 24, 22, 3, new Color(0.62f, 0.66f, 0.72f, 1f), new Color(0.45f, 0.49f, 0.55f, 1f));
            DrawRoundedRect(tex, 34, 12, 60, 12, 5, new Color(0.76f, 0.80f, 0.85f, 1f), new Color(0.56f, 0.60f, 0.66f, 1f));
            // Outer metallic bezel
            DrawRoundedRect(tex, 10, 36, 108, 76, 10, new Color(0.24f, 0.27f, 0.32f, 1f), new Color(0.12f, 0.14f, 0.18f, 1f));
            // Glossy desktop screen inside bezel (mini green hills + blue sky!)
            DrawRoundedRect(tex, 17, 45, 94, 60, 5, new Color(0.22f, 0.64f, 0.94f, 1f), new Color(0.42f, 0.78f, 0.98f, 1f));
            DrawRoundedRect(tex, 17, 45, 94, 22, 4, new Color(0.32f, 0.68f, 0.24f, 1f), new Color(0.20f, 0.50f, 0.16f, 1f));
            // Tiny taskbar at bottom of mini screen
            DrawRoundedRect(tex, 17, 45, 94, 6, 2, new Color(0.10f, 0.16f, 0.26f, 0.9f), new Color(0.10f, 0.16f, 0.26f, 0.9f));
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateMailIcon()
        {
            Texture2D tex = NewIconCanvas(128);
            // Soft shadow
            DrawRoundedRect(tex, 12, 22, 104, 74, 10, new Color(0f, 0f, 0f, 0.24f), new Color(0f, 0f, 0f, 0.24f));
            // Envelope body
            DrawRoundedRect(tex, 10, 26, 108, 74, 10, new Color(0.98f, 0.98f, 0.99f, 1f), new Color(0.86f, 0.89f, 0.94f, 1f));
            // Crisp V-shaped folded flap lines
            DrawLineAA(tex, 16, 94, 64, 56, 3.5f, new Color(0.20f, 0.48f, 0.78f, 0.88f));
            DrawLineAA(tex, 112, 94, 64, 56, 3.5f, new Color(0.20f, 0.48f, 0.78f, 0.88f));
            DrawLineAA(tex, 16, 32, 48, 60, 2.2f, new Color(0.68f, 0.74f, 0.82f, 0.75f));
            DrawLineAA(tex, 112, 32, 80, 60, 2.2f, new Color(0.68f, 0.74f, 0.82f, 0.75f));
            // Small red unread notification badge in top-right corner
            DrawCircleAA(tex, 106, 96, 15, new Color(0.92f, 0.26f, 0.24f, 1f), new Color(0.74f, 0.14f, 0.14f, 1f));
            tex.Apply();
            return tex;
        }

        private static Texture2D CreatePdfIcon()
        {
            Texture2D tex = NewIconCanvas(128);
            DrawRoundedRect(tex, 24, 10, 82, 104, 8, new Color(0f, 0f, 0f, 0.24f), new Color(0f, 0f, 0f, 0.24f));
            // Crisp white paper page
            DrawRoundedRect(tex, 22, 14, 82, 104, 8, new Color(0.99f, 0.98f, 0.96f, 1f), new Color(0.90f, 0.89f, 0.86f, 1f));
            // Folded top-right dog-ear corner
            DrawRoundedRect(tex, 82, 96, 22, 22, 3, new Color(0.82f, 0.81f, 0.78f, 1f), new Color(0.75f, 0.74f, 0.71f, 1f));
            // Red PDF header ribbon
            DrawRoundedRect(tex, 16, 68, 72, 26, 5, new Color(0.84f, 0.20f, 0.20f, 1f), new Color(0.66f, 0.12f, 0.12f, 1f));
            // Subtle document text lines
            DrawRoundedRect(tex, 32, 52, 56, 5, 2, new Color(0.62f, 0.65f, 0.70f, 1f), new Color(0.62f, 0.65f, 0.70f, 1f));
            DrawRoundedRect(tex, 32, 40, 48, 5, 2, new Color(0.62f, 0.65f, 0.70f, 1f), new Color(0.62f, 0.65f, 0.70f, 1f));
            DrawRoundedRect(tex, 32, 28, 40, 5, 2, new Color(0.62f, 0.65f, 0.70f, 1f), new Color(0.62f, 0.65f, 0.70f, 1f));
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateNotesIcon()
        {
            Texture2D tex = NewIconCanvas(128);
            DrawRoundedRect(tex, 24, 10, 82, 104, 8, new Color(0f, 0f, 0f, 0.24f), new Color(0f, 0f, 0f, 0.24f));
            // Warm pale-yellow notepad sheet
            DrawRoundedRect(tex, 22, 14, 82, 102, 8, new Color(0.99f, 0.95f, 0.74f, 1f), new Color(0.93f, 0.87f, 0.60f, 1f));
            // Dark slate top binding header
            DrawRoundedRect(tex, 22, 98, 82, 18, 6, new Color(0.24f, 0.36f, 0.48f, 1f), new Color(0.16f, 0.26f, 0.38f, 1f));
            // Spiral rings
            for (int rx = 34; rx <= 94; rx += 15)
                DrawCircleAA(tex, rx, 114, 5, new Color(0.84f, 0.87f, 0.90f, 1f), new Color(0.58f, 0.62f, 0.68f, 1f));
            // Ruled blue lines
            for (int ry = 30; ry <= 82; ry += 14)
                DrawRoundedRect(tex, 32, ry, 62, 3, 1, new Color(0.42f, 0.60f, 0.76f, 0.65f), new Color(0.42f, 0.60f, 0.76f, 0.65f));
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateTrashIcon()
        {
            Texture2D tex = NewIconCanvas(128);
            // Shadow
            DrawCircleAA(tex, 64, 18, 28, new Color(0f, 0f, 0f, 0.22f), new Color(0f, 0f, 0f, 0.0f));
            // Frosted glass / metallic bin body
            DrawRoundedRect(tex, 30, 16, 68, 78, 10, new Color(0.82f, 0.88f, 0.94f, 0.92f), new Color(0.58f, 0.66f, 0.74f, 0.96f));
            // Crumpled white paper bits visible inside
            DrawCircleAA(tex, 52, 68, 13, new Color(0.97f, 0.96f, 0.92f, 0.95f), new Color(0.84f, 0.82f, 0.76f, 0.95f));
            DrawCircleAA(tex, 74, 62, 14, new Color(0.95f, 0.94f, 0.88f, 0.95f), new Color(0.82f, 0.80f, 0.74f, 0.95f));
            // Vertical glass ribs
            DrawRoundedRect(tex, 44, 24, 6, 60, 3, new Color(0.42f, 0.49f, 0.56f, 0.65f), new Color(0.42f, 0.49f, 0.56f, 0.65f));
            DrawRoundedRect(tex, 61, 24, 6, 60, 3, new Color(0.42f, 0.49f, 0.56f, 0.65f), new Color(0.42f, 0.49f, 0.56f, 0.65f));
            DrawRoundedRect(tex, 78, 24, 6, 60, 3, new Color(0.42f, 0.49f, 0.56f, 0.65f), new Color(0.42f, 0.49f, 0.56f, 0.65f));
            // Top metallic rim & handle
            DrawRoundedRect(tex, 24, 92, 80, 12, 5, new Color(0.74f, 0.80f, 0.86f, 1f), new Color(0.54f, 0.60f, 0.68f, 1f));
            DrawRoundedRect(tex, 48, 102, 32, 10, 4, new Color(0.64f, 0.70f, 0.76f, 1f), new Color(0.50f, 0.56f, 0.64f, 1f));
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateStartLogoIcon()
        {
            Texture2D tex = NewIconCanvas(64);
            Color c1 = new Color(0.62f, 0.88f, 1f, 1f);
            Color c2 = new Color(0.38f, 0.74f, 0.98f, 1f);
            DrawRoundedRect(tex, 8, 34, 22, 22, 3, c1, c2);
            DrawRoundedRect(tex, 34, 34, 22, 22, 3, c1, c2);
            DrawRoundedRect(tex, 8, 8, 22, 22, 3, c1, c2);
            DrawRoundedRect(tex, 34, 8, 22, 22, 3, c1, c2);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateWifiIcon()
        {
            Texture2D tex = NewIconCanvas(64);
            Color white = new Color(0.95f, 0.97f, 1f, 1f);
            DrawCircleAA(tex, 32, 14, 5, white, white);
            DrawArcAA(tex, 32, 14, 16, 4.5f, 42f, 138f, white);
            DrawArcAA(tex, 32, 14, 28, 4.5f, 44f, 136f, white);
            DrawArcAA(tex, 32, 14, 40, 4.5f, 46f, 134f, white);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateVolumeIcon(bool muted)
        {
            Texture2D tex = NewIconCanvas(64);
            Color white = new Color(0.95f, 0.97f, 1f, 1f);
            // Speaker box + flared cone
            DrawRoundedRect(tex, 8, 23, 12, 18, 2, white, white);
            for (int x = 20; x <= 34; x++)
            {
                float t = (x - 20f) / 14f;
                int halfH = Mathf.RoundToInt(Mathf.Lerp(9f, 20f, t));
                for (int y = 32 - halfH; y <= 32 + halfH; y++)
                    BlendPixel(tex, x, y, white);
            }
            if (muted)
            {
                Color red = new Color(0.96f, 0.36f, 0.34f, 1f);
                DrawLineAA(tex, 42, 22, 58, 42, 4.5f, red);
                DrawLineAA(tex, 42, 42, 58, 22, 4.5f, red);
            }
            else
            {
                DrawArcAA(tex, 34, 32, 12, 3.8f, -45f, 45f, white);
                DrawArcAA(tex, 34, 32, 22, 3.8f, -48f, 48f, white);
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D NewIconCanvas(int size = 64)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color[] clear = new Color[size * size];
            for (int i = 0; i < clear.Length; i++) clear[i] = new Color(0, 0, 0, 0);
            tex.SetPixels(clear);
            return tex;
        }

        private static void BlendPixel(Texture2D tex, int x, int y, Color src)
        {
            if (x < 0 || y < 0 || x >= tex.width || y >= tex.height || src.a <= 0.001f) return;
            Color dst = tex.GetPixel(x, y);
            float outA = src.a + dst.a * (1f - src.a);
            if (outA <= 0.001f) return;
            Color res = new Color(
                (src.r * src.a + dst.r * dst.a * (1f - src.a)) / outA,
                (src.g * src.a + dst.g * dst.a * (1f - src.a)) / outA,
                (src.b * src.a + dst.b * dst.a * (1f - src.a)) / outA,
                outA
            );
            tex.SetPixel(x, y, res);
        }

        private static void DrawRoundedRect(Texture2D tex, int x0, int y0, int w, int h, float r, Color topCol, Color botCol)
        {
            for (int y = y0; y < y0 + h; y++)
            {
                float v = h > 1 ? (float)(y - y0) / (h - 1) : 0.5f;
                Color c = Color.Lerp(botCol, topCol, v);
                for (int x = x0; x < x0 + w; x++)
                {
                    float lx = x - x0;
                    float ly = y - y0;
                    float dx = lx < r ? (r - lx) : (lx > w - 1 - r ? lx - (w - 1 - r) : 0f);
                    float dy = ly < r ? (r - ly) : (ly > h - 1 - r ? ly - (h - 1 - r) : 0f);
                    float cov = 1f;
                    if (dx > 0f && dy > 0f)
                        cov = Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                    if (cov > 0.01f)
                        BlendPixel(tex, x, y, new Color(c.r, c.g, c.b, c.a * cov));
                }
            }
        }

        private static void DrawCircleAA(Texture2D tex, float cx, float cy, float r, Color topCol, Color botCol)
        {
            int minX = Mathf.Max(0, Mathf.FloorToInt(cx - r - 1));
            int maxX = Mathf.Min(tex.width - 1, Mathf.CeilToInt(cx + r + 1));
            int minY = Mathf.Max(0, Mathf.FloorToInt(cy - r - 1));
            int maxY = Mathf.Min(tex.height - 1, Mathf.CeilToInt(cy + r + 1));
            for (int y = minY; y <= maxY; y++)
            {
                float v = Mathf.Clamp01((y - (cy - r)) / (2f * r));
                Color c = Color.Lerp(botCol, topCol, v);
                for (int x = minX; x <= maxX; x++)
                {
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    float cov = Mathf.Clamp01(r - dist + 0.5f);
                    if (cov > 0.01f)
                        BlendPixel(tex, x, y, new Color(c.r, c.g, c.b, c.a * cov));
                }
            }
        }

        private static void DrawArcAA(Texture2D tex, float cx, float cy, float r, float thickness, float minDeg, float maxDeg, Color c)
        {
            float halfT = thickness * 0.5f;
            int minX = Mathf.Max(0, Mathf.FloorToInt(cx - r - halfT - 1));
            int maxX = Mathf.Min(tex.width - 1, Mathf.CeilToInt(cx + r + halfT + 1));
            int minY = Mathf.Max(0, Mathf.FloorToInt(cy - r - halfT - 1));
            int maxY = Mathf.Min(tex.height - 1, Mathf.CeilToInt(cy + r + halfT + 1));
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float dx = x - cx, dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float ringCov = Mathf.Clamp01(halfT - Mathf.Abs(dist - r) + 0.5f);
                    if (ringCov <= 0.01f) continue;
                    float ang = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                    if (ang >= minDeg && ang <= maxDeg)
                        BlendPixel(tex, x, y, new Color(c.r, c.g, c.b, c.a * ringCov));
                }
            }
        }

        private static void DrawLineAA(Texture2D tex, float x0, float y0, float x1, float y1, float thickness, Color c)
        {
            float halfT = thickness * 0.5f;
            int minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(x0, x1) - halfT - 1));
            int maxX = Mathf.Min(tex.width - 1, Mathf.CeilToInt(Mathf.Max(x0, x1) + halfT + 1));
            int minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(y0, y1) - halfT - 1));
            int maxY = Mathf.Min(tex.height - 1, Mathf.CeilToInt(Mathf.Max(y0, y1) + halfT + 1));
            Vector2 a = new Vector2(x0, y0), b = new Vector2(x1, y1), ab = b - a;
            float lenSq = ab.sqrMagnitude + 0.0001f;
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float t = Mathf.Clamp01(Vector2.Dot(new Vector2(x, y) - a, ab) / lenSq);
                    float dist = Vector2.Distance(new Vector2(x, y), a + ab * t);
                    float cov = Mathf.Clamp01(halfT - dist + 0.5f);
                    if (cov > 0.01f)
                        BlendPixel(tex, x, y, new Color(c.r, c.g, c.b, c.a * cov));
                }
            }
        }

        private static Sprite CreateRoundedSprite(int size, int radius)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color[] px = new Color[size * size];
            float r = radius;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x < r ? (r - x - 0.5f) : (x >= size - r ? x - (size - r) + 0.5f : 0f);
                    float dy = y < r ? (r - y - 0.5f) : (y >= size - r ? y - (size - r) + 0.5f : 0f);
                    float a = (dx > 0f && dy > 0f) ? Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f) : 1f;
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        private static void SetFill(RectTransform rt, float padX, float padY)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padX, padY); rt.offsetMax = new Vector2(-padX, -padY);
        }

        private RectTransform CreatePanel(Transform parent, string name, Color bgColor, bool raycast, bool rounded = false)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image img = go.AddComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = raycast;
            if (rounded && roundedSprite != null && bgColor.a > 0.01f)
            {
                img.sprite = roundedSprite;
                img.type = Image.Type.Sliced;
            }
            return go.GetComponent<RectTransform>();
        }

        private Text CreateText(Transform parent, string name, string content, int fontSize, Color color, TextAnchor align)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text txt = go.AddComponent<Text>();
            txt.font = uiFont; txt.text = content; txt.fontSize = fontSize;
            txt.color = color; txt.alignment = align; txt.supportRichText = true;
            txt.raycastTarget = false;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            return txt;
        }

        private Button CreateButton(Transform parent, string name, string label, Color bgColor, float height, UnityEngine.Events.UnityAction onClick, Color? textColor = null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, height);

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight = height; le.preferredHeight = height;

            Image img = go.AddComponent<Image>();
            img.color = bgColor;
            if (roundedSprite != null) { img.sprite = roundedSprite; img.type = Image.Type.Sliced; }

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            GameObject txtGo = new GameObject("Label");
            txtGo.transform.SetParent(go.transform, false);
            SetFill(txtGo.AddComponent<RectTransform>(), 8f, 2f);

            Text txt = txtGo.AddComponent<Text>();
            txt.font = uiFont; txt.text = label; txt.fontSize = 12;
            txt.color = textColor ?? Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Truncate;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = 10;
            txt.resizeTextMaxSize = 13;
            txt.raycastTarget = false;

            return btn;
        }
    }

    /// <summary>
    /// Papers, Please style draggable document handler that works cleanly on WorldSpace and ScreenSpace Canvases.
    /// Click to bring document to front; drag anywhere on the document or its header to slide & compare freely.
    /// </summary>
    public class DraggableDocWindow : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler
    {
        private RectTransform targetRect;
        private RectTransform boundsRect;
        private Vector2 pointerOffset;

        public void Init(RectTransform target, RectTransform bounds)
        {
            targetRect = target;
            boundsRect = bounds;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (targetRect != null) targetRect.SetAsLastSibling();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (targetRect == null || boundsRect == null) return;
            targetRect.SetAsLastSibling();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(boundsRect, eventData.position, eventData.pressEventCamera, out Vector2 localPt))
            {
                pointerOffset = targetRect.anchoredPosition - localPt;
            }
            SoundManager.Instance?.Play("paper_rustle", 0.35f);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (targetRect == null || boundsRect == null) return;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(boundsRect, eventData.position, eventData.pressEventCamera, out Vector2 localPt))
            {
                Vector2 desired = localPt + pointerOffset;
                Vector2 halfBounds = boundsRect.rect.size * 0.5f;
                Vector2 halfCard = targetRect.rect.size * 0.5f;
                float maxX = Mathf.Max(20f, halfBounds.x - halfCard.x * 0.35f);
                float maxY = Mathf.Max(16f, halfBounds.y - halfCard.y * 0.35f);
                desired.x = Mathf.Clamp(desired.x, -maxX, maxX);
                desired.y = Mathf.Clamp(desired.y, -maxY, maxY);
                targetRect.anchoredPosition = desired;
            }
        }
    }
}
