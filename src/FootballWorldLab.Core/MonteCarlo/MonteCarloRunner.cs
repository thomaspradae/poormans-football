using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FootballWorldLab.Core.Analysis;
using FootballWorldLab.Core.Events;
using FootballWorldLab.Core.Invariants;
using FootballWorldLab.Core.Simulation;

namespace FootballWorldLab.Core.MonteCarlo
{
    public sealed record WorldSummary(
        int WorldId,
        ulong Seed,
        int YearsSimulated,
        int TotalMatches,
        int TotalGoals,
        double AvgGoalsPerMatch,
        double HomeWinPct,
        double DrawPct,
        double AwayWinPct,
        double MaxElo,
        double MinElo,
        int TransferCount,
        List<EmergentPhenomenon> EmergentPhenomena,
        SimulationEngine Engine);

    public sealed record MonteCarloResult(
        int TargetWorlds,
        int TargetYears,
        TimeSpan ElapsedTime,
        List<WorldSummary> WorldSummaries,
        List<WorldSummary> WeirdestWorlds);

    public static class MonteCarloRunner
    {
        public static MonteCarloResult Run(int numWorlds = 100, int yearsPerWorld = 30, ulong baseSeed = 10000UL)
        {
            var startTime = DateTime.UtcNow;
            var summaries = new ConcurrentBag<WorldSummary>();

            // Use parallel execution for speed while preserving deterministic per-world seeds
            Parallel.For(0, numWorlds, i =>
            {
                ulong seed = baseSeed + (ulong)i * 987654321UL;
                var engine = new SimulationEngine(seed);
                engine.InitializeDefaultWorld(clubsPerLeague: 10, leagues: 1);
                engine.RunYears(yearsPerWorld);

                InvariantChecker.Validate(engine.State, engine);

                var matchEvts = engine.EventHistory.OfType<MatchCompletedEvent>().ToList();
                int totalMatches = matchEvts.Count;
                int totalGoals = matchEvts.Sum(m => m.HomeGoals + m.AwayGoals);
                double avgGoals = totalMatches > 0 ? (double)totalGoals / totalMatches : 0.0;

                int homeWins = matchEvts.Count(m => m.HomeGoals > m.AwayGoals);
                int draws = matchEvts.Count(m => m.HomeGoals == m.AwayGoals);
                int awayWins = matchEvts.Count(m => m.AwayGoals > m.HomeGoals);

                double homePct = totalMatches > 0 ? (double)homeWins / totalMatches : 0.0;
                double drawPct = totalMatches > 0 ? (double)draws / totalMatches : 0.0;
                double awayPct = totalMatches > 0 ? (double)awayWins / totalMatches : 0.0;

                double maxElo = engine.State.Clubs.Count > 0 ? engine.State.Clubs.Values.Max(c => c.RatingElo) : 1500.0;
                double minElo = engine.State.Clubs.Count > 0 ? engine.State.Clubs.Values.Min(c => c.RatingElo) : 1500.0;

                int transferCount = engine.EventHistory.OfType<PlayerTransferredEvent>().Count();
                var phenomena = EmergenceDetector.DetectEmergence(engine);

                summaries.Add(new WorldSummary(
                    i + 1,
                    seed,
                    yearsPerWorld,
                    totalMatches,
                    totalGoals,
                    avgGoals,
                    homePct,
                    drawPct,
                    awayPct,
                    maxElo,
                    minElo,
                    transferCount,
                    phenomena,
                    engine));
            });

            var orderedSummaries = summaries.OrderBy(s => s.WorldId).ToList();

            // Rank weirdest worlds based on top anomaly scores in emergent phenomena
            var weirdest = orderedSummaries
                .OrderByDescending(s => s.EmergentPhenomena.Sum(p => p.AnomalyScore) + Math.Abs(s.MaxElo - 1500.0) / 100.0)
                .Take(5)
                .ToList();

            var elapsed = DateTime.UtcNow - startTime;
            return new MonteCarloResult(numWorlds, yearsPerWorld, elapsed, orderedSummaries, weirdest);
        }
    }
}
