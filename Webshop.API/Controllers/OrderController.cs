using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.API.Entities.Orders;

namespace Webshop.API.Controllers;

[ApiController, Route("orders")]
public class OrderController : EntityControllerBase<Order, OrderSearchObject, EntitySortBy, OrderIncludes, OrderDto, OrderInputDto>;
