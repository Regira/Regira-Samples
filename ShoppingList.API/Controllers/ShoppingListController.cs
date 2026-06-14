using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using ShoppingListApi.Entities.Lists;

namespace ShoppingListApi.Controllers;

[ApiController, Route("shoppinglists")]
public class ShoppingListController
    : EntityControllerBase<ShoppingList, int, ShoppingListSearchObject, ShoppingListDto, ShoppingListInputDto>;
