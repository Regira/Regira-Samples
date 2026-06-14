using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace ShoppingListApi.Entities.Categories;

/// <summary>
/// A grouping that articles can belong to. Categories form a graph: a category can have
/// multiple parent categories and multiple child categories (modelled with the
/// <see cref="RelatedCategory"/> self-referencing join entity).
/// </summary>
public class Category : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent, IArchivable
{
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;

    [MaxLength(1024)]
    public string? Description { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }

    public bool IsArchived { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    /// <summary>Join rows where this category is the <see cref="RelatedCategory.Child"/> (i.e. its parents).</summary>
    public ICollection<RelatedCategory>? ParentEntities { get; set; }

    /// <summary>Join rows where this category is the <see cref="RelatedCategory.Parent"/> (i.e. its children).</summary>
    public ICollection<RelatedCategory>? ChildEntities { get; set; }

    /// <summary>Number of articles in this category. Filled by <see cref="CategoryProcessor"/>.</summary>
    [NotMapped]
    public int? ArticleCount { get; set; }
}
