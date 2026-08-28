using System;
using FootballWorldLab.Core.Ids;
using FootballWorldLab.Core.Provenance;

namespace FootballWorldLab.Core.Entities
{
    public sealed record Country(
        StableId Id,
        string Name,
        string Code,
        ProvenanceInfo Provenance = default);

    public sealed record City(
        StableId Id,
        StableId CountryId,
        string Name,
        ProvenanceInfo Provenance = default);

    public sealed record Club(
        StableId Id,
        StableId CityId,
        string Name,
        string ShortName,
        double RatingElo = 1500.0,
        ProvenanceInfo Provenance = default);

    public sealed record Competition(
        StableId Id,
        StableId CountryId,
        string Name,
        string Format,
        ProvenanceInfo Provenance = default);

    public sealed record Season(
        StableId Id,
        StableId CompetitionId,
        int Year,
        DateTime StartDate,
        DateTime EndDate,
        ProvenanceInfo Provenance = default);

    public sealed record Match(
        StableId Id,
        StableId SeasonId,
        StableId HomeClubId,
        StableId AwayClubId,
        DateTime Date,
        int HomeGoals = 0,
        int AwayGoals = 0,
        bool Played = false,
        ProvenanceInfo Provenance = default);

    public sealed record Person(
        StableId Id,
        StableId NationalityId,
        string FirstName,
        string LastName,
        DateTime BirthDate,
        ProvenanceInfo Provenance = default);

    public sealed record Player(
        StableId Id,
        StableId PersonId,
        string Position,
        int OverallRating,
        int Potential,
        ProvenanceInfo Provenance = default);

    public sealed record Manager(
        StableId Id,
        StableId PersonId,
        int TacticalRating,
        int DevelopmentRating,
        ProvenanceInfo Provenance = default);

    public sealed record SquadMembership(
        StableId Id,
        StableId ClubId,
        StableId PlayerId,
        int ShirtNumber,
        bool IsActive = true,
        ProvenanceInfo Provenance = default);

    public sealed record Contract(
        StableId Id,
        StableId ClubId,
        StableId PersonId,
        decimal WeeklyWage,
        DateTime StartDate,
        DateTime EndDate,
        ProvenanceInfo Provenance = default);
}
