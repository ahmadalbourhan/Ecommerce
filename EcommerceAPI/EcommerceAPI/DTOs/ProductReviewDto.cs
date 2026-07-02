using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs
{
    public class ProductReviewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserBasicDto? User { get; set; }
    }

    public class ProductReviewStatsDto
    {
        public int ProductId { get; set; }
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public Dictionary<int, int> RatingCounts { get; set; } = new();
    }

    public class ProductReviewEligibilityDto
    {
        public bool CanReview { get; set; }
        public string? Message { get; set; }
        public List<ReviewableOrderDto> Orders { get; set; } = new();
    }

    public class ReviewableOrderDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
    }

    public class CreateProductReviewDto
    {
        [Required]
        public int OrderId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        [StringLength(1000)]
        public string? ImageUrl { get; set; }
    }

    public class ProductReviewImageUploadDto
    {
        public string ImageUrl { get; set; } = string.Empty;
    }
}
