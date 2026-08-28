using System;
using System.Collections.Generic;
using System.Linq;
using FootballWorldLab.Core.Entities;
using FootballWorldLab.Core.Ids;
using FootballWorldLab.Core.Simulation;

namespace FootballWorldLab.Core.Analysis
{
    public sealed record EmergentPhenomenon(
        string Type,
        string Description,
        double AnomalyScore,
        StableId AssociatedEntityId,
        string Evidence);

    public static class EmergenceDetector
    {
        public static List<EmergentPhenomenon> DetectEmergence(SimulationEngine engine)
        {
            var phenomena = new List<EmergentPhenomenon>();

            var state = engine.State;
            var matchEvents = engine.EventHistory
                .OfType<Events.MatchCompletedEvent>()
                .ToList();

            if (matchEvents.Count == 0) return phenomena;

            // 1. Detect Dynasties & Collapses based on Elo changes and match results
            var initialEloMap = new Dictionary<StableId, double>();
            foreach (var club in state.Clubs.Values)
            {
                // Simple initial estimate or current rating vs average
                initialEloMap[club.Id] = club.RatingElo;
            }

            // Track win counts per club
            var winCounts = new Dictionary<StableId, int>();
            foreach (var match in matchEvents)
            {
                if (match.HomeGoals > match.AwayGoals)
                {
                    winCounts[match.HomeClubId] = winCounts.GetValueOrDefault(match.HomeClubId) + 1;
                }
                else if (match.AwayGoals > match.HomeGoals)
                {
                    winCounts[match.AwayClubId] = winCounts.GetValueOrDefault(match.AwayClubId) + 1;
                }
            }

            foreach (var club in state.Clubs.Values)
            {
                int wins = winCounts.GetValueOrDefault(club.Id, 0);

                // Dynasty detection (very high Elo and dominant wins)
                if (club.RatingElo >= 1700.0 || wins >= 40)
                {
                    phenomena.Add(new EmergentPhenomenon(
                        "Dynasty",
                        $"Club '{club.Name}' established dominance with Elo {club.RatingElo:F1} and {wins} victories.",
                        Math.Min(1.0, (club.RatingElo - 1500.0) / 300.0),
                        club.Id,
                        $"Elo: {club.RatingElo:F1}, Wins: {wins}"));
                }

                // Collapse detection (Elo dropped below 1350)
                if (club.RatingElo <= 1350.0)
                {
                    phenomena.Add(new EmergentPhenomenon(
                        "Collapse",
                        $"Club '{club.Name}' suffered severe collapse, dropping to Elo {club.RatingElo:F1}.",
                        Math.Min(1.0, (1500.0 - club.RatingElo) / 300.0),
                        club.Id,
                        $"Elo: {club.RatingElo:F1}"));
                }
            }

            // 2. Detect High-Salience High-Scoring Thrillers / Upsets
            foreach (var match in matchEvents)
            {
                int totalGoals = match.HomeGoals + match.AwayGoals;
                if (totalGoals >= 7)
                {
                    string homeName = state.Clubs.TryGetValue(match.HomeClubId, out var h) ? h.Name : match.HomeClubId.Value;
                    string awayName = state.Clubs.TryGetValue(match.AwayClubId, out var a) ? a.Name : match.AwayClubId.Value;

                    phenomena.Add(new EmergentPhenomenon(
                        "ScoringAnomaly",
                        $"High-scoring match thriller: {homeName} {match.HomeGoals} - {match.AwayGoals} {awayName}.",
                        Math.Min(1.0, totalGoals / 10.0),
                        match.MatchId,
                        $"Goals: {totalGoals}"));
                }
            }

            // 3. Detect Player Career Anomalies (e.g. mega overall growth)
            foreach (var player in state.Players.Values)
            {
                if (player.OverallRating >= 88)
                {
                    phenomena.Add(new EmergentPhenomenon(
                        "SuperstarAscent",
                        $"Player {player.Id.Value} achieved elite rating of {player.OverallRating}.",
                        (player.OverallRating - 80) / 20.0,
                        player.Id,
                        $"Overall: {player.OverallRating}, Potential: {player.Potential}"));
                }
            }

            return phenomena;
        }
    }
}
