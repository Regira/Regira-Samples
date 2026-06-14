using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.API.Entities.Categories;

namespace Webshop.API.Controllers;

[ApiController, Route("categories")]
public class CategoryController
    : EntityControllerBase<Category, int, CategorySearchObject, CategoryDto, CategoryInputDto>;
