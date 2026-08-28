using System;
<<<<<<< ours
using System.Collections.Generic;
using System.Linq;
using FootballWorldLab.Core.Events;
using FootballWorldLab.Core.Ids;
=======
using FootballWorldLab.Core.Events;
>>>>>>> theirs
using FootballWorldLab.Core.State;

namespace FootballWorldLab.Core.Salience
{
<<<<<<< ours
    public sealed class CausalThread
    {
        public StableId Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public double HighestSalience { get; set; }
        public List<BaseEvent> Events { get; set; } = new();
        public List<string> Explanations { get; set; } = new();
    }

    public static class SalienceEvaluator
    {
        public static double CalculateSalience(BaseEvent @event, WorldState state)
        {
            return @event switch
            {
                TrophyWonEvent trophy => 90.0 + (trophy.CompetitionId.Value.Contains("LIB") ? 10.0 : 0.0),
                ManagerSackedEvent => 85.0,
                ManagerHiredEvent => 70.0,
                MatchCompletedEvent match => EvaluateMatchSalience(match, state),
                PlayerTransferredEvent transfer => 60.0 + (double)(transfer.TransferFee / 1000000m),
                PlayerAgedEvent => 10.0,
                _ => 20.0
            };
        }

        private static double EvaluateMatchSalience(MatchCompletedEvent match, WorldState state)
        {
            int goalDiff = Math.Abs(match.HomeGoals - match.AwayGoals);
            int totalGoals = match.HomeGoals + match.AwayGoals;

            double salience = 40.0;
            if (totalGoals >= 5) salience += 25.0; // High scoring
            if (goalDiff >= 4) salience += 20.0;  // Thrashing / blowout

            if (state.Clubs.TryGetValue(match.HomeClubId, out var home) &&
                state.Clubs.TryGetValue(match.AwayClubId, out var away))
            {
                double eloDiff = Math.Abs(home.RatingElo - away.RatingElo);
                bool homeUnderdogWon = (home.RatingElo + 100 < away.RatingElo) && match.HomeGoals > match.AwayGoals;
                bool awayUnderdogWon = (away.RatingElo > home.RatingElo + 100) && match.AwayGoals > match.HomeGoals;

                if (homeUnderdogWon || awayUnderdogWon)
                {
                    salience += 30.0; // Upset victory
                }
            }

            return Math.Min(100.0, salience);
        }

        public static List<CausalThread> ClusterThreads(IEnumerable<BaseEvent> events, WorldState state)
        {
            var threads = new List<CausalThread>();
            var eventsList = events.ToList();

            // Group 1: Trophy races & deciders
            var trophyEvents = eventsList.OfType<TrophyWonEvent>().ToList();
            foreach (var trophy in trophyEvents)
            {
                double salience = CalculateSalience(trophy, state);
                string compName = state.Competitions.TryGetValue(trophy.CompetitionId, out var c) ? c.Name : trophy.CompetitionId.Value;
                string clubName = state.Clubs.TryGetValue(trophy.ClubId, out var cl) ? cl.Name : trophy.ClubId.Value;

                var thread = new CausalThread
                {
                    Id = StableId.Create("Thread", $"Trophy_{trophy.CompetitionId.Value}_{trophy.SeasonYear}"),
                    Title = $"{trophy.SeasonYear} {compName} Champion: {clubName}",
                    HighestSalience = salience,
                    Events = new List<BaseEvent> { trophy },
                    Explanations = new List<string>
                    {
                        $"Root Cause: {clubName} secured top standing in season {trophy.SeasonYear}.",
                        $"Outcome: Trophy {compName} awarded to {clubName} (EventId: {trophy.EventId})."
                    }
                };
                threads.Add(thread);
            }

            // Group 2: Manager Sacking & Replacement Threads
            var sackEvents = eventsList.OfType<ManagerSackedEvent>().ToList();
            foreach (var sack in sackEvents)
            {
                double salience = CalculateSalience(sack, state);
                string clubName = state.Clubs.TryGetValue(sack.ClubId, out var cl) ? cl.Name : sack.ClubId.Value;

                var relatedHire = eventsList.OfType<ManagerHiredEvent>()
                    .FirstOrDefault(h => h.ClubId == sack.ClubId && h.Tick >= sack.Tick);

                var threadEvents = new List<BaseEvent> { sack };
                if (relatedHire != null) threadEvents.Add(relatedHire);

                var thread = new CausalThread
                {
                    Id = StableId.Create("Thread", $"Manager_{sack.ManagerId.Value}_{sack.Tick}"),
                    Title = $"Manager Sacking at {clubName}",
                    HighestSalience = salience,
                    Events = threadEvents,
                    Explanations = new List<string>
                    {
                        $"Root Cause: Manager finished poorly, failing club expectations at {clubName}.",
                        $"Event: Manager {sack.ManagerId} sacked (Reason: {sack.Reason}).",
                        relatedHire != null ? $"Follow-up: New manager hired for {clubName} (EventId: {relatedHire.EventId})." : "Follow-up: Manager position vacant."
                    }
                };
                threads.Add(thread);
            }

            // Group 3: High Salience Match Threads (Upsets / Thrashings)
            var matchEvents = eventsList.OfType<MatchCompletedEvent>()
                .Where(m => CalculateSalience(m, state) >= 70.0)
                .Take(10)
                .ToList();

            foreach (var match in matchEvents)
            {
                double salience = CalculateSalience(match, state);
                string homeName = state.Clubs.TryGetValue(match.HomeClubId, out var h) ? h.Name : match.HomeClubId.Value;
                string awayName = state.Clubs.TryGetValue(match.AwayClubId, out var a) ? a.Name : match.AwayClubId.Value;

                var thread = new CausalThread
                {
                    Id = StableId.Create("Thread", $"Match_{match.MatchId.Value}"),
                    Title = $"High-Salience Match: {homeName} {match.HomeGoals}-{match.AwayGoals} {awayName}",
                    HighestSalience = salience,
                    Events = new List<BaseEvent> { match },
                    Explanations = new List<string>
                    {
                        $"Root Cause: Tactical clash and relative Elo dynamic between {homeName} and {awayName}.",
                        $"Outcome: {homeName} scored {match.HomeGoals}, {awayName} scored {match.AwayGoals} (Salience: {salience:F1})."
                    }
                };
                threads.Add(thread);
            }

            return threads.OrderByDescending(t => t.HighestSalience).ToList();
=======
    public static class SalienceEvaluator
    {
        /// <summary>
        /// Calculates a salience score from 0.0 (routine event) to 1.0 (historic event).
        /// </summary>
        public static double EvaluateMatchSalience(MatchCompletedEvent evt, WorldState state, double homeElo, double awayElo, bool isTitleDecider = false)
        {
            double baseScore = 0.2;

            // Upset calculation: if lower Elo team beats higher Elo team
            double eloDiff = homeElo - awayElo; // positive means home is favored
            bool homeWon = evt.HomeGoals > evt.AwayGoals;
            bool awayWon = evt.AwayGoals > evt.HomeGoals;
            
            double upsetFactor = 0.0;
            if (homeWon && eloDiff < -100)
            {
                upsetFactor = Math.Min(0.5, Math.Abs(eloDiff) / 500.0);
            }
            else if (awayWon && eloDiff > 100)
            {
                upsetFactor = Math.Min(0.5, eloDiff / 500.0);
            }

            // High scoring thriller factor
            int totalGoals = evt.HomeGoals + evt.AwayGoals;
            double goalFactor = Math.Min(0.3, totalGoals * 0.05);

            double titleStakesFactor = isTitleDecider ? 0.3 : 0.0;

            double score = baseScore + upsetFactor + goalFactor + titleStakesFactor;
            return Math.Clamp(score, 0.0, 1.0);
        }

        public static double EvaluateTransferSalience(PlayerTransferredEvent evt, double playerRating, decimal fee)
        {
            double ratingFactor = Math.Max(0.0, (playerRating - 75.0) / 25.0) * 0.4;
            double feeFactor = Math.Min(0.4, (double)fee / 50_000_000.0 * 0.4);
            return Math.Clamp(0.1 + ratingFactor + feeFactor, 0.0, 1.0);
>>>>>>> theirs
        }
    }
}
