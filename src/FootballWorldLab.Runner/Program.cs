using System;
using System.IO;
using System.Linq;
using FootballWorldLab.Core.Persistence;
using FootballWorldLab.Core.Salience;
using FootballWorldLab.Core.Simulation;

namespace FootballWorldLab.Runner
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            string command = args[0].ToLowerInvariant();

            switch (command)
            {
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
                    break;
            }
        }

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
        }
    }
}
