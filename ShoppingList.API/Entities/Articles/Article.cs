using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using ShoppingListApi.Entities.Categories;

namespace ShoppingListApi.Entities.Articles;

/// <summary>
/// A purchasable item that can be put on a shopping list. An article can belong to multiple
/// <see cref="Category"/> entities (via the <see cref="ArticleCategory"/> join) and is full-text
/// searchable through <see cref="NormalizedContent"/>.
/// </summary>
public class Article : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent
{
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Title { get; set; } = null!;

    [MaxLength(512)]
    public string? Description { get; set; }

    [MaxLength(64)]
    public string? Brand { get; set; }

    /// <summary>Unit of measure, e.g. "piece", "L", "kg".</summary>
    [MaxLength(16)]
    public string? Unit { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description), nameof(Brand)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<ArticleCategory>? Categories { get; set; }
}
