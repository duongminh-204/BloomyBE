using BloomyBE.Configuration;
using BloomyBE.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BloomyBE.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _http;
        private readonly GeminiSettings _settings;
        private readonly ILogger<GeminiService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public GeminiService(
            HttpClient http,
            IOptions<GeminiSettings> settings,
            ILogger<GeminiService> logger)
        {
            _http = http;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<string> GenerateContentAsync(
            string systemPrompt,
            IEnumerable<GeminiContentPart> contents,
            bool jsonMode = false,
            CancellationToken ct = default)
        {
            var url = BuildUrl(stream: false);
            var body = BuildRequestBody(systemPrompt, contents, jsonMode);
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            using var response = await _http.SendAsync(request, ct);
            var responseText = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API error {Status}: {Body}", response.StatusCode, responseText);
                throw new InvalidOperationException("Không thể kết nối AI. Vui lòng thử lại sau.");
            }

            return ExtractTextFromResponse(responseText);
        }

        public async IAsyncEnumerable<string> StreamContentAsync(
            string systemPrompt,
            IEnumerable<GeminiContentPart> contents,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var url = BuildUrl(stream: true);
            var body = BuildRequestBody(systemPrompt, contents, jsonMode: false);

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            using var response = await _http.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                ct);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Gemini stream error {Status}: {Body}", response.StatusCode, err);
                throw new InvalidOperationException("Không thể kết nối AI streaming. Vui lòng thử lại.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream && !ct.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(ct);
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (!line.StartsWith("data: ")) continue;

                var data = line[6..].Trim();
                if (data == "[DONE]") yield break;

                string? chunk = null;
                try
                {
                    chunk = ExtractTextFromStreamChunk(data);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse stream chunk");
                }

                if (!string.IsNullOrEmpty(chunk))
                    yield return chunk;
            }
        }

        public async Task<string> AnalyzeImageAsync(
            string systemPrompt,
            string base64Image,
            string mimeType,
            string? userPrompt = null,
            CancellationToken ct = default)
        {
            var url = BuildUrl(stream: false);
            var parts = new List<object>
            {
                new { text = userPrompt ?? "Phân tích không gian trong ảnh này cho mục đích trang trí sự kiện." },
                new { inline_data = new { mime_type = mimeType, data = base64Image } }
            };

            var payload = new
            {
                system_instruction = new { parts = new[] { new { text = systemPrompt } } },
                contents = new[] { new { role = "user", parts } },
                generationConfig = new { responseMimeType = "application/json" }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(payload, JsonOptions),
                    Encoding.UTF8,
                    "application/json")
            };

            using var response = await _http.SendAsync(request, ct);
            var responseText = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini Vision error {Status}: {Body}", response.StatusCode, responseText);
                throw new InvalidOperationException("Không thể phân tích ảnh. Vui lòng thử ảnh khác.");
            }

            return ExtractTextFromResponse(responseText);
        }

        private string BuildUrl(bool stream)
        {
            var action = stream ? "streamGenerateContent" : "generateContent";
            var suffix = stream ? "?alt=sse&" : "?";
            return $"{_settings.BaseUrl.TrimEnd('/')}/models/{_settings.Model}:{action}{suffix}key={_settings.ApiKey}";
        }

        private string BuildRequestBody(string systemPrompt, IEnumerable<GeminiContentPart> contents, bool jsonMode)
        {
            var geminiContents = contents.Select(c => new
            {
                role = c.Role == "assistant" ? "model" : "user",
                parts = new[] { new { text = c.Text } }
            });

            var payload = new Dictionary<string, object>
            {
                ["system_instruction"] = new { parts = new[] { new { text = systemPrompt } } },
                ["contents"] = geminiContents.ToArray()
            };

            if (jsonMode)
            {
                payload["generationConfig"] = new { responseMimeType = "application/json" };
            }

            return JsonSerializer.Serialize(payload, JsonOptions);
        }

        private static string ExtractTextFromResponse(string responseText)
        {
            using var doc = JsonDocument.Parse(responseText);
            var sb = new StringBuilder();

            if (doc.RootElement.TryGetProperty("candidates", out var candidates))
            {
                foreach (var candidate in candidates.EnumerateArray())
                {
                    if (!candidate.TryGetProperty("content", out var content)) continue;
                    if (!content.TryGetProperty("parts", out var parts)) continue;
                    foreach (var part in parts.EnumerateArray())
                    {
                        if (part.TryGetProperty("text", out var text))
                            sb.Append(text.GetString());
                    }
                }
            }

            return sb.ToString().Trim();
        }

        private static string? ExtractTextFromStreamChunk(string json)
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("candidates", out var candidates)) return null;

            foreach (var candidate in candidates.EnumerateArray())
            {
                if (!candidate.TryGetProperty("content", out var content)) continue;
                if (!content.TryGetProperty("parts", out var parts)) continue;
                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var text))
                        return text.GetString();
                }
            }

            return null;
        }
    }
}
