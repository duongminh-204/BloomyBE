using System.Text.Json;
using System.Text.RegularExpressions;

namespace BloomyBE.Helpers
{
    public static class AIResponseParser
    {
        private static readonly Regex JsonBlockRegex = new(
            @"```json\s*([\s\S]*?)\s*```",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static (string VisibleText, JsonDocument? Metadata) SplitChatResponse(string raw)
        {
            var match = JsonBlockRegex.Match(raw);
            if (!match.Success)
                return (raw.Trim(), null);

            var visible = raw[..match.Index].Trim();
            try
            {
                var doc = JsonDocument.Parse(match.Groups[1].Value);
                return (visible, doc);
            }
            catch
            {
                return (raw.Trim(), null);
            }
        }

        public static T? ParseJson<T>(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return default;

            var json = raw.Trim();
            var match = JsonBlockRegex.Match(json);
            if (match.Success) json = match.Groups[1].Value.Trim();

            try
            {
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return default;
            }
        }

        public static Dictionary<string, object?> MergeRequirements(
            string existingJson,
            Dictionary<string, object?>? incoming)
        {
            var merged = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(existingJson))
            {
                try
                {
                    using var doc = JsonDocument.Parse(existingJson);
                    foreach (var prop in doc.RootElement.EnumerateObject())
                        merged[prop.Name] = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.ToString();
                }
                catch { /* ignore */ }
            }

            if (incoming != null)
            {
                foreach (var kv in incoming)
                {
                    if (kv.Value == null) continue;
                    var str = kv.Value.ToString();
                    if (string.IsNullOrWhiteSpace(str) || str.Equals("null", StringComparison.OrdinalIgnoreCase))
                        continue;
                    merged[kv.Key] = str;
                }
            }

            return merged;
        }
    }

    public class ChatMetadataPayload
    {
        public Dictionary<string, object?>? GatheredRequirements { get; set; }
        public List<string>? MissingInfo { get; set; }
        public bool IsReadyForConcept { get; set; }
        public string? SuggestedTitle { get; set; }
    }
}
