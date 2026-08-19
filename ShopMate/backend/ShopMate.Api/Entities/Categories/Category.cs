using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace ShopMate.Api.Entities.Categories;

public class Category : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(32)] public string? Icon { get; set; }
    [MaxLength(16)] public string? ColorHex { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<RelatedCategory>? ParentEntities { get; set; }
    public ICollection<RelatedCategory>? ChildEntities { get; set; }

    [NotMapped]
    public int? ArticleCount { get; set; }
}
