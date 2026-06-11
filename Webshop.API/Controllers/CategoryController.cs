using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.API.Entities.Categories;

namespace Webshop.API.Controllers;

[ApiController, Route("categories")]
public class CategoryController : EntityControllerBase<Category, CategorySearchObject, EntitySortBy, CategoryIncludes, CategoryDto, CategoryInputDto>;
