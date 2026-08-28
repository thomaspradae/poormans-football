# Task Backlog

## Task List for V0 Foundation & Current Baseline Status

1. [x] **Documentation & Scope Definition**
   - Record V0 scope, architecture, decision ADRs, progress tracker, and task breakdown.
2. [x] **Headless .NET Solution Setup**
   - Create solution and projects (`FootballWorldLab.Core`, `FootballWorldLab.Runner`, `FootballWorldLab.Core.Tests`).
3. [x] **Deterministic Primitives**
   - Implement `SeededRandom` for reproducible random streams.
   - Implement `SimulationClock` for day/week/season/year progression.
   - Implement `StableId` generator for domain entity identity with custom `StableIdJsonConverter` supporting System.Text.Json property names.
4. [x] **Narrow Core Entities**
   - Define records/classes for `Country`, `City`, `Club`, `Competition`, `Season`, `Match`, `Person`, `Player`, `Manager`, `SquadMembership`, `Contract`.
5. [x] **Unit Testing & Verification**
   - Write comprehensive unit tests for RNG determinism, clock stepping, stable IDs, Save/Reload JSON persistence round-trips, invariants, and Monte Carlo report generation.
6. [x] **Headless Simulation Engine & Save/Reload Persistence**
   - Poisson expected-goals match simulator, Elo rating updates, season loop, SaveManager JSON persistence with StableId dictionary key support, and CLI commands (`simulate`, `monte-carlo`, `inspect`, `why`, `world-stats`).
7. [ ] **Dataset Ingestion Pipeline**
   - Setup parser & ingestion for openfootball Colombia, Argentina, Brazil, and Libertadores datasets.
8. [ ] **Advanced Cognition & Deep Economy**
   - Ingestion-driven real-world club calibration, player drives, dynamic relationships, non-omniscient beliefs, and bounded club investments.
