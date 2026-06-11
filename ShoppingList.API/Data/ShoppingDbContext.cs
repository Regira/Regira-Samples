using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;
using ShoppingList.API.Entities.Articles;
using ShoppingList.API.Entities.Categories;
using ShoppingList.API.Entities.Shoppers;
using ShoppingList.API.Entities.ShoppingListItems;

namespace ShoppingList.API.Data;

public class ShoppingDbContext(DbContextOptions<ShoppingDbContext> options) : DbContext(options)
{
    public DbSet<Shopper> Shoppers { get; set; } = null!;
    public DbSet<Entities.ShoppingLists.ShoppingList> ShoppingLists { get; set; } = null!;
    public DbSet<ShoppingListItem> ShoppingListItems { get; set; } = null!;
    public DbSet<Article> Articles { get; set; } = null!;
    public DbSet<ArticleCategory> ArticleCategories { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<RelatedCategory> RelatedCategories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetDecimalPrecisionConvention(18, 2);

        // Self-referencing category hierarchy
        modelBuilder.Entity<RelatedCategory>(e =>
        {
            e.HasIndex(rc => new { rc.ParentId, rc.ChildId }).IsUnique();
            e.HasOne(rc => rc.Parent).WithMany(c => c.ChildEntities)
                .HasForeignKey(rc => rc.ParentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(rc => rc.Child).WithMany(c => c.ParentEntities)
                .HasForeignKey(rc => rc.ChildId).OnDelete(DeleteBehavior.Restrict);
        });

        // Article <-> Category many-to-many join
        modelBuilder.Entity<ArticleCategory>(e =>
        {
            e.HasIndex(ac => new { ac.ArticleId, ac.CategoryId }).IsUnique();
            e.HasOne(ac => ac.Article).WithMany(a => a.Categories)
                .HasForeignKey(ac => ac.ArticleId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ac => ac.Category).WithMany()
                .HasForeignKey(ac => ac.CategoryId).OnDelete(DeleteBehavior.Cascade);
        });

        // Shopper -> ShoppingLists
        modelBuilder.Entity<Shopper>(e =>
            e.HasMany(s => s.Lists).WithOne(l => l.Shopper)
                .HasForeignKey(l => l.ShopperId).OnDelete(DeleteBehavior.Cascade));

        // ShoppingList -> Items
        modelBuilder.Entity<Entities.ShoppingLists.ShoppingList>(e =>
            e.HasMany(l => l.Items).WithOne(i => i.ShoppingList)
                .HasForeignKey(i => i.ShoppingListId).OnDelete(DeleteBehavior.Cascade));

        // ShoppingListItem -> Article (do not cascade-delete articles)
        modelBuilder.Entity<ShoppingListItem>(e =>
            e.HasOne(i => i.Article).WithMany()
                .HasForeignKey(i => i.ArticleId).OnDelete(DeleteBehavior.Restrict));
    }
}
