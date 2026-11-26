using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HealthFiles
{
    /// <summary>
    /// WP3 + WP2 Unified Detective HUD for The Health Files: Case Zero:
    /// - Aged Parchment Editorial Dossier UI with Georgia/Segoe UI/Consolas typography & crisp 9-slice rounded corners
    /// - Investigator Badge Setup (3-question DHLI onboarding) + Color-Coded DHLI Skill Bars + Investigator Rank
    /// - 2-Column Case Dossier Pause & Options Menu (Esc / Menu) with Volume, Mute, Display & Controls
    /// - Effortless Left / Right Screen Turn Arrows (◄ / ►) + 'Back to Desk' pill (WP2)
    /// - Live 3D WorldSpace Monitor OS (zooms directly onto the 3D monitor glass — WP2)
    /// - Interactive Two-Page Open Book Reader + Sticky Notes, Printer Paper-Jam Mini-Game & Physical Clipboard (WP2)
    /// - Click-to-Skip Phone Typewriter + Character Voice Blips + Case 01 Complete Summary Modal (WP3)
    /// </summary>
    public class CaseZeroHUD : MonoBehaviour
    {
        private Font uiFont;
        private Font headingFont;
        private Font monoFont;
        private Font handwritingFont;
        private Sprite roundedSprite;
        private Sprite pillSprite;
        private readonly Dictionary<string, Texture2D> portraits = new Dictionary<string, Texture2D>();

        // Rich Aged Parchment Dossier Palette (warmer, lower-glare, tactile game feel)
        private static readonly Color PaperBase      = new Color(0.875f, 0.835f, 0.765f, 0.99f);
        private static readonly Color PaperCard      = new Color(0.824f, 0.773f, 0.686f, 1f);
        private static readonly Color PaperBtn       = new Color(0.765f, 0.706f, 0.612f, 1f);
        private static readonly Color PaperBorder    = new Color(0.56f, 0.49f, 0.39f, 0.95f);
        private static readonly Color InkPrimary     = new Color(0.11f, 0.12f, 0.14f, 1f);
        private static readonly Color InkMuted       = new Color(0.30f, 0.29f, 0.27f, 1f);
        private static readonly Color AccentBurgundy = new Color(0.49f, 0.15f, 0.16f, 1f);
        private static readonly Color AccentSage     = new Color(0.17f, 0.37f, 0.30f, 1f);
        private static readonly Color AccentOchre    = new Color(0.58f, 0.39f, 0.12f, 1f);
        private static readonly Color PaperWhite     = new Color(0.95f, 0.92f, 0.86f, 1f);

        // Minimal HUD
        private Text objectiveText;
        private Text evidenceCountBtnText;
        private Text muteBtnText;
        private GameObject toastBanner;
        private Text toastText;
        private Coroutine toastRoutine;
        private GameObject hoverPill;
        private Text hoverLabelText;

        // Effortless Chair-Turn Arrows + Back to Desk (WP2)
        private GameObject leftTurnBtnObj;
        private GameObject rightTurnBtnObj;
        private GameObject backToDeskBtnObj;

        // Collapsible drawers
        private GameObject inventoryDrawer;
        private Transform inventoryListParent;
        private Text evidenceDetailText;
        private GameObject dhliDrawer;
        private Text dhliSubtitleText;
        private Image relBarFill;
        private Text relBarLabel;
        private Image privBarFill;
        private Text privBarLabel;
        private Image navBarFill;
        private Text navBarLabel;
        private Text dhliDetailText;

        // Modals (WP3)
        private GameObject modalBackdrop;
        private GameObject welcomePanel;
        private GameObject onboardingPanel;
        private readonly int[] onboardingAnswers = { 2, 2, 2 }; // 1=Low, 2=Medium, 3=High
        private readonly Image[,] onboardingOptionImages = new Image[3, 3];
        private readonly Text[,] onboardingOptionTexts = new Text[3, 3];
        private readonly string[,] onboardingBaseLabels =
        {
            { "Usually trust it", "Sometimes double-check", "Always verify the source" },
            { "Tap 'Allow All'", "Check some of them", "Allow only what's needed" },
            { "Confusing to me", "Manageable with time", "Easy & straightforward" }
        };
        private Text onboardingPreviewText;
        private Outline onboardingPreviewOutline;
        private Text onboardingSaveBtnText;
        private Text onboardingBackBtnText;
        private bool returnToPauseAfterOnboarding;

        private GameObject phonePanel;
        private RawImage phonePortraitImage;
        private GameObject phonePortraitFrame;
        private Text phoneSpeakerText;
        private Text phoneBodyText;
        private Text phoneSkipHintText;
        private Text phoneFeedbackText;
        private Transform objectionButtonContainer;
        private Coroutine typewriterRoutine;
        private string currentFullDialogue = "";
        private bool isTypingDialogue;

        private GameObject caseCompletePanel;
        private Text caseCompleteSummaryText;
        private RawImage caseCompletePortrait;

        // Pause & Options Menu (WP3)
        private GameObject pausePanel;
        private GameObject pauseMainView;
        private GameObject pauseOptionsView;
        private Text pauseRankText;
        private Text pauseProgressLabel;
        private Image pauseProgressFill;
        private Text pauseNextObjectiveText;
        private Text pauseCluesAndDhliText;
        private Text optionsVolumeText;
        private Text optionsMuteBtnText;
        private Text optionsFullscreenBtnText;
        private Slider optionsVolumeSlider;

        private GameObject docModalPanel;
        private Text docModalTitle;
        private Text docModalBody;

        // Interactive Two-Page Open Book Reader + Sticky Notes (WP2)
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

        // Printer Paper-Jam Mini-Game + Crumpled A4 Photocopy (WP2)
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

        // Styled Physical Clipboard-in-Hand & Alex's Handwritten Letter Modals
        private GameObject clipboardModalPanel;
        private GameObject deskClipboard3D;
        private GameObject alexLetterModalPanel;
        private readonly HashSet<string> bonusBooksRead = new HashSet<string>();

        // OBJECTION! Splash
        private GameObject objectionSplashPanel;
        private RectTransform objectionSplashRect;
        private Text objectionSplashText;

        private struct BookSpread
        {
            public string leftPage;
            public string rightPage;
            public string stickyNote;
        }

        private void Awake()
        {
            Font fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            uiFont = LoadBestFont(new[] { "Segoe UI", "Calibri", "Helvetica Neue", "Arial" }, 14, fallback);
            headingFont = LoadBestFont(new[] { "Georgia", "Palatino Linotype", "Cambria", "Times New Roman" }, 22, uiFont);
            monoFont = LoadBestFont(new[] { "Consolas", "Courier New", "Menlo" }, 13, uiFont);
            handwritingFont = LoadBestFont(new[] { "Ink Free", "Segoe Script", "Bradley Hand ITC", "Gabriola", "Georgia" }, 18, headingFont);

            roundedSprite = CreateRoundedSprite(128, 28);
            pillSprite = CreateRoundedSprite(64, 12);

            foreach (string key in new[] { "alex", "sofia", "daniel" })
            {
                Texture2D tex = Resources.Load<Texture2D>("Portraits/" + key);
                if (tex != null) portraits[key] = tex;
            }
            photocopyTexture = Resources.Load<Texture2D>("Textures/printer_photocopy_jam");
            BuildCanvasUI();
        }

        private static Font LoadBestFont(string[] candidates, int size, Font fallback)
        {
            try
            {
                Font f = Font.CreateDynamicFontFromOSFont(candidates, size);
                if (f != null) return f;
            }
            catch
            {
                // Fall back safely on platforms without these OS fonts
            }
            return fallback;
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
                MissionManager.Instance.OnCase01Completed += OpenCase01CompleteModal;

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

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.OnAudioSettingsChanged += RefreshAudioUI;
                RefreshAudioUI();
            }

            SyncDeskClipboardVisualToLegalPad();
            EnhanceAlexEnvelopeClickability();

            if (modalBackdrop != null) modalBackdrop.SetActive(true);
            welcomePanel.SetActive(true);
            AnimateModalPopIn(welcomePanel);
            SyncModalLock();
        }

        private void Update()
        {
            bool escPressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                escPressed = true;
#else
            if (Input.GetKeyDown(KeyCode.Escape))
                escPressed = true;
#endif
            if (!escPressed) return;

            if (pausePanel != null && pausePanel.activeSelf)
            {
                if (pauseOptionsView != null && pauseOptionsView.activeSelf)
                {
                    SoundManager.Instance?.Play("ui_click", 0.45f);
                    pauseOptionsView.SetActive(false);
                    pauseMainView.SetActive(true);
                }
                else
                {
                    ClosePauseMenu();
                }
                return;
            }

            if (onboardingPanel != null && onboardingPanel.activeSelf && returnToPauseAfterOnboarding)
            {
                OpenPauseMenu();
                return;
            }

            bool anyOverlayOpen =
                (welcomePanel != null && welcomePanel.activeSelf) ||
                (onboardingPanel != null && onboardingPanel.activeSelf) ||
                (caseCompletePanel != null && caseCompletePanel.activeSelf) ||
                (phonePanel != null && phonePanel.activeSelf) ||
                (docModalPanel != null && docModalPanel.activeSelf) ||
                (bookModalPanel != null && bookModalPanel.activeSelf) ||
                (printerJamModal != null && printerJamModal.activeSelf) ||
                (clipboardModalPanel != null && clipboardModalPanel.activeSelf) ||
                (inventoryDrawer != null && inventoryDrawer.activeSelf) ||
                (dhliDrawer != null && dhliDrawer.activeSelf);

            if (anyOverlayOpen)
            {
                SoundManager.Instance?.Play("ui_click", 0.45f);
                CloseAllModals();
            }
            else if (CameraViewpointController.Instance != null && CameraViewpointController.Instance.CurrentViewpointId != "Desk")
            {
                // If zoomed into Monitor/Cabinet/Bookshelf/Printer, let Esc step back to Desk first (WP2 feel)
                CameraViewpointController.Instance.MoveToViewpoint("Desk");
            }
            else
            {
                OpenPauseMenu();
            }
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
                MissionManager.Instance.OnCase01Completed -= OpenCase01CompleteModal;
            }
            if (CameraViewpointController.Instance != null) CameraViewpointController.Instance.OnViewpointChanged -= HandleViewpointChanged;
            if (EvidenceManager.Instance != null) EvidenceManager.Instance.OnInventoryChanged -= RefreshInventoryUI;
            if (PersonalizationManager.Instance != null) PersonalizationManager.Instance.OnProfileUpdated -= RefreshDhliDrawer;
            if (OfficeRaycaster.Instance != null) OfficeRaycaster.Instance.OnHoverLabelChanged -= HandleHoverChanged;
            if (SoundManager.Instance != null) SoundManager.Instance.OnAudioSettingsChanged -= RefreshAudioUI;
        }

        private void StopActiveSpeech()
        {
            if (typewriterRoutine != null)
            {
                StopCoroutine(typewriterRoutine);
                typewriterRoutine = null;
            }
            isTypingDialogue = false;
            if (phoneSkipHintText != null) phoneSkipHintText.gameObject.SetActive(false);
            SoundManager.Instance?.StopVoice();
        }

        private void SyncModalLock()
        {
            bool anyOpen =
                (welcomePanel != null && welcomePanel.activeSelf) ||
                (onboardingPanel != null && onboardingPanel.activeSelf) ||
                (caseCompletePanel != null && caseCompletePanel.activeSelf) ||
                (pausePanel != null && pausePanel.activeSelf) ||
                (phonePanel != null && phonePanel.activeSelf) ||
                (docModalPanel != null && docModalPanel.activeSelf) ||
                (bookModalPanel != null && bookModalPanel.activeSelf) ||
                (printerJamModal != null && printerJamModal.activeSelf) ||
                (clipboardModalPanel != null && clipboardModalPanel.activeSelf) ||
                (alexLetterModalPanel != null && alexLetterModalPanel.activeSelf);

            if (CameraViewpointController.Instance != null)
            {
                CameraViewpointController.Instance.IsCameraLockedByModal = anyOpen;
            }
            RefreshTurnArrows();
        }

        private void CloseAllModals()
        {
            StopActiveSpeech();
            if (modalBackdrop != null) modalBackdrop.SetActive(false);
            if (welcomePanel != null) welcomePanel.SetActive(false);
            if (onboardingPanel != null) onboardingPanel.SetActive(false);
            if (caseCompletePanel != null) caseCompletePanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (phonePanel != null) phonePanel.SetActive(false);
            if (docModalPanel != null) docModalPanel.SetActive(false);
            if (bookModalPanel != null) bookModalPanel.SetActive(false);
            if (printerJamModal != null) printerJamModal.SetActive(false);
            if (clipboardModalPanel != null) clipboardModalPanel.SetActive(false);
            if (alexLetterModalPanel != null) alexLetterModalPanel.SetActive(false);
            if (deskClipboard3D != null) deskClipboard3D.SetActive(true);
            ClipboardPickupAnimator.Instance?.SetHeld(false);
            if (inventoryDrawer != null) inventoryDrawer.SetActive(false);
            if (dhliDrawer != null) dhliDrawer.SetActive(false);
            SyncModalLock();
        }

        private void HandleViewpointChanged(CameraViewpoint _) => RefreshTurnArrows();

        private void RefreshTurnArrows()
        {
            var ctrl = CameraViewpointController.Instance;
            if (ctrl == null) return;
            bool locked = ctrl.IsCameraLockedByModal;
            bool onPC = ctrl.CurrentViewpointId == "Computer";

            if (leftTurnBtnObj != null) leftTurnBtnObj.SetActive(!locked && !onPC && ctrl.CanTurnLeft());
            if (rightTurnBtnObj != null) rightTurnBtnObj.SetActive(!locked && !onPC && ctrl.CanTurnRight());
            if (backToDeskBtnObj != null) backToDeskBtnObj.SetActive(!locked && ctrl.CurrentViewpointId != "Desk");
        }

        private void OpenPauseMenu()
        {
            CloseAllModals();
            if (pausePanel == null) return;
            SoundManager.Instance?.Play("ui_click", 0.5f);
            if (pauseMainView != null) pauseMainView.SetActive(true);
            if (pauseOptionsView != null) pauseOptionsView.SetActive(false);
            RefreshAudioUI();
            RefreshPauseDossier();
            if (modalBackdrop != null) modalBackdrop.SetActive(true);
            pausePanel.SetActive(true);
            AnimateModalPopIn(pausePanel);
            SyncModalLock();
        }

        private void ClosePauseMenu()
        {
            SoundManager.Instance?.Play("ui_click", 0.45f);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (modalBackdrop != null) modalBackdrop.SetActive(false);
            SyncModalLock();
        }

        private void AnimateModalPopIn(GameObject modalObj)
        {
            if (modalObj == null) return;
            RectTransform rt = modalObj.GetComponent<RectTransform>();
            if (rt != null) StartCoroutine(PopInRoutine(rt));
        }

        private static IEnumerator PopInRoutine(RectTransform rt)
        {
            const float duration = 0.14f;
            float elapsed = 0f;
            rt.localScale = new Vector3(0.94f, 0.94f, 1f);
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                float s = Mathf.Lerp(0.94f, 1f, eased);
                rt.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            rt.localScale = Vector3.one;
        }

        private void RefreshAudioUI()
        {
            bool muted = SoundManager.Instance != null && SoundManager.Instance.IsMuted;
            float vol = SoundManager.Instance != null ? SoundManager.Instance.MasterVolume : 1f;
            int pct = Mathf.RoundToInt(vol * 100f);

            if (muteBtnText != null)
                muteBtnText.text = muted ? "SFX: OFF" : "SFX: ON";

            if (optionsMuteBtnText != null)
                optionsMuteBtnText.text = muted ? "Audio: MUTED" : "Audio: ON";

            if (optionsVolumeText != null)
                optionsVolumeText.text = $"Volume:  <b>{(muted ? 0 : pct)}%</b>";

            if (optionsVolumeSlider != null)
                optionsVolumeSlider.SetValueWithoutNotify(vol);

            if (optionsFullscreenBtnText != null)
                optionsFullscreenBtnText.text = Screen.fullScreen ? "Display: Fullscreen" : "Display: Windowed";
        }

        private static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void RefreshPauseDossier()
        {
            var pm = PersonalizationManager.Instance;
            int evCount = EvidenceManager.Instance != null ? EvidenceManager.Instance.CollectedEvidence.Count : 0;
            MissionState st = MissionManager.Instance != null ? MissionManager.Instance.CurrentState : MissionState.Prologue_SupervisorCall;

            if (pauseRankText != null && pm != null)
            {
                string rank = pm.GetInvestigatorRank(evCount);
                pauseRankText.text =
                    $"<b>RANK:  <color=#7D2629>{rank.ToUpper()}</color></b>\n" +
                    $"Mode: <b>{pm.GetAssignedModeLabel()}</b>  •  Focus: <b>{pm.GetWeakestDomain()}</b>";
            }

            int stepNum = st switch
            {
                MissionState.Prologue_SupervisorCall => 1,
                MissionState.Prologue_FindManual => 2,
                MissionState.Alex_AnswerFirstCall => 3,
                MissionState.Alex_FindHiddenLetter => 4,
                MissionState.Alex_InspectBlogAndPolicy => 5,
                MissionState.Alex_ObjectionCall => 6,
                MissionState.Alex_PinPolicyClause => 7,
                _ => 7
            };
            bool case01Done = st >= MissionState.Sofia_Ready;
            float pct = case01Done ? 1f : (stepNum - 1) / 7f;
            int pctInt = Mathf.RoundToInt(pct * 100f);

            if (pauseProgressLabel != null)
            {
                pauseProgressLabel.text = case01Done
                    ? "<b>CASE 01: ALEX</b>  •  <color=#2B5E4D>100% ✓</color>"
                    : $"<b>CASE 01: ALEX</b>  •  Step {stepNum}/7 ({pctInt}%)";
            }

            if (pauseProgressFill != null)
            {
                RectTransform fillRt = pauseProgressFill.GetComponent<RectTransform>();
                fillRt.anchorMin = Vector2.zero;
                fillRt.anchorMax = new Vector2(Mathf.Max(0.05f, pct), 1f);
                fillRt.offsetMin = Vector2.zero;
                fillRt.offsetMax = Vector2.zero;
                pauseProgressFill.color = case01Done ? AccentSage : AccentBurgundy;
            }

            if (pauseNextObjectiveText != null)
            {
                string nextTip = st switch
                {
                    MissionState.Prologue_SupervisorCall => "Answer the ringing phone.",
                    MissionState.Prologue_FindManual => "Turn Right (►) → Open the Manual on the Bookcase.",
                    MissionState.Alex_AnswerFirstCall => "Answer Alex Moretti's call.",
                    MissionState.Alex_FindHiddenLetter => "Check Mail Log → Turn Left (◄) to Drawer B-02.",
                    MissionState.Alex_InspectBlogAndPolicy => "Click the 3D Monitor → Use HealthBrowser on screen.",
                    MissionState.Alex_ObjectionCall => "Call Alex back & hit OBJECTION!",
                    MissionState.Alex_PinPolicyClause => "Turn Left (◄) to Cork Board → Pin Section 4.2.",
                    MissionState.Sofia_Ready => "Answer Sofia's call (Case 02).",
                    _ => MissionManager.GetObjectiveText(st)
                };
                pauseNextObjectiveText.text = $"<b>NEXT:</b>  {nextTip}";
            }

            if (pauseCluesAndDhliText != null && pm != null)
            {
                string rHex = ColorUtility.ToHtmlStringRGB(GetDhliColor(pm.reliabilityScore));
                string pHex = ColorUtility.ToHtmlStringRGB(GetDhliColor(pm.privacyScore));
                string nHex = ColorUtility.ToHtmlStringRGB(GetDhliColor(pm.navigationScore));
                pauseCluesAndDhliText.text =
                    $"<b>CLUES:</b>  {evCount} / 4\n\n" +
                    $"<color=#{rHex}>● Reliability: {pm.reliabilityScore}</color>   " +
                    $"<color=#{pHex}>● Privacy: {pm.privacyScore}</color>   " +
                    $"<color=#{nHex}>● Navigation: {pm.navigationScore}</color>";
            }
        }

        private void HandleMissionChanged(MissionState state, string _)
        {
            if (objectiveText == null) return;
            objectiveText.text = state switch
            {
                MissionState.Prologue_SupervisorCall => "<color=#80D8FF>[1/7]</color>  Answer the ringing Office Phone",
                MissionState.Prologue_FindManual => "<color=#80D8FF>[2/7]</color>  Turn Right (►) → Open MANUAL on Bookcase",
                MissionState.Alex_AnswerFirstCall => "<color=#FFD54F>[3/7]</color>  Case 01: Answer Alex Moretti's call",
                MissionState.Alex_FindHiddenLetter => "<color=#FFD54F>[4/7]</color>  Check Mail Log → Turn Left (◄) to Drawer B-02",
                MissionState.Alex_InspectBlogAndPolicy => "<color=#FFD54F>[5/7]</color>  Click 3D Monitor → Open HealthBrowser",
                MissionState.Alex_ObjectionCall => "<color=#FF8A80>[6/7]</color>  Call Alex back & trigger OBJECTION!",
                MissionState.Alex_PinPolicyClause => "<color=#FFD54F>[7/7]</color>  Turn Left (◄) to Cork Board → Pin Section 4.2",
                MissionState.Sofia_Ready => "<color=#69F0AE>[CASE 01 ✓]</color>  Answer Sofia (WP4) or inspect DHLI Profile",
                _ => MissionManager.GetObjectiveText(state)
            };
        }

        private void ShowToast(string msg)
        {
            if (string.IsNullOrEmpty(msg) || toastBanner == null) return;
            toastText.text = msg;
            toastBanner.transform.SetAsLastSibling();
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

            if (dhliSubtitleText != null)
            {
                dhliSubtitleText.text = pm.onboardingSummary;
            }

            UpdateSkillBar(relBarFill, relBarLabel, "Evaluating Reliability", pm.reliabilityScore);
            UpdateSkillBar(privBarFill, privBarLabel, "Protecting Privacy", pm.privacyScore);
            UpdateSkillBar(navBarFill, navBarLabel, "Digital Navigation", pm.navigationScore);

            dhliDetailText.text =
                $"Mistakes:  <b>R:{pm.reliabilityMistakes}</b>  |  <b>P:{pm.privacyMistakes}</b>  |  <b>N:{pm.navigationMistakes}</b>\n" +
                $"Focus area: <b>{pm.GetWeakestDomain()}</b>\n" +
                $"<i>Hints tighten automatically when a domain is Low (&lt;45).</i>";
        }

        private static void UpdateSkillBar(Image fillImg, Text label, string domainTitle, int score)
        {
            if (fillImg == null || label == null) return;
            float norm = Mathf.Clamp01(score / 100f);
            RectTransform rt = fillImg.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = new Vector2(Mathf.Max(0.03f, norm), 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Color barColor = GetDhliColor(score);
            fillImg.color = barColor;
            string level = PersonalizationManager.GetLevelLabel(score);
            string hex = ColorUtility.ToHtmlStringRGB(barColor);
            label.text = $"{domainTitle}:  <b>{score}/100</b>  <color=#{hex}>[{level}]</color>";
        }

        private static Color GetDhliColor(int score)
        {
            if (score < 45) return new Color(0.68f, 0.22f, 0.22f, 1f);
            if (score < 70) return new Color(0.68f, 0.48f, 0.14f, 1f);
            return new Color(0.18f, 0.48f, 0.34f, 1f);
        }

        private void RefreshInventoryUI()
        {
            if (inventoryListParent == null || EvidenceManager.Instance == null) return;
            IReadOnlyList<EvidenceItem> items = EvidenceManager.Instance.CollectedEvidence;
            if (evidenceCountBtnText != null) evidenceCountBtnText.text = $"Evidence ({items.Count})";

            foreach (Transform child in inventoryListParent) Destroy(child.gameObject);

            if (items.Count == 0)
            {
                CreateText(inventoryListParent, "Empty", "No evidence collected yet.", 13, InkMuted, TextAnchor.UpperLeft, false);
                return;
            }

            foreach (EvidenceItem item in items)
            {
                EvidenceItem captured = item;
                Button btn = CreateButton(
                    inventoryListParent,
                    $"Inv_{item.id}",
                    $"•  {item.title}",
                    PaperBtn,
                    34f,
                    () =>
                    {
                        SoundManager.Instance?.Play("ui_click", 0.5f);
                        evidenceDetailText.text = $"<b>{captured.title}</b> ({captured.id})\nDomain: {captured.dhliDomain}\nSource: {captured.source}\n\n{captured.summary}";
                    },
                    InkPrimary
                );
                btn.GetComponentInChildren<Text>().alignment = TextAnchor.MiddleLeft;
            }
        }

        private void HandlePhoneCallRequested(string speaker, string portraitKey, string body, bool allowObjection)
        {
            CloseAllModals();
            phonePanel.SetActive(true);
            AnimateModalPopIn(phonePanel);
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

            StopActiveSpeech();
            typewriterRoutine = StartCoroutine(TypeTextWithVoiceRoutine(phoneBodyText, body, portraitKey));

            foreach (Transform child in objectionButtonContainer) Destroy(child.gameObject);

            if (allowObjection && EvidenceManager.Instance != null)
            {
                // Stage 1: Ace Attorney-style Press / Interrogate row
                Text pressHdr = CreateText(objectionButtonContainer, "PressHdr", "<b>INTERROGATE ALEX (PRESS FOR DETAILS):</b>", 12, AccentOchre, TextAnchor.MiddleLeft, false);
                pressHdr.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 18f);

                GameObject pressRow = new GameObject("PressRow");
                pressRow.transform.SetParent(objectionButtonContainer, false);
                pressRow.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 32f);
                HorizontalLayoutGroup pLay = pressRow.AddComponent<HorizontalLayoutGroup>();
                pLay.spacing = 8; pLay.childControlWidth = true; pLay.childForceExpandWidth = true;
                pLay.childControlHeight = true; pLay.childForceExpandHeight = true;

                CreateButton(pressRow.transform, "PressAuthorBtn", "Ask: Who wrote the blog?", PaperBtn, 30f, () =>
                {
                    SoundManager.Instance?.Play("ui_click", 0.55f);
                    string hintLine = DialogueManager.Instance != null
                        ? DialogueManager.Instance.GetLine("alex", "hint")
                        : "I don't know the author, but they're the only one saying things are true. Big sites just echo what the companies tell them to.";
                    StopActiveSpeech();
                    typewriterRoutine = StartCoroutine(TypeTextWithVoiceRoutine(phoneBodyText, hintLine, "alex"));
                    phoneFeedbackText.color = AccentOchre;
                    phoneFeedbackText.text = "<b>INVESTIGATOR NOTE:</b> Alex cannot name the author or sources. Present your verified Audit evidence to challenge the blog!";
                }, InkPrimary);

                CreateButton(pressRow.transform, "PressDistrustBtn", "Ask: Why distrust official docs?", PaperBtn, 30f, () =>
                {
                    SoundManager.Instance?.Play("ui_click", 0.55f);
                    const string skepticLine = "Big health sites just repeat whatever companies pay them to say! Why trust official pages when this whistleblower blog already exposed them?";
                    StopActiveSpeech();
                    typewriterRoutine = StartCoroutine(TypeTextWithVoiceRoutine(phoneBodyText, skepticLine, "alex"));
                    phoneFeedbackText.color = AccentOchre;
                    phoneFeedbackText.text = "<b>INVESTIGATOR NOTE:</b> Confirmation bias detected! Don't argue blindly — slam <b>OBJECTION!</b> with verifiable proof.";
                }, InkPrimary);

                // Stage 2: Present Evidence (OBJECTION!)
                Text objHdr = CreateText(objectionButtonContainer, "ObjHdr", "<b>PRESENT EVIDENCE:</b>", 12, AccentBurgundy, TextAnchor.MiddleLeft, false);
                objHdr.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 18f);

                foreach (EvidenceItem item in EvidenceManager.Instance.CollectedEvidence)
                {
                    EvidenceItem captured = item;
                    CreateButton(
                        objectionButtonContainer,
                        $"Present_{item.id}",
                        $"OBJECTION!  {item.title}",
                        AccentBurgundy,
                        32f,
                        () => OnPresentEvidenceClicked(captured.id),
                        PaperWhite
                    );
                }
            }
        }

        /// <summary>
        /// Comfortable reading speed with gentle character voice blips + click-to-skip support.
        /// </summary>
        private IEnumerator TypeTextWithVoiceRoutine(Text target, string fullText, string voiceKey)
        {
            currentFullDialogue = fullText;
            isTypingDialogue = (target == phoneBodyText);
            if (isTypingDialogue && phoneSkipHintText != null) phoneSkipHintText.gameObject.SetActive(true);

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

            isTypingDialogue = false;
            if (phoneSkipHintText != null) phoneSkipHintText.gameObject.SetActive(false);
        }

        private void CompleteTypewriterImmediately()
        {
            if (!isTypingDialogue) return;
            StopActiveSpeech();
            if (phoneBodyText != null) phoneBodyText.text = currentFullDialogue;
            SoundManager.Instance?.Play("ui_click", 0.35f);
        }

        private void OnPresentEvidenceClicked(string evidenceId)
        {
            if (MissionManager.Instance == null) return;
            bool success = MissionManager.Instance.TrySubmitObjection(evidenceId, out string response, out string hint);
            if (success)
            {
                StopActiveSpeech();
                typewriterRoutine = StartCoroutine(TypeTextWithVoiceRoutine(phoneBodyText, response, "alex"));
                phoneFeedbackText.color = AccentSage;
                phoneFeedbackText.text = "<b>OBJECTION ACCEPTED!</b> Claim challenged with verifiable evidence.";
                foreach (Transform child in objectionButtonContainer) Destroy(child.gameObject);
            }
            else
            {
                phoneFeedbackText.color = AccentBurgundy;
                phoneFeedbackText.text = $"{response}\n\n{hint}";
            }
        }

        private void OpenCase01CompleteModal()
        {
            CloseAllModals();
            if (caseCompletePanel == null) return;

            if (caseCompletePortrait != null && portraits.TryGetValue("alex", out Texture2D alexTex))
            {
                caseCompletePortrait.texture = alexTex;
            }

            if (caseCompleteSummaryText != null && PersonalizationManager.Instance != null)
            {
                var pm = PersonalizationManager.Instance;
                string relHex = ColorUtility.ToHtmlStringRGB(GetDhliColor(pm.reliabilityScore));
                string privHex = ColorUtility.ToHtmlStringRGB(GetDhliColor(pm.privacyScore));
                string navHex = ColorUtility.ToHtmlStringRGB(GetDhliColor(pm.navigationScore));
                string rank = pm.GetInvestigatorRank(EvidenceManager.Instance != null ? EvidenceManager.Instance.CollectedEvidence.Count : 4);

                caseCompleteSummaryText.text =
                    "<b>Alex Moretti's Takeaway:</b>\n" +
                    "<i>\"Okay... maybe a blog agreeing with my suspicions isn't proof on its own. " +
                    "Still, I'm glad you pinned Section 4.2 — we need to know who those third-party partners really are!\"</i>\n\n" +
                    $"<b>INVESTIGATOR RANK:  <color=#7D2629>{rank.ToUpper()}</color></b>\n" +
                    $"<color=#5A4F42>DHLI Comparison — Alex's Reliability: <b>4/12 (Low)</b>  vs  Yours: <b><color=#{relHex}>{pm.reliabilityScore}/100</color></b></color>\n" +
                    $"•  Evaluating Reliability:  <b><color=#{relHex}>{pm.reliabilityScore}/100 [{PersonalizationManager.GetLevelLabel(pm.reliabilityScore)}]</color></b>\n" +
                    $"•  Protecting Privacy:      <b><color=#{privHex}>{pm.privacyScore}/100 [{PersonalizationManager.GetLevelLabel(pm.privacyScore)}]</color></b>\n" +
                    $"•  Digital Navigation:      <b><color=#{navHex}>{pm.navigationScore}/100 [{PersonalizationManager.GetLevelLabel(pm.navigationScore)}]</color></b>\n\n" +
                    $"Assigned Mode: <b>{pm.GetAssignedModeLabel()}</b>  •  Focus: <b>{pm.GetWeakestDomain()}</b>\n" +
                    "<color=#2B5E4D><i>The office phone is ringing again — Sofia Romano (Case 02, WP4) needs your help!</i></color>";
            }

            if (modalBackdrop != null) modalBackdrop.SetActive(true);
            caseCompletePanel.SetActive(true);
            AnimateModalPopIn(caseCompletePanel);
            SyncModalLock();
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

        // === INTERACTIVE TWO-PAGE OPEN BOOK READER (WP2) ===
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
                            stickyNote = "Supervisor's Sticky Note:\n'Welcome aboard, rookie! Alex is already calling!\nP.S. Check \"101 Viral Health Myths\" & \"Digital Privacy 101\" right next to me on this bookcase for +5 DHLI & PC Audit tips!'"
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
                            stickyNote = "★ +5 RELIABILITY BONUS — Tip pinned to PC Desk!\n'Watch for promo codes (hidden ads!) and check WHOIS & EU Registry!'"
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
                            stickyNote = "★ +5 PRIVACY BONUS — Tip pinned to PC Desk!\n'Check Policy v4.2 dates & Section 4.2 — vague sharing ≠ proven data sale!'"
                        },
                        new BookSpread {
                            leftPage = "<b>READING PRIVACY POLICIES</b>\n\nWatch for vague phrases like:\n<i>'Some information may be shared with third-party partners.'</i>\n\nVague wording is a real privacy red flag—even if it doesn't automatically prove illegal data sales.",
                            rightPage = "<b>OFFICIAL HEALTH PORTALS</b>\n\nAlways verify hospital or public-health URLs against a trusted directory before logging in or trusting 'Official Approval' screenshots.",
                            stickyNote = ""
                        }
                    };
                    break;

                default:
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
            AnimateModalPopIn(bookModalPanel);
            SyncModalLock();
            RefreshBookSpread();

            // Bookcase Research Bonus: reading optional reference books awards +5 DHLI & unlocks a PC Audit sticky note!
            if ((bookKey == "myths" || bookKey == "privacy") && bonusBooksRead.Add(bookKey))
            {
                DhliDomain dom = bookKey == "myths" ? DhliDomain.Reliability : DhliDomain.Privacy;
                PersonalizationManager.Instance?.RecordCorrectAction(dom, 5);
                WorldSpaceMonitorOS.Instance?.UnlockBookcaseBonusNote(bookKey);
                ShowToast(bookKey == "myths"
                    ? "★ Bookcase Research Bonus (+5 Reliability)! Detective Sticky Note added to your PC Audit desk."
                    : "★ Bookcase Research Bonus (+5 Privacy)! Policy 4.2 Sticky Note added to your PC Audit desk.");
            }
        }

        private static void EnhanceAlexEnvelopeClickability()
        {
            GameObject env = GameObject.Find("AlexHiddenEnvelope");
            if (env == null) return;

            // Lift the yellow envelope slightly higher & tilt it toward the player so it peeks clearly out of Drawer B-02
            env.transform.localPosition = new Vector3(0.08f, -0.045f, 0f);
            env.transform.localRotation = Quaternion.Euler(0f, 8f, -14f);

            BoxCollider col = env.GetComponent<BoxCollider>();
            if (col != null)
            {
                col.size = new Vector3(0.48f, 0.28f, 0.52f);
                col.center = new Vector3(0.04f, 0.08f, 0f);
            }
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

        private void SyncDeskClipboardVisualToLegalPad()
        {
            if (ClipboardPickupAnimator.Instance == null) return;
            deskClipboard3D = ClipboardPickupAnimator.Instance.gameObject;
            Transform root = deskClipboard3D.transform;

            Transform paperMesh = root.Find("LogPaper");
            if (paperMesh != null)
            {
                Renderer rend = paperMesh.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.color = new Color(0.965f, 0.915f, 0.675f, 1f);
                }
            }

            Transform worldCanvasTf = root.Find("ClipboardPaperTextCanvas");
            if (worldCanvasTf == null) return;

            for (int i = worldCanvasTf.childCount - 1; i >= 0; i--)
            {
                Destroy(worldCanvasTf.GetChild(i).gameObject);
            }

            GameObject padBg = new GameObject("DeskLegalPadBg");
            padBg.transform.SetParent(worldCanvasTf, false);
            SetFill(padBg.AddComponent<RectTransform>(), 0f, 0f);
            RawImage raw = padBg.AddComponent<RawImage>();
            raw.texture = CreateLinedLegalPadTexture();
            raw.raycastTarget = false;

            Text hdr = CreateText(padBg.transform, "DeskPadHdr", "Mail Delivery Log — Archive", 42, new Color(0.14f, 0.20f, 0.36f), TextAnchor.MiddleLeft, false);
            if (handwritingFont != null) hdr.font = handwritingFont;
            hdr.fontStyle = FontStyle.Bold;
            RectTransform hRt = hdr.GetComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0f, 1f); hRt.anchorMax = new Vector2(1f, 1f); hRt.pivot = new Vector2(0.5f, 1f);
            hRt.offsetMin = new Vector2(96f, -108f); hRt.offsetMax = new Vector2(-24f, -12f);

            Text body = CreateText(padBg.transform, "DeskPadBody",
                "Filing Cabinet (left wall):\n\n" +
                "•  Drawer B-01 (Top)\n" +
                "    old 2023 office receipts\n\n" +
                "<color=#961C1C>★  Drawer B-02 (Middle) — URGENT!</color>\n" +
                "<color=#961C1C>    A. Moretti — Yellow Envelope</color>\n" +
                "    (VitaPulse blog allegations)\n\n" +
                "•  Drawer B-03 (Bottom)\n" +
                "    blank intake forms",
                36, new Color(0.13f, 0.19f, 0.34f), TextAnchor.UpperLeft, false);
            if (handwritingFont != null) body.font = handwritingFont;
            body.lineSpacing = 1.15f;
            RectTransform bRt = body.GetComponent<RectTransform>();
            bRt.anchorMin = Vector2.zero; bRt.anchorMax = Vector2.one;
            bRt.offsetMin = new Vector2(96f, 24f); bRt.offsetMax = new Vector2(-24f, -122f);
        }

        private void OpenMailLogModal()
        {
            CloseAllModals();
            if (deskClipboard3D != null) deskClipboard3D.SetActive(false);
            clipboardModalPanel.SetActive(true);
            StartCoroutine(LiftClipboardFromBottomRoutine(clipboardModalPanel.GetComponent<RectTransform>()));
            SyncModalLock();
        }

        private static IEnumerator LiftClipboardFromBottomRoutine(RectTransform rt)
        {
            if (rt == null) yield break;
            const float duration = 0.24f;
            float elapsed = 0f;
            Vector2 startPos = new Vector2(0f, -520f);
            Vector2 endPos = Vector2.zero;
            Quaternion startRot = Quaternion.Euler(0f, 0f, -7f);
            Quaternion endRot = Quaternion.Euler(0f, 0f, -1.2f);
            rt.anchoredPosition = startPos;
            rt.localRotation = startRot;
            rt.localScale = Vector3.one;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                rt.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);
                rt.localRotation = Quaternion.Slerp(startRot, endRot, eased);
                yield return null;
            }
            rt.anchoredPosition = endPos;
            rt.localRotation = endRot;
        }

        private static Texture2D CreateLinedLegalPadTexture()
        {
            const int w = 512, h = 640;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color baseYellow = new Color(0.965f, 0.915f, 0.675f, 1f); // Classic pale yellow legal pad
            Color ruleBlue = new Color(0.56f, 0.72f, 0.84f, 0.65f);   // Soft blue horizontal lines
            Color marginRed = new Color(0.84f, 0.36f, 0.36f, 0.58f);  // Classic red vertical margin rule
            Color topBand = new Color(0.92f, 0.86f, 0.58f, 1f);       // Slightly warmer top header area

            Color[] px = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                bool isTopHeader = y > h - 76;
                bool isBlueLine = !isTopHeader && (y % 30 == 0) && y > 18;
                bool isDoubleHeaderLine = (y == h - 76 || y == h - 80);

                for (int x = 0; x < w; x++)
                {
                    // Subtle warm paper fiber grain
                    float grain = (((x * 37 + y * 17) % 19) - 9) * 0.0014f;
                    Color c = isTopHeader ? topBand : baseYellow;
                    c = new Color(c.r + grain, c.g + grain, c.b + grain, 1f);

                    if (isBlueLine || isDoubleHeaderLine)
                    {
                        c = Color.Lerp(c, ruleBlue, ruleBlue.a);
                    }
                    if (x == 62 || x == 66)
                    {
                        c = Color.Lerp(c, marginRed, marginRed.a);
                    }
                    px[y * w + x] = c;
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }

        private void OpenAlexLetterModal()
        {
            CloseAllModals();
            if (alexLetterModalPanel == null) return;
            SoundManager.Instance?.Play("paper_rustle", 0.7f);
            alexLetterModalPanel.SetActive(true);
            StartCoroutine(LiftClipboardFromBottomRoutine(alexLetterModalPanel.GetComponent<RectTransform>()));
            SyncModalLock();
        }

        private void OpenDocModal(string title, string body)
        {
            CloseAllModals();
            docModalTitle.text = title;
            docModalBody.text = body;
            docModalPanel.SetActive(true);
            AnimateModalPopIn(docModalPanel);
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
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // 1. Compact Top-Left Mission Pill
            RectTransform objPill = CreatePanel(canvasGo.transform, "ObjectivePill", new Color(0.08f, 0.10f, 0.14f, 0.88f), raycast: false);
            AddSubtleBorder(objPill.gameObject, new Color(0.85f, 0.72f, 0.42f, 0.35f));
            objPill.anchorMin = new Vector2(0f, 1f); objPill.anchorMax = new Vector2(0f, 1f); objPill.pivot = new Vector2(0f, 1f);
            objPill.sizeDelta = new Vector2(560f, 38f); objPill.anchoredPosition = new Vector2(14f, -12f);
            objectiveText = CreateText(objPill, "ObjText", "Loading...", 14, new Color(1f, 0.93f, 0.55f), TextAnchor.MiddleLeft, false);
            objectiveText.fontStyle = FontStyle.Bold;
            SetFill(objectiveText.GetComponent<RectTransform>(), 14, 4);

            // 2. Top-Right Compact Bar (Evidence + DHLI Profile + SFX Mute + Menu)
            RectTransform topRight = CreatePanel(canvasGo.transform, "TopRightBtns", new Color(0, 0, 0, 0), raycast: false, rounded: false);
            topRight.anchorMin = new Vector2(1f, 1f); topRight.anchorMax = new Vector2(1f, 1f); topRight.pivot = new Vector2(1f, 1f);
            topRight.sizeDelta = new Vector2(436f, 38f); topRight.anchoredPosition = new Vector2(-14f, -12f);
            HorizontalLayoutGroup trLay = topRight.gameObject.AddComponent<HorizontalLayoutGroup>();
            trLay.spacing = 7; trLay.childControlWidth = true; trLay.childForceExpandWidth = true;
            trLay.childControlHeight = true; trLay.childForceExpandHeight = true;

            Button invBtn = CreateButton(topRight, "ToggleInvBtn", "Evidence (0)", new Color(0.14f, 0.28f, 0.42f, 0.94f), 36f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                bool next = !inventoryDrawer.activeSelf;
                dhliDrawer.SetActive(false);
                inventoryDrawer.SetActive(next);
            }, PaperWhite);
            invBtn.GetComponent<LayoutElement>().flexibleWidth = 1.3f;
            evidenceCountBtnText = invBtn.GetComponentInChildren<Text>();

            Button dhliBtn = CreateButton(topRight, "ToggleDhliBtn", "DHLI Profile", AccentSage, 36f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                bool next = !dhliDrawer.activeSelf;
                inventoryDrawer.SetActive(false);
                dhliDrawer.SetActive(next);
                RefreshDhliDrawer();
            }, PaperWhite);
            dhliBtn.GetComponent<LayoutElement>().flexibleWidth = 1.25f;

            Button muteBtn = CreateButton(topRight, "ToggleMuteBtn", "SFX: ON", new Color(0.22f, 0.24f, 0.29f, 0.94f), 36f, () =>
            {
                SoundManager.Instance?.ToggleMute();
                if (SoundManager.Instance != null && !SoundManager.Instance.IsMuted)
                    SoundManager.Instance.Play("ui_click", 0.5f);
            }, PaperWhite);
            muteBtn.GetComponent<LayoutElement>().flexibleWidth = 0.95f;
            muteBtnText = muteBtn.GetComponentInChildren<Text>();

            Button menuBtn = CreateButton(topRight, "PauseMenuBtn", "Menu", AccentBurgundy, 36f, () =>
            {
                if (pausePanel != null && pausePanel.activeSelf) ClosePauseMenu();
                else OpenPauseMenu();
            }, PaperWhite);
            menuBtn.GetComponent<LayoutElement>().flexibleWidth = 0.85f;

            // 3. Effortless Left & Right Screen Turn Arrows (◄ / ►) + Back to Desk (WP2)
            Button leftArrow = CreateButton(canvasGo.transform, "TurnLeftArrow", "◄", new Color(0.08f, 0.10f, 0.14f, 0.70f), 68f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.4f);
                CameraViewpointController.Instance?.TurnLeft();
            }, PaperWhite);
            RectTransform laRect = leftArrow.GetComponent<RectTransform>();
            laRect.anchorMin = new Vector2(0f, 0.5f); laRect.anchorMax = new Vector2(0f, 0.5f); laRect.pivot = new Vector2(0f, 0.5f);
            laRect.sizeDelta = new Vector2(48f, 76f); laRect.anchoredPosition = new Vector2(12f, 0f);
            Text laTxt = leftArrow.GetComponentInChildren<Text>();
            laTxt.resizeTextForBestFit = false; laTxt.fontSize = 26;
            leftTurnBtnObj = leftArrow.gameObject;

            Button rightArrow = CreateButton(canvasGo.transform, "TurnRightArrow", "►", new Color(0.08f, 0.10f, 0.14f, 0.70f), 68f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.4f);
                CameraViewpointController.Instance?.TurnRight();
            }, PaperWhite);
            RectTransform raRect = rightArrow.GetComponent<RectTransform>();
            raRect.anchorMin = new Vector2(1f, 0.5f); raRect.anchorMax = new Vector2(1f, 0.5f); raRect.pivot = new Vector2(1f, 0.5f);
            raRect.sizeDelta = new Vector2(48f, 76f); raRect.anchoredPosition = new Vector2(-12f, 0f);
            Text raTxt = rightArrow.GetComponentInChildren<Text>();
            raTxt.resizeTextForBestFit = false; raTxt.fontSize = 26;
            rightTurnBtnObj = rightArrow.gameObject;

            Button backBtn = CreateButton(canvasGo.transform, "BackToDeskBtn", "↓ Back to Desk (Esc)", new Color(0.12f, 0.18f, 0.26f, 0.90f), 34f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.45f);
                CameraViewpointController.Instance?.MoveToViewpoint("Desk");
            }, PaperWhite);
            RectTransform bdRect = backBtn.GetComponent<RectTransform>();
            bdRect.anchorMin = new Vector2(0.5f, 0f); bdRect.anchorMax = new Vector2(0.5f, 0f); bdRect.pivot = new Vector2(0.5f, 0f);
            bdRect.sizeDelta = new Vector2(195f, 34f); bdRect.anchoredPosition = new Vector2(0f, 14f);
            backToDeskBtnObj = backBtn.gameObject;

            // 4. Toast & Hover Pill
            RectTransform toast = CreatePanel(canvasGo.transform, "ToastBanner", new Color(0.09f, 0.12f, 0.16f, 0.90f), raycast: false);
            toast.anchorMin = new Vector2(0.22f, 1f); toast.anchorMax = new Vector2(0.78f, 1f); toast.pivot = new Vector2(0.5f, 1f);
            toast.sizeDelta = new Vector2(0f, 34f); toast.anchoredPosition = new Vector2(0f, -58f);
            toastText = CreateText(toast, "ToastText", "", 13, PaperWhite, TextAnchor.MiddleCenter, false);
            SetFill(toastText.GetComponent<RectTransform>(), 12, 4);
            toastBanner = toast.gameObject; toastBanner.SetActive(false);

            RectTransform hover = CreatePanel(canvasGo.transform, "HoverPill", new Color(0.07f, 0.09f, 0.12f, 0.86f), raycast: false);
            hover.anchorMin = new Vector2(0.32f, 0f); hover.anchorMax = new Vector2(0.68f, 0f); hover.pivot = new Vector2(0.5f, 0f);
            hover.sizeDelta = new Vector2(0f, 32f); hover.anchoredPosition = new Vector2(0f, 56f);
            hoverLabelText = CreateText(hover, "HoverText", "", 14, new Color(1f, 0.94f, 0.56f), TextAnchor.MiddleCenter, false);
            hoverLabelText.fontStyle = FontStyle.Bold;
            SetFill(hoverLabelText.GetComponent<RectTransform>(), 10, 2);
            hoverPill = hover.gameObject; hoverPill.SetActive(false);

            // 5. Drawers
            BuildEvidenceDrawer(canvasGo.transform);
            BuildDhliDrawer(canvasGo.transform);

            // 6. Warm Espresso Backdrop Dimmer
            RectTransform backdropRt = CreatePanel(canvasGo.transform, "ModalBackdrop", new Color(0.07f, 0.06f, 0.05f, 0.72f), raycast: true, rounded: false);
            SetFill(backdropRt, 0f, 0f);
            modalBackdrop = backdropRt.gameObject;
            modalBackdrop.SetActive(false);

            // 7. Modals (WP3 + WP2 unified!)
            BuildWelcomeModal(canvasGo.transform);
            BuildOnboardingModal(canvasGo.transform);
            BuildCaseCompleteModal(canvasGo.transform);
            BuildPhoneModal(canvasGo.transform);
            BuildOpenBookModal(canvasGo.transform);
            BuildPrinterJamModal(canvasGo.transform);
            BuildClipboardModal(canvasGo.transform);
            BuildAlexLetterModal(canvasGo.transform);
            BuildDocModal(canvasGo.transform);
            BuildPauseModal(canvasGo.transform);
            BuildObjectionSplash(canvasGo.transform);
        }

        private void BuildOpenBookModal(Transform parent)
        {
            RectTransform cover = CreatePanel(parent, "OpenBookModal", new Color(0.14f, 0.22f, 0.36f, 0.99f), raycast: true);
            cover.anchorMin = new Vector2(0.11f, 0.13f); cover.anchorMax = new Vector2(0.89f, 0.87f);
            cover.offsetMin = Vector2.zero; cover.offsetMax = Vector2.zero;
            bookCoverBorder = cover.GetComponent<Image>();

            bookTitleBanner = CreateText(cover, "BookTitle", "BOOK", 18, new Color(1f, 0.90f, 0.55f), TextAnchor.MiddleCenter, false);
            bookTitleBanner.fontStyle = FontStyle.Bold;
            RectTransform btRect = bookTitleBanner.GetComponent<RectTransform>();
            btRect.anchorMin = new Vector2(0f, 1f); btRect.anchorMax = new Vector2(1f, 1f); btRect.pivot = new Vector2(0.5f, 1f);
            btRect.sizeDelta = new Vector2(0f, 34f); btRect.anchoredPosition = new Vector2(0f, -4f);

            GameObject pagesRow = new GameObject("PagesRow");
            pagesRow.transform.SetParent(cover, false);
            RectTransform prRect = pagesRow.AddComponent<RectTransform>();
            prRect.anchorMin = Vector2.zero; prRect.anchorMax = Vector2.one;
            prRect.offsetMin = new Vector2(16f, 54f); prRect.offsetMax = new Vector2(-16f, -40f);
            HorizontalLayoutGroup pLay = pagesRow.AddComponent<HorizontalLayoutGroup>();
            pLay.spacing = 6; pLay.childControlWidth = true; pLay.childForceExpandWidth = true;

            RectTransform leftPage = CreatePanel(pagesRow.transform, "LeftPage", PaperBase, raycast: false);
            bookLeftPageText = CreateText(leftPage, "LeftTxt", "", 15, InkPrimary, TextAnchor.UpperLeft, false);
            SetFill(bookLeftPageText.GetComponent<RectTransform>(), 24f, 22f);

            RectTransform rightPage = CreatePanel(pagesRow.transform, "RightPage", PaperBase, raycast: false);
            bookRightPageText = CreateText(rightPage, "RightTxt", "", 15, InkPrimary, TextAnchor.UpperLeft, false);
            SetFill(bookRightPageText.GetComponent<RectTransform>(), 24f, 22f);

            RectTransform sticky = CreatePanel(rightPage, "StickyNote", new Color(0.95f, 0.88f, 0.45f, 1f), raycast: false);
            sticky.anchorMin = new Vector2(1f, 0f); sticky.anchorMax = new Vector2(1f, 0f); sticky.pivot = new Vector2(1f, 0f);
            sticky.sizeDelta = new Vector2(280f, 108f);
            sticky.anchoredPosition = new Vector2(-18f, 16f);
            sticky.localRotation = Quaternion.Euler(0f, 0f, -3.5f);
            stickyNoteText = CreateText(sticky, "StickyTxt", "", 13, new Color(0.22f, 0.18f, 0.08f), TextAnchor.UpperLeft, false);
            stickyNoteText.fontStyle = FontStyle.Italic;
            SetFill(stickyNoteText.GetComponent<RectTransform>(), 12f, 10f);
            stickyNoteObj = sticky.gameObject;

            GameObject navRow = new GameObject("BookNav");
            navRow.transform.SetParent(cover, false);
            RectTransform nrRect = navRow.AddComponent<RectTransform>();
            nrRect.anchorMin = new Vector2(0f, 0f); nrRect.anchorMax = new Vector2(1f, 0f); nrRect.pivot = new Vector2(0.5f, 0f);
            nrRect.sizeDelta = new Vector2(0f, 44f); nrRect.anchoredPosition = new Vector2(0f, 6f);
            HorizontalLayoutGroup nLay = navRow.AddComponent<HorizontalLayoutGroup>();
            nLay.padding = new RectOffset(20, 20, 4, 4); nLay.spacing = 14;
            nLay.childControlWidth = true; nLay.childForceExpandWidth = true;

            bookPrevBtn = CreateButton(navRow.transform, "PrevPage", "◄ Previous", PaperBtn, 34f, () =>
            {
                if (activeSpreadIndex > 0)
                {
                    activeSpreadIndex--;
                    SoundManager.Instance?.Play("paper_rustle", 0.75f);
                    RefreshBookSpread();
                }
            }, InkPrimary);

            bookPageCounterText = CreateText(navRow.transform, "PageCounter", "Spread 1 / 2", 14, PaperWhite, TextAnchor.MiddleCenter, false);

            bookNextBtn = CreateButton(navRow.transform, "NextPage", "Next ►", PaperBtn, 34f, () =>
            {
                if (activeBookSpreads != null && activeSpreadIndex < activeBookSpreads.Length - 1)
                {
                    activeSpreadIndex++;
                    SoundManager.Instance?.Play("paper_rustle", 0.75f);
                    RefreshBookSpread();
                }
            }, InkPrimary);

            CreateButton(navRow.transform, "CloseBook", "Close", AccentBurgundy, 34f, () =>
            {
                SoundManager.Instance?.Play("paper_rustle", 0.6f);
                bookModalPanel.SetActive(false);
                SyncModalLock();
            }, PaperWhite);

            bookModalPanel = cover.gameObject;
            bookModalPanel.SetActive(false);
        }

        // === INTERACTIVE PRINTER PAPER-JAM MINI-GAME (WP2) ===
        private void OpenPrinterJamModal()
        {
            CloseAllModals();
            printerJamModal.SetActive(true);
            AnimateModalPopIn(printerJamModal);
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
            RectTransform modal = CreatePanel(parent, "PrinterJamModal", new Color(0.11f, 0.12f, 0.14f, 0.98f), raycast: true);
            AddSubtleBorder(modal.gameObject, PaperBorder);
            modal.anchorMin = new Vector2(0.22f, 0.08f); modal.anchorMax = new Vector2(0.78f, 0.92f);
            modal.offsetMin = Vector2.zero; modal.offsetMax = Vector2.zero;

            RectTransform jamPhase = CreatePanel(modal, "JamPhase", new Color(0, 0, 0, 0), raycast: false, rounded: false);
            SetFill(jamPhase, 24f, 20f);

            RectTransform lcd = CreatePanel(jamPhase, "PrinterLCD", AccentBurgundy, raycast: false);
            lcd.anchorMin = new Vector2(0f, 1f); lcd.anchorMax = new Vector2(1f, 1f); lcd.pivot = new Vector2(0.5f, 1f);
            lcd.sizeDelta = new Vector2(0f, 76f); lcd.anchoredPosition = Vector2.zero;
            printerJamStatusText = CreateText(lcd, "LCDTxt", "ERROR E-09: PAPER JAM!", 15, PaperWhite, TextAnchor.MiddleCenter, false);
            printerJamStatusText.fontStyle = FontStyle.Bold;
            SetFill(printerJamStatusText.GetComponent<RectTransform>(), 14f, 8f);

            RectTransform slotBg = CreatePanel(jamPhase, "RollerSlotBg", new Color(0.16f, 0.18f, 0.21f, 1f), raycast: false);
            slotBg.anchorMin = new Vector2(0.10f, 0.24f); slotBg.anchorMax = new Vector2(0.90f, 0.80f);
            slotBg.offsetMin = Vector2.zero; slotBg.offsetMax = Vector2.zero;

            RectTransform slotOpening = CreatePanel(slotBg, "SlotOpening", new Color(0.04f, 0.04f, 0.05f, 1f), raycast: false);
            slotOpening.anchorMin = new Vector2(0.08f, 0.72f); slotOpening.anchorMax = new Vector2(0.92f, 0.84f);
            slotOpening.offsetMin = Vector2.zero; slotOpening.offsetMax = Vector2.zero;

            RectTransform stuckPaper = CreatePanel(slotBg, "StuckPaperSheet", PaperBase, raycast: false);
            stuckPaper.anchorMin = new Vector2(0.5f, 0.76f); stuckPaper.anchorMax = new Vector2(0.5f, 0.76f); stuckPaper.pivot = new Vector2(0.5f, 1f);
            stuckPaper.sizeDelta = new Vector2(220f, 145f);
            stuckPaper.anchoredPosition = new Vector2(0f, -24f);
            stuckPaper.localRotation = Quaternion.Euler(0f, 0f, -7f);
            Text spTxt = CreateText(stuckPaper, "StuckTxt", "=== JAMMED A4 SHEET ===\n[CRUMPLED PHOTOCOPY]\n\n...stuck in rollers...", 13, InkPrimary, TextAnchor.MiddleCenter, false);
            SetFill(spTxt.GetComponent<RectTransform>(), 10f, 10f);
            jammedPaperRect = stuckPaper;

            Button pullBtn = CreateButton(jamPhase, "PullJamBtn", "PULL JAMMED PAPER! (0 / 3)", AccentBurgundy, 46f, OnPullJammedPaperClicked, PaperWhite);
            RectTransform pbRect = pullBtn.GetComponent<RectTransform>();
            pbRect.anchorMin = new Vector2(0.12f, 0f); pbRect.anchorMax = new Vector2(0.88f, 0f); pbRect.pivot = new Vector2(0.5f, 0f);
            pbRect.sizeDelta = new Vector2(0f, 46f); pbRect.anchoredPosition = new Vector2(0f, 52f);
            printerPullBtnText = pullBtn.GetComponentInChildren<Text>();
            printerPullBtnText.fontStyle = FontStyle.Bold;

            Button cancelJamBtn = CreateButton(jamPhase, "CloseJamBtn", "Step Away", PaperBtn, 36f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                printerJamModal.SetActive(false);
                SyncModalLock();
            }, InkPrimary);
            RectTransform cjRect = cancelJamBtn.GetComponent<RectTransform>();
            cjRect.anchorMin = new Vector2(0.32f, 0f); cjRect.anchorMax = new Vector2(0.68f, 0f); cjRect.pivot = new Vector2(0.5f, 0f);
            cjRect.sizeDelta = new Vector2(0f, 36f); cjRect.anchoredPosition = new Vector2(0f, 6f);
            printerJamPhaseObj = jamPhase.gameObject;

            RectTransform photoPhase = CreatePanel(modal, "PhotocopyPhase", new Color(0, 0, 0, 0), raycast: false, rounded: false);
            SetFill(photoPhase, 20f, 14f);

            Text photoHdr = CreateText(photoPhase, "PhotoHdr", "RECOVERED CRUMPLED A4 PHOTOCOPY", 15, new Color(0.95f, 0.85f, 0.52f), TextAnchor.MiddleCenter, false);
            photoHdr.fontStyle = FontStyle.Bold;
            RectTransform phRect = photoHdr.GetComponent<RectTransform>();
            phRect.anchorMin = new Vector2(0f, 1f); phRect.anchorMax = new Vector2(1f, 1f); phRect.pivot = new Vector2(0.5f, 1f);
            phRect.sizeDelta = new Vector2(0f, 28f); phRect.anchoredPosition = Vector2.zero;

            RectTransform a4Frame = CreatePanel(photoPhase, "A4PaperInHand", PaperBase, raycast: false);
            a4Frame.anchorMin = new Vector2(0.5f, 0.59f); a4Frame.anchorMax = new Vector2(0.5f, 0.59f); a4Frame.pivot = new Vector2(0.5f, 0.5f);
            a4Frame.sizeDelta = new Vector2(278f, 390f);
            a4Frame.localRotation = Quaternion.Euler(0f, 0f, -2.8f);

            GameObject rawObj = new GameObject("PhotocopyRaw");
            rawObj.transform.SetParent(a4Frame, false);
            SetFill(rawObj.AddComponent<RectTransform>(), 4f, 4f);
            photocopyRawImage = rawObj.AddComponent<RawImage>();
            if (photocopyTexture != null) photocopyRawImage.texture = photocopyTexture;

            RectTransform commentBox = CreatePanel(photoPhase, "DetectiveCommentBox", PaperCard, raycast: false);
            commentBox.anchorMin = new Vector2(0.06f, 0f); commentBox.anchorMax = new Vector2(0.94f, 0f); commentBox.pivot = new Vector2(0.5f, 0f);
            commentBox.sizeDelta = new Vector2(0f, 64f); commentBox.anchoredPosition = new Vector2(0f, 48f);
            detectiveCommentText = CreateText(commentBox, "DetTxt",
                "Investigator: \"Wait a second... who on earth photocopies their own face on an office printer?!\"",
                14, InkPrimary, TextAnchor.MiddleLeft, false);
            detectiveCommentText.fontStyle = FontStyle.Italic;
            SetFill(detectiveCommentText.GetComponent<RectTransform>(), 14f, 6f);

            GameObject photoBtns = new GameObject("PhotoBtns");
            photoBtns.transform.SetParent(photoPhase, false);
            RectTransform pbsRect = photoBtns.AddComponent<RectTransform>();
            pbsRect.anchorMin = new Vector2(0.14f, 0f); pbsRect.anchorMax = new Vector2(0.86f, 0f); pbsRect.pivot = new Vector2(0.5f, 0f);
            pbsRect.sizeDelta = new Vector2(0f, 38f); pbsRect.anchoredPosition = new Vector2(0f, 2f);
            HorizontalLayoutGroup pbLay = photoBtns.AddComponent<HorizontalLayoutGroup>();
            pbLay.spacing = 12; pbLay.childControlWidth = true; pbLay.childForceExpandWidth = true;

            CreateButton(photoBtns.transform, "ReJamBtn", "Re-Test Jam", PaperBtn, 36f, () =>
            {
                StopActiveSpeech();
                SoundManager.Instance?.Play("wrong_buzz", 0.45f);
                PrinterJamEffect.Instance?.TriggerJamAgain();
                OpenPrinterJamModal();
            }, InkPrimary);
            CreateButton(photoBtns.transform, "ClosePhotoBtn", "Put Down", AccentSage, 36f, () =>
            {
                StopActiveSpeech();
                SoundManager.Instance?.Play("paper_rustle", 0.65f);
                printerJamModal.SetActive(false);
                SyncModalLock();
            }, PaperWhite);

            printerPhotocopyPhaseObj = photoPhase.gameObject;
            printerJamModal = modal.gameObject;
            printerJamModal.SetActive(false);
        }

        // === REALISTIC HAND-HELD YELLOW LINED LEGAL PAD ON WOODEN CLIPBOARD ===
        private void BuildClipboardModal(Transform parent)
        {
            // Warm walnut wooden clipboard backing with soft drop shadow
            RectTransform board = CreatePanel(parent, "ClipboardModal", new Color(0.32f, 0.20f, 0.11f, 0.99f), raycast: true);
            board.anchorMin = new Vector2(0.30f, 0.10f); board.anchorMax = new Vector2(0.70f, 0.90f);
            board.offsetMin = Vector2.zero; board.offsetMax = Vector2.zero;
            Shadow boardShadow = board.gameObject.AddComponent<Shadow>();
            boardShadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
            boardShadow.effectDistance = new Vector2(8f, -10f);

            // Brushed metallic spring clip at top center
            RectTransform metalClip = CreatePanel(board, "MetalClip", new Color(0.62f, 0.65f, 0.70f, 1f), raycast: false);
            metalClip.anchorMin = new Vector2(0.5f, 1f); metalClip.anchorMax = new Vector2(0.5f, 1f); metalClip.pivot = new Vector2(0.5f, 1f);
            metalClip.sizeDelta = new Vector2(164f, 36f); metalClip.anchoredPosition = new Vector2(0f, -6f);
            RectTransform clipClamp = CreatePanel(metalClip, "ClipClamp", new Color(0.46f, 0.49f, 0.54f, 1f), raycast: false);
            clipClamp.anchorMin = new Vector2(0.18f, 0.18f); clipClamp.anchorMax = new Vector2(0.82f, 0.68f);
            clipClamp.offsetMin = Vector2.zero; clipClamp.offsetMax = Vector2.zero;

            // Real pale-yellow blue-ruled red-margin legal pad sheet!
            GameObject padGo = new GameObject("YellowLegalPad");
            padGo.transform.SetParent(board, false);
            RectTransform paper = padGo.AddComponent<RectTransform>();
            paper.anchorMin = Vector2.zero; paper.anchorMax = Vector2.one;
            paper.offsetMin = new Vector2(20f, 58f); paper.offsetMax = new Vector2(-20f, -34f);
            RawImage padImg = padGo.AddComponent<RawImage>();
            padImg.texture = CreateLinedLegalPadTexture();
            padImg.raycastTarget = false;

            // Handwritten header on top margin
            Text hdrTxt = CreateText(paper, "PadHeader", "Mail Delivery Log  —  Archive Notes", 20, new Color(0.14f, 0.20f, 0.36f), TextAnchor.MiddleLeft, false);
            if (handwritingFont != null) hdrTxt.font = handwritingFont;
            hdrTxt.fontStyle = FontStyle.Bold;
            RectTransform hRt = hdrTxt.GetComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0f, 1f); hRt.anchorMax = new Vector2(1f, 1f); hRt.pivot = new Vector2(0.5f, 1f);
            hRt.sizeDelta = new Vector2(0f, 52f); hRt.anchoredPosition = new Vector2(0f, -6f);
            hRt.offsetMin = new Vector2(68f, -58f); hRt.offsetMax = new Vector2(-18f, -6f);

            // Handwritten ballpoint ink notes aligned to the left red margin!
            Text bodyTxt = CreateText(paper, "PadNotes",
                "Filing Cabinet (left wall):\n\n" +
                "•  Drawer B-01 (Top)\n" +
                "    old 2023 office receipts\n\n" +
                "<color=#961C1C>★  Drawer B-02 (Middle)  —  URGENT!</color>\n" +
                "<color=#961C1C>    A. Moretti — Yellow Envelope</color>\n" +
                "    (VitaPulse blog allegations)\n\n" +
                "•  Drawer B-03 (Bottom)\n" +
                "    blank intake forms\n\n" +
                "<i>Note to self: turn left (◄), open B-02\n" +
                "and grab Alex's envelope!</i>",
                17, new Color(0.13f, 0.19f, 0.34f), TextAnchor.UpperLeft, false);
            if (handwritingFont != null) bodyTxt.font = handwritingFont;
            bodyTxt.lineSpacing = 1.12f;
            RectTransform bRt = bodyTxt.GetComponent<RectTransform>();
            bRt.anchorMin = Vector2.zero; bRt.anchorMax = Vector2.one;
            bRt.offsetMin = new Vector2(68f, 16f); bRt.offsetMax = new Vector2(-18f, -66f);

            Button putDownBtn = CreateButton(board, "PutDownClipboard", "Put Notepad Down", PaperBtn, 38f, () =>
            {
                SoundManager.Instance?.Play("paper_rustle", 0.65f);
                clipboardModalPanel.SetActive(false);
                if (deskClipboard3D != null) deskClipboard3D.SetActive(true);
                SyncModalLock();
            }, InkPrimary);
            RectTransform pdRect = putDownBtn.GetComponent<RectTransform>();
            pdRect.anchorMin = new Vector2(0.18f, 0f); pdRect.anchorMax = new Vector2(0.82f, 0f); pdRect.pivot = new Vector2(0.5f, 0f);
            pdRect.sizeDelta = new Vector2(0f, 38f); pdRect.anchoredPosition = new Vector2(0f, 12f);

            clipboardModalPanel = board.gameObject;
            clipboardModalPanel.SetActive(false);
        }

        // === HAND-HELD YELLOW ENVELOPE & HANDWRITTEN LETTER FROM ALEX MORETTI ===
        private void BuildAlexLetterModal(Transform parent)
        {
            // Kraft-yellow manila envelope backing with shadow
            RectTransform envelope = CreatePanel(parent, "AlexLetterModal", new Color(0.84f, 0.71f, 0.42f, 0.99f), raycast: true);
            AddSubtleBorder(envelope.gameObject, new Color(0.55f, 0.44f, 0.22f, 1f));
            envelope.anchorMin = new Vector2(0.24f, 0.10f); envelope.anchorMax = new Vector2(0.76f, 0.90f);
            envelope.offsetMin = Vector2.zero; envelope.offsetMax = Vector2.zero;
            Shadow envShadow = envelope.gameObject.AddComponent<Shadow>();
            envShadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
            envShadow.effectDistance = new Vector2(8f, -10f);

            // Top envelope flap strip
            RectTransform envFlap = CreatePanel(envelope, "EnvFlap", new Color(0.76f, 0.62f, 0.34f, 1f), raycast: false, rounded: false);
            envFlap.anchorMin = new Vector2(0f, 1f); envFlap.anchorMax = new Vector2(1f, 1f); envFlap.pivot = new Vector2(0.5f, 1f);
            envFlap.sizeDelta = new Vector2(0f, 44f); envFlap.anchoredPosition = Vector2.zero;
            Text envTxt = CreateText(envFlap, "EnvLbl", "MANILA ENVELOPE  •  DRAWER B-02  •  EVIDENCE #letter_alex", 11, new Color(0.28f, 0.20f, 0.08f), TextAnchor.MiddleLeft, false);
            envTxt.fontStyle = FontStyle.Bold;
            SetFill(envTxt.GetComponent<RectTransform>(), 16f, 4f);

            // Red priority rubber-stamp badge on envelope corner
            RectTransform stamp = CreatePanel(envFlap, "UrgentStamp", new Color(0.74f, 0.14f, 0.14f, 0.92f), raycast: false);
            stamp.anchorMin = new Vector2(1f, 0.5f); stamp.anchorMax = new Vector2(1f, 0.5f); stamp.pivot = new Vector2(1f, 0.5f);
            stamp.sizeDelta = new Vector2(138f, 24f); stamp.anchoredPosition = new Vector2(-12f, 0f);
            stamp.localRotation = Quaternion.Euler(0f, 0f, -3.5f);
            Text stTxt = CreateText(stamp, "StTxt", "URGENT CITIZEN TIP", 10, PaperWhite, TextAnchor.MiddleCenter, false);
            stTxt.fontStyle = FontStyle.Bold;
            SetFill(stTxt.GetComponent<RectTransform>(), 4f, 2f);

            // Folded cream stationery sheet pulled out of the envelope
            RectTransform sheet = CreatePanel(envelope, "LetterSheet", new Color(0.97f, 0.945f, 0.875f, 1f), raycast: false);
            AddSubtleBorder(sheet.gameObject, new Color(0.70f, 0.63f, 0.50f, 0.9f));
            sheet.anchorMin = Vector2.zero; sheet.anchorMax = Vector2.one;
            sheet.offsetMin = new Vector2(22f, 60f); sheet.offsetMax = new Vector2(-22f, -48f);
            sheet.localRotation = Quaternion.Euler(0f, 0f, 0.8f);

            // Subtle horizontal paper-fold crease across middle of sheet
            RectTransform crease = CreatePanel(sheet, "FoldCrease", new Color(0.68f, 0.62f, 0.50f, 0.28f), raycast: false, rounded: false);
            crease.anchorMin = new Vector2(0.02f, 0.50f); crease.anchorMax = new Vector2(0.98f, 0.50f);
            crease.sizeDelta = new Vector2(0f, 2f);

            // Postmark & sender header
            Text postmark = CreateText(sheet, "Postmark", "MILAN HEALTH OFFICE  •  DELIVERED THIS MORNING\nFROM: ALEX MORETTI (22, RESEARCHER)", 11, InkMuted, TextAnchor.UpperLeft, false);
            if (monoFont != null) postmark.font = monoFont;
            RectTransform pmRt = postmark.GetComponent<RectTransform>();
            pmRt.anchorMin = new Vector2(0f, 1f); pmRt.anchorMax = new Vector2(1f, 1f); pmRt.pivot = new Vector2(0.5f, 1f);
            pmRt.offsetMin = new Vector2(24f, -54f); pmRt.offsetMax = new Vector2(-24f, -14f);

            // Handwritten letter body
            Text letterBody = CreateText(sheet, "LetterBody",
                "To the Digital Health Investigator,\n\n" +
                "Nobody listens when I warn them, so I'm sending this directly to your desk.\n\n" +
                "<color=#961C1C><b>\"VitaPulse is secretly selling everyone's private medical information!\"</b></color>\n\n" +
                "I stayed up all night researching it. Everyone keeps repeating corporate PR, " +
                "but I found an independent blog that finally exposes what's really going on.\n\n" +
                "Check the link on your computer — don't let them sweep this under the rug!\n\n" +
                "<i>— Alex Moretti</i>",
                17, new Color(0.14f, 0.18f, 0.30f), TextAnchor.UpperLeft, false);
            if (handwritingFont != null) letterBody.font = handwritingFont;
            letterBody.lineSpacing = 1.12f;
            RectTransform lbRt = letterBody.GetComponent<RectTransform>();
            lbRt.anchorMin = Vector2.zero; lbRt.anchorMax = Vector2.one;
            lbRt.offsetMin = new Vector2(26f, 76f); lbRt.offsetMax = new Vector2(-26f, -64f);

            // Highlighted URL slip clipped to the bottom of the letter
            RectTransform urlSlip = CreatePanel(sheet, "UrlSlip", new Color(0.95f, 0.91f, 0.72f, 1f), raycast: false);
            AddSubtleBorder(urlSlip.gameObject, AccentOchre);
            urlSlip.anchorMin = new Vector2(0.05f, 0f); urlSlip.anchorMax = new Vector2(0.95f, 0f); urlSlip.pivot = new Vector2(0.5f, 0f);
            urlSlip.sizeDelta = new Vector2(0f, 54f); urlSlip.anchoredPosition = new Vector2(0f, 14f);
            Text urlTxt = CreateText(urlSlip, "UrlTxt",
                "<b>BLOG LINK UNLOCKED IN HEALTHBROWSER (3D MONITOR):</b>\n<color=#7D2629>http://truth-health-watch.blog/vitapulse-exposed</color>",
                12, InkPrimary, TextAnchor.MiddleCenter, false);
            if (monoFont != null) urlTxt.font = monoFont;
            SetFill(urlTxt.GetComponent<RectTransform>(), 10f, 6f);

            Button keepBtn = CreateButton(envelope, "KeepAlexLetterBtn", "Keep Letter as Evidence (Close)", AccentSage, 38f, () =>
            {
                SoundManager.Instance?.Play("paper_rustle", 0.65f);
                alexLetterModalPanel.SetActive(false);
                SyncModalLock();
            }, PaperWhite);
            keepBtn.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;
            RectTransform kbRt = keepBtn.GetComponent<RectTransform>();
            kbRt.anchorMin = new Vector2(0.16f, 0f); kbRt.anchorMax = new Vector2(0.84f, 0f); kbRt.pivot = new Vector2(0.5f, 0f);
            kbRt.sizeDelta = new Vector2(0f, 38f); kbRt.anchoredPosition = new Vector2(0f, 12f);

            alexLetterModalPanel = envelope.gameObject;
            alexLetterModalPanel.SetActive(false);
        }

        private void BuildEvidenceDrawer(Transform parent)
        {
            RectTransform panel = CreatePanel(parent, "EvidenceDrawer", PaperBase, raycast: true);
            AddSubtleBorder(panel.gameObject, PaperBorder);
            panel.anchorMin = new Vector2(1f, 1f); panel.anchorMax = new Vector2(1f, 1f); panel.pivot = new Vector2(1f, 1f);
            panel.sizeDelta = new Vector2(320f, 440f); panel.anchoredPosition = new Vector2(-14f, -58f);

            VerticalLayoutGroup lay = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(14, 14, 12, 12); lay.spacing = 6;
            lay.childControlHeight = false; lay.childForceExpandHeight = false;

            Text hdr = CreateText(panel, "Hdr", "COLLECTED EVIDENCE", 14, AccentBurgundy, TextAnchor.MiddleLeft, false);
            hdr.fontStyle = FontStyle.Bold; hdr.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 22f);

            CreateDividerLine(panel, PaperBorder);

            GameObject listObj = new GameObject("List");
            listObj.transform.SetParent(panel, false);
            listObj.AddComponent<RectTransform>().sizeDelta = new Vector2(292f, 200f);
            VerticalLayoutGroup lLay = listObj.AddComponent<VerticalLayoutGroup>();
            lLay.spacing = 5; lLay.childControlHeight = false; lLay.childForceExpandHeight = false;
            inventoryListParent = listObj.transform;

            RectTransform detailBox = CreatePanel(panel, "DetailBox", PaperCard, raycast: false);
            AddSubtleBorder(detailBox.gameObject, PaperBorder);
            detailBox.sizeDelta = new Vector2(292f, 136f);
            evidenceDetailText = CreateText(detailBox, "Detail", "Select a card above to inspect.", 12, InkPrimary, TextAnchor.UpperLeft, false);
            SetFill(evidenceDetailText.GetComponent<RectTransform>(), 10f, 8f);

            CreateButton(panel, "CloseInv", "Close", PaperBtn, 32f, () => inventoryDrawer.SetActive(false), InkPrimary);

            inventoryDrawer = panel.gameObject;
            inventoryDrawer.SetActive(false);
        }

        private void BuildDhliDrawer(Transform parent)
        {
            RectTransform panel = CreatePanel(parent, "DhliDrawer", PaperBase, raycast: true);
            AddSubtleBorder(panel.gameObject, PaperBorder);
            panel.anchorMin = new Vector2(1f, 1f); panel.anchorMax = new Vector2(1f, 1f); panel.pivot = new Vector2(1f, 1f);
            panel.sizeDelta = new Vector2(360f, 345f); panel.anchoredPosition = new Vector2(-14f, -58f);

            VerticalLayoutGroup lay = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(16, 16, 14, 14); lay.spacing = 8;
            lay.childControlHeight = false; lay.childForceExpandHeight = false;

            Text hdr = CreateText(panel, "DhliHdr", "ADAPTIVE DHLI PROFILE", 15, AccentBurgundy, TextAnchor.MiddleLeft, false);
            hdr.fontStyle = FontStyle.Bold; hdr.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 22f);

            dhliSubtitleText = CreateText(panel, "DhliSub", "Default Baseline (50/50/50)", 11, InkMuted, TextAnchor.MiddleLeft, false);
            dhliSubtitleText.fontStyle = FontStyle.Italic;
            dhliSubtitleText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 16f);

            CreateSkillBarRow(panel, "RelBar", out relBarLabel, out relBarFill);
            CreateSkillBarRow(panel, "PrivBar", out privBarLabel, out privBarFill);
            CreateSkillBarRow(panel, "NavBar", out navBarLabel, out navBarFill);

            dhliDetailText = CreateText(panel, "DhliText", "", 12, InkPrimary, TextAnchor.UpperLeft, false);
            dhliDetailText.GetComponent<RectTransform>().sizeDelta = new Vector2(328f, 70f);

            GameObject dhliBtnRow = new GameObject("DhliBtnRow");
            dhliBtnRow.transform.SetParent(panel, false);
            dhliBtnRow.AddComponent<RectTransform>().sizeDelta = new Vector2(328f, 34f);
            HorizontalLayoutGroup dBtnLay = dhliBtnRow.AddComponent<HorizontalLayoutGroup>();
            dBtnLay.spacing = 8; dBtnLay.childControlWidth = true; dBtnLay.childForceExpandWidth = true;
            dBtnLay.childControlHeight = true; dBtnLay.childForceExpandHeight = true;

            Button recalBtn = CreateButton(dhliBtnRow.transform, "RecalibrateBtn", "Edit Profile", AccentBurgundy, 32f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                returnToPauseAfterOnboarding = false;
                dhliDrawer.SetActive(false);
                RefreshOnboardingButtons();
                if (modalBackdrop != null) modalBackdrop.SetActive(true);
                onboardingPanel.SetActive(true);
                AnimateModalPopIn(onboardingPanel);
                SyncModalLock();
            }, PaperWhite);
            recalBtn.GetComponent<LayoutElement>().flexibleWidth = 1.6f;

            Button closeDhliBtn = CreateButton(dhliBtnRow.transform, "CloseDhli", "Close", PaperBtn, 32f, () => dhliDrawer.SetActive(false), InkPrimary);
            closeDhliBtn.GetComponent<LayoutElement>().flexibleWidth = 1f;

            dhliDrawer = panel.gameObject;
            dhliDrawer.SetActive(false);
        }

        private void CreateSkillBarRow(Transform parent, string name, out Text labelText, out Image fillImage)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent, false);
            row.AddComponent<RectTransform>().sizeDelta = new Vector2(328f, 38f);
            VerticalLayoutGroup rLay = row.AddComponent<VerticalLayoutGroup>();
            rLay.spacing = 4; rLay.childControlHeight = false; rLay.childForceExpandHeight = false;

            labelText = CreateText(row.transform, "Lbl", "", 12, InkPrimary, TextAnchor.MiddleLeft, false);
            labelText.GetComponent<RectTransform>().sizeDelta = new Vector2(328f, 18f);

            RectTransform track = CreatePanel(row.transform, "Track", PaperBtn, raycast: false, rounded: false);
            track.sizeDelta = new Vector2(328f, 12f);
            Image trackImg = track.GetComponent<Image>();
            if (pillSprite != null) { trackImg.sprite = pillSprite; trackImg.type = Image.Type.Sliced; trackImg.pixelsPerUnitMultiplier = 2f; }

            RectTransform fillRt = CreatePanel(track, "Fill", AccentOchre, raycast: false, rounded: false);
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = new Vector2(0.5f, 1f);
            fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;
            fillImage = fillRt.GetComponent<Image>();
            if (pillSprite != null) { fillImage.sprite = pillSprite; fillImage.type = Image.Type.Sliced; fillImage.pixelsPerUnitMultiplier = 2f; }
        }

        private void BuildWelcomeModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "WelcomeModal", PaperBase, raycast: true);
            AddSubtleBorder(modal.gameObject, PaperBorder);
            modal.anchorMin = new Vector2(0.21f, 0.19f); modal.anchorMax = new Vector2(0.79f, 0.81f);
            modal.offsetMin = Vector2.zero; modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup lay = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(32, 32, 24, 24); lay.spacing = 11;
            lay.childControlHeight = false; lay.childForceExpandHeight = false;

            RectTransform badgePill = CreatePanel(modal, "OfficeTag", PaperCard, raycast: false);
            badgePill.sizeDelta = new Vector2(0f, 24f);
            Text tagTxt = CreateText(badgePill, "Tag", "DIGITAL HEALTH INVESTIGATION OFFICE  •  CASE FILE #00", 11, AccentBurgundy, TextAnchor.MiddleCenter, false);
            tagTxt.fontStyle = FontStyle.Bold;
            SetFill(tagTxt.GetComponent<RectTransform>(), 8f, 2f);

            Text title = CreateText(modal, "T", "THE HEALTH FILES: CASE ZERO", 24, InkPrimary, TextAnchor.MiddleCenter, false);
            title.fontStyle = FontStyle.Bold; title.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 34f);

            CreateDividerLine(modal, PaperBorder);

            Text body = CreateText(modal, "B",
                "Investigate the viral health app <b>VitaPulse</b> and uncover the truth behind the claims.\n\n" +
                "•  <b>◄ / ► Arrows (or Q / E):</b> Turn around your 3D office\n" +
                "•  <b>Left-Click:</b> Phone, Live 3D Monitor OS, Books, Printer & Board\n" +
                "•  <b>Esc:</b> Back / Pause Menu",
                14, InkPrimary, TextAnchor.UpperLeft, false);
            body.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 134f);

            Button onbBtn = CreateButton(modal, "OnboardingBtn", "Personalize Profile", AccentBurgundy, 44f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.6f);
                returnToPauseAfterOnboarding = false;
                welcomePanel.SetActive(false);
                RefreshOnboardingButtons();
                if (modalBackdrop != null) modalBackdrop.SetActive(true);
                onboardingPanel.SetActive(true);
                AnimateModalPopIn(onboardingPanel);
                SyncModalLock();
            }, PaperWhite);
            onbBtn.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;

            CreateButton(modal, "StartBtn", "Play Now", PaperBtn, 40f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.6f);
                welcomePanel.SetActive(false);
                if (modalBackdrop != null) modalBackdrop.SetActive(false);
                SyncModalLock();
            }, InkPrimary);

            welcomePanel = modal.gameObject;
            welcomePanel.SetActive(false);
        }

        private void BuildOnboardingModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "OnboardingModal", PaperBase, raycast: true);
            AddSubtleBorder(modal.gameObject, PaperBorder);
            modal.anchorMin = new Vector2(0.12f, 0.06f); modal.anchorMax = new Vector2(0.88f, 0.94f);
            modal.offsetMin = Vector2.zero; modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup lay = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(28, 28, 18, 18); lay.spacing = 9;
            lay.childControlHeight = false; lay.childForceExpandHeight = false;

            RectTransform topTag = CreatePanel(modal, "OnbTopTag", PaperCard, raycast: false);
            topTag.sizeDelta = new Vector2(0f, 22f);
            Text topTagTxt = CreateText(topTag, "Tag", "DAY 1 INTAKE DOSSIER  •  DHLI PERSONALIZATION", 11, AccentBurgundy, TextAnchor.MiddleCenter, false);
            topTagTxt.fontStyle = FontStyle.Bold;
            SetFill(topTagTxt.GetComponent<RectTransform>(), 8f, 2f);

            Text title = CreateText(modal, "OnbTitle", "INVESTIGATOR BADGE SETUP", 22, InkPrimary, TextAnchor.MiddleCenter, false);
            title.fontStyle = FontStyle.Bold; title.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 28f);

            Text sub = CreateText(modal, "OnbSub",
                "Select the habits closest to yours — your case file calibrates hint support to your style.",
                13, InkMuted, TextAnchor.MiddleCenter, false);
            sub.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 18f);

            CreateDividerLine(modal, PaperBorder);

            string[] questions =
            {
                "<color=#7D2629>01  RELIABILITY</color>  —  When you see a viral health claim online, what is your first reaction?",
                "<color=#7D2629>02  PRIVACY</color>  —  When a new health app asks for Contacts, Location & Microphone access:",
                "<color=#7D2629>03  NAVIGATION</color>  —  Finding official medical guidance on a hospital or health portal feels:"
            };

            for (int q = 0; q < 3; q++)
            {
                int qIdx = q;
                RectTransform card = CreatePanel(modal, $"Q{q}_Card", PaperCard, raycast: false);
                AddSubtleBorder(card.gameObject, PaperBorder);
                card.sizeDelta = new Vector2(0f, 88f);

                HorizontalLayoutGroup cardRow = card.gameObject.AddComponent<HorizontalLayoutGroup>();
                cardRow.padding = new RectOffset(0, 14, 0, 0); cardRow.spacing = 12;
                cardRow.childControlWidth = true; cardRow.childForceExpandWidth = false;
                cardRow.childControlHeight = true; cardRow.childForceExpandHeight = true;

                RectTransform stripe = CreatePanel(card, "Accent", AccentBurgundy, raycast: false, rounded: false);
                LayoutElement stripeLe = stripe.gameObject.AddComponent<LayoutElement>();
                stripeLe.minWidth = 4f; stripeLe.preferredWidth = 4f; stripeLe.flexibleWidth = 0f;

                GameObject contentObj = new GameObject("Content");
                contentObj.transform.SetParent(card, false);
                contentObj.AddComponent<RectTransform>();
                LayoutElement contentLe = contentObj.AddComponent<LayoutElement>();
                contentLe.minWidth = 200f; contentLe.flexibleWidth = 1f;

                VerticalLayoutGroup cardLay = contentObj.AddComponent<VerticalLayoutGroup>();
                cardLay.padding = new RectOffset(2, 2, 10, 10); cardLay.spacing = 8;
                cardLay.childControlWidth = true; cardLay.childForceExpandWidth = true;
                cardLay.childControlHeight = false; cardLay.childForceExpandHeight = false;

                Text qTxt = CreateText(contentObj.transform, $"Q{q}_Lbl", questions[q], 13, InkPrimary, TextAnchor.MiddleLeft, false);
                qTxt.fontStyle = FontStyle.Bold; qTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 22f);

                GameObject optRow = new GameObject($"Q{q}_Opts");
                optRow.transform.SetParent(contentObj.transform, false);
                optRow.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 38f);
                HorizontalLayoutGroup oLay = optRow.AddComponent<HorizontalLayoutGroup>();
                oLay.spacing = 10; oLay.childControlWidth = true; oLay.childForceExpandWidth = true;
                oLay.childControlHeight = true; oLay.childForceExpandHeight = true;

                for (int opt = 0; opt < 3; opt++)
                {
                    int levelVal = opt + 1;
                    Button optBtn = CreateButton(optRow.transform, $"Q{q}_Opt{opt}", onboardingBaseLabels[q, opt], PaperBtn, 36f, () =>
                    {
                        SoundManager.Instance?.Play("ui_click", 0.5f);
                        onboardingAnswers[qIdx] = levelVal;
                        RefreshOnboardingButtons();
                    }, InkPrimary);
                    onboardingOptionImages[q, opt] = optBtn.GetComponent<Image>();
                    onboardingOptionTexts[q, opt] = optBtn.GetComponentInChildren<Text>();
                }
            }

            RectTransform previewBox = CreatePanel(modal, "ModePreview", PaperCard, raycast: false);
            onboardingPreviewOutline = previewBox.gameObject.AddComponent<Outline>();
            onboardingPreviewOutline.effectColor = AccentSage;
            onboardingPreviewOutline.effectDistance = new Vector2(1.5f, -1.5f);
            previewBox.sizeDelta = new Vector2(0f, 48f);
            onboardingPreviewText = CreateText(previewBox, "PreviewTxt", "", 13, InkPrimary, TextAnchor.MiddleCenter, false);
            SetFill(onboardingPreviewText.GetComponent<RectTransform>(), 14f, 4f);

            GameObject actionRow = new GameObject("OnbActions");
            actionRow.transform.SetParent(modal, false);
            actionRow.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 46f);
            HorizontalLayoutGroup aLay = actionRow.AddComponent<HorizontalLayoutGroup>();
            aLay.spacing = 10; aLay.childControlWidth = true; aLay.childForceExpandWidth = true;
            aLay.childControlHeight = true; aLay.childForceExpandHeight = true;

            Button backBtn = CreateButton(actionRow.transform, "SkipOnbBtn", "Skip", PaperBtn, 44f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                if (returnToPauseAfterOnboarding)
                {
                    OpenPauseMenu();
                }
                else
                {
                    onboardingPanel.SetActive(false);
                    if (modalBackdrop != null) modalBackdrop.SetActive(false);
                    SyncModalLock();
                }
            }, InkPrimary);
            onboardingBackBtnText = backBtn.GetComponentInChildren<Text>();
            backBtn.GetComponent<LayoutElement>().flexibleWidth = 1.1f;

            Button saveBtn = CreateButton(actionRow.transform, "SaveOnbBtn", "Start Game", AccentBurgundy, 44f, () =>
            {
                SoundManager.Instance?.Play("evidence_unlock", 0.7f);
                PersonalizationManager.Instance?.ApplyOnboardingSelfCheck(onboardingAnswers[0], onboardingAnswers[1], onboardingAnswers[2]);
                RefreshDhliDrawer();
                string mode = PersonalizationManager.Instance != null ? PersonalizationManager.Instance.GetAssignedModeLabel() : "Investigator";
                ShowToast($"Badge issued: {mode}! Focus: {PersonalizationManager.Instance?.GetWeakestDomain()}.");

                if (returnToPauseAfterOnboarding)
                {
                    OpenPauseMenu();
                }
                else
                {
                    onboardingPanel.SetActive(false);
                    if (modalBackdrop != null) modalBackdrop.SetActive(false);
                    SyncModalLock();
                }
            }, PaperWhite);
            onboardingSaveBtnText = saveBtn.GetComponentInChildren<Text>();
            onboardingSaveBtnText.fontStyle = FontStyle.Bold;
            saveBtn.GetComponent<LayoutElement>().flexibleWidth = 2.8f;

            onboardingPanel = modal.gameObject;
            onboardingPanel.SetActive(false);
        }

        private void RefreshOnboardingButtons()
        {
            for (int q = 0; q < 3; q++)
            {
                for (int opt = 0; opt < 3; opt++)
                {
                    bool isSelected = onboardingAnswers[q] == opt + 1;
                    if (onboardingOptionImages[q, opt] != null)
                        onboardingOptionImages[q, opt].color = isSelected ? AccentSage : PaperBtn;

                    if (onboardingOptionTexts[q, opt] != null)
                    {
                        onboardingOptionTexts[q, opt].text = isSelected
                            ? $"✓  {onboardingBaseLabels[q, opt]}"
                            : onboardingBaseLabels[q, opt];
                        onboardingOptionTexts[q, opt].fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;
                        onboardingOptionTexts[q, opt].color = isSelected ? PaperWhite : InkPrimary;
                    }
                }
            }

            if (onboardingBackBtnText != null)
                onboardingBackBtnText.text = returnToPauseAfterOnboarding ? "←  Back" : "Skip";

            if (onboardingSaveBtnText != null)
                onboardingSaveBtnText.text = returnToPauseAfterOnboarding ? "Save & Back" : "Start Game";

            if (onboardingPreviewText == null) return;

            bool anyLow = onboardingAnswers[0] == 1 || onboardingAnswers[1] == 1 || onboardingAnswers[2] == 1;
            bool allHigh = onboardingAnswers[0] == 3 && onboardingAnswers[1] == 3 && onboardingAnswers[2] == 3;

            if (anyLow)
            {
                string focus = onboardingAnswers[0] == 1 ? "Reliability"
                    : (onboardingAnswers[1] == 1 ? "Privacy" : "Navigation");
                onboardingPreviewText.text = $"Mode:  <b><color=#7D2629>Guided</color></b>  •  Extra hints enabled (Focus: <b>{focus}</b>)";
                if (onboardingPreviewOutline != null) onboardingPreviewOutline.effectColor = AccentBurgundy;
            }
            else if (allHigh)
            {
                onboardingPreviewText.text = "Mode:  <b><color=#2B5E4D>Independent</color></b>  •  Minimal hints";
                if (onboardingPreviewOutline != null) onboardingPreviewOutline.effectColor = AccentSage;
            }
            else
            {
                onboardingPreviewText.text = "Mode:  <b><color=#94631F>Balanced</color></b>  •  Standard hints";
                if (onboardingPreviewOutline != null) onboardingPreviewOutline.effectColor = AccentOchre;
            }
        }

        private void BuildCaseCompleteModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "CaseCompleteModal", PaperBase, raycast: true);
            AddSubtleBorder(modal.gameObject, PaperBorder);
            modal.anchorMin = new Vector2(0.16f, 0.14f); modal.anchorMax = new Vector2(0.84f, 0.86f);
            modal.offsetMin = Vector2.zero; modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup lay = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(30, 30, 22, 22); lay.spacing = 10;
            lay.childControlHeight = false; lay.childForceExpandHeight = false;

            RectTransform topTag = CreatePanel(modal, "CCTag", PaperCard, raycast: false);
            topTag.sizeDelta = new Vector2(0f, 24f);
            Text tagTxt = CreateText(topTag, "Tag", "CASE 01 CLOSED  •  EVIDENCE PINNED TO BOARD", 11, AccentSage, TextAnchor.MiddleCenter, false);
            tagTxt.fontStyle = FontStyle.Bold;
            SetFill(tagTxt.GetComponent<RectTransform>(), 8f, 2f);

            Text title = CreateText(modal, "CCTitle", "CASE 01 COMPLETE: ALEX MORETTI", 22, InkPrimary, TextAnchor.MiddleCenter, false);
            title.fontStyle = FontStyle.Bold; title.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 30f);

            CreateDividerLine(modal, PaperBorder);

            GameObject bodyRow = new GameObject("CCRow");
            bodyRow.transform.SetParent(modal, false);
            bodyRow.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 255f);
            HorizontalLayoutGroup rLay = bodyRow.AddComponent<HorizontalLayoutGroup>();
            rLay.spacing = 18; rLay.childControlWidth = true; rLay.childForceExpandWidth = false;
            rLay.childControlHeight = false; rLay.childForceExpandHeight = false;

            RectTransform pFrame = CreatePanel(bodyRow.transform, "CCPortraitFrame", PaperCard, raycast: false);
            AddSubtleBorder(pFrame.gameObject, PaperBorder);
            pFrame.sizeDelta = new Vector2(144f, 144f);
            LayoutElement pLe = pFrame.gameObject.AddComponent<LayoutElement>();
            pLe.minWidth = 144f; pLe.preferredWidth = 144f; pLe.flexibleWidth = 0f;

            GameObject raw = new GameObject("Raw");
            raw.transform.SetParent(pFrame, false);
            SetFill(raw.AddComponent<RectTransform>(), 5f, 5f);
            caseCompletePortrait = raw.AddComponent<RawImage>();

            caseCompleteSummaryText = CreateText(bodyRow.transform, "CCSummary", "", 13, InkPrimary, TextAnchor.UpperLeft, false);
            caseCompleteSummaryText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 250f);
            LayoutElement sLe = caseCompleteSummaryText.gameObject.AddComponent<LayoutElement>();
            sLe.minWidth = 240f; sLe.flexibleWidth = 1f;

            Button contBtn = CreateButton(modal, "ContinueCase02Btn", "Continue", AccentSage, 44f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.6f);
                caseCompletePanel.SetActive(false);
                if (modalBackdrop != null) modalBackdrop.SetActive(false);
                SyncModalLock();
            }, PaperWhite);
            contBtn.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;

            caseCompletePanel = modal.gameObject;
            caseCompletePanel.SetActive(false);
        }

        private void BuildPhoneModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "PhoneModal", PaperBase, raycast: true);
            AddSubtleBorder(modal.gameObject, PaperBorder);
            modal.anchorMin = new Vector2(0.15f, 0.08f); modal.anchorMax = new Vector2(0.85f, 0.92f);
            modal.offsetMin = Vector2.zero; modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup lay = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(24, 24, 16, 16); lay.spacing = 7;
            lay.childControlHeight = false; lay.childForceExpandHeight = false;

            phoneSpeakerText = CreateText(modal, "Speaker", "TELEPHONE", 19, AccentBurgundy, TextAnchor.MiddleLeft, false);
            phoneSpeakerText.fontStyle = FontStyle.Bold; phoneSpeakerText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 28f);

            CreateDividerLine(modal, PaperBorder);

            GameObject row = new GameObject("Row");
            row.transform.SetParent(modal, false);
            row.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 146f);
            HorizontalLayoutGroup rLay = row.AddComponent<HorizontalLayoutGroup>();
            rLay.spacing = 16; rLay.childControlWidth = true; rLay.childForceExpandWidth = false;
            rLay.childControlHeight = false; rLay.childForceExpandHeight = false;

            RectTransform frame = CreatePanel(row.transform, "PortraitFrame", PaperCard, raycast: false);
            AddSubtleBorder(frame.gameObject, PaperBorder);
            frame.sizeDelta = new Vector2(134f, 134f);
            LayoutElement fLe = frame.gameObject.AddComponent<LayoutElement>();
            fLe.minWidth = 134f; fLe.preferredWidth = 134f; fLe.flexibleWidth = 0f;
            GameObject raw = new GameObject("Raw");
            raw.transform.SetParent(frame, false);
            SetFill(raw.AddComponent<RectTransform>(), 4, 4);
            phonePortraitImage = raw.AddComponent<RawImage>();
            phonePortraitFrame = frame.gameObject;

            RectTransform bodyBox = CreatePanel(row.transform, "DialogueClickBox", PaperCard, raycast: true);
            AddSubtleBorder(bodyBox.gameObject, PaperBorder);
            bodyBox.sizeDelta = new Vector2(0f, 142f);
            LayoutElement bLe = bodyBox.gameObject.AddComponent<LayoutElement>();
            bLe.minWidth = 240f; bLe.flexibleWidth = 1f;
            Button skipClickBtn = bodyBox.gameObject.AddComponent<Button>();
            skipClickBtn.targetGraphic = bodyBox.GetComponent<Image>();
            skipClickBtn.onClick.AddListener(CompleteTypewriterImmediately);

            phoneBodyText = CreateText(bodyBox, "Body", "", 15, InkPrimary, TextAnchor.UpperLeft, false);
            SetFill(phoneBodyText.GetComponent<RectTransform>(), 12f, 10f);

            phoneSkipHintText = CreateText(bodyBox, "SkipHint", "Click to reveal »", 11, InkMuted, TextAnchor.LowerRight, false);
            phoneSkipHintText.fontStyle = FontStyle.Italic;
            SetFill(phoneSkipHintText.GetComponent<RectTransform>(), 10f, 6f);
            phoneSkipHintText.gameObject.SetActive(false);

            phoneFeedbackText = CreateText(modal, "Feedback", "", 13, AccentSage, TextAnchor.UpperLeft, false);
            phoneFeedbackText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 56f);

            GameObject box = new GameObject("ObjBtns");
            box.transform.SetParent(modal, false);
            box.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 224f);
            VerticalLayoutGroup bLay = box.AddComponent<VerticalLayoutGroup>();
            bLay.spacing = 4; bLay.childControlHeight = false; bLay.childForceExpandHeight = false;
            objectionButtonContainer = box.transform;

            CreateButton(modal, "HangUpBtn", "Hang Up", PaperBtn, 36f, () =>
            {
                StopActiveSpeech();
                SoundManager.Instance?.Play("ui_click", 0.5f);
                phonePanel.SetActive(false);
                SyncModalLock();
            }, InkPrimary);

            phonePanel = modal.gameObject;
            phonePanel.SetActive(false);
        }

        private void BuildDocModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "DocModal", PaperBase, raycast: true);
            AddSubtleBorder(modal.gameObject, PaperBorder);
            modal.anchorMin = new Vector2(0.22f, 0.24f); modal.anchorMax = new Vector2(0.78f, 0.76f);
            modal.offsetMin = Vector2.zero; modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup lay = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(28, 28, 22, 22); lay.spacing = 12;
            lay.childControlHeight = false; lay.childForceExpandHeight = false;

            docModalTitle = CreateText(modal, "T", "DOCUMENT", 19, AccentBurgundy, TextAnchor.MiddleLeft, false);
            docModalTitle.fontStyle = FontStyle.Bold; docModalTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 30f);

            CreateDividerLine(modal, PaperBorder);

            docModalBody = CreateText(modal, "B", "", 15, InkPrimary, TextAnchor.UpperLeft, false);
            docModalBody.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 205f);

            CreateButton(modal, "CloseDoc", "Close", PaperBtn, 38f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                docModalPanel.SetActive(false);
                SyncModalLock();
            }, InkPrimary);

            docModalPanel = modal.gameObject;
            docModalPanel.SetActive(false);
        }

        private void BuildPauseModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "PauseModal", PaperBase, raycast: true);
            AddSubtleBorder(modal.gameObject, PaperBorder);
            modal.anchorMin = new Vector2(0.12f, 0.12f); modal.anchorMax = new Vector2(0.88f, 0.88f);
            modal.offsetMin = Vector2.zero; modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup rootLay = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            rootLay.padding = new RectOffset(28, 28, 20, 20); rootLay.spacing = 10;
            rootLay.childControlHeight = false; rootLay.childForceExpandHeight = false;

            RectTransform topTag = CreatePanel(modal, "PauseTag", PaperCard, raycast: false);
            topTag.sizeDelta = new Vector2(0f, 22f);
            Text tagTxt = CreateText(topTag, "Tag", "CASE ZERO  •  PAUSED", 11, AccentBurgundy, TextAnchor.MiddleCenter, false);
            tagTxt.fontStyle = FontStyle.Bold;
            SetFill(tagTxt.GetComponent<RectTransform>(), 8f, 2f);

            Text title = CreateText(modal, "PauseTitle", "MENU", 22, InkPrimary, TextAnchor.MiddleCenter, false);
            title.fontStyle = FontStyle.Bold; title.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 28f);

            CreateDividerLine(modal, PaperBorder);

            GameObject bodyCols = new GameObject("PauseCols");
            bodyCols.transform.SetParent(modal, false);
            bodyCols.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 395f);
            HorizontalLayoutGroup colsLay = bodyCols.AddComponent<HorizontalLayoutGroup>();
            colsLay.spacing = 18;
            colsLay.childControlWidth = true; colsLay.childForceExpandWidth = false;
            colsLay.childControlHeight = true; colsLay.childForceExpandHeight = true;

            GameObject leftCol = new GameObject("LeftCol");
            leftCol.transform.SetParent(bodyCols.transform, false);
            leftCol.AddComponent<RectTransform>();
            LayoutElement leftLe = leftCol.AddComponent<LayoutElement>();
            leftLe.minWidth = 260f; leftLe.flexibleWidth = 1f;

            // VIEW 1: Main Pause Actions
            GameObject mainView = new GameObject("PauseMainView");
            mainView.transform.SetParent(leftCol.transform, false);
            SetFill(mainView.AddComponent<RectTransform>(), 0f, 0f);
            VerticalLayoutGroup mLay = mainView.AddComponent<VerticalLayoutGroup>();
            mLay.padding = new RectOffset(0, 0, 6, 0); mLay.spacing = 11;
            mLay.childControlWidth = true; mLay.childForceExpandWidth = true;
            mLay.childControlHeight = false; mLay.childForceExpandHeight = false;

            Button resumeBtn = CreateButton(mainView.transform, "ResumeBtn", "▶  Continue", AccentBurgundy, 46f, ClosePauseMenu, PaperWhite);
            resumeBtn.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;

            CreateButton(mainView.transform, "OptionsBtn", "⚙  Options", PaperBtn, 44f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                pauseMainView.SetActive(false);
                pauseOptionsView.SetActive(true);
                RefreshAudioUI();
            }, InkPrimary);

            CreateButton(mainView.transform, "PauseRecalBtn", "★  DHLI Profile", PaperBtn, 44f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                returnToPauseAfterOnboarding = true;
                pausePanel.SetActive(false);
                RefreshOnboardingButtons();
                if (modalBackdrop != null) modalBackdrop.SetActive(true);
                onboardingPanel.SetActive(true);
                AnimateModalPopIn(onboardingPanel);
                SyncModalLock();
            }, InkPrimary);

            CreateButton(mainView.transform, "RestartBtn", "↻  Restart", PaperBtn, 44f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }, InkPrimary);

            CreateButton(mainView.transform, "QuitBtn", "✕  Quit", PaperCard, 44f, QuitGame, AccentBurgundy);

            Text pauseFoot = CreateText(mainView.transform, "PauseFoot", "<i>Esc: Back / Close</i>", 12, InkMuted, TextAnchor.MiddleCenter, false);
            pauseFoot.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 24f);
            pauseMainView = mainView;

            // VIEW 2: Options, Audio & Controls
            GameObject optView = new GameObject("PauseOptionsView");
            optView.transform.SetParent(leftCol.transform, false);
            SetFill(optView.AddComponent<RectTransform>(), 0f, 0f);
            VerticalLayoutGroup oLay = optView.AddComponent<VerticalLayoutGroup>();
            oLay.padding = new RectOffset(0, 0, 6, 0); oLay.spacing = 10;
            oLay.childControlWidth = true; oLay.childForceExpandWidth = true;
            oLay.childControlHeight = false; oLay.childForceExpandHeight = false;

            optionsVolumeText = CreateText(optView.transform, "VolLbl", "Volume:  <b>100%</b>", 14, InkPrimary, TextAnchor.MiddleLeft, false);
            optionsVolumeText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 22f);

            GameObject sliderGo = new GameObject("VolumeSlider");
            sliderGo.transform.SetParent(optView.transform, false);
            sliderGo.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 26f);

            RectTransform bgRt = CreatePanel(sliderGo.transform, "Background", PaperBtn, raycast: false, rounded: false);
            bgRt.anchorMin = new Vector2(0f, 0.25f); bgRt.anchorMax = new Vector2(1f, 0.75f);
            bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
            if (pillSprite != null) { bgRt.GetComponent<Image>().sprite = pillSprite; bgRt.GetComponent<Image>().type = Image.Type.Sliced; bgRt.GetComponent<Image>().pixelsPerUnitMultiplier = 2f; }

            GameObject fillArea = new GameObject("FillArea");
            fillArea.transform.SetParent(sliderGo.transform, false);
            RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
            fillAreaRt.anchorMin = new Vector2(0f, 0.25f); fillAreaRt.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRt.offsetMin = Vector2.zero; fillAreaRt.offsetMax = Vector2.zero;

            RectTransform fillRt = CreatePanel(fillArea.transform, "Fill", AccentBurgundy, raycast: false, rounded: false);
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;
            if (pillSprite != null) { fillRt.GetComponent<Image>().sprite = pillSprite; fillRt.GetComponent<Image>().type = Image.Type.Sliced; fillRt.GetComponent<Image>().pixelsPerUnitMultiplier = 2f; }

            GameObject handleArea = new GameObject("HandleSlideArea");
            handleArea.transform.SetParent(sliderGo.transform, false);
            SetFill(handleArea.AddComponent<RectTransform>(), 8f, 0f);

            RectTransform handleRt = CreatePanel(handleArea.transform, "Handle", InkPrimary, raycast: true);
            handleRt.sizeDelta = new Vector2(16f, 22f);

            optionsVolumeSlider = sliderGo.AddComponent<Slider>();
            optionsVolumeSlider.fillRect = fillRt;
            optionsVolumeSlider.handleRect = handleRt;
            optionsVolumeSlider.targetGraphic = handleRt.GetComponent<Image>();
            optionsVolumeSlider.direction = Slider.Direction.LeftToRight;
            optionsVolumeSlider.minValue = 0f; optionsVolumeSlider.maxValue = 1f;
            optionsVolumeSlider.value = SoundManager.Instance != null ? SoundManager.Instance.MasterVolume : 1f;
            optionsVolumeSlider.onValueChanged.AddListener(v => SoundManager.Instance?.SetMasterVolume(v));

            Button optMuteBtn = CreateButton(optView.transform, "OptMuteBtn", "Audio: ON", PaperBtn, 38f, () =>
            {
                SoundManager.Instance?.ToggleMute();
                if (SoundManager.Instance != null && !SoundManager.Instance.IsMuted)
                    SoundManager.Instance.Play("ui_click", 0.5f);
            }, InkPrimary);
            optionsMuteBtnText = optMuteBtn.GetComponentInChildren<Text>();

            Button fsBtn = CreateButton(optView.transform, "OptFullscreenBtn", "Display: Fullscreen", PaperBtn, 38f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                Screen.fullScreen = !Screen.fullScreen;
                RefreshAudioUI();
            }, InkPrimary);
            optionsFullscreenBtnText = fsBtn.GetComponentInChildren<Text>();

            RectTransform ctrlCard = CreatePanel(optView.transform, "ControlsCard", PaperCard, raycast: false);
            AddSubtleBorder(ctrlCard.gameObject, PaperBorder);
            ctrlCard.sizeDelta = new Vector2(0f, 114f);
            Text ctrlTxt = CreateText(ctrlCard, "CtrlTxt",
                "<b>CONTROLS</b>\n" +
                "•  <b>Click:</b> Interact\n" +
                "•  <b>◄ / ► or Q / E:</b> Turn office\n" +
                "•  <b>Esc:</b> Back / Menu",
                12, InkPrimary, TextAnchor.UpperLeft, false);
            SetFill(ctrlTxt.GetComponent<RectTransform>(), 12f, 8f);

            Button backMenuBtn = CreateButton(optView.transform, "BackToPauseBtn", "←  Back", AccentBurgundy, 40f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                pauseOptionsView.SetActive(false);
                pauseMainView.SetActive(true);
            }, PaperWhite);
            backMenuBtn.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;

            pauseOptionsView = optView;
            pauseOptionsView.SetActive(false);

            // RIGHT COLUMN: Live Case Dossier
            RectTransform rightCol = CreatePanel(bodyCols.transform, "DossierCol", PaperCard, raycast: false);
            AddSubtleBorder(rightCol.gameObject, PaperBorder);
            LayoutElement rightLe = rightCol.gameObject.AddComponent<LayoutElement>();
            rightLe.minWidth = 300f; rightLe.flexibleWidth = 1.15f;

            VerticalLayoutGroup rLay = rightCol.gameObject.AddComponent<VerticalLayoutGroup>();
            rLay.padding = new RectOffset(16, 16, 14, 14); rLay.spacing = 12;
            rLay.childControlWidth = true; rLay.childForceExpandWidth = true;
            rLay.childControlHeight = false; rLay.childForceExpandHeight = false;

            CreateDossierCard(rightCol, "RankCard", AccentBurgundy, 64f, out Transform rankContent);
            pauseRankText = CreateText(rankContent, "RankTxt", "<b>RANK: ROOKIE INVESTIGATOR</b>", 13, InkPrimary, TextAnchor.MiddleLeft, false);
            pauseRankText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 44f);

            CreateDossierCard(rightCol, "ProgCard", AccentOchre, 148f, out Transform progContent);
            pauseProgressLabel = CreateText(progContent, "ProgLbl", "<b>CASE 01: ALEX</b>", 13, InkPrimary, TextAnchor.MiddleLeft, false);
            pauseProgressLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 22f);

            RectTransform pTrack = CreatePanel(progContent, "ProgTrack", PaperBtn, raycast: false, rounded: false);
            pTrack.sizeDelta = new Vector2(0f, 12f);
            if (pillSprite != null) { pTrack.GetComponent<Image>().sprite = pillSprite; pTrack.GetComponent<Image>().type = Image.Type.Sliced; pTrack.GetComponent<Image>().pixelsPerUnitMultiplier = 2f; }

            RectTransform pFill = CreatePanel(pTrack, "ProgFill", AccentBurgundy, raycast: false, rounded: false);
            pFill.anchorMin = Vector2.zero; pFill.anchorMax = new Vector2(0.3f, 1f);
            pFill.offsetMin = Vector2.zero; pFill.offsetMax = Vector2.zero;
            pauseProgressFill = pFill.GetComponent<Image>();
            if (pillSprite != null) { pauseProgressFill.sprite = pillSprite; pauseProgressFill.type = Image.Type.Sliced; pauseProgressFill.pixelsPerUnitMultiplier = 2f; }

            pauseNextObjectiveText = CreateText(progContent, "NextObj", "", 12, InkPrimary, TextAnchor.UpperLeft, false);
            pauseNextObjectiveText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 74f);

            CreateDossierCard(rightCol, "ClueCard", AccentSage, 112f, out Transform clueContent);
            pauseCluesAndDhliText = CreateText(clueContent, "ClueDhli", "", 12, InkPrimary, TextAnchor.UpperLeft, false);
            pauseCluesAndDhliText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 88f);

            pausePanel = modal.gameObject;
            pausePanel.SetActive(false);
        }

        private RectTransform CreateDossierCard(Transform parent, string name, Color accentColor, float height, out Transform contentParent)
        {
            RectTransform card = CreatePanel(parent, name, PaperBase, raycast: false);
            AddSubtleBorder(card.gameObject, PaperBorder);
            card.sizeDelta = new Vector2(0f, height);

            HorizontalLayoutGroup row = card.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.padding = new RectOffset(0, 12, 0, 0); row.spacing = 10;
            row.childControlWidth = true; row.childForceExpandWidth = false;
            row.childControlHeight = true; row.childForceExpandHeight = true;

            RectTransform stripe = CreatePanel(card, "Stripe", accentColor, raycast: false, rounded: false);
            LayoutElement sLe = stripe.gameObject.AddComponent<LayoutElement>();
            sLe.minWidth = 4f; sLe.preferredWidth = 4f; sLe.flexibleWidth = 0f;

            GameObject content = new GameObject("Content");
            content.transform.SetParent(card, false);
            content.AddComponent<RectTransform>();
            LayoutElement cLe = content.AddComponent<LayoutElement>();
            cLe.minWidth = 180f; cLe.flexibleWidth = 1f;

            VerticalLayoutGroup cLay = content.AddComponent<VerticalLayoutGroup>();
            cLay.padding = new RectOffset(2, 2, 10, 10); cLay.spacing = 6;
            cLay.childControlWidth = true; cLay.childForceExpandWidth = true;
            cLay.childControlHeight = false; cLay.childForceExpandHeight = false;

            contentParent = content.transform;
            return card;
        }

        private void BuildObjectionSplash(Transform parent)
        {
            RectTransform overlay = CreatePanel(parent, "ObjectionSplash", new Color(0.72f, 0.12f, 0.12f, 0.94f), raycast: false);
            overlay.anchorMin = new Vector2(0.18f, 0.36f); overlay.anchorMax = new Vector2(0.82f, 0.64f);
            overlay.offsetMin = Vector2.zero; overlay.offsetMax = Vector2.zero;

            objectionSplashText = CreateText(overlay, "Splash", "OBJECTION!", 64, new Color(1f, 0.92f, 0.30f), TextAnchor.MiddleCenter, false);
            objectionSplashText.fontStyle = FontStyle.Bold;
            SetFill(objectionSplashText.GetComponent<RectTransform>(), 0, 0);

            objectionSplashRect = overlay;
            objectionSplashPanel = overlay.gameObject;
            objectionSplashPanel.SetActive(false);
        }

        private static Sprite CreateRoundedSprite(int size, int radius)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            float r = radius;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = 0f;
                    float dy = 0f;
                    if (x < r) dx = r - x - 0.5f;
                    else if (x >= size - r) dx = x - (size - r) + 0.5f;

                    if (y < r) dy = r - y - 0.5f;
                    else if (y >= size - r) dy = y - (size - r) + 0.5f;

                    float alpha = 1f;
                    if (dx > 0f && dy > 0f)
                    {
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        alpha = Mathf.Clamp01(r - dist + 0.5f);
                    }
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(
                tex,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(radius, radius, radius, radius)
            );
        }

        private void CreateDividerLine(Transform parent, Color color)
        {
            RectTransform line = CreatePanel(parent, "Divider", color, raycast: false, rounded: false);
            line.sizeDelta = new Vector2(0f, 1.5f);
        }

        private static void AddSubtleBorder(GameObject go, Color borderColor)
        {
            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(1.5f, -1.5f);
        }

        private static void SetFill(RectTransform rt, float padX, float padY)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padX, padY); rt.offsetMax = new Vector2(-padX, -padY);
        }

        private RectTransform CreatePanel(Transform parent, string name, Color bgColor, bool raycast, bool rounded = true)
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
                img.pixelsPerUnitMultiplier = 2f;
            }
            return go.GetComponent<RectTransform>();
        }

        private Text CreateText(Transform parent, string name, string content, int fontSize, Color color, TextAnchor align, bool raycast)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Text txt = go.AddComponent<Text>();
            txt.font = (fontSize >= 18 && headingFont != null) ? headingFont : uiFont;
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

        private Button CreateButton(Transform parent, string name, string label, Color bgColor, float height, UnityEngine.Events.UnityAction onClick, Color? textColor = null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, height);

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight = height; le.preferredHeight = height;

            Image img = go.AddComponent<Image>();
            img.color = bgColor;
            if (roundedSprite != null)
            {
                img.sprite = roundedSprite;
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 2f;
            }

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.94f, 0.92f, 0.89f, 1f);
            cb.pressedColor = new Color(0.84f, 0.81f, 0.77f, 1f);
            cb.fadeDuration = 0.08f;
            btn.colors = cb;
            btn.onClick.AddListener(onClick);

            GameObject txtGo = new GameObject("Label");
            txtGo.transform.SetParent(go.transform, false);
            SetFill(txtGo.AddComponent<RectTransform>(), 10f, 2f);

            Text txt = txtGo.AddComponent<Text>();
            txt.font = uiFont;
            txt.text = label;
            txt.fontSize = 13;
            txt.color = textColor ?? PaperWhite;
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
}
