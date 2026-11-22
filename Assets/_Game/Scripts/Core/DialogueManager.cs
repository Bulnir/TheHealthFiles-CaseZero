using System.Collections.Generic;
using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// WP3: Loads pre-generated Hugging Face LLM persona dialogues produced by WP1
    /// from Resources/Data/{npc}_lines.json (alex_lines.json, sofia_lines.json, daniel_lines.json).
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        private readonly Dictionary<string, List<NpcDialogueLine>> linesByNpc =
            new Dictionary<string, List<NpcDialogueLine>>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            LoadNpcFile("alex", "Data/alex_lines");
            LoadNpcFile("sofia", "Data/sofia_lines");
            LoadNpcFile("daniel", "Data/daniel_lines");
        }

        private void LoadNpcFile(string npcKey, string resourcePath)
        {
            TextAsset asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null)
            {
                Debug.LogWarning($"[WP3] Dialogue JSON not found at Resources/{resourcePath}.json");
                return;
            }

            NpcDialogueLine[] parsed = JsonArrayHelper.FromJson<NpcDialogueLine>(asset.text);
            linesByNpc[npcKey.ToLowerInvariant()] = new List<NpcDialogueLine>(parsed);
            Debug.Log($"[WP3] Loaded {parsed.Length} LLM persona lines for '{npcKey}'.");
        }

        /// <summary>
        /// Returns a curated LLM line matching the NPC name and line_type
        /// ("intro", "hint", "wrong_answer", "correct_answer", "clue").
        /// </summary>
        public string GetLine(string npc, string lineType, bool randomize = false)
        {
            string key = npc.ToLowerInvariant();
            if (!linesByNpc.TryGetValue(key, out List<NpcDialogueLine> npcLines))
            {
                return $"[Missing dialogue file for {npc}]";
            }

            List<NpcDialogueLine> matches = npcLines.FindAll(l => l.line_type == lineType);
            if (matches.Count == 0)
            {
                return $"[Missing '{lineType}' line for {npc}]";
            }

            int idx = randomize ? Random.Range(0, matches.Count) : 0;
            return matches[idx].text;
        }
    }
}
