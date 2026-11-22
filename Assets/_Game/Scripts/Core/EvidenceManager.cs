using System;
using System.Collections.Generic;
using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// WP3: Single shared Evidence Inventory used by ALL cases (Alex, Sofia, Daniel, Final Board).
    /// Loads all frozen evidence definitions from Resources/Data/evidence_register.json.
    /// </summary>
    public class EvidenceManager : MonoBehaviour
    {
        public static EvidenceManager Instance { get; private set; }

        private readonly Dictionary<string, EvidenceItem> registry = new Dictionary<string, EvidenceItem>();
        private readonly List<EvidenceItem> collected = new List<EvidenceItem>();

        public event Action<EvidenceItem> OnEvidenceCollected;
        public event Action OnInventoryChanged;

        public IReadOnlyList<EvidenceItem> CollectedEvidence => collected;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            LoadRegistry();
        }

        private void LoadRegistry()
        {
            registry.Clear();
            TextAsset asset = Resources.Load<TextAsset>("Data/evidence_register");
            if (asset == null)
            {
                Debug.LogError("[WP3] Missing Resources/Data/evidence_register.json!");
                return;
            }

            EvidenceItem[] items = JsonArrayHelper.FromJson<EvidenceItem>(asset.text);
            foreach (EvidenceItem item in items)
            {
                if (!string.IsNullOrEmpty(item.id))
                {
                    registry[item.id] = item;
                }
            }

            Debug.Log($"[WP3] Loaded {registry.Count} official evidence definitions from evidence_register.json.");
        }

        public EvidenceItem GetDefinition(string evidenceId)
        {
            registry.TryGetValue(evidenceId, out EvidenceItem item);
            return item;
        }

        public bool HasEvidence(string evidenceId)
        {
            return collected.Exists(e => e.id == evidenceId);
        }

        public bool AddEvidence(string evidenceId)
        {
            if (HasEvidence(evidenceId))
                return false;

            if (!registry.TryGetValue(evidenceId, out EvidenceItem item))
            {
                Debug.LogWarning($"[WP3] Unknown evidence ID requested: {evidenceId}");
                return false;
            }

            collected.Add(item);
            Debug.Log($"[WP3] Evidence collected: {item.id} ({item.title})");
            OnEvidenceCollected?.Invoke(item);
            OnInventoryChanged?.Invoke();
            return true;
        }
    }
}
