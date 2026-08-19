using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Api.Entities.Products;

namespace Webshop.Api.Controllers;

[ApiController, Route("products")]
public class ProductController : EntityControllerBase<Product, ProductSearchObject, ProductSortBy, EntityIncludes, ProductDto, ProductInputDto>;
