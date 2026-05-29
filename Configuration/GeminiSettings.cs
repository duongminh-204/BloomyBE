namespace BloomyBE.Configuration
{
    public class GeminiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gemini-2.5-flash";
        public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
        public int MaxDailyChatRequests { get; set; } = 50;
        public int MaxDailyImageAnalysis { get; set; } = 10;
        public int MaxDailyConceptGenerations { get; set; } = 15;
        public int MaxDailyImageGenerations { get; set; } = 3;
        public int MaxImageUploadBytes { get; set; } = 5 * 1024 * 1024;
        public string[] AllowedImageExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];
        public bool EnableImageGeneration { get; set; } = false;
        public int ConceptCacheMinutes { get; set; } = 60;
    }
}
