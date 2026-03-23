namespace SurvivalSim;

public sealed class PullRequestQueue
{
    private readonly List<PullRequest>  _pending = new();
    private readonly SimulationConfig   _config;
    private int _nextId = 0;

    public IReadOnlyList<PullRequest> PendingPRs => _pending;

    public int SubmittedThisStep { get; private set; }
    public int MergedThisStep    { get; private set; }

    public PullRequestQueue(SimulationConfig config) => _config = config;

    public void ResetStepCounters()
    {
        SubmittedThisStep = 0;
        MergedThisStep    = 0;
    }

    /// Returns and increments the per-queue PR ID counter (no static state).
    public int NextPrId() => _nextId++;

    public void Submit(PullRequest pr)
    {
        _pending.Add(pr);
        SubmittedThisStep++;
    }

    public void MergePR(PullRequest pr)
    {
        pr.SetMerged();
        pr.Author.OnPrMerged();
        _pending.Remove(pr);
        MergedThisStep++;
    }

    public void RejectPR(PullRequest pr)
    {
        pr.SetRejected();
        pr.Author.OnPrRejected();
        _pending.Remove(pr);
    }

    /// Increments age of all pending PRs and applies ignore penalty beyond threshold.
    public void ApplyIgnorePenalties()
    {
        foreach (var pr in _pending)
        {
            pr.IncrementAge();
            if (pr.Age >= _config.PrIgnoreThresholdSteps)
                pr.Author.OnPrIgnoredPenalty();
        }
    }
}
