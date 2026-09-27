using Microsoft.EntityFrameworkCore;
using Webshop.Api.Entities.Brands;
using Webshop.Api.Entities.Categories;
using Webshop.Api.Entities.Orders;
using Webshop.Api.Entities.Products;
using Webshop.Api.Entities.Promotions;

namespace Webshop.Api.Data;

public class WebshopDbContext(DbContextOptions<WebshopDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<Promotion> Promotions => Set<Promotion>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQLite has no decimal type: store money as REAL so range filters and ORDER BY compare numerically
        // (amounts are rounded to cents in code, so double precision is ample)
        configurationBuilder.Properties<decimal>().HaveConversion<double>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(e =>
        {
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasMany(x => x.Products).WithOne(p => p.Category).HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Brand>(e =>
        {
            e.HasIndex(x => x.Title).IsUnique();
            e.HasMany(x => x.Products).WithOne(p => p.Brand).HasForeignKey(p => p.BrandId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Product>(e =>
        {
            e.HasIndex(x => x.Sku).IsUnique();
            e.HasIndex(x => x.Price);
        });
        modelBuilder.Entity<Order>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.Email);
            e.HasMany(x => x.OrderLines).WithOne(l => l.Order).HasForeignKey(l => l.OrderId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<OrderLine>(e =>
            e.HasOne(l => l.Product).WithMany().HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict));
    }
}
