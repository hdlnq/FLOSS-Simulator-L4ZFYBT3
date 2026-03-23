# FLOSS Community Survival Simulation

An agent-based simulation framework for quantifying the survival probability of Free/Libre and Open Source Software (FLOSS) communities.

## Overview

This framework models FLOSS community dynamics at daily resolution over multi-year horizons. The model represents two agent roles — **Committers** and **Contributors** — whose participation is governed by a motivation scalar that decays over time and responds to project events (commits, PR merges, rejections, and review delays).

Community survival is defined as the presence of at least one active Committer. Survival probability is estimated using a Kaplan-Meier-style estimator across 10,000 independent replications.

## Repository Structure

```
├── src/
│   ├── Agents/          # Committer and Contributor agent definitions
│   ├── Core/            # SimulationConfig, SimulationEngine, result types
│   ├── Processes/       # PullRequest and PullRequestQueue
│   ├── Analysis/        # Kaplan-Meier survival analysis
│   ├── Output/          # CSV and plot exporters
│   └── Program.cs       # Entry point
├── FigureGen/           # Publication figure generation utility
├── data/                # Experiment outputs (config, aggregated stats, plots)
│   ├── baseline/
│   ├── scenario_a_committers3/
│   ├── scenario_b_lambda02/
│   └── scenario_c_no_promotion/
├── figures/             # Publication-ready figures
└── MyApp.csproj
```

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- NuGet: `ScottPlot` 5.x (restored automatically)

## Running the Simulation

```bash
# Default parameters (baseline scenario)
dotnet run

# Override specific parameters
dotnet run InitialCommitters=3
dotnet run NewContributorLambda=0.2
dotnet run EnablePromotion=false
```

Output is written to a timestamped subdirectory under `results/`:

- `config.json` — recorded parameter values
- `simulation_log.csv` — per-step detail log (large; excluded from git)
- `timeseries_stats.csv` — aggregated per-step statistics
- `survival_curve.png`, `timeseries.png`, `activity.png` — plots

## Generating Publication Figures

```bash
cd FigureGen
dotnet run
```

Requires .NET 10. Output goes to `figures/`.

## Experiments

Four scenarios were evaluated, each with 10,000 runs over 1825 steps (5 years):

| Scenario               | `InitialCommitters` | `NewContributorLambda` | `EnablePromotion` | Survival Rate |
| ---------------------- | ------------------- | ---------------------- | ----------------- | ------------- |
| Baseline               | 1                   | 0.01                   | true              | 22.1%         |
| A: More committers     | **3**               | 0.01                   | true              | **78.0%**     |
| B: Higher arrival rate | 1                   | **0.2**                | true              | 39.9%         |
| C: No promotion        | 1                   | 0.01                   | **false**         | 0.5%          |

Pre-computed outputs (excluding large log files) are in [`data/`](data/).

## Key Parameters

| Parameter              | Default | Description                                       |
| ---------------------- | ------- | ------------------------------------------------- |
| `MaxSteps`             | 1825    | Simulation horizon (days)                         |
| `SimulationRuns`       | 10000   | Number of simulation replications                 |
| `InitialCommitters`    | 1       | Initial committer count                           |
| `ActivityDecayRate`    | 0.01    | Daily motivation decay                            |
| `DeactivationRate`     | 0.10    | Prob. of deactivation when motivation < threshold |
| `MergeAcceptRate`      | 0.70    | PR acceptance probability                         |
| `EnablePromotion`      | true    | Whether contributors can be promoted              |
| `NewContributorLambda` | 0.01    | Poisson arrival rate for new contributors         |

Full parameter list: [`src/Core/SimulationConfig.cs`](src/Core/SimulationConfig.cs)
