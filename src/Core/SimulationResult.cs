namespace SurvivalSim;

public sealed class SimulationResult
{
    public int  Run       { get; }
    public int  DeathStep { get; }   // -1 if survived all MaxSteps
    public bool Survived  { get; }

    public IReadOnlyList<StepRecord> Steps { get; }

    /// Per-agent motivation history. Populated only for run 0; null for all other runs.
    public IReadOnlyList<AgentMotivationRecord>? MotivationHistory { get; }

    public SimulationResult(
        int run,
        List<StepRecord> steps,
        List<AgentMotivationRecord>? motivationHistory = null)
    {
        Run               = run;
        Steps             = steps;
        MotivationHistory = motivationHistory;

        var deathRecord = steps.FirstOrDefault(s => !s.IsAlive);
        Survived  = deathRecord is null;
        DeathStep = deathRecord?.Step ?? -1;
    }
}
