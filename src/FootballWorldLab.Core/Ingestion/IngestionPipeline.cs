using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FootballWorldLab.Core.Ingestion;

namespace FootballWorldLab.Core.Ingestion
{
    public sealed class IngestionPipeline
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public static SourceManifest LoadManifest(string manifestPath)
        {
            if (!File.Exists(manifestPath))
            {
                throw new FileNotFoundException($"Manifest file not found at {manifestPath}", manifestPath);
            }

            string json = File.ReadAllText(manifestPath);
            return JsonSerializer.Deserialize<SourceManifest>(json, JsonOptions) ?? new SourceManifest();
        }

        public static List<IngestionResult> RunIngestion(SourceManifest manifest, string outputDir, string? reportPath = null)
        {
            var results = new List<IngestionResult>();
            var aliasNormalizer = new AliasNormalizer(manifest.DefaultAliases);
            var parser = new OpenFootballParser();
            var validator = new MatchValidator();

            string rawCacheDir = Path.Combine(outputDir, "raw");
            if (!Directory.Exists(rawCacheDir))
            {
                Directory.CreateDirectory(rawCacheDir);
            }

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            foreach (var env in manifest.Environments)
            {
                var envResult = new IngestionResult { EnvironmentId = env.EnvironmentId };

                if (env.Sources.Count == 0)
                {
                    envResult.ObservedGaps.Add($"No openfootball sources defined for environment {env.EnvironmentId}");
                }

                var allRawRecords = new List<RawMatchRecord>();
                var allCanonicalRecords = new List<CanonicalMatchRecord>();

                foreach (var source in env.Sources)
                {
                    string fileName = $"{source.SourceId}.txt";
                    string rawPath = Path.Combine(rawCacheDir, fileName);
                    string content = string.Empty;

                    // Check cache or download
                    if (File.Exists(rawPath))
                    {
                        content = File.ReadAllText(rawPath);
                    }
                    else if (!string.IsNullOrWhiteSpace(source.Url))
                    {
                        try
                        {
                            var response = httpClient.GetAsync(source.Url).GetAwaiter().GetResult();
                            if (response.IsSuccessStatusCode)
                            {
                                content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                                File.WriteAllText(rawPath, content);
                            }
                            else
                            {
                                envResult.ObservedGaps.Add($"Failed to download source {source.SourceId}: HTTP {response.StatusCode}");
                            }
                        }
                        catch (Exception ex)
                        {
                            envResult.ObservedGaps.Add($"Exception downloading source {source.SourceId}: {ex.Message}");
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        // Verify checksum if expected
                        if (!string.IsNullOrWhiteSpace(source.Sha256))
                        {
                            using var sha256 = SHA256.Create();
                            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
                            string actualHash = Convert.ToHexString(hashBytes).ToLowerInvariant();
                            if (!actualHash.Equals(source.Sha256.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                envResult.ValidationIssues.Add(new ValidationIssue
                                {
                                    Category = ValidationIssueCategory.InvalidDate,
                                    Severity = ValidationIssueSeverity.Error,
                                    Message = $"Checksum mismatch for source {source.SourceId}. Expected {source.Sha256}, got {actualHash}",
                                    EnvironmentId = env.EnvironmentId
                                });
                            }
                        }

                        var rawRecords = parser.ParseContent(content, env.EnvironmentId, source.SourceId);
                        allRawRecords.AddRange(rawRecords);

                        foreach (var raw in rawRecords)
                        {
                            var canonical = parser.ConvertToCanonical(raw, aliasNormalizer, env.CompetitionName);
                            allCanonicalRecords.Add(canonical);
                        }
                    }
                }

                envResult.TotalRawRecords = allRawRecords.Count;
                var issues = validator.ValidateRecords(allRawRecords, allCanonicalRecords, aliasNormalizer, env.EnvironmentId);
                envResult.ValidationIssues.AddRange(issues);

                // Valid matches exclude unresolved clubs / errors
                envResult.ValidMatches = allCanonicalRecords
                    .Where(c => c.HomeClubId.HasValue && c.AwayClubId.HasValue &&
                                c.HomeGoals >= 0 && c.HomeGoals <= MatchValidator.MaxReasonableGoals &&
                                c.AwayGoals >= 0 && c.AwayGoals <= MatchValidator.MaxReasonableGoals)
                    .ToList();

                results.Add(envResult);
            }

            // Save canonical outputs
            string canonicalFile = Path.Combine(outputDir, "canonical_matches.json");
            File.WriteAllText(canonicalFile, JsonSerializer.Serialize(results, JsonOptions));

            // Generate coverage report
            if (!string.IsNullOrEmpty(reportPath))
            {
                var reportGen = new CoverageReportGenerator();
                reportGen.SaveReportToFile(results, reportPath);
            }

            return results;
        }
    }
}
