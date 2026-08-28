using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FootballWorldLab.Core.Ids;
using FootballWorldLab.Core.Ingestion;
using FootballWorldLab.Core.Provenance;
using Xunit;

namespace FootballWorldLab.Core.Tests
{
    public class IngestionTests
    {
        [Fact]
        public void Manifest_LoadsCorrectly_WithAllFourEnvironments()
        {
            string manifestPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../../data/openfootball_manifest.json");
            if (!File.Exists(manifestPath))
            {
                manifestPath = Path.Combine(Directory.GetCurrentDirectory(), "data/openfootball_manifest.json");
            }

            Assert.True(File.Exists(manifestPath), $"Manifest not found at {manifestPath}");

            var manifest = IngestionPipeline.LoadManifest(manifestPath);

            Assert.NotNull(manifest);
            Assert.Equal("MIT", manifest.License);
            Assert.Equal(4, manifest.Environments.Count);

            var envIds = manifest.Environments.Select(e => e.EnvironmentId).ToList();
            Assert.Contains("Colombia", envIds);
            Assert.Contains("Argentina", envIds);
            Assert.Contains("Brazil", envIds);
            Assert.Contains("CopaLibertadores", envIds);
            Assert.NotEmpty(manifest.DefaultAliases);
        }

        [Fact]
        public void AliasNormalizer_ResolvesAliasesAndCleanNames()
        {
            var customAliases = new Dictionary<string, string>
            {
                { "A. Nacional", "Atlético Nacional" },
                { "CA Boca Juniors", "Boca Juniors" }
            };

            var normalizer = new AliasNormalizer(customAliases);

            Assert.True(normalizer.TryResolve("A. Nacional", out string name1, out StableId id1));
            Assert.Equal("Atlético Nacional", name1);
            Assert.Equal("Club:ATLÉTICONACIONAL", id1.Value);

            Assert.True(normalizer.TryResolve("CA Boca Juniors", out string name2, out StableId id2));
            Assert.Equal("Boca Juniors", name2);
            Assert.Equal("Club:BOCAJUNIORS", id2.Value);

            // Stripping heuristics test
            Assert.True(normalizer.TryResolve("FC Millonarios", out string name3, out StableId id3));
            Assert.Equal("Millonarios", name3);
            Assert.Equal("Club:MILLONARIOS", id3.Value);
        }

        [Fact]
        public void OpenFootballParser_ParsesResultsAndTagsObservedProvenance()
        {
            string sampleContent = @"
= 2023
[Fri Aug/18]
  Boca Juniors  2-1  River Plate
  Millonarios FC  3-0  Santa Fe
";
            var parser = new OpenFootballParser();
            var rawRecords = parser.ParseContent(sampleContent, "Argentina", "test-source");

            Assert.Equal(2, rawRecords.Count);
            Assert.Equal("Boca Juniors", rawRecords[0].HomeTeamRaw);
            Assert.Equal("River Plate", rawRecords[0].AwayTeamRaw);

            var normalizer = new AliasNormalizer();
            var canonical = parser.ConvertToCanonical(rawRecords[0], normalizer, "Primera División", 2023);

            Assert.NotNull(canonical);
            Assert.True(canonical.IsObserved);
            Assert.Equal(ProvenanceSource.RealWorld, canonical.Provenance.Source);
            Assert.Equal("Argentina", canonical.EnvironmentId);
            Assert.Equal(2, canonical.HomeGoals);
            Assert.Equal(1, canonical.AwayGoals);
        }

        [Fact]
        public void MatchValidator_DetectsDuplicates_ImpossibleScores_AndUnresolvedClubs()
        {
            var normalizer = new AliasNormalizer();
            var validator = new MatchValidator();

            var raw1 = new RawMatchRecord { EnvironmentId = "Colombia", HomeTeamRaw = "Millonarios", AwayTeamRaw = "Santa Fe", HomeGoalsRaw = "2", AwayGoalsRaw = "1" };
            var raw2 = new RawMatchRecord { EnvironmentId = "Colombia", HomeTeamRaw = "Millonarios", AwayTeamRaw = "Santa Fe", HomeGoalsRaw = "2", AwayGoalsRaw = "1" }; // Duplicate
            var raw3 = new RawMatchRecord { EnvironmentId = "Colombia", HomeTeamRaw = "Nacional", AwayTeamRaw = "Junior", HomeGoalsRaw = "-1", AwayGoalsRaw = "45" }; // Impossible score
            var raw4 = new RawMatchRecord { EnvironmentId = "Colombia", HomeTeamRaw = "", AwayTeamRaw = "Calí", HomeGoalsRaw = "1", AwayGoalsRaw = "0" }; // Unresolved club

            var parser = new OpenFootballParser();
            var canonicals = new List<CanonicalMatchRecord>
            {
                parser.ConvertToCanonical(raw1, normalizer, "Liga Dimayor"),
                parser.ConvertToCanonical(raw2, normalizer, "Liga Dimayor"),
                parser.ConvertToCanonical(raw3, normalizer, "Liga Dimayor"),
                parser.ConvertToCanonical(raw4, normalizer, "Liga Dimayor")
            };

            var issues = validator.ValidateRecords(
                new List<RawMatchRecord> { raw1, raw2, raw3, raw4 },
                canonicals,
                normalizer,
                "Colombia");

            Assert.NotEmpty(issues);
            Assert.Contains(issues, i => i.Category == ValidationIssueCategory.Duplicate);
            Assert.Contains(issues, i => i.Category == ValidationIssueCategory.ImpossibleScore);
            Assert.Contains(issues, i => i.Category == ValidationIssueCategory.UnresolvedClub);
        }

        [Fact]
        public void ObservedData_IsDistinct_FromSyntheticSimulationData()
        {
            var parser = new OpenFootballParser();
            var raw = new RawMatchRecord { EnvironmentId = "Brazil", HomeTeamRaw = "Flamengo", AwayTeamRaw = "Palmeiras", HomeGoalsRaw = "1", AwayGoalsRaw = "0" };
            var normalizer = new AliasNormalizer();
            var observedMatch = parser.ConvertToCanonical(raw, normalizer, "Série A");

            // Verify observed historical data
            Assert.Equal(ProvenanceSource.RealWorld, observedMatch.Provenance.Source);
            Assert.True(observedMatch.IsObserved);

            // Create synthetic simulation match event
            var simProv = new ProvenanceInfo(ProvenanceSource.Synthetic, "Simulated match");
            Assert.Equal(ProvenanceSource.Synthetic, simProv.Source);
            Assert.NotEqual(simProv.Source, observedMatch.Provenance.Source);
        }

        [Fact]
        public void CoverageReportGenerator_GeneratesReport_IncludingAllFourEnvironmentsAndGaps()
        {
            var results = new List<IngestionResult>
            {
                new IngestionResult
                {
                    EnvironmentId = "Colombia",
                    TotalRawRecords = 10,
                    ValidMatches = new List<CanonicalMatchRecord>
                    {
                        new CanonicalMatchRecord { EnvironmentId = "Colombia", CompetitionName = "Categoría Primera A", HomeGoals = 2, AwayGoals = 0 }
                    }
                },
                new IngestionResult
                {
                    EnvironmentId = "Argentina",
                    TotalRawRecords = 5,
                    ValidMatches = new List<CanonicalMatchRecord>
                    {
                        new CanonicalMatchRecord { EnvironmentId = "Argentina", CompetitionName = "Primera División", HomeGoals = 1, AwayGoals = 1 }
                    }
                },
                new IngestionResult
                {
                    EnvironmentId = "Brazil",
                    TotalRawRecords = 0,
                    ObservedGaps = new List<string> { "Missing Série A 2023 text source file" }
                }
                // CopaLibertadores intentionally omitted to test gap detection
            };

            var reportGen = new CoverageReportGenerator();
            string markdown = reportGen.GenerateMarkdownReport(results);

            Assert.Contains("Colombia", markdown);
            Assert.Contains("Argentina", markdown);
            Assert.Contains("Brazil", markdown);
            Assert.Contains("CopaLibertadores", markdown);
            Assert.Contains("NO DATA / GAP DETECTED", markdown);
            Assert.Contains("Missing Série A 2023 text source file", markdown);
            Assert.Contains("ProvenanceSource.RealWorld", markdown);
        }
    }
}
