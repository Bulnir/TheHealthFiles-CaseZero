# The Health Files: Case Zero — Team Data Contract & Unity Guide

**Unity Version (LOCKED for WP2, WP3, WP4, WP5):** `6000.3.12f1` (Unity 6.3 LTS)  
**GitHub Repo:** `https://github.com/Bulnir/TheHealthFiles-CaseZero`  
**Course Collaborators Required:** `@ETauro` and `@chiarapiazzy`

---

## 1. Frozen Evidence IDs (Aligned with WP1 Evidence Register)

All cases use the single shared `EvidenceManager.Instance` (no duplicate inventories).

| ID | Title | Case | Final Board? |
|---|---|---|---|
| `manual_guide` | Investigation Manual | Prologue | No (tutorial) |
| `letter_alex` | Alex's Letter | Alex (WP3) | No (context) |
| `blog_unverified` | Unverified Blog Claim | Alex (WP3) | No |
| `policy_42` | VitaPulse Privacy Policy - Section 4.2 | Alex (WP3) | Yes (Connection A) |
| `promo_agreement` | VitaPulse Promotional Agreement | Sofia (WP4) | Yes (Connection B) |
| `permissions_corrected` | Privacy Permissions Corrected | Sofia (WP4) | Yes (Connection A) |
| `hospital_claim` | Viral Video - Hospital Endorsement Claim | Sofia (WP4) | Yes (Connection B) |
| `clinical_guidance` | Official Clinical Evidence Guidance | Daniel (WP5) | Yes (Connection C) |
| `screenshot_altered` | Altered Approval Screenshot | Daniel (WP5) | Yes (Connection C) |

---

## 2. WP1 -> Unity JSON Format (Drop-in Compatible)

Place curated Hugging Face JSON files directly into:
`Assets/_Game/Resources/Data/`
- `alex_lines.json`
- `sofia_lines.json`
- `daniel_lines.json`

Schema (raw JSON array — read automatically via `JsonArrayHelper`):
```json
[
  {
    "npc": "alex",
    "line_type": "clue",
    "text": "The term \"third-party partners\" isn't clear to me either.",
    "evidence_id": "policy_42"
  }
]
```
Allowed `line_type`: `intro`, `hint`, `wrong_answer`, `correct_answer`, `clue`.

---

## 3. Frozen 3D Office Interactable IDs (WP2 -> WP3/WP4/WP5)

| Interactable ID | 3D Station | Viewpoint ID | Used By |
|---|---|---|---|
| `bookshelf_manual` | Bookshelf & Manual | `Bookshelf` | Prologue (WP3) |
| `desk_phone` | Desk Telephone (Ringing) | `Desk` | WP3 / WP4 / WP5 |
| `desk_mail_log` | Desk Mail Delivery Log | `Desk` | Case 01 Alex (WP3) |
| `cabinet_drawer_1` / `2` / `3` | 3D Sliding Archive Drawers | `Cabinet` | Case 01 Alex (WP2/WP3) |
| `cabinet_alex_letter` | Alex's Yellow Envelope (in Drawer B-02) | `Cabinet` | Case 01 Alex (WP3) |
| `desk_computer` | Office Computer | `Computer` | WP3 / WP4 / WP5 |
| `printer_daniel` | Office Printer Tray | `Printer` | Case 03 Daniel (WP5) |
| `evidence_board` | Wall Cork Evidence Board | `EvidenceBoard` | WP3 + Finale (WP5) |

---

## 4. Shared WP3 C# API for WP4 & WP5

```csharp
// Add or check evidence:
EvidenceManager.Instance.AddEvidence("promo_agreement");
bool hasPolicy = EvidenceManager.Instance.HasEvidence("policy_42");

// Read WP1 LLM persona line:
string line = DialogueManager.Instance.GetLine("sofia", "intro");

// Track DHLI Personalization score & get adaptive hint:
PersonalizationManager.Instance.RecordCorrectAction(DhliDomain.Privacy, 15);
PersonalizationManager.Instance.RecordMistake(DhliDomain.Privacy, 10);
string tip = PersonalizationManager.Instance.GetAdaptiveHint(DhliDomain.Privacy);

// Move camera to a station (WP2):
CameraViewpointController.Instance.MoveToViewpoint("Computer");

// Advance shared mission state:
MissionManager.Instance.SetState(MissionState.Daniel_Ready);
```

---

## 5. Git Rules (Preventing Unity Scene Merge Conflicts!)

1. **WP2** is the primary owner of `Assets/_Game/Scenes/OfficeScene.unity`.
2. **WP3, WP4, WP5** should build new UI panels in their own Prefabs (`Assets/_Game/Prefabs/`) and test scenes before wiring them into `OfficeScene.unity`.
3. Always follow: **1. Commit -> 2. Pull -> 3. Push** in GitHub Desktop.
