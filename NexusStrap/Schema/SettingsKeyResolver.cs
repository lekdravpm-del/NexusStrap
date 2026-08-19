using System;
using System.Collections.Generic;

namespace NexusStrap.Schema
{
    public static class SettingsKeyResolver
    {
        public static string NormalizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;
            return key.Trim().Replace(" ", "_").ToLowerInvariant();
        }

        public static bool TryResolve(string key, IReadOnlyCollection<SettingsDefaultDefinition> definitions, out SettingsDefaultDefinition? definition)
        {
            string normalized = NormalizeKey(key);
            foreach (var def in definitions)
            {
                if (string.Equals(NormalizeKey(def.Name), normalized, StringComparison.OrdinalIgnoreCase))
                {
                    definition = def;
                    return true;
                }
            }
            definition = null;
            return false;
        }
    }
}
