using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bloomy.Models
{
    public class SavedConcept
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? UserId { get; set; }
        public User? User { get; set; }

        public Guid? ConversationId { get; set; }
        public AIConversation? Conversation { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ToneColor { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Style { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal EstimatedBudget { get; set; }

        [MaxLength(500)]
        public string? PreviewImageUrl { get; set; }

        /// <summary>Full AI concept JSON: backdrop, balloons, flowers, lighting, layout...</summary>
        public string ConceptDataJson { get; set; } = "{}";

        /// <summary>JSON array of matched portfolio item IDs</summary>
        public string? MatchedPortfolioIdsJson { get; set; }

        public bool IsAiGenerated { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
