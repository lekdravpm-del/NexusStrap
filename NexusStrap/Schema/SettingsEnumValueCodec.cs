using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace NexusStrap.Schema
{
    public static class SettingsEnumValueCodec
    {
        public static JsonNode EncodeEnum<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            return JsonValue.Create(value.ToString());
        }

        public static bool TryDecodeEnum<TEnum>(JsonNode node, out TEnum value) where TEnum : struct, Enum
        {
            value = default;
            if (node is null) return false;
            string? str = node.GetValue<string>();
            if (string.IsNullOrWhiteSpace(str)) return false;
            return Enum.TryParse<TEnum>(str, true, out value);
        }
    }
}
