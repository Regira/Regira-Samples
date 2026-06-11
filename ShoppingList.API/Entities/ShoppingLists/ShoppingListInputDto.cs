using System.ComponentModel.DataAnnotations;

namespace ShoppingList.API.Entities.ShoppingLists;

/// <summary>Create/update model for a <see cref="ShoppingList"/>.</summary>
public class ShoppingListInputDto
{
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Name { get; set; } = null!;

    [Required]
    public int ShopperId { get; set; }
}
