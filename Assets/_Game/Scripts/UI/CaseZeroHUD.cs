using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HealthFiles
{
    /// <summary>
    /// WP3 + WP2 UI: Clean Detective HUD featuring:
    /// - Effortless Left / Right Screen Turn Arrows (◄ / ►) + 'Back to Desk' pill
    /// - Live 3D WorldSpace Monitor OS (no fake 2D PC popup — zooms straight onto the 3D glass!)
    /// - Interactive Two-Page Open Book Reader with page flipping, real Health Literacy notes & funny yellow Sticky Notes!
    /// - Comfortable reading-speed phone typewriter dialogue + indie character voice mumble blips
    /// </summary>
    public class CaseZeroHUD : MonoBehaviour
    {
        private Font uiFont;
        private readonly Dictionary<string, Texture2D> portraits = new Dictionary<string, Texture2D>();

        // Minimal HUD
        private Text objectiveText;
        private Text evidenceCountBtnText;
        private GameObject toastBanner;
        private Text toastText;
        private Coroutine toastRoutine;
        private GameObject hoverPill;
        private Text hoverLabelText;

        // Effortless Chair-Turn Arrows + Back to Desk
        private GameObject leftTurnBtnObj;
        private GameObject rightTurnBtnObj;
        private GameObject backToDeskBtnObj;

        // Collapsible drawers
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

        // Interactive Two-Page Open Book Reader + Funny Sticky Notes!
        private GameObject bookModalPanel;
        private Image bookCoverBorder;
        private Text bookTitleBanner;
        private Text bookLeftPageText;
        private Text bookRightPageText;
        private Text bookPageCounterText;
        private Button bookPrevBtn;
        private Button bookNextBtn;
        private GameObject stickyNoteObj;
        private Text stickyNoteText;

        private BookSpread[] activeBookSpreads;
        private int activeSpreadIndex;

        // Printer Paper-Jam Mini-Game + Crumpled A4 Photocopied Photo in Hand!
        private GameObject printerJamModal;
        private GameObject printerJamPhaseObj;
        private GameObject printerPhotocopyPhaseObj;
        private RectTransform jammedPaperRect;
        private Text printerJamStatusText;
        private Text printerPullBtnText;
        private Text detectiveCommentText;
        private RawImage photocopyRawImage;
        private Texture2D photocopyTexture;
        private int printerPullCount;

        // Styled Physical Clipboard-in-Hand Modal (Mail Delivery Log)
        private GameObject clipboardModalPanel;

        // OBJECTION! Splash
        private GameObject objectionSplashPanel;
        private RectTransform objectionSplashRect;
        private Text objectionSplashText;

        private struct BookSpread
        {
            public string leftPage;
            public string rightPage;
            public string stickyNote; // empty if no sticky note on this spread
        }

        private void Awake()
        {
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            foreach (string key in new[] { "alex", "sofia", "daniel" })
            {
                Texture2D tex = Resources.Load<Texture2D>("Portraits/" + key);
                if (tex != null) portraits[key] = tex;
            }
            photocopyTexture = Resources.Load<Texture2D>("Textures/printer_photocopy_jam");
            BuildCanvasUI();
        }

        private void Start()
        {
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnMissionChanged += HandleMissionChanged;
                MissionManager.Instance.OnStatusMessage += ShowToast;
                MissionManager.Instance.OnPhoneCallRequested += HandlePhoneCallRequested;
                MissionManager.Instance.OnMailLogOpened += OpenMailLogModal;
                MissionManager.Instance.OnAlexLetterOpened += OpenAlexLetterModal;
                MissionManager.Instance.OnBookOpened += OpenBookReader;
                MissionManager.Instance.OnPrinterJamOpened += OpenPrinterJamModal;
                MissionManager.Instance.OnObjectionSplash += TriggerObjectionSplash;

                HandleMissionChanged(MissionManager.Instance.CurrentState, "");
            }

            if (CameraViewpointController.Instance != null)
            {
                CameraViewpointController.Instance.OnViewpointChanged += HandleViewpointChanged;
                RefreshTurnArrows();
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
            SyncModalLock();
        }

        private void OnDestroy()
        {
            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnMissionChanged -= HandleMissionChanged;
                MissionManager.Instance.OnStatusMessage -= ShowToast;
                MissionManager.Instance.OnPhoneCallRequested -= HandlePhoneCallRequested;
                MissionManager.Instance.OnMailLogOpened -= OpenMailLogModal;
                MissionManager.Instance.OnAlexLetterOpened -= OpenAlexLetterModal;
                MissionManager.Instance.OnBookOpened -= OpenBookReader;
                MissionManager.Instance.OnPrinterJamOpened -= OpenPrinterJamModal;
                MissionManager.Instance.OnObjectionSplash -= TriggerObjectionSplash;
            }
            if (CameraViewpointController.Instance != null) CameraViewpointController.Instance.OnViewpointChanged -= HandleViewpointChanged;
            if (EvidenceManager.Instance != null) EvidenceManager.Instance.OnInventoryChanged -= RefreshInventoryUI;
            if (PersonalizationManager.Instance != null) PersonalizationManager.Instance.OnProfileUpdated -= RefreshDhliDrawer;
            if (OfficeRaycaster.Instance != null) OfficeRaycaster.Instance.OnHoverLabelChanged -= HandleHoverChanged;
        }

        private void StopActiveSpeech()
        {
            if (typewriterRoutine != null)
            {
                StopCoroutine(typewriterRoutine);
                typewriterRoutine = null;
            }
            SoundManager.Instance?.StopVoice();
        }

        private void SyncModalLock()
        {
            bool anyOpen = welcomePanel.activeSelf || phonePanel.activeSelf || docModalPanel.activeSelf || bookModalPanel.activeSelf
                || (printerJamModal != null && printerJamModal.activeSelf)
                || (clipboardModalPanel != null && clipboardModalPanel.activeSelf);
            if (CameraViewpointController.Instance != null)
            {
                CameraViewpointController.Instance.IsCameraLockedByModal = anyOpen;
            }
            RefreshTurnArrows();
        }

        private void CloseAllModals()
        {
            StopActiveSpeech();
            welcomePanel.SetActive(false);
            phonePanel.SetActive(false);
            docModalPanel.SetActive(false);
            bookModalPanel.SetActive(false);
            if (printerJamModal != null) printerJamModal.SetActive(false);
            if (clipboardModalPanel != null) clipboardModalPanel.SetActive(false);
            ClipboardPickupAnimator.Instance?.SetHeld(false);
            inventoryDrawer.SetActive(false);
            dhliDrawer.SetActive(false);
            SyncModalLock();
        }

        private void HandleViewpointChanged(CameraViewpoint _) => RefreshTurnArrows();

        private void RefreshTurnArrows()
        {
            var ctrl = CameraViewpointController.Instance;
            if (ctrl == null) return;
            bool locked = ctrl.IsCameraLockedByModal;
            bool onPC = ctrl.CurrentViewpointId == "Computer";

            // Hide side turn arrows while zoomed directly into the 3D Monitor so you click the OS cleanly!
            if (leftTurnBtnObj != null) leftTurnBtnObj.SetActive(!locked && !onPC && ctrl.CanTurnLeft());
            if (rightTurnBtnObj != null) rightTurnBtnObj.SetActive(!locked && !onPC && ctrl.CanTurnRight());
            if (backToDeskBtnObj != null) backToDeskBtnObj.SetActive(!locked && ctrl.CurrentViewpointId != "Desk");
        }

        private void HandleMissionChanged(MissionState state, string _)
        {
            if (objectiveText == null) return;
            objectiveText.text = state switch
            {
                MissionState.Prologue_SupervisorCall => "Step 1: Answer the ringing Office Phone",
                MissionState.Prologue_FindManual => "Step 2: Turn Right (►) -> Open the Navy-Blue MANUAL on the Bookcase",
                MissionState.Alex_AnswerFirstCall => "Case 01: Answer Alex Moretti's phone call",
                MissionState.Alex_FindHiddenLetter => "Case 01: Check Mail Log on desk -> Turn Left (◄) to Cabinet Drawer B-02",
                MissionState.Alex_InspectBlogAndPolicy => "Case 01: Click the 3D Monitor -> Use HealthBrowser directly on screen",
                MissionState.Alex_ObjectionCall => "Case 01: Call Alex back on Phone & trigger OBJECTION!",
                MissionState.Alex_PinPolicyClause => "Case 01: Turn Left (◄) to the Cork Evidence Board -> Pin Section 4.2",
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
            bool onPC = CameraViewpointController.Instance != null && CameraViewpointController.Instance.CurrentViewpointId == "Computer";
            bool show = !string.IsNullOrEmpty(label) && !onPC && (CameraViewpointController.Instance == null || !CameraViewpointController.Instance.IsCameraLockedByModal);
            hoverPill.SetActive(show);
            if (show) hoverLabelText.text = label;
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
            SyncModalLock();

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
            typewriterRoutine = StartCoroutine(TypeTextWithVoiceRoutine(phoneBodyText, body, portraitKey));

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

        /// <summary>
        /// Comfortable human reading speed (~27 chars/sec) with natural pauses at sentence punctuation
        /// and gentle indie-game voice mumble blips!
        /// </summary>
        private IEnumerator TypeTextWithVoiceRoutine(Text target, string fullText, string voiceKey)
        {
            target.text = "";
            int spokenCounter = 0;
            for (int i = 0; i < fullText.Length; i++)
            {
                char ch = fullText[i];
                target.text += ch;
                if (!char.IsWhiteSpace(ch) && ch != '.' && ch != ',' && ch != '"' && ch != '!' && ch != '?')
                {
                    spokenCounter++;
                    if (spokenCounter % 3 == 1)
                    {
                        SoundManager.Instance?.PlayVoiceBlip(voiceKey, 0.34f);
                    }
                }

                float wait = (ch == '.' || ch == '!' || ch == '?') ? 0.15f
                           : (ch == ',' || ch == ';') ? 0.08f
                           : 0.036f;
                yield return new WaitForSeconds(wait);
            }
        }

        private void OnPresentEvidenceClicked(string evidenceId)
        {
            if (MissionManager.Instance == null) return;
            bool success = MissionManager.Instance.TrySubmitObjection(evidenceId, out string response, out string hint);
            if (success)
            {
                if (typewriterRoutine != null) StopCoroutine(typewriterRoutine);
                typewriterRoutine = StartCoroutine(TypeTextWithVoiceRoutine(phoneBodyText, response, "alex"));
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
                    ? new Vector2(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f))
                    : Vector2.zero;
                yield return null;
            }
            objectionSplashPanel.SetActive(false);
        }

        // === INTERACTIVE TWO-PAGE OPEN BOOK READER ===
        private void OpenBookReader(string bookKey)
        {
            CloseAllModals();

            switch (bookKey)
            {
                case "manual":
                    bookTitleBanner.text = "INVESTIGATION MANUAL — DIGITAL HEALTH OFFICE";
                    bookCoverBorder.color = new Color(0.11f, 0.20f, 0.38f, 1f);
                    activeBookSpreads = new[]
                    {
                        new BookSpread {
                            leftPage = "<b>CHAPTER 1: THE THREE GOLDEN CHECKS</b>\n\nEvery viral health claim must pass three independent checks before you draw a conclusion:\n\n<b>1. SOURCE</b>\nWho wrote or funded this? Are they an identifiable clinician, institution, or an anonymous blog?",
                            rightPage = "<b>2. EVIDENCE</b>\nIs there peer-reviewed clinical proof or official documentation—or just emotional testimonials?\n\n<b>3. CONTEXT</b>\nWere dates, disclaimers, or privacy policy wording cut out to exaggerate a headline?",
                            stickyNote = "Supervisor's Sticky Note:\n'Welcome aboard, rookie! Keep this in your Evidence drawer—Alex is already blowing up the phone!'"
                        },
                        new BookSpread {
                            leftPage = "<b>CHAPTER 2: UNDERSTANDING DHLI</b>\n\nDigital Health Literacy isn't just knowing how to use a smartphone.\n\nA person can be 12/12 at searching online yet still struggle to judge whether a health blog is reliable (like Alex) or whether an app's permissions are safe (like Sofia).",
                            rightPage = "<b>CHAPTER 3: FAIR INVESTIGATION</b>\n\nRemember: pointing out that a blog is unverified does NOT prove the company is 100% innocent!\n\nSeparate verified facts from unresolved questions in your final report.",
                            stickyNote = ""
                        }
                    };
                    break;

                case "myths":
                    bookTitleBanner.text = "101 VIRAL HEALTH MYTHS — FIELD GUIDE";
                    bookCoverBorder.color = new Color(0.52f, 0.15f, 0.17f, 1f);
                    activeBookSpreads = new[]
                    {
                        new BookSpread {
                            leftPage = "<b>MYTH #14: 'MILLIONS OF VIEWS = MEDICAL PROOF'</b>\n\nSocial media algorithms reward shocking headlines and emotional personal stories—not scientific accuracy.\n\nHigh engagement measures <b>attention</b>, never clinical effectiveness.",
                            rightPage = "<b>SPOTTING SPONSORED CONTENT</b>\n\nInfluencers often share glowing personal stories while linked to paid promotional agreements.\n\nAlways check older posts, affiliate links, and commercial disclosures before trusting a review.",
                            stickyNote = "Yellow Sticky Note (penciled in):\n'Reminder for Case 02: Tell Sofia that 4 million TikTok likes is NOT an FDA clinical trial!'"
                        },
                        new BookSpread {
                            leftPage = "<b>MYTH #42: 'DOCTORS DON'T WANT YOU TO KNOW THIS!'</b>\n\nHeadlines that trigger distrust of institutions often exploit <b>confirmation bias</b>—telling readers what they already suspect.",
                            rightPage = "<b>WHAT TO DO INSTEAD</b>\n\nAsk: 'What transparent evidence supports this claim?'\n\nSkeptical researchers respond best to verifiable facts, never blind appeals to authority.",
                            stickyNote = "Coffee-stained Sticky Note:\n'No, drinking warm lemon water at 3 AM still does not cure everything. — Dr. Rossi'"
                        }
                    };
                    break;

                case "privacy":
                    bookTitleBanner.text = "DIGITAL PRIVACY 101 — HEALTH APPS";
                    bookCoverBorder.color = new Color(0.14f, 0.42f, 0.28f, 1f);
                    activeBookSpreads = new[]
                    {
                        new BookSpread {
                            leftPage = "<b>THE PRINCIPLE OF DATA MINIMIZATION</b>\n\nBefore tapping 'Allow All' on a health app, ask one simple question:\n\n<i>Does this app's stated feature genuinely require this permission to work?</i>",
                            rightPage = "<b>EXAMPLE: PERSONAL HEALTH DIARY</b>\n\n• <b>Needed:</b> User-entered health notes\n• <b>Unnecessary:</b> Contacts list, continuous precise GPS location, background Microphone access.\n\nGrant only what is strictly necessary!",
                            stickyNote = "Neon Sticky Note:\n'If a free calorie tracker asks for your microphone and contacts... maybe it's tracking more than calories!'"
                        },
                        new BookSpread {
                            leftPage = "<b>READING PRIVACY POLICIES</b>\n\nWatch for vague phrases like:\n<i>'Some information may be shared with third-party partners.'</i>\n\nVague wording is a real privacy red flag—even if it doesn't automatically prove illegal data sales.",
                            rightPage = "<b>OFFICIAL HEALTH PORTALS</b>\n\nAlways verify hospital or public-health URLs against a trusted directory before logging in or trusting 'Official Approval' screenshots.",
                            stickyNote = ""
                        }
                    };
                    break;

                default: // coffee
                    bookTitleBanner.text = "DETECTIVE'S ESPRESSO & SURVIVAL GUIDE";
                    bookCoverBorder.color = new Color(0.56f, 0.36f, 0.16f, 1f);
                    activeBookSpreads = new[]
                    {
                        new BookSpread {
                            leftPage = "<b>OFFICE SURVIVAL RULE #1</b>\n\nNever attempt to read a 40-page mobile app Privacy Policy before your second Italian espresso.\n\nSide effects include accidentally tapping 'Allow All' on everything.",
                            rightPage = "<b>OFFICE SURVIVAL RULE #2</b>\n\nWhen a citizen calls shouting that an anonymous blog 'proves everything', stay calm, brew a cup, and check:\n\n<b>Source — Evidence — Context.</b>",
                            stickyNote = "Hidden Sticky Note:\n'PS: Someone hid the good espresso beans in Drawer B-03... never mind, it's just blank forms again.'"
                        },
                        new BookSpread {
                            leftPage = "<b>WHY HEALTH LITERACY MATTERS</b>\n\nIn a world full of viral health apps, empowering people to evaluate sources, protect privacy, and navigate official portals saves real lives.",
                            rightPage = "<b>POLIMI eHEALTH OFFICE MOTTO</b>\n\n<i>'The truth isn't always hidden. Sometimes, we just need to know how to look for it.'</i>",
                            stickyNote = ""
                        }
                    };
                    break;
            }

            activeSpreadIndex = 0;
            bookModalPanel.SetActive(true);
            SyncModalLock();
            RefreshBookSpread();
        }

        private void RefreshBookSpread()
        {
            if (activeBookSpreads == null || activeBookSpreads.Length == 0) return;
            BookSpread sp = activeBookSpreads[activeSpreadIndex];
            bookLeftPageText.text = sp.leftPage;
            bookRightPageText.text = sp.rightPage;
            bookPageCounterText.text = $"Spread {activeSpreadIndex + 1} / {activeBookSpreads.Length}";
            bookPrevBtn.interactable = activeSpreadIndex > 0;
            bookNextBtn.interactable = activeSpreadIndex < activeBookSpreads.Length - 1;

            bool hasSticky = !string.IsNullOrEmpty(sp.stickyNote);
            stickyNoteObj.SetActive(hasSticky);
            if (hasSticky) stickyNoteText.text = sp.stickyNote;
        }

        private void OpenMailLogModal()
        {
            CloseAllModals();
            ClipboardPickupAnimator.Instance?.SetHeld(true);
            clipboardModalPanel.SetActive(true);
            SyncModalLock();
        }

        private void OpenAlexLetterModal() => OpenDocModal(
            "ALEX'S LETTER (letter_alex)",
            "From: Alex Moretti\n\n" +
            "<b>\"VitaPulse is selling everyone's private medical information!\"</b>\n\n" +
            "Blog URL unlocked in HealthBrowser on your 3D Monitor:\n<i>http://truth-health-watch.blog/vitapulse-exposed</i>"
        );

        private void OpenDocModal(string title, string body)
        {
            CloseAllModals();
            docModalTitle.text = title;
            docModalBody.text = body;
            docModalPanel.SetActive(true);
            SyncModalLock();
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

            // 1. Compact Top-Left Mission Pill
            RectTransform objPill = CreatePanel(canvasGo.transform, "ObjectivePill", new Color(0.07f, 0.10f, 0.16f, 0.84f), raycast: false);
            objPill.anchorMin = new Vector2(0f, 1f); objPill.anchorMax = new Vector2(0f, 1f); objPill.pivot = new Vector2(0f, 1f);
            objPill.sizeDelta = new Vector2(560f, 36f); objPill.anchoredPosition = new Vector2(14f, -12f);
            objectiveText = CreateText(objPill, "ObjText", "Loading...", 14, new Color(1f, 0.92f, 0.45f), TextAnchor.MiddleLeft, false);
            objectiveText.fontStyle = FontStyle.Bold;
            SetFill(objectiveText.GetComponent<RectTransform>(), 14, 4);

            // 2. Top-Right 2 Compact Drawer Buttons
            RectTransform topRight = CreatePanel(canvasGo.transform, "TopRightBtns", new Color(0, 0, 0, 0), raycast: false);
            topRight.anchorMin = new Vector2(1f, 1f); topRight.anchorMax = new Vector2(1f, 1f); topRight.pivot = new Vector2(1f, 1f);
            topRight.sizeDelta = new Vector2(276f, 36f); topRight.anchoredPosition = new Vector2(-14f, -12f);
            HorizontalLayoutGroup trLay = topRight.gameObject.AddComponent<HorizontalLayoutGroup>();
            trLay.spacing = 8; trLay.childControlWidth = true; trLay.childForceExpandWidth = true;

            Button invBtn = CreateButton(topRight, "ToggleInvBtn", "Evidence (0)", new Color(0.14f, 0.30f, 0.48f, 0.92f), 34f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                bool next = !inventoryDrawer.activeSelf;
                dhliDrawer.SetActive(false);
                inventoryDrawer.SetActive(next);
            });
            evidenceCountBtnText = invBtn.GetComponentInChildren<Text>();

            CreateButton(topRight, "ToggleDhliBtn", "DHLI Profile", new Color(0.16f, 0.48f, 0.32f, 0.92f), 34f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                bool next = !dhliDrawer.activeSelf;
                inventoryDrawer.SetActive(false);
                dhliDrawer.SetActive(next);
                RefreshDhliDrawer();
            });

            // 3. Effortless Left & Right Screen Turn Arrows (◄ / ►)
            Button leftArrow = CreateButton(canvasGo.transform, "TurnLeftArrow", "◄", new Color(0.08f, 0.12f, 0.18f, 0.68f), 68f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.4f);
                CameraViewpointController.Instance?.TurnLeft();
            });
            RectTransform laRect = leftArrow.GetComponent<RectTransform>();
            laRect.anchorMin = new Vector2(0f, 0.5f); laRect.anchorMax = new Vector2(0f, 0.5f); laRect.pivot = new Vector2(0f, 0.5f);
            laRect.sizeDelta = new Vector2(48f, 76f); laRect.anchoredPosition = new Vector2(12f, 0f);
            leftArrow.GetComponentInChildren<Text>().fontSize = 26;
            leftTurnBtnObj = leftArrow.gameObject;

            Button rightArrow = CreateButton(canvasGo.transform, "TurnRightArrow", "►", new Color(0.08f, 0.12f, 0.18f, 0.68f), 68f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.4f);
                CameraViewpointController.Instance?.TurnRight();
            });
            RectTransform raRect = rightArrow.GetComponent<RectTransform>();
            raRect.anchorMin = new Vector2(1f, 0.5f); raRect.anchorMax = new Vector2(1f, 0.5f); raRect.pivot = new Vector2(1f, 0.5f);
            raRect.sizeDelta = new Vector2(48f, 76f); raRect.anchoredPosition = new Vector2(-12f, 0f);
            rightArrow.GetComponentInChildren<Text>().fontSize = 26;
            rightTurnBtnObj = rightArrow.gameObject;

            Button backBtn = CreateButton(canvasGo.transform, "BackToDeskBtn", "↓ Back to Desk (Esc)", new Color(0.12f, 0.24f, 0.38f, 0.88f), 34f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.45f);
                CameraViewpointController.Instance?.MoveToViewpoint("Desk");
            });
            RectTransform bdRect = backBtn.GetComponent<RectTransform>();
            bdRect.anchorMin = new Vector2(0.5f, 0f); bdRect.anchorMax = new Vector2(0.5f, 0f); bdRect.pivot = new Vector2(0.5f, 0f);
            bdRect.sizeDelta = new Vector2(195f, 34f); bdRect.anchoredPosition = new Vector2(0f, 14f);
            backToDeskBtnObj = backBtn.gameObject;

            // 4. Toast & Hover Pill
            RectTransform toast = CreatePanel(canvasGo.transform, "ToastBanner", new Color(0.08f, 0.14f, 0.22f, 0.88f), raycast: false);
            toast.anchorMin = new Vector2(0.22f, 1f); toast.anchorMax = new Vector2(0.78f, 1f); toast.pivot = new Vector2(0.5f, 1f);
            toast.sizeDelta = new Vector2(0f, 34f); toast.anchoredPosition = new Vector2(0f, -56f);
            toastText = CreateText(toast, "ToastText", "", 13, Color.white, TextAnchor.MiddleCenter, false);
            SetFill(toastText.GetComponent<RectTransform>(), 12, 4);
            toastBanner = toast.gameObject; toastBanner.SetActive(false);

            RectTransform hover = CreatePanel(canvasGo.transform, "HoverPill", new Color(0.06f, 0.09f, 0.14f, 0.84f), raycast: false);
            hover.anchorMin = new Vector2(0.32f, 0f); hover.anchorMax = new Vector2(0.68f, 0f); hover.pivot = new Vector2(0.5f, 0f);
            hover.sizeDelta = new Vector2(0f, 32f); hover.anchoredPosition = new Vector2(0f, 56f);
            hoverLabelText = CreateText(hover, "HoverText", "", 14, new Color(1f, 0.94f, 0.52f), TextAnchor.MiddleCenter, false);
            hoverLabelText.fontStyle = FontStyle.Bold;
            SetFill(hoverLabelText.GetComponent<RectTransform>(), 10, 2);
            hoverPill = hover.gameObject; hoverPill.SetActive(false);

            // 5. Drawers & Modals
            BuildEvidenceDrawer(canvasGo.transform);
            BuildDhliDrawer(canvasGo.transform);
            BuildWelcomeModal(canvasGo.transform);
            BuildPhoneModal(canvasGo.transform);
            BuildOpenBookModal(canvasGo.transform);
            BuildPrinterJamModal(canvasGo.transform);
            BuildClipboardModal(canvasGo.transform);
            BuildDocModal(canvasGo.transform);
            BuildObjectionSplash(canvasGo.transform);
        }

        private void BuildOpenBookModal(Transform parent)
        {
            // Dark hardcover binding outer frame
            RectTransform cover = CreatePanel(parent, "OpenBookModal", new Color(0.14f, 0.22f, 0.36f, 0.99f), raycast: true);
            cover.anchorMin = new Vector2(0.11f, 0.13f); cover.anchorMax = new Vector2(0.89f, 0.87f);
            cover.offsetMin = Vector2.zero; cover.offsetMax = Vector2.zero;
            bookCoverBorder = cover.GetComponent<Image>();

            // Title ribbon at top
            bookTitleBanner = CreateText(cover, "BookTitle", "BOOK", 16, new Color(1f, 0.90f, 0.55f), TextAnchor.MiddleCenter, false);
            bookTitleBanner.fontStyle = FontStyle.Bold;
            RectTransform btRect = bookTitleBanner.GetComponent<RectTransform>();
            btRect.anchorMin = new Vector2(0f, 1f); btRect.anchorMax = new Vector2(1f, 1f); btRect.pivot = new Vector2(0.5f, 1f);
            btRect.sizeDelta = new Vector2(0f, 34f); btRect.anchoredPosition = new Vector2(0f, -4f);

            // Two Cream-White Open Paper Pages!
            GameObject pagesRow = new GameObject("PagesRow");
            pagesRow.transform.SetParent(cover, false);
            RectTransform prRect = pagesRow.AddComponent<RectTransform>();
            prRect.anchorMin = Vector2.zero; prRect.anchorMax = Vector2.one;
            prRect.offsetMin = new Vector2(16f, 54f); prRect.offsetMax = new Vector2(-16f, -40f);
            HorizontalLayoutGroup pLay = pagesRow.AddComponent<HorizontalLayoutGroup>();
            pLay.spacing = 6; pLay.childControlWidth = true; pLay.childForceExpandWidth = true;

            Color paperCream = new Color(0.96f, 0.94f, 0.88f, 1f);
            Color inkDark = new Color(0.14f, 0.13f, 0.12f, 1f);

            RectTransform leftPage = CreatePanel(pagesRow.transform, "LeftPage", paperCream, raycast: false);
            bookLeftPageText = CreateText(leftPage, "LeftTxt", "", 15, inkDark, TextAnchor.UpperLeft, false);
            SetFill(bookLeftPageText.GetComponent<RectTransform>(), 24f, 22f);

            RectTransform rightPage = CreatePanel(pagesRow.transform, "RightPage", paperCream, raycast: false);
            bookRightPageText = CreateText(rightPage, "RightTxt", "", 15, inkDark, TextAnchor.UpperLeft, false);
            SetFill(bookRightPageText.GetComponent<RectTransform>(), 24f, 22f);

            // Tilted Yellow Detective Sticky Note tucked onto the bottom-right page!
            RectTransform sticky = CreatePanel(rightPage, "StickyNote", new Color(1.0f, 0.93f, 0.48f, 1f), raycast: false);
            sticky.anchorMin = new Vector2(1f, 0f); sticky.anchorMax = new Vector2(1f, 0f); sticky.pivot = new Vector2(1f, 0f);
            sticky.sizeDelta = new Vector2(280f, 108f);
            sticky.anchoredPosition = new Vector2(-18f, 16f);
            sticky.localRotation = Quaternion.Euler(0f, 0f, -3.5f);
            stickyNoteText = CreateText(sticky, "StickyTxt", "", 13, new Color(0.22f, 0.18f, 0.08f), TextAnchor.UpperLeft, false);
            stickyNoteText.fontStyle = FontStyle.Italic;
            SetFill(stickyNoteText.GetComponent<RectTransform>(), 12f, 10f);
            stickyNoteObj = sticky.gameObject;

            // Bottom Page-Flip Controls Bar
            GameObject navRow = new GameObject("BookNav");
            navRow.transform.SetParent(cover, false);
            RectTransform nrRect = navRow.AddComponent<RectTransform>();
            nrRect.anchorMin = new Vector2(0f, 0f); nrRect.anchorMax = new Vector2(1f, 0f); nrRect.pivot = new Vector2(0.5f, 0f);
            nrRect.sizeDelta = new Vector2(0f, 44f); nrRect.anchoredPosition = new Vector2(0f, 6f);
            HorizontalLayoutGroup nLay = navRow.AddComponent<HorizontalLayoutGroup>();
            nLay.padding = new RectOffset(20, 20, 4, 4); nLay.spacing = 14;
            nLay.childControlWidth = true; nLay.childForceExpandWidth = true;

            bookPrevBtn = CreateButton(navRow.transform, "PrevPage", "◄ Previous Page", new Color(0.20f, 0.28f, 0.38f, 1f), 34f, () =>
            {
                if (activeSpreadIndex > 0)
                {
                    activeSpreadIndex--;
                    SoundManager.Instance?.Play("paper_rustle", 0.75f);
                    RefreshBookSpread();
                }
            });

            bookPageCounterText = CreateText(navRow.transform, "PageCounter", "Spread 1 / 2", 14, Color.white, TextAnchor.MiddleCenter, false);

            bookNextBtn = CreateButton(navRow.transform, "NextPage", "Next Page ►", new Color(0.20f, 0.28f, 0.38f, 1f), 34f, () =>
            {
                if (activeBookSpreads != null && activeSpreadIndex < activeBookSpreads.Length - 1)
                {
                    activeSpreadIndex++;
                    SoundManager.Instance?.Play("paper_rustle", 0.75f);
                    RefreshBookSpread();
                }
            });

            CreateButton(navRow.transform, "CloseBook", "Close Book [X]", new Color(0.62f, 0.20f, 0.18f, 1f), 34f, () =>
            {
                SoundManager.Instance?.Play("paper_rustle", 0.6f);
                bookModalPanel.SetActive(false);
                SyncModalLock();
            });

            bookModalPanel = cover.gameObject;
            bookModalPanel.SetActive(false);
        }

        // === INTERACTIVE PRINTER PAPER-JAM MINI-GAME + CRUMPLED A4 PHOTOCOPY IN HAND ===
        private void OpenPrinterJamModal()
        {
            CloseAllModals();
            printerJamModal.SetActive(true);
            SyncModalLock();

            bool stillJammed = PrinterJamEffect.Instance == null || PrinterJamEffect.Instance.IsJammed;
            if (stillJammed)
            {
                printerPullCount = 0;
                printerJamPhaseObj.SetActive(true);
                printerPhotocopyPhaseObj.SetActive(false);
                jammedPaperRect.anchoredPosition = new Vector2(0f, -24f);
                jammedPaperRect.localRotation = Quaternion.Euler(0f, 0f, -7f);
                printerJamStatusText.text = "ERROR E-09: PAPER JAM IN OUTPUT TRAY!\nGrab & tug the stuck A4 sheet out of the rollers!";
                printerPullBtnText.text = "PULL JAMMED PAPER!  (0 / 3)";
            }
            else
            {
                printerJamPhaseObj.SetActive(false);
                printerPhotocopyPhaseObj.SetActive(true);
            }
        }

        private void OnPullJammedPaperClicked()
        {
            printerPullCount++;
            SoundManager.Instance?.Play("paper_rustle", 0.9f);

            if (printerPullCount == 1)
            {
                jammedPaperRect.anchoredPosition = new Vector2(-12f, -58f);
                jammedPaperRect.localRotation = Quaternion.Euler(0f, 0f, 5f);
                printerJamStatusText.text = "It's budging! One side is still caught in the toner roller — PULL AGAIN!";
                printerPullBtnText.text = "PULL HARDER!  (1 / 3)";
            }
            else if (printerPullCount == 2)
            {
                jammedPaperRect.anchoredPosition = new Vector2(10f, -94f);
                jammedPaperRect.localRotation = Quaternion.Euler(0f, 0f, -4f);
                printerJamStatusText.text = "Almost free! One last tug to yank the crumpled A4 sheet out!";
                printerPullBtnText.text = "YANK IT FREE!  (2 / 3)";
            }
            else
            {
                SoundManager.Instance?.Play("evidence_unlock", 0.8f);
                PrinterJamEffect.Instance?.ClearJam();
                printerJamPhaseObj.SetActive(false);
                printerPhotocopyPhaseObj.SetActive(true);
                StopActiveSpeech();
                typewriterRoutine = StartCoroutine(TypeTextWithVoiceRoutine(
                    detectiveCommentText,
                    "Investigator: \"Wait a second... who on earth photocopies their own face on an office printer?! No wonder Tray B jammed!\"",
                    "supervisor"
                ));
            }
        }

        private void BuildPrinterJamModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "PrinterJamModal", new Color(0.06f, 0.08f, 0.12f, 0.96f), raycast: true);
            modal.anchorMin = new Vector2(0.22f, 0.08f); modal.anchorMax = new Vector2(0.78f, 0.92f);
            modal.offsetMin = Vector2.zero; modal.offsetMax = Vector2.zero;

            // --- PHASE 1: PAPER JAM MINI-GAME ---
            RectTransform jamPhase = CreatePanel(modal, "JamPhase", new Color(0, 0, 0, 0), raycast: false);
            SetFill(jamPhase, 24f, 20f);

            RectTransform lcd = CreatePanel(jamPhase, "PrinterLCD", new Color(0.45f, 0.10f, 0.08f, 1f), raycast: false);
            lcd.anchorMin = new Vector2(0f, 1f); lcd.anchorMax = new Vector2(1f, 1f); lcd.pivot = new Vector2(0.5f, 1f);
            lcd.sizeDelta = new Vector2(0f, 76f); lcd.anchoredPosition = Vector2.zero;
            printerJamStatusText = CreateText(lcd, "LCDTxt", "ERROR E-09: PAPER JAM!", 15, new Color(1f, 0.90f, 0.55f), TextAnchor.MiddleCenter, false);
            printerJamStatusText.fontStyle = FontStyle.Bold;
            SetFill(printerJamStatusText.GetComponent<RectTransform>(), 14f, 8f);

            // Printer dark roller slot + crooked jammed paper sticking out
            RectTransform slotBg = CreatePanel(jamPhase, "RollerSlotBg", new Color(0.16f, 0.18f, 0.22f, 1f), raycast: false);
            slotBg.anchorMin = new Vector2(0.10f, 0.24f); slotBg.anchorMax = new Vector2(0.90f, 0.80f);
            slotBg.offsetMin = Vector2.zero; slotBg.offsetMax = Vector2.zero;

            RectTransform slotOpening = CreatePanel(slotBg, "SlotOpening", new Color(0.03f, 0.04f, 0.06f, 1f), raycast: false);
            slotOpening.anchorMin = new Vector2(0.08f, 0.72f); slotOpening.anchorMax = new Vector2(0.92f, 0.84f);
            slotOpening.offsetMin = Vector2.zero; slotOpening.offsetMax = Vector2.zero;

            RectTransform stuckPaper = CreatePanel(slotBg, "StuckPaperSheet", new Color(0.94f, 0.92f, 0.86f, 1f), raycast: false);
            stuckPaper.anchorMin = new Vector2(0.5f, 0.76f); stuckPaper.anchorMax = new Vector2(0.5f, 0.76f); stuckPaper.pivot = new Vector2(0.5f, 1f);
            stuckPaper.sizeDelta = new Vector2(220f, 145f);
            stuckPaper.anchoredPosition = new Vector2(0f, -24f);
            stuckPaper.localRotation = Quaternion.Euler(0f, 0f, -7f);
            Text spTxt = CreateText(stuckPaper, "StuckTxt", "=== JAMMED A4 SHEET ===\n[CRUMPLED PHOTOCOPY]\n\n...stuck in rollers...", 13, new Color(0.18f, 0.18f, 0.20f), TextAnchor.MiddleCenter, false);
            SetFill(spTxt.GetComponent<RectTransform>(), 10f, 10f);
            jammedPaperRect = stuckPaper;

            Button pullBtn = CreateButton(jamPhase, "PullJamBtn", "PULL JAMMED PAPER! (0 / 3)", new Color(0.84f, 0.34f, 0.12f, 1f), 46f, OnPullJammedPaperClicked);
            RectTransform pbRect = pullBtn.GetComponent<RectTransform>();
            pbRect.anchorMin = new Vector2(0.12f, 0f); pbRect.anchorMax = new Vector2(0.88f, 0f); pbRect.pivot = new Vector2(0.5f, 0f);
            pbRect.sizeDelta = new Vector2(0f, 46f); pbRect.anchoredPosition = new Vector2(0f, 52f);
            printerPullBtnText = pullBtn.GetComponentInChildren<Text>();
            printerPullBtnText.fontSize = 16; printerPullBtnText.fontStyle = FontStyle.Bold;

            Button cancelJamBtn = CreateButton(jamPhase, "CloseJamBtn", "Step Away", new Color(0.24f, 0.28f, 0.36f, 1f), 36f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                printerJamModal.SetActive(false);
                SyncModalLock();
            });
            RectTransform cjRect = cancelJamBtn.GetComponent<RectTransform>();
            cjRect.anchorMin = new Vector2(0.32f, 0f); cjRect.anchorMax = new Vector2(0.68f, 0f); cjRect.pivot = new Vector2(0.5f, 0f);
            cjRect.sizeDelta = new Vector2(0f, 36f); cjRect.anchoredPosition = new Vector2(0f, 6f);
            printerJamPhaseObj = jamPhase.gameObject;

            // --- PHASE 2: HOLDING THE CRUMPLED A4 PHOTOCOPIED PHOTO IN HAND! ---
            RectTransform photoPhase = CreatePanel(modal, "PhotocopyPhase", new Color(0, 0, 0, 0), raycast: false);
            SetFill(photoPhase, 20f, 14f);

            Text photoHdr = CreateText(photoPhase, "PhotoHdr", "RECOVERED CRUMPLED A4 PHOTOCOPY (FROM JAMMED PRINTER)", 14, new Color(1f, 0.90f, 0.45f), TextAnchor.MiddleCenter, false);
            photoHdr.fontStyle = FontStyle.Bold;
            RectTransform phRect = photoHdr.GetComponent<RectTransform>();
            phRect.anchorMin = new Vector2(0f, 1f); phRect.anchorMax = new Vector2(1f, 1f); phRect.pivot = new Vector2(0.5f, 1f);
            phRect.sizeDelta = new Vector2(0f, 28f); phRect.anchoredPosition = Vector2.zero;

            // Slightly tilted crumpled A4 paper sheet held in hand!
            RectTransform a4Frame = CreatePanel(photoPhase, "A4PaperInHand", new Color(0.93f, 0.92f, 0.88f, 1f), raycast: false);
            a4Frame.anchorMin = new Vector2(0.5f, 0.59f); a4Frame.anchorMax = new Vector2(0.5f, 0.59f); a4Frame.pivot = new Vector2(0.5f, 0.5f);
            a4Frame.sizeDelta = new Vector2(278f, 390f);
            a4Frame.localRotation = Quaternion.Euler(0f, 0f, -2.8f);

            GameObject rawObj = new GameObject("PhotocopyRaw");
            rawObj.transform.SetParent(a4Frame, false);
            SetFill(rawObj.AddComponent<RectTransform>(), 4f, 4f);
            photocopyRawImage = rawObj.AddComponent<RawImage>();
            if (photocopyTexture != null) photocopyRawImage.texture = photocopyTexture;

            // Detective's own witty reaction dialogue box below the A4 sheet!
            RectTransform commentBox = CreatePanel(photoPhase, "DetectiveCommentBox", new Color(0.09f, 0.13f, 0.20f, 0.95f), raycast: false);
            commentBox.anchorMin = new Vector2(0.06f, 0f); commentBox.anchorMax = new Vector2(0.94f, 0f); commentBox.pivot = new Vector2(0.5f, 0f);
            commentBox.sizeDelta = new Vector2(0f, 64f); commentBox.anchoredPosition = new Vector2(0f, 48f);
            detectiveCommentText = CreateText(commentBox, "DetTxt",
                "Investigator: \"Wait a second... who on earth photocopies their own face on an office printer?!\"",
                14, new Color(0.95f, 0.96f, 0.98f), TextAnchor.MiddleLeft, false);
            detectiveCommentText.fontStyle = FontStyle.Italic;
            SetFill(detectiveCommentText.GetComponent<RectTransform>(), 14f, 6f);

            GameObject photoBtns = new GameObject("PhotoBtns");
            photoBtns.transform.SetParent(photoPhase, false);
            RectTransform pbsRect = photoBtns.AddComponent<RectTransform>();
            pbsRect.anchorMin = new Vector2(0.14f, 0f); pbsRect.anchorMax = new Vector2(0.86f, 0f); pbsRect.pivot = new Vector2(0.5f, 0f);
            pbsRect.sizeDelta = new Vector2(0f, 38f); pbsRect.anchoredPosition = new Vector2(0f, 2f);
            HorizontalLayoutGroup pbLay = photoBtns.AddComponent<HorizontalLayoutGroup>();
            pbLay.spacing = 12; pbLay.childControlWidth = true; pbLay.childForceExpandWidth = true;

            CreateButton(photoBtns.transform, "ReJamBtn", "Re-Test Paper Jam", new Color(0.54f, 0.32f, 0.16f, 1f), 36f, () =>
            {
                StopActiveSpeech();
                SoundManager.Instance?.Play("wrong_buzz", 0.45f);
                PrinterJamEffect.Instance?.TriggerJamAgain();
                OpenPrinterJamModal();
            });
            CreateButton(photoBtns.transform, "ClosePhotoBtn", "Put Down A4 Sheet [X]", new Color(0.18f, 0.48f, 0.30f, 1f), 36f, () =>
            {
                StopActiveSpeech();
                SoundManager.Instance?.Play("paper_rustle", 0.65f);
                printerJamModal.SetActive(false);
                SyncModalLock();
            });

            printerPhotocopyPhaseObj = photoPhase.gameObject;
            printerJamModal = modal.gameObject;
            printerJamModal.SetActive(false);
        }

        // === REALISTIC WOODEN CLIPBOARD-IN-HAND MODAL (MAIL DELIVERY LOG) ===
        private void BuildClipboardModal(Transform parent)
        {
            // Walnut wooden clipboard backing
            RectTransform board = CreatePanel(parent, "ClipboardModal", new Color(0.36f, 0.22f, 0.12f, 0.99f), raycast: true);
            board.anchorMin = new Vector2(0.31f, 0.10f); board.anchorMax = new Vector2(0.69f, 0.90f);
            board.offsetMin = Vector2.zero; board.offsetMax = Vector2.zero;

            // Brushed metallic clip at top center
            RectTransform metalClip = CreatePanel(board, "MetalClip", new Color(0.64f, 0.68f, 0.74f, 1f), raycast: false);
            metalClip.anchorMin = new Vector2(0.5f, 1f); metalClip.anchorMax = new Vector2(0.5f, 1f); metalClip.pivot = new Vector2(0.5f, 1f);
            metalClip.sizeDelta = new Vector2(150f, 34f); metalClip.anchoredPosition = new Vector2(0f, -6f);

            // Warm cream lined A4 paper sheet
            RectTransform paper = CreatePanel(board, "ClipboardPaper", new Color(0.96f, 0.94f, 0.88f, 1f), raycast: false);
            paper.anchorMin = Vector2.zero; paper.anchorMax = Vector2.one;
            paper.offsetMin = new Vector2(22f, 62f); paper.offsetMax = new Vector2(-22f, -34f);

            // Navy official office header band
            RectTransform hdrBand = CreatePanel(paper, "PaperHeader", new Color(0.13f, 0.28f, 0.48f, 1f), raycast: false);
            hdrBand.anchorMin = new Vector2(0f, 1f); hdrBand.anchorMax = new Vector2(1f, 1f); hdrBand.pivot = new Vector2(0.5f, 1f);
            hdrBand.sizeDelta = new Vector2(0f, 44f); hdrBand.anchoredPosition = Vector2.zero;
            Text hdrTxt = CreateText(hdrBand, "HT", "MAIL DELIVERY LOG — ARCHIVE INDEX", 16, Color.white, TextAnchor.MiddleCenter, false);
            hdrTxt.fontStyle = FontStyle.Bold;
            SetFill(hdrTxt.GetComponent<RectTransform>(), 8f, 4f);

            Text bodyTxt = CreateText(paper, "ClipboardBody",
                "<b>FILING CABINET (LEFT WALL):</b>\n" +
                "──────────────────────────────────\n\n" +
                "<b>• Drawer B-01 (Top)</b>\n" +
                "   Old 2023 Office Receipts\n\n" +
                "<color=#9E1A1A><b>• Drawer B-02 (Middle)  ★ PRIORITY</b></color>\n" +
                "   <b>A. MORETTI — Yellow Envelope</b>\n" +
                "   <i>Subject: VitaPulse Blog Allegations</i>\n\n" +
                "<b>• Drawer B-03 (Bottom)</b>\n" +
                "   Blank Intake Forms & Stationery\n\n" +
                "<i>Tip: Turn Left (◄) to the Archive Cabinet, slide open Drawer B-02, and collect Alex's yellow envelope!</i>",
                15, new Color(0.14f, 0.14f, 0.16f), TextAnchor.UpperLeft, false);
            RectTransform bRt = bodyTxt.GetComponent<RectTransform>();
            bRt.anchorMin = Vector2.zero; bRt.anchorMax = Vector2.one;
            bRt.offsetMin = new Vector2(24f, 16f); bRt.offsetMax = new Vector2(-24f, -58f);

            Button putDownBtn = CreateButton(board, "PutDownClipboard", "Put Clipboard Back on Desk [X]", new Color(0.20f, 0.34f, 0.48f, 1f), 40f, () =>
            {
                SoundManager.Instance?.Play("paper_rustle", 0.65f);
                clipboardModalPanel.SetActive(false);
                ClipboardPickupAnimator.Instance?.SetHeld(false);
                SyncModalLock();
            });
            RectTransform pdRect = putDownBtn.GetComponent<RectTransform>();
            pdRect.anchorMin = new Vector2(0.14f, 0f); pdRect.anchorMax = new Vector2(0.86f, 0f); pdRect.pivot = new Vector2(0.5f, 0f);
            pdRect.sizeDelta = new Vector2(0f, 40f); pdRect.anchoredPosition = new Vector2(0f, 12f);

            clipboardModalPanel = board.gameObject;
            clipboardModalPanel.SetActive(false);
        }

        private void BuildEvidenceDrawer(Transform parent)
        {
            RectTransform panel = CreatePanel(parent, "EvidenceDrawer", new Color(0.07f, 0.10f, 0.16f, 0.95f), raycast: true);
            panel.anchorMin = new Vector2(1f, 1f); panel.anchorMax = new Vector2(1f, 1f); panel.pivot = new Vector2(1f, 1f);
            panel.sizeDelta = new Vector2(310f, 440f); panel.anchoredPosition = new Vector2(-14f, -56f);

            VerticalLayoutGroup lay = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(12, 12, 10, 10); lay.spacing = 6;
            lay.childControlHeight = false; lay.childForceExpandHeight = false;

            Text hdr = CreateText(panel, "Hdr", "COLLECTED EVIDENCE", 14, new Color(0.65f, 0.88f, 1f), TextAnchor.MiddleLeft, false);
            hdr.fontStyle = FontStyle.Bold; hdr.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 22f);

            GameObject listObj = new GameObject("List");
            listObj.transform.SetParent(panel, false);
            listObj.AddComponent<RectTransform>().sizeDelta = new Vector2(286f, 220f);
            VerticalLayoutGroup lLay = listObj.AddComponent<VerticalLayoutGroup>();
            lLay.spacing = 4; lLay.childControlHeight = false; lLay.childForceExpandHeight = false;
            inventoryListParent = listObj.transform;

            evidenceDetailText = CreateText(panel, "Detail", "Click any card above to inspect.", 12, new Color(0.9f, 0.94f, 0.98f), TextAnchor.UpperLeft, false);
            evidenceDetailText.GetComponent<RectTransform>().sizeDelta = new Vector2(286f, 130f);

            CreateButton(panel, "CloseInv", "Close", new Color(0.26f, 0.32f, 0.42f, 1f), 30f, () => inventoryDrawer.SetActive(false));
            inventoryDrawer = panel.gameObject; inventoryDrawer.SetActive(false);
        }

        private void BuildDhliDrawer(Transform parent)
        {
            RectTransform panel = CreatePanel(parent, "DhliDrawer", new Color(0.07f, 0.10f, 0.16f, 0.95f), raycast: true);
            panel.anchorMin = new Vector2(1f, 1f); panel.anchorMax = new Vector2(1f, 1f); panel.pivot = new Vector2(1f, 1f);
            panel.sizeDelta = new Vector2(330f, 290f); panel.anchoredPosition = new Vector2(-14f, -56f);

            VerticalLayoutGroup lay = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(14, 14, 12, 12); lay.spacing = 8;
            lay.childControlHeight = false; lay.childForceExpandHeight = false;

            dhliDetailText = CreateText(panel, "DhliText", "", 13, Color.white, TextAnchor.UpperLeft, false);
            dhliDetailText.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 215f);
            CreateButton(panel, "CloseDhli", "Close", new Color(0.26f, 0.32f, 0.42f, 1f), 30f, () => dhliDrawer.SetActive(false));
            dhliDrawer = panel.gameObject; dhliDrawer.SetActive(false);
        }

        private void BuildWelcomeModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "WelcomeModal", new Color(0.06f, 0.09f, 0.15f, 0.96f), raycast: true);
            modal.anchorMin = new Vector2(0.24f, 0.28f); modal.anchorMax = new Vector2(0.76f, 0.72f);
            modal.offsetMin = Vector2.zero; modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup lay = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(28, 28, 22, 22); lay.spacing = 12;
            lay.childControlHeight = false; lay.childForceExpandHeight = false;

            Text title = CreateText(modal, "T", "THE HEALTH FILES: CASE ZERO", 22, new Color(1f, 0.88f, 0.38f), TextAnchor.MiddleCenter, false);
            title.fontStyle = FontStyle.Bold; title.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 34f);

            Text body = CreateText(modal, "B",
                "A viral health app called <b>VitaPulse</b> is making headlines:\n" +
                "some say it saves lives, others claim it secretly sells medical records.\n\n" +
                "• <b>Move Mouse:</b> Gentle natural look-around\n" +
                "• <b>◄ / ► Arrows (or Q / E):</b> Turn cleanly around your office\n" +
                "• <b>Left-Click:</b> Use Phone, Live 3D Monitor OS, Drawers, Open Books & Board",
                14, Color.white, TextAnchor.UpperLeft, false);
            body.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 165f);

            CreateButton(modal, "StartBtn", "Start Investigation (Day 1)", new Color(0.16f, 0.56f, 0.34f, 1f), 42f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.6f);
                welcomePanel.SetActive(false);
                SyncModalLock();
            });
            welcomePanel = modal.gameObject; welcomePanel.SetActive(false);
        }

        private void BuildPhoneModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "PhoneModal", new Color(0.06f, 0.09f, 0.15f, 0.96f), raycast: true);
            modal.anchorMin = new Vector2(0.16f, 0.16f); modal.anchorMax = new Vector2(0.84f, 0.84f);
            modal.offsetMin = Vector2.zero; modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup lay = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(22, 22, 18, 18); lay.spacing = 10;
            lay.childControlHeight = false; lay.childForceExpandHeight = false;

            phoneSpeakerText = CreateText(modal, "Speaker", "TELEPHONE", 19, new Color(1f, 0.88f, 0.38f), TextAnchor.MiddleLeft, false);
            phoneSpeakerText.fontStyle = FontStyle.Bold; phoneSpeakerText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 28f);

            GameObject row = new GameObject("Row");
            row.transform.SetParent(modal, false);
            row.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 138f);
            HorizontalLayoutGroup rLay = row.AddComponent<HorizontalLayoutGroup>();
            rLay.spacing = 16; rLay.childControlWidth = false; rLay.childForceExpandWidth = false;

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
            bLay.spacing = 5; bLay.childControlHeight = false; bLay.childForceExpandHeight = false;
            objectionButtonContainer = box.transform;

            CreateButton(modal, "HangUpBtn", "Hang Up / Close", new Color(0.26f, 0.32f, 0.42f, 1f), 36f, () =>
            {
                StopActiveSpeech();
                SoundManager.Instance?.Play("ui_click", 0.5f);
                phonePanel.SetActive(false);
                SyncModalLock();
            });
            phonePanel = modal.gameObject; phonePanel.SetActive(false);
        }

        private void BuildDocModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "DocModal", new Color(0.07f, 0.10f, 0.16f, 0.96f), raycast: true);
            modal.anchorMin = new Vector2(0.22f, 0.24f); modal.anchorMax = new Vector2(0.78f, 0.76f);
            modal.offsetMin = Vector2.zero; modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup lay = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(24, 24, 20, 20); lay.spacing = 12;
            lay.childControlHeight = false; lay.childForceExpandHeight = false;

            docModalTitle = CreateText(modal, "T", "DOCUMENT", 19, new Color(1f, 0.88f, 0.40f), TextAnchor.MiddleLeft, false);
            docModalTitle.fontStyle = FontStyle.Bold; docModalTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 30f);

            docModalBody = CreateText(modal, "B", "", 15, Color.white, TextAnchor.UpperLeft, false);
            docModalBody.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 220f);

            CreateButton(modal, "CloseDoc", "Close", new Color(0.26f, 0.32f, 0.42f, 1f), 38f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                docModalPanel.SetActive(false);
                SyncModalLock();
            });
            docModalPanel = modal.gameObject; docModalPanel.SetActive(false);
        }

        private void BuildObjectionSplash(Transform parent)
        {
            RectTransform overlay = CreatePanel(parent, "ObjectionSplash", new Color(0.78f, 0.10f, 0.10f, 0.92f), raycast: false);
            overlay.anchorMin = new Vector2(0.18f, 0.36f); overlay.anchorMax = new Vector2(0.82f, 0.64f);
            overlay.offsetMin = Vector2.zero; overlay.offsetMax = Vector2.zero;

            objectionSplashText = CreateText(overlay, "Splash", "OBJECTION!", 64, new Color(1f, 0.92f, 0.25f), TextAnchor.MiddleCenter, false);
            objectionSplashText.fontStyle = FontStyle.Bold;
            SetFill(objectionSplashText.GetComponent<RectTransform>(), 0, 0);

            objectionSplashRect = overlay;
            objectionSplashPanel = overlay.gameObject;
            objectionSplashPanel.SetActive(false);
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

        private Text CreateText(Transform parent, string name, string content, int fontSize, Color color, TextAnchor align, bool raycast)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text txt = go.AddComponent<Text>();
            txt.font = uiFont; txt.text = content; txt.fontSize = fontSize;
            txt.color = color; txt.alignment = align; txt.supportRichText = true;
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
