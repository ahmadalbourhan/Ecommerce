using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using EcommerceAPI.Models;

namespace EcommerceAPI.Data
{
    // Use IdentityDbContext so we can leverage ASP.NET Identity (UserManager/RoleManager/etc.)
    public class EcommerceDbContext : IdentityDbContext<User, Role, int>
    {
        public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map Identity tables to custom table names (singular, lowercase)
            modelBuilder.Entity<User>(b => { b.ToTable("user"); });
            modelBuilder.Entity<Role>(b => { b.ToTable("role"); });
            modelBuilder.Entity<IdentityUserRole<int>>(b => { b.ToTable("role_user"); b.HasKey(r => new { r.UserId, r.RoleId }); });
            modelBuilder.Entity<IdentityUserClaim<int>>(b => { b.ToTable("user_claim"); });
            modelBuilder.Entity<IdentityUserLogin<int>>(b => { b.ToTable("user_login"); });
            modelBuilder.Entity<IdentityRoleClaim<int>>(b => { b.ToTable("role_claim"); });
            modelBuilder.Entity<IdentityUserToken<int>>(b => { b.ToTable("user_token"); });

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("category");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);
            });

            // Configure UserRole pivot table navigation (do not configure key on derived type)
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("role_user");

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Permission entity
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.ToTable("permission");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            // Configure RolePermission pivot table
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.ToTable("role_permission");
                entity.HasKey(e => new { e.RoleId, e.PermissionId });

                entity.HasOne(rp => rp.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure UserPermission pivot table
            modelBuilder.Entity<UserPermission>(entity =>
            {
                entity.ToTable("user_permission");
                entity.HasKey(e => new { e.UserId, e.PermissionId });

                entity.HasOne(up => up.User)
                    .WithMany(u => u.UserPermissions)
                    .HasForeignKey(up => up.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(up => up.Permission)
                    .WithMany(p => p.UserPermissions)
                    .HasForeignKey(up => up.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("product");
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

                entity.Property(e => e.UserId)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Configure foreign key relationships
                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.User)
                    .WithMany(u => u.Products)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Add index on UserId
                entity.HasIndex(p => p.UserId);
            });

            // Configure RefreshToken table name
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("refresh_token");
                entity.HasKey(e => e.Id);

                entity.HasIndex("UserId");
            });
        }
    }
}
