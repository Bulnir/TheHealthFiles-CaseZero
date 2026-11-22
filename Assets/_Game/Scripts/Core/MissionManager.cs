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

        [SerializeField] private MissionState currentState = MissionState.Prologue_FindManual;

        public MissionState CurrentState => currentState;

        public event Action<MissionState, string> OnMissionChanged;
        public event Action<string> OnStatusMessage;
        public event Action<string, string, bool> OnPhoneCallRequested; // (speakerName, text, allowObjection)
        public event Action OnAlexComputerOpened;

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
            SetState(MissionState.Prologue_FindManual);
            NotifyStatus("Welcome to the Digital Health Investigation Office! Supervisor: 'Find the Investigation Manual on the bookshelf.'");
        }

        /// <summary>
        /// Main entry point called by WP2's InteractableObject whenever the player clicks a 3D object.
        /// Frozen Interactable IDs:
        /// - bookshelf_manual
        /// - desk_phone
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
                case "desk_phone":
                    HandlePhoneClick();
                    break;
                case "cabinet_alex_letter":
                    HandleCabinetClick();
                    break;
                case "desk_computer":
                    HandleComputerClick();
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
            if (currentState == MissionState.Prologue_FindManual)
            {
                EvidenceManager.Instance.AddEvidence("manual_guide");
                PersonalizationManager.Instance?.RecordCorrectAction(DhliDomain.Navigation, 5);
                SetState(MissionState.Alex_AnswerFirstCall);
                NotifyStatus("Found the Investigation Manual (Source - Evidence - Context)! The desk phone is ringing...");
            }
            else
            {
                NotifyStatus("Investigation Manual reminder: Always check Source, Evidence, and Context.");
            }
        }

        private void HandlePhoneClick()
        {
            switch (currentState)
            {
                case MissionState.Prologue_FindManual:
                    NotifyStatus("Find the Investigation Manual on the bookshelf before taking your first call.");
                    break;

                case MissionState.Alex_AnswerFirstCall:
                {
                    string alexIntro = DialogueManager.Instance.GetLine("alex", "intro");
                    OnPhoneCallRequested?.Invoke("Alex Moretti", alexIntro, false);
                    SetState(MissionState.Alex_FindHiddenLetter);
                    break;
                }

                case MissionState.Alex_FindHiddenLetter:
                {
                    string alexHint = DialogueManager.Instance.GetLine("alex", "hint");
                    OnPhoneCallRequested?.Invoke("Alex Moretti", alexHint + "\n\n(Check the Filing Cabinet for Alex's envelope!)", false);
                    break;
                }

                case MissionState.Alex_InspectBlogAndPolicy:
                {
                    string alexHint = DialogueManager.Instance.GetLine("alex", "hint", randomize: true);
                    OnPhoneCallRequested?.Invoke("Alex Moretti", alexHint + "\n\n(Inspect the blog and privacy policy on the Computer first!)", false);
                    break;
                }

                case MissionState.Alex_ObjectionCall:
                {
                    string prompt = "\"That blog proves VitaPulse is selling everyone's medical records!\"\n\nSelect the right evidence card from your inventory to trigger OBJECTION!";
                    OnPhoneCallRequested?.Invoke("Alex Moretti (OBJECTION!)", prompt, true);
                    break;
                }

                case MissionState.Alex_PinPolicyClause:
                {
                    string clue = DialogueManager.Instance.GetLine("alex", "clue");
                    OnPhoneCallRequested?.Invoke("Alex Moretti", clue + "\n\n(Pin Section 4.2 onto the Evidence Board!)", false);
                    break;
                }

                case MissionState.Sofia_Ready:
                {
                    string sofiaIntro = DialogueManager.Instance.GetLine("sofia", "intro");
                    OnPhoneCallRequested?.Invoke("Sofia Romano (WP4 Hook)", sofiaIntro + "\n\n[Alex Vertical Slice Complete! WP4 plugs Sofia's puzzles here.]", false);
                    break;
                }

                default:
                    NotifyStatus("No active phone calls right now.");
                    break;
            }
        }

        private void HandleCabinetClick()
        {
            if (currentState == MissionState.Alex_FindHiddenLetter)
            {
                EvidenceManager.Instance.AddEvidence("letter_alex");
                PersonalizationManager.Instance?.RecordCorrectAction(DhliDomain.Navigation, 10);
                SetState(MissionState.Alex_InspectBlogAndPolicy);
                NotifyStatus("Found Alex's hidden letter! The blog link is now unlocked on your Office Computer.");
            }
            else if (currentState < MissionState.Alex_FindHiddenLetter)
            {
                NotifyStatus("Archive filing cabinet. Answer the phone first to know which envelope to search for.");
            }
            else
            {
                NotifyStatus("You already collected Alex's letter from the archive cabinet.");
            }
        }

        private void HandleComputerClick()
        {
            if (currentState == MissionState.Alex_InspectBlogAndPolicy || currentState == MissionState.Alex_ObjectionCall)
            {
                OnAlexComputerOpened?.Invoke();
            }
            else if (currentState == MissionState.Sofia_Ready)
            {
                NotifyStatus("Computer ready for WP4's Social Media & Privacy Permission UI!");
            }
            else if (currentState == MissionState.Daniel_Ready)
            {
                NotifyStatus("Computer ready for WP5's Verified Health Portal UI!");
            }
            else
            {
                NotifyStatus("VitaPulse viral notifications are on screen. Follow your current objective first.");
            }
        }

        private void HandlePrinterClick()
        {
            if (currentState == MissionState.Daniel_Ready)
            {
                NotifyStatus("Printer tray checked! Ready for WP5's Daniel link sheet.");
            }
            else
            {
                NotifyStatus("Office printer is idle (used in Case 03 - Daniel).");
            }
        }

        private void HandleEvidenceBoardClick()
        {
            if (currentState == MissionState.Alex_PinPolicyClause)
            {
                EvidenceManager.Instance.AddEvidence("policy_42");
                PersonalizationManager.Instance?.RecordCorrectAction(DhliDomain.Privacy, 10);
                SetState(MissionState.Sofia_Ready);
                NotifyStatus("Pinned VitaPulse Privacy Policy - Section 4.2 to the board! Case 01 Complete! The phone is ringing (Sofia - WP4)...");
            }
            else if (currentState == MissionState.FinalBoard_Ready)
            {
                NotifyStatus("Final Evidence Board unlocked! Ready for WP5's drag-and-drop connection finale.");
            }
            else
            {
                NotifyStatus("Evidence Board: Clues from Alex, Sofia, and Daniel will be connected here.");
            }
        }

        /// <summary>
        /// Called by CaseZeroHUD when the player marks the unsupported claims in Alex's blog vs privacy policy.
        /// </summary>
        public void CompleteAlexBlogInspection()
        {
            if (currentState != MissionState.Alex_InspectBlogAndPolicy)
                return;

            EvidenceManager.Instance.AddEvidence("blog_unverified");
            PersonalizationManager.Instance?.RecordCorrectAction(DhliDomain.Reliability, 15);
            SetState(MissionState.Alex_ObjectionCall);
            NotifyStatus("Obtained 'Unverified Blog Claim'! Click the Telephone to call Alex back and present your OBJECTION!");
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
                    PersonalizationManager.Instance?.RecordCorrectAction(DhliDomain.Reliability, 20);
                    string conceded = DialogueManager.Instance.GetLine("alex", "correct_answer");
                    string clue = DialogueManager.Instance.GetLine("alex", "clue");
                    npcResponse = $"OBJECTION ACCEPTED!\n\nAlex: \"{conceded}\"\n\nAlex: \"{clue}\"";
                    SetState(MissionState.Alex_PinPolicyClause);
                    NotifyStatus("OBJECTION succeeded! Now click the Evidence Board to pin Privacy Policy Section 4.2.");
                    return true;
                }
                else
                {
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

        /// <summary>
        /// Public transition API so WP4 (Sofia) and WP5 (Daniel/Finale) can advance the shared state machine.
        /// </summary>
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
                case MissionState.Prologue_FindManual:
                    return "PROLOGUE: Click the Bookshelf (or press 4) to find the Investigation Manual.";
                case MissionState.Alex_AnswerFirstCall:
                    return "CASE 01 (ALEX): The phone is ringing! Click the Telephone on your desk.";
                case MissionState.Alex_FindHiddenLetter:
                    return "CASE 01 (ALEX): Open the Filing Cabinet (press 3) to find Alex's hidden letter.";
                case MissionState.Alex_InspectBlogAndPolicy:
                    return "CASE 01 (ALEX): Click the Computer (press 2) to compare Alex's blog with VitaPulse's privacy policy.";
                case MissionState.Alex_ObjectionCall:
                    return "CASE 01 (ALEX): Call Alex back on the Telephone (press 1) and present your OBJECTION!";
                case MissionState.Alex_PinPolicyClause:
                    return "CASE 01 (ALEX): Click the Evidence Board (press 6) to pin Privacy Policy Section 4.2.";
                case MissionState.Sofia_Ready:
                    return "CASE 01 COMPLETE! Phone ringing: Sofia Romano (WP4 plug-in point).";
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
