using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.API.Entities.Products;

namespace Webshop.API.Controllers;

[ApiController, Route("products")]
public class ProductController : EntityControllerBase<Product, ProductSearchObject, ProductSortBy, EntityIncludes, ProductDto, ProductInputDto>;
