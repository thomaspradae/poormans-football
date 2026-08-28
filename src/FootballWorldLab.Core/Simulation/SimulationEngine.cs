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
using FootballWorldLab.Core.Salience;
using FootballWorldLab.Core.State;

namespace FootballWorldLab.Core.Simulation
{
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

        public SimulationEngine(ulong seed = 42UL, int startYear = 2024)
        {
            Rng = new SeededRandom(seed);
            Clock = new SimulationClock(startYear, 1, 1);
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
            }
        }
    }
}
