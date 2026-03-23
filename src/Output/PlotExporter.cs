using ScottPlot;

namespace SurvivalSim;

public static class PlotExporter
{
    // ── 1. Survival Curve ─────────────────────────────────────────────

    public static void SaveSurvivalCurve(double[] survivalCurve, string filePath)
    {
        var plt    = new Plot();
        double[] steps = Enumerable.Range(1, survivalCurve.Length)
                                   .Select(i => (double)i).ToArray();

        var sc = plt.Add.Scatter(steps, survivalCurve);
        sc.LegendText = "P(alive)";
        plt.Legend.IsVisible = true;
        plt.Title("Project Survival Probability");
        plt.XLabel("Step");
        plt.YLabel("P(alive)");
        plt.Axes.SetLimitsY(0, 1.05);
        plt.SavePng(filePath, 900, 500);
    }

    // ── 2. Activity Chart (commits and PRs per step) ──────────────────

    public static void SaveActivityChart(
        IReadOnlyList<SimulationResult> results,
        int maxSteps,
        string filePath)
    {
        double[] steps       = new double[maxSteps];
        double[] avgCommits  = new double[maxSteps];
        double[] avgPrSub    = new double[maxSteps];
        double[] avgPrMerged = new double[maxSteps];

        var byStep = new Dictionary<int, List<StepRecord>>(maxSteps);
        for (int s = 1; s <= maxSteps; s++)
            byStep[s] = new List<StepRecord>();

        foreach (var result in results)
            foreach (var rec in result.Steps)
                byStep[rec.Step].Add(rec);

        for (int s = 0; s < maxSteps; s++)
        {
            steps[s] = s + 1;
            var recs = byStep[s + 1];
            if (recs.Count > 0)
            {
                avgCommits[s]  = recs.Average(r => r.CommitsThisStep);
                avgPrSub[s]    = recs.Average(r => r.PrSubmittedThisStep);
                avgPrMerged[s] = recs.Average(r => r.PrMergedThisStep);
            }
        }

        var plt = new Plot();
        var sc1 = plt.Add.Scatter(steps, avgCommits);
        sc1.LegendText = "Commits";
        sc1.MarkerSize = 0;
        var sc2 = plt.Add.Scatter(steps, avgPrSub);
        sc2.LegendText = "PRs Submitted";
        sc2.MarkerSize = 0;
        var sc3 = plt.Add.Scatter(steps, avgPrMerged);
        sc3.LegendText = "PRs Merged";
        sc3.MarkerSize = 0;
        plt.Legend.IsVisible = true;
        plt.Title("Mean Commits and Pull Requests per Step");
        plt.XLabel("Step");
        plt.YLabel("Count");
        plt.SavePng(filePath, 900, 500);
    }

    // ── 3. Time-Series (total Committer / Contributor counts per step) ─

    public static void SaveTimeSeries(
        IReadOnlyList<SimulationResult> results,
        int maxSteps,
        string filePath)
    {
        double[] steps          = new double[maxSteps];
        double[] activeComm     = new double[maxSteps];
        double[] inactiveComm   = new double[maxSteps];
        double[] activeContr    = new double[maxSteps];
        double[] inactiveContr  = new double[maxSteps];

        var byStep = new Dictionary<int, List<StepRecord>>(maxSteps);
        for (int s = 1; s <= maxSteps; s++)
            byStep[s] = new List<StepRecord>();

        foreach (var result in results)
            foreach (var rec in result.Steps)
                byStep[rec.Step].Add(rec);

        for (int s = 0; s < maxSteps; s++)
        {
            steps[s] = s + 1;
            var recs = byStep[s + 1];
            if (recs.Count > 0)
            {
                activeComm[s]    = recs.Average(r => r.ActiveCommitters);
                inactiveComm[s]  = recs.Average(r => r.InactiveCommitters);
                activeContr[s]   = recs.Average(r => r.ActiveContributors);
                inactiveContr[s] = recs.Average(r => r.InactiveContributors);
            }
        }

        var plt = new Plot();
        var sc1 = plt.Add.Scatter(steps, activeComm);
        sc1.LegendText = "Committers (active)";
        sc1.MarkerSize = 0;
        var sc2 = plt.Add.Scatter(steps, inactiveComm);
        sc2.LegendText = "Committers (inactive)";
        sc2.MarkerSize = 0;
        var sc3 = plt.Add.Scatter(steps, activeContr);
        sc3.LegendText = "Contributors (active)";
        sc3.MarkerSize = 0;
        var sc4 = plt.Add.Scatter(steps, inactiveContr);
        sc4.LegendText = "Contributors (inactive)";
        sc4.MarkerSize = 0;
        plt.Legend.IsVisible = true;
        plt.Legend.Alignment = ScottPlot.Alignment.UpperLeft;
        plt.Title("Mean Active / Inactive Committer and Contributor Count per Step");
        plt.XLabel("Step");
        plt.YLabel("Count");
        plt.SavePng(filePath, 900, 500);
    }

    // ── 4. Per-agent Motivation History (run 0 only) ──────────────────

    /// <param name="agentType">"Committer" or "Contributor"</param>
    public static void SaveMotivationChart(
        IReadOnlyList<AgentMotivationRecord> history,
        string agentType,
        string filePath)
    {
        var agentIds = history
            .Where(r => r.AgentType == agentType)
            .Select(r => r.AgentId)
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        var plt = new Plot();

        foreach (int id in agentIds)
        {
            var records = history
                .Where(r => r.AgentType == agentType && r.AgentId == id)
                .OrderBy(r => r.Step)
                .ToList();

            double[] steps = records.Select(r => (double)r.Step).ToArray();
            double[] motivs = records.Select(r => r.Motivation).ToArray();

            var sc = plt.Add.Scatter(steps, motivs);
            sc.LegendText = $"{agentType[0]}{id}";
            sc.MarkerSize = 0;
            sc.LineWidth  = 1;
        }

        plt.Legend.IsVisible = true;
        plt.Legend.Alignment = ScottPlot.Alignment.UpperLeft;
        plt.Title($"{agentType} Motivation over Time (run 0)");
        plt.XLabel("Step");
        plt.YLabel("Motivation");
        plt.Axes.SetLimitsY(0, 1.05);
        plt.SavePng(filePath, 900, 500);
    }

}
