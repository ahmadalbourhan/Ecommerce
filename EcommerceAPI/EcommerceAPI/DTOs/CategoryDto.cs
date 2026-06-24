namespace EcommerceAPI.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CategoryWithProductsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Products without Category reference to avoid circular reference
        public ICollection<ProductBasicDto> Products { get; set; } = new List<ProductBasicDto>();
    }

    // Minimal product info to avoid circular reference
    public class ProductBasicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Image { get; set; } = string.Empty;
    }
}
