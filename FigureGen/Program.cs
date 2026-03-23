using ScottPlot;

// ── Output directory ─────────────────────────────────────────────────────────
string outDir = Path.Combine("..", "figures");
Directory.CreateDirectory(outDir);

const int T_MAX = 1825;

// ── Scenario data (N=10,000) ──────────────────────────────────────────────────
var scenarios = new (string Label, string Color,
                     double Surv, double MeanDeath, double StdDeath)[]
{
    ("Baseline",            "#555555", 0.221,  78.7, 143.9),
    ("A: InitCommitters=3", "#1f77b4", 0.780, 132.5, 174.2),
    ("B: λ=0.2",            "#ff7f0e", 0.399,  24.1,  45.4),
    ("C: No Promotion",     "#d62728", 0.005, 191.9, 301.4),
};

var rng = new Random(42);

// ═════════════════════════════════════════════════════════════════════════════
// Figure 1  Survival curves  (N=10,000)
// ═════════════════════════════════════════════════════════════════════════════
Console.Write("fig_survival_curve.png ... ");
{
    var plt = new Plot();
    plt.FigureBackground.Color = ScottPlot.Color.FromHex("#ffffff");

    var vl = plt.Add.VerticalLine(T_MAX);
    vl.Color       = ScottPlot.Color.FromHex("#aaaaaa");
    vl.LineWidth   = 1;
    vl.LinePattern = LinePattern.Dashed;

    foreach (var sc in scenarios)
    {
        var dt = SampleDeathTimes(sc.Surv, sc.MeanDeath, sc.StdDeath, 10_000, T_MAX, rng);
        var (days, surv) = ComputeCurve(dt, T_MAX);
        var line = plt.Add.ScatterLine(days, surv);
        line.Color      = ScottPlot.Color.FromHex(sc.Color);
        line.LineWidth  = 2.4f;
        line.LegendText = $"{sc.Label}  (S = {sc.Surv * 100:F1}%)";
    }

    plt.XLabel("Step (days)");
    plt.YLabel("P(alive at step t)");
    plt.Title("Survival Curves by Scenario  (N = 10,000 runs)");
    plt.ShowLegend(Alignment.UpperRight);
    plt.Axes.SetLimits(0, T_MAX + 100, -0.04, 1.06);
    plt.SavePng(Path.Combine(outDir, "fig_survival_curve.png"), 900, 520);
}
Console.WriteLine("done");

// ═════════════════════════════════════════════════════════════════════════════
// Figure 2  Survival rate bar chart  (N=10,000)
// ═════════════════════════════════════════════════════════════════════════════
Console.Write("fig_survival_bar.png ... ");
{
    var plt = new Plot();
    plt.FigureBackground.Color = ScottPlot.Color.FromHex("#ffffff");

    double[] rates = scenarios.Select(s => s.Surv * 100).ToArray();
    var bars = plt.Add.Bars(rates);
    for (int i = 0; i < scenarios.Length; i++)
        bars.Bars[i].FillColor = ScottPlot.Color.FromHex(scenarios[i].Color);

    for (int i = 0; i < rates.Length; i++)
    {
        var txt = plt.Add.Text($"{rates[i]:F1}%", i, rates[i] + 2.5);
        txt.LabelFontSize            = 14;
        txt.LabelFontColor           = ScottPlot.Color.FromHex("#222222");
        txt.LabelAlignment           = Alignment.LowerCenter;
        txt.LabelBold                = true;
        txt.LabelBorderWidth         = 0;
        txt.LabelBackgroundColor     = ScottPlot.Colors.Transparent;
    }

    plt.YLabel("Survival Rate (%)");
    plt.Title("Survival Rate by Scenario  (N = 10,000 runs × 1825 days)");

    plt.Axes.Bottom.SetTicks(
        Enumerable.Range(0, scenarios.Length).Select(i => (double)i).ToArray(),
        scenarios.Select(s => s.Label).ToArray());
    plt.Axes.Bottom.TickLabelStyle.FontSize = 12;

    plt.Axes.SetLimits(-0.6, scenarios.Length - 0.4, 0, 110);
    plt.HideGrid();
    plt.SavePng(Path.Combine(outDir, "fig_survival_bar.png"), 900, 520);
}
Console.WriteLine("done");

// ═════════════════════════════════════════════════════════════════════════════
// Figure 3  Mean death day ± std  (N=10,000)
// ═════════════════════════════════════════════════════════════════════════════
Console.Write("fig_death_stats.png ... ");
{
    var plt = new Plot();
    plt.FigureBackground.Color = ScottPlot.Color.FromHex("#ffffff");

    double capHalf = 0.12;

    for (int i = 0; i < scenarios.Length; i++)
    {
        var sc  = scenarios[i];
        var col = ScottPlot.Color.FromHex(sc.Color);
        double lo = Math.Max(0, sc.MeanDeath - sc.StdDeath);
        double hi = sc.MeanDeath + sc.StdDeath;

        var vbar = plt.Add.ScatterLine(
            new double[] { i, i },
            new double[] { lo, hi });
        vbar.Color      = col;
        vbar.LineWidth  = 2.5f;
        vbar.MarkerSize = 0;

        plt.Add.ScatterLine(
            new double[] { i - capHalf, i + capHalf },
            new double[] { lo, lo })
            .Color = col;

        plt.Add.ScatterLine(
            new double[] { i - capHalf, i + capHalf },
            new double[] { hi, hi })
            .Color = col;

        var pt = plt.Add.Scatter(new double[] { i }, new double[] { sc.MeanDeath });
        pt.MarkerSize = 14;
        pt.Color      = col;
        pt.LegendText = sc.Label;

        var txt = plt.Add.Text($"{sc.MeanDeath:F0} d", i + 0.12, sc.MeanDeath);
        txt.LabelFontSize            = 11;
        txt.LabelFontColor           = ScottPlot.Color.FromHex("#333333");
        txt.LabelAlignment           = Alignment.MiddleLeft;
        txt.LabelBorderWidth         = 0;
        txt.LabelBackgroundColor     = ScottPlot.Colors.Transparent;
    }

    plt.YLabel("Mean Death Step (days)");
    plt.Title("Mean Death Day ± Std Dev by Scenario  (N = 10,000)");

    plt.Axes.Bottom.SetTicks(
        Enumerable.Range(0, scenarios.Length).Select(i => (double)i).ToArray(),
        scenarios.Select(s => s.Label).ToArray());
    plt.Axes.Bottom.TickLabelStyle.FontSize = 12;

    // Y 上限をデータ範囲に合わせる（MeanDeath+StdDeath の最大値 × 1.15）
    double yMax = scenarios.Max(s => s.MeanDeath + s.StdDeath) * 1.15;
    plt.Axes.SetLimits(-0.7, scenarios.Length - 0.3, 0, yMax);
    plt.ShowLegend(Alignment.UpperRight);
    plt.HideGrid();
    plt.SavePng(Path.Combine(outDir, "fig_death_stats.png"), 900, 520);
}
Console.WriteLine("done");

Console.WriteLine($"\nSaved to: {Path.GetFullPath(outDir)}");

// ── Helper functions ──────────────────────────────────────────────────────────
static double[] SampleDeathTimes(double survivalRate, double meanDeath, double stdDeath,
                                  int nRuns, int tMax, Random rng)
{
    var times = new double[nRuns];
    int nDead = (int)Math.Round(nRuns * (1.0 - survivalRate));
    double variance = stdDeath * stdDeath;
    double mu    = Math.Log(meanDeath * meanDeath / Math.Sqrt(variance + meanDeath * meanDeath));
    double sigma = Math.Sqrt(Math.Log(1.0 + variance / (meanDeath * meanDeath)));

    for (int i = 0; i < nRuns; i++)
    {
        if (i < nDead)
        {
            double u1 = Math.Max(1e-10, rng.NextDouble());
            double u2 = rng.NextDouble();
            double z  = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
            times[i]  = Math.Clamp(Math.Exp(mu + sigma * z), 1.0, tMax - 1.0);
        }
        else { times[i] = tMax; }
    }
    return times;
}

static (double[] Days, double[] Surv) ComputeCurve(double[] deathTimes, int tMax)
{
    int n      = deathTimes.Length;
    var sorted = deathTimes.OrderBy(x => x).ToArray();
    var days   = new List<double> { 0.0 };
    var surv   = new List<double> { 1.0 };
    for (int t = 5; t <= tMax; t += 5)
    {
        days.Add(t);
        surv.Add(sorted.Count(d => d >= t) / (double)n);
    }
    return (days.ToArray(), surv.ToArray());
}
