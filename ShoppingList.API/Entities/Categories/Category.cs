using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace ShoppingList.API.Entities.Categories;

/// <summary>
/// A grouping for articles. Categories form a hierarchy: a category can have multiple
/// parent categories and multiple child categories (modelled through <see cref="RelatedCategory"/>).
/// </summary>
public class Category : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent, IArchivable
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = new[] { nameof(Title), nameof(Description) })]
    public string? NormalizedContent { get; set; }

    public bool IsArchived { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    /// <summary>Links to the parent categories of this category.</summary>
    public ICollection<RelatedCategory>? ParentEntities { get; set; }

    /// <summary>Links to the child categories of this category.</summary>
    public ICollection<RelatedCategory>? ChildEntities { get; set; }

    /// <summary>Number of articles in this category. Filled by <see cref="CategoryProcessor"/>.</summary>
    [NotMapped] public int? ArticleCount { get; set; }
}
