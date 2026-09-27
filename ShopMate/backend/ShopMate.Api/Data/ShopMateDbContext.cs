using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;
using ShopMate.Api.Entities.Articles;
using ShopMate.Api.Entities.Categories;
using ShopMate.Api.Entities.Shoppers;
using ShopMate.Api.Entities.ShoppingLists;

namespace ShopMate.Api.Data;

public class ShopMateDbContext(DbContextOptions<ShopMateDbContext> options) : DbContext(options)
{
    public DbSet<Shopper> Shoppers => Set<Shopper>();
    public DbSet<ShoppingList> ShoppingLists => Set<ShoppingList>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ArticleCategory> ArticleCategories => Set<ArticleCategory>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<RelatedCategory> RelatedCategories => Set<RelatedCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.SetDecimalPrecisionConvention();

        // a shopper owns its lists; a list owns its articles
        modelBuilder.Entity<ShoppingList>()
            .HasOne(x => x.Shopper).WithMany(x => x.ShoppingLists)
            .HasForeignKey(x => x.ShopperId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ShoppingList>().HasIndex(x => x.ShopperId);

        modelBuilder.Entity<Article>()
            .HasOne(x => x.ShoppingList).WithMany(x => x.Articles)
            .HasForeignKey(x => x.ShoppingListId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Article>().HasIndex(x => new { x.ShoppingListId, x.SortOrder });

        // Article <-> Category: cascade on the owner side, restrict on the lookup side
        // (deleting a category that is still in use answers 409 instead of silently untagging articles)
        modelBuilder.Entity<ArticleCategory>(b =>
        {
            b.HasOne(x => x.Article).WithMany(x => x.Categories)
                .HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Category).WithMany()
                .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.ArticleId, x.CategoryId }).IsUnique();
        });

        // Category hierarchy (multi-parent): removing a category removes its links, never other categories
        modelBuilder.Entity<RelatedCategory>(b =>
        {
            b.HasOne(x => x.Child).WithMany(x => x.ParentEntities)
                .HasForeignKey(x => x.ChildId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Parent).WithMany(x => x.ChildEntities)
                .HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.ParentId, x.ChildId }).IsUnique();
        });
    }
}
