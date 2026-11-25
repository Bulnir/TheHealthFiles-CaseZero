using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HealthFiles
{
    /// <summary>
    /// WP3: Clean, Minimal Detective HUD for The Health Files: Case Zero.
    /// Keeps 90% of the screen clear for the 3D office:
    /// - Compact single-line mission pill (top-left)
    /// - Collapsible Evidence Inventory & DHLI Personalization drawers (top-right buttons)
    /// - Slim centered camera quick-jump bar (bottom)
    /// - Always-working Office Computer Desktop (Viral News tab + Case 01 Blog vs Policy Inspector)
    /// - Persona portrait + fast typewriter Phone / OBJECTION! modal
    /// All non-interactive HUD elements set raycastTarget = false so they NEVER block 3D clicks!
    /// </summary>
    public class CaseZeroHUD : MonoBehaviour
    {
        private Font uiFont;
        private readonly Dictionary<string, Texture2D> portraits = new Dictionary<string, Texture2D>();

        // Minimal HUD elements
        private Text objectiveText;
        private Text evidenceCountBtnText;
        private GameObject toastBanner;
        private Text toastText;
        private Coroutine toastRoutine;
        private GameObject hoverPill;
        private Text hoverLabelText;

        // Collapsible drawers (closed by default so 3D office is clean!)
        private GameObject inventoryDrawer;
        private Transform inventoryListParent;
        private Text evidenceDetailText;
        private GameObject dhliDrawer;
        private Text dhliDetailText;

        // Modals
        private GameObject welcomePanel;
        private GameObject phonePanel;
        private RawImage phonePortraitImage;
        private GameObject phonePortraitFrame;
        private Text phoneSpeakerText;
        private Text phoneBodyText;
        private Text phoneFeedbackText;
        private Transform objectionButtonContainer;
        private Coroutine typewriterRoutine;

        private GameObject docModalPanel;
        private Text docModalTitle;
        private Text docModalBody;

        // Office Computer Desktop Modal (Always works when clicking Monitor!)
        private GameObject computerPanel;
        private GameObject pcNewsTabContent;
        private GameObject pcInspectorTabContent;
        private GameObject pcInspectorLockedNotice;
        private GameObject pcInspectorGameArea;
        private Text computerFeedbackText;
        private Button completeInspectionButton;
        private readonly bool[] redFlagsFound = new bool[3];
        private readonly Text[] passageLabels = new Text[6];
        private readonly Image[] passageBackgrounds = new Image[6];

        // Animated OBJECTION! Splash
        private GameObject objectionSplashPanel;
        private RectTransform objectionSplashRect;
        private Text objectionSplashText;

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
                distractorHint = "That's actually good advice—find the 3 unsupported weaknesses instead!"
            }
        };

        private void Awake()
        {
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            foreach (string key in new[] { "alex", "sofia", "daniel" })
            {
                Texture2D tex = Resources.Load<Texture2D>("Portraits/" + key);
                if (tex != null) portraits[key] = tex;
            }
            BuildCanvasUI();
        }

        private void Start()
        {
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnMissionChanged += HandleMissionChanged;
                MissionManager.Instance.OnStatusMessage += ShowToast;
                MissionManager.Instance.OnPhoneCallRequested += HandlePhoneCallRequested;
                MissionManager.Instance.OnAlexComputerOpened += OpenComputerDesktop;
                MissionManager.Instance.OnMailLogOpened += OpenMailLogModal;
                MissionManager.Instance.OnManualOpened += OpenManualModal;
                MissionManager.Instance.OnAlexLetterOpened += OpenAlexLetterModal;
                MissionManager.Instance.OnObjectionSplash += TriggerObjectionSplash;

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
                PersonalizationManager.Instance.OnProfileUpdated += RefreshDhliDrawer;
                RefreshDhliDrawer();
            }

            if (OfficeRaycaster.Instance != null)
            {
                OfficeRaycaster.Instance.OnHoverLabelChanged += HandleHoverChanged;
            }

            welcomePanel.SetActive(true);
        }

        private void OnDestroy()
        {
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnMissionChanged -= HandleMissionChanged;
                MissionManager.Instance.OnStatusMessage -= ShowToast;
                MissionManager.Instance.OnPhoneCallRequested -= HandlePhoneCallRequested;
                MissionManager.Instance.OnAlexComputerOpened -= OpenComputerDesktop;
                MissionManager.Instance.OnMailLogOpened -= OpenMailLogModal;
                MissionManager.Instance.OnManualOpened -= OpenManualModal;
                MissionManager.Instance.OnAlexLetterOpened -= OpenAlexLetterModal;
                MissionManager.Instance.OnObjectionSplash -= TriggerObjectionSplash;
            }
            if (EvidenceManager.Instance != null) EvidenceManager.Instance.OnInventoryChanged -= RefreshInventoryUI;
            if (PersonalizationManager.Instance != null) PersonalizationManager.Instance.OnProfileUpdated -= RefreshDhliDrawer;
            if (OfficeRaycaster.Instance != null) OfficeRaycaster.Instance.OnHoverLabelChanged -= HandleHoverChanged;
        }

        private void CloseAllModals()
        {
            welcomePanel.SetActive(false);
            phonePanel.SetActive(false);
            computerPanel.SetActive(false);
            docModalPanel.SetActive(false);
            inventoryDrawer.SetActive(false);
            dhliDrawer.SetActive(false);
        }

        private void HandleMissionChanged(MissionState state, string _)
        {
            if (objectiveText == null) return;
            objectiveText.text = state switch
            {
                MissionState.Prologue_SupervisorCall => "Step 1: Answer the ringing Office Phone",
                MissionState.Prologue_FindManual => "Step 2: Pick up the Investigation Manual (Bookcase)",
                MissionState.Alex_AnswerFirstCall => "Case 01: Answer Alex Moretti's phone call",
                MissionState.Alex_FindHiddenLetter => "Case 01: Check Mail Log on desk -> Open Cabinet Drawer B-02",
                MissionState.Alex_InspectBlogAndPolicy => "Case 01: Click the Computer Monitor to inspect the Blog",
                MissionState.Alex_ObjectionCall => "Case 01: Call Alex back on Phone & trigger OBJECTION!",
                MissionState.Alex_PinPolicyClause => "Case 01: Click the Cork Evidence Board (left wall) to pin Section 4.2",
                MissionState.Sofia_Ready => "Case 01 Complete! Answer Sofia (WP4) or check DHLI Profile",
                _ => MissionManager.GetObjectiveText(state)
            };
        }

        private void ShowToast(string msg)
        {
            if (string.IsNullOrEmpty(msg) || toastBanner == null) return;
            toastText.text = msg;
            toastBanner.SetActive(true);
            if (toastRoutine != null) StopCoroutine(toastRoutine);
            toastRoutine = StartCoroutine(HideToastAfter(4.5f));
        }

        private IEnumerator HideToastAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            toastBanner.SetActive(false);
        }

        private void HandleHoverChanged(string label)
        {
            if (hoverPill == null) return;
            if (string.IsNullOrEmpty(label))
            {
                hoverPill.SetActive(false);
            }
            else
            {
                hoverLabelText.text = label;
                hoverPill.SetActive(true);
            }
        }

        private void RefreshDhliDrawer()
        {
            if (dhliDetailText == null || PersonalizationManager.Instance == null) return;
            var pm = PersonalizationManager.Instance;
            dhliDetailText.text =
                $"<b>ADAPTIVE DHLI PROFILE</b>\n" +
                $"<i>Personalizes hints & final report feedback</i>\n\n" +
                $"• Evaluating Reliability:  <b>{pm.reliabilityScore} / 100</b>\n" +
                $"• Protecting Privacy:      <b>{pm.privacyScore} / 100</b>\n" +
                $"• Digital Navigation:      <b>{pm.navigationScore} / 100</b>\n\n" +
                $"Mistakes tracked: R:{pm.reliabilityMistakes} | P:{pm.privacyMistakes} | N:{pm.navigationMistakes}\n\n" +
                pm.GetPersonalizedReportSummary();
        }

        private void RefreshInventoryUI()
        {
            if (inventoryListParent == null || EvidenceManager.Instance == null) return;
            IReadOnlyList<EvidenceItem> items = EvidenceManager.Instance.CollectedEvidence;
            if (evidenceCountBtnText != null) evidenceCountBtnText.text = $"Evidence ({items.Count})";

            foreach (Transform child in inventoryListParent) Destroy(child.gameObject);

            if (items.Count == 0)
            {
                CreateText(inventoryListParent, "Empty", "No evidence collected yet.", 13, new Color(0.75f, 0.8f, 0.85f), TextAnchor.UpperLeft, false);
                return;
            }

            foreach (EvidenceItem item in items)
            {
                EvidenceItem captured = item;
                Button btn = CreateButton(
                    inventoryListParent,
                    $"Inv_{item.id}",
                    $"• {item.title}",
                    new Color(0.18f, 0.28f, 0.42f, 0.96f),
                    34f,
                    () =>
                    {
                        SoundManager.Instance?.Play("ui_click", 0.5f);
                        evidenceDetailText.text = $"<b>{captured.title}</b> ({captured.id})\nDomain: {captured.dhliDomain}\nSource: {captured.source}\n\n{captured.summary}";
                    }
                );
                btn.GetComponentInChildren<Text>().alignment = TextAnchor.MiddleLeft;
            }
        }

        private void HandlePhoneCallRequested(string speaker, string portraitKey, string body, bool allowObjection)
        {
            CloseAllModals();
            phonePanel.SetActive(true);
            phoneSpeakerText.text = speaker;
            phoneFeedbackText.text = "";

            if (!string.IsNullOrEmpty(portraitKey) && portraits.TryGetValue(portraitKey, out Texture2D tex))
            {
                phonePortraitFrame.SetActive(true);
                phonePortraitImage.texture = tex;
            }
            else
            {
                phonePortraitFrame.SetActive(false);
            }

            if (typewriterRoutine != null) StopCoroutine(typewriterRoutine);
            typewriterRoutine = StartCoroutine(TypeTextRoutine(phoneBodyText, body));

            foreach (Transform child in objectionButtonContainer) Destroy(child.gameObject);

            if (allowObjection && EvidenceManager.Instance != null)
            {
                CreateText(objectionButtonContainer, "ObjHdr", "Choose Evidence Card to Present:", 13, new Color(1f, 0.88f, 0.4f), TextAnchor.MiddleLeft, false);
                foreach (EvidenceItem item in EvidenceManager.Instance.CollectedEvidence)
                {
                    EvidenceItem captured = item;
                    CreateButton(
                        objectionButtonContainer,
                        $"Present_{item.id}",
                        $"OBJECTION! Present: {item.title}",
                        new Color(0.76f, 0.20f, 0.16f, 1f),
                        36f,
                        () => OnPresentEvidenceClicked(captured.id)
                    );
                }
            }
        }

        private IEnumerator TypeTextRoutine(Text target, string fullText)
        {
            target.text = "";
            for (int i = 0; i < fullText.Length; i++)
            {
                target.text += fullText[i];
                if (i % 2 == 0) yield return new WaitForSeconds(0.008f);
            }
        }

        private void OnPresentEvidenceClicked(string evidenceId)
        {
            if (MissionManager.Instance == null) return;
            bool success = MissionManager.Instance.TrySubmitObjection(evidenceId, out string response, out string hint);
            if (success)
            {
                if (typewriterRoutine != null) StopCoroutine(typewriterRoutine);
                typewriterRoutine = StartCoroutine(TypeTextRoutine(phoneBodyText, response));
                phoneFeedbackText.color = new Color(0.45f, 1f, 0.55f);
                phoneFeedbackText.text = "OBJECTION ACCEPTED! Claim challenged with verifiable evidence.";
                foreach (Transform child in objectionButtonContainer) Destroy(child.gameObject);
            }
            else
            {
                phoneFeedbackText.color = new Color(1f, 0.72f, 0.35f);
                phoneFeedbackText.text = $"{response}\n\n{hint}";
            }
        }

        private void TriggerObjectionSplash(string bannerText) => StartCoroutine(ObjectionSplashRoutine(bannerText));

        private IEnumerator ObjectionSplashRoutine(string bannerText)
        {
            objectionSplashText.text = bannerText;
            objectionSplashPanel.SetActive(true);
            float elapsed = 0f;
            while (elapsed < 1.05f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / 0.16f);
                float scale = Mathf.Lerp(2.1f, 1.0f, t);
                objectionSplashRect.localScale = new Vector3(scale, scale, 1f);
                objectionSplashRect.anchoredPosition = elapsed < 0.3f
                    ? new Vector2(Random.Range(-8f, 8f), Random.Range(-5f, 5f))
                    : Vector2.zero;
                yield return null;
            }
            objectionSplashPanel.SetActive(false);
        }

        private void OpenComputerDesktop()
        {
            CloseAllModals();
            computerPanel.SetActive(true);

            bool hasLetter = EvidenceManager.Instance != null && EvidenceManager.Instance.HasEvidence("letter_alex");
            if (hasLetter)
            {
                ShowComputerTab(inspectorTab: true);
            }
            else
            {
                ShowComputerTab(inspectorTab: false);
            }
        }

        private void ShowComputerTab(bool inspectorTab)
        {
            pcNewsTabContent.SetActive(!inspectorTab);
            pcInspectorTabContent.SetActive(inspectorTab);

            if (inspectorTab)
            {
                bool hasLetter = EvidenceManager.Instance != null && EvidenceManager.Instance.HasEvidence("letter_alex");
                pcInspectorLockedNotice.SetActive(!hasLetter);
                pcInspectorGameArea.SetActive(hasLetter);
                if (hasLetter) UpdateBlogInspectionState();
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
                ? "Collected: Unverified Blog Claim (Close PC & Call Alex!)"
                : (count == 3 ? "CREATE EVIDENCE CARD: Unverified Blog Claim" : $"Click the 3 unsupported weaknesses ({count}/3)");
        }

        private void OpenMailLogModal()
        {
            CloseAllModals();
            docModalTitle.text = "MAIL DELIVERY LOG";
            docModalBody.text =
                "Archive Filing Index:\n\n" +
                "• Drawer B-01 (Top)     — Old 2023 Receipts\n" +
                "• <b>Drawer B-02 (Middle) — A. MORETTI ENVELOPE (VitaPulse)</b>\n" +
                "• Drawer B-03 (Bottom)  — Blank Forms\n\n" +
                "<i>Look left to the Archive Cabinet, slide open Drawer B-02, and click Alex's yellow envelope!</i>";
            docModalPanel.SetActive(true);
        }

        private void OpenManualModal()
        {
            CloseAllModals();
            docModalTitle.text = "INVESTIGATION MANUAL";
            docModalBody.text =
                "Three Golden Health Literacy Checks:\n\n" +
                "1. <b>SOURCE:</b> Who wrote or funded the claim?\n" +
                "2. <b>EVIDENCE:</b> Is there real clinical or official proof?\n" +
                "3. <b>CONTEXT:</b> Were dates or policy clauses twisted?\n\n" +
                "<i>Saved to your Evidence drawer!</i>";
            docModalPanel.SetActive(true);
        }

        private void OpenAlexLetterModal()
        {
            CloseAllModals();
            docModalTitle.text = "ALEX'S LETTER (letter_alex)";
            docModalBody.text =
                "From: Alex Moretti\n\n" +
                "<b>\"VitaPulse is selling everyone's private medical information!\"</b>\n\n" +
                "Blog URL unlocked on your Office Computer:\n<i>http://truth-health-watch.blog/vitapulse-exposed</i>\n\n" +
                "<i>Click your Computer Monitor to compare the blog with VitaPulse's Privacy Policy!</i>";
            docModalPanel.SetActive(true);
        }

        private void BuildCanvasUI()
        {
            GameObject canvasGo = new GameObject("CaseZero_HUD_Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            canvasGo.AddComponent<GraphicRaycaster>();

            // 1. Compact Top-Left Mission Pill (NON-BLOCKING raycast!)
            RectTransform objPill = CreatePanel(canvasGo.transform, "ObjectivePill", new Color(0.07f, 0.10f, 0.16f, 0.84f), raycast: false);
            objPill.anchorMin = new Vector2(0f, 1f);
            objPill.anchorMax = new Vector2(0f, 1f);
            objPill.pivot = new Vector2(0f, 1f);
            objPill.sizeDelta = new Vector2(540f, 38f);
            objPill.anchoredPosition = new Vector2(14f, -12f);
            objectiveText = CreateText(objPill, "ObjText", "Loading...", 15, new Color(1f, 0.92f, 0.45f), TextAnchor.MiddleLeft, false);
            objectiveText.fontStyle = FontStyle.Bold;
            SetFill(objectiveText.GetComponent<RectTransform>(), 14, 4);

            // 2. Top-Right 2 Compact Drawer Buttons (Evidence + DHLI Profile)
            RectTransform topRight = CreatePanel(canvasGo.transform, "TopRightBtns", new Color(0, 0, 0, 0), raycast: false);
            topRight.anchorMin = new Vector2(1f, 1f);
            topRight.anchorMax = new Vector2(1f, 1f);
            topRight.pivot = new Vector2(1f, 1f);
            topRight.sizeDelta = new Vector2(276f, 38f);
            topRight.anchoredPosition = new Vector2(-14f, -12f);
            HorizontalLayoutGroup trLay = topRight.gameObject.AddComponent<HorizontalLayoutGroup>();
            trLay.spacing = 8;
            trLay.childControlWidth = true;
            trLay.childForceExpandWidth = true;

            Button invBtn = CreateButton(topRight, "ToggleInvBtn", "Evidence (0)", new Color(0.14f, 0.30f, 0.48f, 0.92f), 36f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                bool next = !inventoryDrawer.activeSelf;
                dhliDrawer.SetActive(false);
                inventoryDrawer.SetActive(next);
            });
            evidenceCountBtnText = invBtn.GetComponentInChildren<Text>();

            CreateButton(topRight, "ToggleDhliBtn", "DHLI Profile", new Color(0.16f, 0.48f, 0.32f, 0.92f), 36f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                bool next = !dhliDrawer.activeSelf;
                inventoryDrawer.SetActive(false);
                dhliDrawer.SetActive(next);
                RefreshDhliDrawer();
            });

            // 3. Auto-hiding Toast Banner (non-blocking!)
            RectTransform toast = CreatePanel(canvasGo.transform, "ToastBanner", new Color(0.08f, 0.14f, 0.22f, 0.88f), raycast: false);
            toast.anchorMin = new Vector2(0.22f, 1f);
            toast.anchorMax = new Vector2(0.78f, 1f);
            toast.pivot = new Vector2(0.5f, 1f);
            toast.sizeDelta = new Vector2(0f, 36f);
            toast.anchoredPosition = new Vector2(0f, -58f);
            toastText = CreateText(toast, "ToastText", "", 13, Color.white, TextAnchor.MiddleCenter, false);
            SetFill(toastText.GetComponent<RectTransform>(), 12, 4);
            toastBanner = toast.gameObject;
            toastBanner.SetActive(false);

            // 4. Clean Hover Pill (ONLY appears when aiming at a clickable 3D object, non-blocking!)
            RectTransform hover = CreatePanel(canvasGo.transform, "HoverPill", new Color(0.06f, 0.09f, 0.14f, 0.84f), raycast: false);
            hover.anchorMin = new Vector2(0.32f, 0f);
            hover.anchorMax = new Vector2(0.68f, 0f);
            hover.pivot = new Vector2(0.5f, 0f);
            hover.sizeDelta = new Vector2(0f, 32f);
            hover.anchoredPosition = new Vector2(0f, 54f);
            hoverLabelText = CreateText(hover, "HoverText", "", 14, new Color(1f, 0.94f, 0.52f), TextAnchor.MiddleCenter, false);
            hoverLabelText.fontStyle = FontStyle.Bold;
            SetFill(hoverLabelText.GetComponent<RectTransform>(), 10, 2);
            hoverPill = hover.gameObject;
            hoverPill.SetActive(false);

            // 5. Slim Centered Camera Quick-Jump Pill (bottom center, leaves corners completely clear!)
            RectTransform navBar = CreatePanel(canvasGo.transform, "CameraNavPill", new Color(0.07f, 0.10f, 0.16f, 0.78f), raycast: true);
            navBar.anchorMin = new Vector2(0.5f, 0f);
            navBar.anchorMax = new Vector2(0.5f, 0f);
            navBar.pivot = new Vector2(0.5f, 0f);
            navBar.sizeDelta = new Vector2(610f, 38f);
            navBar.anchoredPosition = new Vector2(0f, 10f);
            HorizontalLayoutGroup navLay = navBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            navLay.padding = new RectOffset(6, 6, 4, 4);
            navLay.spacing = 6;
            navLay.childControlWidth = true;
            navLay.childForceExpandWidth = true;

            string[] ids = { "Desk", "Computer", "Cabinet", "Bookshelf", "Printer", "EvidenceBoard" };
            string[] labels = { "Desk", "PC", "Cabinet", "Bookshelf", "Printer", "Board" };
            for (int i = 0; i < ids.Length; i++)
            {
                string id = ids[i];
                CreateButton(navBar, $"Nav_{id}", labels[i], new Color(0.16f, 0.28f, 0.42f, 0.92f), 30f, () =>
                {
                    SoundManager.Instance?.Play("ui_click", 0.45f);
                    CameraViewpointController.Instance?.MoveToViewpoint(id);
                });
            }

            // Tiny non-blocking control hint in bottom-left corner
            RectTransform ctrlHint = CreatePanel(canvasGo.transform, "CtrlHint", new Color(0, 0, 0, 0), raycast: false);
            ctrlHint.anchorMin = new Vector2(0f, 0f);
            ctrlHint.anchorMax = new Vector2(0f, 0f);
            ctrlHint.pivot = new Vector2(0f, 0f);
            ctrlHint.sizeDelta = new Vector2(310f, 24f);
            ctrlHint.anchoredPosition = new Vector2(14f, 14f);
            Text hintTxt = CreateText(ctrlHint, "HintTxt", "Right-Click Drag / WASD: Look  |  Esc: Desk", 12, new Color(0.92f, 0.95f, 1f, 0.85f), TextAnchor.MiddleLeft, false);
            SetFill(hintTxt.GetComponent<RectTransform>(), 0, 0);

            // 6. Collapsible Evidence Drawer (hidden by default!)
            BuildEvidenceDrawer(canvasGo.transform);
            BuildDhliDrawer(canvasGo.transform);

            // 7. Modals
            BuildWelcomeModal(canvasGo.transform);
            BuildPhoneModal(canvasGo.transform);
            BuildComputerDesktopModal(canvasGo.transform);
            BuildDocModal(canvasGo.transform);
            BuildObjectionSplash(canvasGo.transform);
        }

        private void BuildEvidenceDrawer(Transform parent)
        {
            RectTransform panel = CreatePanel(parent, "EvidenceDrawer", new Color(0.07f, 0.10f, 0.16f, 0.95f), raycast: true);
            panel.anchorMin = new Vector2(1f, 1f);
            panel.anchorMax = new Vector2(1f, 1f);
            panel.pivot = new Vector2(1f, 1f);
            panel.sizeDelta = new Vector2(310f, 440f);
            panel.anchoredPosition = new Vector2(-14f, -58f);

            VerticalLayoutGroup lay = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(12, 12, 10, 10);
            lay.spacing = 6;
            lay.childControlHeight = false;
            lay.childForceExpandHeight = false;

            Text hdr = CreateText(panel, "Hdr", "COLLECTED EVIDENCE", 14, new Color(0.65f, 0.88f, 1f), TextAnchor.MiddleLeft, false);
            hdr.fontStyle = FontStyle.Bold;
            hdr.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 22f);

            GameObject listObj = new GameObject("List");
            listObj.transform.SetParent(panel, false);
            listObj.AddComponent<RectTransform>().sizeDelta = new Vector2(286f, 220f);
            VerticalLayoutGroup lLay = listObj.AddComponent<VerticalLayoutGroup>();
            lLay.spacing = 4;
            lLay.childControlHeight = false;
            lLay.childForceExpandHeight = false;
            inventoryListParent = listObj.transform;

            evidenceDetailText = CreateText(panel, "Detail", "Click any card above to inspect.", 12, new Color(0.9f, 0.94f, 0.98f), TextAnchor.UpperLeft, false);
            evidenceDetailText.GetComponent<RectTransform>().sizeDelta = new Vector2(286f, 130f);

            CreateButton(panel, "CloseInv", "Close", new Color(0.26f, 0.32f, 0.42f, 1f), 30f, () => inventoryDrawer.SetActive(false));

            inventoryDrawer = panel.gameObject;
            inventoryDrawer.SetActive(false);
        }

        private void BuildDhliDrawer(Transform parent)
        {
            RectTransform panel = CreatePanel(parent, "DhliDrawer", new Color(0.07f, 0.10f, 0.16f, 0.95f), raycast: true);
            panel.anchorMin = new Vector2(1f, 1f);
            panel.anchorMax = new Vector2(1f, 1f);
            panel.pivot = new Vector2(1f, 1f);
            panel.sizeDelta = new Vector2(330f, 290f);
            panel.anchoredPosition = new Vector2(-14f, -58f);

            VerticalLayoutGroup lay = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(14, 14, 12, 12);
            lay.spacing = 8;
            lay.childControlHeight = false;
            lay.childForceExpandHeight = false;

            dhliDetailText = CreateText(panel, "DhliText", "", 13, Color.white, TextAnchor.UpperLeft, false);
            dhliDetailText.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 215f);

            CreateButton(panel, "CloseDhli", "Close", new Color(0.26f, 0.32f, 0.42f, 1f), 30f, () => dhliDrawer.SetActive(false));

            dhliDrawer = panel.gameObject;
            dhliDrawer.SetActive(false);
        }

        private void BuildWelcomeModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "WelcomeModal", new Color(0.06f, 0.09f, 0.15f, 0.96f), raycast: true);
            modal.anchorMin = new Vector2(0.24f, 0.28f);
            modal.anchorMax = new Vector2(0.76f, 0.72f);
            modal.offsetMin = Vector2.zero;
            modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup lay = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(28, 28, 22, 22);
            lay.spacing = 12;
            lay.childControlHeight = false;
            lay.childForceExpandHeight = false;

            Text title = CreateText(modal, "T", "THE HEALTH FILES: CASE ZERO", 22, new Color(1f, 0.88f, 0.38f), TextAnchor.MiddleCenter, false);
            title.fontStyle = FontStyle.Bold;
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 34f);

            Text body = CreateText(modal, "B",
                "A viral health app called <b>VitaPulse</b> is making headlines:\n" +
                "some say it saves lives, others claim it secretly sells medical records.\n\n" +
                "• <b>Right-Click + Drag (or WASD):</b> Look around your 3D office\n" +
                "• <b>Left-Click:</b> Interact with phone, computer, drawers, books & board",
                14, Color.white, TextAnchor.UpperLeft, false);
            body.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 155f);

            CreateButton(modal, "StartBtn", "Start Investigation (Day 1)", new Color(0.16f, 0.56f, 0.34f, 1f), 42f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.6f);
                welcomePanel.SetActive(false);
            });

            welcomePanel = modal.gameObject;
            welcomePanel.SetActive(false);
        }

        private void BuildPhoneModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "PhoneModal", new Color(0.06f, 0.09f, 0.15f, 0.96f), raycast: true);
            modal.anchorMin = new Vector2(0.16f, 0.16f);
            modal.anchorMax = new Vector2(0.84f, 0.84f);
            modal.offsetMin = Vector2.zero;
            modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup lay = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(22, 22, 18, 18);
            lay.spacing = 10;
            lay.childControlHeight = false;
            lay.childForceExpandHeight = false;

            phoneSpeakerText = CreateText(modal, "Speaker", "TELEPHONE", 19, new Color(1f, 0.88f, 0.38f), TextAnchor.MiddleLeft, false);
            phoneSpeakerText.fontStyle = FontStyle.Bold;
            phoneSpeakerText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 28f);

            GameObject row = new GameObject("Row");
            row.transform.SetParent(modal, false);
            row.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 138f);
            HorizontalLayoutGroup rLay = row.AddComponent<HorizontalLayoutGroup>();
            rLay.spacing = 16;
            rLay.childControlWidth = false;
            rLay.childForceExpandWidth = false;

            RectTransform frame = CreatePanel(row.transform, "PortraitFrame", new Color(0.22f, 0.32f, 0.46f, 1f), raycast: false);
            frame.sizeDelta = new Vector2(134f, 134f);
            GameObject raw = new GameObject("Raw");
            raw.transform.SetParent(frame, false);
            SetFill(raw.AddComponent<RectTransform>(), 4, 4);
            phonePortraitImage = raw.AddComponent<RawImage>();
            phonePortraitFrame = frame.gameObject;

            phoneBodyText = CreateText(row.transform, "Body", "", 15, Color.white, TextAnchor.UpperLeft, false);
            phoneBodyText.GetComponent<RectTransform>().sizeDelta = new Vector2(640f, 134f);

            phoneFeedbackText = CreateText(modal, "Feedback", "", 13, new Color(0.45f, 1f, 0.55f), TextAnchor.UpperLeft, false);
            phoneFeedbackText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 60f);

            GameObject box = new GameObject("ObjBtns");
            box.transform.SetParent(modal, false);
            box.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 145f);
            VerticalLayoutGroup bLay = box.AddComponent<VerticalLayoutGroup>();
            bLay.spacing = 5;
            bLay.childControlHeight = false;
            bLay.childForceExpandHeight = false;
            objectionButtonContainer = box.transform;

            CreateButton(modal, "HangUpBtn", "Hang Up / Close", new Color(0.26f, 0.32f, 0.42f, 1f), 36f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                phonePanel.SetActive(false);
            });

            phonePanel = modal.gameObject;
            phonePanel.SetActive(false);
        }

        private void BuildComputerDesktopModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "ComputerDesktopModal", new Color(0.05f, 0.08f, 0.13f, 0.98f), raycast: true);
            modal.anchorMin = new Vector2(0.08f, 0.10f);
            modal.anchorMax = new Vector2(0.92f, 0.90f);
            modal.offsetMin = Vector2.zero;
            modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup rootLay = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            rootLay.padding = new RectOffset(18, 18, 14, 14);
            rootLay.spacing = 8;
            rootLay.childControlHeight = false;
            rootLay.childForceExpandHeight = false;

            // Top OS Window Bar with Tab Switchers + Close
            GameObject topRow = new GameObject("OSTabs");
            topRow.transform.SetParent(modal, false);
            topRow.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 36f);
            HorizontalLayoutGroup tLay = topRow.AddComponent<HorizontalLayoutGroup>();
            tLay.spacing = 8;
            tLay.childControlWidth = true;
            tLay.childForceExpandWidth = true;

            CreateButton(topRow.transform, "TabNews", "1. Viral News Feed (VitaPulse)", new Color(0.18f, 0.34f, 0.52f, 1f), 34f, () => ShowComputerTab(false));
            CreateButton(topRow.transform, "TabInspect", "2. Case 01: Blog vs Privacy Policy", new Color(0.22f, 0.44f, 0.34f, 1f), 34f, () => ShowComputerTab(true));
            CreateButton(topRow.transform, "CloseOS", "Close Computer [X]", new Color(0.55f, 0.20f, 0.20f, 1f), 34f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                computerPanel.SetActive(false);
            });

            // TAB 1: Viral News Feed (always accessible!)
            RectTransform newsTab = CreatePanel(modal, "NewsTab", new Color(0.09f, 0.13f, 0.20f, 0.96f), raycast: false);
            newsTab.sizeDelta = new Vector2(0f, 470f);
            VerticalLayoutGroup nLay = newsTab.gameObject.AddComponent<VerticalLayoutGroup>();
            nLay.padding = new RectOffset(22, 22, 18, 18);
            nLay.spacing = 12;
            CreateText(newsTab, "NewsBody",
                "<b>VITAPULSE — TRENDING HEALTH NOTIFICATIONS</b>\n\n" +
                "•  <b>[BREAKING]</b> \"VitaPulse Can Detect Serious Diseases Early!\"\n" +
                "•  <b>[SOCIAL]</b> \"Millions Download the New Viral Health App!\"\n" +
                "•  <b>[BLOG]</b> \"Is VitaPulse Secretly Selling Everyone's Health Data?\"\n\n" +
                "<b>Investigator Workstation Status:</b>\n" +
                "• Case 01 (Alex Moretti): Open Tab 2 once you collect Alex's Letter from Filing Cabinet Drawer B-02.\n" +
                "• Case 02 (Sofia Romano — WP4): Social Media & Permission Panel plug-in.\n" +
                "• Case 03 (Daniel Conti — WP5): Verified Official Health Portal plug-in.",
                15, Color.white, TextAnchor.UpperLeft, false);
            pcNewsTabContent = newsTab.gameObject;

            // TAB 2: Case 01 Blog vs Privacy Policy Inspector
            GameObject inspTab = new GameObject("InspectorTab");
            inspTab.transform.SetParent(modal, false);
            inspTab.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 480f);
            VerticalLayoutGroup iRootLay = inspTab.AddComponent<VerticalLayoutGroup>();
            iRootLay.spacing = 8;
            iRootLay.childControlHeight = false;
            iRootLay.childForceExpandHeight = false;

            RectTransform lockedBox = CreatePanel(inspTab.transform, "LockedNotice", new Color(0.14f, 0.12f, 0.10f, 0.96f), raycast: false);
            lockedBox.sizeDelta = new Vector2(0f, 180f);
            Text lockTxt = CreateText(lockedBox, "LockTxt",
                "<b>BLOG URL NOT UNLOCKED YET</b>\n\n" +
                "1. Answer the Office Phone (Supervisor -> Investigation Manual -> Alex Moretti).\n" +
                "2. Check the Mail Delivery Log on your desk.\n" +
                "3. Slide open Archive Cabinet Drawer B-02 and collect Alex's Yellow Envelope!",
                15, new Color(1f, 0.88f, 0.48f), TextAnchor.MiddleCenter, false);
            SetFill(lockTxt.GetComponent<RectTransform>(), 20, 14);
            pcInspectorLockedNotice = lockedBox.gameObject;

            GameObject gameArea = new GameObject("GameArea");
            gameArea.transform.SetParent(inspTab.transform, false);
            gameArea.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 470f);
            VerticalLayoutGroup gLay = gameArea.AddComponent<VerticalLayoutGroup>();
            gLay.spacing = 8;
            gLay.childControlHeight = false;
            gLay.childForceExpandHeight = false;

            GameObject cols = new GameObject("Columns");
            cols.transform.SetParent(gameArea.transform, false);
            cols.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 365f);
            HorizontalLayoutGroup cLay = cols.AddComponent<HorizontalLayoutGroup>();
            cLay.spacing = 12;
            cLay.childControlWidth = true;
            cLay.childForceExpandWidth = true;

            RectTransform leftCol = CreatePanel(cols.transform, "BlogCol", new Color(0.14f, 0.09f, 0.11f, 0.96f), raycast: true);
            VerticalLayoutGroup lLay = leftCol.gameObject.AddComponent<VerticalLayoutGroup>();
            lLay.padding = new RectOffset(10, 10, 8, 8);
            lLay.spacing = 6;
            lLay.childControlHeight = false;
            lLay.childForceExpandHeight = false;

            Text bHdr = CreateText(leftCol, "BHdr", "<b>VIRAL BLOG POST — Click the 3 Unsupported Claims!</b>", 13, new Color(1f, 0.72f, 0.72f), TextAnchor.MiddleLeft, false);
            bHdr.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 22f);

            for (int i = 0; i < blogPassages.Length; i++)
            {
                int idx = i;
                Button pBtn = CreateButton(leftCol, $"Passage_{i}", blogPassages[i].text, new Color(0.16f, 0.21f, 0.28f, 0.96f), 48f, () => OnBlogPassageClicked(idx));
                Text t = pBtn.GetComponentInChildren<Text>();
                t.alignment = TextAnchor.MiddleLeft;
                t.fontSize = 12;
                passageLabels[i] = t;
                passageBackgrounds[i] = pBtn.GetComponent<Image>();
            }

            RectTransform rightCol = CreatePanel(cols.transform, "PolicyCol", new Color(0.09f, 0.15f, 0.22f, 0.96f), raycast: false);
            VerticalLayoutGroup rLay = rightCol.gameObject.AddComponent<VerticalLayoutGroup>();
            rLay.padding = new RectOffset(14, 14, 12, 12);
            CreateText(rightCol, "PolTxt",
                "<b>VITAPULSE PRIVACY POLICY — SECTION 4.2</b>\n\n" +
                "\"Some user information may be shared with third-party partners to operate and improve services.\"\n\n" +
                "<b>Investigator Check:</b>\n" +
                "• This clause raises a real privacy question.\n" +
                "• However, it does NOT prove a secret sale of ALL medical records.\n\n" +
                "<i>Click ONLY the 3 unsupported weaknesses in the blog on the left!</i>",
                14, new Color(0.88f, 0.96f, 1f), TextAnchor.UpperLeft, false);

            computerFeedbackText = CreateText(gameArea.transform, "PCFb", "Compare both documents and click the 3 unsupported blog weaknesses.", 13, new Color(1f, 0.9f, 0.48f), TextAnchor.MiddleLeft, false);
            computerFeedbackText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 34f);

            completeInspectionButton = CreateButton(gameArea.transform, "StampBtn", "Click the 3 unsupported weaknesses (0/3)", new Color(0.15f, 0.56f, 0.34f, 1f), 40f, () =>
            {
                MissionManager.Instance?.CompleteAlexBlogInspection();
                UpdateBlogInspectionState();
            });

            pcInspectorGameArea = gameArea;
            pcInspectorTabContent = inspTab;

            computerPanel = modal.gameObject;
            computerPanel.SetActive(false);
        }

        private void BuildDocModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "DocModal", new Color(0.07f, 0.10f, 0.16f, 0.96f), raycast: true);
            modal.anchorMin = new Vector2(0.22f, 0.24f);
            modal.anchorMax = new Vector2(0.78f, 0.76f);
            modal.offsetMin = Vector2.zero;
            modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup lay = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(24, 24, 20, 20);
            lay.spacing = 12;
            lay.childControlHeight = false;
            lay.childForceExpandHeight = false;

            docModalTitle = CreateText(modal, "T", "DOCUMENT", 19, new Color(1f, 0.88f, 0.40f), TextAnchor.MiddleLeft, false);
            docModalTitle.fontStyle = FontStyle.Bold;
            docModalTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 30f);

            docModalBody = CreateText(modal, "B", "", 15, Color.white, TextAnchor.UpperLeft, false);
            docModalBody.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 220f);

            CreateButton(modal, "CloseDoc", "Close", new Color(0.26f, 0.32f, 0.42f, 1f), 38f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                docModalPanel.SetActive(false);
            });

            docModalPanel = modal.gameObject;
            docModalPanel.SetActive(false);
        }

        private void BuildObjectionSplash(Transform parent)
        {
            RectTransform overlay = CreatePanel(parent, "ObjectionSplash", new Color(0.78f, 0.10f, 0.10f, 0.92f), raycast: false);
            overlay.anchorMin = new Vector2(0.18f, 0.36f);
            overlay.anchorMax = new Vector2(0.82f, 0.64f);
            overlay.offsetMin = Vector2.zero;
            overlay.offsetMax = Vector2.zero;

            objectionSplashText = CreateText(overlay, "Splash", "OBJECTION!", 64, new Color(1f, 0.92f, 0.25f), TextAnchor.MiddleCenter, false);
            objectionSplashText.fontStyle = FontStyle.Bold;
            SetFill(objectionSplashText.GetComponent<RectTransform>(), 0, 0);

            objectionSplashRect = overlay;
            objectionSplashPanel = overlay.gameObject;
            objectionSplashPanel.SetActive(false);
        }

        private static void SetFill(RectTransform rt, float padX, float padY)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padX, padY);
            rt.offsetMax = new Vector2(-padX, -padY);
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

        private Text CreateText(Transform parent, string name, string content, int fontSize, Color color, TextAnchor align, bool raycast)
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
            txt.raycastTarget = raycast;
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
            le.minHeight = height;
            le.preferredHeight = height;

            Image img = go.AddComponent<Image>();
            img.color = bgColor;

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            GameObject txtGo = new GameObject("Label");
            txtGo.transform.SetParent(go.transform, false);
            SetFill(txtGo.AddComponent<RectTransform>(), 8f, 2f);

            Text txt = txtGo.AddComponent<Text>();
            txt.font = uiFont;
            txt.text = label;
            txt.fontSize = 13;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;

            return btn;
        }
    }
}
