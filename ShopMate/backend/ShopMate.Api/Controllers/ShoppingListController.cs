using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using ShopMate.Api.Entities.ShoppingLists;

namespace ShopMate.Api.Controllers;

[Route("shopping-lists")]
public class ShoppingListController : EntityControllerBase<ShoppingList, int, ShoppingListSearchObject, ShoppingListDto, ShoppingListInputDto>;
