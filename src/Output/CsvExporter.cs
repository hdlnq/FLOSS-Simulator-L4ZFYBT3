namespace SurvivalSim;

public static class CsvExporter
{
    public static void WriteStepRecords(IEnumerable<StepRecord> records, string filePath)
    {
        using var writer = new StreamWriter(filePath);
        writer.WriteLine(
            "Run,Step,ActiveCommitters,InactiveCommitters," +
            "ActiveContributors,InactiveContributors," +
            "PendingPRs,PromotionsThisStep,NewArrivalsThisStep," +
            "CommitsThisStep,PrSubmittedThisStep,PrMergedThisStep,IsAlive");

        foreach (var r in records)
        {
            writer.WriteLine(
                $"{r.Run},{r.Step}," +
                $"{r.ActiveCommitters},{r.InactiveCommitters}," +
                $"{r.ActiveContributors},{r.InactiveContributors}," +
                $"{r.PendingPRs},{r.PromotionsThisStep},{r.NewArrivalsThisStep}," +
                $"{r.CommitsThisStep},{r.PrSubmittedThisStep},{r.PrMergedThisStep},{r.IsAlive}");
        }
    }

    public static void WriteTimeSeriesStats(
        IReadOnlyList<SimulationResult> results,
        int maxSteps,
        string filePath)
    {
        var byStep = new Dictionary<int, List<StepRecord>>(maxSteps);
        for (int s = 1; s <= maxSteps; s++)
            byStep[s] = new List<StepRecord>();

        foreach (var result in results)
            foreach (var rec in result.Steps)
                byStep[rec.Step].Add(rec);

        using var writer = new StreamWriter(filePath);
        writer.WriteLine("Step,AliveRuns,AvgCommitters,AvgContributors");

        for (int s = 1; s <= maxSteps; s++)
        {
            var recs = byStep[s];
            if (recs.Count == 0) break;

            writer.WriteLine(
                $"{s},{recs.Count}," +
                $"{recs.Average(r => r.ActiveCommitters + r.InactiveCommitters):F2}," +
                $"{recs.Average(r => r.ActiveContributors + r.InactiveContributors):F2}");
        }
    }


}
