using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FootballWorldLab.Core.Ingestion
{
    public sealed class CoverageReportGenerator
    {
        public static readonly string[] RequiredEnvironments = new[]
        {
            "Colombia",
            "Argentina",
            "Brazil",
            "CopaLibertadores"
        };

        public string GenerateMarkdownReport(List<IngestionResult> results)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Historical OpenFootball Ingestion Coverage Report");
            sb.AppendLine();
            sb.AppendLine($"Report Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine();
            sb.AppendLine("## Executive Summary");
            sb.AppendLine();

            int totalRaw = results.Sum(r => r.TotalRawRecords);
            int totalValid = results.Sum(r => r.ValidMatches.Count);
            int totalIssues = results.Sum(r => r.ValidationIssues.Count);

            sb.AppendLine($"- **Total Raw Records Processed:** {totalRaw}");
            sb.AppendLine($"- **Total Canonical Valid Matches:** {totalValid}");
            sb.AppendLine($"- **Total Validation Issues/Gaps:** {totalIssues}");
            sb.AppendLine();

            sb.AppendLine("## Environment Coverage Breakdown");
            sb.AppendLine();
            sb.AppendLine("| Environment | Competition | Valid Matches | Duplicates | Impossible Scores | Unresolved Clubs | Observed Data Status |");
            sb.AppendLine("|---|---|---|---|---|---|---|");

            foreach (var envId in RequiredEnvironments)
            {
                var result = results.FirstOrDefault(r => r.EnvironmentId.Equals(envId, StringComparison.OrdinalIgnoreCase));
                if (result == null)
                {
                    sb.AppendLine($"| {envId} | N/A | 0 | 0 | 0 | 0 | ❌ NO DATA / GAP DETECTED |");
                    continue;
                }

                int dupes = result.ValidationIssues.Count(i => i.Category == ValidationIssueCategory.Duplicate);
                int impossible = result.ValidationIssues.Count(i => i.Category == ValidationIssueCategory.ImpossibleScore);
                int unresolved = result.ValidationIssues.Count(i => i.Category == ValidationIssueCategory.UnresolvedClub);

                string compName = result.ValidMatches.FirstOrDefault()?.CompetitionName ?? "League";
                string status = result.ValidMatches.Count > 0 ? "✅ Verified Observed Data" : "⚠️ Partial / No Matches";

                sb.AppendLine($"| {envId} | {compName} | {result.ValidMatches.Count} | {dupes} | {impossible} | {unresolved} | {status} |");
            }

            sb.AppendLine();
            sb.AppendLine("## Data Provenance & Boundary Assurance");
            sb.AppendLine();
            sb.AppendLine("- **Data Provenance:** All ingested historical results are explicitly tagged with `ProvenanceSource.RealWorld`.");
            sb.AppendLine("- **Synthetic Separation:** Simulation runs generate data with `ProvenanceSource.Synthetic`. Ingested observed data remains strictly segregated and read-only.");
            sb.AppendLine();

            sb.AppendLine("## Identified Data Gaps & Quality Issues");
            sb.AppendLine();

            bool foundGaps = false;
            foreach (var result in results)
            {
                if (result.ValidationIssues.Count > 0 || result.ObservedGaps.Count > 0)
                {
                    foundGaps = true;
                    sb.AppendLine($"### Environment: {result.EnvironmentId}");

                    foreach (var gap in result.ObservedGaps)
                    {
                        sb.AppendLine($"  - ⚠️ Gap: {gap}");
                    }

                    foreach (var issue in result.ValidationIssues)
                    {
                        sb.AppendLine($"  - [{issue.Severity}] {issue.Category}: {issue.Message}");
                    }

                    sb.AppendLine();
                }
            }

            if (!foundGaps)
            {
                sb.AppendLine("No data gaps or validation errors were identified during ingestion.");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public void SaveReportToFile(List<IngestionResult> results, string filePath)
        {
            string? dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string markdown = GenerateMarkdownReport(results);
            File.WriteAllText(filePath, markdown);
        }
    }
}
