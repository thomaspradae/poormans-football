using System;
using System.IO;
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
        }
    }
}
