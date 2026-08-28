using System;
using System.Security.Cryptography;
using System.Text;
<<<<<<< ours
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FootballWorldLab.Core.Ids
{
    public class StableIdJsonConverter : JsonConverter<StableId>
    {
        public override StableId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? val = reader.GetString();
            return string.IsNullOrEmpty(val) ? default : new StableId(val);
        }

        public override void Write(Utf8JsonWriter writer, StableId value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }

        public override StableId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? val = reader.GetString();
            return string.IsNullOrEmpty(val) ? default : new StableId(val);
        }

        public override void WriteAsPropertyName(Utf8JsonWriter writer, StableId value, JsonSerializerOptions options)
        {
            writer.WritePropertyName(value.Value ?? string.Empty);
        }
    }

    /// <summary>
    /// Represents a stable, deterministic identifier for domain entities.
    /// </summary>
    [JsonConverter(typeof(StableIdJsonConverter))]
=======

namespace FootballWorldLab.Core.Ids
{
    /// <summary>
    /// Represents a stable, deterministic identifier for domain entities.
    /// </summary>
>>>>>>> theirs
    public readonly struct StableId : IEquatable<StableId>, IComparable<StableId>
    {
        public string Value { get; }

        public StableId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Id value cannot be null or empty.", nameof(value));

            Value = value;
        }

        public static StableId Create(string entityType, string key)
        {
            return new StableId($"{entityType}:{key}");
        }

        public static StableId CreateDeterministic(string entityType, string namespaceKey, long sequence)
        {
            string raw = $"{entityType}:{namespaceKey}:{sequence}";
            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(raw));
            string shortHash = Convert.ToHexString(hash, 0, 8).ToLowerInvariant();
            return new StableId($"{entityType}-{shortHash}");
        }

        public bool Equals(StableId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object? obj) => obj is StableId other && Equals(other);

        public override int GetHashCode() => Value != null ? StringComparer.Ordinal.GetHashCode(Value) : 0;

        public int CompareTo(StableId other) => string.Compare(Value, other.Value, StringComparison.Ordinal);

        public override string ToString() => Value ?? string.Empty;

        public static bool operator ==(StableId left, StableId right) => left.Equals(right);

        public static bool operator !=(StableId left, StableId right) => !left.Equals(right);
    }
}
