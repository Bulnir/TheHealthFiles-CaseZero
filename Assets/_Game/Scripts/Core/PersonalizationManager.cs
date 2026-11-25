using System;
using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// WP3 Personalization System (Directly addresses Project Requirements Section II: Personalization Techniques).
    /// Tracks player accuracy across the 3 Digital Health Literacy Instrument (DHLI) dimensions
    /// represented by the personas: Reliability (Alex), Privacy (Sofia), Navigation (Daniel).
    /// Automatically activates adaptive scaffolding hints when a player makes mistakes in a specific domain.
    /// </summary>
    public class PersonalizationManager : MonoBehaviour
    {
        public static PersonalizationManager Instance { get; private set; }

        [Header("DHLI Skill Scores (0 - 100, starts at 50)")]
        public int reliabilityScore = 50;
        public int privacyScore = 50;
        public int navigationScore = 50;

        [Header("Mistake Counters (triggers adaptive hints)")]
        public int reliabilityMistakes;
        public int privacyMistakes;
        public int navigationMistakes;

        [Header("Onboarding Self-Check State")]
        public bool onboardingCompleted;
        public string onboardingSummary = "Default Baseline (50/50/50)";

        public event Action OnProfileUpdated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Seeds initial DHLI scores from the optional 3-question Quick Investigator Onboarding check.
        /// Confidence values: 1 = Low (38), 2 = Medium (55), 3 = High (72).
        /// </summary>
        public void ApplyOnboardingSelfCheck(int reliabilityConfidence, int privacyConfidence, int navigationConfidence)
        {
            reliabilityScore = ConfidenceToInitialScore(reliabilityConfidence);
            privacyScore = ConfidenceToInitialScore(privacyConfidence);
            navigationScore = ConfidenceToInitialScore(navigationConfidence);

            onboardingCompleted = true;
            onboardingSummary = $"Calibrated via Self-Check (Focus: {GetWeakestDomain()})";
            OnProfileUpdated?.Invoke();
        }

        private static int ConfidenceToInitialScore(int level)
        {
            switch (level)
            {
                case 1: return 38; // Low -> triggers concrete adaptive scaffolding earlier
                case 3: return 72; // High -> confident baseline
                default: return 55; // Medium -> balanced baseline
            }
        }

        public static string GetLevelLabel(int score)
        {
            if (score < 45) return "Low";
            if (score < 70) return "Intermediate";
            return "High";
        }

        public int GetScore(DhliDomain domain)
        {
            switch (domain)
            {
                case DhliDomain.Reliability: return reliabilityScore;
                case DhliDomain.Privacy: return privacyScore;
                case DhliDomain.Navigation: return navigationScore;
                default: return 50;
            }
        }

        public bool IsGuidedMode(DhliDomain domain)
        {
            return GetScore(domain) < 45;
        }

        public string GetAssignedModeLabel()
        {
            if (reliabilityScore < 45 || privacyScore < 45 || navigationScore < 45)
                return "Guided Investigator";
            if (reliabilityScore >= 70 && privacyScore >= 70 && navigationScore >= 70)
                return "Independent Analyst";
            return "Balanced Investigator";
        }

        public string GetInvestigatorRank(int collectedEvidenceCount)
        {
            int avg = (reliabilityScore + privacyScore + navigationScore) / 3;
            if (collectedEvidenceCount >= 4 || avg >= 74) return "Lead Evidence Analyst";
            if (collectedEvidenceCount >= 3 || avg >= 62) return "Health Literacy Detective";
            if (collectedEvidenceCount >= 1 || avg >= 54) return "Junior Fact-Checker";
            return "Rookie Investigator";
        }

        public DhliDomain GetWeakestDomain()
        {
            DhliDomain weakest = DhliDomain.Reliability;
            int minVal = reliabilityScore;
            if (privacyScore < minVal) { minVal = privacyScore; weakest = DhliDomain.Privacy; }
            if (navigationScore < minVal) { weakest = DhliDomain.Navigation; }
            return weakest;
        }

        public void RecordCorrectAction(DhliDomain domain, int bonus = 15)
        {
            AdjustScore(domain, bonus);
            OnProfileUpdated?.Invoke();
        }

        public void RecordMistake(DhliDomain domain, int penalty = 10)
        {
            AdjustScore(domain, -penalty);
            switch (domain)
            {
                case DhliDomain.Reliability: reliabilityMistakes++; break;
                case DhliDomain.Privacy: privacyMistakes++; break;
                case DhliDomain.Navigation: navigationMistakes++; break;
            }
            OnProfileUpdated?.Invoke();
        }

        /// <summary>
        /// Returns an adaptive hint tailored to the player's struggling DHLI domain.
        /// Concrete scaffolding activates after 2+ mistakes, OR after 1 mistake if the player's
        /// score in that domain is Low (< 45, e.g. from onboarding self-check).
        /// </summary>
        public string GetAdaptiveHint(DhliDomain domain)
        {
            switch (domain)
            {
                case DhliDomain.Reliability:
                    bool concreteRel = reliabilityMistakes >= 2 || (reliabilityScore < 45 && reliabilityMistakes >= 1);
                    return concreteRel
                        ? "Adaptive Tip (Source Check): Look for the card proving the blog has NO author and NO traceable source ('Unverified Blog Claim')."
                        : "Tip (Source - Evidence - Context): Check who wrote the claim and whether solid evidence backs it up.";
                case DhliDomain.Privacy:
                    bool concretePriv = privacyMistakes >= 2 || (privacyScore < 45 && privacyMistakes >= 1);
                    return concretePriv
                        ? "Adaptive Tip (Data Minimization): A personal health diary only needs health data—turn off Contacts, Location, and Microphone."
                        : "Tip (Privacy): Review what data each permission collects and whether the app's stated purpose actually requires it.";
                case DhliDomain.Navigation:
                    bool concreteNav = navigationMistakes >= 2 || (navigationScore < 45 && navigationMistakes >= 1);
                    return concreteNav
                        ? "Adaptive Tip (Official Sources): Match the portal URL against the verified directory, then open Health Resources."
                        : "Tip (Navigation): Always verify a portal link before trusting screenshots or medical approval claims.";
                default:
                    return "Review your collected evidence carefully.";
            }
        }

        public string GetPersonalizedReportSummary()
        {
            DhliDomain weakest = GetWeakestDomain();
            return $"Investigator DHLI Profile -> Reliability: {reliabilityScore}/100 ({GetLevelLabel(reliabilityScore)}) | " +
                   $"Privacy: {privacyScore}/100 ({GetLevelLabel(privacyScore)}) | " +
                   $"Navigation: {navigationScore}/100 ({GetLevelLabel(navigationScore)}). " +
                   $"Recommended focus area: {weakest}.";
        }

        private void AdjustScore(DhliDomain domain, int delta)
        {
            switch (domain)
            {
                case DhliDomain.Reliability:
                    reliabilityScore = Mathf.Clamp(reliabilityScore + delta, 0, 100);
                    break;
                case DhliDomain.Privacy:
                    privacyScore = Mathf.Clamp(privacyScore + delta, 0, 100);
                    break;
                case DhliDomain.Navigation:
                    navigationScore = Mathf.Clamp(navigationScore + delta, 0, 100);
                    break;
            }
        }
    }
}
