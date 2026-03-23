namespace SurvivalSim;

public sealed class SimulationEngine
{
    private readonly SimulationConfig _config;

    public SimulationEngine(SimulationConfig config) => _config = config;

    public SimulationResult RunOnce(int runIndex)
    {
        var rng = _config.RandomSeed.HasValue
            ? new Random(_config.RandomSeed.Value + runIndex)
            : new Random();

        int nextId = 0;
        var committers   = new List<Committer>();
        var contributors = new List<Contributor>();

        for (int i = 0; i < _config.InitialCommitters; i++)
            committers.Add(new Committer(nextId++, RandomMotivation(rng), RandomSkillLevel(rng), _config, rng));

        for (int i = 0; i < _config.InitialContributors; i++)
            contributors.Add(new Contributor(nextId++, RandomMotivation(rng), RandomSkillLevel(rng), _config, rng));

        var prQueue       = new PullRequestQueue(_config);
        var records       = new List<StepRecord>(_config.MaxSteps);
        var motivHistory  = runIndex == 0
            ? new List<AgentMotivationRecord>()
            : null;

        for (int step = 1; step <= _config.MaxSteps; step++)
        {
            int promotionsThisStep  = 0;
            int newArrivalsThisStep = 0;

            // 1. Decay + probabilistic state transition
            foreach (var a in committers)   { a.ApplyDecay(); a.ApplyStateTransition(); }
            foreach (var a in contributors) { a.ApplyDecay(); a.ApplyStateTransition(); }

            // 2. PR ignore penalties
            prQueue.ApplyIgnorePenalties();

            // 3. Reset per-step PR counters
            prQueue.ResetStepCounters();

            // 4. Contributor steps
            foreach (var c in contributors)
                c.Step(prQueue, step);

            // 5. Committer steps (commit + review PRs)
            int commitsThisStep = 0;
            foreach (var cm in committers)
                commitsThisStep += cm.Step(prQueue, step);

            // 6. Promotion pass: each active Committer may promote one eligible Contributor
            if (_config.EnablePromotion)
            {
                var eligible = contributors
                    .Where(c => c.EligibleForPromotion)
                    .ToList();

                var promoted = new List<Contributor>();
                foreach (var cm in committers.Where(c => c.IsActive).ToList())
                {
                    if (eligible.Count == 0) break;

                    var candidate    = eligible[rng.Next(eligible.Count)];
                    var newCommitter = cm.TryPromote(candidate, nextId);
                    if (newCommitter is not null)
                    {
                        committers.Add(newCommitter);
                        promoted.Add(candidate);
                        eligible.Remove(candidate);
                        nextId++;
                        promotionsThisStep++;
                    }
                }
                foreach (var p in promoted) contributors.Remove(p);
            }

            // 7. New contributor arrivals (Poisson)
            if (_config.EnableNewArrivals)
            {
                int arrivals = SamplePoisson(rng, _config.NewContributorLambda);
                for (int i = 0; i < arrivals; i++)
                {
                    contributors.Add(new Contributor(nextId++, RandomMotivation(rng), RandomSkillLevel(rng), _config, rng));
                    newArrivalsThisStep++;
                }
            }

            // 8. Record state
            int activeC  = committers.Count(a => a.IsActive);
            int activeK  = contributors.Count(a => a.IsActive);
            bool isAlive = activeC > 0;

            records.Add(new StepRecord(
                Run:                  runIndex,
                Step:                 step,
                ActiveCommitters:     activeC,
                InactiveCommitters:   committers.Count - activeC,
                ActiveContributors:   activeK,
                InactiveContributors: contributors.Count - activeK,
                PendingPRs:           prQueue.PendingPRs.Count,
                PromotionsThisStep:   promotionsThisStep,
                NewArrivalsThisStep:  newArrivalsThisStep,
                CommitsThisStep:      commitsThisStep,
                PrSubmittedThisStep:  prQueue.SubmittedThisStep,
                PrMergedThisStep:     prQueue.MergedThisStep,
                IsAlive:              isAlive
            ));

            // 9. Record per-agent motivation (run 0 only)
            if (motivHistory is not null)
            {
                foreach (var c in committers)
                    motivHistory.Add(new AgentMotivationRecord(step, c.Id, "Committer", c.Motivation, c.State));
                foreach (var c in contributors)
                    motivHistory.Add(new AgentMotivationRecord(step, c.Id, "Contributor", c.Motivation, c.State));
            }

            // 10. Early exit on death
            if (!isAlive) break;
        }

        return new SimulationResult(runIndex, records, motivHistory);
    }

    private static double RandomMotivation(Random rng) => rng.NextDouble();
    private static double RandomSkillLevel(Random rng)  => rng.NextDouble();

    /// Knuth's algorithm for Poisson sampling.
    private static int SamplePoisson(Random rng, double lambda)
    {
        if (lambda <= 0.0) return 0;

        double L = Math.Exp(-lambda);
        int k = 0;
        double p = 1.0;
        do
        {
            k++;
            p *= rng.NextDouble();
        }
        while (p > L);

        return k - 1;
    }
}
