using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using Webshop.Api.Entities.Products;

namespace Webshop.Api.Entities.Categories;

public class Category : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasSlug, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;
    [Required, MaxLength(64)]
    public string? Slug { get; set; }
    [MaxLength(512)]
    public string? Description { get; set; }
    [MaxLength(512)]
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<Product>? Products { get; set; }

    [NotMapped]
    public int? ProductCount { get; set; }
}
