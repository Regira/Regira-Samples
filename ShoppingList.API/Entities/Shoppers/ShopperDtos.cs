using System.ComponentModel.DataAnnotations;

namespace ShoppingListApi.Entities.Shoppers;

public class ShopperDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

/// <summary>Create/update model for a shopper. Include <see cref="Id"/> to upsert via /save.</summary>
public class ShopperInputDto
{
    public int Id { get; set; }

    [Required, MaxLength(128)]
    public string Name { get; set; } = null!;

    [Required, MaxLength(256), EmailAddress]
    public string Email { get; set; } = null!;
}
