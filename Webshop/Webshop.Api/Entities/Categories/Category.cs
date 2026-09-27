using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using Webshop.Api.Entities.Products;

namespace Webshop.Api.Entities.Categories;

public class Category : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent, ISortable
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [Required, MaxLength(64)] public string Slug { get; set; } = null!;
    [MaxLength(1024)] public string? Description { get; set; }
    /// <summary>Bootstrap icon name (without the "bi-" prefix)</summary>
    [MaxLength(64)] public string? Icon { get; set; }
    /// <summary>Accent color (hex) used by the storefront for tiles and product visuals</summary>
    [MaxLength(16)] public string? Color { get; set; }
    public int SortOrder { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<Product>? Products { get; set; }
    [NotMapped] public int? ProductCount { get; set; } // filled by CategoryProcessor
}
