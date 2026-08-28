# Club Metadata and Finance Enrichment

This layer enriches the canonical club identities produced by the results
pipeline. It is intentionally separate from match history and simulation
truth.

## Source policy

1. OpenFootball club/stadium records (public domain) for names, aliases,
   cities, founding dates and venues.
2. Wikidata for structured identity and Wikipedia sitelinks. Wikidata values
   and sitelinks are CC0; retain the QID as the identity bridge.
3. CC0 supplemental club/stadium datasets such as topbin where fields are
   missing.
4. Public-domain stadium datasets for capacity and coordinates where needed.

RSSSF, scraped Wikipedia pages, Transfermarkt, Soccerway and OpenStreetMap are
not canonical enrichment sources for V0.

## Canonical record shape

Each field is independently sourced; conflicting values are retained rather
than silently overwritten.

```json
{
  "club_id": "COL_BOG_SFE",
  "wikidata_id": "Q...",
  "names": {"display": "Independiente Santa Fe", "aliases": []},
  "location": {"country": "COL", "city": "Bogota"},
  "founded": {"value": 1941, "source": "wikidata", "confidence": 1.0},
  "stadium_id": "STAD_BOG_ELCAMPIN",
  "sitelinks": {
    "en": {"title": "...", "url": "..."},
    "es": {"title": "...", "url": "..."}
  },
  "field_sources": []
}
```

Wikipedia links come from Wikidata sitelinks, never constructed URL strings.
Store English when available and all relevant country-language pages that
exist. Country language preferences are configuration, not hard-coded club
logic.

## Stadium record

```json
{
  "stadium_id": "STAD_BOG_ELCAMPIN",
  "name": "Estadio El Campin",
  "city": "Bogota",
  "capacity": {"value": 36343, "year": 2024},
  "coordinates": {"latitude": 4.6457, "longitude": -74.0775},
  "field_sources": []
}
```

## Finance record

Financial values must state whether they are observed, derived or estimated.
Never present an estimate as an audited fact.

```json
{
  "club_id": "COL_BOG_SFE",
  "metric": "revenue",
  "value": 0,
  "currency": "COP",
  "fiscal_year": 2022,
  "source_type": "observed",
  "source": "supersociedades",
  "confidence": 1.0,
  "retrieved_at": "2026-08-28"
}
```

The estimator may derive revenue capacity, wage budget, transfer budget,
matchday income and commercial income from observed anchors, but every derived
value carries a model version and confidence. Missing values remain missing
until the estimator is explicitly run.

## Coverage report

Every enrichment run must report, by country:

- clubs resolved to Wikidata;
- clubs with city, founding date and stadium;
- stadiums with capacity and coordinates;
- clubs with English and local-language Wikipedia sitelinks;
- clubs with observed, partially observed and estimated finances;
- unresolved identity matches and conflicting source values.

No enrichment is considered complete until unresolved and conflicting records
are visible in the report.
