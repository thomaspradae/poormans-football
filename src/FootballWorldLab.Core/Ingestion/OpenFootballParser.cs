using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using FootballWorldLab.Core.Ids;
using FootballWorldLab.Core.Provenance;

namespace FootballWorldLab.Core.Ingestion
{
    public sealed class OpenFootballParser
    {
        // Regex patterns for openfootball match results line
        // E.g.: "   [Fri Aug/18]   Boca Juniors  2-1  River Plate"
        // E.g.: "   18.08.2023  Millonarios FC  3-0  Santa Fe"
        // E.g.: "   Millonarios FC  1-1  Santa Fe"
        private static readonly Regex ResultRegex = new Regex(
            @"^(?:\[?[A-Za-z]{3}\s+)?(?:([A-Za-z]{3}/\d{1,2}|\d{1,2}\.\d{1,2}\.\d{4}|\d{4}-\d{2}-\d{2})\s+)?(.*?)\s+(\d{1,2})\s*-\s*(\d{1,2})\s+(.*?)$",
            RegexOptions.Compiled);

        public List<RawMatchRecord> ParseContent(string content, string environmentId, string sourceId)
        {
            var rawRecords = new List<RawMatchRecord>();
            if (string.IsNullOrWhiteSpace(content)) return rawRecords;

            using var reader = new StringReader(content);
            string? line;
            string currentDate = string.Empty;
            int currentYear = 2023; // default fallback year

            while ((line = reader.ReadLine()) != null)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#") || trimmed.StartsWith("="))
                    continue;

                // Check for year headers like "= 2023" or "=== 2023 ==="
                if (trimmed.StartsWith("=") && Regex.IsMatch(trimmed, @"\b(19\d\d|20\d\d)\b"))
                {
                    var mYear = Regex.Match(trimmed, @"\b(19\d\d|20\d\d)\b");
                    if (mYear.Success)
                    {
                        int.TryParse(mYear.Value, out currentYear);
                    }
                    continue;
                }

                // Check for date line headers e.g. "Group A", "Round 1", "[Fri Aug 18]"
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]") && !trimmed.Contains("-"))
                {
                    currentDate = trimmed.Trim('[', ']');
                    continue;
                }

                // Parse match line
                var match = ResultRegex.Match(trimmed);
                if (match.Success)
                {
                    string datePart = match.Groups[1].Value;
                    if (string.IsNullOrWhiteSpace(datePart))
                    {
                        datePart = string.IsNullOrWhiteSpace(currentDate) ? $"{currentYear}-01-01" : currentDate;
                    }

                    string homeTeam = match.Groups[2].Value.Trim();
                    string homeGoals = match.Groups[3].Value.Trim();
                    string awayGoals = match.Groups[4].Value.Trim();
                    string awayTeam = match.Groups[5].Value.Trim();

                    rawRecords.Add(new RawMatchRecord
                    {
                        EnvironmentId = environmentId,
                        SourceId = sourceId,
                        RawText = trimmed,
                        DateText = datePart,
                        HomeTeamRaw = homeTeam,
                        AwayTeamRaw = awayTeam,
                        HomeGoalsRaw = homeGoals,
                        AwayGoalsRaw = awayGoals
                    });
                }
            }

            return rawRecords;
        }

        public CanonicalMatchRecord ConvertToCanonical(RawMatchRecord raw, AliasNormalizer normalizer, string competitionName, int defaultYear = 2023)
        {
            DateTime date = ParseDate(raw.DateText, defaultYear);
            bool homeResolved = normalizer.TryResolve(raw.HomeTeamRaw, out string canonicalHome, out StableId homeClubId);
            bool awayResolved = normalizer.TryResolve(raw.AwayTeamRaw, out string canonicalAway, out StableId awayClubId);

            int.TryParse(raw.HomeGoalsRaw, out int homeGoals);
            int.TryParse(raw.AwayGoalsRaw, out int awayGoals);

            string matchKey = $"{raw.EnvironmentId}-{date:yyyyMMdd}-{canonicalHome}-vs-{canonicalAway}";
            var matchId = StableId.Create("Match", matchKey);

            var provenance = new ProvenanceInfo(
                ProvenanceSource.RealWorld,
                $"Observed openfootball historical result from source '{raw.SourceId}' ({raw.EnvironmentId})",
                DateTime.UtcNow,
                "OpenFootballIngestor");

            return new CanonicalMatchRecord
            {
                MatchId = matchId,
                EnvironmentId = raw.EnvironmentId,
                CompetitionName = competitionName,
                Date = date,
                HomeTeamRaw = raw.HomeTeamRaw,
                AwayTeamRaw = raw.AwayTeamRaw,
                CanonicalHomeTeam = canonicalHome,
                CanonicalAwayTeam = canonicalAway,
                HomeClubId = homeResolved ? homeClubId : null,
                AwayClubId = awayResolved ? awayClubId : null,
                HomeGoals = homeGoals,
                AwayGoals = awayGoals,
                Provenance = provenance
            };
        }

        private static DateTime ParseDate(string dateText, int defaultYear)
        {
            if (DateTime.TryParse(dateText, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                return parsed;
            }

            // Try "Jan/15" or "Aug/18" format
            var match = Regex.Match(dateText, @"([A-Za-z]{3})/(\d{1,2})");
            if (match.Success)
            {
                string monthName = match.Groups[1].Value;
                if (int.TryParse(match.Groups[2].Value, out int day))
                {
                    int month = DateTime.ParseExact(monthName, "MMM", CultureInfo.InvariantCulture).Month;
                    return new DateTime(defaultYear, month, day);
                }
            }

            return new DateTime(defaultYear, 1, 1);
        }
    }
}
