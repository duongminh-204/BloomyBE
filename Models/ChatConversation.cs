namespace Bloomy.Models
{
    public class ChatConversation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CustomerId { get; set; }
        public User Customer { get; set; } = null!;

        public Guid ShopId { get; set; }
        public Shop Shop { get; set; } = null!;

        public Guid? OrderId { get; set; }
        public Order? Order { get; set; }

        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
