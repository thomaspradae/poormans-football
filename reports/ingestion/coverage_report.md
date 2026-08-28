# Historical OpenFootball Ingestion Coverage Report

Report Generated: 2026-08-28 18:16:18 UTC

## Executive Summary

- **Total Raw Records Processed:** 1216
- **Total Canonical Valid Matches:** 1216
- **Total Validation Issues/Gaps:** 0

## Environment Coverage Breakdown

| Environment | Competition | Valid Matches | Duplicates | Impossible Scores | Unresolved Clubs | Observed Data Status |
|---|---|---|---|---|---|---|
| Colombia | Categoría Primera A | 402 | 0 | 0 | 0 | ✅ Verified Observed Data |
| Argentina | Primera División | 324 | 0 | 0 | 0 | ✅ Verified Observed Data |
| Brazil | Série A | 350 | 0 | 0 | 0 | ✅ Verified Observed Data |
| CopaLibertadores | Copa Libertadores | 140 | 0 | 0 | 0 | ✅ Verified Observed Data |

## Data Provenance & Boundary Assurance

- **Data Provenance:** All ingested historical results are explicitly tagged with `ProvenanceSource.RealWorld`.
- **Synthetic Separation:** Simulation runs generate data with `ProvenanceSource.Synthetic`. Ingested observed data remains strictly segregated and read-only.

## Identified Data Gaps & Quality Issues

No data gaps or validation errors were identified during ingestion.

