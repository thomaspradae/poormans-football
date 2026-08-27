# Task Backlog

## Task List for V0 Milestone 1 & Foundation

1. [x] **Documentation & Scope Definition**
   - Record V0 scope, architecture, decision ADRs, progress tracker, and task breakdown.
2. [x] **Headless .NET Solution Setup**
   - Create solution and projects (`FootballWorldLab.Core`, `FootballWorldLab.Runner`, `FootballWorldLab.Core.Tests`).
3. [x] **Deterministic Primitives**
   - Implement `SeededRandom` for reproducible random streams.
   - Implement `SimulationClock` for day/week/season/year progression.
   - Implement `StableId` generator for domain entity identity.
4. [x] **Narrow Core Entities**
   - Define records/classes for `Country`, `City`, `Club`, `Competition`, `Season`, `Match`, `Person`, `Player`, `Manager`, `SquadMembership`, `Contract`.
5. [x] **Unit Testing & Verification**
   - Write comprehensive tests for RNG determinism, clock stepping, stable IDs, and core entity creation.
6. [ ] **Dataset Ingestion Pipeline**
   - Setup parser & ingestion for openfootball Colombia, Argentina, Brazil, and Libertadores datasets.
