using System;
using FootballWorldLab.Core.Clock;
using FootballWorldLab.Core.Entities;
using FootballWorldLab.Core.Ids;
using FootballWorldLab.Core.Rng;
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
    }
}
