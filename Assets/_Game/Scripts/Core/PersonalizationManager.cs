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
        /// After 1+ mistakes, guidance becomes more concrete (scaffolding).
        /// </summary>
        public string GetAdaptiveHint(DhliDomain domain)
        {
            switch (domain)
            {
                case DhliDomain.Reliability:
                    return reliabilityMistakes >= 2
                        ? "Adaptive Tip (Source Check): Look for the card proving the blog has NO author and NO traceable source ('Unverified Blog Claim')."
                        : "Tip (Source - Evidence - Context): Check who wrote the claim and whether solid evidence backs it up.";
                case DhliDomain.Privacy:
                    return privacyMistakes >= 2
                        ? "Adaptive Tip (Data Minimization): A personal health diary only needs health data—turn off Contacts, Location, and Microphone."
                        : "Tip (Privacy): Review what data each permission collects and whether the app's stated purpose actually requires it.";
                case DhliDomain.Navigation:
                    return navigationMistakes >= 2
                        ? "Adaptive Tip (Official Sources): Match the portal URL against the verified directory, then open Health Resources."
                        : "Tip (Navigation): Always verify a portal link before trusting screenshots or medical approval claims.";
                default:
                    return "Review your collected evidence carefully.";
            }
        }

        public string GetPersonalizedReportSummary()
        {
            DhliDomain weakest = DhliDomain.Reliability;
            int minVal = reliabilityScore;
            if (privacyScore < minVal) { minVal = privacyScore; weakest = DhliDomain.Privacy; }
            if (navigationScore < minVal) { weakest = DhliDomain.Navigation; }

            return $"Investigator DHLI Profile -> Reliability: {reliabilityScore}/100 | Privacy: {privacyScore}/100 | Navigation: {navigationScore}/100. " +
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
