using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HealthFiles
{
    /// <summary>
    /// WP3: Complete starter HUD for the Investigation Office + Alex Vertical Slice.
    /// Builds its clean Canvas UI automatically at runtime so there are zero missing Inspector links,
    /// while keeping everything modular so WP3/WP4/WP5 can style or replace panels later.
    /// </summary>
    public class CaseZeroHUD : MonoBehaviour
    {
        private Font uiFont;
        private Canvas rootCanvas;

        // Top HUD
        private Text objectiveText;
        private Text dhliProfileText;
        private Text statusToastText;
        private Text hoverLabelText;

        // Inventory panel
        private Transform inventoryListParent;
        private Text evidenceDetailText;

        // Phone / Objection modal
        private GameObject phonePanel;
        private Text phoneSpeakerText;
        private Text phoneBodyText;
        private Text phoneFeedbackText;
        private Transform objectionButtonContainer;

        // Alex Computer Compare modal (Papers, Please style)
        private GameObject computerPanel;
        private readonly bool[] blogFlagsMarked = new bool[3];
        private readonly Text[] blogFlagLabels = new Text[3];
        private Button completeInspectionButton;

        private readonly string[] blogFlagTexts =
        {
            "Flag 1: Author is completely unidentified (no credentials or name)",
            "Flag 2: Zero traceable sources or clinical links provided",
            "Flag 3: Headline claims 'secret sale of ALL records' beyond policy text"
        };

        private void Awake()
        {
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildCanvasUI();
        }

        private void Start()
        {
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnMissionChanged += HandleMissionChanged;
                MissionManager.Instance.OnStatusMessage += HandleStatusMessage;
                MissionManager.Instance.OnPhoneCallRequested += HandlePhoneCallRequested;
                MissionManager.Instance.OnAlexComputerOpened += OpenAlexComputerPanel;

                HandleMissionChanged(
                    MissionManager.Instance.CurrentState,
                    MissionManager.GetObjectiveText(MissionManager.Instance.CurrentState)
                );
            }

            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.OnInventoryChanged += RefreshInventoryUI;
                RefreshInventoryUI();
            }

            if (PersonalizationManager.Instance != null)
            {
                PersonalizationManager.Instance.OnProfileUpdated += RefreshDhliProfile;
                RefreshDhliProfile();
            }

            if (OfficeRaycaster.Instance != null)
            {
                OfficeRaycaster.Instance.OnHoverLabelChanged += HandleHoverChanged;
            }
        }

        private void OnDestroy()
        {
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnMissionChanged -= HandleMissionChanged;
                MissionManager.Instance.OnStatusMessage -= HandleStatusMessage;
                MissionManager.Instance.OnPhoneCallRequested -= HandlePhoneCallRequested;
                MissionManager.Instance.OnAlexComputerOpened -= OpenAlexComputerPanel;
            }

            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.OnInventoryChanged -= RefreshInventoryUI;
            }

            if (PersonalizationManager.Instance != null)
            {
                PersonalizationManager.Instance.OnProfileUpdated -= RefreshDhliProfile;
            }

            if (OfficeRaycaster.Instance != null)
            {
                OfficeRaycaster.Instance.OnHoverLabelChanged -= HandleHoverChanged;
            }
        }

        private void HandleMissionChanged(MissionState state, string objective)
        {
            if (objectiveText != null)
            {
                objectiveText.text = objective;
            }
        }

        private void HandleStatusMessage(string msg)
        {
            if (statusToastText != null)
            {
                statusToastText.text = msg;
            }
        }

        private void HandleHoverChanged(string label)
        {
            if (hoverLabelText != null)
            {
                hoverLabelText.text = string.IsNullOrEmpty(label) ? "" : $"[Click] {label}";
            }
        }

        private void RefreshDhliProfile()
        {
            if (dhliProfileText == null || PersonalizationManager.Instance == null)
                return;

            var pm = PersonalizationManager.Instance;
            dhliProfileText.text = $"DHLI Profile  |  Reliability: {pm.reliabilityScore}  |  Privacy: {pm.privacyScore}  |  Navigation: {pm.navigationScore}";
        }

        private void RefreshInventoryUI()
        {
            if (inventoryListParent == null || EvidenceManager.Instance == null)
                return;

            foreach (Transform child in inventoryListParent)
            {
                Destroy(child.gameObject);
            }

            IReadOnlyList<EvidenceItem> items = EvidenceManager.Instance.CollectedEvidence;
            if (items.Count == 0)
            {
                CreateText(inventoryListParent, "EmptyHint", "(No evidence collected yet)", 13, new Color(0.75f, 0.8f, 0.85f), TextAnchor.UpperLeft);
                return;
            }

            foreach (EvidenceItem item in items)
            {
                EvidenceItem captured = item;
                Button btn = CreateButton(
                    inventoryListParent,
                    $"Inv_{item.id}",
                    $"• {item.title}",
                    new Color(0.18f, 0.28f, 0.42f, 0.95f),
                    34f,
                    () =>
                    {
                        if (evidenceDetailText != null)
                        {
                            evidenceDetailText.text = $"<b>{captured.title}</b> ({captured.id})\nSource: {captured.source}\n\n{captured.summary}";
                        }
                    }
                );
                btn.GetComponentInChildren<Text>().alignment = TextAnchor.MiddleLeft;
            }
        }

        private void HandlePhoneCallRequested(string speaker, string body, bool allowObjection)
        {
            phonePanel.SetActive(true);
            computerPanel.SetActive(false);

            phoneSpeakerText.text = $"TELEPHONE CALL — {speaker}";
            phoneBodyText.text = body;
            phoneFeedbackText.text = "";

            foreach (Transform child in objectionButtonContainer)
            {
                Destroy(child.gameObject);
            }

            if (allowObjection && EvidenceManager.Instance != null)
            {
                CreateText(
                    objectionButtonContainer,
                    "ObjTitle",
                    "PRESENT EVIDENCE TO TRIGGER OBJECTION!:",
                    14,
                    new Color(1f, 0.85f, 0.35f),
                    TextAnchor.MiddleLeft
                );

                foreach (EvidenceItem item in EvidenceManager.Instance.CollectedEvidence)
                {
                    EvidenceItem captured = item;
                    CreateButton(
                        objectionButtonContainer,
                        $"Present_{item.id}",
                        $"OBJECTION! Present: {item.title}",
                        new Color(0.72f, 0.22f, 0.18f, 1f),
                        38f,
                        () => OnPresentEvidenceClicked(captured.id)
                    );
                }
            }
        }

        private void OnPresentEvidenceClicked(string evidenceId)
        {
            if (MissionManager.Instance == null) return;

            bool success = MissionManager.Instance.TrySubmitObjection(evidenceId, out string response, out string hint);
            if (success)
            {
                phoneBodyText.text = response;
                phoneFeedbackText.color = new Color(0.45f, 1f, 0.55f);
                phoneFeedbackText.text = "OBJECTION! Claim challenged using verifiable evidence.";

                foreach (Transform child in objectionButtonContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            else
            {
                phoneFeedbackText.color = new Color(1f, 0.72f, 0.35f);
                phoneFeedbackText.text = $"{response}\n\n{hint}";
            }
        }

        private void OpenAlexComputerPanel()
        {
            computerPanel.SetActive(true);
            phonePanel.SetActive(false);
            UpdateBlogInspectionState();
        }

        private void ToggleBlogFlag(int index)
        {
            blogFlagsMarked[index] = !blogFlagsMarked[index];
            UpdateBlogInspectionState();
        }

        private void UpdateBlogInspectionState()
        {
            int count = 0;
            for (int i = 0; i < blogFlagsMarked.Length; i++)
            {
                if (blogFlagsMarked[i]) count++;
                if (blogFlagLabels[i] != null)
                {
                    string box = blogFlagsMarked[i] ? "[X]" : "[ ]";
                    blogFlagLabels[i].text = $"{box} {blogFlagTexts[i]}";
                    blogFlagLabels[i].color = blogFlagsMarked[i] ? new Color(0.45f, 1f, 0.55f) : Color.white;
                }
            }

            if (completeInspectionButton != null)
            {
                bool alreadyCollected = EvidenceManager.Instance != null && EvidenceManager.Instance.HasEvidence("blog_unverified");
                completeInspectionButton.interactable = (count == 3) && !alreadyCollected;
                completeInspectionButton.GetComponentInChildren<Text>().text = alreadyCollected
                    ? "Card Collected: Unverified Blog Claim (Close & Call Alex!)"
                    : (count == 3
                        ? "COLLECT EVIDENCE CARD: Unverified Blog Claim"
                        : $"Mark all 3 unsupported blog claims ({count}/3)");
            }
        }

        private void BuildCanvasUI()
        {
            GameObject canvasGo = new GameObject("CaseZero_HUD_Canvas");
            rootCanvas = canvasGo.AddComponent<Canvas>();
            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvas.sortingOrder = 10;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            canvasGo.AddComponent<GraphicRaycaster>();

            // 1. Top Mission & DHLI Header
            RectTransform topBar = CreatePanel(canvasGo.transform, "TopBar", new Color(0.08f, 0.12f, 0.18f, 0.90f));
            topBar.anchorMin = new Vector2(0f, 1f);
            topBar.anchorMax = new Vector2(1f, 1f);
            topBar.pivot = new Vector2(0.5f, 1f);
            topBar.sizeDelta = new Vector2(0f, 84f);
            topBar.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup topLayout = topBar.gameObject.AddComponent<VerticalLayoutGroup>();
            topLayout.padding = new RectOffset(16, 16, 8, 8);
            topLayout.spacing = 4;

            objectiveText = CreateText(topBar, "ObjectiveText", "Loading mission...", 18, new Color(1f, 0.92f, 0.45f), TextAnchor.MiddleLeft);
            objectiveText.fontStyle = FontStyle.Bold;
            dhliProfileText = CreateText(topBar, "DhliProfileText", "DHLI Profile", 13, new Color(0.72f, 0.88f, 1f), TextAnchor.MiddleLeft);
            statusToastText = CreateText(topBar, "StatusToast", "", 13, Color.white, TextAnchor.MiddleLeft);

            // 2. Hover Tooltip Label
            RectTransform hoverBar = CreatePanel(canvasGo.transform, "HoverBar", new Color(0f, 0f, 0f, 0f));
            hoverBar.anchorMin = new Vector2(0.25f, 0f);
            hoverBar.anchorMax = new Vector2(0.75f, 0f);
            hoverBar.pivot = new Vector2(0.5f, 0f);
            hoverBar.sizeDelta = new Vector2(0f, 34f);
            hoverBar.anchoredPosition = new Vector2(0f, 64f);
            hoverLabelText = CreateText(hoverBar, "HoverLabel", "", 18, new Color(1f, 0.95f, 0.55f), TextAnchor.MiddleCenter);
            hoverLabelText.fontStyle = FontStyle.Bold;

            // 3. Bottom Viewpoint Switcher Bar (WP2)
            RectTransform bottomBar = CreatePanel(canvasGo.transform, "ViewpointBar", new Color(0.08f, 0.12f, 0.18f, 0.90f));
            bottomBar.anchorMin = new Vector2(0f, 0f);
            bottomBar.anchorMax = new Vector2(1f, 0f);
            bottomBar.pivot = new Vector2(0.5f, 0f);
            bottomBar.sizeDelta = new Vector2(0f, 54f);
            bottomBar.anchoredPosition = Vector2.zero;

            HorizontalLayoutGroup navLayout = bottomBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            navLayout.padding = new RectOffset(12, 12, 8, 8);
            navLayout.spacing = 8;
            navLayout.childControlWidth = true;
            navLayout.childForceExpandWidth = true;

            string[] viewIds = { "Desk", "Computer", "Cabinet", "Bookshelf", "Printer", "EvidenceBoard" };
            string[] viewLabels = { "1. Desk", "2. Computer", "3. Cabinet", "4. Bookshelf", "5. Printer", "6. Evidence Board" };
            for (int i = 0; i < viewIds.Length; i++)
            {
                string id = viewIds[i];
                CreateButton(bottomBar, $"View_{id}", viewLabels[i], new Color(0.16f, 0.34f, 0.52f, 1f), 38f, () =>
                {
                    CameraViewpointController.Instance?.MoveToViewpoint(id);
                });
            }

            // 4. Right-side Evidence Inventory Panel (WP3)
            RectTransform invPanel = CreatePanel(canvasGo.transform, "InventoryPanel", new Color(0.08f, 0.12f, 0.18f, 0.88f));
            invPanel.anchorMin = new Vector2(1f, 0f);
            invPanel.anchorMax = new Vector2(1f, 1f);
            invPanel.pivot = new Vector2(1f, 0.5f);
            invPanel.sizeDelta = new Vector2(285f, -158f);
            invPanel.anchoredPosition = new Vector2(-10f, -14f);

            VerticalLayoutGroup invLayout = invPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            invLayout.padding = new RectOffset(12, 12, 10, 10);
            invLayout.spacing = 6;
            invLayout.childControlHeight = false;
            invLayout.childForceExpandHeight = false;

            Text invTitle = CreateText(invPanel, "InvTitle", "EVIDENCE INVENTORY", 15, new Color(0.65f, 0.88f, 1f), TextAnchor.MiddleLeft);
            invTitle.fontStyle = FontStyle.Bold;
            invTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 24f);

            GameObject listObj = new GameObject("List");
            listObj.transform.SetParent(invPanel, false);
            RectTransform listRect = listObj.AddComponent<RectTransform>();
            listRect.sizeDelta = new Vector2(260f, 270f);
            VerticalLayoutGroup listLayout = listObj.AddComponent<VerticalLayoutGroup>();
            listLayout.spacing = 5;
            listLayout.childControlHeight = false;
            listLayout.childForceExpandHeight = false;
            inventoryListParent = listObj.transform;

            evidenceDetailText = CreateText(invPanel, "EvidenceDetail", "Click any collected card above to inspect its claim and source.", 12, new Color(0.88f, 0.92f, 0.96f), TextAnchor.UpperLeft);
            evidenceDetailText.GetComponent<RectTransform>().sizeDelta = new Vector2(260f, 160f);

            // 5. Phone & Objection Modal (WP3)
            BuildPhoneModal(canvasGo.transform);

            // 6. Computer Side-by-Side Comparison Modal (WP3 - Alex)
            BuildAlexComputerModal(canvasGo.transform);
        }

        private void BuildPhoneModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "PhoneModal", new Color(0.07f, 0.10f, 0.16f, 0.96f));
            modal.anchorMin = new Vector2(0.15f, 0.18f);
            modal.anchorMax = new Vector2(0.75f, 0.82f);
            modal.offsetMin = Vector2.zero;
            modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 18, 18);
            layout.spacing = 10;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;

            phoneSpeakerText = CreateText(modal, "Speaker", "TELEPHONE", 20, new Color(1f, 0.86f, 0.35f), TextAnchor.MiddleLeft);
            phoneSpeakerText.fontStyle = FontStyle.Bold;
            phoneSpeakerText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 32f);

            phoneBodyText = CreateText(modal, "Body", "", 16, Color.white, TextAnchor.UpperLeft);
            phoneBodyText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 110f);

            phoneFeedbackText = CreateText(modal, "Feedback", "", 14, new Color(0.45f, 1f, 0.55f), TextAnchor.UpperLeft);
            phoneFeedbackText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 75f);

            GameObject btnBox = new GameObject("ObjectionButtons");
            btnBox.transform.SetParent(modal, false);
            RectTransform btnRect = btnBox.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(0f, 140f);
            VerticalLayoutGroup btnLayout = btnBox.AddComponent<VerticalLayoutGroup>();
            btnLayout.spacing = 6;
            btnLayout.childControlHeight = false;
            btnLayout.childForceExpandHeight = false;
            objectionButtonContainer = btnBox.transform;

            CreateButton(modal, "ClosePhoneBtn", "Hang Up / Close", new Color(0.28f, 0.34f, 0.42f, 1f), 38f, () =>
            {
                phonePanel.SetActive(false);
            });

            phonePanel = modal.gameObject;
            phonePanel.SetActive(false);
        }

        private void BuildAlexComputerModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "AlexComputerModal", new Color(0.06f, 0.09f, 0.14f, 0.97f));
            modal.anchorMin = new Vector2(0.06f, 0.12f);
            modal.anchorMax = new Vector2(0.76f, 0.88f);
            modal.offsetMin = Vector2.zero;
            modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup rootLayout = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(18, 18, 14, 14);
            rootLayout.spacing = 10;
            rootLayout.childControlHeight = false;
            rootLayout.childForceExpandHeight = false;

            Text header = CreateText(modal, "Header", "OFFICE COMPUTER — SIDE-BY-SIDE DOCUMENT INSPECTION (CASE 01: ALEX)", 17, new Color(0.55f, 0.88f, 1f), TextAnchor.MiddleLeft);
            header.fontStyle = FontStyle.Bold;
            header.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 28f);

            // Two columns: Left = Blog, Right = Privacy Policy
            GameObject cols = new GameObject("Columns");
            cols.transform.SetParent(modal, false);
            RectTransform colsRect = cols.AddComponent<RectTransform>();
            colsRect.sizeDelta = new Vector2(0f, 215f);
            HorizontalLayoutGroup colsLayout = cols.AddComponent<HorizontalLayoutGroup>();
            colsLayout.spacing = 12;
            colsLayout.childControlWidth = true;
            colsLayout.childForceExpandWidth = true;

            RectTransform leftCol = CreatePanel(cols.transform, "BlogCol", new Color(0.18f, 0.11f, 0.12f, 0.95f));
            VerticalLayoutGroup lLay = leftCol.gameObject.AddComponent<VerticalLayoutGroup>();
            lLay.padding = new RectOffset(12, 12, 10, 10);
            CreateText(leftCol, "BlogText",
                "<b>VIRAL BLOG POST</b>\nAuthor: [Anonymous / Not Listed]\nSources: None cited\n\n" +
                "\"EXPOSED! VitaPulse is secretly selling EVERYONE'S private medical records right now! " +
                "Don't trust big institutions—this proves the whole app is a massive data-selling conspiracy!\"",
                14, Color.white, TextAnchor.UpperLeft);

            RectTransform rightCol = CreatePanel(cols.transform, "PolicyCol", new Color(0.10f, 0.16f, 0.22f, 0.95f));
            VerticalLayoutGroup rLay = rightCol.gameObject.AddComponent<VerticalLayoutGroup>();
            rLay.padding = new RectOffset(12, 12, 10, 10);
            CreateText(rightCol, "PolicyText",
                "<b>VITAPULSE PRIVACY POLICY — SECTION 4.2</b>\nSource: VitaPulse Official App Legal Notice\n\n" +
                "\"Some user information may be shared with third-party partners to operate and improve our services.\"\n\n" +
                "<i>Note: This raises a real privacy question about third-party sharing, but does NOT prove a secret sale of all medical records.</i>",
                14, new Color(0.88f, 0.96f, 1f), TextAnchor.UpperLeft);

            Text prompt = CreateText(modal, "FlagPrompt", "Click each unsupported weakness in the blog claim below:", 14, new Color(1f, 0.88f, 0.4f), TextAnchor.MiddleLeft);
            prompt.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 22f);

            for (int i = 0; i < blogFlagTexts.Length; i++)
            {
                int idx = i;
                Button flagBtn = CreateButton(modal, $"FlagBtn_{i}", $"[ ] {blogFlagTexts[i]}", new Color(0.16f, 0.22f, 0.30f, 1f), 34f, () => ToggleBlogFlag(idx));
                Text txt = flagBtn.GetComponentInChildren<Text>();
                txt.alignment = TextAnchor.MiddleLeft;
                blogFlagLabels[i] = txt;
            }

            completeInspectionButton = CreateButton(modal, "CompleteInspectBtn", "Mark all 3 unsupported blog claims (0/3)", new Color(0.15f, 0.55f, 0.32f, 1f), 40f, () =>
            {
                MissionManager.Instance?.CompleteAlexBlogInspection();
                UpdateBlogInspectionState();
            });

            CreateButton(modal, "ClosePCBtn", "Close Computer", new Color(0.28f, 0.34f, 0.42f, 1f), 34f, () =>
            {
                computerPanel.SetActive(false);
            });

            computerPanel = modal.gameObject;
            computerPanel.SetActive(false);
        }

        private RectTransform CreatePanel(Transform parent, string name, Color bgColor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image img = go.AddComponent<Image>();
            img.color = bgColor;
            return go.GetComponent<RectTransform>();
        }

        private Text CreateText(Transform parent, string name, string content, int fontSize, Color color, TextAnchor align)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text txt = go.AddComponent<Text>();
            txt.font = uiFont;
            txt.text = content;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = align;
            txt.supportRichText = true;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            return txt;
        }

        private Button CreateButton(Transform parent, string name, string label, Color bgColor, float height, UnityEngine.Events.UnityAction onClick)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, height);

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;

            Image img = go.AddComponent<Image>();
            img.color = bgColor;

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            GameObject txtGo = new GameObject("Label");
            txtGo.transform.SetParent(go.transform, false);
            RectTransform txtRect = txtGo.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = new Vector2(10f, 2f);
            txtRect.offsetMax = new Vector2(-10f, -2f);

            Text txt = txtGo.AddComponent<Text>();
            txt.font = uiFont;
            txt.text = label;
            txt.fontSize = 14;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;

            return btn;
        }
    }
}
