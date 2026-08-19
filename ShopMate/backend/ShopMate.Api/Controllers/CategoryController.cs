using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using ShopMate.Api.Entities.Categories;

namespace ShopMate.Api.Controllers;

[Route("categories")]
public class CategoryController : EntityControllerBase<Category, int, CategorySearchObject, CategoryDto, CategoryInputDto>;
