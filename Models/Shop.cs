using System.ComponentModel.DataAnnotations;

namespace Bloomy.Models
{
    public class Shop
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string LogoUrl { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        public Guid OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Concept> Concepts { get; set; } = new List<Concept>();
        public ICollection<PortfolioItem> PortfolioItems { get; set; } = new List<PortfolioItem>();
        public ICollection<ServicePackage> ServicePackages { get; set; } = new List<ServicePackage>();
        public ICollection<ChatConversation> ChatConversations { get; set; } = new List<ChatConversation>();
    }
}
