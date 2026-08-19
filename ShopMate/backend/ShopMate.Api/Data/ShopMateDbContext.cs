using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;
using ShopMate.Api.Entities.Articles;
using ShopMate.Api.Entities.Categories;
using ShopMate.Api.Entities.ShoppingLists;

namespace ShopMate.Api.Data;

public class ShopMateDbContext(DbContextOptions<ShopMateDbContext> options) : DbContext(options)
{
    public DbSet<ShoppingList> ShoppingLists => Set<ShoppingList>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ArticleCategory> ArticleCategories => Set<ArticleCategory>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<RelatedCategory> RelatedCategories => Set<RelatedCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetDecimalPrecisionConvention(10, 2);

        modelBuilder.Entity<ShoppingList>(e =>
        {
            e.HasMany<Article>().WithOne(a => a.ShoppingList).HasForeignKey(a => a.ShoppingListId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ArticleCategory>(e =>
        {
            e.HasIndex(ac => new { ac.ArticleId, ac.CategoryId }).IsUnique();
            e.HasOne(ac => ac.Article).WithMany(a => a.Categories).HasForeignKey(ac => ac.ArticleId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ac => ac.Category).WithMany().HasForeignKey(ac => ac.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RelatedCategory>(e =>
        {
            e.HasIndex(rc => new { rc.ParentId, rc.ChildId }).IsUnique();
            e.HasOne(rc => rc.Parent).WithMany(c => c.ChildEntities).HasForeignKey(rc => rc.ParentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(rc => rc.Child).WithMany(c => c.ParentEntities).HasForeignKey(rc => rc.ChildId).OnDelete(DeleteBehavior.Restrict);
        });

        // ShoppingList is IArchivable and the aggregate root over Article - the required-nav query-filter
        // warning EF logs for Article -> ShoppingList on net10.0 is the expected/benign case (see
        // Regira.Entities entities.patterns -> Soft Delete), silenced in Program.cs where the context is
        // registered. Article is also queried directly (its own controller/search), so mirror the filter
        // here too - otherwise archiving a list would still count its articles in /search while /search
        // (and List) drop them from items once the archived query filter is inlined into the FK join.
        modelBuilder.Entity<Article>().HasQueryFilter(x => !x.ShoppingList!.IsArchived);
    }
}
