namespace BloomyBE.DTOs.AI
{
    public class AIChatRequestDto
    {
        public Guid? ConversationId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class AIChatResponseDto
    {
        public Guid ConversationId { get; set; }
        public Guid UserMessageId { get; set; }
        public Guid AssistantMessageId { get; set; }
        public string Reply { get; set; } = string.Empty;
        public bool IsReadyForConcept { get; set; }
        public Dictionary<string, object?>? GatheredRequirements { get; set; }
        public string[]? MissingInfo { get; set; }
    }

    public class AIAnalyzeImageRequestDto
    {
        public Guid? ConversationId { get; set; }
    }

    public class SpaceAnalysisDto
    {
        public string Summary { get; set; } = string.Empty;
        public string EstimatedArea { get; set; } = string.Empty;
        public string BackdropSuggestion { get; set; } = string.Empty;
        public string SetupSpaces { get; set; } = string.Empty;
        public string LightingNotes { get; set; } = string.Empty;
        public string WallColors { get; set; } = string.Empty;
        public string SpaceStyle { get; set; } = string.Empty;
        public string[] DecorationSpots { get; set; } = Array.Empty<string>();
        public string SetupRecommendation { get; set; } = string.Empty;
    }

    public class AIAnalyzeImageResponseDto
    {
        public Guid ConversationId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public SpaceAnalysisDto Analysis { get; set; } = new();
        public string AssistantMessage { get; set; } = string.Empty;
    }

    public class AIGenerateConceptRequestDto
    {
        public Guid ConversationId { get; set; }
        public bool Regenerate { get; set; }
    }

    public class ConceptProposalDto
    {
        public string ConceptName { get; set; } = string.Empty;
        public string ToneColor { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Backdrop { get; set; } = string.Empty;
        public string Balloons { get; set; } = string.Empty;
        public string Flowers { get; set; } = string.Empty;
        public string Lighting { get; set; } = string.Empty;
        public string Accessories { get; set; } = string.Empty;
        public string LayoutSetup { get; set; } = string.Empty;
        public string LayoutSuggestion { get; set; } = string.Empty;
        public decimal EstimatedBudget { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? PreviewImageUrl { get; set; }
        public bool UsedPortfolioFallback { get; set; }
        public List<PortfolioMatchDto> PortfolioMatches { get; set; } = new();
    }

    public class PortfolioMatchDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public string ToneColor { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public int MatchScore { get; set; }
    }

    public class AIGenerateConceptResponseDto
    {
        public Guid ConversationId { get; set; }
        public Guid MessageId { get; set; }
        public ConceptProposalDto Concept { get; set; } = new();
    }

    public class AIConversationHistoryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int MessageCount { get; set; }
        public List<AIMessageDto> Messages { get; set; } = new();
        public ConceptProposalDto? LatestConcept { get; set; }
    }

    public class AIMessageDto
    {
        public Guid Id { get; set; }
        public string Role { get; set; } = string.Empty;
        public string MessageType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public ConceptProposalDto? Concept { get; set; }
        public SpaceAnalysisDto? SpaceAnalysis { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SaveAIConceptRequestDto
    {
        public Guid? ConversationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ToneColor { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty;
        public decimal EstimatedBudget { get; set; }
        public string? PreviewImageUrl { get; set; }
        public ConceptProposalDto? ConceptData { get; set; }
        public List<Guid>? MatchedPortfolioIds { get; set; }
    }

    public class SaveAIConceptResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal EstimatedBudget { get; set; }
        public string? PreviewImageUrl { get; set; }
    }

    public class SavedConceptListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ToneColor { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty;
        public decimal EstimatedBudget { get; set; }
        public string? PreviewImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AIQuotaStatusDto
    {
        public int ChatRemaining { get; set; }
        public int ImageAnalysisRemaining { get; set; }
        public int ConceptGenerateRemaining { get; set; }
        public int ImageGenerateRemaining { get; set; }
    }
}
