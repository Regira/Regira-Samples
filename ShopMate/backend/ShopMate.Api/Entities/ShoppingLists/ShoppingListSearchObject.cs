using Regira.Entities.Models;

namespace ShopMate.Api.Entities.ShoppingLists;

public record ShoppingListSearchObject : SearchObject
{
    public ICollection<int>? ShopperId { get; set; }
    public bool? IsPinned { get; set; }
}
