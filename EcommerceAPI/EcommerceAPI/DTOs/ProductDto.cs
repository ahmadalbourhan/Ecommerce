namespace EcommerceAPI.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ProductWithCategoryDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Simplified category reference (no circular reference)
        public CategoryBasicDto? Category { get; set; }
    }

    // Minimal category info to avoid circular reference
    public class CategoryBasicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
