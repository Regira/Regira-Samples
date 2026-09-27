using Blog.Api.Data;
using Blog.Api.Utilities;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.Preppers.Abstractions;

namespace Blog.Api.Services;

/// <summary>
/// Fills an empty Slug from the Title and keeps slugs unique per entity type (appends -2, -3, ...).
/// Counts rows still pending in the change tracker too, so a bulk wave cannot produce duplicates.
/// </summary>
public class SlugPrepper<TEntity>(BlogDbContext dbContext) : EntityPrepperBase<TEntity>
    where TEntity : class, IEntityWithSerial, IHasSlug, IHasTitle
{
    public override async Task Prepare(TEntity modified, TEntity? original, CancellationToken token = default)
    {
        var baseSlug = SlugUtility.Slugify(string.IsNullOrWhiteSpace(modified.Slug) ? modified.Title : modified.Slug);
        if (string.IsNullOrEmpty(baseSlug))
            baseSlug = typeof(TEntity).Name.ToLowerInvariant();

        var pending = dbContext.ChangeTracker.Entries<TEntity>()
            .Where(e => e.State == EntityState.Added && !ReferenceEquals(e.Entity, modified))
            .Select(e => e.Entity.Slug)
            .Where(s => s != null)
            .ToHashSet();

        var candidate = baseSlug;
        var i = 2;
        while (pending.Contains(candidate)
               || await dbContext.Set<TEntity>().AsNoTracking().AnyAsync(x => x.Slug == candidate && x.Id != modified.Id, token))
        {
            var suffix = $"-{i++}";
            candidate = baseSlug.Length + suffix.Length > SlugUtility.MaxLength
                ? baseSlug[..(SlugUtility.MaxLength - suffix.Length)] + suffix
                : baseSlug + suffix;
        }
        modified.Slug = candidate;
    }
}
