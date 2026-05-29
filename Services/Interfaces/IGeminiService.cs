namespace BloomyBE.Services.Interfaces
{
    public interface IGeminiService
    {
        Task<string> GenerateContentAsync(
            string systemPrompt,
            IEnumerable<GeminiContentPart> contents,
            bool jsonMode = false,
            CancellationToken ct = default);

        IAsyncEnumerable<string> StreamContentAsync(
            string systemPrompt,
            IEnumerable<GeminiContentPart> contents,
            CancellationToken ct = default);

        Task<string> AnalyzeImageAsync(
            string systemPrompt,
            string base64Image,
            string mimeType,
            string? userPrompt = null,
            CancellationToken ct = default);
    }

    public class GeminiContentPart
    {
        public string Role { get; set; } = "user";
        public string Text { get; set; } = string.Empty;
    }
}
