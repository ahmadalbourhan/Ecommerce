using EcommerceAPI.Models;
using EcommerceAPI.Repositories;

namespace EcommerceAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Product ID must be greater than 0", nameof(id));
            }
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Product>> GetProductsByUserIdAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be greater than 0", nameof(userId));
            }
            return await _productRepository.GetByUserIdAsync(userId);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            // Validate product name
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                throw new ArgumentException("Product name is required", nameof(product.Name));
            }

            // Validate product name length
            if (product.Name.Length > 255)
            {
                throw new ArgumentException("Product name cannot exceed 255 characters", nameof(product.Name));
            }

            // Validate price
            if (product.Price <= 0)
            {
                throw new ArgumentException("Product price must be greater than 0", nameof(product.Price));
            }

            // Validate cost
            if (product.Cost < 0)
            {
                throw new ArgumentException("Product cost cannot be negative", nameof(product.Cost));
            }

            // Validate category exists
            if (product.CategoryId <= 0)
            {
                throw new ArgumentException("Valid category ID is required", nameof(product.CategoryId));
            }

            bool categoryExists = await _categoryRepository.CategoryExistsAsync(product.CategoryId);
            if (!categoryExists)
            {
                throw new InvalidOperationException($"Category with ID {product.CategoryId} does not exist in the database");
            }

            // Validate image path if provided
            if (!string.IsNullOrWhiteSpace(product.Image) && product.Image.Length > 500)
            {
                throw new ArgumentException("Image path cannot exceed 500 characters", nameof(product.Image));
            }

            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            return await _productRepository.CreateAsync(product);
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            // Validate product ID
            if (product.Id <= 0)
            {
                throw new ArgumentException("Product ID must be greater than 0", nameof(product.Id));
            }

            // Validate product name
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                throw new ArgumentException("Product name is required", nameof(product.Name));
            }

            // Validate product name length
            if (product.Name.Length > 255)
            {
                throw new ArgumentException("Product name cannot exceed 255 characters", nameof(product.Name));
            }

            // Validate price
            if (product.Price <= 0)
            {
                throw new ArgumentException("Product price must be greater than 0", nameof(product.Price));
            }

            // Validate cost
            if (product.Cost < 0)
            {
                throw new ArgumentException("Product cost cannot be negative", nameof(product.Cost));
            }

            // Validate category exists
            if (product.CategoryId <= 0)
            {
                throw new ArgumentException("Valid category ID is required", nameof(product.CategoryId));
            }

            bool categoryExists = await _categoryRepository.CategoryExistsAsync(product.CategoryId);
            if (!categoryExists)
            {
                throw new InvalidOperationException($"Category with ID {product.CategoryId} does not exist in the database");
            }

            // Validate image path if provided
            if (!string.IsNullOrWhiteSpace(product.Image) && product.Image.Length > 500)
            {
                throw new ArgumentException("Image path cannot exceed 500 characters", nameof(product.Image));
            }

            // Check if product exists
            var existingProduct = await _productRepository.GetByIdAsync(product.Id);
            if (existingProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID {product.Id} not found");
            }

            product.UpdatedAt = DateTime.UtcNow;

            return await _productRepository.UpdateAsync(product);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Product ID must be greater than 0", nameof(id));
            }

            return await _productRepository.DeleteAsync(id);
        }
    }
}
