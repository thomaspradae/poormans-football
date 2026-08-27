# V0 Scope, Limitations, and Data Boundaries

## Scope

Football World Lab V0 provides a deterministic, inspectable, headless laboratory for simulating alternate 20–30 year football histories initially covering South American environments:
- **Colombia** (Categoría Primera A)
- **Argentina** (Primera División)
- **Brazil** (Série A)
- **Copa Libertadores** (Continental competition)

## Data Boundaries

1. **Observed Historical Data**: Real historical match results, standings, and team records ingested from openfootball datasets. Stored read-only.
2. **Derived Calibration**: Elo ratings, team baseline strengths, home advantage parameters, K-factors, and prestige indices derived from historical records.
3. **Synthetic Simulation History**: Generated outcomes, contracts, player careers, match events, dynamic Elo shifts, and causal provenance produced deterministically during simulation runs.

## Limitations in V0

- Match outcomes are modeled via expected-goals/Poisson approximations with fatigue, upsets, and substitutions, rather than 2D/3D physics.
- Financial modeling is bounded and simplified (budgets, wages, transfer fees, debt limits).
- Cognitive models for players and managers use compact attributes, sparse memory decay, and basic utility actions.
- No LLM runtime calls, paid APIs, or non-deterministic external dependencies.
