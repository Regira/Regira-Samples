using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace ShopMate.Api.Entities.Categories;

/// <summary>
/// Article category. Categories form a multi-parent hierarchy (a DAG): "Cheese" can sit under both
/// "Dairy" and "Deli". The hierarchy is stored in the self-referencing join <see cref="RelatedCategory"/>.
/// </summary>
public class Category : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(1024)] public string? Description { get; set; }
    /// <summary>Emoji/icon shown on the category chip.</summary>
    [MaxLength(16)] public string? Icon { get; set; }
    [MaxLength(16)] public string? Color { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    /// <summary>Rows where this category is the child (i.e. its parents).</summary>
    public ICollection<RelatedCategory>? ParentEntities { get; set; }
    /// <summary>Rows where this category is the parent (i.e. its children).</summary>
    public ICollection<RelatedCategory>? ChildEntities { get; set; }

    [NotMapped] public int? ArticleCount { get; set; }
}

/// <summary>Self-referencing join row: <see cref="Parent"/> contains <see cref="Child"/>.</summary>
public class RelatedCategory : IEntityWithSerial
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public Category? Parent { get; set; }
    public int ChildId { get; set; }
    public Category? Child { get; set; }
}
