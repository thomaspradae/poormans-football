using System;
using System.IO;
using System.Linq;
using FootballWorldLab.Core.Analysis;
using FootballWorldLab.Core.Clock;
using FootballWorldLab.Core.Entities;
using FootballWorldLab.Core.Ids;
using FootballWorldLab.Core.Invariants;
using FootballWorldLab.Core.MonteCarlo;
using FootballWorldLab.Core.Persistence;
using FootballWorldLab.Core.Rng;
using FootballWorldLab.Core.Simulation;
using Xunit;

namespace FootballWorldLab.Core.Tests
{
    public class CoreUnitTests
    {
        [Fact]
        public void SeededRandom_ProducesDeterministicSequence()
        {
            const ulong seed = 12345UL;

            var rng1 = new SeededRandom(seed);
            var sequence1 = new int[10];
            for (int i = 0; i < sequence1.Length; i++)
            {
                sequence1[i] = rng1.NextInt(1, 100);
            }

            var rng2 = new SeededRandom(seed);
            var sequence2 = new int[10];
            for (int i = 0; i < sequence2.Length; i++)
            {
                sequence2[i] = rng2.NextInt(1, 100);
            }

            Assert.Equal(sequence1, sequence2);
        }

        [Fact]
        public void SeededRandom_DifferentSeeds_ProduceDifferentSequences()
        {
            var rng1 = new SeededRandom(11111UL);
            var rng2 = new SeededRandom(99999UL);

            int val1 = rng1.NextInt(1, 100000);
            int val2 = rng2.NextInt(1, 100000);

            Assert.NotEqual(val1, val2);
        }

        [Fact]
        public void SimulationClock_StepsDaysAndWeeksCorrectly()
        {
            var clock = new SimulationClock(2024, 1, 1);
            Assert.Equal(new DateTime(2024, 1, 1), clock.CurrentDate);

            clock.StepDay(5);
            Assert.Equal(new DateTime(2024, 1, 6), clock.CurrentDate);
            Assert.Equal(5, clock.TotalTicks);

            clock.StepWeek(2);
            Assert.Equal(new DateTime(2024, 1, 20), clock.CurrentDate);
            Assert.Equal(19, clock.TotalTicks);

            clock.AdvanceToNextSeason();
            Assert.Equal(2025, clock.SeasonStartYear);
            Assert.Equal(new DateTime(2025, 1, 1), clock.CurrentDate);
        }

        [Fact]
        public void StableId_CreatesConsistentAndEqualValues()
        {
            var id1 = StableId.Create("Club", "Boca");
            var id2 = StableId.Create("Club", "Boca");
            var id3 = StableId.Create("Club", "River");

            Assert.Equal(id1, id2);
            Assert.NotEqual(id1, id3);

            var det1 = StableId.CreateDeterministic("Match", "Season2024", 42);
            var det2 = StableId.CreateDeterministic("Match", "Season2024", 42);

            Assert.Equal(det1, det2);
            Assert.StartsWith("Match-", det1.Value);
        }

        [Fact]
        public void CoreEntities_InstantiateWithValidProperties()
        {
            var countryId = StableId.Create("Country", "COL");
            var country = new Country(countryId, "Colombia", "COL");
            Assert.Equal("Colombia", country.Name);

            var cityId = StableId.Create("City", "BOG");
            var city = new City(cityId, country.Id, "Bogotá");
            Assert.Equal("Bogotá", city.Name);

            var clubId = StableId.Create("Club", "MILL");
            var club = new Club(clubId, city.Id, "Millonarios FC", "Millonarios", 1650.0);
            Assert.Equal(1650.0, club.RatingElo);

            var personId = StableId.Create("Person", "P1");
            var person = new Person(personId, country.Id, "Radamel", "Falcao", new DateTime(1986, 2, 10));

            var playerId = StableId.Create("Player", "PLY1");
            var player = new Player(playerId, person.Id, "ST", 82, 85);
            Assert.Equal("ST", player.Position);

            var contractId = StableId.Create("Contract", "C1");
            var contract = new Contract(contractId, club.Id, person.Id, 25000m, new DateTime(2024, 1, 1), new DateTime(2025, 12, 31));
            Assert.Equal(25000m, contract.WeeklyWage);
        }

        [Fact]
        public void SaveAndReload_WorldState_SerializesAndDeserializesWithStableIdKeys()
        {
            var engine = new SimulationEngine(42UL);
            engine.InitializeDefaultWorld(clubsPerLeague: 4, leagues: 1);
            engine.RunYears(1);

            string tempFile = Path.Combine(Path.GetTempPath(), $"save_test_{Guid.NewGuid()}.json");
            try
            {
                SaveManager.SaveToFile(engine.State, tempFile);
                Assert.True(File.Exists(tempFile));

                var reloadedState = SaveManager.LoadFromFile(tempFile);
                Assert.Equal(engine.State.Clubs.Count, reloadedState.Clubs.Count);
                Assert.Equal(engine.State.Players.Count, reloadedState.Players.Count);
                Assert.Equal(engine.State.Competitions.Count, reloadedState.Competitions.Count);

                foreach (var kvp in engine.State.Clubs)
                {
                    Assert.True(reloadedState.Clubs.TryGetValue(kvp.Key, out var reloadedClub));
                    Assert.Equal(kvp.Value.Name, reloadedClub.Name);
                    Assert.Equal(kvp.Value.RatingElo, reloadedClub.RatingElo);
                }
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void SimulationEngine_RunsSeasonsAndProducesMatches()
        {
            var engine = new SimulationEngine(42UL);
            engine.InitializeDefaultWorld(clubsPerLeague: 6, leagues: 1);
            Assert.Equal(6, engine.State.Clubs.Count);

            engine.RunYears(3);
            Assert.Equal(2027, engine.Clock.CurrentYear);
            Assert.True(engine.EventHistory.Count > 0);
        }

        [Fact]
        public void SimulationEngine_Determinism_IdenticalSeedProducesIdenticalResults()
        {
            var engine1 = new SimulationEngine(999UL);
            engine1.InitializeDefaultWorld(clubsPerLeague: 6, leagues: 1);
            engine1.RunYears(2);

            var engine2 = new SimulationEngine(999UL);
            engine2.InitializeDefaultWorld(clubsPerLeague: 6, leagues: 1);
            engine2.RunYears(2);

            Assert.Equal(engine1.State.Clubs.Count, engine2.State.Clubs.Count);
            foreach (var kvp in engine1.State.Clubs)
            {
                Assert.True(engine2.State.Clubs.TryGetValue(kvp.Key, out var club2));
                Assert.Equal(kvp.Value.RatingElo, club2.RatingElo, precision: 4);
            }
        }

        [Fact]
        public void InvariantChecker_ValidatesStateAndCatchesViolations()
        {
            var engine = new SimulationEngine(123UL);
            engine.InitializeDefaultWorld(clubsPerLeague: 6, leagues: 1);
            engine.RunYears(1);

            // Valid state should pass with zero violations
            var errors = InvariantChecker.GetInvariantViolations(engine.State, engine);
            Assert.Empty(errors);

            // Corrupt club Elo with NaN and verify violation caught
            var corruptedClub = engine.State.Clubs.Values.First() with { RatingElo = double.NaN };
            var corruptedState = engine.State.WithClub(corruptedClub);

            Assert.Throws<InvariantViolationException>(() => InvariantChecker.Validate(corruptedState, engine));
        }

        [Fact]
        public void EmergenceDetector_And_CausalExplainer_FunctionCorrectly()
        {
            var engine = new SimulationEngine(777UL);
            engine.InitializeDefaultWorld(clubsPerLeague: 8, leagues: 1);
            engine.RunYears(5);

            var phenomena = EmergenceDetector.DetectEmergence(engine);
            Assert.NotNull(phenomena);

            var firstClub = engine.State.Clubs.Keys.First();
            var explanation = CausalExplainer.ExplainEntity(engine, firstClub);

            Assert.NotNull(explanation);
            Assert.Equal(firstClub.Value, explanation.TargetKey);
            Assert.False(string.IsNullOrWhiteSpace(explanation.PrimaryConclusion));
        }

        [Fact]
        public void StressAndSensitivity_TestsRunWithoutExceptions()
        {
            Assert.True(StressTestRunner.RunStressTest(numWorlds: 2, yearsPerWorld: 3, baseSeed: 100UL));
            Assert.True(StressTestRunner.RunSensitivityTest(eloNoiseStdDev: 30.0));
        }

        [Fact]
        public void MonteCarlo_100Worlds_30Years_ExecutesAndGeneratesReports()
        {
            string testOutputDir = Path.Combine(Path.GetTempPath(), "FWL_MC_Test_" + Guid.NewGuid().ToString("N"));

            try
            {
                // Run Monte Carlo suite (100 worlds x 30 years)
                var result = MonteCarloRunner.Run(numWorlds: 100, yearsPerWorld: 30, baseSeed: 10000UL);

                Assert.Equal(100, result.TargetWorlds);
                Assert.Equal(30, result.TargetYears);
                Assert.Equal(100, result.WorldSummaries.Count);
                Assert.True(result.WeirdestWorlds.Count > 0);

                // Generate reports
                MonteCarloReportGenerator.GenerateReports(result, testOutputDir);

                string summaryPath = Path.Combine(testOutputDir, "summary.html");
                string aggregatePath = Path.Combine(testOutputDir, "aggregate.json");
                string weirdestPath = Path.Combine(testOutputDir, "weirdest_worlds.md");

                Assert.True(File.Exists(summaryPath));
                Assert.True(File.Exists(aggregatePath));
                Assert.True(File.Exists(weirdestPath));

                string summaryContent = File.ReadAllText(summaryPath);
                Assert.Contains("Monte Carlo Diagnostic Summary", summaryContent);
                Assert.Contains("Human Handoff Assessment", summaryContent);

                string aggregateContent = File.ReadAllText(aggregatePath);
                Assert.Contains("AverageGoalsPerMatch", aggregateContent);

                string weirdestContent = File.ReadAllText(weirdestPath);
                Assert.Contains("Weirdest Worlds Report", weirdestContent);
            }
            finally
            {
                if (Directory.Exists(testOutputDir))
                {
                    Directory.Delete(testOutputDir, recursive: true);
                }
            }
        }
    }
}
