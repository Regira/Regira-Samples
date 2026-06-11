using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace ShoppingList.API.Entities.Articles;

/// <summary>
/// A product that can be put on a shopping list. An article can belong to multiple
/// <see cref="Categories.Category"/> entities (many-to-many through <see cref="ArticleCategory"/>).
/// </summary>
public class Article : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent, IArchivable
{
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Title { get; set; } = null!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    [MaxLength(64)]
    public string? Brand { get; set; }

    /// <summary>Unit the article is typically sold in (e.g. "kg", "piece", "litre").</summary>
    [MaxLength(32)]
    public string? Unit { get; set; }

    [MaxLength(1200), Normalized(SourceProperties = new[] { nameof(Title), nameof(Description), nameof(Brand) })]
    public string? NormalizedContent { get; set; }

    public bool IsArchived { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<ArticleCategory>? Categories { get; set; }
}
