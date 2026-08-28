using System;
using System.Collections.Generic;
using System.Linq;
using FootballWorldLab.Core.Events;
using FootballWorldLab.Core.Ids;
using FootballWorldLab.Core.Simulation;

namespace FootballWorldLab.Core.Analysis
{
    public sealed record CausalStep(
        long Tick,
        string Summary,
        double Salience,
        string Source);

    public sealed record CausalExplanation(
        string TargetKey,
        string PrimaryConclusion,
        List<CausalStep> ChainOfEvents);

    public static class CausalExplainer
    {
        public static CausalExplanation ExplainEntity(SimulationEngine engine, StableId entityId)
        {
            string key = entityId.Value;
            var chain = new List<CausalStep>();

            // 1. If entity is a Club
            if (engine.State.Clubs.TryGetValue(entityId, out var club))
            {
                // Find matches involving this club
                var clubMatches = engine.EventHistory
                    .OfType<MatchCompletedEvent>()
                    .Where(m => m.HomeClubId == entityId || m.AwayClubId == entityId)
                    .OrderBy(m => m.Tick)
                    .ToList();

                int totalWins = 0, totalLosses = 0, totalDraws = 0;
                foreach (var match in clubMatches)
                {
                    bool isHome = match.HomeClubId == entityId;
                    int scored = isHome ? match.HomeGoals : match.AwayGoals;
                    int conceded = isHome ? match.AwayGoals : match.HomeGoals;

                    if (scored > conceded) totalWins++;
                    else if (scored < conceded) totalLosses++;
                    else totalDraws++;

                    double salience = engine.EventSalience.GetValueOrDefault(match.EventId, 0.1);
                    if (salience >= 0.3 || scored + conceded >= 5)
                    {
                        string outcome = scored > conceded ? "Won" : (scored < conceded ? "Lost" : "Drew");
                        string opponentId = isHome ? match.AwayClubId.Value : match.HomeClubId.Value;
                        string oppName = engine.State.Clubs.TryGetValue(isHome ? match.AwayClubId : match.HomeClubId, out var opp) ? opp.Name : opponentId;

                        chain.Add(new CausalStep(
                            match.Tick,
                            $"{club.Name} {outcome} {scored}-{conceded} vs {oppName}",
                            salience,
                            match.Provenance.Source.ToString()));
                    }
                }

                string conclusion = $"{club.Name} reached Elo {club.RatingElo:F1} after {clubMatches.Count} matches ({totalWins}W, {totalDraws}D, {totalLosses}L).";
                return new CausalExplanation(key, conclusion, chain);
            }

            // 2. If entity is a Player
            if (engine.State.Players.TryGetValue(entityId, out var player))
            {
                var playerTransfers = engine.EventHistory
                    .OfType<PlayerTransferredEvent>()
                    .Where(t => t.PlayerId == entityId)
                    .ToList();

                foreach (var trf in playerTransfers)
                {
                    string fromName = engine.State.Clubs.TryGetValue(trf.FromClubId, out var f) ? f.Name : trf.FromClubId.Value;
                    string toName = engine.State.Clubs.TryGetValue(trf.ToClubId, out var t) ? t.Name : trf.ToClubId.Value;

                    chain.Add(new CausalStep(
                        trf.Tick,
                        $"Transferred from {fromName} to {toName} for ${trf.TransferFee:N0}",
                        engine.EventSalience.GetValueOrDefault(trf.EventId, 0.2),
                        trf.Provenance.Source.ToString()));
                }

                string conclusion = $"Player {entityId.Value} is rated {player.OverallRating}/{player.Potential} in position {player.Position}.";
                return new CausalExplanation(key, conclusion, chain);
            }

            // Fallback for general query
            return new CausalExplanation(key, $"No specific entity history found for '{key}'.", chain);
        }
    }
}
