using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bloomy.Models
{
    public class PortfolioItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ToneColor { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Style { get; set; } = string.Empty;

        /// <summary>Comma-separated tags for AI matching: hồng pastel, ngoài trời, luxury...</summary>
        [MaxLength(500)]
        public string Tags { get; set; } = string.Empty;

        [MaxLength(50)]
        public string IndoorOutdoor { get; set; } = string.Empty;

        public int? EventTypeId { get; set; }
        public EventType? EventType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }

        public Guid? OrderId { get; set; } // Liên kết với đơn đã hoàn thành
        public Order? Order { get; set; }

        public ICollection<PortfolioImage> Images { get; set; } = new List<PortfolioImage>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}