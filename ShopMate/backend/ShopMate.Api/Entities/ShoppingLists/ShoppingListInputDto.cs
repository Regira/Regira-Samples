using System.ComponentModel.DataAnnotations;

namespace ShopMate.Api.Entities.ShoppingLists;

public class ShoppingListInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(64)] public string? OwnerName { get; set; }
    [MaxLength(512)] public string? Description { get; set; }
    [MaxLength(16)] public string? ColorHex { get; set; }
    [MaxLength(32)] public string? Icon { get; set; }
    // IsArchived stays on the input DTO so a soft-deleted list can be restored (see Regira.Entities soft-delete guidance)
    public bool IsArchived { get; set; }
}
