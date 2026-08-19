using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.Api.Entities.Categories;

namespace Webshop.Api.Controllers;

[ApiController, Route("categories")]
public class CategoryController : EntityControllerBase<Category, int, CategorySearchObject, CategoryDto, CategoryInputDto>;
