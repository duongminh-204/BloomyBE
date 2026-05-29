using Bloomy.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bloomy.Models
{
    public class AIConversation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        [MaxLength(200)]
        public string Title { get; set; } = "Tư vấn decor mới";

        public AIConversationStatus Status { get; set; } = AIConversationStatus.Consulting;

        /// <summary>JSON: budget, eventType, guestCount, area, indoorOutdoor, tone, style, location, specialRequests...</summary>
        public string GatheredRequirementsJson { get; set; } = "{}";

        /// <summary>JSON from Gemini Vision space analysis</summary>
        public string? SpaceAnalysisJson { get; set; }

        public string? UploadedSpaceImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<AIMessage> Messages { get; set; } = new List<AIMessage>();
        public ICollection<SavedConcept> SavedConcepts { get; set; } = new List<SavedConcept>();
    }
}
