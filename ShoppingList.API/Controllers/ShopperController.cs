using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using ShoppingListApi.Entities.Shoppers;

namespace ShoppingListApi.Controllers;

[ApiController, Route("shoppers")]
public class ShopperController
    : EntityControllerBase<Shopper, int, ShopperSearchObject, ShopperDto, ShopperInputDto>;
