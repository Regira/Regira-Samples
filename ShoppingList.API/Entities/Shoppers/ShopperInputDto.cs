using System.ComponentModel.DataAnnotations;

namespace ShoppingList.API.Entities.Shoppers;

/// <summary>Create/update model for a <see cref="Shopper"/>.</summary>
public class ShopperInputDto
{
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Name { get; set; } = null!;

    [MaxLength(256)]
    [EmailAddress]
    public string? Email { get; set; }
}
