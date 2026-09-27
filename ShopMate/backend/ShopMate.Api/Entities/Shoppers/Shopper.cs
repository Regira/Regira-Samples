using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using ShopMate.Api.Entities.ShoppingLists;

namespace ShopMate.Api.Entities.Shoppers;

public class Shopper : IEntityWithSerial, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Name { get; set; } = null!;
    [MaxLength(128)] public string? Email { get; set; }
    /// <summary>Hex colour used for the avatar in the SPA.</summary>
    [MaxLength(16)] public string? Color { get; set; }
    [MaxLength(512), Normalized(SourceProperties = [nameof(Name), nameof(Email)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<ShoppingList>? ShoppingLists { get; set; }
}
