using System.Text.Json;
using SurvivalSim;

var config = new SimulationConfig();

// Parse key=value command-line overrides (e.g. InitialCommitters=3)
foreach (var arg in args)
{
    var sep = arg.IndexOf('=');
    if (sep < 0) continue;
    var key = arg[..sep];
    var val = arg[(sep + 1)..];
    config = key switch
    {
        "InitialCommitters"    => config with { InitialCommitters    = int.Parse(val) },
        "NewContributorLambda" => config with { NewContributorLambda = double.Parse(val) },
        "EnablePromotion"      => config with { EnablePromotion      = bool.Parse(val) },
        "SimulationRuns"       => config with { SimulationRuns       = int.Parse(val) },
        _                      => config
    };
}

// Resolve seed: if null, generate one so config.json captures the actual seed used
if (!config.RandomSeed.HasValue)
    config = config with { RandomSeed = Random.Shared.Next() };

string runDir = Path.Combine(
    config.OutputDirectory,
    DateTime.Now.ToString("yyyyMM-ddHH-mmss"));
Directory.CreateDirectory(runDir);

Console.WriteLine("=== OSS Community Survival Simulation ===");
Console.WriteLine($"Runs: {config.SimulationRuns}  Steps: {config.MaxSteps}");
Console.WriteLine($"Initial Committers: {config.InitialCommitters}  " +
                  $"Contributors: {config.InitialContributors}");
Console.WriteLine();

// ── 1. Monte Carlo ──────────────────────────────────────────────────
Console.WriteLine("Running Monte Carlo simulation...");
var engine  = new SimulationEngine(config);
var results = new List<SimulationResult>(config.SimulationRuns);

for (int i = 0; i < config.SimulationRuns; i++)
{
    results.Add(engine.RunOnce(i));
    if ((i + 1) % 100 == 0)
        Console.WriteLine($"  {i + 1}/{config.SimulationRuns} runs completed");
}

double survivalRate = results.Count(r => r.Survived) * 100.0 / results.Count;
var (meanDeath, stdDeath) = SurvivalAnalyzer.DeathStepStats(results);

Console.WriteLine();
Console.WriteLine($"Survival rate (all {config.MaxSteps} steps): {survivalRate:F1}%");
if (!double.IsNaN(meanDeath))
    Console.WriteLine($"Mean death step (non-survivors): {meanDeath:F1} ± {stdDeath:F1}");
Console.WriteLine();

// ── 2. Config export ────────────────────────────────────────────────
Console.Write("Writing config... ");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(runDir, "config.json"),
    JsonSerializer.Serialize(config, jsonOptions));
Console.WriteLine("done");

// ── 3. Survival curve ───────────────────────────────────────────────
Console.Write("Generating survival curve... ");
var curve = SurvivalAnalyzer.ComputeSurvivalCurve(results, config.MaxSteps);
PlotExporter.SaveSurvivalCurve(
    curve,
    Path.Combine(runDir, "survival_curve.png"));
Console.WriteLine("done");

// ── 4. Activity chart ────────────────────────────────────────────────
Console.Write("Generating activity chart... ");
PlotExporter.SaveActivityChart(
    results, config.MaxSteps,
    Path.Combine(runDir, "activity.png"));
Console.WriteLine("done");

// ── 5. Time series ──────────────────────────────────────────────────
Console.Write("Generating time series graph... ");
PlotExporter.SaveTimeSeries(
    results, config.MaxSteps,
    Path.Combine(runDir, "timeseries.png"));
Console.WriteLine("done");

// ── 6. Per-agent motivation charts (run 0) ──────────────────────────
var motivHistory = results[0].MotivationHistory!;
Console.Write("Generating motivation charts... ");
PlotExporter.SaveMotivationChart(
    motivHistory, "Committer",
    Path.Combine(runDir, "motivation_committers.png"));
PlotExporter.SaveMotivationChart(
    motivHistory, "Contributor",
    Path.Combine(runDir, "motivation_contributors.png"));
Console.WriteLine("done");

// ── 7. CSV log ──────────────────────────────────────────────────────
Console.Write("Writing simulation log CSV... ");
var allSteps = results.SelectMany(r => r.Steps);
CsvExporter.WriteStepRecords(
    allSteps,
    Path.Combine(runDir, "simulation_log.csv"));
Console.WriteLine("done");

Console.Write("Writing time-series stats CSV... ");
CsvExporter.WriteTimeSeriesStats(
    results, config.MaxSteps,
    Path.Combine(runDir, "timeseries_stats.csv"));
Console.WriteLine("done");

Console.WriteLine();
Console.WriteLine("Output files:");
foreach (var file in Directory.GetFiles(runDir))
    Console.WriteLine($"  {file}");

Console.WriteLine();
Console.WriteLine($"Done. Output directory: {Path.GetFullPath(runDir)}");
