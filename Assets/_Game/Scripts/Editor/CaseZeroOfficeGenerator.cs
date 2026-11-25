#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
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
            CreateDirectionalLight(lightRoot.transform, "SunLight", new Vector3(38f, -125f, 0f), new Color(1f, 0.96f, 0.89f), 1.15f);
            CreatePointLight(lightRoot.transform, "CeilingGlow", new Vector3(0f, 2.98f, 0.6f), new Color(1f, 0.97f, 0.91f), 12f, 11f);
            CreatePointLight(lightRoot.transform, "DeskLampGlow", new Vector3(0.68f, 1.18f, 1.48f), new Color(1f, 0.84f, 0.54f), 5.0f, 3.8f);
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

            // 4. Interactable: Office Computer (exact real-world proportions: 42cm monitor, 44cm keyboard, 11.5cm mouse + glowing live screen!)
            GameObject computerStation = PlacePrefabClean(roomRoot.transform, "OfficeComputer", P_PC, new Vector3(-0.04f, deskTopY + 0.006f, 1.39f), 0.42f, 90f);
            FixKeyboardAndMouseScale(computerStation, deskTopY + 0.006f);
            AddLiveMonitorScreenOverlay(computerStation);
            Bounds pcBounds = GetWorldBounds(computerStation);
            AddCleanColliderAndInteractable(
                computerStation,
                "desk_computer",
                "Office Computer (Click to Open Workstation)",
                "Computer",
                new Vector3(0.60f, 0.50f, 0.72f),
                computerStation.transform.InverseTransformPoint(pcBounds.center)
            );

            // PC Tower standing cleanly ON THE RIGHT CORNER OF THE DESK (visible while seated, with glowing green LED & click interaction!)
            GameObject pcTower = PlacePrefabClean(roomRoot.transform, "PCTower", P_PCCase, new Vector3(0.84f, deskTopY, 1.50f), 0.40f, 102f);
            CreateBox(pcTower.transform, "PC_PowerLED", new Vector3(0.105f, 0.34f, -0.04f), new Vector3(0.012f, 0.012f, 0.024f), new Color(0.22f, 0.96f, 0.42f), local: true, removeCollider: true);
            AddCleanColliderAndInteractable(pcTower, "desk_pc_tower", "Workstation PC Tower (Check Security Status)", "Desk", new Vector3(0.24f, 0.42f, 0.42f), new Vector3(0f, 0.20f, 0f));

            // 5. Interactable: Sleek Modern Office IP Telephone (angled toward player, real 21cm size!)
            GameObject phone = BuildModernOfficeIPPhone(roomRoot.transform, new Vector3(-0.64f, deskTopY, 1.24f));
            phone.AddComponent<RingingPhoneEffect>();
            AddCleanColliderAndInteractable(phone, "desk_phone", "Office IP Telephone (Answer / Call Personas)", "Desk", new Vector3(0.28f, 0.18f, 0.26f), new Vector3(0f, 0.08f, 0f));

            // 6. Interactable: Mail Delivery Log Clipboard (A4 proportions, right of Desk Mat)
            GameObject mailLog = BuildMailLogClipboard(roomRoot.transform, new Vector3(0.54f, deskTopY, 1.12f));
            AddCleanColliderAndInteractable(mailLog, "desk_mail_log", "Mail Delivery Log (Check Archive Index)", "Desk", new Vector3(0.26f, 0.08f, 0.34f), new Vector3(0f, 0.02f, 0f));

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
            AddCleanColliderAndInteractable(
                board,
                "evidence_board",
                "Evidence Cork Board (Pin & Connect Clues)",
                "EvidenceBoard",
                new Vector3(boardWidth + 0.08f, 1.18f, 0.24f),
                new Vector3(0f, 0.56f, 0f)
            );

            // Wall Clock mounted cleanly above the Archive Cabinet on the Left Wall (rotY = 0 -> faces +X!)
            PlacePrefabClean(roomRoot.transform, "WallClock", P_Clock, new Vector3(-3.26f, 2.48f, 2.20f), 0.36f, 0f);

            // 9. LEFT WALL STATION 3: Office Printer (z = -1.15, faces +X cleanly)
            GameObject printer = PlacePrefabClean(roomRoot.transform, "OfficePrinter", P_Printer, new Vector3(-2.74f, 0f, -1.15f), 0.96f, 0f);
            Bounds printerBounds = GetWorldBounds(printer);
            CreateBox(printer.transform, "DanielOutputSheet", new Vector3(0.28f, printerBounds.max.y - 0.12f, 0f), new Vector3(0.22f, 0.014f, 0.28f), new Color(0.98f, 0.98f, 0.94f), local: true, removeCollider: true);
            AddCleanColliderAndInteractable(
                printer,
                "printer_daniel",
                "Office Printer (Output Tray — Case 03 Daniel)",
                "Printer",
                new Vector3(Mathf.Max(0.85f, printerBounds.size.x), Mathf.Max(0.98f, printerBounds.size.y), Mathf.Max(0.85f, printerBounds.size.z)),
                printer.transform.InverseTransformPoint(printerBounds.center)
            );

            // Water Dispenser neatly in Front-Left Corner
            PlacePrefabClean(roomRoot.transform, "WaterDispenser", P_WaterDispenser, new Vector3(-2.78f, 0f, -2.35f), 1.18f, 0f);

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

            // 12. Camera Viewpoints (EvidenceBoard now cleanly frames the Left Wall Cork Board!)
            GameObject vpRoot = new GameObject("--- VIEWPOINTS (WP2) ---");
            List<CameraViewpoint> viewpoints = new List<CameraViewpoint>
            {
                CreateViewpoint(vpRoot.transform, "Desk", "Desk", new Vector3(0f, 1.44f, 0.36f), new Vector3(0f, 0.98f, 1.65f)),
                CreateViewpoint(vpRoot.transform, "Computer", "Computer", new Vector3(0f, 1.12f, 0.66f), new Vector3(0f, 0.98f, 1.38f)),
                CreateViewpoint(vpRoot.transform, "Cabinet", "Cabinet", new Vector3(-1.35f, 1.14f, 2.20f), new Vector3(-2.85f, 0.78f, 2.20f)),
                CreateViewpoint(vpRoot.transform, "Bookshelf", "Bookshelf", new Vector3(1.25f, 1.28f, 1.80f), new Vector3(2.96f, 1.12f, 1.80f)),
                CreateViewpoint(vpRoot.transform, "Printer", "Printer", new Vector3(-1.15f, 1.22f, -1.15f), new Vector3(-2.74f, 0.68f, -1.15f)),
                CreateViewpoint(vpRoot.transform, "EvidenceBoard", "Evidence Board", new Vector3(-1.35f, 1.78f, 0.55f), new Vector3(-3.24f, 1.78f, 0.55f))
            };

            // 13. Main Camera + OfficeRaycaster
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.fieldOfView = 58f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.76f, 0.95f);
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

        private static void AddLiveMonitorScreenOverlay(GameObject pcRoot)
        {
            Transform monitor = FindChildRecursive(pcRoot.transform, "PC_monitor");
            if (monitor == null) return;
            Bounds mb = GetWorldBounds(monitor.gameObject);

            // Glowing live OS wallpaper + subtle taskbar & notification card right on the 3D monitor face (-Z side)!
            GameObject screen = new GameObject("LiveMonitorDisplay");
            screen.transform.SetParent(pcRoot.transform, false);
            screen.transform.position = new Vector3(mb.center.x, mb.center.y + mb.size.y * 0.06f, mb.min.z - 0.004f);
            screen.transform.rotation = Quaternion.identity;

            float sw = mb.size.x * 0.90f;
            float sh = mb.size.y * 0.74f;
            CreateBox(screen.transform, "OS_Wallpaper", Vector3.zero, new Vector3(sw, sh, 0.004f), new Color(0.11f, 0.24f, 0.40f), local: true, removeCollider: true);
            CreateBox(screen.transform, "OS_Taskbar", new Vector3(0f, -sh * 0.44f, -0.003f), new Vector3(sw, sh * 0.11f, 0.003f), new Color(0.06f, 0.09f, 0.14f), local: true, removeCollider: true);
            CreateBox(screen.transform, "OS_AlertCard1", new Vector3(-sw * 0.18f, sh * 0.14f, -0.003f), new Vector3(sw * 0.50f, sh * 0.22f, 0.003f), new Color(0.88f, 0.94f, 0.99f), local: true, removeCollider: true);
            CreateBox(screen.transform, "OS_AlertHeader", new Vector3(-sw * 0.18f, sh * 0.22f, -0.005f), new Vector3(sw * 0.50f, sh * 0.06f, 0.002f), new Color(0.84f, 0.22f, 0.20f), local: true, removeCollider: true);
            CreateBox(screen.transform, "OS_AlertCard2", new Vector3(sw * 0.24f, -sh * 0.08f, -0.003f), new Vector3(sw * 0.34f, sh * 0.30f, 0.003f), new Color(0.18f, 0.44f, 0.66f), local: true, removeCollider: true);
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
            // Long vertical brushed-steel office push/pull bar handle (both sides)
            CreateBox(parent, "DoorHandle_BarIn", new Vector3(-1.32f, 1.08f, 3.45f), new Vector3(0.032f, 0.52f, 0.032f), silverMetal);
            CreateBox(parent, "DoorHandle_MountTop", new Vector3(-1.32f, 1.28f, 3.50f), new Vector3(0.03f, 0.03f, 0.10f), silverMetal);
            CreateBox(parent, "DoorHandle_MountBot", new Vector3(-1.32f, 0.88f, 3.50f), new Vector3(0.03f, 0.03f, 0.10f), silverMetal);
            // Overhead hydraulic door closer + 3 steel hinges + room number plaque
            CreateBox(parent, "DoorCloserBox", new Vector3(-1.98f, 2.18f, 3.49f), new Vector3(0.26f, 0.06f, 0.06f), silverMetal);
            CreateBox(parent, "DoorHinge1", new Vector3(-2.33f, 1.95f, 3.50f), new Vector3(0.03f, 0.10f, 0.04f), silverMetal);
            CreateBox(parent, "DoorHinge2", new Vector3(-2.33f, 1.15f, 3.50f), new Vector3(0.03f, 0.10f, 0.04f), silverMetal);
            CreateBox(parent, "DoorHinge3", new Vector3(-2.33f, 0.38f, 3.50f), new Vector3(0.03f, 0.10f, 0.04f), silverMetal);
            CreateBox(parent, "DoorPlaque", new Vector3(-1.75f, 1.62f, 3.52f), new Vector3(0.46f, 0.11f, 0.015f), new Color(0.14f, 0.24f, 0.38f));

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

            // Glass Front Wall of the Opposite Conference Room (at z = 6.48)
            CreateBox(ext.transform, "Across_GlassHeader", new Vector3(0.5f, 3.04f, 6.48f), new Vector3(5.7f, 0.52f, 0.14f), corrWall);
            CreateBox(ext.transform, "Across_TopBar", new Vector3(0.5f, 2.75f, 6.48f), new Vector3(5.6f, 0.08f, 0.16f), steelFrame);
            CreateBox(ext.transform, "Across_BotBar", new Vector3(0.5f, 0.08f, 6.48f), new Vector3(5.6f, 0.16f, 0.16f), steelFrame);
            foreach (float mx in new[] { -2.25f, -0.45f, 1.35f, 3.25f })
            {
                CreateBox(ext.transform, $"Across_Mullion_{mx:F1}", new Vector3(mx, 1.40f, 6.48f), new Vector3(0.07f, 2.70f, 0.14f), steelFrame);
            }
            CreateGlassPane(ext.transform, "Across_GlassPane", new Vector3(0.5f, 1.42f, 6.48f), new Vector3(5.45f, 2.60f, 0.03f), new Color(0.74f, 0.88f, 0.96f, 0.22f));

            // Inside the Opposite Conference Room: Conference Table, 4 Chairs, Wall Presentation Screen, Plant!
            PlacePrefabClean(ext.transform, "Across_ConfTable", P_ConferenceTable, new Vector3(0.55f, 0f, 8.25f), 0.76f, 90f);
            PlacePrefabClean(ext.transform, "Across_Chair1", P_Chair, new Vector3(-0.15f, 0f, 7.55f), 0.95f, 270f);
            PlacePrefabClean(ext.transform, "Across_Chair2", P_Chair, new Vector3(1.25f, 0f, 7.55f), 0.95f, 270f);
            PlacePrefabClean(ext.transform, "Across_Chair3", P_Chair, new Vector3(-0.15f, 0f, 8.95f), 0.95f, 90f);
            PlacePrefabClean(ext.transform, "Across_Chair4", P_Chair, new Vector3(1.25f, 0f, 8.95f), 0.95f, 90f);
            PlacePrefabClean(ext.transform, "Across_Plant", P_Plant1, new Vector3(2.65f, 0f, 9.45f), 1.35f, 90f);
            // Wall presentation TV / Whiteboard on back wall of Conference Room
            CreateBox(ext.transform, "Across_ScreenFrame", new Vector3(0.55f, 1.75f, 10.02f), new Vector3(2.40f, 1.25f, 0.06f), steelFrame, removeCollider: true);
            CreateBox(ext.transform, "Across_ScreenFace", new Vector3(0.55f, 1.75f, 9.98f), new Vector3(2.26f, 1.12f, 0.02f), new Color(0.92f, 0.96f, 0.99f), removeCollider: true);
            CreateBox(ext.transform, "Across_ScreenAccent", new Vector3(0.55f, 2.12f, 9.96f), new Vector3(2.00f, 0.14f, 0.01f), navyAccent, removeCollider: true);

            // Detailed modern city skyline with glowing glass office windows & distant trees outside the right window!
            BuildCitySkyscraper(ext.transform, "City_Bldg1", new Vector3(13.8f, 0f, -4.4f), new Vector3(3.0f, 11.5f, 3.2f), new Color(0.76f, 0.74f, 0.70f), 6, 4);
            BuildCitySkyscraper(ext.transform, "City_Bldg2", new Vector3(15.6f, 0f, -0.7f), new Vector3(3.4f, 14.5f, 3.8f), new Color(0.66f, 0.72f, 0.78f), 8, 5);
            BuildCitySkyscraper(ext.transform, "City_Bldg3", new Vector3(13.0f, 0f, 2.9f), new Vector3(2.8f, 9.8f, 3.0f), new Color(0.80f, 0.75f, 0.68f), 5, 4);
            BuildCitySkyscraper(ext.transform, "City_Bldg4", new Vector3(18.5f, 0f, 1.3f), new Vector3(3.8f, 17.5f, 4.2f), new Color(0.58f, 0.65f, 0.74f), 9, 5);
            // Warm street-level greenery & horizon ground visible below the buildings
            CreateBox(ext.transform, "City_StreetGround", new Vector3(13f, -1.6f, 0f), new Vector3(18f, 0.2f, 24f), new Color(0.52f, 0.54f, 0.52f), removeCollider: true);
            for (int t = 0; t < 5; t++)
            {
                float tz = -4.2f + t * 2.1f;
                CreateBox(ext.transform, $"City_TreeCanopy_{t}", new Vector3(9.4f, 0.9f, tz), new Vector3(1.1f, 1.2f, 1.1f), new Color(0.28f, 0.48f, 0.30f), removeCollider: true);
                CreateBox(ext.transform, $"City_TreeTrunk_{t}", new Vector3(9.4f, -0.4f, tz), new Vector3(0.22f, 1.5f, 0.22f), new Color(0.36f, 0.26f, 0.18f), removeCollider: true);
            }
        }

        private static void BuildCitySkyscraper(Transform parent, string name, Vector3 basePos, Vector3 size, Color facadeColor, int floors, int cols)
        {
            GameObject bldg = new GameObject(name);
            bldg.transform.SetParent(parent, false);
            bldg.transform.position = basePos;

            Color trimColor = facadeColor * 0.78f;
            Color glassBlue = new Color(0.38f, 0.58f, 0.76f);
            Color warmLit = new Color(0.96f, 0.88f, 0.62f);

            // Main building tower + architectural roof parapet + rooftop AC box
            CreateBox(bldg.transform, $"{name}_Body", new Vector3(0f, size.y * 0.5f - 1.5f, 0f), size, facadeColor, local: true, removeCollider: true);
            CreateBox(bldg.transform, $"{name}_RoofCap", new Vector3(0f, size.y - 1.40f, 0f), new Vector3(size.x + 0.18f, 0.24f, size.z + 0.18f), trimColor, local: true, removeCollider: true);
            CreateBox(bldg.transform, $"{name}_RoofUnit", new Vector3(0.2f, size.y - 1.05f, 0.2f), new Vector3(size.x * 0.35f, 0.45f, size.z * 0.35f), trimColor, local: true, removeCollider: true);

            // Window grid on the -X face (facing straight toward our office panoramic window!)
            float faceX = -size.x * 0.5f - 0.03f;
            float usableH = size.y - 2.6f;
            float usableZ = size.z - 0.7f;
            float winH = (usableH / floors) * 0.56f;
            float winW = (usableZ / cols) * 0.64f;

            for (int f = 0; f < floors; f++)
            {
                float wy = 0.2f + (f + 0.5f) * (usableH / floors);
                // Horizontal architectural floor ledge
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

            Color[] bookPalette =
            {
                new Color(0.18f, 0.28f, 0.44f),
                new Color(0.56f, 0.18f, 0.18f),
                new Color(0.18f, 0.40f, 0.28f),
                new Color(0.42f, 0.34f, 0.24f),
                new Color(0.24f, 0.24f, 0.30f),
                new Color(0.62f, 0.44f, 0.18f)
            };

            for (int i = 0; i < 6; i++)
            {
                float zOff = -0.48f + i * 0.075f;
                CreateBox(shelf.transform, $"BinderA_{i}", new Vector3(0.02f, 0.84f, zOff), new Vector3(0.26f, 0.30f, 0.055f), bookPalette[i % bookPalette.Length], local: true, removeCollider: true);
            }
            for (int i = 0; i < 5; i++)
            {
                float zOff = 0.20f + i * 0.07f;
                CreateBox(shelf.transform, $"BinderB_{i}", new Vector3(0.02f, 0.82f, zOff), new Vector3(0.24f, 0.26f, 0.05f), bookPalette[(i + 2) % bookPalette.Length], local: true, removeCollider: true);
            }

            for (int i = 0; i < 5; i++)
            {
                float zOff = -0.50f + i * 0.062f;
                float h = 0.22f + (i % 3) * 0.03f;
                CreateBox(shelf.transform, $"BookMidL_{i}", new Vector3(0.03f, 1.14f + h * 0.5f, zOff), new Vector3(0.22f, h, 0.045f), bookPalette[(i + 1) % bookPalette.Length], local: true, removeCollider: true);
            }
            for (int i = 0; i < 5; i++)
            {
                float zOff = 0.24f + i * 0.062f;
                float h = 0.23f + (i % 2) * 0.04f;
                CreateBox(shelf.transform, $"BookMidR_{i}", new Vector3(0.03f, 1.14f + h * 0.5f, zOff), new Vector3(0.22f, h, 0.045f), bookPalette[(i + 3) % bookPalette.Length], local: true, removeCollider: true);
            }

            GameObject manual = new GameObject("InvestigationManual");
            manual.transform.SetParent(shelf.transform, false);
            manual.transform.localPosition = new Vector3(-0.04f, 1.29f, 0.0f);
            manual.transform.localRotation = Quaternion.Euler(0f, 18f, -12f);
            CreateBox(manual.transform, "ManualCover", Vector3.zero, new Vector3(0.045f, 0.28f, 0.21f), new Color(0.92f, 0.72f, 0.16f), local: true, removeCollider: true);
            CreateBox(manual.transform, "ManualBadge", new Vector3(-0.025f, 0.04f, 0f), new Vector3(0.008f, 0.08f, 0.14f), new Color(0.14f, 0.26f, 0.44f), local: true, removeCollider: true);
            AddCleanColliderAndInteractable(manual, "bookshelf_manual", "Investigation Manual (Source - Evidence - Context)", "Bookshelf", new Vector3(0.22f, 0.34f, 0.28f), Vector3.zero);

            for (int i = 0; i < 8; i++)
            {
                float zOff = -0.46f + i * 0.07f;
                float h = 0.24f + (i % 3) * 0.025f;
                CreateBox(shelf.transform, $"BookTop_{i}", new Vector3(0.03f, 1.60f + h * 0.5f, zOff), new Vector3(0.22f, h, 0.052f), bookPalette[i % bookPalette.Length], local: true, removeCollider: true);
            }

            AddCleanColliderAndInteractable(shelf, "bookshelf_manual", "Office Bookcase — Pick Up Investigation Manual", "Bookshelf", new Vector3(0.48f, 2.04f, 1.36f), new Vector3(0f, 1.02f, 0f));
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

            CreateBox(root.transform, "ClipboardWood", new Vector3(0f, 0.007f, 0f), new Vector3(0.22f, 0.014f, 0.30f), new Color(0.48f, 0.31f, 0.18f), local: true, removeCollider: true);
            CreateBox(root.transform, "LogPaper", new Vector3(0f, 0.016f, -0.01f), new Vector3(0.19f, 0.005f, 0.25f), new Color(0.96f, 0.95f, 0.90f), local: true, removeCollider: true);
            CreateBox(root.transform, "LogClip", new Vector3(0f, 0.022f, 0.12f), new Vector3(0.08f, 0.012f, 0.032f), new Color(0.72f, 0.74f, 0.78f), local: true, removeCollider: true);
            CreateBox(root.transform, "LogHeaderStripe", new Vector3(0f, 0.0195f, 0.075f), new Vector3(0.15f, 0.003f, 0.022f), new Color(0.16f, 0.38f, 0.65f), local: true, removeCollider: true);

            return root;
        }

        private static void BuildSlidingArchiveCabinet(Transform parent, Vector3 pos)
        {
            GameObject cab = new GameObject("ArchiveFilingCabinet");
            cab.transform.SetParent(parent, false);
            cab.transform.position = pos;

            Color steelDark = new Color(0.26f, 0.30f, 0.35f);
            Color steelDrawer = new Color(0.34f, 0.39f, 0.45f);
            Color brass = new Color(0.84f, 0.68f, 0.28f);

            CreateBox(cab.transform, "CabBack", new Vector3(-0.34f, 0.68f, 0f), new Vector3(0.06f, 1.36f, 0.66f), steelDark, local: true);
            CreateBox(cab.transform, "CabTop", new Vector3(0f, 1.34f, 0f), new Vector3(0.72f, 0.06f, 0.66f), steelDark, local: true);
            CreateBox(cab.transform, "CabBottom", new Vector3(0f, 0.03f, 0f), new Vector3(0.72f, 0.06f, 0.66f), steelDark, local: true);
            CreateBox(cab.transform, "CabSideL", new Vector3(0f, 0.68f, -0.31f), new Vector3(0.72f, 1.32f, 0.05f), steelDark, local: true);
            CreateBox(cab.transform, "CabSideR", new Vector3(0f, 0.68f, 0.31f), new Vector3(0.72f, 1.32f, 0.05f), steelDark, local: true);

            string[] ids = { "cabinet_drawer_1", "cabinet_drawer_2", "cabinet_drawer_3" };
            string[] titles = { "Drawer B-01 (Top)", "Drawer B-02 (Middle — A. Moretti)", "Drawer B-03 (Bottom)" };
            string[] emptyMsgs =
            {
                "Old 2023 office receipts. Nothing from Alex here.",
                "",
                "Blank intake forms and stationery. Check the Mail Delivery Log on your desk!"
            };
            float[] heights = { 1.08f, 0.68f, 0.28f };

            for (int i = 0; i < 3; i++)
            {
                GameObject drawerObj = new GameObject($"CabinetDrawer_{i + 1}");
                drawerObj.transform.SetParent(cab.transform, false);
                drawerObj.transform.localPosition = new Vector3(0.02f, heights[i], 0f);

                CreateBox(drawerObj.transform, $"DrawerFront_{i + 1}", new Vector3(0.32f, 0f, 0f), new Vector3(0.04f, 0.35f, 0.56f), steelDrawer, local: true, removeCollider: true);
                CreateBox(drawerObj.transform, $"DrawerTray_{i + 1}", new Vector3(0f, -0.13f, 0f), new Vector3(0.60f, 0.04f, 0.52f), steelDark, local: true, removeCollider: true);
                CreateBox(drawerObj.transform, $"DrawerHandle_{i + 1}", new Vector3(0.36f, 0.02f, 0f), new Vector3(0.03f, 0.04f, 0.16f), brass, local: true, removeCollider: true);
                CreateBox(drawerObj.transform, $"DrawerLabel_{i + 1}", new Vector3(0.345f, 0.09f, 0f), new Vector3(0.01f, 0.06f, 0.12f), new Color(0.95f, 0.94f, 0.88f), local: true, removeCollider: true);

                SlidingDrawer sd = drawerObj.AddComponent<SlidingDrawer>();
                sd.drawerTitle = titles[i];
                sd.openLocalOffset = new Vector3(0.46f, 0f, 0f);
                sd.containsAlexLetter = (i == 1);
                sd.emptyMessage = emptyMsgs[i];

                AddCleanColliderAndInteractable(drawerObj, ids[i], $"Slide Open {titles[i]}", "Cabinet", new Vector3(0.08f, 0.36f, 0.58f), new Vector3(0.33f, 0f, 0f));

                if (i == 1)
                {
                    GameObject env = new GameObject("AlexHiddenEnvelope");
                    env.transform.SetParent(drawerObj.transform, false);
                    env.transform.localPosition = new Vector3(0.05f, -0.07f, 0f);
                    env.transform.localRotation = Quaternion.Euler(0f, 8f, 0f);

                    CreateBox(env.transform, "EnvelopeBody", Vector3.zero, new Vector3(0.28f, 0.03f, 0.36f), new Color(0.96f, 0.86f, 0.45f), local: true, removeCollider: true);
                    CreateBox(env.transform, "RedWaxSeal", new Vector3(0f, 0.02f, 0f), new Vector3(0.06f, 0.01f, 0.06f), new Color(0.82f, 0.14f, 0.14f), local: true, removeCollider: true);

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
