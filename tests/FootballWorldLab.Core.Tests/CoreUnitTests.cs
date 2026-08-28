using System;
using System.IO;
using System.Linq;
using FootballWorldLab.Core.Analysis;
using FootballWorldLab.Core.Clock;
using FootballWorldLab.Core.Entities;
using FootballWorldLab.Core.Events;
using FootballWorldLab.Core.Ids;
using FootballWorldLab.Core.Invariants;
using FootballWorldLab.Core.MonteCarlo;
using FootballWorldLab.Core.Persistence;
using FootballWorldLab.Core.Provenance;
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
        public void CoreEntities_All11Entities_InstantiateWithStableIdAndProvenance()
        {
            var prov = new ProvenanceInfo(ProvenanceSource.RealWorld, "Test Dataset");

            var countryId = StableId.Create("Country", "COL");
            var country = new Country(countryId, "Colombia", "COL", prov);
            Assert.Equal("Country:COL", country.Id.Value);
            Assert.Equal(ProvenanceSource.RealWorld, country.Provenance.Source);

            var cityId = StableId.Create("City", "BOG");
            var city = new City(cityId, country.Id, "Bogotá", prov);
            Assert.Equal("City:BOG", city.Id.Value);
            Assert.Equal(ProvenanceSource.RealWorld, city.Provenance.Source);

            var clubId = StableId.Create("Club", "MILL");
            var club = new Club(clubId, city.Id, "Millonarios FC", "Millonarios", 1650.0, prov);
            Assert.Equal("Club:MILL", club.Id.Value);
            Assert.Equal(1650.0, club.RatingElo);
            Assert.Equal(ProvenanceSource.RealWorld, club.Provenance.Source);

            var competitionId = StableId.Create("Competition", "COMP1");
            var competition = new Competition(competitionId, country.Id, "Liga Dimayor", "League", prov);
            Assert.Equal("Competition:COMP1", competition.Id.Value);

            var seasonId = StableId.Create("Season", "S1");
            var season = new Season(seasonId, competition.Id, 2024, new DateTime(2024, 1, 1), new DateTime(2024, 12, 31), prov);
            Assert.Equal("Season:S1", season.Id.Value);

            var matchId = StableId.Create("Match", "M1");
            var match = new Match(matchId, season.Id, club.Id, club.Id, DateTime.UtcNow, 2, 1, true, prov);
            Assert.Equal("Match:M1", match.Id.Value);

            var personId = StableId.Create("Person", "P1");
            var person = new Person(personId, country.Id, "Radamel", "Falcao", new DateTime(1986, 2, 10), prov);
            Assert.Equal("Person:P1", person.Id.Value);

            var playerId = StableId.Create("Player", "PLY1");
            var player = new Player(playerId, person.Id, "ST", 82, 85, prov);
            Assert.Equal("Player:PLY1", player.Id.Value);
            Assert.Equal("ST", player.Position);

            var managerId = StableId.Create("Manager", "MGR1");
            var manager = new Manager(managerId, person.Id, 80, 85, prov);
            Assert.Equal("Manager:MGR1", manager.Id.Value);

            var squadId = StableId.Create("SquadMembership", "SQ1");
            var squad = new SquadMembership(squadId, club.Id, player.Id, 9, true, prov);
            Assert.Equal("SquadMembership:SQ1", squad.Id.Value);

            var contractId = StableId.Create("Contract", "C1");
            var contract = new Contract(contractId, club.Id, person.Id, 25000m, new DateTime(2024, 1, 1), new DateTime(2025, 12, 31), prov);
            Assert.Equal("Contract:C1", contract.Id.Value);
            Assert.Equal(25000m, contract.WeeklyWage);
        }

        [Fact]
        public void StateContributionLedger_ReturnsContributionsInDeterministicOrder()
        {
            var entityId = StableId.Create("Club", "C1");
            var prov = new ProvenanceInfo(ProvenanceSource.Synthetic, "Test");

            var c1 = new StateContribution(StableId.Create("Contrib", "10-2"), 10, entityId, "RatingElo", 1500.0, 1510.0, StableId.Create("Event", "E2"), "Rule1", prov);
            var c2 = new StateContribution(StableId.Create("Contrib", "5-1"), 5, entityId, "RatingElo", 1490.0, 1500.0, StableId.Create("Event", "E1"), "Rule1", prov);
            var c3 = new StateContribution(StableId.Create("Contrib", "10-1"), 10, entityId, "RatingElo", 1500.0, 1505.0, StableId.Create("Event", "E2"), "Rule1", prov);

            var ledger = new StateContributionLedger(new[] { c1, c2, c3 });

            var queryResult = ledger.GetContributionsForEntity(entityId).ToList();

            Assert.Equal(3, queryResult.Count);
            Assert.Equal(5, queryResult[0].Tick);
            Assert.Equal(10, queryResult[1].Tick);
            Assert.Equal("Contrib:10-1", queryResult[1].ContributionId.Value);
            Assert.Equal(10, queryResult[2].Tick);
            Assert.Equal("Contrib:10-2", queryResult[2].ContributionId.Value);
        }

        [Fact]
        public void MultiYearSimulation_RecordsStateContributionsAndProvenanceAcrossYears()
        {
            var engine = new SimulationEngine(42UL);
            engine.InitializeDefaultWorld(clubsPerLeague: 4, leagues: 1);

            Assert.NotEmpty(engine.State.Ledger.Entries);

            int initialContribCount = engine.State.Ledger.Entries.Count;
            engine.RunYears(3);

            Assert.True(engine.State.Ledger.Entries.Count > initialContribCount);

            var clubId = engine.State.Clubs.Keys.First();
            var clubContribs = engine.State.Ledger.GetContributionsForProperty(clubId, "RatingElo").ToList();

            Assert.NotEmpty(clubContribs);
            for (int i = 0; i < clubContribs.Count - 1; i++)
            {
                Assert.True(clubContribs[i].Tick <= clubContribs[i + 1].Tick);
            }
        }

        [Fact]
        public void SaveAndReload_WorldState_SerializesAndDeserializesWithStableIdKeysAndLedger()
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
                Assert.Equal(engine.State.Ledger.Entries.Count, reloadedState.Ledger.Entries.Count);

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
            Assert.NotEmpty(explanation.StateContributions);
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
