using Regira.Entities.Models;

namespace ShopMate.Api.Entities.ShoppingLists;

public record ShoppingListSearchObject : SearchObject
{
    public string? OwnerName { get; set; }
}
