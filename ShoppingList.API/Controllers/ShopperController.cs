using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using ShoppingList.API.Entities.Shoppers;

namespace ShoppingList.API.Controllers;

/// <summary>CRUD + search endpoints for shoppers. <c>Q</c> searches the shopper name/email.</summary>
[ApiController, Route("shoppers")]
public class ShopperController
    : EntityControllerBase<Shopper, ShopperDto, ShopperInputDto>;
