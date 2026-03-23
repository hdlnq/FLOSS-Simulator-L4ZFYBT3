namespace SurvivalSim;

public sealed record SimulationConfig
{
    // ── Simulation scope ──────────────────────────────────────────────
    public int MaxSteps { get; init; } = 365 * 5;
    public int SimulationRuns { get; init; } = 10_000;
    public int? RandomSeed { get; init; } = null;

    // ── Initial population ───────────────────────────────────────────
    public int InitialCommitters { get; init; } = 1;
    public int InitialContributors { get; init; } = 0;

    // ── Activity dynamics ────────────────────────────────────────────
    // 1 step = 1 day: decay=0.01 → inactive threshold reached in ~80 days of no activity
    public double ActivityDecayRate { get; init; } = 0.01;

    // ── State transition probabilities ───────────────────────────────
    // P(Active→Inactive) = DeactivationRate × (1 − Motivation)
    // P(Inactive→Active) = ActivationRate   × Motivation
    public double DeactivationRate { get; init; } = 0.10;
    public double ActivationRate { get; init; } = 0.20;

    // ── Motivation ───────────────────────────────────────────────────
    // Per-agent motivation [0.0, 1.0]; decays each step, boosted by positive events
    // Initial value is uniformly random in [0.0, 1.0]

    // ── Committer behaviour ──────────────────────────────────────────
    public double CommitProbability { get; init; } = 0.40;
    public double CommitActivityBoost { get; init; } = 0.15;
    public double ReviewProbability { get; init; } = 0.60;
    public double MergeAcceptRate { get; init; } = 0.70;

    // ── Contributor behaviour ────────────────────────────────────────
    public double PrSubmitProbability { get; init; } = 0.30;
    public double PrMergeActivityBoost { get; init; } = 0.20;
    public double PrRejectActivityPenalty { get; init; } = 0.10;
    public double PrIgnoredActivityPenalty { get; init; } = 0.05;
    public int PrIgnoreThresholdSteps { get; init; } = 10;

    // ── Promotion ────────────────────────────────────────────────────
    public bool EnablePromotion { get; init; } = true;
    public double PromotionThreshold { get; init; } = 0.80;

    // 1 step = 1 day: 0.01 → eligible contributor promoted on average once every ~100 days
    public double PromotionProbability { get; init; } = 0.01;

    // ── Arrival process ──────────────────────────────────────────────
    // 1 step = 1 day: lambda=0.2 → ~1 new contributor per 5 days (~6/month)
    public bool EnableNewArrivals { get; init; } = true;
    public double NewContributorLambda { get; init; } = 0.01;

    // ── Output ───────────────────────────────────────────────────────
    public string OutputDirectory { get; init; } = "results";
}
