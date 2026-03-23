# Experiment Data

Pre-computed outputs from four simulation scenarios (10,000 runs × 1825 steps each).

> **Note:** `simulation_log.csv` (per-step detail logs, 60–500 MB each) are excluded from this repository. Regenerate them by running `dotnet run` with the corresponding parameters.

## Directory Structure

```
data/
├── baseline/
│   ├── run1/    # RandomSeed: 1902601876
│   └── run2/    # RandomSeed: 1274746242  (independent replication)
├── scenario_a_committers3/
│   └── run1/    # RandomSeed: 1888083015
├── scenario_b_lambda02/
│   └── run1/    # RandomSeed: 543393002
└── scenario_c_no_promotion/
    └── run1/    # RandomSeed: 1695735350
```

## Scenarios

| Directory | Changed Parameter | Value | Survival Rate |
|---|---|---|---|
| `baseline/` | — (default) | InitialCommitters=1, λ=0.01, Promotion=true | 22.1% |
| `scenario_a_committers3/` | `InitialCommitters` | **3** | 78.0% |
| `scenario_b_lambda02/` | `NewContributorLambda` | **0.2** | 39.9% |
| `scenario_c_no_promotion/` | `EnablePromotion` | **false** | 0.5% |

## Files per Run

| File | Description |
|---|---|
| `config.json` | Full parameter record (enables reproducibility) |
| `timeseries_stats.csv` | Per-step aggregated statistics (Step, AliveRuns, AvgCommitters, AvgContributors, ...) |
| `survival_curve.png` | Kaplan-Meier survival curve |
| `timeseries.png` | Mean agent counts over time |
| `activity.png` | Commit and PR activity over time |
| `motivation_committers.png` | Per-agent motivation trajectory (run 0 only) |
| `motivation_contributors.png` | Per-agent motivation trajectory (run 0 only) |

## Reproducing a Run

```bash
# Example: reproduce scenario_a_committers3/run1
dotnet run InitialCommitters=3 RandomSeed=1888083015
```
