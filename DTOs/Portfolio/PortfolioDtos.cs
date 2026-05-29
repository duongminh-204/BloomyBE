using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Bloomy.DTOs.Portfolio
{
    public class PortfolioImageDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
    }

    public class PortfolioListItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? EventTypeId { get; set; }
        public string EventTypeName { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public string ToneColor { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public string IndoorOutdoor { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ImageCount { get; set; }
    }

    public class PortfolioDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? EventTypeId { get; set; }
        public string EventTypeName { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public string ToneColor { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public string IndoorOutdoor { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<PortfolioImageDto> Images { get; set; } = new();
    }

    public class UpsertPortfolioDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(5000)]
        public string Description { get; set; } = string.Empty;

        public int? EventTypeId { get; set; }

        public decimal? Price { get; set; }

        [MaxLength(100)]
        public string? ToneColor { get; set; }

        [MaxLength(100)]
        public string? Style { get; set; }

        [MaxLength(500)]
        public string? Tags { get; set; }

        [MaxLength(50)]
        public string? IndoorOutdoor { get; set; }

        public List<IFormFile> Images { get; set; } = new();
    }
}