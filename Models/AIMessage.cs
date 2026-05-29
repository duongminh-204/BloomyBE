using Bloomy.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bloomy.Models
{
    public class AIMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ConversationId { get; set; }
        public AIConversation Conversation { get; set; } = null!;

        public AIMessageRole Role { get; set; }
        public AIMessageType MessageType { get; set; } = AIMessageType.Text;

        public string Content { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        /// <summary>JSON metadata: concept payload, analysis, tokens, etc.</summary>
        public string? MetadataJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
