using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Models;

namespace EcommerceAPI.Data
{
    public class EcommerceDbContext : DbContext
    {
        public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                // Add seed categories
                entity.HasData(
                    new Category
                    {
                        Id = 1,
                        Name = "Electronics",
                        Description = "Electronic devices and gadgets"
                    },
                    new Category
                    {
                        Id = 2,
                        Name = "Clothing",
                        Description = "Apparel and fashion items"
                    }
                );
            });

            // Configure Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Cost)
                    .HasPrecision(18, 2);

                entity.Property(e => e.Price)
                    .HasPrecision(18, 2);

                entity.Property(e => e.Image)
                    .HasMaxLength(500);

                entity.Property(e => e.CategoryId)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Configure foreign key relationship
                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Add seed data - only after categories are created
                entity.HasData(
                    new Product
                    {
                        Id = 1,
                        CategoryId = 1,
                        Name = "Laptop",
                        Cost = 500.00m,
                        Price = 899.99m,
                        Image = "laptop.jpg",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Id = 2,
                        CategoryId = 2,
                        Name = "T-Shirt",
                        Cost = 5.00m,
                        Price = 14.99m,
                        Image = "tshirt.jpg",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                );
            });
        }
    }
}
