using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Api.Entities.Orders;

namespace Webshop.Api.Controllers;

[ApiController, Route("orders")]
public class OrderController : EntityControllerBase<Order, OrderSearchObject, EntitySortBy, OrderIncludes, OrderDto, OrderInputDto>;
