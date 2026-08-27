# Poor Man's Football — Football World Lab V0

A deterministic, inspectable, headless football simulation laboratory written in C# / .NET, paired with a Unity simulation & game environment.

## Project Structure

- `src/FootballWorldLab.Core/` — Headless C# Portable Core Library (deterministic clock, RNG, stable IDs, domain entities). Zero Unity dependency.
- `src/FootballWorldLab.Runner/` — .NET CLI runner application for executing simulations and inspections.
- `tests/FootballWorldLab.Core.Tests/` — Unit tests for the C# core library (RNG determinism, clock ticks, entities).
- `Assets/` — Unity simulation & presentation project.
- `docs/` — Canonical architecture, decisions, scope, tasks, and progress documentation.

## Building and Running (.NET Headless Core)

### Prerequisites
- .NET 8.0 SDK (or later)

### Build Solution
```bash
dotnet build
```

### Run Core Unit Tests
```bash
dotnet test
```

### Run Headless Simulator CLI
```bash
dotnet run --project src/FootballWorldLab.Runner
```

## Documentation

- [docs/FOOTBALL_WORLD_LAB_V0.md](docs/FOOTBALL_WORLD_LAB_V0.md) — Canonical Build Specification for V0
- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) — System Architecture & Component Design
- [docs/DECISIONS.md](docs/DECISIONS.md) — Architectural Decision Records (ADRs)
- [docs/PROGRESS.md](docs/PROGRESS.md) — Milestone Progress Tracker
- [docs/TASKS.md](docs/TASKS.md) — Task Breakdown & Backlog
- [docs/V0_SCOPE_LIMITATIONS_DATA_BOUNDARIES.md](docs/V0_SCOPE_LIMITATIONS_DATA_BOUNDARIES.md) — Scope, Data Boundaries & Calibration Plan
