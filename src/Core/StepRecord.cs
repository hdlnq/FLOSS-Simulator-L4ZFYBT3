namespace SurvivalSim;

public sealed record StepRecord(
    int  Run,
    int  Step,
    int  ActiveCommitters,
    int  InactiveCommitters,
    int  ActiveContributors,
    int  InactiveContributors,
    int  PendingPRs,
    int  PromotionsThisStep,
    int  NewArrivalsThisStep,
    int  CommitsThisStep,
    int  PrSubmittedThisStep,
    int  PrMergedThisStep,
    bool IsAlive
);
