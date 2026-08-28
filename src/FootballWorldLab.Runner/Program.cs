using System;
using System.IO;
using System.Linq;
using FootballWorldLab.Core.Analysis;
using FootballWorldLab.Core.Ids;
using FootballWorldLab.Core.Invariants;
using FootballWorldLab.Core.MonteCarlo;
using FootballWorldLab.Core.Persistence;
using FootballWorldLab.Core.Simulation;

namespace FootballWorldLab.Runner
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("=== Football World Lab V0 CLI ===");

            if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
            {
                if (args.Length == 0)
                {
                    // Default behavior: execute 100 worlds x 30 years Monte Carlo
                    RunMonteCarlo(100, 30, "reports");
                    return;
                }

                PrintHelp();
                return;
            }

            string command = args[0].ToLowerInvariant();

            switch (command)
            {
                case "monte-carlo":
                    int mcWorlds = GetIntOption(args, "--worlds", 100);
                    int mcYears = GetIntOption(args, "--years", 30);
                    string mcOut = GetStringOption(args, "--out", "reports");
                    RunMonteCarlo(mcWorlds, mcYears, mcOut);
                    break;

                case "simulate":
                    int simWorlds = GetIntOption(args, "--worlds", 1);
                    int simYears = GetIntOption(args, "--years", 10);
                    ulong simSeed = (ulong)GetIntOption(args, "--seed", 42);
                    string simOut = GetStringOption(args, "--out", "reports");

                    if (simWorlds > 1)
                    {
                        RunMonteCarlo(simWorlds, simYears, simOut, simSeed);
                    }
                    else
                    {
                        RunSingleSimulation(simYears, simSeed, simOut);
                    }
                    break;

                case "inspect":
                    string saveFile = GetStringOption(args, "--file", "world_save.json");
                    InspectSaveFile(saveFile);
                    break;

                case "why":
                    string entityKey = GetStringOption(args, "--entity", "Club:C1");
                    ExplainEntity(entityKey);
                    break;

                case "world-stats":
                    RunWorldStats();
                    break;

                default:
                    Console.WriteLine($"Unknown command: '{command}'");
                    PrintHelp();
                    break;
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run --project src/FootballWorldLab.Runner -- [command] [options]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  monte-carlo [--worlds N] [--years Y] [--out DIR] Run Monte Carlo simulation & generate reports.");
            Console.WriteLine("  simulate    [--worlds N] [--years Y] [--seed S] [--out DIR] Run simulation run(s).");
            Console.WriteLine("  inspect     [--file PATH]                       Inspect a saved WorldState JSON file.");
            Console.WriteLine("  why         [--entity ID]                       Generate structured causal explanation.");
            Console.WriteLine("  world-stats                                     Display baseline world statistics.");
            Console.WriteLine();
        }

        private static void RunMonteCarlo(int worlds, int years, string outDir, ulong baseSeed = 10000UL)
        {
            Console.WriteLine($"Running Monte Carlo simulation: {worlds} worlds x {years} years...");
            var result = MonteCarloRunner.Run(worlds, years, baseSeed);
            Console.WriteLine($"Monte Carlo completed in {result.ElapsedTime.TotalSeconds:F2} seconds.");

            MonteCarloReportGenerator.GenerateReports(result, outDir);
            Console.WriteLine($"Reports generated in '{outDir}':");
            Console.WriteLine($"  - {Path.Combine(outDir, "summary.html")}");
            Console.WriteLine($"  - {Path.Combine(outDir, "aggregate.json")}");
            Console.WriteLine($"  - {Path.Combine(outDir, "weirdest_worlds.md")}");
        }

        private static void RunSingleSimulation(int years, ulong seed, string outDir)
        {
            Console.WriteLine($"Running single simulation for {years} years (seed: {seed})...");
            var engine = new SimulationEngine(seed);
            engine.InitializeDefaultWorld(clubsPerLeague: 10, leagues: 1);

            // Run years and step matches
            for (int y = 0; y < years; y++)
            {
                engine.StepSeason();
            }

            int playedMatches = engine.EventHistory.OfType<FootballWorldLab.Core.Events.MatchCompletedEvent>().Count();
            int totalMatchesInState = engine.State.Matches.Count(m => m.Value.Played);

            InvariantChecker.Validate(engine.State, engine);
            Console.WriteLine($"Simulation complete. Current year: {engine.Clock.CurrentYear}. Played matches: {playedMatches}");

            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
            string savePath = Path.Combine(outDir, "world_save.json");
            SaveManager.SaveToFile(engine.State, savePath);
            Console.WriteLine($"Saved state to '{savePath}'");

            // Verify round-trip loading
            var reloadedState = SaveManager.LoadFromFile(savePath);
            Console.WriteLine($"Successfully reloaded state with {reloadedState.Clubs.Count} clubs and {reloadedState.Players.Count} players.");
        }

        private static void InspectSaveFile(string saveFile)
        {
            if (!File.Exists(saveFile))
            {
                Console.WriteLine($"Save file '{saveFile}' not found. Running baseline sample inspection...");
                var engine = new SimulationEngine(42UL);
                engine.InitializeDefaultWorld();
                engine.RunYears(5);
                PrintStateStats(engine.State);
                return;
            }

            var state = SaveManager.LoadFromFile(saveFile);
            Console.WriteLine($"Loaded save file '{saveFile}':");
            PrintStateStats(state);
        }

        private static void PrintStateStats(FootballWorldLab.Core.State.WorldState state)
        {
            Console.WriteLine($"  Clubs: {state.Clubs.Count}");
            Console.WriteLine($"  Players: {state.Players.Count}");
            Console.WriteLine($"  Competitions: {state.Competitions.Count}");
            Console.WriteLine($"  Contracts: {state.Contracts.Count}");
            if (state.Clubs.Count > 0)
            {
                double maxElo = state.Clubs.Values.Max(c => c.RatingElo);
                double minElo = state.Clubs.Values.Min(c => c.RatingElo);
                Console.WriteLine($"  Elo Range: {minElo:F1} - {maxElo:F1}");
            }
        }

        private static void ExplainEntity(string entityKey)
        {
            var engine = new SimulationEngine(42UL);
            engine.InitializeDefaultWorld();
            engine.RunYears(10);

            var stableId = new StableId(entityKey);
            var explanation = CausalExplainer.ExplainEntity(engine, stableId);

            Console.WriteLine($"Structured Causal Explanation for '{entityKey}':");
            Console.WriteLine($"Primary Conclusion: {explanation.PrimaryConclusion}");
            Console.WriteLine("Causal Thread:");
            foreach (var step in explanation.ChainOfEvents)
            {
                Console.WriteLine($"  [Tick {step.Tick}] {step.Summary} (Salience: {step.Salience:F2})");
            }
        }

        private static void RunWorldStats()
        {
            var engine = new SimulationEngine(1001UL);
            engine.InitializeDefaultWorld(clubsPerLeague: 10, leagues: 1);
            engine.RunYears(10);

            Console.WriteLine("World Baseline Statistics:");
            PrintStateStats(engine.State);
            var phenomena = EmergenceDetector.DetectEmergence(engine);
            Console.WriteLine($"Emergent Phenomena Detected: {phenomena.Count}");
            foreach (var p in phenomena.Take(5))
            {
                Console.WriteLine($"  [{p.Type}] {p.Description}");
            }
        }

        private static int GetIntOption(string[] args, string flag, int defaultValue)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals(flag, StringComparison.OrdinalIgnoreCase) && int.TryParse(args[i + 1], out int val))
                {
                    return val;
                }
            }
            return defaultValue;
        }

        private static string GetStringOption(string[] args, string flag, string defaultValue)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals(flag, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }
            return defaultValue;
        }
    }
}
