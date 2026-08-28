# Historical OpenFootball Ingestion Coverage Report

Report Generated: 2026-08-28 18:19:45 UTC

## Executive Summary

- **Total Raw Records Processed:** 1743
- **Total Canonical Valid Matches:** 1743
- **Total Validation Issues/Gaps:** 9

## Environment Coverage Breakdown

| Environment | Competition | Valid Matches | Duplicates | Impossible Scores | Unresolved Clubs | Observed Data Status |
|---|---|---|---|---|---|---|
| Colombia | Categoría Primera A | 929 | 9 | 0 | 0 | ✅ Verified Observed Data |
| Argentina | Primera División | 324 | 0 | 0 | 0 | ✅ Verified Observed Data |
| Brazil | Série A | 350 | 0 | 0 | 0 | ✅ Verified Observed Data |
| CopaLibertadores | Copa Libertadores | 140 | 0 | 0 | 0 | ✅ Verified Observed Data |

## Data Provenance & Boundary Assurance

- **Data Provenance:** All ingested historical results are explicitly tagged with `ProvenanceSource.RealWorld`.
- **Synthetic Separation:** Simulation runs generate data with `ProvenanceSource.Synthetic`. Ingested observed data remains strictly segregated and read-only.

## Identified Data Gaps & Quality Issues

### Environment: Colombia
  - [Warning] Duplicate: Duplicate match detected on 2023-01-01: 20:20 Deportivo Pasto v Santa Fe vs (0-0)
  - [Warning] Duplicate: Duplicate match detected on 2023-01-01: 20:20 Boyacá Chicó v Independiente Medellín vs (1-0)
  - [Warning] Duplicate: Duplicate match detected on 2023-01-01: 20:20 Atlético Junior v Deportes Tolima vs (0-1)
  - [Warning] Duplicate: Duplicate match detected on 2023-01-01: 20:30 Deportivo Pasto v Atlético Junior vs (0-0)
  - [Warning] Duplicate: Duplicate match detected on 2023-01-01: 14:00 Envigado v Independiente Medellín vs (0-1)
  - [Warning] Duplicate: Duplicate match detected on 2023-01-01: 18:30 Atlético Junior v Once Caldas vs (1-0)
  - [Warning] Duplicate: Duplicate match detected on 2023-01-01: 16:00 Envigado v Deportivo Pasto vs (0-1)
  - [Warning] Duplicate: Duplicate match detected on 2023-01-01: 18:20 América de Cali v Alianza vs (0-0)
  - [Warning] Duplicate: Duplicate match detected on 2023-01-01: 19:00 Santa Fe v Independiente Medellín vs (0-1)

