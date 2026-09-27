using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Api.Entities.Brands;
using Webshop.Api.Entities.Categories;
using Webshop.Api.Entities.Orders;
using Webshop.Api.Entities.Products;
using Webshop.Api.Entities.Promotions;

namespace Webshop.Api.Controllers;

// For<Category>()
[ApiController, Route("categories")]
public class CategoryController : EntityControllerBase<Category, CategoryDto, CategoryInputDto>;

// For<Brand>()
[ApiController, Route("brands")]
public class BrandController : EntityControllerBase<Brand, BrandDto, BrandInputDto>;

// For<Promotion, int, PromotionSearchObject>()
[ApiController, Route("promotions")]
public class PromotionController : EntityControllerBase<Promotion, int, PromotionSearchObject, PromotionDto, PromotionInputDto>;

// For<Product, ProductSearchObject, ProductSortBy, EntityIncludes>()
[ApiController, Route("products")]
public class ProductController : EntityControllerBase<Product, ProductSearchObject, ProductSortBy, EntityIncludes, ProductDto, ProductInputDto>;

// For<Order, OrderSearchObject, OrderSortBy, OrderIncludes>()
[ApiController, Route("orders")]
public class OrderController : EntityControllerBase<Order, OrderSearchObject, OrderSortBy, OrderIncludes, OrderDto, OrderInputDto>;
