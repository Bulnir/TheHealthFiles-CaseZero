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
        private Sprite roundedSprite;
        private Sprite pillSprite;
        private readonly Dictionary<string, Texture2D> portraits = new Dictionary<string, Texture2D>();

        // Warm Editorial Paper Dossier Palette (60% Paper Base, 30% Manila Cards, 10% Burgundy/Sage Accents)
        private static readonly Color PaperBase      = new Color(0.95f, 0.925f, 0.882f, 0.99f);
        private static readonly Color PaperCard      = new Color(0.905f, 0.875f, 0.812f, 1f);
        private static readonly Color PaperBtn       = new Color(0.855f, 0.815f, 0.738f, 1f);
        private static readonly Color PaperBorder    = new Color(0.73f, 0.67f, 0.56f, 0.95f);
        private static readonly Color InkPrimary     = new Color(0.13f, 0.15f, 0.18f, 1f);
        private static readonly Color InkMuted       = new Color(0.36f, 0.38f, 0.41f, 1f);
        private static readonly Color AccentBurgundy = new Color(0.50f, 0.16f, 0.17f, 1f);
        private static readonly Color AccentSage     = new Color(0.18f, 0.39f, 0.32f, 1f);
        private static readonly Color AccentOchre    = new Color(0.60f, 0.42f, 0.14f, 1f);
        private static readonly Color PaperWhite     = new Color(0.97f, 0.95f, 0.91f, 1f);

        // Minimal HUD elements
        private Text objectiveText;
        private Text evidenceCountBtnText;
        private Text muteBtnText;
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
        private Text dhliSubtitleText;
        private Image relBarFill;
        private Text relBarLabel;
        private Image privBarFill;
        private Text privBarLabel;
        private Image navBarFill;
        private Text navBarLabel;
        private Text dhliDetailText;

        // Modals
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

        // Pause & Options Menu (Esc or Top-Right Menu button)
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

        // Office Computer Desktop Modal (Always works when clicking Monitor!)
        private GameObject computerPanel;
        private GameObject pcNewsTabContent;
        private GameObject pcInspectorTabContent;
        private GameObject pcInspectorLockedNotice;
        private GameObject pcInspectorGameArea;
        private Text computerModeBannerText;
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
            roundedSprite = CreateRoundedSprite(128, 28);
            pillSprite = CreateRoundedSprite(64, 12);
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
                MissionManager.Instance.OnCase01Completed += OpenCase01CompleteModal;

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

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.OnAudioSettingsChanged += RefreshAudioUI;
                RefreshAudioUI();
            }

            if (modalBackdrop != null) modalBackdrop.SetActive(true);
            welcomePanel.SetActive(true);
            AnimateModalPopIn(welcomePanel);
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
                (computerPanel != null && computerPanel.activeSelf) ||
                (docModalPanel != null && docModalPanel.activeSelf) ||
                (inventoryDrawer != null && inventoryDrawer.activeSelf) ||
                (dhliDrawer != null && dhliDrawer.activeSelf);

            if (anyOverlayOpen)
            {
                SoundManager.Instance?.Play("ui_click", 0.45f);
                CloseAllModals();
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
                MissionManager.Instance.OnAlexComputerOpened -= OpenComputerDesktop;
                MissionManager.Instance.OnMailLogOpened -= OpenMailLogModal;
                MissionManager.Instance.OnManualOpened -= OpenManualModal;
                MissionManager.Instance.OnAlexLetterOpened -= OpenAlexLetterModal;
                MissionManager.Instance.OnObjectionSplash -= TriggerObjectionSplash;
                MissionManager.Instance.OnCase01Completed -= OpenCase01CompleteModal;
            }
            if (EvidenceManager.Instance != null) EvidenceManager.Instance.OnInventoryChanged -= RefreshInventoryUI;
            if (PersonalizationManager.Instance != null) PersonalizationManager.Instance.OnProfileUpdated -= RefreshDhliDrawer;
            if (OfficeRaycaster.Instance != null) OfficeRaycaster.Instance.OnHoverLabelChanged -= HandleHoverChanged;
            if (SoundManager.Instance != null) SoundManager.Instance.OnAudioSettingsChanged -= RefreshAudioUI;
        }

        private void CloseAllModals()
        {
            if (modalBackdrop != null) modalBackdrop.SetActive(false);
            welcomePanel.SetActive(false);
            if (onboardingPanel != null) onboardingPanel.SetActive(false);
            if (caseCompletePanel != null) caseCompletePanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            phonePanel.SetActive(false);
            computerPanel.SetActive(false);
            docModalPanel.SetActive(false);
            inventoryDrawer.SetActive(false);
            dhliDrawer.SetActive(false);
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
            rt.localScale = new Vector3(0.93f, 0.93f, 1f);
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                float s = Mathf.Lerp(0.93f, 1f, eased);
                rt.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            rt.localScale = Vector3.one;
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
                    $"<b>RANK:  <color=#7D2626>{rank.ToUpper()}</color></b>\n" +
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
                    ? "<b>CASE 01: ALEX</b>  •  <color=#2E5E4E>100% ✓</color>"
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
                    MissionState.Prologue_FindManual => "Take the Manual from the Bookshelf (4).",
                    MissionState.Alex_AnswerFirstCall => "Answer Alex Moretti's call.",
                    MissionState.Alex_FindHiddenLetter => "Check Mail Log → Open Drawer B-02.",
                    MissionState.Alex_InspectBlogAndPolicy => "Inspect the Blog on your PC (2).",
                    MissionState.Alex_ObjectionCall => "Call Alex back & hit OBJECTION!",
                    MissionState.Alex_PinPolicyClause => "Pin Section 4.2 on the Cork Board (6).",
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

        private void ClosePauseMenu()
        {
            SoundManager.Instance?.Play("ui_click", 0.45f);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (modalBackdrop != null) modalBackdrop.SetActive(false);
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

        private void HandleMissionChanged(MissionState state, string _)
        {
            if (objectiveText == null) return;
            objectiveText.text = state switch
            {
                MissionState.Prologue_SupervisorCall => "<color=#80D8FF>[1/7]</color>  Answer the ringing Office Phone",
                MissionState.Prologue_FindManual => "<color=#80D8FF>[2/7]</color>  Pick up Investigation Manual (Bookcase)",
                MissionState.Alex_AnswerFirstCall => "<color=#FFD54F>[3/7]</color>  Case 01: Answer Alex Moretti's call",
                MissionState.Alex_FindHiddenLetter => "<color=#FFD54F>[4/7]</color>  Check Mail Log → Open Cabinet Drawer B-02",
                MissionState.Alex_InspectBlogAndPolicy => "<color=#FFD54F>[5/7]</color>  Click Computer Monitor to inspect Blog",
                MissionState.Alex_ObjectionCall => "<color=#FF8A80>[6/7]</color>  Call Alex back & trigger OBJECTION!",
                MissionState.Alex_PinPolicyClause => "<color=#FFD54F>[7/7]</color>  Pin Section 4.2 on the Cork Evidence Board",
                MissionState.Sofia_Ready => "<color=#69F0AE>[CASE 01 ✓]</color>  Answer Sofia (WP4) or inspect DHLI Profile",
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

            if (dhliSubtitleText != null)
            {
                dhliSubtitleText.text = pm.onboardingSummary;
            }

            UpdateSkillBar(relBarFill, relBarLabel, "Evaluating Reliability", pm.reliabilityScore);
            UpdateSkillBar(privBarFill, privBarLabel, "Protecting Privacy", pm.privacyScore);
            UpdateSkillBar(navBarFill, navBarLabel, "Digital Navigation", pm.navigationScore);

            dhliDetailText.text =
                $"Mistakes tracked:  <b>R:{pm.reliabilityMistakes}</b>  |  <b>P:{pm.privacyMistakes}</b>  |  <b>N:{pm.navigationMistakes}</b>\n" +
                $"Focus area: <b>{pm.GetWeakestDomain()}</b>\n" +
                $"<i>Adaptive hints tighten automatically when a domain is Low (&lt;45) or after repeated mistakes.</i>";
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
            if (score < 45) return new Color(0.68f, 0.22f, 0.22f, 1f);      // Low (Muted Crimson)
            if (score < 70) return new Color(0.68f, 0.48f, 0.14f, 1f);      // Intermediate (Warm Ochre)
            return new Color(0.18f, 0.48f, 0.34f, 1f);                      // High (Archival Sage)
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
            currentFullDialogue = fullText;
            isTypingDialogue = true;
            if (phoneSkipHintText != null) phoneSkipHintText.gameObject.SetActive(true);

            target.text = "";
            for (int i = 0; i < fullText.Length; i++)
            {
                target.text += fullText[i];
                if (i % 2 == 0) yield return new WaitForSeconds(0.008f);
            }

            isTypingDialogue = false;
            if (phoneSkipHintText != null) phoneSkipHintText.gameObject.SetActive(false);
        }

        private void CompleteTypewriterImmediately()
        {
            if (!isTypingDialogue) return;
            if (typewriterRoutine != null) StopCoroutine(typewriterRoutine);
            if (phoneBodyText != null) phoneBodyText.text = currentFullDialogue;
            isTypingDialogue = false;
            if (phoneSkipHintText != null) phoneSkipHintText.gameObject.SetActive(false);
            SoundManager.Instance?.Play("ui_click", 0.35f);
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
                    $"<b>INVESTIGATOR RANK:  <color=#7D2626>{rank.ToUpper()}</color></b>\n" +
                    $"•  Evaluating Reliability:  <b><color=#{relHex}>{pm.reliabilityScore}/100 [{PersonalizationManager.GetLevelLabel(pm.reliabilityScore)}]</color></b>\n" +
                    $"•  Protecting Privacy:      <b><color=#{privHex}>{pm.privacyScore}/100 [{PersonalizationManager.GetLevelLabel(pm.privacyScore)}]</color></b>\n" +
                    $"•  Digital Navigation:      <b><color=#{navHex}>{pm.navigationScore}/100 [{PersonalizationManager.GetLevelLabel(pm.navigationScore)}]</color></b>\n\n" +
                    $"Assigned Mode: <b>{pm.GetAssignedModeLabel()}</b>  •  Next Focus Area: <b>{pm.GetWeakestDomain()}</b>\n" +
                    "<color=#2E5E4E><i>The office phone is ringing again — Sofia Romano (Case 02, WP4) needs your help!</i></color>";
            }

            if (modalBackdrop != null) modalBackdrop.SetActive(true);
            caseCompletePanel.SetActive(true);
            AnimateModalPopIn(caseCompletePanel);
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

            if (computerModeBannerText != null && PersonalizationManager.Instance != null)
            {
                var pm = PersonalizationManager.Instance;
                if (pm.IsGuidedMode(DhliDomain.Reliability))
                {
                    computerModeBannerText.color = new Color(1f, 0.88f, 0.45f);
                    computerModeBannerText.text =
                        "<b>[GUIDED SUPPORT ACTIVE]</b> Check for: (1) Extreme headline, (2) Anonymous author, (3) Missing proof/sources.";
                }
                else if (pm.reliabilityScore >= 70)
                {
                    computerModeBannerText.color = new Color(0.45f, 0.95f, 0.65f);
                    computerModeBannerText.text =
                        "<b>[INDEPENDENT ANALYST MODE]</b> No extra hints — spot all 3 unsupported blog weaknesses on your own!";
                }
                else
                {
                    computerModeBannerText.color = new Color(0.65f, 0.88f, 1f);
                    computerModeBannerText.text =
                        "<b>[BALANCED MODE]</b> Compare the blog against Privacy Policy Section 4.2 to find the 3 unsupported claims.";
                }
            }
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
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // 1. Compact Top-Left Mission Pill (NON-BLOCKING raycast!)
            RectTransform objPill = CreatePanel(canvasGo.transform, "ObjectivePill", new Color(0.06f, 0.09f, 0.15f, 0.88f), raycast: false);
            AddSubtleBorder(objPill.gameObject, new Color(0.95f, 0.80f, 0.32f, 0.35f));
            objPill.anchorMin = new Vector2(0f, 1f);
            objPill.anchorMax = new Vector2(0f, 1f);
            objPill.pivot = new Vector2(0f, 1f);
            objPill.sizeDelta = new Vector2(560f, 38f);
            objPill.anchoredPosition = new Vector2(14f, -12f);
            objectiveText = CreateText(objPill, "ObjText", "Loading...", 15, new Color(1f, 0.93f, 0.50f), TextAnchor.MiddleLeft, false);
            objectiveText.fontStyle = FontStyle.Bold;
            SetFill(objectiveText.GetComponent<RectTransform>(), 14, 4);

            // 2. Top-Right Compact Bar (Evidence + DHLI Profile + Sound Toggle + Pause Menu)
            RectTransform topRight = CreatePanel(canvasGo.transform, "TopRightBtns", new Color(0, 0, 0, 0), raycast: false, rounded: false);
            topRight.anchorMin = new Vector2(1f, 1f);
            topRight.anchorMax = new Vector2(1f, 1f);
            topRight.pivot = new Vector2(1f, 1f);
            topRight.sizeDelta = new Vector2(436f, 38f);
            topRight.anchoredPosition = new Vector2(-14f, -12f);
            HorizontalLayoutGroup trLay = topRight.gameObject.AddComponent<HorizontalLayoutGroup>();
            trLay.spacing = 7;
            trLay.childControlWidth = true;
            trLay.childForceExpandWidth = true;
            trLay.childControlHeight = true;
            trLay.childForceExpandHeight = true;

            Button invBtn = CreateButton(topRight, "ToggleInvBtn", "Evidence (0)", new Color(0.14f, 0.30f, 0.48f, 0.94f), 36f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                bool next = !inventoryDrawer.activeSelf;
                dhliDrawer.SetActive(false);
                inventoryDrawer.SetActive(next);
            });
            invBtn.GetComponent<LayoutElement>().flexibleWidth = 1.3f;
            evidenceCountBtnText = invBtn.GetComponentInChildren<Text>();

            Button dhliBtn = CreateButton(topRight, "ToggleDhliBtn", "DHLI Profile", new Color(0.15f, 0.48f, 0.34f, 0.94f), 36f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                bool next = !dhliDrawer.activeSelf;
                inventoryDrawer.SetActive(false);
                dhliDrawer.SetActive(next);
                RefreshDhliDrawer();
            });
            dhliBtn.GetComponent<LayoutElement>().flexibleWidth = 1.25f;

            Button muteBtn = CreateButton(topRight, "ToggleMuteBtn", "SFX: ON", new Color(0.22f, 0.26f, 0.36f, 0.94f), 36f, () =>
            {
                SoundManager.Instance?.ToggleMute();
                if (SoundManager.Instance != null && !SoundManager.Instance.IsMuted)
                    SoundManager.Instance.Play("ui_click", 0.5f);
            });
            muteBtn.GetComponent<LayoutElement>().flexibleWidth = 0.95f;
            muteBtnText = muteBtn.GetComponentInChildren<Text>();

            Button menuBtn = CreateButton(topRight, "PauseMenuBtn", "Menu", new Color(0.32f, 0.24f, 0.42f, 0.94f), 36f, () =>
            {
                if (pausePanel != null && pausePanel.activeSelf) ClosePauseMenu();
                else OpenPauseMenu();
            });
            menuBtn.GetComponent<LayoutElement>().flexibleWidth = 0.85f;

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
            RectTransform ctrlHint = CreatePanel(canvasGo.transform, "CtrlHint", new Color(0, 0, 0, 0), raycast: false, rounded: false);
            ctrlHint.anchorMin = new Vector2(0f, 0f);
            ctrlHint.anchorMax = new Vector2(0f, 0f);
            ctrlHint.pivot = new Vector2(0f, 0f);
            ctrlHint.sizeDelta = new Vector2(360f, 24f);
            ctrlHint.anchoredPosition = new Vector2(14f, 14f);
            Text hintTxt = CreateText(ctrlHint, "HintTxt", "Right-Click Drag / WASD: Look  |  Esc: Pause Menu", 12, new Color(0.92f, 0.95f, 1f, 0.85f), TextAnchor.MiddleLeft, false);
            SetFill(hintTxt.GetComponent<RectTransform>(), 0, 0);

            // 6. Collapsible Evidence Drawer (hidden by default!)
            BuildEvidenceDrawer(canvasGo.transform);
            BuildDhliDrawer(canvasGo.transform);

            // 7. Fullscreen Warm Espresso Backdrop Dimmer (behind Paper Dossier modals)
            RectTransform backdropRt = CreatePanel(canvasGo.transform, "ModalBackdrop", new Color(0.07f, 0.06f, 0.05f, 0.72f), raycast: true, rounded: false);
            SetFill(backdropRt, 0f, 0f);
            modalBackdrop = backdropRt.gameObject;
            modalBackdrop.SetActive(false);

            // 8. Modals
            BuildWelcomeModal(canvasGo.transform);
            BuildOnboardingModal(canvasGo.transform);
            BuildCaseCompleteModal(canvasGo.transform);
            BuildPhoneModal(canvasGo.transform);
            BuildComputerDesktopModal(canvasGo.transform);
            BuildDocModal(canvasGo.transform);
            BuildPauseModal(canvasGo.transform);
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
            RectTransform panel = CreatePanel(parent, "DhliDrawer", PaperBase, raycast: true);
            AddSubtleBorder(panel.gameObject, PaperBorder);
            panel.anchorMin = new Vector2(1f, 1f);
            panel.anchorMax = new Vector2(1f, 1f);
            panel.pivot = new Vector2(1f, 1f);
            panel.sizeDelta = new Vector2(360f, 345f);
            panel.anchoredPosition = new Vector2(-14f, -58f);

            VerticalLayoutGroup lay = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(16, 16, 14, 14);
            lay.spacing = 8;
            lay.childControlHeight = false;
            lay.childForceExpandHeight = false;

            Text hdr = CreateText(panel, "DhliHdr", "ADAPTIVE DHLI PROFILE", 15, AccentBurgundy, TextAnchor.MiddleLeft, false);
            hdr.fontStyle = FontStyle.Bold;
            hdr.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 22f);

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
            dBtnLay.spacing = 8;
            dBtnLay.childControlWidth = true;
            dBtnLay.childForceExpandWidth = true;
            dBtnLay.childControlHeight = true;
            dBtnLay.childForceExpandHeight = true;

            Button recalBtn = CreateButton(dhliBtnRow.transform, "RecalibrateBtn", "Recalibrate Badge", AccentBurgundy, 32f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                returnToPauseAfterOnboarding = false;
                dhliDrawer.SetActive(false);
                RefreshOnboardingButtons();
                if (modalBackdrop != null) modalBackdrop.SetActive(true);
                onboardingPanel.SetActive(true);
                AnimateModalPopIn(onboardingPanel);
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
            rLay.spacing = 4;
            rLay.childControlHeight = false;
            rLay.childForceExpandHeight = false;

            labelText = CreateText(row.transform, "Lbl", "", 12, InkPrimary, TextAnchor.MiddleLeft, false);
            labelText.GetComponent<RectTransform>().sizeDelta = new Vector2(328f, 18f);

            RectTransform track = CreatePanel(row.transform, "Track", PaperBtn, raycast: false, rounded: false);
            track.sizeDelta = new Vector2(328f, 12f);
            Image trackImg = track.GetComponent<Image>();
            if (pillSprite != null) { trackImg.sprite = pillSprite; trackImg.type = Image.Type.Sliced; trackImg.pixelsPerUnitMultiplier = 2f; }

            RectTransform fillRt = CreatePanel(track, "Fill", AccentOchre, raycast: false, rounded: false);
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(0.5f, 1f);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            fillImage = fillRt.GetComponent<Image>();
            if (pillSprite != null) { fillImage.sprite = pillSprite; fillImage.type = Image.Type.Sliced; fillImage.pixelsPerUnitMultiplier = 2f; }
        }

        private void BuildWelcomeModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "WelcomeModal", PaperBase, raycast: true);
            AddSubtleBorder(modal.gameObject, PaperBorder);
            modal.anchorMin = new Vector2(0.21f, 0.19f);
            modal.anchorMax = new Vector2(0.79f, 0.81f);
            modal.offsetMin = Vector2.zero;
            modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup lay = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(32, 32, 24, 24);
            lay.spacing = 11;
            lay.childControlHeight = false;
            lay.childForceExpandHeight = false;

            RectTransform badgePill = CreatePanel(modal, "OfficeTag", PaperCard, raycast: false);
            badgePill.sizeDelta = new Vector2(0f, 24f);
            Text tagTxt = CreateText(badgePill, "Tag", "DIGITAL HEALTH INVESTIGATION OFFICE  •  CASE FILE #00", 11, AccentBurgundy, TextAnchor.MiddleCenter, false);
            tagTxt.fontStyle = FontStyle.Bold;
            SetFill(tagTxt.GetComponent<RectTransform>(), 8f, 2f);

            Text title = CreateText(modal, "T", "THE HEALTH FILES: CASE ZERO", 24, InkPrimary, TextAnchor.MiddleCenter, false);
            title.fontStyle = FontStyle.Bold;
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 34f);

            CreateDividerLine(modal, PaperBorder);

            Text body = CreateText(modal, "B",
                "Investigate the viral health app <b>VitaPulse</b> and uncover the truth behind the claims.\n\n" +
                "•  <b>Right-Click Drag / WASD:</b> Look around\n" +
                "•  <b>Left-Click:</b> Interact  •  <b>Esc:</b> Menu",
                14, InkPrimary, TextAnchor.UpperLeft, false);
            body.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 118f);

            Button onbBtn = CreateButton(modal, "OnboardingBtn", "Personalize Profile", AccentBurgundy, 44f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.6f);
                returnToPauseAfterOnboarding = false;
                welcomePanel.SetActive(false);
                RefreshOnboardingButtons();
                if (modalBackdrop != null) modalBackdrop.SetActive(true);
                onboardingPanel.SetActive(true);
                AnimateModalPopIn(onboardingPanel);
            }, PaperWhite);
            onbBtn.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;

            CreateButton(modal, "StartBtn", "Play Now", PaperBtn, 40f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.6f);
                welcomePanel.SetActive(false);
                if (modalBackdrop != null) modalBackdrop.SetActive(false);
            }, InkPrimary);

            welcomePanel = modal.gameObject;
            welcomePanel.SetActive(false);
        }

        private void BuildOnboardingModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "OnboardingModal", PaperBase, raycast: true);
            AddSubtleBorder(modal.gameObject, PaperBorder);
            modal.anchorMin = new Vector2(0.12f, 0.06f);
            modal.anchorMax = new Vector2(0.88f, 0.94f);
            modal.offsetMin = Vector2.zero;
            modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup lay = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(28, 28, 18, 18);
            lay.spacing = 9;
            lay.childControlHeight = false;
            lay.childForceExpandHeight = false;

            RectTransform topTag = CreatePanel(modal, "OnbTopTag", PaperCard, raycast: false);
            topTag.sizeDelta = new Vector2(0f, 22f);
            Text topTagTxt = CreateText(topTag, "Tag", "DAY 1 INTAKE DOSSIER  •  DHLI PERSONALIZATION", 11, AccentBurgundy, TextAnchor.MiddleCenter, false);
            topTagTxt.fontStyle = FontStyle.Bold;
            SetFill(topTagTxt.GetComponent<RectTransform>(), 8f, 2f);

            Text title = CreateText(modal, "OnbTitle", "INVESTIGATOR BADGE SETUP", 22, InkPrimary, TextAnchor.MiddleCenter, false);
            title.fontStyle = FontStyle.Bold;
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 28f);

            Text sub = CreateText(modal, "OnbSub",
                "Select the habits closest to yours — your case file calibrates hint support to your style.",
                13, InkMuted, TextAnchor.MiddleCenter, false);
            sub.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 18f);

            CreateDividerLine(modal, PaperBorder);

            string[] questions =
            {
                "<color=#7D2626>01  RELIABILITY</color>  —  When you see a viral health claim online, what is your first reaction?",
                "<color=#7D2626>02  PRIVACY</color>  —  When a new health app asks for Contacts, Location & Microphone access:",
                "<color=#7D2626>03  NAVIGATION</color>  —  Finding official medical guidance on a hospital or health portal feels:"
            };

            for (int q = 0; q < 3; q++)
            {
                int qIdx = q;
                RectTransform card = CreatePanel(modal, $"Q{q}_Card", PaperCard, raycast: false);
                AddSubtleBorder(card.gameObject, PaperBorder);
                card.sizeDelta = new Vector2(0f, 88f);

                HorizontalLayoutGroup cardRow = card.gameObject.AddComponent<HorizontalLayoutGroup>();
                cardRow.padding = new RectOffset(0, 14, 0, 0);
                cardRow.spacing = 12;
                cardRow.childControlWidth = true;
                cardRow.childForceExpandWidth = false;
                cardRow.childControlHeight = true;
                cardRow.childForceExpandHeight = true;

                // Subtle archival burgundy left rule (uniform, no rainbow!)
                RectTransform stripe = CreatePanel(card, "Accent", AccentBurgundy, raycast: false, rounded: false);
                LayoutElement stripeLe = stripe.gameObject.AddComponent<LayoutElement>();
                stripeLe.minWidth = 4f;
                stripeLe.preferredWidth = 4f;
                stripeLe.flexibleWidth = 0f;

                GameObject contentObj = new GameObject("Content");
                contentObj.transform.SetParent(card, false);
                contentObj.AddComponent<RectTransform>();
                LayoutElement contentLe = contentObj.AddComponent<LayoutElement>();
                contentLe.minWidth = 200f;
                contentLe.flexibleWidth = 1f;

                VerticalLayoutGroup cardLay = contentObj.AddComponent<VerticalLayoutGroup>();
                cardLay.padding = new RectOffset(2, 2, 10, 10);
                cardLay.spacing = 8;
                cardLay.childControlWidth = true;
                cardLay.childForceExpandWidth = true;
                cardLay.childControlHeight = false;
                cardLay.childForceExpandHeight = false;

                Text qTxt = CreateText(contentObj.transform, $"Q{q}_Lbl", questions[q], 13, InkPrimary, TextAnchor.MiddleLeft, false);
                qTxt.fontStyle = FontStyle.Bold;
                qTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 22f);

                GameObject optRow = new GameObject($"Q{q}_Opts");
                optRow.transform.SetParent(contentObj.transform, false);
                optRow.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 38f);
                HorizontalLayoutGroup oLay = optRow.AddComponent<HorizontalLayoutGroup>();
                oLay.spacing = 10;
                oLay.childControlWidth = true;
                oLay.childForceExpandWidth = true;
                oLay.childControlHeight = true;
                oLay.childForceExpandHeight = true;

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

            // Live Assigned Mode Preview Card
            RectTransform previewBox = CreatePanel(modal, "ModePreview", PaperCard, raycast: false);
            onboardingPreviewOutline = previewBox.gameObject.AddComponent<Outline>();
            onboardingPreviewOutline.effectColor = AccentSage;
            onboardingPreviewOutline.effectDistance = new Vector2(1.5f, -1.5f);
            previewBox.sizeDelta = new Vector2(0f, 54f);
            onboardingPreviewText = CreateText(previewBox, "PreviewTxt", "", 13, InkPrimary, TextAnchor.MiddleCenter, false);
            SetFill(onboardingPreviewText.GetComponent<RectTransform>(), 14f, 4f);

            GameObject actionRow = new GameObject("OnbActions");
            actionRow.transform.SetParent(modal, false);
            actionRow.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 46f);
            HorizontalLayoutGroup aLay = actionRow.AddComponent<HorizontalLayoutGroup>();
            aLay.spacing = 10;
            aLay.childControlWidth = true;
            aLay.childForceExpandWidth = true;
            aLay.childControlHeight = true;
            aLay.childForceExpandHeight = true;

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
                }
            }, InkPrimary);
            onboardingBackBtnText = backBtn.GetComponentInChildren<Text>();
            backBtn.GetComponent<LayoutElement>().flexibleWidth = 1.1f;

            Button saveBtn = CreateButton(actionRow.transform, "SaveOnbBtn", "Issue My Badge & Start Day 1  →", AccentBurgundy, 44f, () =>
            {
                SoundManager.Instance?.Play("evidence_unlock", 0.7f);
                PersonalizationManager.Instance?.ApplyOnboardingSelfCheck(onboardingAnswers[0], onboardingAnswers[1], onboardingAnswers[2]);
                RefreshDhliDrawer();
                string mode = PersonalizationManager.Instance != null ? PersonalizationManager.Instance.GetAssignedModeLabel() : "Investigator";
                ShowToast($"Badge issued: {mode}! Hints calibrated for {PersonalizationManager.Instance?.GetWeakestDomain()}.");

                if (returnToPauseAfterOnboarding)
                {
                    OpenPauseMenu();
                }
                else
                {
                    onboardingPanel.SetActive(false);
                    if (modalBackdrop != null) modalBackdrop.SetActive(false);
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
                    {
                        onboardingOptionImages[q, opt].color = isSelected ? AccentSage : PaperBtn;
                    }
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
                onboardingPreviewText.text = $"Mode:  <b><color=#7D2626>Guided</color></b>  •  Extra hints enabled (Focus: <b>{focus}</b>)";
                if (onboardingPreviewOutline != null) onboardingPreviewOutline.effectColor = AccentBurgundy;
            }
            else if (allHigh)
            {
                onboardingPreviewText.text = "Mode:  <b><color=#2E5E4E>Independent</color></b>  •  Minimal hints";
                if (onboardingPreviewOutline != null) onboardingPreviewOutline.effectColor = AccentSage;
            }
            else
            {
                onboardingPreviewText.text = "Mode:  <b><color=#9A6B22>Balanced</color></b>  •  Standard hints";
                if (onboardingPreviewOutline != null) onboardingPreviewOutline.effectColor = AccentOchre;
            }
        }

        private void BuildCaseCompleteModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "CaseCompleteModal", PaperBase, raycast: true);
            AddSubtleBorder(modal.gameObject, PaperBorder);
            modal.anchorMin = new Vector2(0.16f, 0.14f);
            modal.anchorMax = new Vector2(0.84f, 0.86f);
            modal.offsetMin = Vector2.zero;
            modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup lay = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(30, 30, 22, 22);
            lay.spacing = 10;
            lay.childControlHeight = false;
            lay.childForceExpandHeight = false;

            RectTransform topTag = CreatePanel(modal, "CCTag", PaperCard, raycast: false);
            topTag.sizeDelta = new Vector2(0f, 24f);
            Text tagTxt = CreateText(topTag, "Tag", "CASE 01 CLOSED  •  EVIDENCE PINNED TO BOARD", 11, AccentSage, TextAnchor.MiddleCenter, false);
            tagTxt.fontStyle = FontStyle.Bold;
            SetFill(tagTxt.GetComponent<RectTransform>(), 8f, 2f);

            Text title = CreateText(modal, "CCTitle", "CASE 01 COMPLETE: ALEX MORETTI", 22, InkPrimary, TextAnchor.MiddleCenter, false);
            title.fontStyle = FontStyle.Bold;
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 30f);

            CreateDividerLine(modal, PaperBorder);

            GameObject bodyRow = new GameObject("CCRow");
            bodyRow.transform.SetParent(modal, false);
            bodyRow.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 255f);
            HorizontalLayoutGroup rLay = bodyRow.AddComponent<HorizontalLayoutGroup>();
            rLay.spacing = 18;
            rLay.childControlWidth = true;
            rLay.childForceExpandWidth = false;
            rLay.childControlHeight = false;
            rLay.childForceExpandHeight = false;

            RectTransform pFrame = CreatePanel(bodyRow.transform, "CCPortraitFrame", PaperCard, raycast: false);
            AddSubtleBorder(pFrame.gameObject, PaperBorder);
            pFrame.sizeDelta = new Vector2(144f, 144f);
            LayoutElement pLe = pFrame.gameObject.AddComponent<LayoutElement>();
            pLe.minWidth = 144f;
            pLe.preferredWidth = 144f;
            pLe.flexibleWidth = 0f;

            GameObject raw = new GameObject("Raw");
            raw.transform.SetParent(pFrame, false);
            SetFill(raw.AddComponent<RectTransform>(), 5f, 5f);
            caseCompletePortrait = raw.AddComponent<RawImage>();

            caseCompleteSummaryText = CreateText(bodyRow.transform, "CCSummary", "", 13, InkPrimary, TextAnchor.UpperLeft, false);
            caseCompleteSummaryText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 250f);
            LayoutElement sLe = caseCompleteSummaryText.gameObject.AddComponent<LayoutElement>();
            sLe.minWidth = 240f;
            sLe.flexibleWidth = 1f;

            Button contBtn = CreateButton(modal, "ContinueCase02Btn", "Continue", AccentSage, 44f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.6f);
                caseCompletePanel.SetActive(false);
                if (modalBackdrop != null) modalBackdrop.SetActive(false);
            }, PaperWhite);
            contBtn.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;

            caseCompletePanel = modal.gameObject;
            caseCompletePanel.SetActive(false);
        }

        private void BuildPhoneModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "PhoneModal", new Color(0.06f, 0.09f, 0.15f, 0.97f), raycast: true);
            AddSubtleBorder(modal.gameObject, new Color(0.95f, 0.80f, 0.32f, 0.65f));
            modal.anchorMin = new Vector2(0.16f, 0.15f);
            modal.anchorMax = new Vector2(0.84f, 0.85f);
            modal.offsetMin = Vector2.zero;
            modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup lay = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            lay.padding = new RectOffset(22, 22, 18, 18);
            lay.spacing = 8;
            lay.childControlHeight = false;
            lay.childForceExpandHeight = false;

            phoneSpeakerText = CreateText(modal, "Speaker", "TELEPHONE", 19, new Color(1f, 0.88f, 0.38f), TextAnchor.MiddleLeft, false);
            phoneSpeakerText.fontStyle = FontStyle.Bold;
            phoneSpeakerText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 28f);

            GameObject row = new GameObject("Row");
            row.transform.SetParent(modal, false);
            row.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 146f);
            HorizontalLayoutGroup rLay = row.AddComponent<HorizontalLayoutGroup>();
            rLay.spacing = 16;
            rLay.childControlWidth = true;
            rLay.childForceExpandWidth = false;
            rLay.childControlHeight = false;
            rLay.childForceExpandHeight = false;

            RectTransform frame = CreatePanel(row.transform, "PortraitFrame", new Color(0.22f, 0.32f, 0.46f, 1f), raycast: false);
            frame.sizeDelta = new Vector2(134f, 134f);
            LayoutElement fLe = frame.gameObject.AddComponent<LayoutElement>();
            fLe.minWidth = 134f;
            fLe.preferredWidth = 134f;
            fLe.flexibleWidth = 0f;
            GameObject raw = new GameObject("Raw");
            raw.transform.SetParent(frame, false);
            SetFill(raw.AddComponent<RectTransform>(), 4, 4);
            phonePortraitImage = raw.AddComponent<RawImage>();
            phonePortraitFrame = frame.gameObject;

            // Clickable dialogue box (click anywhere on dialogue to skip typewriter instantly!)
            RectTransform bodyBox = CreatePanel(row.transform, "DialogueClickBox", new Color(0.09f, 0.13f, 0.21f, 0.85f), raycast: true);
            bodyBox.sizeDelta = new Vector2(0f, 142f);
            LayoutElement bLe = bodyBox.gameObject.AddComponent<LayoutElement>();
            bLe.minWidth = 240f;
            bLe.flexibleWidth = 1f;
            Button skipClickBtn = bodyBox.gameObject.AddComponent<Button>();
            skipClickBtn.targetGraphic = bodyBox.GetComponent<Image>();
            skipClickBtn.onClick.AddListener(CompleteTypewriterImmediately);

            phoneBodyText = CreateText(bodyBox, "Body", "", 15, Color.white, TextAnchor.UpperLeft, false);
            SetFill(phoneBodyText.GetComponent<RectTransform>(), 12f, 10f);

            phoneSkipHintText = CreateText(bodyBox, "SkipHint", "Click text to reveal instantly »", 11, new Color(0.68f, 0.82f, 0.96f, 0.85f), TextAnchor.LowerRight, false);
            phoneSkipHintText.fontStyle = FontStyle.Italic;
            SetFill(phoneSkipHintText.GetComponent<RectTransform>(), 10f, 6f);
            phoneSkipHintText.gameObject.SetActive(false);

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
            rLay.spacing = 10;
            rLay.childControlHeight = false;
            rLay.childForceExpandHeight = false;

            Text polTxt = CreateText(rightCol, "PolTxt",
                "<b>VITAPULSE PRIVACY POLICY — SECTION 4.2</b>\n\n" +
                "\"Some user information may be shared with third-party partners to operate and improve services.\"\n\n" +
                "<b>Investigator Check:</b>\n" +
                "• This clause raises a real privacy question.\n" +
                "• However, it does NOT prove a secret sale of ALL medical records.\n\n" +
                "<i>Click ONLY the 3 unsupported weaknesses in the blog on the left!</i>",
                14, new Color(0.88f, 0.96f, 1f), TextAnchor.UpperLeft, false);
            polTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 240f);

            RectTransform modeBox = CreatePanel(rightCol, "ModeHintBox", new Color(0.07f, 0.11f, 0.17f, 0.95f), raycast: false);
            AddSubtleBorder(modeBox.gameObject, new Color(0.32f, 0.62f, 0.80f, 0.6f));
            modeBox.sizeDelta = new Vector2(0f, 72f);
            computerModeBannerText = CreateText(modeBox, "ModeHintTxt", "", 12, new Color(0.65f, 0.88f, 1f), TextAnchor.MiddleLeft, false);
            SetFill(computerModeBannerText.GetComponent<RectTransform>(), 10f, 6f);

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

        private void BuildPauseModal(Transform parent)
        {
            RectTransform modal = CreatePanel(parent, "PauseModal", PaperBase, raycast: true);
            AddSubtleBorder(modal.gameObject, PaperBorder);
            modal.anchorMin = new Vector2(0.12f, 0.12f);
            modal.anchorMax = new Vector2(0.88f, 0.88f);
            modal.offsetMin = Vector2.zero;
            modal.offsetMax = Vector2.zero;

            VerticalLayoutGroup rootLay = modal.gameObject.AddComponent<VerticalLayoutGroup>();
            rootLay.padding = new RectOffset(28, 28, 20, 20);
            rootLay.spacing = 10;
            rootLay.childControlHeight = false;
            rootLay.childForceExpandHeight = false;

            RectTransform topTag = CreatePanel(modal, "PauseTag", PaperCard, raycast: false);
            topTag.sizeDelta = new Vector2(0f, 22f);
            Text tagTxt = CreateText(topTag, "Tag", "CASE ZERO  •  PAUSED", 11, AccentBurgundy, TextAnchor.MiddleCenter, false);
            tagTxt.fontStyle = FontStyle.Bold;
            SetFill(tagTxt.GetComponent<RectTransform>(), 8f, 2f);

            Text title = CreateText(modal, "PauseTitle", "MENU", 22, InkPrimary, TextAnchor.MiddleCenter, false);
            title.fontStyle = FontStyle.Bold;
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 28f);

            CreateDividerLine(modal, PaperBorder);

            // 2-Column Body: Left = Menu / Options, Right = Live Case Dossier
            GameObject bodyCols = new GameObject("PauseCols");
            bodyCols.transform.SetParent(modal, false);
            bodyCols.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 395f);
            HorizontalLayoutGroup colsLay = bodyCols.AddComponent<HorizontalLayoutGroup>();
            colsLay.spacing = 18;
            colsLay.childControlWidth = true;
            colsLay.childForceExpandWidth = false;
            colsLay.childControlHeight = true;
            colsLay.childForceExpandHeight = true;

            // LEFT COLUMN (Actions / Options)
            GameObject leftCol = new GameObject("LeftCol");
            leftCol.transform.SetParent(bodyCols.transform, false);
            leftCol.AddComponent<RectTransform>();
            LayoutElement leftLe = leftCol.AddComponent<LayoutElement>();
            leftLe.minWidth = 260f;
            leftLe.flexibleWidth = 1f;

            // VIEW 1: Main Pause Actions (short, crisp labels!)
            GameObject mainView = new GameObject("PauseMainView");
            mainView.transform.SetParent(leftCol.transform, false);
            SetFill(mainView.AddComponent<RectTransform>(), 0f, 0f);
            VerticalLayoutGroup mLay = mainView.AddComponent<VerticalLayoutGroup>();
            mLay.padding = new RectOffset(0, 0, 6, 0);
            mLay.spacing = 11;
            mLay.childControlWidth = true;
            mLay.childForceExpandWidth = true;
            mLay.childControlHeight = false;
            mLay.childForceExpandHeight = false;

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
            }, InkPrimary);

            CreateButton(mainView.transform, "RestartBtn", "↻  Restart", PaperBtn, 44f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.5f);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }, InkPrimary);

            CreateButton(mainView.transform, "QuitBtn", "✕  Quit", PaperCard, 44f, QuitGame, AccentBurgundy);

            Text pauseFoot = CreateText(mainView.transform, "PauseFoot",
                "<i>Esc: Back / Close</i>",
                12, InkMuted, TextAnchor.MiddleCenter, false);
            pauseFoot.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 24f);

            pauseMainView = mainView;

            // VIEW 2: Options, Audio & Controls (swaps inside Left Column, always has Back to Menu!)
            GameObject optView = new GameObject("PauseOptionsView");
            optView.transform.SetParent(leftCol.transform, false);
            SetFill(optView.AddComponent<RectTransform>(), 0f, 0f);
            VerticalLayoutGroup oLay = optView.AddComponent<VerticalLayoutGroup>();
            oLay.padding = new RectOffset(0, 0, 6, 0);
            oLay.spacing = 10;
            oLay.childControlWidth = true;
            oLay.childForceExpandWidth = true;
            oLay.childControlHeight = false;
            oLay.childForceExpandHeight = false;

            optionsVolumeText = CreateText(optView.transform, "VolLbl", "Master Volume:  <b>100%</b>", 14, InkPrimary, TextAnchor.MiddleLeft, false);
            optionsVolumeText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 22f);

            GameObject sliderGo = new GameObject("VolumeSlider");
            sliderGo.transform.SetParent(optView.transform, false);
            sliderGo.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 26f);

            RectTransform bgRt = CreatePanel(sliderGo.transform, "Background", PaperBtn, raycast: false, rounded: false);
            bgRt.anchorMin = new Vector2(0f, 0.25f);
            bgRt.anchorMax = new Vector2(1f, 0.75f);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            if (pillSprite != null) { bgRt.GetComponent<Image>().sprite = pillSprite; bgRt.GetComponent<Image>().type = Image.Type.Sliced; bgRt.GetComponent<Image>().pixelsPerUnitMultiplier = 2f; }

            GameObject fillArea = new GameObject("FillArea");
            fillArea.transform.SetParent(sliderGo.transform, false);
            RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
            fillAreaRt.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRt.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRt.offsetMin = Vector2.zero;
            fillAreaRt.offsetMax = Vector2.zero;

            RectTransform fillRt = CreatePanel(fillArea.transform, "Fill", AccentBurgundy, raycast: false, rounded: false);
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
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
            optionsVolumeSlider.minValue = 0f;
            optionsVolumeSlider.maxValue = 1f;
            optionsVolumeSlider.value = SoundManager.Instance != null ? SoundManager.Instance.MasterVolume : 1f;
            optionsVolumeSlider.onValueChanged.AddListener(v => SoundManager.Instance?.SetMasterVolume(v));

            Button optMuteBtn = CreateButton(optView.transform, "OptMuteBtn", "Audio: ON (Click to Mute)", PaperBtn, 38f, () =>
            {
                SoundManager.Instance?.ToggleMute();
                if (SoundManager.Instance != null && !SoundManager.Instance.IsMuted)
                    SoundManager.Instance.Play("ui_click", 0.5f);
            }, InkPrimary);
            optionsMuteBtnText = optMuteBtn.GetComponentInChildren<Text>();

            Button fsBtn = CreateButton(optView.transform, "OptFullscreenBtn", "Display Mode: Fullscreen", PaperBtn, 38f, () =>
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
                "•  <b>Drag / WASD:</b> Look\n" +
                "•  <b>1 – 6:</b> Stations  •  <b>Esc:</b> Menu",
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

            // RIGHT COLUMN: Live Case Dossier & Progress
            RectTransform rightCol = CreatePanel(bodyCols.transform, "DossierCol", PaperCard, raycast: false);
            AddSubtleBorder(rightCol.gameObject, PaperBorder);
            LayoutElement rightLe = rightCol.gameObject.AddComponent<LayoutElement>();
            rightLe.minWidth = 300f;
            rightLe.flexibleWidth = 1.15f;

            VerticalLayoutGroup rLay = rightCol.gameObject.AddComponent<VerticalLayoutGroup>();
            rLay.padding = new RectOffset(16, 16, 14, 14);
            rLay.spacing = 12;
            rLay.childControlWidth = true;
            rLay.childForceExpandWidth = true;
            rLay.childControlHeight = false;
            rLay.childForceExpandHeight = false;

            // Card 1: Investigator Rank Badge
            CreateDossierCard(rightCol, "RankCard", AccentBurgundy, 64f, out Transform rankContent);
            pauseRankText = CreateText(rankContent, "RankTxt", "<b>RANK: ROOKIE INVESTIGATOR</b>", 13, InkPrimary, TextAnchor.MiddleLeft, false);
            pauseRankText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 44f);

            // Card 2: Active Case Progress + Bar + Next Objective
            CreateDossierCard(rightCol, "ProgCard", AccentOchre, 148f, out Transform progContent);
            pauseProgressLabel = CreateText(progContent, "ProgLbl", "<b>CASE 01: ALEX MORETTI</b>", 13, InkPrimary, TextAnchor.MiddleLeft, false);
            pauseProgressLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 22f);

            RectTransform pTrack = CreatePanel(progContent, "ProgTrack", PaperBtn, raycast: false, rounded: false);
            pTrack.sizeDelta = new Vector2(0f, 12f);
            if (pillSprite != null) { pTrack.GetComponent<Image>().sprite = pillSprite; pTrack.GetComponent<Image>().type = Image.Type.Sliced; pTrack.GetComponent<Image>().pixelsPerUnitMultiplier = 2f; }

            RectTransform pFill = CreatePanel(pTrack, "ProgFill", AccentBurgundy, raycast: false, rounded: false);
            pFill.anchorMin = Vector2.zero;
            pFill.anchorMax = new Vector2(0.3f, 1f);
            pFill.offsetMin = Vector2.zero;
            pFill.offsetMax = Vector2.zero;
            pauseProgressFill = pFill.GetComponent<Image>();
            if (pillSprite != null) { pauseProgressFill.sprite = pillSprite; pauseProgressFill.type = Image.Type.Sliced; pauseProgressFill.pixelsPerUnitMultiplier = 2f; }

            pauseNextObjectiveText = CreateText(progContent, "NextObj", "", 12, InkPrimary, TextAnchor.UpperLeft, false);
            pauseNextObjectiveText.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 74f);

            // Card 3: Collected Evidence & Mini DHLI Scores
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
            row.padding = new RectOffset(0, 12, 0, 0);
            row.spacing = 10;
            row.childControlWidth = true;
            row.childForceExpandWidth = false;
            row.childControlHeight = true;
            row.childForceExpandHeight = true;

            RectTransform stripe = CreatePanel(card, "Stripe", accentColor, raycast: false, rounded: false);
            LayoutElement sLe = stripe.gameObject.AddComponent<LayoutElement>();
            sLe.minWidth = 4f;
            sLe.preferredWidth = 4f;
            sLe.flexibleWidth = 0f;

            GameObject content = new GameObject("Content");
            content.transform.SetParent(card, false);
            content.AddComponent<RectTransform>();
            LayoutElement cLe = content.AddComponent<LayoutElement>();
            cLe.minWidth = 180f;
            cLe.flexibleWidth = 1f;

            VerticalLayoutGroup cLay = content.AddComponent<VerticalLayoutGroup>();
            cLay.padding = new RectOffset(2, 2, 10, 10);
            cLay.spacing = 6;
            cLay.childControlWidth = true;
            cLay.childForceExpandWidth = true;
            cLay.childControlHeight = false;
            cLay.childForceExpandHeight = false;

            contentParent = content.transform;
            return card;
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
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padX, padY);
            rt.offsetMax = new Vector2(-padX, -padY);
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

        private Button CreateButton(Transform parent, string name, string label, Color bgColor, float height, UnityEngine.Events.UnityAction onClick, Color? textColor = null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, height);

            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;

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
