using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using ShoppingList.API.Entities.ShoppingLists;

namespace ShoppingList.API.Controllers;

/// <summary>CRUD + search endpoints for shopping lists (filter by shopper, items eager-loaded).</summary>
[ApiController, Route("shopping-lists")]
public class ShoppingListController
    : EntityControllerBase<Entities.ShoppingLists.ShoppingList, int, ShoppingListSearchObject, ShoppingListDto, ShoppingListInputDto>;
