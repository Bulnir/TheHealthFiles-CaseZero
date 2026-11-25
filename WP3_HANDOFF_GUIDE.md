# WP3 Handoff Guide — Shared C# Systems, UI Polish & Case 01 (Alex)

**Project:** The Health Files: Case Zero (PoliMi eHealth Methods & Applications, Group 12)  
**Unity Version (EXACT — MUST MATCH):** `6000.3.12f1` (Unity 6.3 LTS)  
**GitHub Repo:** `https://github.com/Bulnir/TheHealthFiles-CaseZero`  
**Deadlines:**
- **Interim Progress Demo:** Friday, 27 November 2026
- **Final Submission:** Tuesday, 15 December 2026 (`Group_12_eHealth2627.zip` on WeBeep)

---

## 1. Git Branch & Zero-Conflict Ownership Rules (CRITICAL!)

To prevent Unity scene merge conflicts between **WP2 (3D Office)** and **WP3 (Systems + UI + Alex)**:

1. **Always work on your own Git branch:** `wp3-systems-alex`
   - Pull latest `main` first, then create/switch to `wp3-systems-alex`.
2. **WP3 OWNS these folders (edit freely!):**
   - `Assets/_Game/Scripts/Core/` (`EvidenceModels.cs`, `EvidenceManager.cs`, `DialogueManager.cs`, `MissionManager.cs`, `PersonalizationManager.cs`, `SoundManager.cs`, `JsonArrayHelper.cs`)
   - `Assets/_Game/Scripts/UI/` (`CaseZeroHUD.cs` and any new modular UI scripts/prefabs)
   - `Assets/_Game/Resources/Data/` (`evidence_register.json`, `alex_lines.json`, `sofia_lines.json`, `daniel_lines.json`)
   - `Assets/_Game/Resources/Portraits/` (`alex.png`, `sofia.png`, `daniel.png`)
   - `Assets/_Game/Resources/Audio/`
   - `Assets/_Game/Prefabs/` (create reusable UI prefabs for WP4 & WP5 here!)
3. **DO NOT edit these (WP2 owns them):**
   - `Assets/_Game/Scenes/OfficeScene.unity`
   - `Assets/_Game/Scripts/Office/`
   - `Assets/_Game/Scripts/Editor/CaseZeroOfficeGenerator.cs`

---

## 2. What Is Already Built & Working

- **WP1 Integration (Praveen):** `DialogueManager.cs` reads Praveen's raw Hugging Face JSON arrays (`alex_lines.json`, `sofia_lines.json`, `daniel_lines.json`) via `JsonArrayHelper.cs`. All 8 official Evidence IDs from `Evidence Register.xlsx` are loaded from `evidence_register.json`.
- **Playable Case 01 Vertical Slice:** Prologue (Supervisor call + Bookshelf Investigation Manual) -> Alex first call (with portrait & typewriter text) -> Desk Mail Delivery Log -> Cabinet Drawer B-02 (Alex's Yellow Envelope) -> Computer Desktop (Viral News + 6-passage Blog vs Privacy Policy comparison mini-game) -> OBJECTION! call (animated splash + sound + wrong-answer adaptive hints) -> Cork Board pin (`policy_42`) -> Sofia hook (`Sofia_Ready`).
- **Personalization System (Required in Final Report Section II):** `PersonalizationManager.cs` tracks live DHLI scores (`Reliability`, `Privacy`, `Navigation`), mistake counts, adaptive scaffolding hints, and generates a personalized investigator summary.

---

## 3. Recommended WP3 Next Tasks (Before Friday Demo & Week 3)

1. **UI/UX Visual Polish (`CaseZeroHUD.cs` or modular Unity Canvas Prefabs):**
   - Upgrade styling (cleaner typography, subtle panel borders, progress badges, color-coded DHLI skill bars in the Profile drawer).
   - Add a **3-question optional "Quick Investigator Onboarding" (self-check)** at start (e.g., confidence in source reliability / privacy / digital portals) that seeds initial DHLI personalization hints — huge bonus for Section II of the final report!
2. **Case 01 (Alex) Polish:**
   - Add a quick "Skip Typewriter" click-to-complete on dialogue text so players don't have to wait.
   - Add a clean "Case 01 Complete" celebration modal showing Alex's closing message + live DHLI breakdown before Sofia calls.
   - Add subtle ambient office background audio (optional toggle/mute button in top corner).
3. **WP4 (Sofia) & WP5 (Daniel) Plug-in Readiness:**
   - Keep `MissionManager.cs`, `EvidenceManager.cs`, `DialogueManager.cs`, and `PersonalizationManager.cs` public APIs backwards-compatible with `DATA_CONTRACT.md`.
   - Document any new hooks in `DATA_CONTRACT.md` so WP4 & WP5 can plug straight in next week.
