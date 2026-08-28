using System;
using FootballWorldLab.Core.Simulation;

namespace FootballWorldLab.Core.Invariants
{
    public static class StressTestRunner
    {
        public static bool RunStressTest(int numWorlds = 5, int yearsPerWorld = 10, ulong baseSeed = 1000UL)
        {
            for (int i = 0; i < numWorlds; i++)
            {
                ulong seed = baseSeed + (ulong)i * 12345UL;
                var engine = new SimulationEngine(seed);
                engine.InitializeDefaultWorld(clubsPerLeague: 12, leagues: 2);

                for (int y = 0; y < yearsPerWorld; y++)
                {
                    engine.StepSeason();
                    InvariantChecker.Validate(engine.State, engine);
                }
            }
            return true;
        }

        public static bool RunSensitivityTest(double eloNoiseStdDev = 50.0)
        {
            var engine = new SimulationEngine(55555UL);
            engine.InitializeDefaultWorld(clubsPerLeague: 10, leagues: 1);

            // Perturb Elo ratings
            foreach (var club in engine.State.Clubs.Values)
            {
                double noise = (engine.Rng.NextDouble() * 2.0 - 1.0) * eloNoiseStdDev;
                var perturbedClub = club with { RatingElo = club.RatingElo + noise };
                engine.UpdateState(engine.State.WithClub(perturbedClub));
            }

            // Run 5 seasons and validate state
            for (int y = 0; y < 5; y++)
            {
                engine.StepSeason();
                InvariantChecker.Validate(engine.State, engine);
            }

            return true;
        }
    }
}
