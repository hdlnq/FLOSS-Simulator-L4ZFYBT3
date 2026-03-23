namespace SurvivalSim;

public sealed class Contributor : AgentBase
{
    public bool EligibleForPromotion => Motivation >= Config.PromotionThreshold;

    public Contributor(int id, double motivation, double skillLevel, SimulationConfig config, Random rng)
        : base(id, motivation, skillLevel, config, rng) { }

    public override int Step(PullRequestQueue prQueue, int currentStep)
    {
        if (!IsActive) return 0;

        // PR submission probability scaled by Motivation
        if (Rng.NextDouble() < Config.PrSubmitProbability * Motivation)
            prQueue.Submit(new PullRequest(prQueue.NextPrId(), this, currentStep));

        return 0;
    }

    public void OnPrMerged()
    {
        Motivation += Config.PrMergeActivityBoost;
        ClampMotivation();
    }

    public void OnPrRejected()
    {
        Motivation = Math.Max(0.0, Motivation - Config.PrRejectActivityPenalty);
    }

    public void OnPrIgnoredPenalty()
    {
        Motivation = Math.Max(0.0, Motivation - Config.PrIgnoredActivityPenalty);
    }
}
