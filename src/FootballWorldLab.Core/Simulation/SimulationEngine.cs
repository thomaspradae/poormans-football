using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FootballWorldLab.Core.Clock;
using FootballWorldLab.Core.Entities;
using FootballWorldLab.Core.Events;
using FootballWorldLab.Core.Ids;
using FootballWorldLab.Core.Provenance;
using FootballWorldLab.Core.Rng;
using FootballWorldLab.Core.State;

namespace FootballWorldLab.Core.Simulation
{
    public sealed class ManagerCareerRecord
    {
        public StableId ManagerId { get; set; }
        public StableId PersonId { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> History { get; set; } = new();
        public int TotalMatches { get; set; }
        public int TotalWins { get; set; }
        public int TotalTrophies { get; set; }
        public int TotalSacks { get; set; }
    }

    public sealed class ClubStanding
    {
        public StableId ClubId { get; set; }
        public string ClubName { get; set; } = string.Empty;
        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int GoalDifference => GoalsFor - GoalsAgainst;
        public int Points => Won * 3 + Drawn;
    }

    public sealed class SimulationEngine
    {
        public WorldState State { get; private set; } = new WorldState();
        public SimulationClock Clock { get; private set; }
        public SeededRandom Rng { get; private set; }

        public List<BaseEvent> EventLog { get; private set; } = new();
        public Dictionary<StableId, ManagerCareerRecord> ManagerCareers { get; private set; } = new();
        public Dictionary<StableId, List<ClubStanding>> DomesticStandings { get; private set; } = new();

        public SimulationEngine(ulong seed = 42UL, int startYear = 2024)
        {
            Rng = new SeededRandom(seed);
            Clock = new SimulationClock(startYear, 1, 1);
            InitializeWorld();
        }

        private void InitializeWorld()
        {
            var col = new Country(StableId.Create("Country", "COL"), "Colombia", "COL");
            var arg = new Country(StableId.Create("Country", "ARG"), "Argentina", "ARG");
            var bra = new Country(StableId.Create("Country", "BRA"), "Brazil", "BRA");

            State = State.WithCountry(col).WithCountry(arg).WithCountry(bra);

            var cityBog = new City(StableId.Create("City", "BOG"), col.Id, "Bogotá");
            var cityMed = new City(StableId.Create("City", "MED"), col.Id, "Medellín");
            var cityBue = new City(StableId.Create("City", "BUE"), arg.Id, "Buenos Aires");
            var cityRos = new City(StableId.Create("City", "ROS"), arg.Id, "Rosario");
            var cityRio = new City(StableId.Create("City", "RIO"), bra.Id, "Rio de Janeiro");
            var citySao = new City(StableId.Create("City", "SAO"), bra.Id, "São Paulo");

            State = State.WithCity(cityBog).WithCity(cityMed)
                         .WithCity(cityBue).WithCity(cityRos)
                         .WithCity(cityRio).WithCity(citySao);

            var compCol = new Competition(StableId.Create("Competition", "COL_LIGA"), col.Id, "Liga BetPlay", "League");
            var compArg = new Competition(StableId.Create("Competition", "ARG_LIGA"), arg.Id, "Liga Profesional", "League");
            var compBra = new Competition(StableId.Create("Competition", "BRA_LIGA"), bra.Id, "Brasileirão", "League");
            var compLib = new Competition(StableId.Create("Competition", "COPA_LIB"), StableId.Create("Country", "SA"), "Copa Libertadores", "Continental");

            State = State.WithCompetition(compCol).WithCompetition(compArg)
                         .WithCompetition(compBra).WithCompetition(compLib);

            CreateClubsForCountry(col, cityBog, cityMed, new[] { "Millonarios", "Santa Fe", "Atlético Nacional", "DIM" }, compCol.Id, 1600.0);
            CreateClubsForCountry(arg, cityBue, cityRos, new[] { "Boca Juniors", "River Plate", "Racing Club", "Independiente" }, compArg.Id, 1750.0);
            CreateClubsForCountry(bra, cityRio, citySao, new[] { "Flamengo", "Palmeiras", "São Paulo", "Fluminense" }, compBra.Id, 1780.0);
        }

        private void CreateClubsForCountry(Country country, City city1, City city2, string[] clubNames, StableId compId, double baseElo)
        {
            for (int i = 0; i < clubNames.Length; i++)
            {
                var city = (i % 2 == 0) ? city1 : city2;
                var clubId = StableId.Create("Club", clubNames[i].Replace(" ", ""));
                double elo = baseElo + Rng.NextInt(-50, 50);
                var club = new Club(clubId, city.Id, clubNames[i], clubNames[i], elo);
                State = State.WithClub(club);

                // Create Manager
                var mPersonId = StableId.Create("Person", $"M_{clubNames[i]}");
                var mPerson = new Person(mPersonId, country.Id, "Manager", clubNames[i], new DateTime(1975, 1, 1));
                var mId = StableId.Create("Manager", $"M_{clubNames[i]}");
                var manager = new Manager(mId, mPerson.Id, Rng.NextInt(60, 90), Rng.NextInt(60, 90));

                State = State.WithPerson(mPerson).WithManager(manager);

                var contract = new Contract(StableId.Create("Contract", $"M_{clubNames[i]}"), club.Id, mPerson.Id, 5000m, Clock.CurrentDate, Clock.CurrentDate.AddYears(2));
                State = State.WithContract(contract);

                ManagerCareers[mId] = new ManagerCareerRecord
                {
                    ManagerId = mId,
                    PersonId = mPerson.Id,
                    Name = $"{mPerson.FirstName} {mPerson.LastName}",
                    History = new List<string> { $"{Clock.CurrentYear}: Appointed at {club.Name}" }
                };

                // Create Squad (11 Players)
                for (int p = 1; p <= 11; p++)
                {
                    var pPersonId = StableId.Create("Person", $"P_{clubNames[i]}_{p}");
                    var pPerson = new Person(pPersonId, country.Id, "Player", $"{clubNames[i]}_{p}", new DateTime(2000 - (p % 10), 5, 10));
                    var pId = StableId.Create("Player", $"P_{clubNames[i]}_{p}");
                    var player = new Player(pId, pPerson.Id, p <= 2 ? "GK" : (p <= 5 ? "DEF" : (p <= 8 ? "MID" : "FWD")), Rng.NextInt(65, 85), Rng.NextInt(75, 90));

                    State = State.WithPerson(pPerson).WithPlayer(player);

                    var membership = new SquadMembership(StableId.Create("Squad", $"P_{clubNames[i]}_{p}"), club.Id, pId, p, true);
                    State = State.WithSquadMembership(membership);
                }
            }
        }

        public void RunYears(int years)
        {
            for (int y = 0; y < years; y++)
            {
                RunSeason();
            }
        }

        public void RunSeason()
        {
            int year = Clock.CurrentYear;

            // 1. Domestic Leagues
            var domesticComps = State.Competitions.Values.Where(c => c.Format == "League").ToList();
            var topClubsPerCountry = new List<Club>();

            foreach (var comp in domesticComps)
            {
                var countryClubs = State.Clubs.Values.Where(c => {
                    var city = State.Cities[c.CityId];
                    return city.CountryId == comp.CountryId;
                }).ToList();

                var standings = SimulateLeagueSeason(comp, countryClubs);
                DomesticStandings[comp.Id] = standings;

                if (standings.Count > 0)
                {
                    var champion = State.Clubs[standings[0].ClubId];
                    var trophyEvent = new TrophyWonEvent(
                        StableId.Create("Event", $"Trophy_{comp.Id.Value}_{year}"),
                        Clock.TotalTicks,
                        new ProvenanceInfo(ProvenanceSource.Derived, $"{champion.Name} won {comp.Name}"),
                        comp.Id, champion.Id, year);
                    EventLog.Add(trophyEvent);

                    // Top 2 qualify for Libertadores
                    topClubsPerCountry.Add(champion);
                    if (standings.Count > 1)
                    {
                        topClubsPerCountry.Add(State.Clubs[standings[1].ClubId]);
                    }
                }

                // Evaluate Managers for domestic clubs
                EvaluateManagers(standings);
            }

            // 2. Copa Libertadores (Continental)
            var libComp = State.Competitions.Values.FirstOrDefault(c => c.Format == "Continental");
            if (libComp != null && topClubsPerCountry.Count >= 2)
            {
                SimulateContinentalTournament(libComp, topClubsPerCountry);
            }

            // 3. Player Aging & Youth
            UpdatePlayersAndSquads();

            // Advance Clock Year
            Clock.AdvanceToNextSeason();
        }

        private List<ClubStanding> SimulateLeagueSeason(Competition comp, List<Club> clubs)
        {
            var standings = clubs.Select(c => new ClubStanding { ClubId = c.Id, ClubName = c.Name }).ToDictionary(s => s.ClubId);

            // Double round robin
            for (int i = 0; i < clubs.Count; i++)
            {
                for (int j = 0; j < clubs.Count; j++)
                {
                    if (i == j) continue;
                    var home = clubs[i];
                    var away = clubs[j];

                    SimulateMatch(comp.Id, home, away, standings);
                    Clock.StepDay(3);
                }
            }

            return standings.Values
                .OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.GoalDifference)
                .ThenByDescending(s => s.GoalsFor)
                .ToList();
        }

        private void SimulateContinentalTournament(Competition libComp, List<Club> qualifiedClubs)
        {
            var standings = qualifiedClubs.Select(c => new ClubStanding { ClubId = c.Id, ClubName = c.Name }).ToDictionary(s => s.ClubId);

            // Group stage simulation
            for (int i = 0; i < qualifiedClubs.Count; i++)
            {
                for (int j = i + 1; j < qualifiedClubs.Count; j++)
                {
                    SimulateMatch(libComp.Id, qualifiedClubs[i], qualifiedClubs[j], standings);
                    SimulateMatch(libComp.Id, qualifiedClubs[j], qualifiedClubs[i], standings);
                }
            }

            var ranked = standings.Values.OrderByDescending(s => s.Points).ThenByDescending(s => s.GoalDifference).ToList();
            if (ranked.Count >= 2)
            {
                // Final match between top 2
                var home = State.Clubs[ranked[0].ClubId];
                var away = State.Clubs[ranked[1].ClubId];

                int homeGoals = Rng.NextInt(0, 4) + (home.RatingElo > away.RatingElo ? 1 : 0);
                int awayGoals = Rng.NextInt(0, 4);
                if (homeGoals == awayGoals) homeGoals++; // Final decider

                var winner = homeGoals > awayGoals ? home : away;
                var trophyEvent = new TrophyWonEvent(
                    StableId.Create("Event", $"Trophy_LIB_{Clock.CurrentYear}"),
                    Clock.TotalTicks,
                    new ProvenanceInfo(ProvenanceSource.Derived, $"{winner.Name} won Copa Libertadores"),
                    libComp.Id, winner.Id, Clock.CurrentYear);
                EventLog.Add(trophyEvent);

                if (ManagerCareers.TryGetValue(winner.Id, out var mgrRec))
                {
                    mgrRec.TotalTrophies++;
                }
            }
        }

        private void SimulateMatch(StableId compId, Club home, Club away, Dictionary<StableId, ClubStanding> standings)
        {
            // Elo expected goals calculation
            double eloDiff = home.RatingElo + 50.0 - away.RatingElo; // 50 home advantage
            double expectedHome = 1.2 + (eloDiff / 400.0);
            double expectedAway = 1.0 - (eloDiff / 400.0);

            int homeGoals = Math.Max(0, (int)Math.Round(expectedHome + (Rng.NextDouble() * 2.5 - 1.25)));
            int awayGoals = Math.Max(0, (int)Math.Round(expectedAway + (Rng.NextDouble() * 2.5 - 1.25)));

            var matchId = StableId.CreateDeterministic("Match", compId.Value, Clock.TotalTicks);
            var matchEvent = new MatchCompletedEvent(
                matchId,
                Clock.TotalTicks,
                new ProvenanceInfo(ProvenanceSource.Derived, $"{home.Name} {homeGoals}-{awayGoals} {away.Name}"),
                matchId, home.Id, away.Id, homeGoals, awayGoals);

            EventLog.Add(matchEvent);

            var hStand = standings[home.Id];
            var aStand = standings[away.Id];

            hStand.Played++;
            aStand.Played++;
            hStand.GoalsFor += homeGoals;
            hStand.GoalsAgainst += awayGoals;
            aStand.GoalsFor += awayGoals;
            aStand.GoalsAgainst += homeGoals;

            if (homeGoals > awayGoals)
            {
                hStand.Won++;
                aStand.Lost++;
            }
            else if (awayGoals > homeGoals)
            {
                aStand.Won++;
                hStand.Lost++;
            }
            else
            {
                hStand.Drawn++;
                aStand.Drawn++;
            }

            // Update Elo slightly
            double k = 20.0;
            double actualHome = homeGoals > awayGoals ? 1.0 : (homeGoals == awayGoals ? 0.5 : 0.0);
            double winProbHome = 1.0 / (1.0 + Math.Pow(10, (away.RatingElo - (home.RatingElo + 50.0)) / 400.0));
            double newHomeElo = home.RatingElo + k * (actualHome - winProbHome);
            double newAwayElo = away.RatingElo - k * (actualHome - winProbHome);

            State = State.WithClub(home with { RatingElo = newHomeElo })
                         .WithClub(away with { RatingElo = newAwayElo });
        }

        private void EvaluateManagers(List<ClubStanding> standings)
        {
            for (int i = 0; i < standings.Count; i++)
            {
                var st = standings[i];
                var club = State.Clubs[st.ClubId];
                var contract = State.Contracts.Values.FirstOrDefault(c => c.ClubId == club.Id);
                if (contract == null) continue;

                var manager = State.Managers.Values.FirstOrDefault(m => m.PersonId == contract.PersonId);
                if (manager == null) continue;

                if (!ManagerCareers.TryGetValue(manager.Id, out var record))
                {
                    record = new ManagerCareerRecord { ManagerId = manager.Id, PersonId = manager.PersonId };
                    ManagerCareers[manager.Id] = record;
                }

                record.TotalMatches += st.Played;
                record.TotalWins += st.Won;

                // Sack manager if finished last and Elo was expected higher
                if (i == standings.Count - 1 && club.RatingElo > 1550.0)
                {
                    record.TotalSacks++;
                    record.History.Add($"{Clock.CurrentYear}: Sacked by {club.Name} due to poor performance");

                    var sackEvent = new ManagerSackedEvent(
                        StableId.Create("Event", $"Sack_{manager.Id.Value}_{Clock.CurrentYear}"),
                        Clock.TotalTicks,
                        new ProvenanceInfo(ProvenanceSource.Derived, $"{record.Name} sacked by {club.Name}"),
                        manager.Id, club.Id, "Finished last place");
                    EventLog.Add(sackEvent);

                    // Hire new replacement manager
                    var city = State.Cities[club.CityId];
                    var newMPersonId = StableId.Create("Person", $"M_New_{club.Name}_{Clock.CurrentYear}");
                    var newMPerson = new Person(newMPersonId, city.CountryId, "Manager", $"New_{Clock.CurrentYear}", new DateTime(1980, 1, 1));
                    var newMId = StableId.Create("Manager", $"M_New_{club.Name}_{Clock.CurrentYear}");
                    var newManager = new Manager(newMId, newMPerson.Id, Rng.NextInt(65, 85), Rng.NextInt(65, 85));

                    State = State.WithPerson(newMPerson).WithManager(newManager);
                    State = State.RemoveContract(contract.Id);

                    var newContract = new Contract(StableId.Create("Contract", $"M_New_{club.Name}_{Clock.CurrentYear}"), club.Id, newMPerson.Id, 6000m, Clock.CurrentDate, Clock.CurrentDate.AddYears(2));
                    State = State.WithContract(newContract);

                    var newRecord = new ManagerCareerRecord
                    {
                        ManagerId = newMId,
                        PersonId = newMPerson.Id,
                        Name = $"{newMPerson.FirstName} {newMPerson.LastName}",
                        History = new List<string> { $"{Clock.CurrentYear}: Appointed at {club.Name} replacing sacked manager" }
                    };
                    ManagerCareers[newMId] = newRecord;

                    var hireEvent = new ManagerHiredEvent(
                        StableId.Create("Event", $"Hire_{newMId.Value}_{Clock.CurrentYear}"),
                        Clock.TotalTicks,
                        new ProvenanceInfo(ProvenanceSource.Derived, $"{newRecord.Name} hired by {club.Name}"),
                        newMId, club.Id);
                    EventLog.Add(hireEvent);
                }
            }
        }

        private void UpdatePlayersAndSquads()
        {
            foreach (var player in State.Players.Values.ToList())
            {
                var person = State.People[player.PersonId];
                int age = Clock.CurrentYear - person.BirthDate.Year;

                // Player progression / regression
                int ratingDelta = age < 25 ? Rng.NextInt(1, 4) : (age > 30 ? Rng.NextInt(-3, 0) : Rng.NextInt(-1, 2));
                int newRating = Math.Clamp(player.OverallRating + ratingDelta, 50, 95);

                State = State.WithPlayer(player with { OverallRating = newRating });

                var agedEvent = new PlayerAgedEvent(
                    StableId.Create("Event", $"Age_{player.Id.Value}_{Clock.CurrentYear}"),
                    Clock.TotalTicks,
                    new ProvenanceInfo(ProvenanceSource.Derived, $"Player {player.Id} aged to {age}"),
                    player.Id, age);
                EventLog.Add(agedEvent);
            }
        }
    }
}
