# System Architecture — Football World Lab V0

## Overview

The Football World Lab simulator is designed around a strict separation of simulation truth from presentation.

```
       ┌────────────────────────────────────────────────────────┐
       │                   FootballWorldLab.Runner              │
       │                   (.NET CLI CLI Tools)                 │
       └───────────────────────────┬────────────────────────────┘
                                   │
                                   ▼
       ┌────────────────────────────────────────────────────────┐
       │                    FootballWorldLab.Core               │
       │   ┌────────────────────────────────────────────────┐   │
       │   │  Deterministic Clock & Seeded RNG              │   │
       │   ├────────────────────────────────────────────────┤   │
       │   │  Stable IDs & Provenance Identification        │   │
       │   ├────────────────────────────────────────────────┤   │
       │   │  Core Domain Entities                          │   │
       │   │  (Country, City, Club, Competition, Season,    │   │
       │   │   Match, Person, Player, Manager,              │   │
       │   │   SquadMembership, Contract)                   │   │
       │   └────────────────────────────────────────────────┘   │
       └────────────────────────────────────────────────────────┘
```

## Key Architectural Principles

1. **Portable C# Core**: The simulation logic in `FootballWorldLab.Core` has zero runtime or compile-time dependency on Unity or external heavy frameworks.
2. **Determinism by Construction**:
   - Randomness is governed by an explicit `SeededRandom` provider.
   - Time is driven by a discrete `SimulationClock` (stepping by day, week, season, year).
   - Entities receive stable IDs (`StableId`) derived deterministically or stored persistently.
3. **Immutability & Provenance**: State changes generate provenance logs/events enabling `why` queries to explain any historical outcome.
4. **Clean Boundaries**: Observed historical datasets (openfootball South America), derived calibration rules, and synthetic simulation histories are maintained separately.
