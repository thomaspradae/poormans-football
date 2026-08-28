using System.Collections.Immutable;
using FootballWorldLab.Core.Entities;
using FootballWorldLab.Core.Ids;

namespace FootballWorldLab.Core.State
{
    /// <summary>
    /// Represents the immutable state of the football world simulation.
    /// </summary>
    public sealed record WorldState
    {
        public ImmutableDictionary<StableId, Club> Clubs { get; init; } = ImmutableDictionary<StableId, Club>.Empty;
        public ImmutableDictionary<StableId, Player> Players { get; init; } = ImmutableDictionary<StableId, Player>.Empty;
        public ImmutableDictionary<StableId, Manager> Managers { get; init; } = ImmutableDictionary<StableId, Manager>.Empty;
        public ImmutableDictionary<StableId, Person> People { get; init; } = ImmutableDictionary<StableId, Person>.Empty;
        public ImmutableDictionary<StableId, City> Cities { get; init; } = ImmutableDictionary<StableId, City>.Empty;
        public ImmutableDictionary<StableId, Country> Countries { get; init; } = ImmutableDictionary<StableId, Country>.Empty;
        public ImmutableDictionary<StableId, Competition> Competitions { get; init; } = ImmutableDictionary<StableId, Competition>.Empty;
        public ImmutableDictionary<StableId, Season> Seasons { get; init; } = ImmutableDictionary<StableId, Season>.Empty;
        public ImmutableDictionary<StableId, Match> Matches { get; init; } = ImmutableDictionary<StableId, Match>.Empty;
        public ImmutableDictionary<StableId, SquadMembership> SquadMemberships { get; init; } = ImmutableDictionary<StableId, SquadMembership>.Empty;
        public ImmutableDictionary<StableId, Contract> Contracts { get; init; } = ImmutableDictionary<StableId, Contract>.Empty;

<<<<<<< ours
        public WorldState WithClub(Club club) => this with { Clubs = Clubs.SetOrAdd(club.Id, club) };
        public WorldState WithPlayer(Player player) => this with { Players = Players.SetOrAdd(player.Id, player) };
        public WorldState WithManager(Manager manager) => this with { Managers = Managers.SetOrAdd(manager.Id, manager) };
        public WorldState WithPerson(Person person) => this with { People = People.SetOrAdd(person.Id, person) };
        public WorldState WithCity(City city) => this with { Cities = Cities.SetOrAdd(city.Id, city) };
        public WorldState WithCountry(Country country) => this with { Countries = Countries.SetOrAdd(country.Id, country) };
        public WorldState WithCompetition(Competition competition) => this with { Competitions = Competitions.SetOrAdd(competition.Id, competition) };
        public WorldState WithSeason(Season season) => this with { Seasons = Seasons.SetOrAdd(season.Id, season) };
        public WorldState WithMatch(Match match) => this with { Matches = Matches.SetOrAdd(match.Id, match) };
        public WorldState WithSquadMembership(SquadMembership membership) => this with { SquadMemberships = SquadMemberships.SetOrAdd(membership.Id, membership) };
        public WorldState WithContract(Contract contract) => this with { Contracts = Contracts.SetOrAdd(contract.Id, contract) };
=======
        public WorldState WithClub(Club club) => this with { Clubs = Clubs.SetItem(club.Id, club) };
        public WorldState WithPlayer(Player player) => this with { Players = Players.SetItem(player.Id, player) };
        public WorldState WithManager(Manager manager) => this with { Managers = Managers.SetItem(manager.Id, manager) };
        public WorldState WithPerson(Person person) => this with { People = People.SetItem(person.Id, person) };
        public WorldState WithCity(City city) => this with { Cities = Cities.SetItem(city.Id, city) };
        public WorldState WithCountry(Country country) => this with { Countries = Countries.SetItem(country.Id, country) };
        public WorldState WithCompetition(Competition competition) => this with { Competitions = Competitions.SetItem(competition.Id, competition) };
        public WorldState WithSeason(Season season) => this with { Seasons = Seasons.SetItem(season.Id, season) };
        public WorldState WithMatch(Match match) => this with { Matches = Matches.SetItem(match.Id, match) };
        public WorldState WithSquadMembership(SquadMembership membership) => this with { SquadMemberships = SquadMemberships.SetItem(membership.Id, membership) };
        public WorldState WithContract(Contract contract) => this with { Contracts = Contracts.SetItem(contract.Id, contract) };
>>>>>>> theirs

        public WorldState RemoveClub(StableId clubId) => this with { Clubs = Clubs.Remove(clubId) };
        public WorldState RemovePlayer(StableId playerId) => this with { Players = Players.Remove(playerId) };
        public WorldState RemoveManager(StableId managerId) => this with { Managers = Managers.Remove(managerId) };
        public WorldState RemovePerson(StableId personId) => this with { People = People.Remove(personId) };
        public WorldState RemoveCity(StableId cityId) => this with { Cities = Cities.Remove(cityId) };
        public WorldState RemoveCountry(StableId countryId) => this with { Countries = Countries.Remove(countryId) };
        public WorldState RemoveCompetition(StableId competitionId) => this with { Competitions = Competitions.Remove(competitionId) };
        public WorldState RemoveSeason(StableId seasonId) => this with { Seasons = Seasons.Remove(seasonId) };
        public WorldState RemoveMatch(StableId matchId) => this with { Matches = Matches.Remove(matchId) };
        public WorldState RemoveSquadMembership(StableId membershipId) => this with { SquadMemberships = SquadMemberships.Remove(membershipId) };
        public WorldState RemoveContract(StableId contractId) => this with { Contracts = Contracts.Remove(contractId) };
    }
<<<<<<< ours
}
=======
}
>>>>>>> theirs
