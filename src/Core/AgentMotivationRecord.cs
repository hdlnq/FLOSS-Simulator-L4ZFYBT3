namespace SurvivalSim;

public sealed record AgentMotivationRecord(
    int        Step,
    int        AgentId,
    string     AgentType,   // "Committer" or "Contributor"
    double     Motivation,
    AgentState State
);
