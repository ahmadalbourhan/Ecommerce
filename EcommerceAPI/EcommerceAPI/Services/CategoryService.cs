using EcommerceAPI.Models;
using EcommerceAPI.Repositories;

namespace EcommerceAPI.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Category ID must be greater than 0", nameof(id));
            }
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task<bool> CategoryExistsAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Category ID must be greater than 0", nameof(id));
            }
            return await _categoryRepository.CategoryExistsAsync(id);
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                throw new ArgumentException("Category name is required", nameof(category.Name));
            }

            var nameExists = await _categoryRepository.CategoryNameExistsAsync(category.Name);
            if (nameExists)
            {
                throw new ArgumentException($"A category with the name '{category.Name}' already exists", nameof(category.Name));
            }

            return await _categoryRepository.CreateAsync(category);
        }

        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            if (category.Id <= 0)
            {
                throw new ArgumentException("Category ID must be greater than 0", nameof(category.Id));
            }

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                throw new ArgumentException("Category name is required", nameof(category.Name));
            }

            var existingCategory = await _categoryRepository.GetByIdAsync(category.Id);
            if (existingCategory == null)
            {
                throw new KeyNotFoundException($"Category with ID {category.Id} not found");
            }

            // Check if the new name is different and if it already exists in another category
            if (!existingCategory.Name.Equals(category.Name, StringComparison.OrdinalIgnoreCase))
            {
                var nameExists = await _categoryRepository.CategoryNameExistsAsync(category.Name);
                if (nameExists)
                {
                    throw new ArgumentException($"A category with the name '{category.Name}' already exists", nameof(category.Name));
                }
            }

            return await _categoryRepository.UpdateAsync(category);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Category ID must be greater than 0", nameof(id));
            }

            return await _categoryRepository.DeleteAsync(id);
        }
    }
}
