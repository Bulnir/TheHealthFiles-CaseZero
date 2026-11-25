using System.Collections.Generic;
using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// WP2: Dynamically pins physical sticky notes / evidence cards onto the 3D Cork Board
    /// as the player collects evidence across Alex, Sofia, and Daniel's cases.
    /// </summary>
    public class EvidenceBoardVisual : MonoBehaviour
    {
        [Tooltip("Local positions on the corkboard face (origin is bottom-center of board) where collected cards appear.")]
        [SerializeField] private Vector3[] pinSlots =
        {
            new Vector3(-0.42f, 0.82f, -0.06f),
            new Vector3( 0.00f, 0.82f, -0.06f),
            new Vector3( 0.42f, 0.82f, -0.06f),
            new Vector3(-0.42f, 0.54f, -0.06f),
            new Vector3( 0.00f, 0.54f, -0.06f),
            new Vector3( 0.42f, 0.54f, -0.06f),
            new Vector3(-0.21f, 0.28f, -0.06f),
            new Vector3( 0.21f, 0.28f, -0.06f)
        };

        private readonly HashSet<string> pinnedIds = new HashSet<string>();
        private int nextSlot;

        private void Start()
        {
            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.OnEvidenceCollected += HandleEvidenceCollected;
            }
        }

        private void OnDestroy()
        {
            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.OnEvidenceCollected -= HandleEvidenceCollected;
            }
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
                "alex" => new Color(1f, 0.92f, 0.55f),
                "sofia" => new Color(0.75f, 0.90f, 1f),
                "daniel" => new Color(0.76f, 0.96f, 0.80f),
                _ => Color.white
            };

            GameObject card = GameObject.CreatePrimitive(PrimitiveType.Cube);
            card.name = $"PinnedCard_{item.id}";
            card.transform.SetParent(transform, false);
            card.transform.localPosition = localPos;
            card.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-5f, 5f));
            card.transform.localScale = new Vector3(0.28f, 0.20f, 0.012f);
            Destroy(card.GetComponent<Collider>());

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", noteColor);
            else mat.color = noteColor;
            card.GetComponent<Renderer>().material = mat;

            // Tiny red pushpin at top center of the card
            GameObject pin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pin.name = "PushPin";
            pin.transform.SetParent(card.transform, false);
            pin.transform.localPosition = new Vector3(0f, 0.38f, -0.9f);
            pin.transform.localScale = new Vector3(0.11f, 0.15f, 2.2f);
            Destroy(pin.GetComponent<Collider>());

            Material pinMat = new Material(shader);
            if (pinMat.HasProperty("_BaseColor")) pinMat.SetColor("_BaseColor", new Color(0.85f, 0.12f, 0.12f));
            else pinMat.color = new Color(0.85f, 0.12f, 0.12f);
            pin.GetComponent<Renderer>().material = pinMat;
        }
    }
}
