using System;
using System.Collections.Generic;

namespace FootballWorldLab.Core.Ingestion
{
    public sealed class MatchValidator
    {
        public const int MaxReasonableGoals = 30;

        public List<ValidationIssue> ValidateRecords(
            List<RawMatchRecord> rawRecords,
            List<CanonicalMatchRecord> canonicalRecords,
            AliasNormalizer normalizer,
            string environmentId)
        {
            var issues = new List<ValidationIssue>();
            var seenMatchKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < canonicalRecords.Count; i++)
            {
                var canonical = canonicalRecords[i];
                var raw = i < rawRecords.Count ? rawRecords[i] : null;

                // 1. Check Unresolved Clubs
                if (!canonical.HomeClubId.HasValue || string.IsNullOrWhiteSpace(canonical.CanonicalHomeTeam))
                {
                    issues.Add(new ValidationIssue
                    {
                        Category = ValidationIssueCategory.UnresolvedClub,
                        Severity = ValidationIssueSeverity.Error,
                        Message = $"Unresolved home club alias: '{canonical.HomeTeamRaw}'",
                        EnvironmentId = environmentId,
                        RawRecord = raw,
                        CanonicalRecord = canonical
                    });
                }

                if (!canonical.AwayClubId.HasValue || string.IsNullOrWhiteSpace(canonical.CanonicalAwayTeam))
                {
                    issues.Add(new ValidationIssue
                    {
                        Category = ValidationIssueCategory.UnresolvedClub,
                        Severity = ValidationIssueSeverity.Error,
                        Message = $"Unresolved away club alias: '{canonical.AwayTeamRaw}'",
                        EnvironmentId = environmentId,
                        RawRecord = raw,
                        CanonicalRecord = canonical
                    });
                }

                // 2. Check Impossible Scores
                if (canonical.HomeGoals < 0 || canonical.AwayGoals < 0 ||
                    canonical.HomeGoals > MaxReasonableGoals || canonical.AwayGoals > MaxReasonableGoals)
                {
                    issues.Add(new ValidationIssue
                    {
                        Category = ValidationIssueCategory.ImpossibleScore,
                        Severity = ValidationIssueSeverity.Error,
                        Message = $"Impossible match score detected: {canonical.HomeGoals}-{canonical.AwayGoals} ({canonical.CanonicalHomeTeam} vs {canonical.CanonicalAwayTeam})",
                        EnvironmentId = environmentId,
                        RawRecord = raw,
                        CanonicalRecord = canonical
                    });
                }

                // 3. Check Duplicate Matches
                string dedupeKey = $"{canonical.Date:yyyy-MM-dd}:{canonical.CanonicalHomeTeam.ToLowerInvariant()}:{canonical.CanonicalAwayTeam.ToLowerInvariant()}";
                if (seenMatchKeys.Contains(dedupeKey))
                {
                    issues.Add(new ValidationIssue
                    {
                        Category = ValidationIssueCategory.Duplicate,
                        Severity = ValidationIssueSeverity.Warning,
                        Message = $"Duplicate match detected on {canonical.Date:yyyy-MM-dd}: {canonical.CanonicalHomeTeam} vs {canonical.CanonicalAwayTeam}",
                        EnvironmentId = environmentId,
                        RawRecord = raw,
                        CanonicalRecord = canonical
                    });
                }
                else
                {
                    seenMatchKeys.Add(dedupeKey);
                }
            }

            return issues;
        }
    }
}
