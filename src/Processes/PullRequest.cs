namespace SurvivalSim;

public enum PrState { Pending, Merged, Rejected }

public sealed class PullRequest
{
    public int         Id          { get; }
    public Contributor Author      { get; }
    public int         SubmittedAt { get; }
    public PrState     State       { get; private set; } = PrState.Pending;
    public int         Age         { get; private set; }

    public PullRequest(int id, Contributor author, int currentStep)
    {
        Id          = id;
        Author      = author;
        SubmittedAt = currentStep;
    }

    public void IncrementAge() => Age++;
    public void SetMerged()    => State = PrState.Merged;
    public void SetRejected()  => State = PrState.Rejected;
}
