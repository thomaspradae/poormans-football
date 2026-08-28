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
<<<<<<< ours
=======
using FootballWorldLab.Core.Salience;
>>>>>>> theirs
using FootballWorldLab.Core.State;

namespace FootballWorldLab.Core.Simulation
{
<<<<<<< ours
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
=======
    public sealed class SimulationEngine
    {
        public WorldState State { get; private set; }

        public void UpdateState(WorldState newState)
        {
            State = newState;
        }
        public SimulationClock Clock { get; }
        public SeededRandom Rng { get; }
        public List<BaseEvent> EventHistory { get; } = new List<BaseEvent>();
        
        // Dictionary tracking event ID -> parent/cause event ID
        public Dictionary<StableId, StableId?> CausalLinks { get; } = new Dictionary<StableId, StableId?>();
        // Map event ID -> salience score
        public Dictionary<StableId, double> EventSalience { get; } = new Dictionary<StableId, double>();
>>>>>>> theirs

        public SimulationEngine(ulong seed = 42UL, int startYear = 2024)
        {
            Rng = new SeededRandom(seed);
            Clock = new SimulationClock(startYear, 1, 1);
<<<<<<< ours
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
=======
            State = new WorldState();
        }

        public SimulationEngine(WorldState initialState, SimulationClock clock, SeededRandom rng)
        {
            State = initialState;
            Clock = clock;
            Rng = rng;
        }

        public void InitializeDefaultWorld(int clubsPerLeague = 10, int leagues = 2)
        {
            var country = new Country(StableId.Create("Country", "COL"), "Colombia", "COL");
            State = State.WithCountry(country);

            string[] leagueNames = new[] { "Liga Dimayor Primera A", "Liga Dimayor Primera B" };
            
            int totalClubs = 0;
            for (int l = 0; l < Math.Min(leagues, leagueNames.Length); l++)
            {
                var compId = StableId.Create("Competition", $"L{l + 1}");
                var competition = new Competition(compId, country.Id, leagueNames[l], "League");
                State = State.WithCompetition(competition);

                var seasonId = StableId.Create("Season", $"Comp-{l + 1}-{Clock.SeasonStartYear}");
                var season = new Season(seasonId, compId, Clock.SeasonStartYear, Clock.CurrentDate, Clock.CurrentDate.AddMonths(11));
                State = State.WithSeason(season);

                List<Club> leagueClubs = new List<Club>();
                for (int i = 0; i < clubsPerLeague; i++)
                {
                    totalClubs++;
                    string clubName = $"Club {totalClubs}";
                    if (l == 0)
                    {
                        string[] famousNames = { "Millonarios", "Atlético Nacional", "América de Cali", "Junior", "Santa Fe", "Deportivo Cali", "Independiente Medellín", "Once Caldas", "Tolima", "Bucaramanga" };
                        if (i < famousNames.Length) clubName = famousNames[i];
                    }

                    double initialElo = 1500.0 + (clubsPerLeague - i) * 15.0 - (l * 100.0) + (Rng.NextDouble() * 30.0 - 15.0);
                    var cityId = StableId.Create("City", $"CITY-{totalClubs}");
                    var city = new City(cityId, country.Id, $"{clubName} City");
                    State = State.WithCity(city);

                    var clubId = StableId.Create("Club", $"C{totalClubs}");
                    var club = new Club(clubId, cityId, clubName, clubName.Substring(0, Math.Min(3, clubName.Length)).ToUpperInvariant(), initialElo);
                    State = State.WithClub(club);
                    leagueClubs.Add(club);

                    // Create Manager
                    var mgrPersonId = StableId.Create("Person", $"MGR-P-{totalClubs}");
                    var mgrPerson = new Person(mgrPersonId, country.Id, "Manager", $"{clubName}", Clock.CurrentDate.AddYears(-Rng.NextInt(35, 65)));
                    State = State.WithPerson(mgrPerson);

                    var mgrId = StableId.Create("Manager", $"MGR-{totalClubs}");
                    var mgr = new Manager(mgrId, mgrPersonId, Rng.NextInt(60, 85), Rng.NextInt(60, 85));
                    State = State.WithManager(mgr);

                    // Create Players & Squad
                    for (int p = 1; p <= 18; p++)
                    {
                        string pos = p switch
                        {
                            1 or 2 => "GK",
                            >= 3 and <= 7 => "DEF",
                            >= 8 and <= 13 => "MID",
                            _ => "FWD"
                        };
                        int age = Rng.NextInt(18, 33);
                        int overall = (int)Math.Clamp(initialElo / 20.0 + Rng.NextInt(-8, 8), 55, 88);
                        int potential = Math.Clamp(overall + Rng.NextInt(0, 10), overall, 92);

                        var pPersonId = StableId.Create("Person", $"P-{totalClubs}-{p}");
                        var pPerson = new Person(pPersonId, country.Id, $"Player", $"{totalClubs}-{p}", Clock.CurrentDate.AddYears(-age));
                        State = State.WithPerson(pPerson);

                        var playerId = StableId.Create("Player", $"PLY-{totalClubs}-{p}");
                        var player = new Player(playerId, pPersonId, pos, overall, potential);
                        State = State.WithPlayer(player);

                        var squadId = StableId.Create("Squad", $"SQ-{totalClubs}-{p}");
                        var squad = new SquadMembership(squadId, clubId, playerId, p, true);
                        State = State.WithSquadMembership(squad);

                        var contractId = StableId.Create("Contract", $"CON-{totalClubs}-{p}");
                        var contract = new Contract(contractId, clubId, pPersonId, (decimal)(overall * 100), Clock.CurrentDate, Clock.CurrentDate.AddYears(Rng.NextInt(1, 4)));
                        State = State.WithContract(contract);
                    }
                }

                // Generate Fixtures for Season
                GenerateLeagueFixtures(season.Id, leagueClubs);
            }
        }

        private void GenerateLeagueFixtures(StableId seasonId, List<Club> clubs)
        {
            int n = clubs.Count;
            int matchCounter = 0;
            DateTime matchDate = Clock.CurrentDate.AddDays(7);

            for (int round = 0; round < (n - 1) * 2; round++)
            {
                for (int i = 0; i < n / 2; i++)
                {
                    int homeIdx = (round + i) % (n - 1);
                    int awayIdx = (n - 1 - i + round) % (n - 1);
                    if (i == 0) awayIdx = n - 1;

                    if (round % 2 == 1)
                    {
                        int temp = homeIdx;
                        homeIdx = awayIdx;
                        awayIdx = temp;
                    }

                    matchCounter++;
                    var matchId = StableId.Create("Match", $"{seasonId.Value}-M{matchCounter}");
                    var match = new Match(matchId, seasonId, clubs[homeIdx].Id, clubs[awayIdx].Id, matchDate, 0, 0, false);
                    State = State.WithMatch(match);
                }
                matchDate = matchDate.AddDays(7);
            }
        }

        public void StepWeek()
        {
            Clock.StepWeek(1);
            long tick = Clock.TotalTicks;

            // Find matches scheduled on or before current date that haven't been played
            var unplayed = State.Matches.Values
                .Where(m => !m.Played && m.Date <= Clock.CurrentDate)
                .OrderBy(m => m.Date)
                .ToList();

            foreach (var match in unplayed)
            {
                SimulateMatch(match, tick);
            }
        }

        public void SimulateMatch(Match match, long tick)
        {
            if (!State.Clubs.TryGetValue(match.HomeClubId, out var homeClub) ||
                !State.Clubs.TryGetValue(match.AwayClubId, out var awayClub))
                return;

            // Expected goals calculation based on Elo
            double eloDiff = homeClub.RatingElo + 40.0 - awayClub.RatingElo; // +40 home advantage
            double expectedHomeGoals = Math.Clamp(1.35 * Math.Pow(10, eloDiff / 400.0), 0.2, 4.5);
            double expectedAwayGoals = Math.Clamp(1.10 * Math.Pow(10, -eloDiff / 400.0), 0.1, 4.0);

            int homeGoals = SamplePoisson(expectedHomeGoals);
            int awayGoals = SamplePoisson(expectedAwayGoals);

            // Update Match State
            var updatedMatch = match with { HomeGoals = homeGoals, AwayGoals = awayGoals, Played = true };
            State = State.WithMatch(updatedMatch);

            // Update Elo
            double kFactor = 32.0;
            double actualHomeScore = homeGoals > awayGoals ? 1.0 : (homeGoals == awayGoals ? 0.5 : 0.0);
            double expectedHomeScore = 1.0 / (1.0 + Math.Pow(10, (awayClub.RatingElo - (homeClub.RatingElo + 40.0)) / 400.0));

            double newHomeElo = homeClub.RatingElo + kFactor * (actualHomeScore - expectedHomeScore);
            double newAwayElo = awayClub.RatingElo + kFactor * ((1.0 - actualHomeScore) - (1.0 - expectedHomeScore));

            State = State.WithClub(homeClub with { RatingElo = Math.Clamp(newHomeElo, 500.0, 3000.0) });
            State = State.WithClub(awayClub with { RatingElo = Math.Clamp(newAwayElo, 500.0, 3000.0) });

            // Record Event & Salience
            var evtId = StableId.Create("Event", $"Match-{match.Id.Value}");
            var provenance = new ProvenanceInfo(ProvenanceSource.Synthetic, "Simulated match result");
            var matchEvt = new MatchCompletedEvent(evtId, tick, provenance, match.Id, homeClub.Id, awayClub.Id, homeGoals, awayGoals);
            
            double salience = SalienceEvaluator.EvaluateMatchSalience(matchEvt, State, homeClub.RatingElo, awayClub.RatingElo);
            
            RecordEvent(matchEvt, null, salience);
        }

        private int SamplePoisson(double lambda)
        {
            double L = Math.Exp(-lambda);
            double k = 0;
            double p = 1.0;
            do
            {
                k++;
                p *= Rng.NextDouble();
            } while (p > L && k < 12);
            return (int)(k - 1);
        }

        public void RecordEvent(BaseEvent evt, StableId? parentEventId = null, double salience = 0.1)
        {
            EventHistory.Add(evt);
            CausalLinks[evt.EventId] = parentEventId;
            EventSalience[evt.EventId] = salience;
        }

        public void StepSeason()
        {
            // Simulate remaining unplayed matches in current season
            while (State.Matches.Values.Any(m => !m.Played))
            {
                StepWeek();
            }

            // End-of-season processing: aging, transfers, manager evaluations, next season generation
            ProcessOffseason();
        }

        private void ProcessOffseason()
        {
            long tick = Clock.TotalTicks;

            // 1. Player aging and attribute development / decline
            foreach (var player in State.Players.Values.ToList())
            {
                if (!State.People.TryGetValue(player.PersonId, out var person)) continue;
                
                int currentAge = Clock.CurrentYear - person.BirthDate.Year;
                int newAge = currentAge + 1;

                int delta = 0;
                if (newAge <= 23)
                {
                    delta = Rng.NextInt(1, 4);
                }
                else if (newAge >= 30)
                {
                    delta = -Rng.NextInt(1, 4);
                }
                else
                {
                    delta = Rng.NextInt(-1, 2);
                }

                int newOverall = Math.Clamp(player.OverallRating + delta, 40, player.Potential);
                var updatedPlayer = player with { OverallRating = newOverall };
                State = State.WithPlayer(updatedPlayer);

                var ageEvtId = StableId.Create("Event", $"Age-{player.Id.Value}-{Clock.SeasonStartYear}");
                var ageEvt = new PlayerAgedEvent(ageEvtId, tick, new ProvenanceInfo(ProvenanceSource.Synthetic, "Annual age step"), player.Id, newAge);
                RecordEvent(ageEvt, null, 0.05);
            }

            // 2. Transfers (simple transfer simulation)
            var topClubs = State.Clubs.Values.OrderByDescending(c => c.RatingElo).Take(3).ToList();
            var bottomClubs = State.Clubs.Values.OrderBy(c => c.RatingElo).Take(3).ToList();

            if (topClubs.Count > 0 && bottomClubs.Count > 0 && Rng.NextBool(0.6))
            {
                var sellerClub = bottomClubs[Rng.NextInt(bottomClubs.Count)];
                var buyerClub = topClubs[Rng.NextInt(topClubs.Count)];

                var sellerSquad = State.SquadMemberships.Values
                    .Where(s => s.ClubId == sellerClub.Id && s.IsActive)
                    .ToList();

                if (sellerSquad.Count > 11)
                {
                    var squadMember = sellerSquad[Rng.NextInt(sellerSquad.Count)];
                    if (State.Players.TryGetValue(squadMember.PlayerId, out var player))
                    {
                        // Transfer player
                        var newSquadMember = squadMember with { ClubId = buyerClub.Id };
                        State = State.WithSquadMembership(newSquadMember);

                        decimal fee = (decimal)(player.OverallRating * 50_000);
                        var transferEvtId = StableId.Create("Event", $"Trf-{player.Id.Value}-{Clock.SeasonStartYear}");
                        var transferEvt = new PlayerTransferredEvent(
                            transferEvtId,
                            tick,
                            new ProvenanceInfo(ProvenanceSource.Synthetic, "Offseason transfer"),
                            player.Id,
                            sellerClub.Id,
                            buyerClub.Id,
                            fee);

                        double salience = SalienceEvaluator.EvaluateTransferSalience(transferEvt, player.OverallRating, fee);
                        RecordEvent(transferEvt, null, salience);
                    }
                }
            }

            // 3. Prepare next season
            Clock.AdvanceToNextSeason();

            // Clear old played matches and regenerate fixtures for next season
            var matchesToRemove = State.Matches.Keys.ToList();
            foreach (var key in matchesToRemove)
            {
                State = State.RemoveMatch(key);
            }

            foreach (var competition in State.Competitions.Values)
            {
                var newSeasonId = StableId.Create("Season", $"{competition.Id.Value}-{Clock.SeasonStartYear}");
                var newSeason = new Season(newSeasonId, competition.Id, Clock.SeasonStartYear, Clock.CurrentDate, Clock.CurrentDate.AddMonths(11));
                State = State.WithSeason(newSeason);

                var compClubs = State.Clubs.Values.ToList(); // Simple multi-club list
                GenerateLeagueFixtures(newSeasonId, compClubs);
            }
        }

        public void RunYears(int years)
        {
            if (State.Clubs.Count == 0)
            {
                InitializeDefaultWorld();
            }

            for (int y = 0; y < years; y++)
            {
                StepSeason();
>>>>>>> theirs
            }
        }
    }
}
