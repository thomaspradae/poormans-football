# Progress & Milestone Tracker

## Milestone 1: Boundaries, Data & V0 Foundation
- [x] Create project README and docs (`ARCHITECTURE.md`, `DECISIONS.md`, `PROGRESS.md`, `TASKS.md`, `V0_SCOPE_LIMITATIONS_DATA_BOUNDARIES.md`).
- [x] Establish C# .NET solution structure (`FootballWorldLab.sln`, `FootballWorldLab.Core`, `FootballWorldLab.Runner`, `FootballWorldLab.Core.Tests`).
- [x] Implement deterministic seeded RNG, stable IDs (with JSON property name converter), discrete simulation clock, and narrow domain entities.
- [ ] Ingest openfootball South America datasets (Colombia, Argentina, Brazil, Libertadores).

## Milestone 2: Domain & Provenance
- [x] Define core entity models (`Country`, `City`, `Club`, `Competition`, `Season`, `Match`, `Person`, `Player`, `Manager`, `SquadMembership`, `Contract`).
- [x] Contribution ledger & explainable state history (`why` command foundation & CausalExplainer).

## Milestone 3–9: Calibration, Economy, Cognition, Events, Football, Loops, Salience, Diagnostics
- [x] Bounded Elo & baseline financial models (M3 baseline)
- [ ] Player/Manager synthetic drives & deep cognition (M4 remaining)
- [x] Event loop & decay rules baseline (M5)
- [x] Expected-goals / Poisson match simulator (M6 baseline)
- [x] Preseason-to-postseason loop (M7 baseline)
- [x] CLI inspection, Save/Reload persistence, & causal thread clustering (M8 baseline)
- [x] Monte Carlo stress testing & anomaly report outputs (M9 baseline - 100x30 worlds runner & HTML/JSON/MD report generator)
