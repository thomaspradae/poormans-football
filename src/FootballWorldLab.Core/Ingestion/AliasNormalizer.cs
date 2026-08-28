using System;
using System.Collections.Generic;
using FootballWorldLab.Core.Ids;

namespace FootballWorldLab.Core.Ingestion
{
    public sealed class AliasNormalizer
    {
        private readonly Dictionary<string, string> _aliases = new(StringComparer.OrdinalIgnoreCase);

        public AliasNormalizer(Dictionary<string, string>? customAliases = null)
        {
            if (customAliases != null)
            {
                foreach (var kvp in customAliases)
                {
                    _aliases[kvp.Key.Trim()] = kvp.Value.Trim();
                }
            }
        }

        public void RegisterAlias(string alias, string canonicalName)
        {
            if (!string.IsNullOrWhiteSpace(alias) && !string.IsNullOrWhiteSpace(canonicalName))
            {
                _aliases[alias.Trim()] = canonicalName.Trim();
            }
        }

        public bool TryResolve(string rawName, out string canonicalName, out StableId clubId)
        {
            if (string.IsNullOrWhiteSpace(rawName))
            {
                canonicalName = string.Empty;
                clubId = default;
                return false;
            }

            string clean = rawName.Trim();
            if (_aliases.TryGetValue(clean, out var resolved))
            {
                canonicalName = resolved;
                clubId = StableId.Create("Club", CleanIdKey(resolved));
                return true;
            }

            // Fallback heuristics: trim common prefixes/suffixes like "CA", "FC", "CD", "SD"
            string stripped = CleanClubName(clean);
            if (_aliases.TryGetValue(stripped, out resolved))
            {
                canonicalName = resolved;
                clubId = StableId.Create("Club", CleanIdKey(resolved));
                return true;
            }

            // If stripped is non-empty, use stripped as canonical
            if (!string.IsNullOrWhiteSpace(stripped) && stripped.Length >= 3)
            {
                canonicalName = stripped;
                clubId = StableId.Create("Club", CleanIdKey(stripped));
                return true;
            }

            canonicalName = clean;
            clubId = StableId.Create("Club", CleanIdKey(clean));
            return false;
        }

        private static string CleanClubName(string name)
        {
            string s = name.Trim();
            string[] tokens = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var kept = new List<string>();
            foreach (var t in tokens)
            {
                string upper = t.ToUpperInvariant().Replace(".", "");
                if (upper == "CA" || upper == "FC" || upper == "CD" || upper == "SD" || upper == "CR" || upper == "SE")
                    continue;
                kept.Add(t);
            }
            return kept.Count > 0 ? string.Join(" ", kept) : s;
        }

        private static string CleanIdKey(string canonicalName)
        {
            return canonicalName.Replace(" ", "").Replace("-", "").Replace(".", "").ToUpperInvariant();
        }
    }
}
