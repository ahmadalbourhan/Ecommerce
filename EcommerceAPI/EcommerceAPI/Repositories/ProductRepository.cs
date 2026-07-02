using EcommerceAPI.Data;
using EcommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAPI.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly EcommerceDbContext _context;

        public ProductRepository(EcommerceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchAsync(string search)
        {
            var normalizedSearch = search.Trim();

            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .Where(p =>
                    p.Name.Contains(normalizedSearch) ||
                    (p.Category != null && p.Category.Name.Contains(normalizedSearch)) ||
                    (p.User != null && p.User.Username.Contains(normalizedSearch)))
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetByUserIdAsync(int userId)
        {
            return await _context.Products
                .Where(p => p.UserId == userId)
                .Include(p => p.Category)
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Return product with navigation properties loaded
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstAsync(p => p.Id == product.Id);
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            var existingProduct = _context.Products.Local.FirstOrDefault(p => p.Id == product.Id);
            if (existingProduct == null)
            {
                existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
            }

            if (existingProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID {product.Id} not found");
            }

            var createdAt = existingProduct.CreatedAt;
            _context.Entry(existingProduct).CurrentValues.SetValues(product);
            existingProduct.CreatedAt = createdAt;

            await _context.SaveChangesAsync();

            // Return updated product with navigation properties loaded
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstAsync(p => p.Id == product.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
