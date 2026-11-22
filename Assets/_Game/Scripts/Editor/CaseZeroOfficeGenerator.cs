#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace HealthFiles.EditorTools
{
    /// <summary>
    /// 1-Click Starter Scene Generator for WP2 & WP3!
    /// Open Unity top menu -> HealthFiles -> 1. Build Starter Office & Alex Prototype
    /// Generates Assets/_Game/Scenes/OfficeScene.unity with a working 3D greybox office,
    /// 6 camera viewpoints, all frozen interactable IDs, and WP3 core systems wired up.
    /// </summary>
    public static class CaseZeroOfficeGenerator
    {
        private const string SceneFolder = "Assets/_Game/Scenes";
        private const string MaterialFolder = "Assets/_Game/Materials";
        private const string ScenePath = SceneFolder + "/OfficeScene.unity";

        [MenuItem("HealthFiles/1. Build Starter Office & Alex Prototype")]
        public static void BuildStarterOfficeScene()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Game"))
                AssetDatabase.CreateFolder("Assets", "_Game");
            if (!AssetDatabase.IsValidFolder(SceneFolder))
                AssetDatabase.CreateFolder("Assets/_Game", "Scenes");
            if (!AssetDatabase.IsValidFolder(MaterialFolder))
                AssetDatabase.CreateFolder("Assets/_Game", "Materials");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Lighting
            GameObject dirLightObj = new GameObject("Directional Light");
            Light dirLight = dirLightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.intensity = 1.1f;
            dirLight.color = new Color(1f, 0.96f, 0.88f);
            dirLightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject roomLightObj = new GameObject("Office Ceiling Light");
            Light roomLight = roomLightObj.AddComponent<Light>();
            roomLight.type = LightType.Point;
            roomLight.range = 14f;
            roomLight.intensity = 18f;
            roomLight.color = new Color(1f, 0.95f, 0.85f);
            roomLightObj.transform.position = new Vector3(0f, 3.2f, 0f);

            // 2. Office Architecture (Greybox — WP2 can replace with free 3D assets anytime!)
            GameObject roomRoot = new GameObject("--- OFFICE 3D (WP2) ---");
            CreateBox(roomRoot.transform, "Floor", new Vector3(0f, 0f, 0f), new Vector3(7f, 0.1f, 7f), new Color(0.32f, 0.27f, 0.22f));
            CreateBox(roomRoot.transform, "BackWall", new Vector3(0f, 1.8f, 3.5f), new Vector3(7f, 3.6f, 0.15f), new Color(0.76f, 0.78f, 0.80f));
            CreateBox(roomRoot.transform, "LeftWall", new Vector3(-3.5f, 1.8f, 0f), new Vector3(0.15f, 3.6f, 7f), new Color(0.72f, 0.75f, 0.78f));
            CreateBox(roomRoot.transform, "RightWall", new Vector3(3.5f, 1.8f, 0f), new Vector3(0.15f, 3.6f, 7f), new Color(0.72f, 0.75f, 0.78f));

            // Desk
            CreateBox(roomRoot.transform, "DeskTop", new Vector3(0f, 0.78f, 1.2f), new Vector3(2.2f, 0.08f, 1.0f), new Color(0.45f, 0.30f, 0.18f));
            CreateBox(roomRoot.transform, "DeskLegL", new Vector3(-0.98f, 0.39f, 1.2f), new Vector3(0.1f, 0.78f, 0.9f), new Color(0.35f, 0.22f, 0.13f));
            CreateBox(roomRoot.transform, "DeskLegR", new Vector3(0.98f, 0.39f, 1.2f), new Vector3(0.1f, 0.78f, 0.9f), new Color(0.35f, 0.22f, 0.13f));

            // Interactable 1: Office Computer (center of desk)
            GameObject computer = CreateBox(roomRoot.transform, "OfficeComputer", new Vector3(0.05f, 1.15f, 1.42f), new Vector3(0.85f, 0.52f, 0.08f), new Color(0.12f, 0.16f, 0.22f));
            CreateBox(computer.transform, "MonitorScreen", new Vector3(0f, 0f, -0.045f), new Vector3(0.78f, 0.44f, 0.02f), new Color(0.18f, 0.56f, 0.82f), local: true);
            AttachInteractable(computer, "desk_computer", "Office Computer (Inspect Blog & Policy)", "Computer");

            // Interactable 2: Desk Telephone (left of monitor)
            GameObject phone = CreateBox(roomRoot.transform, "DeskTelephone", new Vector3(-0.75f, 0.90f, 1.15f), new Vector3(0.28f, 0.16f, 0.26f), new Color(0.78f, 0.18f, 0.16f));
            AttachInteractable(phone, "desk_phone", "Telephone (Answer / Call Personas)", "Desk");

            // Interactable 3: Filing Cabinet + Alex's Hidden Letter (left wall)
            GameObject cabinet = CreateBox(roomRoot.transform, "FilingCabinet", new Vector3(-2.95f, 0.85f, 1.4f), new Vector3(0.75f, 1.7f, 0.85f), new Color(0.36f, 0.42f, 0.48f));
            CreateBox(cabinet.transform, "AlexEnvelopeHint", new Vector3(0.39f, 0.22f, 0f), new Vector3(0.04f, 0.20f, 0.32f), new Color(0.96f, 0.90f, 0.65f), local: true);
            AttachInteractable(cabinet, "cabinet_alex_letter", "Archive Filing Cabinet (Search for Alex's Letter)", "Cabinet");

            // Interactable 4: Bookshelf + Investigation Manual (right wall)
            GameObject bookshelf = CreateBox(roomRoot.transform, "Bookshelf", new Vector3(2.95f, 1.05f, 1.3f), new Vector3(0.55f, 2.1f, 1.3f), new Color(0.42f, 0.28f, 0.16f));
            CreateBox(bookshelf.transform, "InvestigationManual", new Vector3(-0.26f, 0.15f, -0.1f), new Vector3(0.18f, 0.26f, 0.20f), new Color(0.92f, 0.72f, 0.18f), local: true);
            AttachInteractable(bookshelf, "bookshelf_manual", "Bookshelf (Find Investigation Manual)", "Bookshelf");

            // Interactable 5: Office Printer (left-back area, Case 03 Daniel)
            GameObject printer = CreateBox(roomRoot.transform, "OfficePrinter", new Vector3(-2.65f, 0.55f, -0.8f), new Vector3(0.8f, 1.1f, 0.75f), new Color(0.84f, 0.86f, 0.88f));
            CreateBox(printer.transform, "OutputTrayPaper", new Vector3(0.42f, 0.30f, 0f), new Vector3(0.18f, 0.03f, 0.26f), Color.white, local: true);
            AttachInteractable(printer, "printer_daniel", "Office Printer (Check Output Tray - Case 03)", "Printer");

            // Interactable 6: Evidence Board (back wall behind desk)
            GameObject board = CreateBox(roomRoot.transform, "EvidenceBoard", new Vector3(0.2f, 1.95f, 3.35f), new Vector3(2.4f, 1.35f, 0.08f), new Color(0.68f, 0.52f, 0.34f));
            CreateBox(board.transform, "PinnedNote1", new Vector3(-0.55f, 0.18f, -0.05f), new Vector3(0.32f, 0.24f, 0.02f), new Color(1f, 0.96f, 0.72f), local: true);
            CreateBox(board.transform, "PinnedNote2", new Vector3(0.42f, -0.10f, -0.05f), new Vector3(0.32f, 0.24f, 0.02f), new Color(0.85f, 0.94f, 1f), local: true);
            AttachInteractable(board, "evidence_board", "Evidence Board (Pin & Connect Clues)", "EvidenceBoard");

            // 3. Camera Viewpoints (WP2)
            GameObject vpRoot = new GameObject("--- VIEWPOINTS (WP2) ---");
            List<CameraViewpoint> viewpoints = new List<CameraViewpoint>
            {
                CreateViewpoint(vpRoot.transform, "Desk", "Desk", new Vector3(0f, 1.48f, -0.45f), new Vector3(0f, 1.05f, 1.35f)),
                CreateViewpoint(vpRoot.transform, "Computer", "Computer", new Vector3(0.05f, 1.24f, 0.45f), new Vector3(0.05f, 1.15f, 1.42f)),
                CreateViewpoint(vpRoot.transform, "Cabinet", "Cabinet", new Vector3(-1.2f, 1.35f, 1.15f), new Vector3(-2.95f, 1.05f, 1.4f)),
                CreateViewpoint(vpRoot.transform, "Bookshelf", "Bookshelf", new Vector3(1.1f, 1.35f, 1.05f), new Vector3(2.95f, 1.2f, 1.3f)),
                CreateViewpoint(vpRoot.transform, "Printer", "Printer", new Vector3(-1.05f, 1.25f, -0.75f), new Vector3(-2.65f, 0.75f, -0.8f)),
                CreateViewpoint(vpRoot.transform, "EvidenceBoard", "Evidence Board", new Vector3(0.2f, 1.88f, 1.25f), new Vector3(0.2f, 1.95f, 3.35f))
            };

            // 4. Main Camera + OfficeRaycaster (WP2)
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.fieldOfView = 60f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f);
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<OfficeRaycaster>();
            camObj.transform.position = viewpoints[0].transform.position;
            camObj.transform.rotation = viewpoints[0].transform.rotation;

            // 5. EventSystem (Unity 6 Input System UI support!)
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            esObj.AddComponent<InputSystemUIInputModule>();
#else
            esObj.AddComponent<StandaloneInputModule>();
#endif

            // 6. Core Systems Object (WP2 + WP3)
            GameObject sysObj = new GameObject("--- SYSTEMS (WP2 + WP3) ---");
            CameraViewpointController vpCtrl = sysObj.AddComponent<CameraViewpointController>();
            vpCtrl.SetViewpoints(viewpoints, camObj.transform);
            sysObj.AddComponent<EvidenceManager>();
            sysObj.AddComponent<DialogueManager>();
            sysObj.AddComponent<PersonalizationManager>();
            sysObj.AddComponent<MissionManager>();
            sysObj.AddComponent<CaseZeroHUD>();

            // Save Scene & Register in Build Settings
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            AssetDatabase.Refresh();
            Debug.Log("[HealthFiles] Starter Office & Alex Prototype created at " + ScenePath + "! Press PLAY to test!");
            EditorUtility.DisplayDialog(
                "Health Files: Case Zero",
                "Starter 3D Office & Alex Vertical Slice generated!\n\nPress the PLAY button at the top of Unity to play Prologue + Case 01 (Alex).",
                "Awesome!"
            );
        }

        private static GameObject CreateBox(Transform parent, string name, Vector3 pos, Vector3 scale, Color color, bool local = false)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            if (local) obj.transform.localPosition = pos;
            else obj.transform.position = pos;
            obj.transform.localScale = scale;

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

        private static void AttachInteractable(GameObject obj, string id, string label, string focusViewpoint)
        {
            InteractableObject inter = obj.AddComponent<InteractableObject>();
            inter.interactableId = id;
            inter.displayLabel = label;
            inter.focusViewpointId = focusViewpoint;
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
    }
}
#endif
