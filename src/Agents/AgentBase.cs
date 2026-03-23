namespace SurvivalSim;

public abstract class AgentBase
{
    public int        Id         { get; }
    public double     Motivation  { get; protected set; }
    public double     SkillLevel  { get; protected set; }
    public AgentState State      { get; private set; } = AgentState.Active;
    public bool       IsActive   => State == AgentState.Active;

    protected readonly SimulationConfig Config;
    protected readonly Random Rng;

    protected AgentBase(int id, double motivation, double skillLevel, SimulationConfig config, Random rng)
    {
        Id         = id;
        Motivation = Math.Clamp(motivation,  0.0, 1.0);
        SkillLevel = Math.Clamp(skillLevel,  0.0, 1.0);
        Config     = config;
        Rng        = rng;
    }

    public void ApplyDecay()
    {
        Motivation = Math.Max(0.0, Motivation - Config.ActivityDecayRate);
    }

    /// <summary>
    /// Probabilistic state transition each step.
    /// Active → Inactive: P = DeactivationRate × (1 − Motivation)
    /// Inactive → Active: P = ActivationRate   × Motivation
    /// </summary>
    public void ApplyStateTransition()
    {
        if (State == AgentState.Active)
        {
            double p = Config.DeactivationRate * (1.0 - Motivation);
            if (Rng.NextDouble() < p)
                State = AgentState.Inactive;
        }
        else
        {
            double p = Config.ActivationRate * Motivation;
            if (Rng.NextDouble() < p)
                State = AgentState.Active;
        }
    }

    /// <summary>Executes one simulation step. Returns the number of commits made (0 for Contributors).</summary>
    public abstract int Step(PullRequestQueue prQueue, int currentStep);

    protected void ClampMotivation()
    {
        Motivation = Math.Clamp(Motivation, 0.0, 1.0);
    }
}
