namespace SurvivalSim;

public static class SurvivalAnalyzer
{
    /// <summary>
    /// Returns double[step-1] = P(project alive at that step).
    /// Index 0 corresponds to step 1, length = maxSteps.
    /// </summary>
    public static double[] ComputeSurvivalCurve(
        IReadOnlyList<SimulationResult> results,
        int maxSteps)
    {
        int n     = results.Count;
        var curve = new double[maxSteps];

        for (int step = 1; step <= maxSteps; step++)
        {
            int alive = results.Count(r => r.Survived || r.DeathStep > step);
            curve[step - 1] = (double)alive / n;
        }

        return curve;
    }

    /// <summary>
    /// Returns (mean, stdDev) of the death step across runs that died.
    /// Returns (NaN, NaN) if every run survived.
    /// </summary>
    public static (double Mean, double StdDev) DeathStepStats(
        IReadOnlyList<SimulationResult> results)
    {
        var deaths = results
            .Where(r => !r.Survived)
            .Select(r => (double)r.DeathStep)
            .ToList();

        if (deaths.Count == 0) return (double.NaN, double.NaN);

        double mean     = deaths.Average();
        double variance = deaths.Select(d => Math.Pow(d - mean, 2)).Average();
        return (mean, Math.Sqrt(variance));
    }
}
