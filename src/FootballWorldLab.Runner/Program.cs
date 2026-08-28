using System;
using System.IO;
using System.Linq;
<<<<<<< ours
using FootballWorldLab.Core.Persistence;
using FootballWorldLab.Core.Salience;
=======
using FootballWorldLab.Core.Analysis;
using FootballWorldLab.Core.Ids;
using FootballWorldLab.Core.Invariants;
using FootballWorldLab.Core.MonteCarlo;
using FootballWorldLab.Core.Persistence;
>>>>>>> theirs
using FootballWorldLab.Core.Simulation;

namespace FootballWorldLab.Runner
{
    internal class Program
    {
        private static void Main(string[] args)
        {
<<<<<<< ours
            if (args.Length == 0)
            {
                PrintUsage();
=======
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
>>>>>>> theirs
                return;
            }

            string command = args[0].ToLowerInvariant();

            switch (command)
            {
<<<<<<< ours
                case "simulate":
                    HandleSimulate(args);
                    break;
                case "inspect":
                    HandleInspect(args);
                    break;
                case "why":
                    HandleWhy(args);
                    break;
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    PrintUsage();
=======
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
>>>>>>> theirs
                    break;
            }
        }

<<<<<<< ours
        private static void PrintUsage()
        {
            Console.WriteLine("Football World Lab CLI");
            Console.WriteLine("Usage:");
            Console.WriteLine("  simulate [--years <N>] [--seed <S>] [--save <path>]");
            Console.WriteLine("  inspect [--load <path>]");
            Console.WriteLine("  why <thread-id|event-id> [--load <path>]");
        }

        private static void HandleSimulate(string[] args)
        {
            int years = 1;
            ulong seed = 42UL;
            string? savePath = null;

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "--years" && i + 1 < args.Length)
                {
                    int.TryParse(args[++i], out years);
                }
                else if (args[i] == "--seed" && i + 1 < args.Length)
                {
                    ulong.TryParse(args[++i], out seed);
                }
                else if (args[i] == "--save" && i + 1 < args.Length)
                {
                    savePath = args[++i];
                }
            }

            Console.WriteLine($"Starting simulation: {years} year(s), Seed={seed}...");
            var engine = new SimulationEngine(seed);
            engine.RunYears(years);

            string hash = SaveManager.ComputeCanonicalHash(engine);
            Console.WriteLine($"Simulation complete! Final Date: {engine.Clock.CurrentDate:yyyy-MM-dd}. Canonical Hash: {hash}");

            if (!string.IsNullOrEmpty(savePath))
            {
                SaveManager.SaveToFile(engine, savePath, years, seed);
                Console.WriteLine($"State saved to {savePath}");
            }
        }

        private static void HandleInspect(string[] args)
        {
            int years = 1;
            ulong seed = 42UL;
            string? savePath = GetArgValue(args, "--load");

            SimulationEngine engine;
            if (!string.IsNullOrEmpty(savePath) && File.Exists(savePath))
            {
                engine = SaveManager.ReloadAndRunContinuous(savePath);
            }
            else
            {
                engine = new SimulationEngine(seed);
                engine.RunYears(years);
            }

            Console.WriteLine("=== WORLD INSPECTION ===");
            Console.WriteLine($"Clock: {engine.Clock}");
            Console.WriteLine($"Total Clubs: {engine.State.Clubs.Count}");
            Console.WriteLine($"Total Players: {engine.State.Players.Count}");
            Console.WriteLine($"Total Events Recorded: {engine.EventLog.Count}");

            Console.WriteLine("\n--- DOMESTIC LEAGUE STANDINGS ---");
            foreach (var kvp in engine.DomesticStandings)
            {
                var comp = engine.State.Competitions[kvp.Key];
                Console.WriteLine($"\n[Competition: {comp.Name}]");
                Console.WriteLine("Pos | Club                      | Pld |  W |  D |  L |  GF |  GA |  GD | Pts");
                Console.WriteLine("------------------------------------------------------------------------");
                int pos = 1;
                foreach (var st in kvp.Value)
                {
                    Console.WriteLine($"{pos,3} | {st.ClubName,-25} | {st.Played,3} | {st.Won,2} | {st.Drawn,2} | {st.Lost,2} | {st.GoalsFor,3} | {st.GoalsAgainst,3} | {st.GoalDifference,3} | {st.Points,3}");
                    pos++;
                }
            }

            Console.WriteLine("\n--- TOP MANAGER CAREERS ---");
            foreach (var record in engine.ManagerCareers.Values.Take(5))
            {
                Console.WriteLine($"Manager: {record.Name} (Matches: {record.TotalMatches}, Wins: {record.TotalWins}, Trophies: {record.TotalTrophies}, Sacks: {record.TotalSacks})");
                foreach (var h in record.History)
                {
                    Console.WriteLine($"  - {h}");
                }
            }

            Console.WriteLine("\n--- TOP SALIENCE THREADS ---");
            var threads = SalienceEvaluator.ClusterThreads(engine.EventLog, engine.State);
            foreach (var t in threads.Take(5))
            {
                Console.WriteLine($"Thread ID: {t.Id.Value} | Title: {t.Title} (Salience: {t.HighestSalience:F1})");
            }
        }

        private static void HandleWhy(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: why <thread-id|event-id> [--load <path>]");
                return;
            }

            string targetId = args[1];
            string? savePath = GetArgValue(args, "--load");

            SimulationEngine engine;
            if (!string.IsNullOrEmpty(savePath) && File.Exists(savePath))
            {
                engine = SaveManager.ReloadAndRunContinuous(savePath);
            }
            else
            {
                engine = new SimulationEngine(42UL);
                engine.RunYears(1);
            }

            var threads = SalienceEvaluator.ClusterThreads(engine.EventLog, engine.State);
            var targetThread = threads.FirstOrDefault(t => t.Id.Value.Equals(targetId, StringComparison.OrdinalIgnoreCase)
                || t.Events.Any(e => e.EventId.Value.Equals(targetId, StringComparison.OrdinalIgnoreCase)));

            if (targetThread == null)
            {
                Console.WriteLine($"Causal explanation not found for ID: {targetId}");
                Console.WriteLine("Available top thread IDs:");
                foreach (var t in threads.Take(5))
                {
                    Console.WriteLine($"  - {t.Id.Value}");
                }
                return;
            }

            Console.WriteLine($"=== CAUSAL EXPLANATION: {targetThread.Title} ===");
            Console.WriteLine($"Thread ID: {targetThread.Id.Value}");
            Console.WriteLine($"Salience Score: {targetThread.HighestSalience:F1}");
            Console.WriteLine("\nCausal Explanations:");
            foreach (var exp in targetThread.Explanations)
            {
                Console.WriteLine($"  * {exp}");
            }

            Console.WriteLine("\nAssociated Events:");
            foreach (var ev in targetThread.Events)
            {
                Console.WriteLine($"  - [{ev.GetType().Name}] Tick {ev.Tick} (EventId: {ev.EventId.Value}) Source: {ev.Provenance.Source}");
                Console.WriteLine($"    Description: {ev.Provenance.Description}");
            }
        }

        private static string? GetArgValue(string[] args, string flag)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == flag) return args[i + 1];
            }
            return null;
=======
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
            engine.RunYears(years);

            InvariantChecker.Validate(engine.State, engine);
            Console.WriteLine($"Simulation complete. Current year: {engine.Clock.CurrentYear}. Total matches: {engine.State.Matches.Count(m => m.Value.Played)}");

            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
            string savePath = Path.Combine(outDir, "world_save.json");
            SaveManager.SaveToFile(engine.State, savePath);
            Console.WriteLine($"Saved state to '{savePath}'");
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
>>>>>>> theirs
        }
    }
}
