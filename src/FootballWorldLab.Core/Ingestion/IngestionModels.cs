using System;
using System.Collections.Generic;
using FootballWorldLab.Core.Ids;
using FootballWorldLab.Core.Provenance;

namespace FootballWorldLab.Core.Ingestion
{
    public sealed class SourceConfig
    {
        public string SourceId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Sha256 { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public sealed class EnvironmentConfig
    {
        public string EnvironmentId { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string CompetitionName { get; set; } = string.Empty;
        public List<SourceConfig> Sources { get; set; } = new();
    }

    public sealed class SourceManifest
    {
        public string DatasetName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
        public string LicenseUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<EnvironmentConfig> Environments { get; set; } = new();
        public Dictionary<string, string> DefaultAliases { get; set; } = new();
    }

    public sealed class RawMatchRecord
    {
        public string EnvironmentId { get; set; } = string.Empty;
        public string SourceId { get; set; } = string.Empty;
        public string RawText { get; set; } = string.Empty;
        public string DateText { get; set; } = string.Empty;
        public string HomeTeamRaw { get; set; } = string.Empty;
        public string AwayTeamRaw { get; set; } = string.Empty;
        public string HomeGoalsRaw { get; set; } = string.Empty;
        public string AwayGoalsRaw { get; set; } = string.Empty;
    }

    public sealed class CanonicalMatchRecord
    {
        public StableId MatchId { get; set; }
        public string EnvironmentId { get; set; } = string.Empty;
        public string CompetitionName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string HomeTeamRaw { get; set; } = string.Empty;
        public string AwayTeamRaw { get; set; } = string.Empty;
        public string CanonicalHomeTeam { get; set; } = string.Empty;
        public string CanonicalAwayTeam { get; set; } = string.Empty;
        public StableId? HomeClubId { get; set; }
        public StableId? AwayClubId { get; set; }
        public int HomeGoals { get; set; }
        public int AwayGoals { get; set; }
        public ProvenanceInfo Provenance { get; set; }

        public bool IsObserved => Provenance.Source == ProvenanceSource.RealWorld;
    }

    public enum ValidationIssueCategory
    {
        Duplicate,
        ImpossibleScore,
        UnresolvedClub,
        InvalidDate
    }

    public enum ValidationIssueSeverity
    {
        Warning,
        Error
    }

    public sealed class ValidationIssue
    {
        public ValidationIssueCategory Category { get; set; }
        public ValidationIssueSeverity Severity { get; set; }
        public string Message { get; set; } = string.Empty;
        public string EnvironmentId { get; set; } = string.Empty;
        public RawMatchRecord? RawRecord { get; set; }
        public CanonicalMatchRecord? CanonicalRecord { get; set; }
    }

    public sealed class IngestionResult
    {
        public string EnvironmentId { get; set; } = string.Empty;
        public int TotalRawRecords { get; set; }
        public List<CanonicalMatchRecord> ValidMatches { get; set; } = new();
        public List<ValidationIssue> ValidationIssues { get; set; } = new();
        public List<string> ObservedGaps { get; set; } = new();
    }
}
