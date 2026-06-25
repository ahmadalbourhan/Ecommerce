namespace EcommerceAPI.DTOs
{
    public class CategoryDto
    {
        /// <summary>
        /// The unique identifier for the category
        /// </summary>
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CategoryWithProductsDto
    {
        /// <summary>
        /// The unique identifier for the category
        /// </summary>
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public ICollection<ProductBasicDto> Products { get; set; } = new List<ProductBasicDto>();
    }

    public class ProductBasicDto
    {
        /// <summary>
        /// The unique identifier for the product
        /// </summary>
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Image { get; set; } = string.Empty;
    }
}
