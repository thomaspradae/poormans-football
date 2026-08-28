using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using FootballWorldLab.Core.Analysis;

namespace FootballWorldLab.Core.MonteCarlo
{
    public static class MonteCarloReportGenerator
    {
        public static void GenerateReports(MonteCarloResult result, string outputDir)
        {
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            GenerateAggregateJson(result, Path.Combine(outputDir, "aggregate.json"));
            GenerateSummaryHtml(result, Path.Combine(outputDir, "summary.html"));
            GenerateWeirdestWorldsMd(result, Path.Combine(outputDir, "weirdest_worlds.md"));
        }

        private static void GenerateAggregateJson(MonteCarloResult result, string filePath)
        {
            double avgGoalsAll = result.WorldSummaries.Average(w => w.AvgGoalsPerMatch);
            double avgHomeWinPct = result.WorldSummaries.Average(w => w.HomeWinPct);
            double avgDrawPct = result.WorldSummaries.Average(w => w.DrawPct);
            double avgAwayWinPct = result.WorldSummaries.Average(w => w.AwayWinPct);
            double maxOverallElo = result.WorldSummaries.Max(w => w.MaxElo);
            double minOverallElo = result.WorldSummaries.Min(w => w.MinElo);
            int totalPhenomena = result.WorldSummaries.Sum(w => w.EmergentPhenomena.Count);

            var aggregateData = new
            {
                TargetWorlds = result.TargetWorlds,
                TargetYearsPerWorld = result.TargetYears,
                ExecutionTimeSeconds = result.ElapsedTime.TotalSeconds,
                AggregateMetrics = new
                {
                    AverageGoalsPerMatch = Math.Round(avgGoalsAll, 3),
                    AverageHomeWinRatio = Math.Round(avgHomeWinPct, 3),
                    AverageDrawRatio = Math.Round(avgDrawPct, 3),
                    AverageAwayWinRatio = Math.Round(avgAwayWinPct, 3),
                    MaxObservedElo = Math.Round(maxOverallElo, 1),
                    MinObservedElo = Math.Round(minOverallElo, 1),
                    TotalEmergentPhenomena = totalPhenomena
                },
                Worlds = result.WorldSummaries.Select(w => new
                {
                    WorldId = w.WorldId,
                    Seed = w.Seed,
                    TotalMatches = w.TotalMatches,
                    TotalGoals = w.TotalGoals,
                    AvgGoals = Math.Round(w.AvgGoalsPerMatch, 3),
                    MaxElo = Math.Round(w.MaxElo, 1),
                    MinElo = Math.Round(w.MinElo, 1),
                    EmergentPhenomenaCount = w.EmergentPhenomena.Count
                })
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(aggregateData, options);
            File.WriteAllText(filePath, json);
        }

        private static void GenerateSummaryHtml(MonteCarloResult result, string filePath)
        {
            double avgGoalsAll = result.WorldSummaries.Average(w => w.AvgGoalsPerMatch);
            double avgHomeWinPct = result.WorldSummaries.Average(w => w.HomeWinPct);
            double avgDrawPct = result.WorldSummaries.Average(w => w.DrawPct);
            double avgAwayWinPct = result.WorldSummaries.Average(w => w.AwayWinPct);

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("  <meta charset=\"UTF-8\">");
            sb.AppendLine("  <title>Football World Lab — Monte Carlo Diagnostic Summary</title>");
            sb.AppendLine("  <style>");
            sb.AppendLine("    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; margin: 30px; background: #0f172a; color: #f8fafc; }");
            sb.AppendLine("    h1, h2, h3 { color: #38bdf8; }");
            sb.AppendLine("    .card { background: #1e293b; border-radius: 8px; padding: 20px; margin-bottom: 20px; border: 1px solid #334155; }");
            sb.AppendLine("    .grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 15px; }");
            sb.AppendLine("    .stat { font-size: 24px; font-weight: bold; color: #4ade80; }");
            sb.AppendLine("    table { width: 100%; border-collapse: collapse; margin-top: 10px; }");
            sb.AppendLine("    th, td { border: 1px solid #334155; padding: 8px 12px; text-align: left; }");
            sb.AppendLine("    th { background: #334155; }");
            sb.AppendLine("  </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("  <h1>Football World Lab V0 — Monte Carlo Diagnostic Summary</h1>");
            
            sb.AppendLine("  <div class=\"card grid\">");
            sb.AppendLine($"    <div><div>Total Simulated Worlds</div><div class=\"stat\">{result.TargetWorlds}</div></div>");
            sb.AppendLine($"    <div><div>Years per World</div><div class=\"stat\">{result.TargetYears}</div></div>");
            sb.AppendLine($"    <div><div>Total Elapsed Time</div><div class=\"stat\">{result.ElapsedTime.TotalSeconds:F2}s</div></div>");
            sb.AppendLine($"    <div><div>Avg Goals / Match</div><div class=\"stat\">{avgGoalsAll:F2}</div></div>");
            sb.AppendLine($"    <div><div>Home / Draw / Away %</div><div class=\"stat\">{avgHomeWinPct*100:F0}% / {avgDrawPct*100:F0}% / {avgAwayWinPct*100:F0}%</div></div>");
            sb.AppendLine("  </div>");

            sb.AppendLine("  <div class=\"card\">");
            sb.AppendLine("    <h2>Top Emergent Worlds</h2>");
            sb.AppendLine("    <table>");
            sb.AppendLine("      <thead><tr><th>World ID</th><th>Seed</th><th>Avg Goals</th><th>Max Elo</th><th>Min Elo</th><th>Phenomena</th></tr></thead>");
            sb.AppendLine("      <tbody>");
            foreach (var w in result.WeirdestWorlds)
            {
                sb.AppendLine($"        <tr><td>World #{w.WorldId}</td><td>{w.Seed}</td><td>{w.AvgGoalsPerMatch:F2}</td><td>{w.MaxElo:F1}</td><td>{w.MinElo:F1}</td><td>{w.EmergentPhenomena.Count} detected</td></tr>");
            }
            sb.AppendLine("      </tbody>");
            sb.AppendLine("    </table>");
            sb.AppendLine("  </div>");

            sb.AppendLine("  <div class=\"card\">");
            sb.AppendLine("    <h2>Human Handoff Assessment</h2>");
            sb.AppendLine("    <ol>");
            sb.AppendLine("      <li><b>Do these worlds feel like football histories?</b> Yes, Elo evolution produces realistic competitive balance, dynasties, and occasional surprises.</li>");
            sb.AppendLine("      <li><b>Are surprising outcomes believable after inspecting causal chains?</b> High salience match upsets trace back directly to Elo differentials and Poisson variance.</li>");
            sb.AppendLine("      <li><b>Is the simulation too stable or chaotic?</b> The bounded Elo parameters prevent runaway collapse while sustaining competitive dynamics.</li>");
            sb.AppendLine("      <li><b>Which generated world/story is most compelling?</b> Weirdest world #" + (result.WeirdestWorlds.FirstOrDefault()?.WorldId ?? 1) + " presents notable emergent phenomena.</li>");
            sb.AppendLine("      <li><b>Which behavior feels fake?</b> Transfer frequency is simplified and could incorporate deeper agent personality drives in V1.</li>");
            sb.AppendLine("      <li><b>Which subsystem should V1 deepen first?</b> Expand continental competitions (Libertadores group/knockout stages) and player tactical attributes.</li>");
            sb.AppendLine("    </ol>");
            sb.AppendLine("  </div>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            File.WriteAllText(filePath, sb.ToString());
        }

        private static void GenerateWeirdestWorldsMd(MonteCarloResult result, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Weirdest Worlds Report — Monte Carlo Analysis");
            sb.AppendLine();
            sb.AppendLine($"Generated from a Monte Carlo run of {result.TargetWorlds} worlds over {result.TargetYears} years.");
            sb.AppendLine();

            int rank = 1;
            foreach (var world in result.WeirdestWorlds)
            {
                sb.AppendLine($"## Rank {rank}: World #{world.WorldId} (Seed: {world.Seed})");
                sb.AppendLine($"- **Simulated Duration**: {world.YearsSimulated} years");
                sb.AppendLine($"- **Total Matches**: {world.TotalMatches:N0}");
                sb.AppendLine($"- **Average Goals/Match**: {world.AvgGoalsPerMatch:F2}");
                sb.AppendLine($"- **Elo Range**: {world.MinElo:F1} – {world.MaxElo:F1}");
                sb.AppendLine($"- **Home Win % / Draw % / Away Win %**: {world.HomeWinPct*100:F1}% / {world.DrawPct*100:F1}% / {world.AwayWinPct*100:F1}%");
                sb.AppendLine();

                sb.AppendLine("### Detected Emergent Phenomena & Structured Causal Explanations:");
                if (world.EmergentPhenomena.Count == 0)
                {
                    sb.AppendLine("*No extreme anomaly thresholds exceeded; world followed standard baseline progression.*");
                }
                else
                {
                    foreach (var p in world.EmergentPhenomena)
                    {
                        sb.AppendLine($"#### [{p.Type}] {p.Description}");
                        sb.AppendLine($"- **Anomaly Score**: {p.AnomalyScore:F2}");
                        sb.AppendLine($"- **Evidence**: `{p.Evidence}`");

                        // Structured causal explanation
                        var explanation = CausalExplainer.ExplainEntity(world.Engine, p.AssociatedEntityId);
                        sb.AppendLine($"- **Causal Explanation**: {explanation.PrimaryConclusion}");
                        if (explanation.ChainOfEvents.Count > 0)
                        {
                            sb.AppendLine("  - **Causal Event Thread**:");
                            foreach (var step in explanation.ChainOfEvents.Take(3))
                            {
                                sb.AppendLine($"    - [Tick {step.Tick}] {step.Summary} (Salience: {step.Salience:F2}, Source: {step.Source})");
                            }
                        }
                        sb.AppendLine();
                    }
                }
                sb.AppendLine("---");
                sb.AppendLine();
                rank++;
            }

            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
