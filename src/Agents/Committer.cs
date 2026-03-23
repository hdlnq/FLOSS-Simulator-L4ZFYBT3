namespace SurvivalSim;

public sealed class Committer : AgentBase
{
    public Committer(int id, double motivation, double skillLevel, SimulationConfig config, Random rng)
        : base(id, motivation, skillLevel, config, rng) { }

    public override int Step(PullRequestQueue prQueue, int currentStep)
    {
        if (!IsActive) return 0;

        int commits = 0;

        // Possibly commit directly (scaled by Motivation)
        if (Rng.NextDouble() < Config.CommitProbability * Motivation)
        {
            Motivation += Config.CommitActivityBoost;
            ClampMotivation();
            commits++;
        }

        // Review each pending PR (iterate snapshot to allow safe removal)
        foreach (var pr in prQueue.PendingPRs.ToList())
        {
            if (pr.State != PrState.Pending) continue;

            if (Rng.NextDouble() < Config.ReviewProbability)
            {
                if (Rng.NextDouble() < Config.MergeAcceptRate)
                    prQueue.MergePR(pr);
                else
                    prQueue.RejectPR(pr);
            }
        }

        return commits;
    }

    /// <summary>
    /// Attempts to promote <paramref name="candidate"/> to a Committer.
    /// Returns the new Committer, or null if promotion did not happen.
    /// </summary>
    public Committer? TryPromote(Contributor candidate, int newId)
    {
        if (candidate.Motivation < Config.PromotionThreshold) return null;
        if (Rng.NextDouble() >= Config.PromotionProbability)  return null;

        return new Committer(newId, candidate.Motivation, candidate.SkillLevel, Config, Rng);
    }
}
