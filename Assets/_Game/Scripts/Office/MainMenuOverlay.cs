using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HealthFiles
{
    /// <summary>
    /// WP2: Editorial Aged-Parchment Main Menu (matching WP3's Esc Pause Menu aesthetic 1:1!)
    /// - Transparent over the live 3D Detective Office with smooth multi-angle cinematic camera shots
    /// - Plays chill lo-fi detective ambient music ('Audio/menu_chill') while muting in-game SFX/phone ring
    /// - Includes Settings (Volume slider, Mute, Graphics Quality Low/Med/High, Fullscreen) & Visual Keycap Controls!
    /// </summary>
    public class MainMenuOverlay : MonoBehaviour
    {
        public static MainMenuOverlay Instance { get; private set; }

        private static readonly Color PaperBase      = new Color(0.875f, 0.835f, 0.765f, 0.95f);
        private static readonly Color PaperCard      = new Color(0.824f, 0.773f, 0.686f, 0.97f);
        private static readonly Color PaperBorder    = new Color(0.56f,  0.49f,  0.39f,  0.95f);
        private static readonly Color InkPrimary     = new Color(0.11f,  0.12f,  0.14f,  1f);
        private static readonly Color InkMuted       = new Color(0.30f,  0.29f,  0.27f,  1f);
        private static readonly Color AccentBurgundy = new Color(0.49f,  0.15f,  0.16f,  1f);
        private static readonly Color AccentSage     = new Color(0.17f,  0.37f,  0.30f,  1f);
        private static readonly Color AccentOchre    = new Color(0.58f,  0.39f,  0.12f,  1f);
        private static readonly Color KeycapBg       = new Color(0.95f,  0.93f,  0.88f,  1f);

        private GameObject menuCanvasObj;
        private GameObject settingsCardObj;
        private GameObject controlsCardObj;
        private GameObject inGameHudCanvas;
        private AudioSource musicSource;
        private bool isMenuActive = true;
        private float shotTimer;
        private int currentShotIndex;

        // Live Settings UI refs
        private Text volValueText;
        private Text muteBtnLabel;
        private Text qualityBtnLabel;
        private Text fullscreenBtnLabel;
        private bool isMuted;
        private int graphicsTier = 2; // 0 = Low, 1 = Medium, 2 = High

        private readonly Vector3[] shotStarts =
        {
            new Vector3(-1.35f, 1.82f, -1.25f),
            new Vector3( 0.85f, 1.62f, -1.40f),
            new Vector3(-0.30f, 1.75f,  1.65f)
        };
        private readonly Vector3[] shotEnds =
        {
            new Vector3( 0.95f, 1.68f, -0.85f),
            new Vector3( 1.25f, 1.48f,  0.55f),
            new Vector3(-0.65f, 1.55f, -0.45f)
        };
        private readonly Vector3[] shotTargets =
        {
            new Vector3( 0.05f, 1.02f,  1.85f),
            new Vector3( 3.10f, 1.48f, -0.40f),
            new Vector3(-3.00f, 1.32f,  0.95f)
        };

        public static bool IsActive => Instance != null && Instance.isMenuActive;

        private void Awake()
        {
            Instance = this;
            BuildMainMenuUI();
            SetupMenuMusic();
        }

        private void Start()
        {
            isMenuActive = true;
            HideOtherHudCanvases(true);
            if (CameraViewpointController.Instance != null)
            {
                CameraViewpointController.Instance.IsCameraLockedByModal = true;
            }
        }

        private void HideOtherHudCanvases(bool hide)
        {
            if (inGameHudCanvas == null)
            {
                CaseZeroHUD hud = FindFirstObjectByType<CaseZeroHUD>();
                if (hud != null)
                {
                    Canvas[] all = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    foreach (Canvas c in all)
                    {
                        if (c.gameObject != menuCanvasObj && c.renderMode == RenderMode.ScreenSpaceOverlay)
                        {
                            inGameHudCanvas = c.gameObject;
                            break;
                        }
                    }
                }
            }
            if (inGameHudCanvas != null)
            {
                inGameHudCanvas.SetActive(!hide);
            }
        }

        private void SetupMenuMusic()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
            musicSource.volume = 0.42f;
            AudioClip chill = Resources.Load<AudioClip>("Audio/menu_chill");
            if (chill != null)
            {
                musicSource.clip = chill;
                musicSource.Play();
            }
        }

        private void Update()
        {
            if (!isMenuActive)
            {
                if (musicSource != null && musicSource.isPlaying)
                {
                    musicSource.volume = Mathf.MoveTowards(musicSource.volume, 0f, Time.deltaTime * 0.8f);
                    if (musicSource.volume <= 0.005f) musicSource.Stop();
                }
                return;
            }

            if (CameraViewpointController.Instance == null) return;
            if (inGameHudCanvas == null || inGameHudCanvas.activeSelf) HideOtherHudCanvases(true);

            const float shotDuration = 8.5f;
            shotTimer += Time.deltaTime;
            if (shotTimer >= shotDuration)
            {
                shotTimer = 0f;
                currentShotIndex = (currentShotIndex + 1) % shotStarts.Length;
            }

            float t = Mathf.SmoothStep(0f, 1f, shotTimer / shotDuration);
            Vector3 pos = Vector3.Lerp(shotStarts[currentShotIndex], shotEnds[currentShotIndex], t);
            Vector3 look = shotTargets[currentShotIndex];
            CameraViewpointController.Instance.SetTemporaryOverridePose(pos, Quaternion.LookRotation(look - pos));
        }

        public void StartGame()
        {
            if (!isMenuActive) return;
            isMenuActive = false;
            SoundManager.Instance?.Play("ui_click", 0.7f);
            if (menuCanvasObj != null) menuCanvasObj.SetActive(false);
            if (inGameHudCanvas != null) inGameHudCanvas.SetActive(true);
            if (CameraViewpointController.Instance != null)
            {
                CameraViewpointController.Instance.ClearTemporaryOverridePose();
                CameraViewpointController.Instance.MoveToViewpoint("Desk");
            }
        }

        private void ApplyGraphicsTier(int tier)
        {
            graphicsTier = tier % 3;
            int maxLevels = Mathf.Max(1, QualitySettings.names.Length);
            int targetIdx = graphicsTier == 0 ? 0 : (graphicsTier == 1 ? maxLevels / 2 : maxLevels - 1);
            QualitySettings.SetQualityLevel(targetIdx, true);

            if (Camera.main != null)
            {
                var urp = Camera.main.GetComponent<UniversalAdditionalCameraData>();
                if (urp != null)
                {
                    urp.antialiasing = graphicsTier == 0 ? AntialiasingMode.None : AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    urp.antialiasingQuality = graphicsTier == 2 ? AntialiasingQuality.High : AntialiasingQuality.Low;
                }
            }

            if (qualityBtnLabel != null)
            {
                string name = graphicsTier == 0 ? "LOW (Performance)" : (graphicsTier == 1 ? "MEDIUM (Balanced)" : "HIGH (Crisp SMAA)");
                qualityBtnLabel.text = "Graphics Quality:  " + name;
            }
        }

        private void BuildMainMenuUI()
        {
            Font fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Font headingFont = Font.CreateDynamicFontFromOSFont(new[] { "Georgia", "Cambria", "Times New Roman" }, 28) ?? fallback;
            Font bodyFont = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Calibri", "Arial" }, 14) ?? fallback;
            Font monoFont = Font.CreateDynamicFontFromOSFont(new[] { "Consolas", "Courier New", "Arial" }, 13) ?? fallback;

            menuCanvasObj = new GameObject("MainMenu_Overlay_Canvas");
            Canvas canvas = menuCanvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60;

            CanvasScaler scaler = menuCanvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            menuCanvasObj.AddComponent<GraphicRaycaster>();

            GameObject blocker = new GameObject("TransparentBlocker");
            blocker.transform.SetParent(menuCanvasObj.transform, false);
            Image bImg = blocker.AddComponent<Image>();
            bImg.color = new Color(0.06f, 0.05f, 0.04f, 0.14f);
            bImg.raycastTarget = true;
            SetFill(blocker.GetComponent<RectTransform>(), 0, 0);

            // Floating Aged-Parchment Editorial Dossier Card on the left
            GameObject card = new GameObject("MainMenuDossierCard");
            card.transform.SetParent(blocker.transform, false);
            card.AddComponent<Image>().color = PaperBase;
            Outline ol = card.AddComponent<Outline>();
            ol.effectColor = PaperBorder; ol.effectDistance = new Vector2(2f, -2f);
            RectTransform cRt = card.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.045f, 0.12f); cRt.anchorMax = new Vector2(0.385f, 0.88f);
            cRt.offsetMin = Vector2.zero; cRt.offsetMax = Vector2.zero;

            GameObject topBar = new GameObject("EditorialTopBar");
            topBar.transform.SetParent(card.transform, false);
            topBar.AddComponent<Image>().color = AccentBurgundy;
            RectTransform tbRt = topBar.GetComponent<RectTransform>();
            tbRt.anchorMin = new Vector2(0f, 1f); tbRt.anchorMax = new Vector2(1f, 1f); tbRt.pivot = new Vector2(0.5f, 1f);
            tbRt.sizeDelta = new Vector2(0f, 6f); tbRt.anchoredPosition = Vector2.zero;

            GameObject col = new GameObject("MenuCol");
            col.transform.SetParent(card.transform, false);
            SetFill(col.AddComponent<RectTransform>(), 28, 22);
            VerticalLayoutGroup lay = col.AddComponent<VerticalLayoutGroup>();
            lay.spacing = 10f;
            lay.childControlWidth = true; lay.childForceExpandWidth = true;
            lay.childControlHeight = false; lay.childForceExpandHeight = false;

            CreateLabel(col.transform, "Tag", "POLIMI eHEALTH LAB  •  CASE DOSSIER #00", bodyFont, 11, AccentOchre, FontStyle.Bold, 20f);
            CreateLabel(col.transform, "Title", "THE HEALTH FILES", headingFont, 34, InkPrimary, FontStyle.Bold, 44f);
            CreateLabel(col.transform, "Sub", "Case Zero: The VitaPulse Investigation", headingFont, 17, AccentBurgundy, FontStyle.Italic, 24f);

            GameObject div = new GameObject("Divider");
            div.transform.SetParent(col.transform, false);
            div.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 2f);
            LayoutElement dle = div.AddComponent<LayoutElement>(); dle.minHeight = 2f; dle.preferredHeight = 2f;
            div.AddComponent<Image>().color = PaperBorder;

            CreateLabel(col.transform, "Desc",
                "Step into Room 504 at the Digital Health Investigation Office. Inspect viral claims, check real privacy policies, and separate evidence from hype.",
                bodyFont, 13, InkMuted, FontStyle.Normal, 64f);

            CreateMenuButton(col.transform, "BtnStart",    "▶   START INVESTIGATION", bodyFont, AccentSage, Color.white, 46f, StartGame);
            CreateMenuButton(col.transform, "BtnSettings", "⚙   SETTINGS (AUDIO & GRAPHICS)", bodyFont, PaperCard, InkPrimary, 40f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.55f);
                bool show = !settingsCardObj.activeSelf;
                controlsCardObj.SetActive(false);
                settingsCardObj.SetActive(show);
            });
            CreateMenuButton(col.transform, "BtnControls", "⌨   CONTROLS & KEYBINDINGS", bodyFont, PaperCard, InkPrimary, 40f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.55f);
                bool show = !controlsCardObj.activeSelf;
                settingsCardObj.SetActive(false);
                controlsCardObj.SetActive(show);
            });
            CreateMenuButton(col.transform, "BtnQuit",     "✕   QUIT TO DESKTOP", bodyFont, AccentBurgundy, Color.white, 38f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.55f);
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });

            CreateLabel(col.transform, "Footer", "Group 12  •  Room 504  •  Press Esc in-game for Pause Menu", bodyFont, 11, InkMuted, FontStyle.Italic, 20f);

            // === RIGHT PANEL 1: SETTINGS (AUDIO + GRAPHICS + FULLSCREEN) ===
            BuildSettingsCard(blocker.transform, headingFont, bodyFont);

            // === RIGHT PANEL 2: VISUAL KEYCAP CONTROLS ===
            BuildVisualControlsCard(blocker.transform, headingFont, bodyFont, monoFont);
        }

        private void BuildSettingsCard(Transform parent, Font headingFont, Font bodyFont)
        {
            GameObject sCard = new GameObject("SettingsDossierCard");
            sCard.transform.SetParent(parent, false);
            sCard.AddComponent<Image>().color = PaperBase;
            Outline ol = sCard.AddComponent<Outline>(); ol.effectColor = PaperBorder; ol.effectDistance = new Vector2(2f, -2f);
            RectTransform rt = sCard.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.43f, 0.20f); rt.anchorMax = new Vector2(0.92f, 0.80f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            GameObject col = new GameObject("SettingsCol");
            col.transform.SetParent(sCard.transform, false);
            SetFill(col.AddComponent<RectTransform>(), 26, 22);
            VerticalLayoutGroup lay = col.AddComponent<VerticalLayoutGroup>();
            lay.spacing = 12f;
            lay.childControlWidth = true; lay.childForceExpandWidth = true;
            lay.childControlHeight = false; lay.childForceExpandHeight = false;

            CreateLabel(col.transform, "SHdr", "SETTINGS — AUDIO & GRAPHICS", headingFont, 21, AccentBurgundy, FontStyle.Bold, 30f);

            // Master Volume Label + Slider
            volValueText = CreateLabel(col.transform, "VolLbl", $"Master Volume:  {Mathf.RoundToInt(AudioListener.volume * 100f)}%", bodyFont, 14, InkPrimary, FontStyle.Bold, 22f);
            BuildVolumeSlider(col.transform);

            // Mute Toggle
            Button muteBtn = CreateMenuButton(col.transform, "MuteToggle", "Audio Output:  ON (Click to Mute)", bodyFont, PaperCard, InkPrimary, 38f, () =>
            {
                isMuted = !isMuted;
                AudioListener.pause = isMuted;
                if (muteBtnLabel != null) muteBtnLabel.text = isMuted ? "Audio Output:  MUTED (Click to Unmute)" : "Audio Output:  ON (Click to Mute)";
            });
            muteBtnLabel = muteBtn.GetComponentInChildren<Text>();

            // Graphics Quality Cycle (Low / Medium / High)
            Button qualBtn = CreateMenuButton(col.transform, "QualityToggle", "Graphics Quality:  HIGH (Crisp SMAA)", bodyFont, PaperCard, InkPrimary, 38f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.55f);
                ApplyGraphicsTier(graphicsTier + 1);
            });
            qualityBtnLabel = qualBtn.GetComponentInChildren<Text>();

            // Fullscreen / Windowed Toggle
            Button fsBtn = CreateMenuButton(col.transform, "FullscreenToggle", Screen.fullScreen ? "Display Mode:  FULLSCREEN" : "Display Mode:  WINDOWED", bodyFont, PaperCard, InkPrimary, 38f, () =>
            {
                SoundManager.Instance?.Play("ui_click", 0.55f);
                Screen.fullScreen = !Screen.fullScreen;
                if (fullscreenBtnLabel != null) fullscreenBtnLabel.text = Screen.fullScreen ? "Display Mode:  FULLSCREEN" : "Display Mode:  WINDOWED";
            });
            fullscreenBtnLabel = fsBtn.GetComponentInChildren<Text>();

            CreateLabel(col.transform, "SNote", "Tip: Lower Graphics Quality to LOW or MEDIUM for smoother performance on laptops.", bodyFont, 12, InkMuted, FontStyle.Italic, 30f);

            settingsCardObj = sCard;
            settingsCardObj.SetActive(false);
        }

        private void BuildVolumeSlider(Transform parent)
        {
            GameObject sGo = new GameObject("VolSlider");
            sGo.transform.SetParent(parent, false);
            sGo.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 26f);
            LayoutElement le = sGo.AddComponent<LayoutElement>(); le.minHeight = 26f; le.preferredHeight = 26f;

            GameObject bg = new GameObject("Bg"); bg.transform.SetParent(sGo.transform, false);
            RectTransform bgRt = bg.AddComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0.3f); bgRt.anchorMax = new Vector2(1f, 0.7f); bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
            bg.AddComponent<Image>().color = PaperCard;

            GameObject fillArea = new GameObject("FillArea"); fillArea.transform.SetParent(sGo.transform, false);
            RectTransform faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = new Vector2(0f, 0.3f); faRt.anchorMax = new Vector2(1f, 0.7f); faRt.offsetMin = Vector2.zero; faRt.offsetMax = Vector2.zero;

            GameObject fill = new GameObject("Fill"); fill.transform.SetParent(fillArea.transform, false);
            fill.AddComponent<RectTransform>();
            fill.AddComponent<Image>().color = AccentSage;

            GameObject handleArea = new GameObject("HandleArea"); handleArea.transform.SetParent(sGo.transform, false);
            SetFill(handleArea.AddComponent<RectTransform>(), 8, 0);

            GameObject handle = new GameObject("Handle"); handle.transform.SetParent(handleArea.transform, false);
            RectTransform hRt = handle.AddComponent<RectTransform>(); hRt.sizeDelta = new Vector2(18f, 24f);
            Image hImg = handle.AddComponent<Image>(); hImg.color = AccentBurgundy;

            Slider sl = sGo.AddComponent<Slider>();
            sl.fillRect = fill.GetComponent<RectTransform>();
            sl.handleRect = hRt;
            sl.targetGraphic = hImg;
            sl.minValue = 0f; sl.maxValue = 1f; sl.value = AudioListener.volume;
            sl.onValueChanged.AddListener(v =>
            {
                AudioListener.volume = v;
                if (volValueText != null) volValueText.text = $"Master Volume:  {Mathf.RoundToInt(v * 100f)}%";
            });
        }

        private void BuildVisualControlsCard(Transform parent, Font headingFont, Font bodyFont, Font monoFont)
        {
            GameObject cCard = new GameObject("ControlsDossierCard");
            cCard.transform.SetParent(parent, false);
            cCard.AddComponent<Image>().color = PaperBase;
            Outline ol = cCard.AddComponent<Outline>(); ol.effectColor = PaperBorder; ol.effectDistance = new Vector2(2f, -2f);
            RectTransform rt = cCard.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.43f, 0.16f); rt.anchorMax = new Vector2(0.93f, 0.84f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            GameObject col = new GameObject("ControlsCol");
            col.transform.SetParent(cCard.transform, false);
            SetFill(col.AddComponent<RectTransform>(), 24, 20);
            VerticalLayoutGroup lay = col.AddComponent<VerticalLayoutGroup>();
            lay.spacing = 10f;
            lay.childControlWidth = true; lay.childForceExpandWidth = true;
            lay.childControlHeight = false; lay.childForceExpandHeight = false;

            CreateLabel(col.transform, "CHdr", "CONTROLS & KEYBINDINGS", headingFont, 21, AccentBurgundy, FontStyle.Bold, 30f);

            AddKeycapRow(col.transform, new[] { "MOUSE" }, "Gentle free look-around inside the 3D office", bodyFont, monoFont);
            AddKeycapRow(col.transform, new[] { "LMB" },   "Interact: Phone, 3D Monitor, Books, Drawers & Printer", bodyFont, monoFont);
            AddKeycapRow(col.transform, new[] { "Q", "E" }, "Turn chair Left / Right around office stations", bodyFont, monoFont);
            AddKeycapRow(col.transform, new[] { "◄", "►" }, "Arrow keys also turn between office stations", bodyFont, monoFont);
            AddKeycapRow(col.transform, new[] { "ESC" },   "Step back to Desk / Open Pause & Options Menu", bodyFont, monoFont);
            AddKeycapRow(col.transform, new[] { "1–6" },   "Quick-jump directly to Desk, PC, Cabinet, Shelf, Printer, Board", bodyFont, monoFont);

            CreateLabel(col.transform, "CTip", "Golden Investigation Rule: Always verify SOURCE, EVIDENCE & CONTEXT!", bodyFont, 12, AccentOchre, FontStyle.BoldAndItalic, 24f);

            controlsCardObj = cCard;
            controlsCardObj.SetActive(false);
        }

        private void AddKeycapRow(Transform parent, string[] keys, string description, Font bodyFont, Font monoFont)
        {
            GameObject row = new GameObject("KeyRow");
            row.transform.SetParent(parent, false);
            row.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, 38f);
            LayoutElement rle = row.AddComponent<LayoutElement>(); rle.minHeight = 38f; rle.preferredHeight = 38f;
            row.AddComponent<Image>().color = PaperCard;
            HorizontalLayoutGroup hl = row.AddComponent<HorizontalLayoutGroup>();
            hl.padding = new RectOffset(10, 10, 5, 5); hl.spacing = 8f;
            hl.childAlignment = TextAnchor.MiddleLeft;
            hl.childControlWidth = false; hl.childForceExpandWidth = false;
            hl.childControlHeight = true; hl.childForceExpandHeight = true;

            foreach (string k in keys)
            {
                GameObject cap = new GameObject("Key_" + k);
                cap.transform.SetParent(row.transform, false);
                float w = Mathf.Max(34f, k.Length * 11f + 16f);
                cap.AddComponent<RectTransform>().sizeDelta = new Vector2(w, 28f);
                LayoutElement kle = cap.AddComponent<LayoutElement>(); kle.minWidth = w; kle.preferredWidth = w;
                cap.AddComponent<Image>().color = KeycapBg;
                Outline kol = cap.AddComponent<Outline>(); kol.effectColor = InkPrimary; kol.effectDistance = new Vector2(1.5f, -2f);

                GameObject kt = new GameObject("KT"); kt.transform.SetParent(cap.transform, false);
                SetFill(kt.AddComponent<RectTransform>(), 2, 1);
                Text t = kt.AddComponent<Text>();
                t.font = monoFont; t.text = k; t.fontSize = 12; t.fontStyle = FontStyle.Bold;
                t.color = InkPrimary; t.alignment = TextAnchor.MiddleCenter; t.raycastTarget = false;
            }

            GameObject dGo = new GameObject("Desc");
            dGo.transform.SetParent(row.transform, false);
            dGo.AddComponent<RectTransform>().sizeDelta = new Vector2(360f, 28f);
            LayoutElement dle = dGo.AddComponent<LayoutElement>(); dle.flexibleWidth = 1f;
            Text dt = dGo.AddComponent<Text>();
            dt.font = bodyFont; dt.text = description; dt.fontSize = 13;
            dt.color = InkPrimary; dt.alignment = TextAnchor.MiddleLeft; dt.raycastTarget = false;
        }

        private static void SetFill(RectTransform rt, float px, float py)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(px, py); rt.offsetMax = new Vector2(-px, -py);
        }

        private static Text CreateLabel(Transform parent, string name, string text, Font font, int size, Color col, FontStyle style, float height)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, height);
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight = height; le.preferredHeight = height;
            Text t = go.AddComponent<Text>();
            t.font = font; t.text = text; t.fontSize = size; t.color = col; t.fontStyle = style;
            t.alignment = TextAnchor.MiddleLeft; t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        private static Button CreateMenuButton(Transform parent, string name, string label, Font font, Color bg, Color txtCol, float height, UnityEngine.Events.UnityAction onClick)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>().sizeDelta = new Vector2(0f, height);
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.minHeight = height; le.preferredHeight = height;
            Image img = go.AddComponent<Image>(); img.color = bg;
            Outline ol = go.AddComponent<Outline>(); ol.effectColor = PaperBorder; ol.effectDistance = new Vector2(1f, -1f);
            Button btn = go.AddComponent<Button>(); btn.targetGraphic = img; btn.onClick.AddListener(onClick);

            GameObject tGo = new GameObject("Label");
            tGo.transform.SetParent(go.transform, false);
            SetFill(tGo.AddComponent<RectTransform>(), 16, 4);
            Text t = tGo.AddComponent<Text>();
            t.font = font; t.text = label; t.fontSize = 14; t.fontStyle = FontStyle.Bold;
            t.color = txtCol; t.alignment = TextAnchor.MiddleLeft; t.raycastTarget = false;
            return btn;
        }
    }
}
