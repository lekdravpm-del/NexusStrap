using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NexusStrap.Schema
{
    public sealed record SettingsDefaultDefinition(string Name, JsonNode DefaultValue);

    public static class SettingsSchemaImporter
    {
        private const long MaximumSchemaBytes = 4L * 1024 * 1024;
        private const int MaximumDefinitions = 10_000;
        private static readonly Regex PropertyExpression = new(
            @"^\s*public\s+(?<type>[A-Za-z_][A-Za-z0-9_?.]*(?:<[^;{}]+>)?)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\{\s*get;\s*set;\s*\}(?:\s*=\s*(?<initializer>[^;]+))?;",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline);

        public static string DefaultSchemaPath => Path.Combine(AppContext.BaseDirectory, "Catalog", "AppSettings.cs");

        public static async Task<IReadOnlyCollection<SettingsDefaultDefinition>> LoadAsync(string? schemaPath = null, CancellationToken cancellationToken = default)
        {
            string path = string.IsNullOrWhiteSpace(schemaPath) ? DefaultSchemaPath : schemaPath!;
            if (!File.Exists(path)) return Array.Empty<SettingsDefaultDefinition>();

            if (new FileInfo(path).Length > MaximumSchemaBytes) return Array.Empty<SettingsDefaultDefinition>();

            string source = await File.ReadAllTextAsync(path, cancellationToken);
            return Parse(source);
        }

        public static IReadOnlyCollection<SettingsDefaultDefinition> Parse(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return Array.Empty<SettingsDefaultDefinition>();

            return PropertyExpression.Matches(source)
                .Take(MaximumDefinitions)
                .Select(m => TryCreateDefinition(m, out var def) ? def : null)
                .Where(d => d != null)
                .Cast<SettingsDefaultDefinition>()
                .GroupBy(d => d.Name, StringComparer.Ordinal)
                .Select(g => g.First())
                .ToArray();
        }

        private static bool TryCreateDefinition(Match match, out SettingsDefaultDefinition? definition)
        {
            string type = match.Groups["type"].Value;
            string name = match.Groups["name"].Value;
            string initializer = match.Groups["initializer"].Success ? match.Groups["initializer"].Value.Trim() : string.Empty;

            if (!TryCreateDefaultValue(type, initializer, out JsonNode? value) || value is null)
            {
                definition = null;
                return false;
            }

            definition = new SettingsDefaultDefinition(name, value);
            return true;
        }

        private static bool TryCreateDefaultValue(string type, string initializer, out JsonNode? value)
        {
            string normalized = type.TrimEnd('?');
            switch (normalized)
            {
                case "bool":
                    value = JsonValue.Create(string.Equals(initializer, "true", StringComparison.OrdinalIgnoreCase));
                    return true;
                case "int":
                    value = JsonValue.Create(int.TryParse(initializer, out int i) ? i : 0);
                    return true;
                case "long":
                    value = JsonValue.Create(long.TryParse(initializer, out long l) ? l : 0L);
                    return true;
                case "double":
                    value = JsonValue.Create(double.TryParse(initializer, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double d) ? d : 0D);
                    return true;
                case "decimal":
                    value = JsonValue.Create(decimal.TryParse(initializer, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out decimal dec) ? dec : 0M);
                    return true;
                case "string":
                    if (string.Equals(initializer, "string.Empty", StringComparison.Ordinal))
                    {
                        value = JsonValue.Create(string.Empty);
                        return true;
                    }
                    if (TryParseString(initializer, out string s))
                    {
                        value = JsonValue.Create(s);
                        return true;
                    }
                    value = null;
                    return false;
            }

            value = null;
            return false;
        }

        private static bool TryParseString(string initializer, out string value)
        {
            if (initializer.Length >= 2 && initializer[0] == '"' && initializer[^1] == '"')
            {
                try
                {
                    value = Regex.Unescape(initializer[1..^1]);
                }
                catch
                {
                    value = initializer[1..^1];
                }
                return true;
            }
            value = string.Empty;
            return false;
        }
    }
}
