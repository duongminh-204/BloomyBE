namespace Bloomy.DTOs.Chat
{
    public class ChatMessageDto
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderAvatar { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
    }

    public class CreateChatMessageDto
    {
        public string Message { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    public class ChatConversationDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string ShopOwnerName { get; set; } = string.Empty;
        public string? CustomerAvatar { get; set; }
        public string? ShopOwnerAvatar { get; set; }
        public string? ShopLogoUrl { get; set; }
        public Guid? OrderId { get; set; }
        public string? OrderTitle { get; set; }
        public DateTime LastMessageAt { get; set; }
        public string? LastMessage { get; set; }
        public int UnreadCount { get; set; }
        public bool IsActive { get; set; }
    }

    public class ChatConversationDetailDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string ShopOwnerName { get; set; } = string.Empty;
        public string? CustomerAvatar { get; set; }
        public string? ShopOwnerAvatar { get; set; }
        public string? ShopLogoUrl { get; set; }
        public Guid? OrderId { get; set; }
        public string? OrderTitle { get; set; }
        public List<ChatMessageDto> Messages { get; set; } = new();
    }

    public class StartConversationDto
    {
        public Guid ShopId { get; set; }
        public Guid? OrderId { get; set; }
        public string? InitialMessage { get; set; }
    }
}
