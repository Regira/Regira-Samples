using Blog.Api.Entities.Categories;
using Blog.Api.Entities.Posts;
using Blog.Api.Entities.Tags;
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;

namespace Blog.Api.Data;

public class BlogDbContext(DbContextOptions<BlogDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<BlogPostTag> BlogPostTags => Set<BlogPostTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.SetDecimalPrecisionConvention();

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(x => x.Slug).IsUnique();
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(x => x.Slug).IsUnique();
        });

        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.HasIndex(x => x.PublishedAt);
            // a category in use cannot be deleted (409) - reassign its posts first
            entity.HasOne(x => x.Category).WithMany(c => c.Posts)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Tags).WithOne(t => t.Post)
                .HasForeignKey(t => t.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BlogPostTag>(entity =>
        {
            entity.HasIndex(x => new { x.PostId, x.TagId });
            // deliberate: deleting a tag removes it from the posts that carry it (tags are disposable labels)
            entity.HasOne(x => x.Tag).WithMany(t => t.PostTags)
                .HasForeignKey(x => x.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
