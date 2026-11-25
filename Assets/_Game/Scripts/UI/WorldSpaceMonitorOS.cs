using System;
using UnityEngine;
using UnityEngine.UI;

namespace HealthFiles
{
    /// <summary>
    /// WP2 + WP3: REAL World-Space 'HealthOS' Desktop Canvas mounted directly onto the 3D iMac Monitor!
    /// - Runs LIVE in the background even while you are sitting at your Desk looking around the room.
    /// - When you click the 3D Monitor, the camera smoothly zooms right into the monitor bezel,
    ///   disables the outer BoxCollider, and lets you click directly on the 3D screen's Desktop Icons,
    ///   HealthBrowser, PrivacyPolicy PDF, and Taskbar!
    /// - When you click 'Step Back' (or Esc), the camera pulls back to Desk and your open OS window stays live!
    /// </summary>
    public class WorldSpaceMonitorOS : MonoBehaviour
    {
        public static WorldSpaceMonitorOS Instance { get; private set; }

        [Header("3D Monitor Binding")]
        [SerializeField] private BoxCollider outerMonitorCollider;
        [SerializeField] private Vector2 screenWorldSize = new Vector2(0.56f, 0.33f);

        private Font uiFont;
        private Canvas worldCanvas;
        private GraphicRaycaster worldRaycaster;

        // OS UI elements
        private GameObject osWindowFrame;
        private Text osWindowTitle;
        private GameObject osNewsAppContent;
        private GameObject osBrowserAppContent;
        private GameObject osPolicyPdfContent;
        private GameObject osNotesAppContent;
        private GameObject osBrowserLockedBox;
        private GameObject osBrowserGameBox;
        private Text osClockText;
        private Text osTaskbarActiveApp;
        private Text computerFeedbackText;
        private Button completeInspectionButton;

        private readonly bool[] redFlagsFound = new bool[3];
        private readonly Text[] passageLabels = new Text[6];
        private readonly Image[] passageBackgrounds = new Image[6];

        private struct BlogPassage
        {
            public string text;
            public bool isRedFlag;
            public int redFlagIndex;
            public string distractorHint;
        }

        private readonly BlogPassage[] blogPassages =
        {
            new BlogPassage {
                text = "HEADLINE: \"EXPOSED! VitaPulse Secretly Sells EVERYONE'S Medical Records!\"",
                isRedFlag = true, redFlagIndex = 0
            },
            new BlogPassage {
                text = "AUTHOR: Posted by [Anonymous] — No name, credentials, or institution.",
                isRedFlag = true, redFlagIndex = 1
            },
            new BlogPassage {
                text = "CONTEXT: \"VitaPulse is a popular health diary downloaded by millions.\"",
                isRedFlag = false, redFlagIndex = -1,
                distractorHint = "That's just background context—look for unsupported accusations or missing sources!"
            },
            new BlogPassage {
                text = "SOURCES: \"Everyone online knows it's true. No proof or links needed!\"",
                isRedFlag = true, redFlagIndex = 2
            },
            new BlogPassage {
                text = "POLICY QUOTE: \"Section 4.2 mentions sharing some data with third-party partners.\"",
                isRedFlag = false, redFlagIndex = -1,
                distractorHint = "Section 4.2 actually DOES say that! Look for where the blog goes beyond the policy."
            },
            new BlogPassage {
                text = "ADVICE: \"Always check an app's privacy policy before installing it.\"",
                isRedFlag = false, redFlagIndex = -1,
                distractorHint = "That's reasonable advice—find the 3 unsupported weaknesses instead!"
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
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildWorldSpaceDesktop();
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

            // Show the Viral News Alerts window live on the monitor right from Day 1 so the 3D screen looks alive!
            OpenOSApp("news", playSound: false);
        }

        private void OnDestroy()
        {
            if (CameraViewpointController.Instance != null)
            {
                CameraViewpointController.Instance.OnViewpointChanged -= HandleViewpointChanged;
            }
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnAlexComputerOpened -= FocusComputerScreen;
            }
        }

        private void Update()
        {
            if (worldCanvas != null && worldCanvas.worldCamera == null && Camera.main != null)
            {
                worldCanvas.worldCamera = Camera.main;
            }
            if (osClockText != null)
            {
                osClockText.text = DateTime.Now.ToString("HH:mm");
            }
        }

        private void HandleViewpointChanged(CameraViewpoint vp)
        {
            bool zoomedOnPC = CameraViewpointController.Instance != null &&
                              CameraViewpointController.Instance.CurrentViewpointId == "Computer";

            // While sitting at Desk: disable WorldSpace GraphicRaycaster so clicking ANYWHERE on the monitor
            // hits outerMonitorCollider and zooms smoothly in!
            // While zoomed on Computer: disable outerMonitorCollider and enable GraphicRaycaster so every pixel is clickable!
            if (outerMonitorCollider != null)
            {
                outerMonitorCollider.enabled = !zoomedOnPC;
            }
            if (worldRaycaster != null)
            {
                worldRaycaster.enabled = zoomedOnPC;
            }
        }

        public void FocusComputerScreen()
        {
            CameraViewpointController.Instance?.MoveToViewpoint("Computer");

            // If player already found Alex's letter and is on the inspection step, open HealthBrowser automatically!
            if (MissionManager.Instance != null && MissionManager.Instance.CurrentState == MissionState.Alex_InspectBlogAndPolicy)
            {
                OpenOSApp("browser", playSound: false);
            }
        }

        private void OpenOSApp(string appId, bool playSound = true)
        {
            if (playSound) SoundManager.Instance?.Play("ui_click", 0.5f);
            osWindowFrame.SetActive(true);
            osNewsAppContent.SetActive(false);
            osBrowserAppContent.SetActive(false);
            osPolicyPdfContent.SetActive(false);
            osNotesAppContent.SetActive(false);

            switch (appId)
            {
                case "news":
                    osWindowTitle.text = "VitaPulse News Feed — Alerts.exe";
                    osTaskbarActiveApp.text = "Alerts.exe";
                    osNewsAppContent.SetActive(true);
                    break;
                case "browser":
                    osWindowTitle.text = "HealthBrowser — http://truth-health-watch.blog/vitapulse-exposed";
                    osTaskbarActiveApp.text = "HealthBrowser";
                    osBrowserAppContent.SetActive(true);
                    bool hasLetter = EvidenceManager.Instance != null && EvidenceManager.Instance.HasEvidence("letter_alex");
                    osBrowserLockedBox.SetActive(!hasLetter);
                    osBrowserGameBox.SetActive(hasLetter);
                    if (hasLetter) UpdateBlogInspectionState();
                    break;
                case "policy":
                    osWindowTitle.text = "PDF Reader — VitaPulse_Privacy_Policy_v4.2.pdf";
                    osTaskbarActiveApp.text = "PrivacyPolicy.pdf";
                    osPolicyPdfContent.SetActive(true);
                    break;
                case "notes":
                    osWindowTitle.text = "CaseNotes.txt — Digital Health Investigation Office";
                    osTaskbarActiveApp.text = "CaseNotes.txt";
                    osNotesAppContent.SetActive(true);
                    break;
            }
        }

        private void OnBlogPassageClicked(int idx)
        {
            BlogPassage p = blogPassages[idx];
            if (p.isRedFlag)
            {
                if (!redFlagsFound[p.redFlagIndex])
                {
                    redFlagsFound[p.redFlagIndex] = true;
                    SoundManager.Instance?.Play("evidence_unlock", 0.55f);
                    computerFeedbackText.color = new Color(0.45f, 1f, 0.55f);
                    computerFeedbackText.text = "Flagged! That part of the blog is unsupported.";
                }
            }
            else
            {
                MissionManager.Instance?.RecordWrongBlogSelection();
                computerFeedbackText.color = new Color(1f, 0.65f, 0.35f);
                computerFeedbackText.text = p.distractorHint;
            }
            UpdateBlogInspectionState();
        }

        private void UpdateBlogInspectionState()
        {
            int count = 0;
            foreach (bool b in redFlagsFound) if (b) count++;

            for (int i = 0; i < blogPassages.Length; i++)
            {
                BlogPassage p = blogPassages[i];
                if (p.isRedFlag && redFlagsFound[p.redFlagIndex])
                {
                    passageLabels[i].text = "[FLAGGED] " + p.text;
                    passageLabels[i].color = new Color(1f, 0.95f, 0.72f);
                    passageBackgrounds[i].color = new Color(0.56f, 0.16f, 0.16f, 1f);
                }
                else
                {
                    passageLabels[i].text = p.text;
                    passageLabels[i].color = Color.white;
                    passageBackgrounds[i].color = new Color(0.16f, 0.21f, 0.28f, 0.96f);
                }
            }

            bool collected = EvidenceManager.Instance != null && EvidenceManager.Instance.HasEvidence("blog_unverified");
            completeInspectionButton.interactable = (count == 3) && !collected;
            completeInspectionButton.GetComponentInChildren<Text>().text = collected
                ? "Collected: Unverified Blog Claim (Step Back & Call Alex!)"
                : (count == 3 ? "CREATE EVIDENCE CARD: Unverified Blog Claim" : $"Click the 3 unsupported weaknesses ({count}/3)");
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
            worldRaycaster.enabled = false; // Enabled automatically when zoomed into Computer viewpoint

            RectTransform crt = canvasGo.GetComponent<RectTransform>();
            crt.sizeDelta = new Vector2(resW, resH);
            crt.localScale = new Vector3(screenWorldSize.x / resW, screenWorldSize.y / resH, 1f);

            // Wallpaper
            RectTransform wall = CreatePanel(canvasGo.transform, "OS_Wallpaper", new Color(0.09f, 0.22f, 0.38f, 1f), raycast: false);
            SetFill(wall, 0, 0);

            // Desktop Icons (left column)
            RectTransform iconCol = CreatePanel(wall, "DesktopIcons", new Color(0, 0, 0, 0), raycast: false);
            iconCol.anchorMin = new Vector2(0f, 1f); iconCol.anchorMax = new Vector2(0f, 1f); iconCol.pivot = new Vector2(0f, 1f);
            iconCol.sizeDelta = new Vector2(146f, 360f);
            iconCol.anchoredPosition = new Vector2(14f, -14f);
            VerticalLayoutGroup icLay = iconCol.gameObject.AddComponent<VerticalLayoutGroup>();
            icLay.spacing = 10; icLay.childControlHeight = false; icLay.childForceExpandHeight = false;

            CreateButton(iconCol, "Icon_Browser", "HealthBrowser\n(Blog Check)", new Color(0.16f, 0.52f, 0.72f, 0.96f), 68f, () => OpenOSApp("browser"));
            CreateButton(iconCol, "Icon_News",    "VitaPulse\nViral Alerts",    new Color(0.74f, 0.24f, 0.22f, 0.96f), 68f, () => OpenOSApp("news"));
            CreateButton(iconCol, "Icon_Policy",  "PrivacyPolicy\nv4.2.pdf",    new Color(0.24f, 0.44f, 0.34f, 0.96f), 68f, () => OpenOSApp("policy"));
            CreateButton(iconCol, "Icon_Notes",   "CaseNotes.txt\n(Office)",    new Color(0.46f, 0.40f, 0.22f, 0.96f), 68f, () => OpenOSApp("notes"));

            // Application Window Frame
            RectTransform win = CreatePanel(wall, "OS_AppWindow", new Color(0.08f, 0.11f, 0.16f, 0.99f), raycast: true);
            win.anchorMin = new Vector2(0.165f, 0.095f);
            win.anchorMax = new Vector2(0.985f, 0.975f);
            win.offsetMin = Vector2.zero; win.offsetMax = Vector2.zero;

            RectTransform titleBar = CreatePanel(win, "WinTitleBar", new Color(0.12f, 0.18f, 0.28f, 1f), raycast: false);
            titleBar.anchorMin = new Vector2(0f, 1f); titleBar.anchorMax = new Vector2(1f, 1f); titleBar.pivot = new Vector2(0.5f, 1f);
            titleBar.sizeDelta = new Vector2(0f, 34f); titleBar.anchoredPosition = Vector2.zero;

            osWindowTitle = CreateText(titleBar, "WinTitle", "Window", 14, Color.white, TextAnchor.MiddleLeft);
            osWindowTitle.fontStyle = FontStyle.Bold;
            SetFill(osWindowTitle.GetComponent<RectTransform>(), 12, 2);

            Button winClose = CreateButton(titleBar, "WinCloseX", "X", new Color(0.76f, 0.18f, 0.18f, 1f), 26f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                osWindowFrame.SetActive(false);
                osTaskbarActiveApp.text = "Desktop";
            });
            RectTransform wcRect = winClose.GetComponent<RectTransform>();
            wcRect.anchorMin = new Vector2(1f, 0.5f); wcRect.anchorMax = new Vector2(1f, 0.5f); wcRect.pivot = new Vector2(1f, 0.5f);
            wcRect.sizeDelta = new Vector2(34f, 26f); wcRect.anchoredPosition = new Vector2(-4f, 0f);

            RectTransform winBody = CreatePanel(win, "WinBody", new Color(0, 0, 0, 0), raycast: false);
            winBody.anchorMin = Vector2.zero; winBody.anchorMax = Vector2.one;
            winBody.offsetMin = new Vector2(12, 10); winBody.offsetMax = new Vector2(-12, -40);

            // App 1: News
            RectTransform newsObj = CreatePanel(winBody, "App_News", new Color(0.10f, 0.14f, 0.21f, 1f), raycast: false);
            SetFill(newsObj, 0, 0);
            Text nTxt = CreateText(newsObj, "NT",
                "<b>VITAPULSE — LIVE TRENDING HEALTH ALERTS</b>\n\n" +
                "•  <b>[VIRAL]</b> \"VitaPulse Can Detect Serious Diseases Early!\"\n" +
                "•  <b>[SOCIAL]</b> \"Millions Download the New Health App!\"\n" +
                "•  <b>[BLOG]</b> \"Is VitaPulse Secretly Selling Everyone's Medical Records?\"\n\n" +
                "<i>Click 'HealthBrowser' on the left desktop icons once you collect Alex's Letter from Cabinet Drawer B-02!</i>",
                16, Color.white, TextAnchor.UpperLeft);
            SetFill(nTxt.GetComponent<RectTransform>(), 18, 16);
            osNewsAppContent = newsObj.gameObject;

            // App 2: Policy PDF
            RectTransform polObj = CreatePanel(winBody, "App_Policy", new Color(0.95f, 0.95f, 0.93f, 1f), raycast: false);
            SetFill(polObj, 0, 0);
            Text pTxt = CreateText(polObj, "PT",
                "<b>VITAPULSE — OFFICIAL PRIVACY POLICY (v4.2)</b>\n\n" +
                "<b>Section 4.2 — Third-Party Data Sharing:</b>\n" +
                "\"Some user information may be shared with third-party partners to operate, analyze, and improve app services.\"\n\n" +
                "<b>Investigator Legal Note:</b>\n" +
                "• Does NOT specify which third-party partners receive data (legitimate privacy concern!).\n" +
                "• Does NOT establish a secret sale of all private medical records.",
                16, new Color(0.12f, 0.14f, 0.18f), TextAnchor.UpperLeft);
            SetFill(pTxt.GetComponent<RectTransform>(), 20, 18);
            osPolicyPdfContent = polObj.gameObject;

            // App 3: Case Notes
            RectTransform notesObj = CreatePanel(winBody, "App_Notes", new Color(0.10f, 0.12f, 0.16f, 1f), raycast: false);
            SetFill(notesObj, 0, 0);
            Text cTxt = CreateText(notesObj, "CT",
                "<b>DIGITAL HEALTH INVESTIGATION OFFICE — CASE WORKSPACE</b>\n\n" +
                "• Case 01 — Alex Moretti (Active): Blog claim vs Privacy Policy Section 4.2\n" +
                "• Case 02 — Sofia Romano (WP4): Viral influencer video & App permissions\n" +
                "• Case 03 — Daniel Conti (WP5): Official health portal & Altered screenshot\n" +
                "• Final Case (WP5): Connect all clues on the Cork Board & publish public report.",
                16, new Color(0.86f, 0.92f, 0.98f), TextAnchor.UpperLeft);
            SetFill(cTxt.GetComponent<RectTransform>(), 18, 16);
            osNotesAppContent = notesObj.gameObject;

            // App 4: HealthBrowser (Blog vs Policy Papers, Please Mini-Game!)
            GameObject browserObj = new GameObject("App_Browser");
            browserObj.transform.SetParent(winBody, false);
            SetFill(browserObj.AddComponent<RectTransform>(), 0, 0);

            RectTransform lockBox = CreatePanel(browserObj.transform, "BrowserLocked", new Color(0.14f, 0.12f, 0.10f, 1f), raycast: false);
            SetFill(lockBox, 18, 50);
            Text lTxt = CreateText(lockBox, "LT",
                "<b>NO BLOG URL ENTERED YET</b>\n\n" +
                "To unlock Alex's viral blog:\n" +
                "1. Answer the Office Phone (Supervisor -> Investigation Manual -> Alex).\n" +
                "2. Check the Mail Delivery Log on your desk.\n" +
                "3. Open Filing Cabinet Drawer B-02 and collect Alex's Yellow Envelope!",
                16, new Color(1f, 0.88f, 0.48f), TextAnchor.MiddleCenter);
            SetFill(lTxt.GetComponent<RectTransform>(), 18, 14);
            osBrowserLockedBox = lockBox.gameObject;

            GameObject gameBox = new GameObject("BrowserGame");
            gameBox.transform.SetParent(browserObj.transform, false);
            SetFill(gameBox.AddComponent<RectTransform>(), 0, 0);
            VerticalLayoutGroup gLay = gameBox.AddComponent<VerticalLayoutGroup>();
            gLay.spacing = 6; gLay.childControlHeight = false; gLay.childForceExpandHeight = false;

            RectTransform urlBar = CreatePanel(gameBox.transform, "UrlBar", new Color(0.14f, 0.18f, 0.24f, 1f), raycast: false);
            urlBar.sizeDelta = new Vector2(0f, 26f);
            Text urlTxt = CreateText(urlBar, "UrlTxt", "  [Not Secure]  http://truth-health-watch.blog/vitapulse-exposed   |   PrivacyPolicy_v4.2.pdf", 12, new Color(0.85f, 0.90f, 0.95f), TextAnchor.MiddleLeft);
            SetFill(urlTxt.GetComponent<RectTransform>(), 8, 2);

            GameObject cols = new GameObject("Cols");
            cols.transform.SetParent(gameBox.transform, false);
            cols.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 340f);
            HorizontalLayoutGroup cLay = cols.AddComponent<HorizontalLayoutGroup>();
            cLay.spacing = 8; cLay.childControlWidth = true; cLay.childForceExpandWidth = true;

            RectTransform leftCol = CreatePanel(cols.transform, "BlogCol", new Color(0.14f, 0.09f, 0.11f, 1f), raycast: true);
            VerticalLayoutGroup bLay = leftCol.gameObject.AddComponent<VerticalLayoutGroup>();
            bLay.padding = new RectOffset(8, 8, 6, 6); bLay.spacing = 5;
            bLay.childControlHeight = false; bLay.childForceExpandHeight = false;

            Text bHdr = CreateText(leftCol, "BH", "<b>BLOG POST — Click the 3 Unsupported Weaknesses!</b>", 12, new Color(1f, 0.72f, 0.72f), TextAnchor.MiddleLeft);
            bHdr.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 20f);

            for (int i = 0; i < blogPassages.Length; i++)
            {
                int idx = i;
                Button pBtn = CreateButton(leftCol, $"Passage_{i}", blogPassages[i].text, new Color(0.16f, 0.21f, 0.28f, 1f), 46f, () => OnBlogPassageClicked(idx));
                Text t = pBtn.GetComponentInChildren<Text>();
                t.alignment = TextAnchor.MiddleLeft;
                t.fontSize = 11;
                passageLabels[i] = t;
                passageBackgrounds[i] = pBtn.GetComponent<Image>();
            }

            RectTransform rightCol = CreatePanel(cols.transform, "PolicyCol", new Color(0.95f, 0.95f, 0.93f, 1f), raycast: false);
            VerticalLayoutGroup pLay = rightCol.gameObject.AddComponent<VerticalLayoutGroup>();
            pLay.padding = new RectOffset(12, 12, 10, 10);
            CreateText(rightCol, "PolTxt",
                "<b>VITAPULSE PRIVACY POLICY — SECTION 4.2</b>\n\n" +
                "\"Some user information may be shared with third-party partners to operate and improve services.\"\n\n" +
                "<b>Compare Side-by-Side:</b>\n" +
                "• Does this prove a secret sale of ALL medical records? <b>NO.</b>\n" +
                "• Does it raise a real privacy question? <b>YES.</b>\n\n" +
                "<i>Click ONLY the 3 unsupported weaknesses on the left!</i>",
                13, new Color(0.12f, 0.14f, 0.18f), TextAnchor.UpperLeft);

            computerFeedbackText = CreateText(gameBox.transform, "PCFb", "Compare both documents and click the 3 unsupported weaknesses.", 13, new Color(1f, 0.9f, 0.48f), TextAnchor.MiddleLeft);
            computerFeedbackText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 26f);

            completeInspectionButton = CreateButton(gameBox.transform, "StampBtn", "Click the 3 unsupported weaknesses (0/3)", new Color(0.15f, 0.56f, 0.34f, 1f), 36f, () =>
            {
                MissionManager.Instance?.CompleteAlexBlogInspection();
                UpdateBlogInspectionState();
            });

            osBrowserGameBox = gameBox;
            osBrowserAppContent = browserObj;
            osWindowFrame = win.gameObject;

            // Bottom Windows Taskbar
            RectTransform taskbar = CreatePanel(wall, "OS_Taskbar", new Color(0.06f, 0.08f, 0.12f, 0.98f), raycast: true);
            taskbar.anchorMin = new Vector2(0f, 0f); taskbar.anchorMax = new Vector2(1f, 0f); taskbar.pivot = new Vector2(0.5f, 0f);
            taskbar.sizeDelta = new Vector2(0f, 38f); taskbar.anchoredPosition = Vector2.zero;

            Button startBtn = CreateButton(taskbar, "StartBtn", "HealthOS", new Color(0.14f, 0.36f, 0.62f, 1f), 28f, () => OpenOSApp("notes"));
            RectTransform sbRect = startBtn.GetComponent<RectTransform>();
            sbRect.anchorMin = new Vector2(0f, 0.5f); sbRect.anchorMax = new Vector2(0f, 0.5f); sbRect.pivot = new Vector2(0f, 0.5f);
            sbRect.sizeDelta = new Vector2(100f, 28f); sbRect.anchoredPosition = new Vector2(8f, 0f);

            osTaskbarActiveApp = CreateText(taskbar, "ActiveApp", "Desktop", 13, new Color(0.75f, 0.86f, 0.96f), TextAnchor.MiddleLeft);
            RectTransform aaRect = osTaskbarActiveApp.GetComponent<RectTransform>();
            aaRect.anchorMin = new Vector2(0f, 0.5f); aaRect.anchorMax = new Vector2(0f, 0.5f); aaRect.pivot = new Vector2(0f, 0.5f);
            aaRect.sizeDelta = new Vector2(200f, 28f); aaRect.anchoredPosition = new Vector2(120f, 0f);

            Button stepBack = CreateButton(taskbar, "StepBackPC", "↓ Step Back (Esc)", new Color(0.68f, 0.22f, 0.20f, 1f), 28f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                CameraViewpointController.Instance?.MoveToViewpoint("Desk");
            });
            RectTransform stRect = stepBack.GetComponent<RectTransform>();
            stRect.anchorMin = new Vector2(1f, 0.5f); stRect.anchorMax = new Vector2(1f, 0.5f); stRect.pivot = new Vector2(1f, 0.5f);
            stRect.sizeDelta = new Vector2(150f, 28f); stRect.anchoredPosition = new Vector2(-82f, 0f);

            osClockText = CreateText(taskbar, "Clock", "22:00", 13, Color.white, TextAnchor.MiddleRight);
            RectTransform clRect = osClockText.GetComponent<RectTransform>();
            clRect.anchorMin = new Vector2(1f, 0.5f); clRect.anchorMax = new Vector2(1f, 0.5f); clRect.pivot = new Vector2(1f, 0.5f);
            clRect.sizeDelta = new Vector2(68f, 28f); clRect.anchoredPosition = new Vector2(-10f, 0f);
        }

        private static void SetFill(RectTransform rt, float padX, float padY)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padX, padY); rt.offsetMax = new Vector2(-padX, -padY);
        }

        private RectTransform CreatePanel(Transform parent, string name, Color bgColor, bool raycast)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image img = go.AddComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = raycast;
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

        private Button CreateButton(Transform parent, string name, string label, Color bgColor, float height, UnityEngine.Events.UnityAction onClick)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, height);

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight = height; le.preferredHeight = height;

            Image img = go.AddComponent<Image>();
            img.color = bgColor;

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            GameObject txtGo = new GameObject("Label");
            txtGo.transform.SetParent(go.transform, false);
            SetFill(txtGo.AddComponent<RectTransform>(), 8f, 2f);

            Text txt = txtGo.AddComponent<Text>();
            txt.font = uiFont; txt.text = label; txt.fontSize = 13;
            txt.color = Color.white; txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;

            return btn;
        }
    }
}
