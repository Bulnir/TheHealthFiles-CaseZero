using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HealthFiles
{
    /// <summary>
    /// WP2: Detective Cork Evidence Board with:
    /// - Central 'VITAPULSE — CASE ZERO' investigation hub pinned from Day 1
    /// - Crisp WorldSpace readable 3D Evidence Cards with red pushpins as clues are collected
    /// - Taut 3D Red Detective Yarn Strings connecting every collected clue's pin to the central hub!
    /// </summary>
    public class EvidenceBoardVisual : MonoBehaviour
    {
        // Board native front is -Z (rotated -90 around Y on left wall so -Z faces +X into the room)
        private static readonly Vector3 HubPinLocalPos = new Vector3(0f, 0.56f, -0.068f);

        [SerializeField] private Vector3[] pinSlots =
        {
            new Vector3(-0.52f, 0.86f, -0.055f),
            new Vector3( 0.52f, 0.86f, -0.055f),
            new Vector3(-0.56f, 0.54f, -0.055f),
            new Vector3( 0.56f, 0.54f, -0.055f),
            new Vector3(-0.46f, 0.24f, -0.055f),
            new Vector3( 0.00f, 0.22f, -0.055f),
            new Vector3( 0.46f, 0.24f, -0.055f),
            new Vector3( 0.00f, 0.88f, -0.055f)
        };

        private readonly HashSet<string> pinnedIds = new HashSet<string>();
        private int nextSlot;
        private Font uiFont;
        private Shader litShader;
        private bool photocopyPinned;

        private void Awake()
        {
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        }

        private void Start()
        {
            SpawnCentralHubCard();

            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.OnEvidenceCollected += HandleEvidenceCollected;
                foreach (EvidenceItem existing in EvidenceManager.Instance.CollectedEvidence)
                {
                    HandleEvidenceCollected(existing);
                }
            }

            if (PrinterJamEffect.Instance != null)
            {
                PrinterJamEffect.Instance.OnJamCleared += HandlePrinterJamCleared;
            }
        }

        private void OnDestroy()
        {
            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.OnEvidenceCollected -= HandleEvidenceCollected;
            }
            if (PrinterJamEffect.Instance != null)
            {
                PrinterJamEffect.Instance.OnJamCleared -= HandlePrinterJamCleared;
            }
        }

        private void HandlePrinterJamCleared()
        {
            if (photocopyPinned) return;
            photocopyPinned = true;

            Texture2D tex = Resources.Load<Texture2D>("Textures/printer_photocopy_jam");
            if (tex == null) return;

            // Pin the rescued crumpled A4 photocopy casually tucked into the very bottom-right corner (barely takes space!)
            GameObject frame = new GameObject("Pinned_PhotocopySouvenir");
            frame.transform.SetParent(transform, false);
            frame.transform.localPosition = new Vector3(0.78f, 0.14f, -0.056f);
            frame.transform.localRotation = Quaternion.Euler(0f, 0f, -8.5f);

            GameObject paper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            paper.name = "A4PaperBacking";
            paper.transform.SetParent(frame.transform, false);
            paper.transform.localScale = new Vector3(0.17f, 0.23f, 0.007f);
            Destroy(paper.GetComponent<Collider>());
            Material pMat = new Material(litShader);
            if (pMat.HasProperty("_BaseColor")) pMat.SetColor("_BaseColor", new Color(0.95f, 0.94f, 0.90f));
            else pMat.color = new Color(0.95f, 0.94f, 0.90f);
            paper.GetComponent<Renderer>().material = pMat;

            GameObject canvasGo = new GameObject("PhotocopyCanvas");
            canvasGo.transform.SetParent(frame.transform, false);
            canvasGo.transform.localPosition = new Vector3(0f, 0f, -0.005f);
            Canvas c = canvasGo.AddComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            RectTransform crt = canvasGo.GetComponent<RectTransform>();
            crt.sizeDelta = new Vector2(170f, 230f);
            crt.localScale = new Vector3(0.162f / 170f, 0.222f / 230f, 1f);

            GameObject imgGo = new GameObject("RawPhoto");
            imgGo.transform.SetParent(canvasGo.transform, false);
            RectTransform irt = imgGo.AddComponent<RectTransform>();
            irt.anchorMin = Vector2.zero; irt.anchorMax = Vector2.one;
            irt.offsetMin = Vector2.zero; irt.offsetMax = Vector2.zero;
            RawImage ri = imgGo.AddComponent<RawImage>();
            ri.texture = tex;
            ri.raycastTarget = false;

            // Red pushpin holding the photocopy souvenir!
            GameObject pin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pin.name = "SouvenirPushPin";
            pin.transform.SetParent(frame.transform, false);
            pin.transform.localPosition = new Vector3(0f, 0.102f, -0.011f);
            pin.transform.localScale = new Vector3(0.020f, 0.020f, 0.015f);
            Destroy(pin.GetComponent<Collider>());
            Material pinMat = new Material(litShader);
            if (pinMat.HasProperty("_BaseColor")) pinMat.SetColor("_BaseColor", new Color(0.86f, 0.10f, 0.10f));
            else pinMat.color = new Color(0.86f, 0.10f, 0.10f);
            pin.GetComponent<Renderer>().material = pinMat;
        }

        private void SpawnCentralHubCard()
        {
            // Top title strip on the corkboard (sized cleanly so the full title fits on one line!)
            CreateReadableCard(
                "BoardHeaderBanner",
                new Vector3(0f, 1.02f, -0.052f),
                new Vector2(0.86f, 0.11f),
                new Color(0.14f, 0.22f, 0.34f),
                Color.white,
                "<b>VITAPULSE — CASE ZERO</b>",
                22,
                0f,
                addPin: false
            );

            // Central VitaPulse target card with bright red pushpin
            CreateReadableCard(
                "CentralHub_VitaPulse",
                new Vector3(0f, 0.56f, -0.055f),
                new Vector2(0.32f, 0.22f),
                new Color(0.96f, 0.94f, 0.88f),
                new Color(0.12f, 0.12f, 0.15f),
                "<color=#A81818><b>TARGET APP</b></color>\n<b>VITAPULSE</b>\n<i>Claims vs. Facts</i>",
                26,
                -1.5f,
                addPin: true
            );
        }

        private void HandleEvidenceCollected(EvidenceItem item)
        {
            if (item == null || item.id == "manual_guide" || pinnedIds.Contains(item.id))
                return;

            pinnedIds.Add(item.id);
            SpawnPinnedNote(item);
        }

        private void SpawnPinnedNote(EvidenceItem item)
        {
            if (pinSlots == null || pinSlots.Length == 0) return;
            Vector3 localPos = pinSlots[nextSlot % pinSlots.Length];
            nextSlot++;

            Color noteColor = item.caseOwner switch
            {
                "alex" => new Color(1.00f, 0.93f, 0.62f),
                "sofia" => new Color(0.80f, 0.92f, 1.00f),
                "daniel" => new Color(0.80f, 0.96f, 0.84f),
                _ => new Color(0.96f, 0.95f, 0.90f)
            };

            string ownerTag = string.IsNullOrEmpty(item.caseOwner) ? "CLUE" : item.caseOwner.ToUpperInvariant();
            string label = $"<color=#8C1A1A><b>[{ownerTag}]</b></color>\n<b>{item.title}</b>";

            Vector3 cardPinPos = CreateReadableCard(
                $"PinnedCard_{item.id}",
                localPos,
                new Vector2(0.34f, 0.21f),
                noteColor,
                new Color(0.12f, 0.13f, 0.16f),
                label,
                22,
                Random.Range(-4.5f, 4.5f),
                addPin: true
            );

            // Connect this clue's red pushpin to the central VitaPulse hub with taut 3D red detective yarn!
            CreateRedString(HubPinLocalPos, cardPinPos, $"String_{item.id}");
        }

        private Vector3 CreateReadableCard(
            string cardName,
            Vector3 localPos,
            Vector2 sizeMeters,
            Color bgColor,
            Color textColor,
            string textContent,
            int fontSize,
            float tiltDeg,
            bool addPin)
        {
            GameObject card = new GameObject(cardName);
            card.transform.SetParent(transform, false);
            card.transform.localPosition = localPos;
            card.transform.localRotation = Quaternion.Euler(0f, 0f, tiltDeg);

            // Thin 3D cardstock backing
            GameObject mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mesh.name = "PaperMesh";
            mesh.transform.SetParent(card.transform, false);
            mesh.transform.localScale = new Vector3(sizeMeters.x, sizeMeters.y, 0.008f);
            Destroy(mesh.GetComponent<Collider>());
            Material mat = new Material(litShader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", bgColor);
            else mat.color = bgColor;
            mesh.GetComponent<Renderer>().material = mat;

            // Crisp WorldSpace Canvas text on the -Z face (facing out into the office!)
            GameObject canvasGo = new GameObject("CardTextCanvas");
            canvasGo.transform.SetParent(card.transform, false);
            canvasGo.transform.localPosition = new Vector3(0f, 0f, -0.006f);
            canvasGo.transform.localRotation = Quaternion.identity;

            Canvas c = canvasGo.AddComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            RectTransform crt = canvasGo.GetComponent<RectTransform>();
            float resW = 340f;
            float resH = Mathf.Max(80f, resW * (sizeMeters.y / sizeMeters.x));
            crt.sizeDelta = new Vector2(resW, resH);
            crt.localScale = new Vector3(sizeMeters.x / resW, sizeMeters.y / resH, 1f);

            GameObject txtGo = new GameObject("Label");
            txtGo.transform.SetParent(canvasGo.transform, false);
            RectTransform trt = txtGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(16f, 10f); trt.offsetMax = new Vector2(-16f, -14f);

            Text txt = txtGo.AddComponent<Text>();
            txt.font = uiFont;
            txt.text = textContent;
            txt.fontSize = fontSize;
            txt.color = textColor;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.supportRichText = true;
            txt.raycastTarget = false;

            Vector3 pinLocalOnBoard = localPos + new Vector3(0f, sizeMeters.y * 0.38f, -0.014f);
            if (addPin)
            {
                GameObject pin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pin.name = "RedPushPin";
                pin.transform.SetParent(card.transform, false);
                pin.transform.localPosition = new Vector3(0f, sizeMeters.y * 0.40f, -0.012f);
                pin.transform.localScale = new Vector3(0.024f, 0.024f, 0.018f);
                Destroy(pin.GetComponent<Collider>());

                Material pinMat = new Material(litShader);
                if (pinMat.HasProperty("_BaseColor")) pinMat.SetColor("_BaseColor", new Color(0.86f, 0.10f, 0.10f));
                else pinMat.color = new Color(0.86f, 0.10f, 0.10f);
                pin.GetComponent<Renderer>().material = pinMat;

                pinLocalOnBoard = transform.InverseTransformPoint(pin.transform.position);
            }

            return pinLocalOnBoard;
        }

        private void CreateRedString(Vector3 startLocal, Vector3 endLocal, string stringName)
        {
            Vector3 diff = endLocal - startLocal;
            float dist = diff.magnitude;
            if (dist < 0.01f) return;

            GameObject yarn = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            yarn.name = stringName;
            yarn.transform.SetParent(transform, false);
            yarn.transform.localPosition = (startLocal + endLocal) * 0.5f + new Vector3(0f, 0f, -0.004f);
            yarn.transform.localRotation = Quaternion.FromToRotation(Vector3.up, diff.normalized);
            yarn.transform.localScale = new Vector3(0.0055f, dist * 0.5f, 0.0055f);
            Destroy(yarn.GetComponent<Collider>());

            Material yarnMat = new Material(litShader);
            if (yarnMat.HasProperty("_BaseColor")) yarnMat.SetColor("_BaseColor", new Color(0.82f, 0.11f, 0.11f));
            else yarnMat.color = new Color(0.82f, 0.11f, 0.11f);
            yarn.GetComponent<Renderer>().material = yarnMat;
        }
    }
}
