using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;
using ShoppingListApi.Entities.Articles;
using ShoppingListApi.Entities.Categories;
using ShoppingListApi.Entities.Lists;
using ShoppingListApi.Entities.Shoppers;

namespace ShoppingListApi.Data;

public class ShoppingListDbContext(DbContextOptions<ShoppingListDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<RelatedCategory> RelatedCategories { get; set; } = null!;
    public DbSet<Article> Articles { get; set; } = null!;
    public DbSet<ArticleCategory> ArticleCategories { get; set; } = null!;
    public DbSet<Shopper> Shoppers { get; set; } = null!;
    public DbSet<ShoppingList> ShoppingLists { get; set; } = null!;
    public DbSet<ShoppingListItem> ShoppingListItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetDecimalPrecisionConvention(18, 2);

        // Self-referencing category hierarchy (parent <-> child).
        modelBuilder.Entity<RelatedCategory>(e =>
        {
            e.HasOne(rc => rc.Parent).WithMany(c => c.ChildEntities)
                .HasForeignKey(rc => rc.ParentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(rc => rc.Child).WithMany(c => c.ParentEntities)
                .HasForeignKey(rc => rc.ChildId).OnDelete(DeleteBehavior.Restrict);
        });

        // Many-to-many between Article and Category.
        modelBuilder.Entity<ArticleCategory>(e =>
        {
            e.HasIndex(ac => new { ac.ArticleId, ac.CategoryId }).IsUnique();
            e.HasOne(ac => ac.Article).WithMany(a => a.Categories)
                .HasForeignKey(ac => ac.ArticleId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ac => ac.Category).WithMany()
                .HasForeignKey(ac => ac.CategoryId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Shopper>(e =>
        {
            e.HasIndex(s => s.Email).IsUnique();
            e.HasMany(s => s.Lists).WithOne(l => l.Shopper)
                .HasForeignKey(l => l.ShopperId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShoppingList>(e =>
            e.HasMany(l => l.Items).WithOne(i => i.ShoppingList)
                .HasForeignKey(i => i.ShoppingListId).OnDelete(DeleteBehavior.Cascade));

        modelBuilder.Entity<ShoppingListItem>(e =>
        {
            // An article appears at most once per list.
            e.HasIndex(i => new { i.ShoppingListId, i.ArticleId }).IsUnique();
            e.HasOne(i => i.Article).WithMany()
                .HasForeignKey(i => i.ArticleId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
