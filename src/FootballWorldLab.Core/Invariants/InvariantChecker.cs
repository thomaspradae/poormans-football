using System;
using System.Collections.Generic;
using System.Linq;
using FootballWorldLab.Core.Simulation;
using FootballWorldLab.Core.State;

namespace FootballWorldLab.Core.Invariants
{
    public sealed class InvariantViolationException : Exception
    {
        public InvariantViolationException(string message) : base(message) { }
    }

    public static class InvariantChecker
    {
        public static void Validate(WorldState state, SimulationEngine? engine = null)
        {
            var errors = GetInvariantViolations(state, engine);
            if (errors.Count > 0)
            {
                throw new InvariantViolationException($"Invariant failure(s): {string.Join("; ", errors)}");
            }
        }

        public static List<string> GetInvariantViolations(WorldState state, SimulationEngine? engine = null)
        {
            var errors = new List<string>();

            // 1. Clock checks
            if (engine != null)
            {
                if (engine.Clock.TotalTicks < 0)
                    errors.Add("Clock TotalTicks cannot be negative.");
                if (engine.Clock.CurrentYear < 1900 || engine.Clock.CurrentYear > 2200)
                    errors.Add($"Clock CurrentYear out of valid range: {engine.Clock.CurrentYear}");
            }

            // 2. Club & Elo checks
            if (state.Clubs.Count == 0 && engine?.Clock.TotalTicks > 0)
            {
                errors.Add("World state contains zero clubs.");
            }

            foreach (var club in state.Clubs.Values)
            {
                if (double.IsNaN(club.RatingElo) || double.IsInfinity(club.RatingElo))
                    errors.Add($"Club '{club.Name}' ({club.Id.Value}) has invalid Elo: {club.RatingElo}");
                else if (club.RatingElo < 100.0 || club.RatingElo > 4000.0)
                    errors.Add($"Club '{club.Name}' ({club.Id.Value}) Elo out of bounds: {club.RatingElo}");
            }

            // 3. Player checks
            foreach (var player in state.Players.Values)
            {
                if (player.OverallRating < 1 || player.OverallRating > 100)
                    errors.Add($"Player {player.Id.Value} OverallRating out of bounds: {player.OverallRating}");
                if (player.Potential < player.OverallRating)
                    errors.Add($"Player {player.Id.Value} Potential ({player.Potential}) lower than Overall ({player.OverallRating})");
            }

            // 4. Contract checks
            foreach (var contract in state.Contracts.Values)
            {
                if (contract.WeeklyWage < 0m)
                    errors.Add($"Contract {contract.Id.Value} has negative wage: {contract.WeeklyWage}");
                if (contract.StartDate > contract.EndDate)
                    errors.Add($"Contract {contract.Id.Value} StartDate after EndDate.");
            }

            // 5. Match checks
            foreach (var match in state.Matches.Values)
            {
                if (match.HomeClubId == match.AwayClubId)
                    errors.Add($"Match {match.Id.Value} has identical Home and Away club.");
                if (match.HomeGoals < 0 || match.AwayGoals < 0)
                    errors.Add($"Match {match.Id.Value} has negative goals.");
            }

            // 6. Squad membership checks
            foreach (var squad in state.SquadMemberships.Values)
            {
                if (!state.Clubs.ContainsKey(squad.ClubId))
                    errors.Add($"SquadMembership {squad.Id.Value} references non-existent Club {squad.ClubId.Value}.");
                if (!state.Players.ContainsKey(squad.PlayerId))
                    errors.Add($"SquadMembership {squad.Id.Value} references non-existent Player {squad.PlayerId.Value}.");
            }

            return errors;
        }
    }
}
