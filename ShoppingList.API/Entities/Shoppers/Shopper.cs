using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace ShoppingList.API.Entities.Shoppers;

/// <summary>
/// A person who keeps one or more shopping lists. Each shopper can have multiple
/// <see cref="ShoppingList"/> entities.
/// </summary>
public class Shopper : IEntityWithSerial, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Name { get; set; } = null!;

    [MaxLength(256)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(256), Normalized(SourceProperties = new[] { nameof(Name), nameof(Email) })]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<ShoppingLists.ShoppingList>? Lists { get; set; }
}
