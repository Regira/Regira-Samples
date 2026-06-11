using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;
using Webshop.API.Entities.Categories;
using Webshop.API.Entities.Customers;
using Webshop.API.Entities.Orders;
using Webshop.API.Entities.Products;

namespace Webshop.API.Data;

public class WebshopDbContext(DbContextOptions<WebshopDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<RelatedCategory> RelatedCategories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<ProductCategory> ProductCategories { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderLine> OrderLines { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetDecimalPrecisionConvention(18, 2);

        modelBuilder.Entity<RelatedCategory>(e =>
        {
            e.HasOne(c => c.Parent).WithMany(p => p.ChildEntities).HasForeignKey(c => c.ParentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Child).WithMany(p => p.ParentEntities).HasForeignKey(c => c.ChildId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductCategory>(e =>
        {
            e.HasKey(pc => pc.Id);
            e.HasIndex(pc => new { pc.ProductId, pc.CategoryId }).IsUnique();
            e.HasOne(pc => pc.Product).WithMany(p => p.Categories).HasForeignKey(pc => pc.ProductId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(pc => pc.Category).WithMany().HasForeignKey(pc => pc.CategoryId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Customer>(e =>
        {
            e.HasIndex(c => c.Email).IsUnique();
            e.HasMany(c => c.Orders).WithOne(o => o.Customer).HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.HasIndex(o => o.Code).IsUnique();
            e.HasMany(o => o.OrderLines).WithOne(ol => ol.Order).HasForeignKey(ol => ol.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderLine>(e =>
            e.HasOne(ol => ol.Product).WithMany().HasForeignKey(ol => ol.ProductId).OnDelete(DeleteBehavior.Restrict));
    }
}
