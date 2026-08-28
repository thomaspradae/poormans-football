using FootballWorldLab.Core.Entities;
using FootballWorldLab.Core.Ids;
using FootballWorldLab.Core.Provenance;

namespace FootballWorldLab.Core.Events
{
    /// <summary>
    /// Event representing a player transfer between clubs.
    /// </summary>
    public sealed record PlayerTransferredEvent : BaseEvent
    {
        public StableId PlayerId { get; }
        public StableId FromClubId { get; }
        public StableId ToClubId { get; }
        public decimal TransferFee { get; }

        public PlayerTransferredEvent(StableId eventId, long tick, ProvenanceInfo provenance,
                                      StableId playerId, StableId fromClubId, StableId toClubId, decimal transferFee)
            : base(eventId, tick, provenance)
        {
            PlayerId = playerId;
            FromClubId = fromClubId;
            ToClubId = toClubId;
            TransferFee = transferFee;
        }
    }
    
    /// <summary>
    /// Event representing a player aging (birthday).
    /// </summary>
    public sealed record PlayerAgedEvent : BaseEvent
    {
        public StableId PlayerId { get; }
        public int NewAge { get; }

        public PlayerAgedEvent(StableId eventId, long tick, ProvenanceInfo provenance,
                               StableId playerId, int newAge)
            : base(eventId, tick, provenance)
        {
            PlayerId = playerId;
            NewAge = newAge;
        }
    }
    
    /// <summary>
    /// Event representing a match result.
    /// </summary>
    public sealed record MatchCompletedEvent : BaseEvent
    {
        public StableId MatchId { get; }
        public StableId HomeClubId { get; }
        public StableId AwayClubId { get; }
        public int HomeGoals { get; }
        public int AwayGoals { get; }

        public MatchCompletedEvent(StableId eventId, long tick, ProvenanceInfo provenance,
                                   StableId matchId, StableId homeClubId, StableId awayClubId,
                                   int homeGoals, int awayGoals)
            : base(eventId, tick, provenance)
        {
            MatchId = matchId;
            HomeClubId = homeClubId;
            AwayClubId = awayClubId;
            HomeGoals = homeGoals;
            AwayGoals = awayGoals;
        }
    }
    
    /// <summary>
    /// Event representing a change in player belief or opinion.
    /// </summary>
    public sealed record PlayerBeliefUpdatedEvent : BaseEvent
    {
        public StableId PlayerId { get; }
        public string BeliefKey { get; }
        public object NewValue { get; }

        public PlayerBeliefUpdatedEvent(StableId eventId, long tick, ProvenanceInfo provenance,
                                        StableId playerId, string beliefKey, object newValue)
            : base(eventId, tick, provenance)
        {
            PlayerId = playerId;
            BeliefKey = beliefKey;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// Event representing a manager being sacked.
    /// </summary>
    public sealed record ManagerSackedEvent : BaseEvent
    {
        public StableId ManagerId { get; }
        public StableId ClubId { get; }
        public string Reason { get; }

        public ManagerSackedEvent(StableId eventId, long tick, ProvenanceInfo provenance,
                                  StableId managerId, StableId clubId, string reason)
            : base(eventId, tick, provenance)
        {
            ManagerId = managerId;
            ClubId = clubId;
            Reason = reason;
        }
    }

    /// <summary>
    /// Event representing a manager being hired by a club.
    /// </summary>
    public sealed record ManagerHiredEvent : BaseEvent
    {
        public StableId ManagerId { get; }
        public StableId ClubId { get; }

        public ManagerHiredEvent(StableId eventId, long tick, ProvenanceInfo provenance,
                                 StableId managerId, StableId clubId)
            : base(eventId, tick, provenance)
        {
            ManagerId = managerId;
            ClubId = clubId;
        }
    }

    /// <summary>
    /// Event representing a competition trophy won by a club.
    /// </summary>
    public sealed record TrophyWonEvent : BaseEvent
    {
        public StableId CompetitionId { get; }
        public StableId ClubId { get; }
        public int SeasonYear { get; }

        public TrophyWonEvent(StableId eventId, long tick, ProvenanceInfo provenance,
                              StableId competitionId, StableId clubId, int seasonYear)
            : base(eventId, tick, provenance)
        {
            CompetitionId = competitionId;
            ClubId = clubId;
            SeasonYear = seasonYear;
        }
    }
}
