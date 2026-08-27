# Football World Lab V0

Canonical build specification for a deterministic, inspectable, headless football-world laboratory.

## Mission

Simulate alternate 20–30 year football histories initially covering Colombia, Argentina, Brazil, and Libertadores-style continental competition. Outcomes must be statistically plausible, persistent, diverse, causally explainable, and reproducible from a seed.

`world state → event → rules/effects → state, memories, beliefs, relationships → decisions → events`

Simulation truth is separate from presentation. No LLM, Unity gameplay, paid API, or scripted storyline belongs in the canonical simulator.

## Engineering contract

- Portable C# core and simulation libraries; no Unity dependency.
- .NET CLI runner, SQLite/JSON persistence, Python ingestion/analysis.
- Stable IDs; immutable, versioned typed events; deterministic seeded RNG.
- Every important state change carries provenance and is queryable with a `why` command.
- Small composable rules/utilities; centralized tunable parameters.
- Observed historical data, derived calibration, and synthetic future data remain distinct.
- Tests and invariants run at every milestone; clean checkouts build and run.
- Raw datasets, caches, and generated runs are excluded from version control.

## Milestones

1. **Boundaries and data** — create/update README, ARCHITECTURE, DECISIONS, PROGRESS, TASKS, V0 scope/limitations/data-boundary docs; ingest openfootball South America results for the three countries and Libertadores with source manifests, aliases, coverage, gaps, and validation.
2. **Domain/provenance** — stable-ID Country, City, Club, Competition, Season, Match, Person, Player, Manager, SquadMembership, Contract; contribution ledger and explainable state history.
3. **Calibration/economy** — deterministic Elo (home advantage, K, regression), strength/prestige descriptors, expectations/surprise, bounded synthetic finances, investment, sales, debt and invariant tests.
4. **People/cognition** — synthetic players/managers; compact football attributes, personality, drives, dynamic state, jobs/careers; sparse relationships, decaying memories, non-omniscient beliefs, perception/attention, utility actions, and simple inspectable learned associations.
5. **Time/events/effects** — deterministic day/week/season/year clock; immutable typed match, trophy, contract, transfer, playing-time, injury, manager, youth, retirement, and finance events; modular rules with instant, decaying, persistent, memory, and scheduled effects.
6. **Football/competitions** — crude deterministic expected-goals/Poisson-like matches with draws, home advantage, upsets, minutes, fatigue, substitutions and low injury probability; configurable domestic and simplified continental league/group/knockout/final stages.
7. **Season/career loops** — preseason through fixtures, standings, continental play, transfers, managers, finances, development, youth and retirement; age curves, late bloomers, decline, failed negotiations and contextual manager expectations.
8. **Salience/persistence/inspection** — silent-to-historic salience using stakes, surprise, novelty, relationships and repetition; causal thread clustering; versioned save/reload including RNG state; CLI `simulate`, `inspect`, `why`, `history`, `player`, `club`, `competition`, `world-stats`, `monte-carlo`; static debug inspector.
9. **Diagnostics/emergence** — match, competition, player, economy, manager and social distributions; invariant/fuzz/stress/sensitivity/historical sanity/performance tests; deterministic Monte Carlo (10×10, 50×20, 100×30, and 1,000×30 where feasible); detect but never force dynasties, collapses, Cinderella rises, manager ascents, academy cohorts, export booms and career anomalies; produce structured causal explanations and `summary.html`, `aggregate.json`, `weirdest_worlds.md`.

## Definition of done

A clean checkout builds; ingestion is reproducible; the four initial football environments exist; clubs are calibrated; synthetic people, finances, competitions, matches, transfers, youth, managers, relationships, memories, beliefs, drives, learning, provenance, salience, threads, persistence, inspection, Monte Carlo, diagnostics, emergence detection, causal explanations, determinism, invariants, stress tests, and reports all work. Limitations are documented honestly. Unity work never derails the laboratory.

## Human handoff

Document how to run one world, 100 worlds, open reports, change parameters, and inspect causality. End reports with:

1. Do these worlds feel like football histories?
2. Are surprising outcomes believable after inspecting causal chains?
3. Is the simulation too stable or chaotic?
4. Which generated world/story is most compelling?
5. Which behavior feels fake?
6. Which subsystem should V1 deepen first?

