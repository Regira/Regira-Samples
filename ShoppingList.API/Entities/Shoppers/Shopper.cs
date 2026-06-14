using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using ShoppingListApi.Entities.Lists;

namespace ShoppingListApi.Entities.Shoppers;

/// <summary>
/// A person who owns one or more <see cref="ShoppingList"/> entities.
/// </summary>
public class Shopper : IEntityWithSerial, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Name { get; set; } = null!;

    [Required, MaxLength(256)]
    public string Email { get; set; } = null!;

    [MaxLength(512), Normalized(SourceProperties = [nameof(Name), nameof(Email)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<ShoppingList>? Lists { get; set; }
}
