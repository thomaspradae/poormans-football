using System;
using System.IO;
<<<<<<< ours
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FootballWorldLab.Core.Simulation;

namespace FootballWorldLab.Core.Persistence
{
    public sealed class SaveStateData
    {
        public ulong MasterSeed { get; set; }
        public int CurrentYear { get; set; }
        public int CurrentMonth { get; set; }
        public int CurrentDay { get; set; }
        public long TotalTicks { get; set; }
        public int YearsSimulated { get; set; }
        public string WorldStateJson { get; set; } = string.Empty;
    }

    public static class SaveManager
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public static string ComputeCanonicalHash(SimulationEngine engine)
        {
            var sb = new StringBuilder();

            // Hash Clock State
            sb.Append($"Clock:{engine.Clock.CurrentYear}-{engine.Clock.CurrentMonth}-{engine.Clock.CurrentDay}:{engine.Clock.TotalTicks};");

            // Hash Clubs sorted deterministically
            foreach (var club in engine.State.Clubs.Values.OrderBy(c => c.Id.Value))
            {
                sb.Append($"Club:{club.Id.Value}:{club.Name}:{club.RatingElo:F4};");
            }

            // Hash Players sorted deterministically
            foreach (var player in engine.State.Players.Values.OrderBy(p => p.Id.Value))
            {
                sb.Append($"Player:{player.Id.Value}:{player.Position}:{player.OverallRating};");
            }

            // Hash Managers sorted deterministically
            foreach (var mgr in engine.State.Managers.Values.OrderBy(m => m.Id.Value))
            {
                sb.Append($"Mgr:{mgr.Id.Value}:{mgr.TacticalRating}:{mgr.DevelopmentRating};");
            }

            // Hash Manager Career Records sorted deterministically
            foreach (var kvp in engine.ManagerCareers.OrderBy(k => k.Key.Value))
            {
                sb.Append($"MgrCareer:{kvp.Key.Value}:{kvp.Value.TotalMatches}:{kvp.Value.TotalWins}:{kvp.Value.TotalTrophies}:{kvp.Value.TotalSacks};");
            }

            // Hash Domestic Standings sorted deterministically
            foreach (var compKvp in engine.DomesticStandings.OrderBy(k => k.Key.Value))
            {
                sb.Append($"StandingComp:{compKvp.Key.Value};");
                foreach (var st in compKvp.Value.OrderBy(s => s.ClubId.Value))
                {
                    sb.Append($"StandingRow:{st.ClubId.Value}:{st.Played}:{st.Points}:{st.GoalDifference};");
                }
            }

            // Hash Event Log summary
            sb.Append($"EventLogCount:{engine.EventLog.Count};");
            if (engine.EventLog.Count > 0)
            {
                var firstEv = engine.EventLog.First();
                var lastEv = engine.EventLog.Last();
                sb.Append($"FirstEv:{firstEv.EventId.Value}:{firstEv.Tick};LastEv:{lastEv.EventId.Value}:{lastEv.Tick};");
            }

            using var sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
            byte[] hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        public static void SaveToFile(SimulationEngine engine, string filepath, int yearsSimulated, ulong masterSeed)
        {
            var data = new SaveStateData
            {
                MasterSeed = masterSeed,
                CurrentYear = engine.Clock.CurrentYear,
                CurrentMonth = engine.Clock.CurrentMonth,
                CurrentDay = engine.Clock.CurrentDay,
                TotalTicks = engine.Clock.TotalTicks,
                YearsSimulated = yearsSimulated,
                WorldStateJson = JsonSerializer.Serialize(engine.State, JsonOptions)
            };

            string json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(filepath, json);
        }

        public static SimulationEngine ReloadAndRunContinuous(string filepath)
        {
            string json = File.ReadAllText(filepath);
            var data = JsonSerializer.Deserialize<SaveStateData>(json);
            if (data == null)
                throw new InvalidDataException("Failed to deserialize save state.");

            // Reconstruct engine from master seed and fast forward
            var engine = new SimulationEngine(data.MasterSeed);
            engine.RunYears(data.YearsSimulated);
            return engine;
=======
using System.Text.Json;
using FootballWorldLab.Core.State;

namespace FootballWorldLab.Core.Persistence
{
    public static class SaveManager
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public static string SerializeWorldState(WorldState state)
        {
            return JsonSerializer.Serialize(state, Options);
        }

        public static WorldState DeserializeWorldState(string json)
        {
            return JsonSerializer.Deserialize<WorldState>(json, Options) ?? new WorldState();
        }

        public static void SaveToFile(WorldState state, string filePath)
        {
            string? dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string json = SerializeWorldState(state);
            File.WriteAllText(filePath, json);
        }

        public static WorldState LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Save file not found at {filePath}", filePath);

            string json = File.ReadAllText(filePath);
            return DeserializeWorldState(json);
>>>>>>> theirs
        }
    }
}
