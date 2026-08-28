using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FootballWorldLab.Core.Ids;

namespace FootballWorldLab.Core.Provenance
{
    /// <summary>
    /// Represents a recorded scalar state change provenance contribution.
    /// </summary>
    public sealed record StateContribution
    {
        public StableId ContributionId { get; }
        public long Tick { get; }
        public StableId TargetEntityId { get; }
        public string PropertyName { get; }
        public object? PreviousValue { get; }
        public object? NewValue { get; }
        public StableId? SourceEventId { get; }
        public string? RuleId { get; }
        public ProvenanceInfo Provenance { get; }

        public StateContribution(
            StableId contributionId,
            long tick,
            StableId targetEntityId,
            string propertyName,
            object? previousValue,
            object? newValue,
            StableId? sourceEventId,
            string? ruleId,
            ProvenanceInfo provenance)
        {
            ContributionId = contributionId;
            Tick = tick;
            TargetEntityId = targetEntityId;
            PropertyName = propertyName;
            PreviousValue = previousValue;
            NewValue = newValue;
            SourceEventId = sourceEventId;
            RuleId = ruleId;
            Provenance = provenance;
        }
    }

    /// <summary>
    /// An immutable or queryable ledger holding state contributions with deterministic ordering.
    /// </summary>
    public sealed record StateContributionLedger
    {
        public ImmutableList<StateContribution> Entries { get; init; } = ImmutableList<StateContribution>.Empty;

        public StateContributionLedger() { }

        public StateContributionLedger(IEnumerable<StateContribution> entries)
        {
            Entries = entries.ToImmutableList();
        }

        public StateContributionLedger Add(StateContribution contribution)
        {
            return this with { Entries = Entries.Add(contribution) };
        }

        public StateContributionLedger AddRange(IEnumerable<StateContribution> contributions)
        {
            return this with { Entries = Entries.AddRange(contributions) };
        }

        public IEnumerable<StateContribution> GetContributionsForEntity(StableId entityId)
        {
            return Entries
                .Where(c => c.TargetEntityId == entityId)
                .OrderBy(c => c.Tick)
                .ThenBy(c => c.ContributionId.Value, StringComparer.Ordinal);
        }

        public IEnumerable<StateContribution> GetContributionsForProperty(StableId entityId, string propertyName)
        {
            return Entries
                .Where(c => c.TargetEntityId == entityId && string.Equals(c.PropertyName, propertyName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c.Tick)
                .ThenBy(c => c.ContributionId.Value, StringComparer.Ordinal);
        }

        public IEnumerable<StateContribution> GetContributionsByEvent(StableId eventId)
        {
            return Entries
                .Where(c => c.SourceEventId.HasValue && c.SourceEventId.Value == eventId)
                .OrderBy(c => c.Tick)
                .ThenBy(c => c.ContributionId.Value, StringComparer.Ordinal);
        }
    }
}
