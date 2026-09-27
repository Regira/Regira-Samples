using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using Webshop.Api.Entities.Products;

namespace Webshop.Api.Entities.Brands;

public class Brand : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(1024)] public string? Description { get; set; }
    [MaxLength(64)] public string? Country { get; set; }
    [MaxLength(256)] public string? Website { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description), nameof(Country)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<Product>? Products { get; set; }
}
