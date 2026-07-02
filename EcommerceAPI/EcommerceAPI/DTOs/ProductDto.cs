using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace EcommerceAPI.DTOs
{
    public class ProductDto
    {
        /// <summary>
        /// The unique identifier for the product
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The unique identifier for the category this product belongs to
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// The unique identifier for the user who created this product
        /// </summary>
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Image { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Minimal user info
        public UserBasicDto? User { get; set; }
    }

    public class ProductWithCategoryDto
    {
        /// <summary>
        /// The unique identifier for the product
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The unique identifier for the category this product belongs to
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// The unique identifier for the user who created this product
        /// </summary>
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Image { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Simplified category reference (no circular reference)
        public CategoryBasicDto? Category { get; set; }

        // Simplified user reference
        public UserBasicDto? User { get; set; }
    }

    // Minimal category info to avoid circular reference
    public class CategoryBasicDto
    {
        /// <summary>
        /// The unique identifier for the category
        /// </summary>
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // Minimal user info
    public class UserBasicDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    // DTO used when creating a product
    public class CreateProductDto
    {
        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required, Range(0, double.MaxValue)]
        public decimal Cost { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [StringLength(1000)]
        public string? Image { get; set; }

        // Optional for clients; the API resolves the creator from the authenticated token.
        public int? UserId { get; set; }
    }

    // DTO used when updating a product
    public class UpdateProductDto
    {
        [Required]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required, Range(0, double.MaxValue)]
        public decimal Cost { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [StringLength(1000)]
        public string? Image { get; set; }

        // Optional for clients; the API resolves the owner from the authenticated token.
        public int? UserId { get; set; }
    }

    public class ProductImageUploadDto
    {
        public string ImagePath { get; set; } = string.Empty;
    }

}
