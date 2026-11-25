using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// WP2: Visual Paper-Jam indicator on the 3D Office Printer.
    /// Blinks an orange/red error LED and subtly shakes the jammed paper sticking out of the tray
    /// until the player clears the jam in the mini-game.
    /// </summary>
    public class PrinterJamEffect : MonoBehaviour
    {
        public static PrinterJamEffect Instance { get; private set; }

        [SerializeField] private Renderer ledRenderer;
        [SerializeField] private Transform jammedPaperTransform;

        private bool isJammed = true;
        private Vector3 paperBaseLocalPos;
        private Quaternion paperBaseLocalRot;

        public bool IsJammed => isJammed;
        public event System.Action OnJamCleared;

        public void Configure(Renderer led, Transform jammedPaper)
        {
            ledRenderer = led;
            jammedPaperTransform = jammedPaper;
        }

        private void Awake()
        {
            Instance = this;
            if (jammedPaperTransform != null)
            {
                paperBaseLocalPos = jammedPaperTransform.localPosition;
                paperBaseLocalRot = jammedPaperTransform.localRotation;
            }
        }

        private void Update()
        {
            if (ledRenderer != null)
            {
                Color c;
                if (isJammed)
                {
                    bool blink = Mathf.FloorToInt(Time.time * 4.5f) % 2 == 0;
                    c = blink ? new Color(1f, 0.28f, 0.08f) : new Color(0.28f, 0.08f, 0.04f);
                }
                else
                {
                    c = new Color(0.18f, 0.90f, 0.36f);
                }

                if (ledRenderer.material.HasProperty("_BaseColor"))
                    ledRenderer.material.SetColor("_BaseColor", c);
                else
                    ledRenderer.material.color = c;
            }

            if (jammedPaperTransform != null)
            {
                jammedPaperTransform.gameObject.SetActive(isJammed);
                if (isJammed)
                {
                    float jitter = Mathf.Sin(Time.time * 18f) * 0.002f;
                    jammedPaperTransform.localPosition = paperBaseLocalPos + new Vector3(jitter, 0f, 0f);
                    jammedPaperTransform.localRotation = paperBaseLocalRot * Quaternion.Euler(0f, Mathf.Sin(Time.time * 12f) * 1.4f, 0f);
                }
            }
        }

        public void ClearJam()
        {
            isJammed = false;
            if (jammedPaperTransform != null)
            {
                jammedPaperTransform.gameObject.SetActive(false);
            }
            OnJamCleared?.Invoke();
        }

        public void TriggerJamAgain()
        {
            isJammed = true;
            if (jammedPaperTransform != null)
            {
                jammedPaperTransform.gameObject.SetActive(true);
            }
        }
    }
}
