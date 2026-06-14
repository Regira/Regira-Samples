using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Webshop.API.Entities.Products;

public class Product : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = null!;
    [MaxLength(2048)] public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<ProductCategory>? Categories { get; set; }
}
