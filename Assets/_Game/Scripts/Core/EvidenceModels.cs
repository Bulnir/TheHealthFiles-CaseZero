using System;

namespace HealthFiles
{
    /// <summary>
    /// Shared data contracts aligned with WP1's Evidence Register and LLM JSON outputs.
    /// Do NOT rename JSON field names without updating WP1's Python pipeline.
    /// </summary>
    [Serializable]
    public class EvidenceItem
    {
        public string id;
        public string title;
        public string caseOwner;
        public string claim;
        public string source;
        public string summary;
        public string dhliDomain; // "Reliability", "Privacy", or "Navigation"
        public bool usedOnFinalBoard;
    }

    /// <summary>
    /// Exact JSON schema produced by WP1's Hugging Face pipeline (alex_lines.json, sofia_lines.json, daniel_lines.json).
    /// </summary>
    [Serializable]
    public class NpcDialogueLine
    {
        public string npc;          // "alex", "sofia", "daniel"
        public string line_type;    // "intro", "hint", "wrong_answer", "correct_answer", "clue"
        public string text;
        public string evidence_id;  // e.g. "blog_unverified", "policy_42", or ""
    }

    public enum DhliDomain
    {
        Reliability,
        Privacy,
        Navigation
    }

    /// <summary>
    /// Frozen mission progression states shared across WP2, WP3 (Alex), WP4 (Sofia), and WP5 (Daniel + Finale).
    /// </summary>
    public enum MissionState
    {
        Prologue_SupervisorCall,
        Prologue_FindManual,
        Alex_AnswerFirstCall,
        Alex_FindHiddenLetter,
        Alex_InspectBlogAndPolicy,
        Alex_ObjectionCall,
        Alex_PinPolicyClause,
        Sofia_Ready,       // WP4 starts here
        Daniel_Ready,      // WP5 starts here
        FinalBoard_Ready,  // WP5 finale
        CaseClosed
    }
}
