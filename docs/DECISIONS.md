# Architectural Decision Records (ADRs)

## ADR-001: Separation of Headless C# Core from Presentation Layer

- **Status**: Decided
- **Context**: The simulation must be able to run millions of matches across 20–30 year histories headlessly, deterministically, and fast without rendering overhead.
- **Decision**: Put all simulation logic into `FootballWorldLab.Core` using standard C# (.NET 8). Unity will act purely as a consumer/presentation layer when visual play is needed.
- **Consequences**: No `UnityEngine` references allowed inside `FootballWorldLab.Core`. High portability and fast CLI execution.

## ADR-002: Deterministic Random Seed Provider

- **Status**: Decided
- **Context**: Simulation runs must produce identical statistical and event-by-event outputs when given the same initial seed.
- **Decision**: Implement a custom wrapper around deterministic random generation (`RandomGenerator` / `SeededRandom`) allowing branching child streams per subsystem while preserving master seed state.
- **Consequences**: Avoid global `System.Random.Shared` or `UnityEngine.Random` calls in domain logic.

## ADR-003: Discrete Step-Based Simulation Clock

- **Status**: Decided
- **Context**: Time advancement must be predictable, serializable, and inspectable across days, weeks, seasons, and years.
- **Decision**: Model `SimulationClock` with explicit step methods (`AdvanceDays`, `AdvanceWeeks`, etc.) tracking current `DateTime`, season year, and total step count.
- **Consequences**: Avoid system clocks (`DateTime.Now`). All temporal logic relies on `SimulationClock`.

## ADR-004: Stable Identifiers for Core Entities

- **Status**: Decided
- **Context**: Domain entities must retain persistent identity across serialization, runs, and exports.
- **Decision**: Define strong value types (`StableId` or typed IDs) that can be generated deterministically from seed/namespace or stored.
- **Consequences**: Enables reliable cross-referencing and causal provenance tracking.
