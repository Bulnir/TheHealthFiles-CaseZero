#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace HealthFiles.EditorTools
{
    /// <summary>
    /// 1-Click Modern Glass-Walled 3D Detective Office Builder (WP2 + WP3).
    /// Menu: HealthFiles -> 1. Build Realistic 3D Detective Office (Demo Ready)
    ///
    /// Features:
    /// - Full-width floor-to-ceiling modern glass back wall with detailed office entrance door
    /// - Lively outer office corridor (enclosed, vending machine, plants, waiting area) +
    ///   neighboring glass-walled Conference Room visible across the hallway!
    /// - Framed Cork Evidence Board mounted cleanly on the solid left wall (no cramped wall mismatch)
    /// - Executive walnut desk with dark leather desk mat, properly scaled keyboard & mouse,
    ///   sleek modern dark-graphite IP office phone (with glowing LCD & keypad), PC tower neatly clear of desk
    /// - Perfectly aligned visitor sofa + coffee table under the panoramic right city-view window
    /// - Enclosed wooden office bookcase with normal-sized hardcover books & golden Investigation Manual
    /// </summary>
    public static class CaseZeroOfficeGenerator
    {
        private const string SceneFolder = "Assets/_Game/Scenes";
        private const string MaterialFolder = "Assets/_Game/Materials";
        private const string ScenePath = SceneFolder + "/OfficeScene.unity";

        // Axis-verified nappin prefabs (native front = +X)
        private const string NappinRoot = "Assets/nappin/OfficeEssentialsPack/Prefabs/";
        private const string P_Chair = NappinRoot + "(Prb)OfficeChair.prefab";
        private const string P_PC = NappinRoot + "(Prb)PC.prefab";
        private const string P_PCCase = NappinRoot + "(Prb)PCCase.prefab";
        private const string P_Printer = NappinRoot + "(Prb)Printer.prefab";
        private const string P_DeskLight = NappinRoot + "(Prb)DeskLight.prefab";
        private const string P_DeskPlant = NappinRoot + "(Prb)DeskPlant.prefab";
        private const string P_Mug = NappinRoot + "(Prb)Mug.prefab";
        private const string P_PenHolder = NappinRoot + "(Prb)PenHolder.prefab";
        private const string P_Clock = NappinRoot + "(Prb)Clock.prefab";
        private const string P_TrashCan = NappinRoot + "(Prb)TrashCan.prefab";
        private const string P_CeilingLight = NappinRoot + "(Prb)CeilingLight1.prefab";
        private const string P_CeilingLight2 = NappinRoot + "(Prb)CeilingLight2.prefab";
        private const string P_Plant1 = NappinRoot + "(Prb)Plant1.prefab";
        private const string P_Plant2 = NappinRoot + "(Prb)Plant2.prefab";
        private const string P_Sofa = NappinRoot + "(Prb)Sofa1.prefab";
        private const string P_CoffeeTable = NappinRoot + "(Prb)CoffeTable.prefab";
        private const string P_WaterDispenser = NappinRoot + "(Prb)WaterDispenser.prefab";
        private const string P_ConferenceTable = NappinRoot + "(Prb)ConferenceTable.prefab";
        private const string P_VendingMachine = NappinRoot + "(Prb)VendingMachine.prefab";

        // Corkboard_Desk (verified: native front = -Z at rotY = 0)
        private const string P_CorkBoard = "Assets/Corkboard_Desk/Prefab/Cork Board.prefab";

        [MenuItem("HealthFiles/1. Build Realistic 3D Detective Office (Demo Ready)")]
        public static void BuildRealisticDetectiveOffice()
        {
            EnsureFolders();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Bright, Lively Daylight + Warm Interior Lighting
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.52f, 0.58f, 0.68f);
            RenderSettings.ambientEquatorColor = new Color(0.42f, 0.44f, 0.48f);
            RenderSettings.ambientGroundColor = new Color(0.24f, 0.22f, 0.20f);

            GameObject lightRoot = new GameObject("--- LIGHTING (WP2) ---");
            CreateDirectionalLight(lightRoot.transform, "SunLight", new Vector3(38f, -125f, 0f), new Color(1f, 0.96f, 0.89f), 1.10f);
            CreatePointLight(lightRoot.transform, "CeilingGlow", new Vector3(0f, 2.98f, 0.6f), new Color(1f, 0.97f, 0.91f), 9.5f, 11f);
            // DeskLampGlow intentionally off so desk paper & PC tower never suffer white specular glare!
            CreatePointLight(lightRoot.transform, "CorridorLightL", new Vector3(-1.8f, 2.85f, 5.1f), new Color(0.94f, 0.97f, 1f), 12f, 10f);
            CreatePointLight(lightRoot.transform, "CorridorLightR", new Vector3(1.8f, 2.85f, 5.1f), new Color(0.94f, 0.97f, 1f), 12f, 10f);
            CreatePointLight(lightRoot.transform, "AcrossRoomLight", new Vector3(0.4f, 2.85f, 8.2f), new Color(1f, 0.94f, 0.82f), 14f, 11f);

            // 2. Modern Full-Glass Office Architecture + Living Corridor + Across-the-Hall Meeting Room + City View
            GameObject roomRoot = new GameObject("--- OFFICE 3D (WP2) ---");
            BuildModernOfficeShell(roomRoot.transform);
            BuildLivingCorridorAndAcrossOffice(roomRoot.transform);

            // 3. Executive Detective Desk (Wide walnut + dark steel frame + dark leather Desk Mat)
            float deskTopY = 0.78f;
            BuildExecutiveDesk(roomRoot.transform, new Vector3(0f, 0f, 1.32f), deskTopY);

            // Dark charcoal leather desk mat sized cleanly to frame 44cm keyboard + 11.5cm mouse!
            CreateBox(roomRoot.transform, "DeskMat", new Vector3(-0.02f, deskTopY + 0.003f, 1.24f), new Vector3(0.76f, 0.006f, 0.38f), new Color(0.14f, 0.16f, 0.20f), removeCollider: true);

            // Red Office Chair positioned RIGHT AT THE DESK (facing +Z toward Monitor: rotY = 270)
            PlacePrefabClean(roomRoot.transform, "OfficeChair", P_Chair, new Vector3(0f, 0f, 0.56f), 0.98f, 270f);

            // 4. Interactable: Office Computer (exact auto-fit collider + LIVE WorldSpace HealthOS directly on the 3D glass!)
            GameObject computerStation = PlacePrefabClean(roomRoot.transform, "OfficeComputer", P_PC, new Vector3(-0.04f, deskTopY + 0.006f, 1.39f), 0.42f, 90f);
            FixKeyboardAndMouseScale(computerStation, deskTopY + 0.006f);
            AddAutoFitColliderAndInteractable(
                computerStation,
                "desk_computer",
                "Office Computer (Click to Zoom Into HealthOS)",
                "Computer",
                padding: 0.03f
            );
            Vector3 monitorScreenCenter = AttachWorldSpaceMonitorOS(computerStation, computerStation.GetComponent<BoxCollider>());

            // PC Tower standing cleanly ON THE RIGHT CORNER OF THE DESK (visible while seated, with glowing green LED & click interaction!)
            GameObject pcTower = PlacePrefabClean(roomRoot.transform, "PCTower", P_PCCase, new Vector3(0.84f, deskTopY, 1.50f), 0.40f, 102f);
            CreateBox(pcTower.transform, "PC_PowerLED", new Vector3(0.105f, 0.34f, -0.04f), new Vector3(0.012f, 0.012f, 0.024f), new Color(0.22f, 0.96f, 0.42f), local: true, removeCollider: true);
            AddAutoFitColliderAndInteractable(pcTower, "desk_pc_tower", "Workstation PC Tower (Check Security Status)", "Desk", padding: 0.03f);

            // 5. Interactable: Sleek Modern Office IP Telephone (angled toward player, real 21cm size!)
            GameObject phone = BuildModernOfficeIPPhone(roomRoot.transform, new Vector3(-0.64f, deskTopY, 1.24f));
            phone.AddComponent<RingingPhoneEffect>();
            AddAutoFitColliderAndInteractable(phone, "desk_phone", "Office IP Telephone (Answer / Call Personas)", "Desk", padding: 0.035f);

            // 6. Interactable: Mail Delivery Log Clipboard (A4 proportions, readable 3D text right on the desk!)
            GameObject mailLog = BuildMailLogClipboard(roomRoot.transform, new Vector3(0.54f, deskTopY, 1.12f));
            AddAutoFitColliderAndInteractable(mailLog, "desk_mail_log", "Mail Delivery Log (Click to Pick Up & Read)", "Desk", padding: 0.03f, minLocalSize: new Vector3(0.24f, 0.07f, 0.32f));

            // Clean, Real-World-Proportioned Desk Props (44cm lamp, 9.5cm mug, 13cm pen cup, 20cm plant)
            PlacePrefabClean(roomRoot.transform, "DeskLamp", P_DeskLight, new Vector3(0.58f, deskTopY, 1.58f), 0.44f, 140f);
            PlacePrefabClean(roomRoot.transform, "CoffeeMug", P_Mug, new Vector3(-0.56f, deskTopY, 0.98f), 0.095f, 45f);
            PlacePrefabClean(roomRoot.transform, "PenHolder", P_PenHolder, new Vector3(0.38f, deskTopY, 1.56f), 0.13f, 90f);
            PlacePrefabClean(roomRoot.transform, "DeskPlant", P_DeskPlant, new Vector3(-0.80f, deskTopY, 1.58f), 0.20f, 90f);

            // 7. LEFT WALL STATION 1: 3-Drawer Steel Archive Cabinet (z = +2.20, slides toward +X)
            BuildSlidingArchiveCabinet(roomRoot.transform, new Vector3(-2.85f, 0f, 2.20f));

            // 8. LEFT WALL STATION 2: Framed Cork Evidence Board (unobstructed solid left wall at z = +0.55, facing +X!)
            GameObject board = PlacePrefabClean(roomRoot.transform, "EvidenceBoard", P_CorkBoard, new Vector3(-3.24f, 1.22f, 0.55f), 1.12f, -90f);
            Bounds boardBounds = GetWorldBounds(board);
            float boardWidth = Mathf.Max(1.60f, boardBounds.size.z); // along world Z when rotated -90
            CreateBox(board.transform, "BoardFrameTop", new Vector3(0f, 1.13f, 0f), new Vector3(boardWidth + 0.08f, 0.05f, 0.07f), new Color(0.22f, 0.15f, 0.10f), local: true, removeCollider: true);
            CreateBox(board.transform, "BoardFrameBot", new Vector3(0f, -0.01f, 0f), new Vector3(boardWidth + 0.08f, 0.05f, 0.07f), new Color(0.22f, 0.15f, 0.10f), local: true, removeCollider: true);
            CreateBox(board.transform, "BoardFrameL", new Vector3(-boardWidth * 0.5f - 0.02f, 0.56f, 0f), new Vector3(0.05f, 1.18f, 0.07f), new Color(0.22f, 0.15f, 0.10f), local: true, removeCollider: true);
            CreateBox(board.transform, "BoardFrameR", new Vector3(boardWidth * 0.5f + 0.02f, 0.56f, 0f), new Vector3(0.05f, 1.18f, 0.07f), new Color(0.22f, 0.15f, 0.10f), local: true, removeCollider: true);

            board.AddComponent<EvidenceBoardVisual>();
            AddAutoFitColliderAndInteractable(
                board,
                "evidence_board",
                "Evidence Cork Board (Pin & Connect Clues)",
                "EvidenceBoard",
                padding: 0.04f,
                minLocalSize: new Vector3(1.50f, 1.10f, 0.20f)
            );

            // Sleek custom modern Wall Clock above the Archive Cabinet (clean recessed dial, 12 tick marks, live ticking hands!)
            BuildModernWallClock(roomRoot.transform, new Vector3(-3.26f, 2.48f, 2.20f));

            // 9. LEFT WALL STATION 3: Office Printer with properly-seated output-tray jammed sheet & front-panel LED!
            GameObject printer = PlacePrefabClean(roomRoot.transform, "OfficePrinter", P_Printer, new Vector3(-2.74f, 0f, -1.15f), 0.96f, 0f);
            Bounds pb = GetWorldBounds(printer);
            // Place jammed sheet emerging naturally out of the left output slot onto the tray
            Vector3 sheetLocal = printer.transform.InverseTransformPoint(new Vector3(pb.center.x + 0.02f, pb.max.y - 0.24f, pb.min.z + 0.18f));
            GameObject jammedSheet = CreateBox(printer.transform, "PrinterJammedSheet", sheetLocal, new Vector3(0.20f, 0.008f, 0.24f), new Color(0.95f, 0.94f, 0.89f), local: true, removeCollider: true);
            jammedSheet.transform.localRotation = Quaternion.Euler(14f, 16f, -6f);
            // Small blinking error LED seated cleanly on the dark front control bezel (never on the paper tray!)
            Vector3 ledLocal = printer.transform.InverseTransformPoint(new Vector3(pb.max.x - 0.12f, pb.max.y - 0.28f, pb.center.z + 0.06f));
            GameObject jamLed = CreateBox(printer.transform, "PrinterJamLED", ledLocal, new Vector3(0.024f, 0.016f, 0.024f), new Color(1f, 0.28f, 0.08f), local: true, removeCollider: true);
            PrinterJamEffect jamFx = printer.AddComponent<PrinterJamEffect>();
            jamFx.Configure(jamLed.GetComponent<Renderer>(), jammedSheet.transform);
            AddAutoFitColliderAndInteractable(
                printer,
                "printer_daniel",
                "Office Printer (WARNING: PAPER JAM! Click to Fix)",
                "Printer",
                padding: 0.04f
            );

            // Classic Office Damacana Water Cooler neatly in Front-Left Corner
            BuildOfficeWaterCooler(roomRoot.transform, new Vector3(-2.82f, 0f, -2.35f));

            // 10. RIGHT WALL STATION: Enclosed Wooden Office Bookcase + Golden Manual (z = +1.80)
            BuildEnclosedOfficeBookcase(roomRoot.transform, new Vector3(2.96f, 0f, 1.80f));

            // 11. PERFECTLY ALIGNED Visitor Seating Area under the Right Panoramic Window!
            // Both Sofa and Coffee Table share exact z = -1.35 and exact parallel rotY = 180 (facing -X into room!)
            PlacePrefabClean(roomRoot.transform, "VisitorSofa", P_Sofa, new Vector3(2.58f, 0f, -1.35f), 0.85f, 180f);
            PlacePrefabClean(roomRoot.transform, "VisitorCoffeeTable", P_CoffeeTable, new Vector3(1.58f, 0f, -1.35f), 0.42f, 180f);

            // Room Plants, Trash Can, Ceiling Lights
            PlacePrefabClean(roomRoot.transform, "CornerPlantRight", P_Plant1, new Vector3(2.68f, 0f, 2.96f), 1.25f, 180f);
            PlacePrefabClean(roomRoot.transform, "CornerPlantLeft", P_Plant2, new Vector3(-2.80f, 0f, 2.98f), 1.08f, 0f);
            PlacePrefabClean(roomRoot.transform, "TrashCan", P_TrashCan, new Vector3(-1.12f, 0f, 1.40f), 0.34f, 0f);
            PlacePrefabClean(roomRoot.transform, "CeilingFixtureA", P_CeilingLight, new Vector3(-1.1f, 3.12f, 0.6f), 0.14f, 0f);
            PlacePrefabClean(roomRoot.transform, "CeilingFixtureB", P_CeilingLight, new Vector3(1.1f, 3.12f, 0.6f), 0.14f, 0f);

            // 12. Camera Viewpoints (VP_Computer zooms right up so the live 3D WorldSpace Monitor fills the screen!)
            GameObject vpRoot = new GameObject("--- VIEWPOINTS (WP2) ---");
            List<CameraViewpoint> viewpoints = new List<CameraViewpoint>
            {
                CreateViewpoint(vpRoot.transform, "Desk", "Desk", new Vector3(0f, 1.44f, 0.36f), new Vector3(0f, 0.98f, 1.65f)),
                CreateViewpoint(vpRoot.transform, "Computer", "Computer", monitorScreenCenter + new Vector3(0f, 0f, -0.45f), monitorScreenCenter),
                CreateViewpoint(vpRoot.transform, "Cabinet", "Cabinet", new Vector3(-1.02f, 1.56f, 2.20f), new Vector3(-2.62f, 0.70f, 2.20f)),
                CreateViewpoint(vpRoot.transform, "Bookshelf", "Bookshelf", new Vector3(1.55f, 1.24f, 1.80f), new Vector3(2.96f, 1.18f, 1.80f)),
                CreateViewpoint(vpRoot.transform, "Printer", "Printer", new Vector3(-1.15f, 1.22f, -1.15f), new Vector3(-2.74f, 0.68f, -1.15f)),
                CreateViewpoint(vpRoot.transform, "EvidenceBoard", "Evidence Board", new Vector3(-1.35f, 1.78f, 0.55f), new Vector3(-3.24f, 1.78f, 0.55f))
            };

            // 13. Main Camera + High-Quality SMAA Anti-Aliasing (crisp edges even when standing still!) + OfficeRaycaster
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.fieldOfView = 58f;
            cam.nearClipPlane = 0.05f;
            cam.allowMSAA = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.76f, 0.95f);
            UniversalAdditionalCameraData urpCam = camObj.AddComponent<UniversalAdditionalCameraData>();
            urpCam.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            urpCam.antialiasingQuality = AntialiasingQuality.High;
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<OfficeRaycaster>();
            camObj.transform.position = viewpoints[0].transform.position;
            camObj.transform.rotation = viewpoints[0].transform.rotation;

            // 14. EventSystem (Unity 6 Input System UI)
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            esObj.AddComponent<InputSystemUIInputModule>();
#else
            esObj.AddComponent<StandaloneInputModule>();
#endif

            // 15. Core Systems Object (WP2 + WP3)
            GameObject sysObj = new GameObject("--- SYSTEMS (WP2 + WP3) ---");
            sysObj.AddComponent<SoundManager>();
            CameraViewpointController vpCtrl = sysObj.AddComponent<CameraViewpointController>();
            vpCtrl.SetViewpoints(viewpoints, camObj.transform);
            sysObj.AddComponent<EvidenceManager>();
            sysObj.AddComponent<DialogueManager>();
            sysObj.AddComponent<PersonalizationManager>();
            sysObj.AddComponent<MissionManager>();
            sysObj.AddComponent<CaseZeroHUD>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            AssetDatabase.Refresh();
            Debug.Log("[HealthFiles] Polished Full-Glass Modern 3D Detective Office built!");
            EditorUtility.DisplayDialog(
                "Health Files: Case Zero",
                "Full-Glass Modern Detective Office rebuilt!\n\n- Full floor-to-ceiling glass back wall + unmistakable office door\n- Living hallway + neighboring glass Conference Room across the corridor\n- Cork Evidence Board cleanly mounted on left wall\n- Modern IP office phone + desk mat + enlarged keyboard & mouse\n- PC tower clear of desk + perfectly aligned sofa & coffee table",
                "Awesome!"
            );
        }

        private static void FixKeyboardAndMouseScale(GameObject pcRoot, float surfaceY)
        {
            Transform kb = FindChildRecursive(pcRoot.transform, "PC_keyboard");
            float kbRightX = 0.20f;
            float kbCenterZ = 1.22f;

            if (kb != null)
            {
                // Target real full-size office keyboard width = 44cm (0.44m) along world X!
                Bounds kbB = GetWorldBounds(kb.gameObject);
                if (kbB.size.x > 0.001f)
                {
                    float factor = 0.44f / kbB.size.x;
                    kb.localScale *= factor;
                }
                kbB = GetWorldBounds(kb.gameObject);
                // Center keyboard cleanly at x = -0.06, z = 1.20 on the desk mat, flush with surfaceY
                kb.position += new Vector3(-0.06f - kbB.center.x, surfaceY - kbB.min.y, 1.20f - kbB.center.z);
                kbB = GetWorldBounds(kb.gameObject);
                kbRightX = kbB.max.x;
                kbCenterZ = kbB.center.z;
            }

            Transform mouse = FindChildRecursive(pcRoot.transform, "PC_mouse");
            if (mouse != null)
            {
                // Target real ergonomic office mouse length = 11.5cm (0.115m) along world Z!
                Bounds mB = GetWorldBounds(mouse.gameObject);
                if (mB.size.z > 0.001f)
                {
                    float factor = 0.115f / mB.size.z;
                    mouse.localScale *= factor;
                }
                mB = GetWorldBounds(mouse.gameObject);
                // Place cleanly 9cm to the right of the keyboard on the dark desk mat!
                float targetMouseX = kbRightX + 0.09f;
                mouse.position += new Vector3(targetMouseX - mB.center.x, surfaceY - mB.min.y, kbCenterZ - mB.center.z);
            }
        }

        private static Vector3 AttachWorldSpaceMonitorOS(GameObject pcRoot, BoxCollider outerCollider)
        {
            Transform monitor = FindChildRecursive(pcRoot.transform, "PC_monitor");
            Vector3 screenCenter = pcRoot.transform.position + new Vector3(0f, 0.28f, -0.05f);
            Vector2 screenSize = new Vector2(0.54f, 0.31f);

            if (monitor != null)
            {
                Bounds mb = GetWorldBounds(monitor.gameObject);
                screenCenter = new Vector3(mb.center.x, mb.center.y + mb.size.y * 0.055f, mb.min.z - 0.014f);
                screenSize = new Vector2(mb.size.x * 0.91f, mb.size.y * 0.76f);
            }

            GameObject osHost = new GameObject("WorldSpaceMonitorOS_Host");
            osHost.transform.SetParent(pcRoot.transform, false);
            osHost.transform.position = screenCenter;
            osHost.transform.rotation = Quaternion.identity;

            WorldSpaceMonitorOS os = osHost.AddComponent<WorldSpaceMonitorOS>();
            os.Configure(outerCollider, screenSize);
            return screenCenter;
        }

        private static void BuildModernWallClock(Transform parent, Vector3 pos)
        {
            GameObject clockRoot = new GameObject("WallClock");
            clockRoot.transform.SetParent(parent, false);
            clockRoot.transform.position = pos;

            Color bezelBlack = new Color(0.11f, 0.12f, 0.15f);
            Color dialWhite = new Color(0.95f, 0.95f, 0.93f);
            Color tickDark = new Color(0.16f, 0.17f, 0.20f);

            // Outer dark circular casing (faces +X into the room)
            GameObject casing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            casing.name = "ClockCasing";
            casing.transform.SetParent(clockRoot.transform, false);
            casing.transform.localPosition = new Vector3(0.015f, 0f, 0f);
            casing.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            casing.transform.localScale = new Vector3(0.36f, 0.018f, 0.36f);
            Object.DestroyImmediate(casing.GetComponent<Collider>());
            ApplyColorMaterial(casing.GetComponent<Renderer>(), "ClockCasing", bezelBlack);

            // Crisp recessed white dial face
            GameObject dial = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dial.name = "ClockDial";
            dial.transform.SetParent(clockRoot.transform, false);
            dial.transform.localPosition = new Vector3(0.028f, 0f, 0f);
            dial.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            dial.transform.localScale = new Vector3(0.318f, 0.008f, 0.318f);
            Object.DestroyImmediate(dial.GetComponent<Collider>());
            ApplyColorMaterial(dial.GetComponent<Renderer>(), "ClockDialClean", dialWhite);

            // 12 crisp 3D hour tick marks around the dial
            for (int h = 0; h < 12; h++)
            {
                float rad = h * 30f * Mathf.Deg2Rad;
                bool major = (h % 3 == 0);
                float r = major ? 0.130f : 0.136f;
                GameObject tick = CreateBox(
                    clockRoot.transform,
                    $"ClockTick_{h}",
                    new Vector3(0.036f, Mathf.Cos(rad) * r, Mathf.Sin(rad) * r),
                    new Vector3(0.003f, major ? 0.026f : 0.014f, major ? 0.007f : 0.004f),
                    tickDark,
                    local: true,
                    removeCollider: true
                );
                tick.transform.localRotation = Quaternion.Euler(h * 30f, 0f, 0f);
            }

            // Live ticking hands sitting cleanly in front of the dial
            GameObject pivot = new GameObject("LiveClockHands");
            pivot.transform.SetParent(clockRoot.transform, false);
            pivot.transform.localPosition = new Vector3(0.039f, 0f, 0f);

            CreateBox(pivot.transform, "ClockHubCap", new Vector3(0.007f, 0f, 0f), new Vector3(0.008f, 0.016f, 0.016f), bezelBlack, local: true, removeCollider: true);

            GameObject hourPivot = new GameObject("HourHandPivot");
            hourPivot.transform.SetParent(pivot.transform, false);
            CreateBox(hourPivot.transform, "HourBlade", new Vector3(0.002f, 0.040f, 0f), new Vector3(0.003f, 0.080f, 0.009f), bezelBlack, local: true, removeCollider: true);

            GameObject minPivot = new GameObject("MinuteHandPivot");
            minPivot.transform.SetParent(pivot.transform, false);
            CreateBox(minPivot.transform, "MinuteBlade", new Vector3(0.004f, 0.056f, 0f), new Vector3(0.003f, 0.112f, 0.006f), tickDark, local: true, removeCollider: true);

            GameObject secPivot = new GameObject("SecondHandPivot");
            secPivot.transform.SetParent(pivot.transform, false);
            CreateBox(secPivot.transform, "SecondBlade", new Vector3(0.006f, 0.060f, 0f), new Vector3(0.002f, 0.120f, 0.0035f), new Color(0.86f, 0.14f, 0.14f), local: true, removeCollider: true);

            RealTimeWallClock live = clockRoot.AddComponent<RealTimeWallClock>();
            live.Configure(hourPivot.transform, minPivot.transform, secPivot.transform);
        }

        private static void ApplyColorMaterial(Renderer rend, string matName, Color color)
        {
            string matPath = $"{MaterialFolder}/Mat_{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                AssetDatabase.CreateAsset(mat, matPath);
            }
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            else mat.color = color;
            EditorUtility.SetDirty(mat);
            rend.sharedMaterial = mat;
        }

        private static void AddWorldSignText(Transform parent, string name, Vector3 pos, Quaternion rot, Vector2 sizeMeters, string content, int fontSize, Color textColor)
        {
            GameObject canvasGo = new GameObject(name);
            canvasGo.transform.SetParent(parent, false);
            canvasGo.transform.position = pos;
            canvasGo.transform.rotation = rot;

            Canvas c = canvasGo.AddComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            RectTransform crt = canvasGo.GetComponent<RectTransform>();
            float resW = 520f;
            float resH = Mathf.Max(60f, resW * (sizeMeters.y / sizeMeters.x));
            crt.sizeDelta = new Vector2(resW, resH);
            crt.localScale = new Vector3(sizeMeters.x / resW, sizeMeters.y / resH, 1f);

            GameObject txtGo = new GameObject("SignText");
            txtGo.transform.SetParent(canvasGo.transform, false);
            RectTransform trt = txtGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;

            Text txt = txtGo.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = content;
            txt.fontSize = fontSize;
            txt.color = textColor;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.supportRichText = true;
            txt.raycastTarget = false;
        }

        private static Transform FindChildRecursive(Transform root, string targetName)
        {
            foreach (Transform child in root)
            {
                if (child.name == targetName) return child;
                Transform found = FindChildRecursive(child, targetName);
                if (found != null) return found;
            }
            return null;
        }

        private static void BuildModernOfficeShell(Transform parent)
        {
            // Unified warm architectural wall color across the ENTIRE office (zero color mismatch!)
            Color wallUniform = new Color(0.84f, 0.83f, 0.80f);
            Color steelFrame = new Color(0.15f, 0.17f, 0.21f);
            Color silverMetal = new Color(0.76f, 0.79f, 0.83f);
            Color woodTrim = new Color(0.24f, 0.17f, 0.12f);

            // Floor, Warm Cream/Beige Woven Office Rug with Taupe Border, Ceiling
            CreateBox(parent, "Floor_Hardwood", new Vector3(0f, -0.05f, 0.2f), new Vector3(6.8f, 0.1f, 6.8f), new Color(0.32f, 0.24f, 0.17f));
            CreateBox(parent, "Rug_Border", new Vector3(0f, 0.003f, 0.90f), new Vector3(3.52f, 0.006f, 2.62f), new Color(0.68f, 0.62f, 0.54f), removeCollider: true);
            CreateBox(parent, "Rug_Office", new Vector3(0f, 0.007f, 0.90f), new Vector3(3.34f, 0.008f, 2.44f), new Color(0.86f, 0.82f, 0.75f), removeCollider: true);
            CreateBox(parent, "Ceiling", new Vector3(0f, 3.35f, 0.2f), new Vector3(6.8f, 0.1f, 6.8f), new Color(0.92f, 0.91f, 0.89f));

            // Left Wall (solid — hosts Cabinet, Cork Board, Clock, Printer) & Front Wall (behind player)
            CreateBox(parent, "LeftWall", new Vector3(-3.35f, 1.65f, 0.2f), new Vector3(0.14f, 3.3f, 6.8f), wallUniform);
            CreateBox(parent, "FrontWall", new Vector3(0f, 1.65f, -3.15f), new Vector3(6.8f, 3.3f, 0.14f), wallUniform);
            CreateBox(parent, "Trim_Left", new Vector3(-3.26f, 0.08f, 0.2f), new Vector3(0.04f, 0.16f, 6.6f), woodTrim);

            // === FULL-WIDTH MODERN FLOOR-TO-CEILING GLASS BACK WALL (z = 3.55, x = -3.35 .. +3.35) ===
            // Clean continuous uniform top soffit & bottom rail
            CreateBox(parent, "BackGlass_TopHeader", new Vector3(0f, 3.04f, 3.55f), new Vector3(6.80f, 0.52f, 0.14f), wallUniform);
            CreateBox(parent, "BackGlass_TopTrack", new Vector3(0f, 2.75f, 3.55f), new Vector3(6.70f, 0.08f, 0.16f), steelFrame);
            CreateBox(parent, "BackGlass_BotTrackR", new Vector3(1.12f, 0.06f, 3.55f), new Vector3(4.36f, 0.12f, 0.16f), steelFrame);

            // --- UNMISTAKABLE MODERN GLASS OFFICE ENTRANCE DOOR (x = -2.35 .. -1.15) ---
            CreateBox(parent, "DoorJamb_L", new Vector3(-2.38f, 1.38f, 3.55f), new Vector3(0.09f, 2.76f, 0.18f), steelFrame);
            CreateBox(parent, "DoorJamb_R", new Vector3(-1.12f, 1.38f, 3.55f), new Vector3(0.09f, 2.76f, 0.18f), steelFrame);
            CreateBox(parent, "DoorTransomBar", new Vector3(-1.75f, 2.26f, 3.55f), new Vector3(1.26f, 0.09f, 0.18f), steelFrame);
            // Distinct thick aluminum-framed door leaf rails (top, bottom kickplate, left & right stiles)
            CreateBox(parent, "DoorLeaf_StileL", new Vector3(-2.29f, 1.12f, 3.55f), new Vector3(0.09f, 2.20f, 0.07f), new Color(0.24f, 0.27f, 0.32f));
            CreateBox(parent, "DoorLeaf_StileR", new Vector3(-1.21f, 1.12f, 3.55f), new Vector3(0.09f, 2.20f, 0.07f), new Color(0.24f, 0.27f, 0.32f));
            CreateBox(parent, "DoorLeaf_TopRail", new Vector3(-1.75f, 2.18f, 3.55f), new Vector3(1.16f, 0.10f, 0.07f), new Color(0.24f, 0.27f, 0.32f));
            CreateBox(parent, "DoorLeaf_Kickplate", new Vector3(-1.75f, 0.14f, 3.55f), new Vector3(1.16f, 0.26f, 0.07f), new Color(0.24f, 0.27f, 0.32f));
            // Door glass + subtle frosted privacy band + vertical push/pull chrome handle + overhead door closer + hinges!
            CreateGlassPane(parent, "Door_GlassPane", new Vector3(-1.75f, 1.20f, 3.55f), new Vector3(1.00f, 1.88f, 0.03f), new Color(0.75f, 0.88f, 0.96f, 0.24f));
            CreateGlassPane(parent, "Door_TransomGlass", new Vector3(-1.75f, 2.50f, 3.55f), new Vector3(1.18f, 0.44f, 0.03f), new Color(0.75f, 0.88f, 0.96f, 0.22f));
            CreateGlassPane(parent, "Door_FrostBand", new Vector3(-1.75f, 1.24f, 3.54f), new Vector3(0.98f, 0.34f, 0.035f), new Color(0.94f, 0.97f, 1.0f, 0.56f));
            // Long vertical brushed-steel office push/pull bar handle (BOTH inside & outside corridor side!)
            CreateBox(parent, "DoorHandle_BarIn", new Vector3(-1.32f, 1.08f, 3.45f), new Vector3(0.032f, 0.52f, 0.032f), silverMetal);
            CreateBox(parent, "DoorHandle_MountTop", new Vector3(-1.32f, 1.28f, 3.55f), new Vector3(0.03f, 0.03f, 0.20f), silverMetal);
            CreateBox(parent, "DoorHandle_MountBot", new Vector3(-1.32f, 0.88f, 3.55f), new Vector3(0.03f, 0.03f, 0.20f), silverMetal);
            CreateBox(parent, "DoorHandle_BarOut", new Vector3(-1.32f, 1.08f, 3.65f), new Vector3(0.032f, 0.52f, 0.032f), silverMetal);
            // 3 flush steel hinges cleanly on the left jamb seam (no automatic door closer box!) + room plaque
            CreateBox(parent, "DoorHinge1", new Vector3(-2.34f, 1.95f, 3.55f), new Vector3(0.025f, 0.10f, 0.09f), silverMetal);
            CreateBox(parent, "DoorHinge2", new Vector3(-2.34f, 1.15f, 3.55f), new Vector3(0.025f, 0.10f, 0.09f), silverMetal);
            CreateBox(parent, "DoorHinge3", new Vector3(-2.34f, 0.38f, 3.55f), new Vector3(0.025f, 0.10f, 0.09f), silverMetal);
            CreateBox(parent, "DoorPlaque", new Vector3(-1.75f, 1.62f, 3.52f), new Vector3(0.58f, 0.16f, 0.015f), new Color(0.12f, 0.22f, 0.36f));
            AddWorldSignText(parent, "DoorPlaqueText", new Vector3(-1.75f, 1.62f, 3.508f), Quaternion.identity, new Vector2(0.56f, 0.145f),
                "<color=#F4D47C><b>DIGITAL HEALTH OFFICE</b></color>\n<color=#DCE8F6>ROOM 504  •  CASE ZERO</color>", 26, new Color(0.95f, 0.97f, 1f));

            // Far-left sidelight glass bay (x = -3.28 .. -2.40)
            CreateBox(parent, "Mullion_FarL", new Vector3(-3.26f, 1.38f, 3.55f), new Vector3(0.08f, 2.76f, 0.16f), steelFrame);
            CreateBox(parent, "BotTrack_FarL", new Vector3(-2.82f, 0.06f, 3.55f), new Vector3(0.88f, 0.12f, 0.16f), steelFrame);
            CreateGlassPane(parent, "BackGlass_FarL", new Vector3(-2.82f, 1.42f, 3.55f), new Vector3(0.84f, 2.60f, 0.03f), new Color(0.74f, 0.88f, 0.96f, 0.20f));

            // --- WIDE FULL-GLASS PANORAMIC OFFICE PARTITION (x = -1.10 .. +3.28, 3 equal architectural bays!) ---
            float[] mullionsX = { 0.35f, 1.82f, 3.28f };
            foreach (float mx in mullionsX)
            {
                CreateBox(parent, $"BackMullion_{mx:F1}", new Vector3(mx, 1.38f, 3.55f), new Vector3(0.07f, 2.76f, 0.15f), steelFrame);
            }
            CreateGlassPane(parent, "BackGlass_MainSpan", new Vector3(1.10f, 1.42f, 3.55f), new Vector3(4.36f, 2.62f, 0.03f), new Color(0.74f, 0.88f, 0.96f, 0.18f));
            // Subtle modern frosted privacy stripe across the glass wall (consistent height with door frost band!)
            CreateGlassPane(parent, "BackGlass_FrostStrip", new Vector3(1.10f, 1.24f, 3.535f), new Vector3(4.32f, 0.34f, 0.035f), new Color(0.94f, 0.97f, 1.0f, 0.44f));

            // === RIGHT WALL (x = +3.35): Bookcase Section (z = +0.55..3.55) + PANORAMIC CITY WINDOW (z = -2.25..+0.55) ===
            CreateBox(parent, "RightWall_Back", new Vector3(3.35f, 1.65f, 2.05f), new Vector3(0.14f, 3.3f, 3.00f), wallUniform);
            CreateBox(parent, "RightWall_Front", new Vector3(3.35f, 1.65f, -2.70f), new Vector3(0.14f, 3.3f, 0.90f), wallUniform);
            CreateBox(parent, "RightWall_UnderWindow", new Vector3(3.35f, 0.44f, -0.85f), new Vector3(0.14f, 0.88f, 2.80f), wallUniform);
            CreateBox(parent, "RightWall_AboveWindow", new Vector3(3.35f, 2.98f, -0.85f), new Vector3(0.14f, 0.64f, 2.80f), wallUniform);
            CreateBox(parent, "Trim_Right", new Vector3(3.26f, 0.08f, 0.2f), new Vector3(0.04f, 0.16f, 6.6f), woodTrim);

            // Panoramic Window Frame + Sill + Blind + Clear Glass
            CreateBox(parent, "WinSill", new Vector3(3.30f, 0.89f, -0.85f), new Vector3(0.24f, 0.05f, 2.84f), new Color(0.90f, 0.89f, 0.86f));
            CreateBox(parent, "WinFrame_Top", new Vector3(3.35f, 2.64f, -0.85f), new Vector3(0.16f, 0.07f, 2.84f), steelFrame);
            CreateBox(parent, "WinFrame_L", new Vector3(3.35f, 1.76f, 0.53f), new Vector3(0.16f, 1.78f, 0.07f), steelFrame);
            CreateBox(parent, "WinFrame_R", new Vector3(3.35f, 1.76f, -2.23f), new Vector3(0.16f, 1.78f, 0.07f), steelFrame);
            CreateBox(parent, "WinFrame_Mid", new Vector3(3.35f, 1.76f, -0.85f), new Vector3(0.12f, 1.78f, 0.06f), steelFrame);
            CreateBox(parent, "WinBlind_Top", new Vector3(3.26f, 2.46f, -0.85f), new Vector3(0.05f, 0.30f, 2.72f), new Color(0.86f, 0.80f, 0.70f));
            CreateGlassPane(parent, "WinGlass_Pane", new Vector3(3.35f, 1.76f, -0.85f), new Vector3(0.03f, 1.72f, 2.74f), new Color(0.76f, 0.90f, 1.0f, 0.16f));
        }

        private static void BuildLivingCorridorAndAcrossOffice(Transform parent)
        {
            GameObject ext = new GameObject("Exterior_CorridorAndAcrossOffice");
            ext.transform.SetParent(parent, false);

            Color corrWall = new Color(0.80f, 0.82f, 0.85f);
            Color steelFrame = new Color(0.15f, 0.17f, 0.21f);
            Color navyAccent = new Color(0.16f, 0.30f, 0.48f);

            // === ENCLOSED MODERN OFFICE HALLWAY (z = 3.60 .. 6.40, x = -5.60 .. +5.60) ===
            CreateBox(ext.transform, "Corridor_Floor", new Vector3(0f, -0.05f, 5.0f), new Vector3(11.4f, 0.1f, 3.0f), new Color(0.72f, 0.74f, 0.77f));
            CreateBox(ext.transform, "Corridor_RunnerRug", new Vector3(0f, 0.004f, 5.0f), new Vector3(9.8f, 0.008f, 1.35f), new Color(0.24f, 0.32f, 0.42f), removeCollider: true);
            CreateBox(ext.transform, "Corridor_Ceiling", new Vector3(0f, 3.35f, 5.0f), new Vector3(11.4f, 0.1f, 3.0f), new Color(0.92f, 0.92f, 0.92f));
            // Close left and right hallway ends so you NEVER see grey Unity void!
            CreateBox(ext.transform, "Corridor_EndWallL", new Vector3(-5.6f, 1.65f, 5.0f), new Vector3(0.16f, 3.3f, 3.2f), corrWall);
            CreateBox(ext.transform, "Corridor_EndWallR", new Vector3(5.6f, 1.65f, 5.0f), new Vector3(0.16f, 3.3f, 3.2f), corrWall);
            // Hallway end doors for depth
            CreateBox(ext.transform, "Corridor_EndDoorL", new Vector3(-5.50f, 1.15f, 5.0f), new Vector3(0.06f, 2.3f, 1.1f), new Color(0.38f, 0.28f, 0.20f));
            CreateBox(ext.transform, "Corridor_EndDoorR", new Vector3(5.50f, 1.15f, 5.0f), new Vector3(0.06f, 2.3f, 1.1f), new Color(0.38f, 0.28f, 0.20f));

            // Corridor Ceiling Lights, Vending Machine, Plants, Waiting Nook (all cleanly aligned!)
            PlacePrefabClean(ext.transform, "Corr_LightL", P_CeilingLight2, new Vector3(-2.0f, 3.16f, 5.0f), 0.14f, 0f);
            PlacePrefabClean(ext.transform, "Corr_LightR", P_CeilingLight2, new Vector3(2.0f, 3.16f, 5.0f), 0.14f, 0f);
            PlacePrefabClean(ext.transform, "Corr_VendingMachine", P_VendingMachine, new Vector3(-3.65f, 0f, 6.02f), 1.85f, 90f);
            PlacePrefabClean(ext.transform, "Corr_PlantL", P_Plant1, new Vector3(-2.65f, 0f, 6.05f), 1.32f, 90f);
            PlacePrefabClean(ext.transform, "Corr_PlantR", P_Plant2, new Vector3(3.35f, 0f, 6.02f), 1.25f, 90f);
            // Small hallway lounge bench facing our office
            PlacePrefabClean(ext.transform, "Corr_BenchSofa", P_Sofa, new Vector3(4.35f, 0f, 5.90f), 0.84f, 90f);

            // === ACROSS-THE-HALL GLASS CONFERENCE ROOM (z = 6.45 .. 10.2, x = -2.2 .. +3.2)! ===
            // Solid side/back walls of the opposite office + its own glass front partition facing our hallway!
            CreateBox(ext.transform, "Across_Floor", new Vector3(0.5f, -0.05f, 8.3f), new Vector3(5.8f, 0.1f, 3.8f), new Color(0.36f, 0.28f, 0.21f));
            CreateBox(ext.transform, "Across_Ceiling", new Vector3(0.5f, 3.35f, 8.3f), new Vector3(5.8f, 0.1f, 3.8f), new Color(0.92f, 0.91f, 0.88f));
            CreateBox(ext.transform, "Across_BackWall", new Vector3(0.5f, 1.65f, 10.15f), new Vector3(5.8f, 3.3f, 0.16f), new Color(0.84f, 0.85f, 0.87f));
            CreateBox(ext.transform, "Across_SideWallL", new Vector3(-2.35f, 1.65f, 8.3f), new Vector3(0.16f, 3.3f, 3.8f), corrWall);
            CreateBox(ext.transform, "Across_SideWallR", new Vector3(3.35f, 1.65f, 8.3f), new Vector3(0.16f, 3.3f, 3.8f), corrWall);
            // Solid corridor wing walls flanking the opposite glass conference room
            CreateBox(ext.transform, "Corr_SouthWingL", new Vector3(-4.0f, 1.65f, 6.48f), new Vector3(3.3f, 3.3f, 0.16f), corrWall);
            CreateBox(ext.transform, "Corr_SouthWingR", new Vector3(4.5f, 1.65f, 6.48f), new Vector3(2.3f, 3.3f, 0.16f), corrWall);
            CreateBox(ext.transform, "Corr_AccentSign", new Vector3(-4.0f, 1.75f, 6.38f), new Vector3(1.6f, 0.45f, 0.03f), navyAccent);

            // Glass Front Wall of the Opposite Conference Room (at z = 6.48) WITH its own glass entrance door!
            CreateBox(ext.transform, "Across_GlassHeader", new Vector3(0.5f, 3.04f, 6.48f), new Vector3(5.7f, 0.52f, 0.14f), corrWall);
            CreateBox(ext.transform, "Across_TopBar", new Vector3(0.5f, 2.75f, 6.48f), new Vector3(5.6f, 0.08f, 0.16f), steelFrame);
            CreateBox(ext.transform, "Across_BotBar", new Vector3(1.15f, 0.08f, 6.48f), new Vector3(4.3f, 0.16f, 0.16f), steelFrame);
            foreach (float mx in new[] { -2.25f, -1.05f, 0.95f, 3.25f })
            {
                CreateBox(ext.transform, $"Across_Mullion_{mx:F1}", new Vector3(mx, 1.40f, 6.48f), new Vector3(0.07f, 2.70f, 0.14f), steelFrame);
            }
            // Opposite Conference Room Glass Entrance Door (x = -2.20 .. -1.10)
            Color acrossLeaf = new Color(0.24f, 0.27f, 0.32f);
            Color silverHandle = new Color(0.76f, 0.79f, 0.83f);
            CreateBox(ext.transform, "AcrossDoor_Transom",   new Vector3(-1.65f, 2.24f, 6.48f), new Vector3(1.18f, 0.08f, 0.14f), steelFrame, removeCollider: true);
            CreateBox(ext.transform, "AcrossDoor_StileL",    new Vector3(-2.17f, 1.12f, 6.48f), new Vector3(0.08f, 2.20f, 0.07f), acrossLeaf, removeCollider: true);
            CreateBox(ext.transform, "AcrossDoor_StileR",    new Vector3(-1.13f, 1.12f, 6.48f), new Vector3(0.08f, 2.20f, 0.07f), acrossLeaf, removeCollider: true);
            CreateBox(ext.transform, "AcrossDoor_TopRail",   new Vector3(-1.65f, 2.17f, 6.48f), new Vector3(1.10f, 0.09f, 0.07f), acrossLeaf, removeCollider: true);
            CreateBox(ext.transform, "AcrossDoor_Kickplate", new Vector3(-1.65f, 0.14f, 6.48f), new Vector3(1.10f, 0.24f, 0.07f), acrossLeaf, removeCollider: true);
            CreateBox(ext.transform, "AcrossDoor_HandleIn",   new Vector3(-1.24f, 1.08f, 6.38f), new Vector3(0.03f, 0.48f, 0.03f), silverHandle, removeCollider: true);
            CreateBox(ext.transform, "AcrossDoor_HandleOut",  new Vector3(-1.24f, 1.08f, 6.58f), new Vector3(0.03f, 0.48f, 0.03f), silverHandle, removeCollider: true);
            CreateBox(ext.transform, "AcrossDoor_MountTop",   new Vector3(-1.24f, 1.26f, 6.48f), new Vector3(0.028f, 0.028f, 0.20f), silverHandle, removeCollider: true);
            CreateBox(ext.transform, "AcrossDoor_MountBot",   new Vector3(-1.24f, 0.90f, 6.48f), new Vector3(0.028f, 0.028f, 0.20f), silverHandle, removeCollider: true);
            CreateBox(ext.transform, "AcrossDoor_Plaque",     new Vector3(-1.65f, 1.62f, 6.45f), new Vector3(0.54f, 0.15f, 0.015f), new Color(0.12f, 0.22f, 0.36f), removeCollider: true);
            AddWorldSignText(ext.transform, "AcrossDoor_PlaqueText", new Vector3(-1.65f, 1.62f, 6.438f), Quaternion.identity, new Vector2(0.52f, 0.14f),
                "<color=#F4D47C><b>MEETING ROOM</b></color>\n<color=#DCE8F6>ROOM 505</color>", 28, Color.white);
            CreateGlassPane(ext.transform, "Across_GlassPane", new Vector3(0.5f, 1.42f, 6.48f), new Vector3(5.45f, 2.60f, 0.03f), new Color(0.74f, 0.88f, 0.96f, 0.22f));

            // Inside the Opposite Conference Room: Conference Table, 4 Chairs, Wall Presentation Screen, Plant!
            PlacePrefabClean(ext.transform, "Across_ConfTable", P_ConferenceTable, new Vector3(0.55f, 0f, 8.25f), 0.76f, 90f);
            PlacePrefabClean(ext.transform, "Across_Chair1", P_Chair, new Vector3(-0.15f, 0f, 7.55f), 0.95f, 270f);
            PlacePrefabClean(ext.transform, "Across_Chair2", P_Chair, new Vector3(1.25f, 0f, 7.55f), 0.95f, 270f);
            PlacePrefabClean(ext.transform, "Across_Chair3", P_Chair, new Vector3(-0.15f, 0f, 8.95f), 0.95f, 90f);
            PlacePrefabClean(ext.transform, "Across_Chair4", P_Chair, new Vector3(1.25f, 0f, 8.95f), 0.95f, 90f);
            PlacePrefabClean(ext.transform, "Across_Plant", P_Plant1, new Vector3(2.65f, 0f, 9.45f), 1.35f, 90f);
            // Wall presentation TV / Briefing Slide on back wall of Conference Room (visible across the hallway!)
            CreateBox(ext.transform, "Across_ScreenFrame", new Vector3(0.55f, 1.75f, 10.02f), new Vector3(2.40f, 1.25f, 0.06f), steelFrame, removeCollider: true);
            CreateBox(ext.transform, "Across_ScreenFace", new Vector3(0.55f, 1.75f, 9.98f), new Vector3(2.26f, 1.12f, 0.02f), new Color(0.92f, 0.96f, 0.99f), removeCollider: true);
            CreateBox(ext.transform, "Across_ScreenAccent", new Vector3(0.55f, 2.16f, 9.96f), new Vector3(2.14f, 0.18f, 0.01f), navyAccent, removeCollider: true);
            // Colorful bar-chart visual on the opposite meeting room's presentation display
            CreateBox(ext.transform, "Across_Bar1", new Vector3(0.05f, 1.46f, 9.96f), new Vector3(0.16f, 0.32f, 0.01f), new Color(0.20f, 0.56f, 0.80f), removeCollider: true);
            CreateBox(ext.transform, "Across_Bar2", new Vector3(0.32f, 1.56f, 9.96f), new Vector3(0.16f, 0.52f, 0.01f), new Color(0.18f, 0.68f, 0.48f), removeCollider: true);
            CreateBox(ext.transform, "Across_Bar3", new Vector3(0.59f, 1.50f, 9.96f), new Vector3(0.16f, 0.40f, 0.01f), new Color(0.86f, 0.58f, 0.18f), removeCollider: true);
            CreateBox(ext.transform, "Across_Bar4", new Vector3(0.86f, 1.64f, 9.96f), new Vector3(0.16f, 0.68f, 0.01f), new Color(0.76f, 0.24f, 0.24f), removeCollider: true);
            AddWorldSignText(ext.transform, "Across_ScreenTitle", new Vector3(0.55f, 2.16f, 9.948f), Quaternion.identity, new Vector2(2.08f, 0.16f),
                "<b>eHEALTH LAB — VITAPULSE BRIEFING</b>", 28, Color.white);

            // === 5TH-FLOOR ELEVATED CITY SKYLINE (fills BOTH left & right panoramic window views!) ===
            // Street level is 11 meters below our 5th-floor office!
            CreateBox(ext.transform, "City_StreetGround", new Vector3(18f, -11.2f, -2f), new Vector3(32f, 0.4f, 44f), new Color(0.42f, 0.44f, 0.46f), removeCollider: true);

            // Midground avenue buildings (ONLY visible ones — removed unseen rear towers & hidden trees for clean rendering!)
            BuildCitySkyscraper(ext.transform, "City_FarRightLow1", new Vector3(14.8f, -11f, -21.0f), new Vector3(3.6f, 13.4f, 4.2f), new Color(0.82f, 0.72f, 0.62f), 5, 4);
            BuildCitySkyscraper(ext.transform, "City_FarRightLow2", new Vector3(15.2f, -11f, -15.8f), new Vector3(3.8f, 14.2f, 4.4f), new Color(0.76f, 0.76f, 0.72f), 5, 4);
            BuildCitySkyscraper(ext.transform, "City_Mid1", new Vector3(14.5f, -11f, -9.8f), new Vector3(3.4f, 21.0f, 3.8f), new Color(0.76f, 0.72f, 0.66f), 8, 4);
            BuildCitySkyscraper(ext.transform, "City_Mid2", new Vector3(15.2f, -11f, -5.4f), new Vector3(3.6f, 24.5f, 4.0f), new Color(0.64f, 0.70f, 0.78f), 9, 5);
            BuildCitySkyscraper(ext.transform, "City_Mid3", new Vector3(14.0f, -11f, -1.1f), new Vector3(3.2f, 19.5f, 3.6f), new Color(0.80f, 0.76f, 0.70f), 7, 4);

            // 7 widely-spaced fluffy clouds spanning the FULL blue sky height (y = 5.2 .. 11.2m), spawning off-screen right (z = -38) and wrapping behind the tall skyscrapers!
            BuildDriftingCloud(ext.transform, "SkyCloud_1", new Vector3(20.5f,  5.4f, -36.5f), 0.76f);
            BuildDriftingCloud(ext.transform, "SkyCloud_2", new Vector3(22.5f, 10.6f, -31.5f), 0.96f);
            BuildDriftingCloud(ext.transform, "SkyCloud_3", new Vector3(19.8f,  7.6f, -26.5f), 0.80f);
            BuildDriftingCloud(ext.transform, "SkyCloud_4", new Vector3(23.0f, 11.2f, -21.5f), 1.00f);
            BuildDriftingCloud(ext.transform, "SkyCloud_5", new Vector3(20.8f,  6.2f, -16.5f), 0.78f);
            BuildDriftingCloud(ext.transform, "SkyCloud_6", new Vector3(22.0f,  9.4f, -11.5f), 0.92f);
            BuildDriftingCloud(ext.transform, "SkyCloud_7", new Vector3(21.0f,  8.2f,  -6.5f), 0.84f);

            // Only the 3 boulevard trees actually visible through the panoramic window!
            float[] treeZs = { -11.5f, -6.5f, -2.6f };
            for (int t = 0; t < treeZs.Length; t++)
            {
                BuildOrganicBoulevardTree(ext.transform, $"City_Tree_{t}", new Vector3(10.5f, -11f, treeZs[t]));
            }
        }

        private static void BuildDriftingCloud(Transform parent, string name, Vector3 pos, float scale)
        {
            GameObject cloud = new GameObject(name);
            cloud.transform.SetParent(parent, false);
            cloud.transform.position = pos;
            cloud.transform.localScale = Vector3.one * scale;
            cloud.AddComponent<CloudDrift>();

            Color cWhite = new Color(0.97f, 0.98f, 1.0f);
            CreateSphere(cloud.transform, $"{name}_P1", new Vector3(0f, 0f, 0f), new Vector3(2.2f, 1.3f, 2.8f), cWhite);
            CreateSphere(cloud.transform, $"{name}_P2", new Vector3(0.2f, 0.4f, 1.3f), new Vector3(1.8f, 1.1f, 2.0f), cWhite);
            CreateSphere(cloud.transform, $"{name}_P3", new Vector3(-0.2f, 0.25f, -1.4f), new Vector3(1.7f, 1.0f, 1.9f), cWhite);
            CreateSphere(cloud.transform, $"{name}_P4", new Vector3(0f, 0.65f, 0.2f), new Vector3(1.5f, 1.0f, 1.6f), cWhite);
        }

        private static void BuildOrganicBoulevardTree(Transform parent, string name, Vector3 groundPos)
        {
            GameObject tree = new GameObject(name);
            tree.transform.SetParent(parent, false);
            tree.transform.position = groundPos;

            // Tall trunk rising from street level (-11m) up to ~-2.5m
            CreateBox(tree.transform, $"{name}_Trunk", new Vector3(0f, 4.2f, 0f), new Vector3(0.45f, 8.4f, 0.45f), new Color(0.34f, 0.25f, 0.18f), local: true, removeCollider: true);

            // Multi-sphere layered lush foliage crown (only top tips peek gently below the 5th-floor sill!)
            Color leaf1 = new Color(0.24f, 0.44f, 0.28f);
            Color leaf2 = new Color(0.30f, 0.52f, 0.32f);
            Color leaf3 = new Color(0.20f, 0.38f, 0.24f);
            CreateSphere(tree.transform, $"{name}_CrownMain", new Vector3(0f, 9.2f, 0f), new Vector3(2.8f, 2.3f, 2.8f), leaf1);
            CreateSphere(tree.transform, $"{name}_CrownTop", new Vector3(0.2f, 10.3f, 0.1f), new Vector3(2.0f, 1.7f, 2.0f), leaf2);
            CreateSphere(tree.transform, $"{name}_CrownSideA", new Vector3(-0.7f, 9.0f, 0.8f), new Vector3(1.9f, 1.6f, 1.9f), leaf3);
            CreateSphere(tree.transform, $"{name}_CrownSideB", new Vector3(0.6f, 8.8f, -0.9f), new Vector3(2.1f, 1.7f, 2.1f), leaf2);
        }

        private static void CreateSphere(Transform parent, string name, Vector3 localPos, Vector3 scale, Color color)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPos;
            obj.transform.localScale = scale;
            Collider c = obj.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);

            Renderer rend = obj.GetComponent<Renderer>();
            string matPath = $"{MaterialFolder}/Mat_{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            else mat.color = color;
            EditorUtility.SetDirty(mat);
            rend.sharedMaterial = mat;
        }

        private static void BuildCitySkyscraper(Transform parent, string name, Vector3 basePos, Vector3 size, Color facadeColor, int floors, int cols)
        {
            GameObject bldg = new GameObject(name);
            bldg.transform.SetParent(parent, false);
            bldg.transform.position = basePos;

            Color trimColor = facadeColor * 0.78f;
            Color glassBlue = new Color(0.36f, 0.56f, 0.75f);
            Color warmLit = new Color(0.96f, 0.88f, 0.62f);

            // Main building tower sitting cleanly on basePos.y
            CreateBox(bldg.transform, $"{name}_Body", new Vector3(0f, size.y * 0.5f, 0f), size, facadeColor, local: true, removeCollider: true);
            CreateBox(bldg.transform, $"{name}_RoofCap", new Vector3(0f, size.y + 0.12f, 0f), new Vector3(size.x + 0.18f, 0.24f, size.z + 0.18f), trimColor, local: true, removeCollider: true);
            CreateBox(bldg.transform, $"{name}_RoofUnit", new Vector3(0.2f, size.y + 0.45f, 0.2f), new Vector3(size.x * 0.35f, 0.55f, size.z * 0.35f), trimColor, local: true, removeCollider: true);

            // Window grid on the -X face (facing straight toward our office panoramic window!)
            float faceX = -size.x * 0.5f - 0.03f;
            float usableH = size.y - 2.4f;
            float usableZ = size.z - 0.7f;
            float winH = (usableH / floors) * 0.56f;
            float winW = (usableZ / cols) * 0.64f;

            for (int f = 0; f < floors; f++)
            {
                float wy = 1.4f + (f + 0.5f) * (usableH / floors);
                CreateBox(bldg.transform, $"{name}_Ledge_{f}", new Vector3(faceX - 0.02f, wy - winH * 0.65f, 0f), new Vector3(0.08f, 0.08f, size.z + 0.06f), trimColor, local: true, removeCollider: true);

                for (int c = 0; c < cols; c++)
                {
                    float wz = -usableZ * 0.5f + (c + 0.5f) * (usableZ / cols);
                    bool isWarmLit = ((f * 7 + c * 3) % 5 == 0);
                    Color wCol = isWarmLit ? warmLit : glassBlue;
                    CreateBox(bldg.transform, $"{name}_W_{f}_{c}", new Vector3(faceX, wy, wz), new Vector3(0.04f, winH, winW), wCol, local: true, removeCollider: true);
                }
            }
        }

        private static void BuildExecutiveDesk(Transform parent, Vector3 pos, float topY)
        {
            GameObject desk = new GameObject("OfficeDesk_Executive");
            desk.transform.SetParent(parent, false);
            desk.transform.position = pos;

            Color walnut = new Color(0.44f, 0.28f, 0.16f);
            Color darkSteel = new Color(0.16f, 0.18f, 0.22f);
            Color drawerFace = new Color(0.36f, 0.23f, 0.13f);
            Color handle = new Color(0.80f, 0.68f, 0.34f);

            CreateBox(desk.transform, "Desk_Top", new Vector3(0f, topY - 0.03f, 0f), new Vector3(1.92f, 0.06f, 0.94f), walnut, local: true);
            CreateBox(desk.transform, "Desk_ModestyPanel", new Vector3(0f, topY * 0.55f, 0.37f), new Vector3(1.78f, topY * 0.55f, 0.04f), drawerFace, local: true);
            CreateBox(desk.transform, "Desk_LegL", new Vector3(-0.87f, (topY - 0.06f) * 0.5f, 0f), new Vector3(0.06f, topY - 0.06f, 0.84f), darkSteel, local: true);
            CreateBox(desk.transform, "Desk_PedestalR", new Vector3(0.70f, (topY - 0.06f) * 0.5f, 0f), new Vector3(0.42f, topY - 0.06f, 0.82f), drawerFace, local: true);
            CreateBox(desk.transform, "Desk_Handle1", new Vector3(0.70f, 0.58f, -0.42f), new Vector3(0.14f, 0.025f, 0.025f), handle, local: true, removeCollider: true);
            CreateBox(desk.transform, "Desk_Handle2", new Vector3(0.70f, 0.38f, -0.42f), new Vector3(0.14f, 0.025f, 0.025f), handle, local: true, removeCollider: true);
            CreateBox(desk.transform, "Desk_Handle3", new Vector3(0.70f, 0.18f, -0.42f), new Vector3(0.14f, 0.025f, 0.025f), handle, local: true, removeCollider: true);
        }

        /// <summary>
        /// Sculpted sleek modern Cisco/Polycom-style dark-graphite Office IP Telephone
        /// with sloped console, backlit blue-teal caller-ID LCD display, keypad buttons,
        /// LED status light, and ergonomic left-side handset.
        /// </summary>
        private static GameObject BuildModernOfficeIPPhone(Transform parent, Vector3 pos)
        {
            GameObject phoneRoot = new GameObject("DeskTelephone");
            phoneRoot.transform.SetParent(parent, false);
            phoneRoot.transform.position = pos;
            phoneRoot.transform.rotation = Quaternion.Euler(0f, 20f, 0f);

            Color graphite = new Color(0.16f, 0.18f, 0.21f);
            Color charcoal = new Color(0.10f, 0.11f, 0.14f);
            Color lcdBlue = new Color(0.22f, 0.66f, 0.88f);
            Color keyGrey = new Color(0.70f, 0.74f, 0.78f);
            Color ledRed = new Color(0.92f, 0.18f, 0.18f);

            // Sleek low base foot + ergonomic sloped console facing STRAIGHT AT THE PLAYER (-22° tilt!)
            CreateBox(phoneRoot.transform, "IPPhone_Foot", new Vector3(0f, 0.014f, 0.01f), new Vector3(0.21f, 0.026f, 0.19f), charcoal, local: true, removeCollider: true);
            CreateBox(phoneRoot.transform, "IPPhone_StandBack", new Vector3(0f, 0.042f, 0.065f), new Vector3(0.18f, 0.056f, 0.05f), charcoal, local: true, removeCollider: true);
            GameObject console = new GameObject("ConsoleSloped");
            console.transform.SetParent(phoneRoot.transform, false);
            console.transform.localPosition = new Vector3(0f, 0.056f, 0.005f);
            console.transform.localRotation = Quaternion.Euler(-22f, 0f, 0f);

            CreateBox(console.transform, "IPPhone_Body", Vector3.zero, new Vector3(0.22f, 0.036f, 0.19f), graphite, local: true, removeCollider: true);
            // Glowing Caller-ID LCD Screen at upper-right angled straight toward player eyes
            CreateBox(console.transform, "IPPhone_LCDBezel", new Vector3(0.034f, 0.020f, 0.036f), new Vector3(0.125f, 0.008f, 0.068f), charcoal, local: true, removeCollider: true);
            CreateBox(console.transform, "IPPhone_LCDScreen", new Vector3(0.034f, 0.024f, 0.036f), new Vector3(0.112f, 0.004f, 0.056f), lcdBlue, local: true, removeCollider: true);
            // Status / Incoming-call LED bar at top right
            CreateBox(console.transform, "IPPhone_LED", new Vector3(0.082f, 0.023f, 0.078f), new Vector3(0.026f, 0.008f, 0.010f), ledRed, local: true, removeCollider: true);

            // 3x4 tactile keypad grid below LCD facing the player
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    float kx = 0.005f + col * 0.028f;
                    float kz = -0.018f - row * 0.019f;
                    CreateBox(console.transform, $"IPKey_{row}_{col}", new Vector3(kx, 0.021f, kz), new Vector3(0.018f, 0.007f, 0.012f), keyGrey, local: true, removeCollider: true);
                }
            }

            // Sleek ergonomic left-side Handset resting vertically along the left cradle
            CreateBox(console.transform, "IPPhone_HandsetGrip", new Vector3(-0.072f, 0.038f, 0f), new Vector3(0.034f, 0.026f, 0.185f), charcoal, local: true, removeCollider: true);
            CreateBox(console.transform, "IPPhone_HandsetEar", new Vector3(-0.072f, 0.028f, 0.072f), new Vector3(0.048f, 0.034f, 0.048f), graphite, local: true, removeCollider: true);
            CreateBox(console.transform, "IPPhone_HandsetMic", new Vector3(-0.072f, 0.028f, -0.072f), new Vector3(0.046f, 0.034f, 0.046f), graphite, local: true, removeCollider: true);

            return phoneRoot;
        }

        private static void BuildEnclosedOfficeBookcase(Transform parent, Vector3 pos)
        {
            GameObject shelf = new GameObject("OfficeBookcase");
            shelf.transform.SetParent(parent, false);
            shelf.transform.position = pos;

            Color woodDark = new Color(0.28f, 0.18f, 0.11f);
            Color woodMid = new Color(0.36f, 0.24f, 0.15f);

            CreateBox(shelf.transform, "BC_Back", new Vector3(0.21f, 1.02f, 0f), new Vector3(0.04f, 2.04f, 1.32f), woodDark, local: true, removeCollider: true);
            CreateBox(shelf.transform, "BC_SideL", new Vector3(0f, 1.02f, -0.64f), new Vector3(0.44f, 2.04f, 0.05f), woodMid, local: true, removeCollider: true);
            CreateBox(shelf.transform, "BC_SideR", new Vector3(0f, 1.02f, 0.64f), new Vector3(0.44f, 2.04f, 0.05f), woodMid, local: true, removeCollider: true);
            CreateBox(shelf.transform, "BC_Top", new Vector3(-0.02f, 2.04f, 0f), new Vector3(0.50f, 0.06f, 1.40f), woodDark, local: true, removeCollider: true);
            CreateBox(shelf.transform, "BC_Plinth", new Vector3(0f, 0.06f, 0f), new Vector3(0.44f, 0.12f, 1.32f), woodDark, local: true, removeCollider: true);

            CreateBox(shelf.transform, "BC_DoorL", new Vector3(-0.21f, 0.38f, -0.30f), new Vector3(0.03f, 0.52f, 0.58f), woodMid, local: true, removeCollider: true);
            CreateBox(shelf.transform, "BC_DoorR", new Vector3(-0.21f, 0.38f, 0.30f), new Vector3(0.03f, 0.52f, 0.58f), woodMid, local: true, removeCollider: true);
            CreateBox(shelf.transform, "BC_KnobL", new Vector3(-0.24f, 0.44f, -0.05f), new Vector3(0.03f, 0.06f, 0.025f), new Color(0.84f, 0.68f, 0.28f), local: true, removeCollider: true);
            CreateBox(shelf.transform, "BC_KnobR", new Vector3(-0.24f, 0.44f, 0.05f), new Vector3(0.03f, 0.06f, 0.025f), new Color(0.84f, 0.68f, 0.28f), local: true, removeCollider: true);

            float[] shelfYs = { 0.66f, 1.12f, 1.58f };
            foreach (float y in shelfYs)
            {
                CreateBox(shelf.transform, $"BC_Shelf_{y:F2}", new Vector3(0f, y, 0f), new Vector3(0.42f, 0.035f, 1.24f), woodMid, local: true, removeCollider: true);
            }

            // === DETAILED HARDCOVER BOOKS WITH WHITE PAGES, SPINE BANDS & READABLE 3D SPINE TITLES! ===
            Color goldAccent = new Color(0.88f, 0.74f, 0.32f);
            Color silverAccent = new Color(0.82f, 0.84f, 0.88f);

            // Shelf 1 (y = 0.678): Zero-overlap neat packed rows + leaning book + natively horizontal stacked books + "PRIVACY"!
            CreateDetailedBook(shelf.transform, "Book_Ethics",   new Vector3(0.02f, 0.678f, -0.55f), 0.28f, 0.055f, 0.22f, new Color(0.18f, 0.26f, 0.38f), "ETHICS",   goldAccent);
            CreateDetailedBook(shelf.transform, "Book_GDPR",     new Vector3(0.02f, 0.678f, -0.49f), 0.26f, 0.052f, 0.21f, new Color(0.32f, 0.22f, 0.20f), "GDPR",     silverAccent);
            CreateDetailedBook(shelf.transform, "Book_Clinical", new Vector3(0.02f, 0.678f, -0.43f), 0.29f, 0.056f, 0.22f, new Color(0.20f, 0.34f, 0.34f), "CLINICAL", goldAccent);
            CreateDetailedBook(shelf.transform, "Book_Stats",    new Vector3(0.02f, 0.678f, -0.37f), 0.25f, 0.050f, 0.20f, new Color(0.36f, 0.30f, 0.22f), "STATS",    silverAccent);
            // Leaning book gently resting INWARD against Book_Stats (-14 deg around X tilts top toward -Z!)
            GameObject lean1 = CreateDetailedBook(shelf.transform, "Book_Lean1", new Vector3(0.02f, 0.678f, -0.27f), 0.25f, 0.046f, 0.20f, new Color(0.42f, 0.24f, 0.28f), "REPORT", goldAccent);
            lean1.transform.localRotation = Quaternion.Euler(-14f, 0f, 0f);
            // Cleanly stacked horizontal books centered at z = -0.06 (spanning -0.18 .. +0.06 — completely clear of PRIVACY at +0.16!)
            CreateHorizontalBook(shelf.transform, "Book_Flat1", new Vector3(0.02f, 0.678f, -0.05f), 0.24f, 0.048f, 0.20f, new Color(0.24f, 0.30f, 0.38f), "INDEX",  silverAccent);
            CreateHorizontalBook(shelf.transform, "Book_Flat2", new Vector3(0.02f, 0.727f, -0.05f), 0.22f, 0.044f, 0.19f, new Color(0.46f, 0.32f, 0.18f), "DIGEST", goldAccent);

            // Clickable Easter-Egg Book #1: Digital Privacy 101 (Emerald Green, pulled slightly forward!)
            GameObject bookPrivacy = CreateDetailedBook(shelf.transform, "Book_Privacy101", new Vector3(-0.02f, 0.678f, 0.16f), 0.28f, 0.060f, 0.22f, new Color(0.14f, 0.44f, 0.30f), "PRIVACY", goldAccent);
            AddCleanColliderAndInteractable(bookPrivacy, "book_easter_privacy", "Book: 'Digital Privacy 101' (Read)", "Bookshelf", new Vector3(0.24f, 0.30f, 0.10f), new Vector3(0f, 0.14f, 0f));

            CreateDetailedBook(shelf.transform, "Book_Cases24",  new Vector3(0.02f, 0.678f, 0.24f), 0.27f, 0.054f, 0.22f, new Color(0.26f, 0.28f, 0.34f), "FILES 24", silverAccent);
            CreateDetailedBook(shelf.transform, "Book_Cases25",  new Vector3(0.02f, 0.678f, 0.30f), 0.27f, 0.054f, 0.22f, new Color(0.26f, 0.28f, 0.34f), "FILES 25", silverAccent);
            CreateDetailedBook(shelf.transform, "Book_Law",      new Vector3(0.02f, 0.678f, 0.36f), 0.29f, 0.056f, 0.22f, new Color(0.42f, 0.20f, 0.20f), "EU LAW",   goldAccent);
            CreateDetailedBook(shelf.transform, "Book_Audit",    new Vector3(0.02f, 0.678f, 0.42f), 0.26f, 0.052f, 0.21f, new Color(0.22f, 0.32f, 0.38f), "AUDIT",    silverAccent);
            CreateDetailedBook(shelf.transform, "Book_Regs",     new Vector3(0.02f, 0.678f, 0.48f), 0.28f, 0.054f, 0.22f, new Color(0.34f, 0.26f, 0.22f), "REGS",     goldAccent);

            // Shelf 2 (y = 1.138): Eye-Level Shelf with INVESTIGATION MANUAL + Clickable "MYTHS" + Zero-overlap neat rows!
            CreateDetailedBook(shelf.transform, "Book_Media",    new Vector3(0.02f, 1.138f, -0.55f), 0.26f, 0.052f, 0.21f, new Color(0.30f, 0.34f, 0.40f), "MEDIA",    silverAccent);
            CreateDetailedBook(shelf.transform, "Book_Logic",    new Vector3(0.02f, 1.138f, -0.49f), 0.24f, 0.048f, 0.20f, new Color(0.44f, 0.34f, 0.22f), "LOGIC",    goldAccent);
            CreateDetailedBook(shelf.transform, "Book_Data",     new Vector3(0.02f, 1.138f, -0.43f), 0.27f, 0.052f, 0.21f, new Color(0.22f, 0.36f, 0.42f), "DATA",     silverAccent);
            CreateDetailedBook(shelf.transform, "Book_Facts",    new Vector3(0.02f, 1.138f, -0.37f), 0.25f, 0.050f, 0.21f, new Color(0.32f, 0.26f, 0.34f), "FACTS",    goldAccent);
            CreateDetailedBook(shelf.transform, "Book_Sources",  new Vector3(0.02f, 1.138f, -0.31f), 0.28f, 0.054f, 0.22f, new Color(0.20f, 0.32f, 0.28f), "SOURCES",  silverAccent);
            // Sleek dark steel bookend right against Book_Sources
            CreateBox(shelf.transform, "Bookend_MidL", new Vector3(0.02f, 1.21f, -0.27f), new Vector3(0.16f, 0.14f, 0.012f), new Color(0.16f, 0.17f, 0.20f), local: true, removeCollider: true);

            // THE INVESTIGATION MANUAL: Sleek Navy-Blue & Gold Leather Handbook, upright & pulled 5cm forward with red bookmark!
            GameObject manual = CreateDetailedBook(shelf.transform, "InvestigationManual", new Vector3(-0.04f, 1.138f, -0.05f), 0.30f, 0.068f, 0.23f, new Color(0.11f, 0.20f, 0.38f), "MANUAL", goldAccent);
            CreateBox(manual.transform, "ManualBookmark", new Vector3(-0.08f, 0.315f, 0f), new Vector3(0.015f, 0.04f, 0.022f), new Color(0.84f, 0.16f, 0.16f), local: true, removeCollider: true);
            AddCleanColliderAndInteractable(manual, "bookshelf_manual", "Investigation Manual (Pick Up — Essential!)", "Bookshelf", new Vector3(0.26f, 0.34f, 0.14f), new Vector3(0f, 0.15f, 0f));

            // Clickable Easter-Egg Book #2: 101 Viral Health Myths (Deep Burgundy, pulled slightly forward!)
            GameObject bookMyths = CreateDetailedBook(shelf.transform, "Book_HealthMyths", new Vector3(-0.02f, 1.138f, 0.16f), 0.27f, 0.058f, 0.21f, new Color(0.56f, 0.16f, 0.18f), "MYTHS", goldAccent);
            AddCleanColliderAndInteractable(bookMyths, "book_easter_myths", "Book: '101 Viral Health Myths' (Read)", "Bookshelf", new Vector3(0.24f, 0.30f, 0.10f), new Vector3(0f, 0.14f, 0f));

            CreateDetailedBook(shelf.transform, "Book_Journal",  new Vector3(0.02f, 1.138f, 0.24f), 0.25f, 0.050f, 0.20f, new Color(0.24f, 0.36f, 0.28f), "JOURNAL",  silverAccent);
            CreateDetailedBook(shelf.transform, "Book_Notes",    new Vector3(0.02f, 1.138f, 0.30f), 0.23f, 0.048f, 0.20f, new Color(0.46f, 0.38f, 0.26f), "NOTES",    goldAccent);
            CreateDetailedBook(shelf.transform, "Book_Trial",    new Vector3(0.02f, 1.138f, 0.36f), 0.27f, 0.054f, 0.21f, new Color(0.28f, 0.30f, 0.38f), "TRIALS",   silverAccent);
            // Leaning book resting naturally INWARD against Book_Trial!
            GameObject lean2 = CreateDetailedBook(shelf.transform, "Book_Lean2", new Vector3(0.02f, 1.138f, 0.46f), 0.25f, 0.048f, 0.20f, new Color(0.38f, 0.24f, 0.20f), "REVIEW", goldAccent);
            lean2.transform.localRotation = Quaternion.Euler(-14f, 0f, 0f);

            // Shelf 3 (y = 1.598): Upper Reference Shelf + Clean Horizontal Stack + Clickable "COFFEE"!
            CreateDetailedBook(shelf.transform, "Book_Archive1", new Vector3(0.02f, 1.598f, -0.55f), 0.26f, 0.052f, 0.21f, new Color(0.28f, 0.26f, 0.32f), "VOL I",    silverAccent);
            CreateDetailedBook(shelf.transform, "Book_Archive2", new Vector3(0.02f, 1.598f, -0.49f), 0.26f, 0.052f, 0.21f, new Color(0.28f, 0.26f, 0.32f), "VOL II",   silverAccent);
            CreateDetailedBook(shelf.transform, "Book_Archive3", new Vector3(0.02f, 1.598f, -0.43f), 0.26f, 0.052f, 0.21f, new Color(0.28f, 0.26f, 0.32f), "VOL III",  silverAccent);
            CreateDetailedBook(shelf.transform, "Book_Archive4", new Vector3(0.02f, 1.598f, -0.37f), 0.26f, 0.052f, 0.21f, new Color(0.28f, 0.26f, 0.32f), "VOL IV",   silverAccent);
            // Cleanly stacked horizontal WHO reports centered at z = -0.15 (spanning -0.27 .. -0.03 — completely clear of COFFEE at +0.10!)
            CreateHorizontalBook(shelf.transform, "Book_Flat3", new Vector3(0.02f, 1.598f, -0.15f), 0.24f, 0.046f, 0.20f, new Color(0.20f, 0.34f, 0.30f), "WHO 24", goldAccent);
            CreateHorizontalBook(shelf.transform, "Book_Flat4", new Vector3(0.02f, 1.645f, -0.15f), 0.22f, 0.042f, 0.19f, new Color(0.40f, 0.22f, 0.22f), "WHO 25", silverAccent);

            // Clickable Easter-Egg Book #3: Detective's Coffee Guide (Warm Caramel!)
            GameObject bookCoffee = CreateDetailedBook(shelf.transform, "Book_CoffeeGuide", new Vector3(-0.02f, 1.598f, 0.10f), 0.25f, 0.058f, 0.21f, new Color(0.62f, 0.40f, 0.18f), "COFFEE", goldAccent);
            AddCleanColliderAndInteractable(bookCoffee, "book_easter_coffee", "Book: 'Detective's Coffee Guide' (Read)", "Bookshelf", new Vector3(0.24f, 0.28f, 0.10f), new Vector3(0f, 0.13f, 0f));

            CreateDetailedBook(shelf.transform, "Book_PubHealth",new Vector3(0.02f, 1.598f, 0.20f), 0.28f, 0.054f, 0.22f, new Color(0.18f, 0.34f, 0.44f), "eHEALTH",  goldAccent);
            CreateDetailedBook(shelf.transform, "Book_Guide",    new Vector3(0.02f, 1.598f, 0.26f), 0.25f, 0.050f, 0.21f, new Color(0.34f, 0.24f, 0.30f), "POLICY",   silverAccent);
            CreateDetailedBook(shelf.transform, "Book_Atlas",    new Vector3(0.02f, 1.598f, 0.32f), 0.27f, 0.054f, 0.22f, new Color(0.26f, 0.36f, 0.30f), "ATLAS",    silverAccent);
            CreateDetailedBook(shelf.transform, "Book_MedRef",   new Vector3(0.02f, 1.598f, 0.38f), 0.29f, 0.056f, 0.22f, new Color(0.20f, 0.24f, 0.34f), "MED REF",  goldAccent);
            CreateDetailedBook(shelf.transform, "Book_Manual2",  new Vector3(0.02f, 1.598f, 0.45f), 0.24f, 0.048f, 0.20f, new Color(0.44f, 0.30f, 0.20f), "FIELD",    silverAccent);
        }

        /// <summary>
        /// Builds a natively horizontal hardcover book sitting flat on a shelf (zero rotation clipping!)
        /// with its spine facing -X and right-side-up readable lettering!
        /// </summary>
        private static void CreateHorizontalBook(
            Transform parent, string name, Vector3 bottomCenterLocalPos,
            float lengthZ, float thicknessY, float depthX,
            Color coverColor, string spineTitle, Color accentColor)
        {
            GameObject book = new GameObject(name);
            book.transform.SetParent(parent, false);
            book.transform.localPosition = bottomCenterLocalPos;

            float boardT = 0.005f;
            Color pageCream = new Color(0.95f, 0.93f, 0.87f);

            // Spine facing -X
            CreateBox(book.transform, $"{name}_Spine",  new Vector3(-depthX * 0.5f + 0.005f, thicknessY * 0.5f, 0f), new Vector3(0.010f, thicknessY, lengthZ), coverColor, local: true, removeCollider: true);
            // Bottom & Top Hardcover Boards
            CreateBox(book.transform, $"{name}_CoverB", new Vector3(0f, boardT * 0.5f, 0f), new Vector3(depthX, boardT, lengthZ), coverColor, local: true, removeCollider: true);
            CreateBox(book.transform, $"{name}_CoverT", new Vector3(0f, thicknessY - boardT * 0.5f, 0f), new Vector3(depthX, boardT, lengthZ), coverColor, local: true, removeCollider: true);
            // Inset Cream Paper Pages
            CreateBox(book.transform, $"{name}_Pages",  new Vector3(0.005f, thicknessY * 0.5f, 0f), new Vector3(depthX - 0.012f, thicknessY - boardT * 2.1f, lengthZ - 0.010f), pageCream, local: true, removeCollider: true);

            if (!string.IsNullOrEmpty(spineTitle))
            {
                GameObject txtObj = new GameObject($"{name}_SpineTitle");
                txtObj.transform.SetParent(book.transform, false);
                txtObj.transform.localPosition = new Vector3(-depthX * 0.5f - 0.003f, thicknessY * 0.5f, 0f);
                txtObj.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                txtObj.transform.localScale = Vector3.one * 0.007f;

                TextMesh tm = txtObj.AddComponent<TextMesh>();
                tm.text = spineTitle;
                tm.fontSize = 32;
                tm.fontStyle = FontStyle.Bold;
                tm.anchor = TextAnchor.MiddleCenter;
                tm.alignment = TextAlignment.Center;
                tm.color = accentColor;
            }
        }

        /// <summary>
        /// Sculpted classic office 19L Damacana Water Cooler with inverted bottle neck,
        /// tapered dome shoulder, ribbed translucent polycarbonate bottle, visible water fill inside,
        /// hot (red) & cold (blue) taps, drip grate, and cup dispenser!
        /// </summary>
        private static void BuildOfficeWaterCooler(Transform parent, Vector3 pos)
        {
            GameObject cooler = new GameObject("OfficeWaterCooler");
            cooler.transform.SetParent(parent, false);
            cooler.transform.position = pos;

            Color bodyWhite = new Color(0.92f, 0.93f, 0.95f);
            Color darkTrim = new Color(0.18f, 0.20f, 0.24f);
            Color cupWhite = new Color(0.96f, 0.96f, 0.94f);

            // Upright white cabinet body + base plinth + recessed tap alcove + drip tray
            CreateBox(cooler.transform, "Cooler_Base", new Vector3(0f, 0.46f, 0f), new Vector3(0.36f, 0.92f, 0.36f), bodyWhite, local: true);
            CreateBox(cooler.transform, "Cooler_Foot", new Vector3(0f, 0.02f, 0f), new Vector3(0.38f, 0.04f, 0.38f), darkTrim, local: true, removeCollider: true);
            CreateBox(cooler.transform, "Cooler_Alcove", new Vector3(0.145f, 0.70f, 0f), new Vector3(0.09f, 0.22f, 0.26f), darkTrim, local: true, removeCollider: true);
            CreateBox(cooler.transform, "Cooler_DripTray", new Vector3(0.19f, 0.59f, 0f), new Vector3(0.09f, 0.024f, 0.26f), new Color(0.32f, 0.34f, 0.38f), local: true, removeCollider: true);
            CreateBox(cooler.transform, "Cooler_TapHot", new Vector3(0.195f, 0.78f, -0.055f), new Vector3(0.035f, 0.038f, 0.024f), new Color(0.86f, 0.16f, 0.16f), local: true, removeCollider: true);
            CreateBox(cooler.transform, "Cooler_TapCold", new Vector3(0.195f, 0.78f,  0.055f), new Vector3(0.035f, 0.038f, 0.024f), new Color(0.16f, 0.48f, 0.88f), local: true, removeCollider: true);

            // Round white collar seat on top of cabinet
            GameObject collar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            collar.name = "Cooler_RoundCollar";
            collar.transform.SetParent(cooler.transform, false);
            collar.transform.localPosition = new Vector3(0f, 0.945f, 0f);
            collar.transform.localScale = new Vector3(0.22f, 0.028f, 0.22f);
            Object.DestroyImmediate(collar.GetComponent<Collider>());
            ApplyColorMaterial(collar.GetComponent<Renderer>(), "CoolerCollar", new Color(0.82f, 0.84f, 0.88f));

            // Shared translucent polycarbonate blue material for the sculpted 19L damacana bottle!
            Material jMat = GetOrCreateTransparentMaterial("DamacanaGlass", new Color(0.34f, 0.74f, 0.98f, 0.44f));
            Material ribMat = GetOrCreateTransparentMaterial("DamacanaRibs", new Color(0.22f, 0.62f, 0.90f, 0.58f));
            Material waterMat = GetOrCreateTransparentMaterial("DamacanaWaterFill", new Color(0.18f, 0.58f, 0.92f, 0.52f));

            // 1) Inverted bottle neck plugged into the collar
            CreateCoolerPart(cooler.transform, PrimitiveType.Cylinder, "Jug_Neck", new Vector3(0f, 0.99f, 0f), new Vector3(0.085f, 0.045f, 0.085f), jMat);
            // 2) Tapered rounded shoulder (inverted damacana shoulder!)
            CreateCoolerPart(cooler.transform, PrimitiveType.Sphere, "Jug_Shoulder", new Vector3(0f, 1.075f, 0f), new Vector3(0.27f, 0.14f, 0.27f), jMat);
            // 3) Main 19L cylindrical body
            CreateCoolerPart(cooler.transform, PrimitiveType.Cylinder, "Jug_Body", new Vector3(0f, 1.245f, 0f), new Vector3(0.27f, 0.165f, 0.27f), jMat);
            // 4) Rounded top base cap (inverted bottom of damacana!)
            CreateCoolerPart(cooler.transform, PrimitiveType.Sphere, "Jug_TopDome", new Vector3(0f, 1.405f, 0f), new Vector3(0.265f, 0.075f, 0.265f), jMat);
            // 5) 3 realistic molded structural ribs around the damacana waist
            CreateCoolerPart(cooler.transform, PrimitiveType.Cylinder, "Jug_Rib1", new Vector3(0f, 1.16f, 0f), new Vector3(0.278f, 0.012f, 0.278f), ribMat);
            CreateCoolerPart(cooler.transform, PrimitiveType.Cylinder, "Jug_Rib2", new Vector3(0f, 1.245f, 0f), new Vector3(0.278f, 0.012f, 0.278f), ribMat);
            CreateCoolerPart(cooler.transform, PrimitiveType.Cylinder, "Jug_Rib3", new Vector3(0f, 1.33f, 0f), new Vector3(0.278f, 0.012f, 0.278f), ribMat);
            // 6) Visible inner water level (70% full!)
            CreateCoolerPart(cooler.transform, PrimitiveType.Cylinder, "Jug_WaterInside", new Vector3(0f, 1.19f, 0f), new Vector3(0.242f, 0.125f, 0.242f), waterMat);

            // Cylindrical paper cup dispenser tube on the right side
            GameObject cupTube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cupTube.name = "Cooler_CupTube";
            cupTube.transform.SetParent(cooler.transform, false);
            cupTube.transform.localPosition = new Vector3(0.02f, 0.72f, 0.215f);
            cupTube.transform.localScale = new Vector3(0.072f, 0.145f, 0.072f);
            Object.DestroyImmediate(cupTube.GetComponent<Collider>());
            ApplyColorMaterial(cupTube.GetComponent<Renderer>(), "CoolerCupTube", cupWhite);
        }

        private static void CreateCoolerPart(Transform parent, PrimitiveType type, string name, Vector3 localPos, Vector3 scale, Material mat)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPos;
            obj.transform.localScale = scale;
            Object.DestroyImmediate(obj.GetComponent<Collider>());
            Renderer r = obj.GetComponent<Renderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
        }

        private static Material GetOrCreateTransparentMaterial(string matName, Color tint)
        {
            string matPath = $"{MaterialFolder}/Mat_{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                AssetDatabase.CreateAsset(mat, matPath);
            }
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_Smoothness", 0.92f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)RenderQueue.Transparent;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
            else mat.color = tint;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        /// <summary>
        /// Builds a realistic hardcover book standing on a right-wall shelf (spine faces -X into the room)
        /// with front/back cover boards, cream page block, metallic spine bands, and readable 3D spine lettering!
        /// </summary>
        private static GameObject CreateDetailedBook(
            Transform parent,
            string name,
            Vector3 bottomCenterLocalPos,
            float height,
            float thickness,
            float depth,
            Color coverColor,
            string spineTitle,
            Color accentColor)
        {
            GameObject book = new GameObject(name);
            book.transform.SetParent(parent, false);
            book.transform.localPosition = bottomCenterLocalPos;

            float boardThick = 0.006f;
            Color pageCream = new Color(0.95f, 0.93f, 0.87f);

            // Spine (facing -X)
            CreateBox(book.transform, $"{name}_Spine", new Vector3(-depth * 0.5f + 0.006f, height * 0.5f, 0f), new Vector3(0.012f, height, thickness), coverColor, local: true, removeCollider: true);
            // Left & Right Hardcover Boards
            CreateBox(book.transform, $"{name}_CoverL", new Vector3(0f, height * 0.5f, -thickness * 0.5f + boardThick * 0.5f), new Vector3(depth, height, boardThick), coverColor, local: true, removeCollider: true);
            CreateBox(book.transform, $"{name}_CoverR", new Vector3(0f, height * 0.5f,  thickness * 0.5f - boardThick * 0.5f), new Vector3(depth, height, boardThick), coverColor, local: true, removeCollider: true);
            // Inset Cream-White Paper Pages Block
            CreateBox(book.transform, $"{name}_Pages", new Vector3(0.006f, height * 0.5f, 0f), new Vector3(depth - 0.014f, height - 0.012f, thickness - boardThick * 2.1f), pageCream, local: true, removeCollider: true);
            // Metallic decorative bands on the spine
            CreateBox(book.transform, $"{name}_BandTop", new Vector3(-depth * 0.5f - 0.001f, height * 0.84f, 0f), new Vector3(0.004f, 0.010f, thickness + 0.002f), accentColor, local: true, removeCollider: true);
            CreateBox(book.transform, $"{name}_BandBot", new Vector3(-depth * 0.5f - 0.001f, height * 0.16f, 0f), new Vector3(0.004f, 0.010f, thickness + 0.002f), accentColor, local: true, removeCollider: true);

            // Crisp readable 3D Spine Title facing -X (out into the room!)
            if (!string.IsNullOrEmpty(spineTitle))
            {
                GameObject txtObj = new GameObject($"{name}_SpineTitle");
                txtObj.transform.SetParent(book.transform, false);
                txtObj.transform.localPosition = new Vector3(-depth * 0.5f - 0.004f, height * 0.50f, 0f);
                // Rotate so text faces -X and reads vertically along the spine
                txtObj.transform.localRotation = Quaternion.Euler(0f, 90f, 90f);
                txtObj.transform.localScale = Vector3.one * 0.008f;

                TextMesh tm = txtObj.AddComponent<TextMesh>();
                tm.text = spineTitle;
                tm.fontSize = 34;
                tm.fontStyle = FontStyle.Bold;
                tm.anchor = TextAnchor.MiddleCenter;
                tm.alignment = TextAlignment.Center;
                tm.color = accentColor;
            }

            return book;
        }

        private static GameObject PlacePrefabClean(
            Transform parent,
            string containerName,
            string prefabPath,
            Vector3 bottomCenterWorldPos,
            float targetHeightMeters,
            float yRotationDegrees)
        {
            GameObject container = new GameObject(containerName);
            container.transform.SetParent(parent, false);
            container.transform.position = bottomCenterWorldPos;
            container.transform.rotation = Quaternion.Euler(0f, yRotationDegrees, 0f);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[HealthFiles] Prefab not found: {prefabPath}");
                return container;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, container.transform);
            instance.transform.localPosition = Vector3.zero;

            foreach (Collider col in instance.GetComponentsInChildren<Collider>())
            {
                Object.DestroyImmediate(col);
            }

            Bounds b = GetWorldBounds(instance);
            if (b.size.y > 0.001f && targetHeightMeters > 0f)
            {
                float scaleFactor = targetHeightMeters / b.size.y;
                instance.transform.localScale *= scaleFactor;
                b = GetWorldBounds(instance);
            }

            Vector3 currentBottomCenter = new Vector3(b.center.x, b.min.y, b.center.z);
            instance.transform.position += (bottomCenterWorldPos - currentBottomCenter);

            return container;
        }

        private static GameObject BuildMailLogClipboard(Transform parent, Vector3 pos)
        {
            GameObject root = new GameObject("MailDeliveryLog");
            root.transform.SetParent(parent, false);
            root.transform.position = pos;
            root.transform.rotation = Quaternion.Euler(0f, -14f, 0f);

            CreateBox(root.transform, "ClipboardWood", new Vector3(0f, 0.007f, 0f), new Vector3(0.22f, 0.014f, 0.30f), new Color(0.44f, 0.27f, 0.15f), local: true, removeCollider: true);
            CreateBox(root.transform, "LogPaper", new Vector3(0f, 0.016f, -0.01f), new Vector3(0.19f, 0.005f, 0.25f), new Color(0.95f, 0.93f, 0.87f), local: true, removeCollider: true);
            CreateBox(root.transform, "LogClip", new Vector3(0f, 0.022f, 0.12f), new Vector3(0.08f, 0.012f, 0.032f), new Color(0.66f, 0.68f, 0.72f), local: true, removeCollider: true);

            // High-res (720x940) crisp WorldSpace Canvas cleanly 2.5mm above LogPaper (zero Z-fighting, exact same text as modal!)
            GameObject canvasGo = new GameObject("ClipboardPaperTextCanvas");
            canvasGo.transform.SetParent(root.transform, false);
            canvasGo.transform.localPosition = new Vector3(0f, 0.021f, -0.01f);
            canvasGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            Canvas c = canvasGo.AddComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            RectTransform crt = canvasGo.GetComponent<RectTransform>();
            crt.sizeDelta = new Vector2(720f, 940f);
            crt.localScale = new Vector3(0.186f / 720f, 0.244f / 940f, 1f);

            Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Crisp navy header bar built directly in UI (zero 3D cube Z-fighting!)
            GameObject hdrBar = new GameObject("HeaderBar");
            hdrBar.transform.SetParent(canvasGo.transform, false);
            Image hImg = hdrBar.AddComponent<Image>();
            hImg.color = new Color(0.13f, 0.28f, 0.48f, 1f);
            hImg.raycastTarget = false;
            RectTransform hRt = hdrBar.GetComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0.04f, 1f); hRt.anchorMax = new Vector2(0.96f, 1f); hRt.pivot = new Vector2(0.5f, 1f);
            hRt.sizeDelta = new Vector2(0f, 110f); hRt.anchoredPosition = new Vector2(0f, -45f);

            GameObject hTxtGo = new GameObject("HeaderTxt");
            hTxtGo.transform.SetParent(hdrBar.transform, false);
            RectTransform htRt = hTxtGo.AddComponent<RectTransform>();
            htRt.anchorMin = Vector2.zero; htRt.anchorMax = Vector2.one; htRt.offsetMin = Vector2.zero; htRt.offsetMax = Vector2.zero;
            Text hTxt = hTxtGo.AddComponent<Text>();
            hTxt.font = f; hTxt.fontSize = 44; hTxt.fontStyle = FontStyle.Bold;
            hTxt.color = Color.white; hTxt.alignment = TextAnchor.MiddleCenter; hTxt.raycastTarget = false;
            hTxt.text = "MAIL DELIVERY LOG";

            // Matching crisp archive index body text
            GameObject txtGo = new GameObject("LogText3D");
            txtGo.transform.SetParent(canvasGo.transform, false);
            RectTransform trt = txtGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(36f, 30f); trt.offsetMax = new Vector2(-36f, -180f);

            Text txt = txtGo.AddComponent<Text>();
            txt.font = f;
            txt.fontSize = 42;
            txt.lineSpacing = 1.22f;
            txt.color = new Color(0.10f, 0.11f, 0.14f, 1f);
            txt.alignment = TextAnchor.UpperLeft;
            txt.supportRichText = true;
            txt.raycastTarget = false;
            txt.text =
                "<b>FILING CABINET INDEX:</b>\n" +
                "──────────────────────\n\n" +
                "<b>• B-01 (Top)</b>\n" +
                "   Old 2023 Receipts\n\n" +
                "<color=#9E1A1A><b>• B-02 (Middle) ★</b></color>\n" +
                "   <b>A. MORETTI Envelope</b>\n" +
                "   <i>(VitaPulse File)</i>\n\n" +
                "<b>• B-03 (Bottom)</b>\n" +
                "   Blank Forms";

            root.AddComponent<ClipboardPickupAnimator>();
            return root;
        }

        private static void BuildSlidingArchiveCabinet(Transform parent, Vector3 pos)
        {
            GameObject cab = new GameObject("ArchiveFilingCabinet");
            cab.transform.SetParent(parent, false);
            cab.transform.position = pos;

            Color steelDark = new Color(0.24f, 0.28f, 0.33f);
            Color steelDrawer = new Color(0.34f, 0.39f, 0.45f);
            Color steelInner = new Color(0.29f, 0.33f, 0.38f);
            Color railSilver = new Color(0.74f, 0.77f, 0.81f);
            Color brass = new Color(0.84f, 0.68f, 0.28f);

            // Solid enclosed steel outer cabinet shell + horizontal crossbars between drawers (zero hollow gaps!)
            CreateBox(cab.transform, "CabBack",    new Vector3(-0.34f, 0.68f, 0f),     new Vector3(0.06f, 1.36f, 0.66f), steelDark, local: true);
            CreateBox(cab.transform, "CabTop",     new Vector3(0f,     1.33f, 0f),     new Vector3(0.72f, 0.06f, 0.66f), steelDark, local: true);
            CreateBox(cab.transform, "CabBottom",  new Vector3(0f,     0.03f, 0f),     new Vector3(0.72f, 0.06f, 0.66f), steelDark, local: true);
            CreateBox(cab.transform, "CabSideL",   new Vector3(0f,     0.68f, -0.305f),new Vector3(0.72f, 1.30f, 0.05f), steelDark, local: true);
            CreateBox(cab.transform, "CabSideR",   new Vector3(0f,     0.68f,  0.305f),new Vector3(0.72f, 1.30f, 0.05f), steelDark, local: true);
            CreateBox(cab.transform, "CabBarMid1", new Vector3(0.325f, 0.89f, 0f),     new Vector3(0.03f, 0.018f, 0.58f), steelDark, local: true, removeCollider: true);
            CreateBox(cab.transform, "CabBarMid2", new Vector3(0.325f, 0.48f, 0f),     new Vector3(0.03f, 0.018f, 0.58f), steelDark, local: true, removeCollider: true);

            string[] ids = { "cabinet_drawer_1", "cabinet_drawer_2", "cabinet_drawer_3" };
            string[] codes = { "B-01", "B-02", "B-03" };
            string[] titles = { "Drawer B-01 (Top)", "Drawer B-02 (Middle — A. Moretti)", "Drawer B-03 (Bottom)" };
            string[] emptyMsgs =
            {
                "Old 2023 office receipts. Nothing from Alex here.",
                "",
                "Blank intake forms and stationery. Check the Mail Delivery Log on your desk!"
            };
            float[] heights = { 1.095f, 0.685f, 0.275f };

            for (int i = 0; i < 3; i++)
            {
                GameObject drawerObj = new GameObject($"CabinetDrawer_{i + 1}");
                drawerObj.transform.SetParent(cab.transform, false);
                drawerObj.transform.localPosition = new Vector3(0.02f, heights[i], 0f);

                // Snug flush front panel (zero big gaps!) + full 3D drawer box (bottom, left/right side walls, back wall) + silver metal slide rails!
                CreateBox(drawerObj.transform, $"DrawerFront_{i + 1}",  new Vector3(0.315f, 0f,     0f),     new Vector3(0.04f, 0.394f, 0.562f), steelDrawer, local: true, removeCollider: true);
                CreateBox(drawerObj.transform, $"DrawerTray_{i + 1}",   new Vector3(0.01f, -0.155f, 0f),     new Vector3(0.58f, 0.026f, 0.52f),  steelInner,  local: true, removeCollider: true);
                CreateBox(drawerObj.transform, $"DrawerSideL_{i + 1}",  new Vector3(0.01f, -0.02f, -0.252f), new Vector3(0.58f, 0.28f,  0.022f), steelInner,  local: true, removeCollider: true);
                CreateBox(drawerObj.transform, $"DrawerSideR_{i + 1}",  new Vector3(0.01f, -0.02f,  0.252f), new Vector3(0.58f, 0.28f,  0.022f), steelInner,  local: true, removeCollider: true);
                CreateBox(drawerObj.transform, $"DrawerBack_{i + 1}",   new Vector3(-0.27f,-0.02f,  0f),     new Vector3(0.024f,0.28f,  0.52f),  steelInner,  local: true, removeCollider: true);
                // Metallic ball-bearing slide rails on both outer sides of the drawer
                CreateBox(drawerObj.transform, $"DrawerRailL_{i + 1}",  new Vector3(0.01f, -0.05f, -0.268f), new Vector3(0.56f, 0.028f, 0.014f), railSilver,  local: true, removeCollider: true);
                CreateBox(drawerObj.transform, $"DrawerRailR_{i + 1}",  new Vector3(0.01f, -0.05f,  0.268f), new Vector3(0.56f, 0.028f, 0.014f), railSilver,  local: true, removeCollider: true);

                // Brass handle + white index card holder + depth-occluded WorldSpace UI label (NEVER bleeds through open drawers!)
                CreateBox(drawerObj.transform, $"DrawerHandle_{i + 1}", new Vector3(0.352f, 0.01f,  0f),     new Vector3(0.03f, 0.036f, 0.16f),  brass,       local: true, removeCollider: true);
                CreateBox(drawerObj.transform, $"DrawerLabel_{i + 1}",  new Vector3(0.340f, 0.095f, 0f),     new Vector3(0.01f, 0.068f, 0.135f), new Color(0.95f, 0.94f, 0.88f), local: true, removeCollider: true);

                GameObject lblCanvasGo = new GameObject($"DrawerCodeCanvas_{i + 1}");
                lblCanvasGo.transform.SetParent(drawerObj.transform, false);
                lblCanvasGo.transform.localPosition = new Vector3(0.347f, 0.095f, 0f);
                lblCanvasGo.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                Canvas lblCanvas = lblCanvasGo.AddComponent<Canvas>();
                lblCanvas.renderMode = RenderMode.WorldSpace;
                RectTransform lblRt = lblCanvasGo.GetComponent<RectTransform>();
                lblRt.sizeDelta = new Vector2(180f, 90f);
                lblRt.localScale = new Vector3(0.13f / 180f, 0.065f / 90f, 1f);

                GameObject lblTxtGo = new GameObject("CodeTxt");
                lblTxtGo.transform.SetParent(lblCanvasGo.transform, false);
                RectTransform ltRt = lblTxtGo.AddComponent<RectTransform>();
                ltRt.anchorMin = Vector2.zero; ltRt.anchorMax = Vector2.one; ltRt.offsetMin = Vector2.zero; ltRt.offsetMax = Vector2.zero;
                Text dtm = lblTxtGo.AddComponent<Text>();
                dtm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                dtm.text = codes[i];
                dtm.fontSize = 46;
                dtm.fontStyle = FontStyle.Bold;
                dtm.alignment = TextAnchor.MiddleCenter;
                dtm.color = new Color(0.12f, 0.14f, 0.18f, 1f);
                dtm.raycastTarget = false;

                SlidingDrawer sd = drawerObj.AddComponent<SlidingDrawer>();
                sd.drawerTitle = titles[i];
                sd.openLocalOffset = new Vector3(0.46f, 0f, 0f);
                sd.containsAlexLetter = (i == 1);
                sd.emptyMessage = emptyMsgs[i];

                AddCleanColliderAndInteractable(drawerObj, ids[i], $"Slide Open {titles[i]}", "Cabinet", new Vector3(0.09f, 0.39f, 0.56f), new Vector3(0.32f, 0f, 0f));

                if (i == 1)
                {
                    GameObject env = new GameObject("AlexHiddenEnvelope");
                    env.transform.SetParent(drawerObj.transform, false);
                    env.transform.localPosition = new Vector3(0.04f, -0.115f, 0f);
                    env.transform.localRotation = Quaternion.Euler(0f, 8f, 0f);

                    CreateBox(env.transform, "EnvelopeBody", Vector3.zero, new Vector3(0.28f, 0.028f, 0.36f), new Color(0.96f, 0.86f, 0.45f), local: true, removeCollider: true);
                    CreateBox(env.transform, "RedWaxSeal", new Vector3(0f, 0.018f, 0f), new Vector3(0.06f, 0.01f, 0.06f), new Color(0.82f, 0.14f, 0.14f), local: true, removeCollider: true);

                    AddCleanColliderAndInteractable(env, "cabinet_alex_letter", "Alex Moretti's Yellow Envelope (Pick Up!)", "Cabinet", new Vector3(0.32f, 0.12f, 0.40f), new Vector3(0f, 0.02f, 0f));
                }
            }
        }

        private static GameObject CreateGlassPane(Transform parent, string name, Vector3 pos, Vector3 scale, Color tint)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.position = pos;
            obj.transform.localScale = scale;
            Collider c = obj.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);

            Renderer rend = obj.GetComponent<Renderer>();
            string matPath = $"{MaterialFolder}/Mat_{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_Smoothness", 0.92f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)RenderQueue.Transparent;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
            else mat.color = tint;

            EditorUtility.SetDirty(mat);
            rend.sharedMaterial = mat;
            rend.shadowCastingMode = ShadowCastingMode.Off;
            return obj;
        }

        private static Bounds GetWorldBounds(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(go.transform.position, Vector3.zero);
            }

            Bounds combined = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                combined.Encapsulate(renderers[i].bounds);
            }
            return combined;
        }

        /// <summary>
        /// Computes an exact rotation-aware local-space BoxCollider wrapping EVERY Renderer child
        /// of target (with generous padding and optional minimum clickable size).
        /// </summary>
        private static void AddAutoFitColliderAndInteractable(
            GameObject target,
            string id,
            string label,
            string focusViewpoint,
            float padding = 0.03f,
            Vector3? minLocalSize = null)
        {
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
            bool hasBounds = false;
            Bounds localBounds = new Bounds();

            foreach (Renderer r in renderers)
            {
                if (r is MeshRenderer || r is SkinnedMeshRenderer)
                {
                    Bounds wb = r.bounds;
                    Vector3 ext = wb.extents;
                    Vector3 ctr = wb.center;
                    for (int sx = -1; sx <= 1; sx += 2)
                    for (int sy = -1; sy <= 1; sy += 2)
                    for (int sz = -1; sz <= 1; sz += 2)
                    {
                        Vector3 worldCorner = ctr + new Vector3(ext.x * sx, ext.y * sy, ext.z * sz);
                        Vector3 localCorner = target.transform.InverseTransformPoint(worldCorner);
                        if (!hasBounds)
                        {
                            localBounds = new Bounds(localCorner, Vector3.zero);
                            hasBounds = true;
                        }
                        else
                        {
                            localBounds.Encapsulate(localCorner);
                        }
                    }
                }
            }

            Vector3 finalSize = hasBounds ? localBounds.size + Vector3.one * (padding * 2f) : new Vector3(0.25f, 0.25f, 0.25f);
            Vector3 finalCenter = hasBounds ? localBounds.center : Vector3.zero;

            if (minLocalSize.HasValue)
            {
                Vector3 m = minLocalSize.Value;
                finalSize = new Vector3(Mathf.Max(finalSize.x, m.x), Mathf.Max(finalSize.y, m.y), Mathf.Max(finalSize.z, m.z));
            }

            AddCleanColliderAndInteractable(target, id, label, focusViewpoint, finalSize, finalCenter);
        }

        private static void AddCleanColliderAndInteractable(GameObject target, string id, string label, string focusViewpoint, Vector3 colliderSize, Vector3 colliderCenter)
        {
            BoxCollider box = target.GetComponent<BoxCollider>();
            if (box == null) box = target.AddComponent<BoxCollider>();
            box.size = colliderSize;
            box.center = colliderCenter;

            InteractableObject inter = target.GetComponent<InteractableObject>();
            if (inter == null) inter = target.AddComponent<InteractableObject>();
            inter.interactableId = id;
            inter.displayLabel = label;
            inter.focusViewpointId = focusViewpoint;
        }

        private static GameObject CreateBox(Transform parent, string name, Vector3 pos, Vector3 scale, Color color, bool local = false, bool removeCollider = false)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            if (local) obj.transform.localPosition = pos;
            else obj.transform.position = pos;
            obj.transform.localScale = scale;

            if (removeCollider)
            {
                Collider c = obj.GetComponent<Collider>();
                if (c != null) Object.DestroyImmediate(c);
            }

            Renderer rend = obj.GetComponent<Renderer>();
            string matPath = $"{MaterialFolder}/Mat_{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            else mat.color = color;
            EditorUtility.SetDirty(mat);
            rend.sharedMaterial = mat;
            return obj;
        }

        private static void CreateDirectionalLight(Transform parent, string name, Vector3 euler, Color color, float intensity)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.rotation = Quaternion.Euler(euler);
            Light l = go.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = color;
            l.intensity = intensity;
            l.shadows = LightShadows.Soft;
        }

        private static void CreatePointLight(Transform parent, string name, Vector3 pos, Color color, float intensity, float range)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            Light l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.intensity = intensity;
            l.range = range;
            l.shadows = LightShadows.Soft;
        }

        private static CameraViewpoint CreateViewpoint(Transform parent, string id, string label, Vector3 pos, Vector3 lookTarget)
        {
            GameObject vpObj = new GameObject($"VP_{id}");
            vpObj.transform.SetParent(parent, false);
            vpObj.transform.position = pos;
            vpObj.transform.LookAt(lookTarget);

            CameraViewpoint vp = vpObj.AddComponent<CameraViewpoint>();
            vp.viewpointId = id;
            vp.displayName = label;
            return vp;
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Game"))
                AssetDatabase.CreateFolder("Assets", "_Game");
            if (!AssetDatabase.IsValidFolder(SceneFolder))
                AssetDatabase.CreateFolder("Assets/_Game", "Scenes");
            if (!AssetDatabase.IsValidFolder(MaterialFolder))
                AssetDatabase.CreateFolder("Assets/_Game", "Materials");
        }
    }
}
#endif
