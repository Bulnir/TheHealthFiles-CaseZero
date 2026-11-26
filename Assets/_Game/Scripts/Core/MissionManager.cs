using System;
using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// WP3: Central mission state machine connecting WP2's 3D office interactions to WP3 (Alex),
    /// WP4 (Sofia), and WP5 (Daniel + Final Board).
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager Instance { get; private set; }

        [SerializeField] private MissionState currentState = MissionState.Prologue_SupervisorCall;

        public MissionState CurrentState => currentState;

        public event Action<MissionState, string> OnMissionChanged;
        public event Action<string> OnStatusMessage;
        // (speakerName, portraitKey, text, allowObjection)
        public event Action<string, string, string, bool> OnPhoneCallRequested;
        public event Action OnAlexComputerOpened;
        public event Action OnMailLogOpened;
        public event Action OnManualOpened;
        public event Action OnAlexLetterOpened;
        public event Action<string> OnBookOpened; // "manual", "myths", "privacy", "coffee"
        public event Action OnPrinterJamOpened;
        public event Action<string> OnObjectionSplash;
        public event Action OnCase01Completed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            SetState(MissionState.Prologue_SupervisorCall);
            NotifyStatus("Day 1 at the Digital Health Investigation Office. Three viral VitaPulse alerts just hit your computer—answer the ringing phone!");
        }

        /// <summary>
        /// Main entry point called by WP2's InteractableObject whenever the player clicks a 3D object.
        /// Frozen Interactable IDs:
        /// - bookshelf_manual
        /// - desk_phone
        /// - desk_mail_log
        /// - cabinet_drawer_1 / cabinet_drawer_2 / cabinet_drawer_3
        /// - cabinet_alex_letter
        /// - desk_computer
        /// - printer_daniel
        /// - evidence_board
        /// </summary>
        public void HandleInteractableClicked(string interactableId)
        {
            switch (interactableId)
            {
                case "bookshelf_manual":
                    HandleBookshelfClick();
                    break;
                case "book_easter_myths":
                    SoundManager.Instance?.Play("paper_rustle", 0.8f);
                    OnBookOpened?.Invoke("myths");
                    break;
                case "book_easter_privacy":
                    SoundManager.Instance?.Play("paper_rustle", 0.8f);
                    OnBookOpened?.Invoke("privacy");
                    break;
                case "book_easter_coffee":
                    SoundManager.Instance?.Play("paper_rustle", 0.8f);
                    OnBookOpened?.Invoke("coffee");
                    break;
                case "desk_phone":
                    HandlePhoneClick();
                    break;
                case "desk_mail_log":
                    SoundManager.Instance?.Play("paper_rustle", 0.8f);
                    OnMailLogOpened?.Invoke();
                    break;
                case "cabinet_alex_letter":
                    HandleAlexEnvelopeClick();
                    break;
                case "cabinet_drawer_1":
                case "cabinet_drawer_2":
                case "cabinet_drawer_3":
                case "desk_lamp":
                case "desk_coffee":
                case "office_water_cooler":
                case "desk_trash_can":
                case "office_window":
                    // Handled visually & audibly by WP2 SlidingDrawer / OfficePropInteractions.
                    break;
                case "desk_computer":
                    HandleComputerClick();
                    break;
                case "desk_pc_tower":
                    SoundManager.Instance?.Play("ui_click", 0.7f);
                    NotifyStatus("Investigation Workstation PC — Privacy Firewall: ACTIVE | Encrypted Storage: ON | Local Audit Mode Ready.");
                    break;
                case "printer_daniel":
                    HandlePrinterClick();
                    break;
                case "evidence_board":
                    HandleEvidenceBoardClick();
                    break;
                default:
                    NotifyStatus($"Inspected: {interactableId}");
                    break;
            }
        }

        private void HandleBookshelfClick()
        {
            if (currentState == MissionState.Prologue_SupervisorCall)
            {
                NotifyStatus("Your desk telephone is ringing! Answer your Supervisor first.");
                return;
            }

            SoundManager.Instance?.Play("paper_rustle", 0.8f);
            if (currentState == MissionState.Prologue_FindManual)
            {
                EvidenceManager.Instance.AddEvidence("manual_guide");
                SoundManager.Instance?.Play("evidence_unlock", 0.7f);
                PersonalizationManager.Instance?.RecordCorrectAction(DhliDomain.Navigation, 8);
                SetState(MissionState.Alex_AnswerFirstCall);
                NotifyStatus("Found the Investigation Manual (Source - Evidence - Context)! Now the phone is ringing with Case 01...");
            }
            OnBookOpened?.Invoke("manual");
        }

        private void HandlePhoneClick()
        {
            switch (currentState)
            {
                case MissionState.Prologue_SupervisorCall:
                {
                    string supText =
                        "\"Welcome to your first day, Investigator. A viral health app called VitaPulse is everywhere today—" +
                        "some claim it detects serious diseases early, others claim it secretly sells medical records.\n\n" +
                        "Your job is to investigate the evidence without jumping to conclusions. First, grab the Investigation Manual from the Bookshelf on your right.\"";
                    OnPhoneCallRequested?.Invoke("Office Supervisor", "supervisor", supText, false);
                    SetState(MissionState.Prologue_FindManual);
                    break;
                }

                case MissionState.Prologue_FindManual:
                    NotifyStatus("Check the Bookshelf on the right wall (key 4) to pick up the Investigation Manual.");
                    break;

                case MissionState.Alex_AnswerFirstCall:
                {
                    string alexIntro = DialogueManager.Instance.GetLine("alex", "intro");
                    OnPhoneCallRequested?.Invoke("Alex Moretti (Case 01)", "alex", alexIntro, false);
                    SetState(MissionState.Alex_FindHiddenLetter);
                    NotifyStatus("Tip: Check the Mail Delivery Log on your desk to see which Filing Cabinet drawer holds Alex's envelope!");
                    break;
                }

                case MissionState.Alex_FindHiddenLetter:
                {
                    string alexHint = DialogueManager.Instance.GetLine("alex", "hint");
                    OnPhoneCallRequested?.Invoke("Alex Moretti", "alex", alexHint + "\n\n(Check the Mail Delivery Log on your desk, then slide open the Archive Cabinet drawer!)", false);
                    break;
                }

                case MissionState.Alex_InspectBlogAndPolicy:
                {
                    string alexHint = DialogueManager.Instance.GetLine("alex", "hint", randomize: true);
                    OnPhoneCallRequested?.Invoke("Alex Moretti", "alex", alexHint + "\n\n(Inspect the Blog vs Privacy Policy on your Office Computer first!)", false);
                    break;
                }

                case MissionState.Alex_ObjectionCall:
                {
                    string prompt = "\"That blog proves VitaPulse is selling everyone's medical records!\"\n\nSelect the right evidence card below to challenge Alex's claim with OBJECTION!";
                    OnPhoneCallRequested?.Invoke("Alex Moretti — OBJECTION!", "alex", prompt, true);
                    break;
                }

                case MissionState.Alex_PinPolicyClause:
                {
                    string clue = DialogueManager.Instance.GetLine("alex", "clue");
                    OnPhoneCallRequested?.Invoke("Alex Moretti", "alex", clue + "\n\n(Click the Cork Evidence Board behind your desk to pin Section 4.2!)", false);
                    break;
                }

                case MissionState.Sofia_Ready:
                {
                    string sofiaIntro = DialogueManager.Instance.GetLine("sofia", "intro");
                    OnPhoneCallRequested?.Invoke("Sofia Romano (Case 02 — WP4)", "sofia", sofiaIntro + "\n\n[Case 01 Alex Vertical Slice Complete! WP4 plugs Sofia's social-media & privacy puzzles here.]", false);
                    break;
                }

                default:
                    NotifyStatus("No active phone calls right now.");
                    break;
            }
        }

        private void HandleAlexEnvelopeClick()
        {
            if (currentState < MissionState.Alex_FindHiddenLetter)
            {
                NotifyStatus("An archive envelope labeled 'A. Moretti'. Answer your incoming calls first!");
                return;
            }

            SoundManager.Instance?.Play("paper_rustle", 0.85f);
            if (currentState == MissionState.Alex_FindHiddenLetter)
            {
                EvidenceManager.Instance.AddEvidence("letter_alex");
                SoundManager.Instance?.Play("evidence_unlock", 0.75f);
                PersonalizationManager.Instance?.RecordCorrectAction(DhliDomain.Navigation, 12);
                SetState(MissionState.Alex_InspectBlogAndPolicy);
                NotifyStatus("Collected Alex's Letter! Click your Office Computer (key 2) to inspect the blog side by side with VitaPulse's Privacy Policy.");
            }
            OnAlexLetterOpened?.Invoke();
        }

        private void HandleComputerClick()
        {
            // Always open the Office Computer Desktop UI so the monitor always feels alive & responsive!
            OnAlexComputerOpened?.Invoke();
        }

        private void HandlePrinterClick()
        {
            SoundManager.Instance?.Play("wrong_buzz", 0.45f);
            OnPrinterJamOpened?.Invoke();
        }

        private void HandleEvidenceBoardClick()
        {
            if (currentState == MissionState.Alex_PinPolicyClause)
            {
                EvidenceManager.Instance.AddEvidence("policy_42");
                SoundManager.Instance?.Play("evidence_unlock", 0.85f);
                PersonalizationManager.Instance?.RecordCorrectAction(DhliDomain.Privacy, 12);
                SetState(MissionState.Sofia_Ready);
                OnCase01Completed?.Invoke();
                NotifyStatus("Pinned VitaPulse Privacy Policy - Section 4.2 to the Cork Board! Case 01 Complete! Sofia Romano is calling...");
            }
            else if (currentState == MissionState.FinalBoard_Ready)
            {
                NotifyStatus("Final Evidence Board unlocked! Ready for WP5's drag-and-drop finale.");
            }
            else
            {
                NotifyStatus("Evidence Cork Board: Every major clue you uncover gets pinned right here.");
            }
        }

        public void RecordWrongBlogSelection()
        {
            SoundManager.Instance?.Play("wrong_buzz", 0.65f);
            PersonalizationManager.Instance?.RecordMistake(DhliDomain.Reliability, 6);
        }

        public void CompleteAlexBlogInspection()
        {
            if (currentState != MissionState.Alex_InspectBlogAndPolicy)
                return;

            EvidenceManager.Instance.AddEvidence("blog_unverified");
            SoundManager.Instance?.Play("evidence_unlock", 0.85f);
            PersonalizationManager.Instance?.RecordCorrectAction(DhliDomain.Reliability, 15);
            SetState(MissionState.Alex_ObjectionCall);
            NotifyStatus("Obtained 'Unverified Blog Claim'! Click the Desk Telephone (key 1) to call Alex back and present your OBJECTION!");
        }

        /// <summary>
        /// Reusable Objection validation API for WP3 (Alex) and WP4 (Sofia).
        /// </summary>
        public bool TrySubmitObjection(string presentedEvidenceId, out string npcResponse, out string adaptiveHint)
        {
            adaptiveHint = string.Empty;

            if (currentState == MissionState.Alex_ObjectionCall)
            {
                if (presentedEvidenceId == "blog_unverified")
                {
                    SoundManager.Instance?.Play("objection_sting", 0.9f);
                    OnObjectionSplash?.Invoke("OBJECTION!");
                    PersonalizationManager.Instance?.RecordCorrectAction(DhliDomain.Reliability, 20);
                    string conceded = DialogueManager.Instance.GetLine("alex", "correct_answer");
                    string clue = DialogueManager.Instance.GetLine("alex", "clue");
                    npcResponse = $"Alex: \"{conceded}\"\n\nAlex: \"{clue}\"";
                    SetState(MissionState.Alex_PinPolicyClause);
                    NotifyStatus("OBJECTION succeeded! Now click the Cork Evidence Board (key 6) to pin Section 4.2.");
                    return true;
                }
                else
                {
                    SoundManager.Instance?.Play("wrong_buzz", 0.75f);
                    PersonalizationManager.Instance?.RecordMistake(DhliDomain.Reliability, 10);
                    npcResponse = $"Alex: \"{DialogueManager.Instance.GetLine("alex", "wrong_answer", randomize: true)}\"";
                    adaptiveHint = PersonalizationManager.Instance != null
                        ? PersonalizationManager.Instance.GetAdaptiveHint(DhliDomain.Reliability)
                        : "Check which card proves the blog has no reliable author or source.";
                    return false;
                }
            }

            npcResponse = "No active objection right now.";
            return false;
        }

        public void SetState(MissionState newState)
        {
            currentState = newState;
            OnMissionChanged?.Invoke(currentState, GetObjectiveText(currentState));
        }

        public void NotifyStatus(string message)
        {
            OnStatusMessage?.Invoke(message);
        }

        public static string GetObjectiveText(MissionState state)
        {
            switch (state)
            {
                case MissionState.Prologue_SupervisorCall:
                    return "PROLOGUE: Your desk Telephone is ringing! Click the red Phone to speak with your Supervisor.";
                case MissionState.Prologue_FindManual:
                    return "PROLOGUE: Look at the Bookshelf (key 4) and pick up the Investigation Manual.";
                case MissionState.Alex_AnswerFirstCall:
                    return "CASE 01 (ALEX): Incoming call! Click the Telephone on your desk.";
                case MissionState.Alex_FindHiddenLetter:
                    return "CASE 01 (ALEX): Check the Mail Delivery Log on your desk, then slide open the Archive Cabinet (key 3) to find Alex's envelope.";
                case MissionState.Alex_InspectBlogAndPolicy:
                    return "CASE 01 (ALEX): Click the Office Computer (key 2) to inspect Alex's blog against VitaPulse's Privacy Policy.";
                case MissionState.Alex_ObjectionCall:
                    return "CASE 01 (ALEX): Call Alex back on the Telephone (key 1) and challenge his claim with OBJECTION!";
                case MissionState.Alex_PinPolicyClause:
                    return "CASE 01 (ALEX): Click the Cork Evidence Board (key 6) to pin Privacy Policy Section 4.2.";
                case MissionState.Sofia_Ready:
                    return "CASE 01 COMPLETE! Sofia Romano is calling (WP4) — click 'Demo Summary' anytime to inspect your DHLI Profile!";
                case MissionState.Daniel_Ready:
                    return "CASE 03 (DANIEL): Check the Printer tray and Verified Health Portal (WP5).";
                case MissionState.FinalBoard_Ready:
                    return "FINAL CASE: Connect all evidence pairs on the Evidence Board and publish the report (WP5).";
                case MissionState.CaseClosed:
                    return "THE HEALTH FILES: CASE ZERO - CASE CLOSED.";
                default:
                    return string.Empty;
            }
        }
    }
}
