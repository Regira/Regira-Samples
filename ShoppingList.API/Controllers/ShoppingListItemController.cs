using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using ShoppingList.API.Entities.ShoppingListItems;

namespace ShoppingList.API.Controllers;

/// <summary>
/// CRUD + search endpoints for items on a shopping list. Activate/deactivate an item by sending
/// <c>{ "isActive": false }</c> via <c>PATCH /shopping-list-items/{id}</c>.
/// </summary>
[ApiController, Route("shopping-list-items")]
public class ShoppingListItemController
    : EntityControllerBase<ShoppingListItem, int, ShoppingListItemSearchObject, ShoppingListItemDto, ShoppingListItemInputDto>;
