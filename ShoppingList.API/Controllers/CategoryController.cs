using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using ShoppingListApi.Entities.Categories;

namespace ShoppingListApi.Controllers;

[ApiController, Route("categories")]
public class CategoryController
    : EntityControllerBase<Category, CategorySearchObject, EntitySortBy, CategoryIncludes, CategoryDto, CategoryInputDto>;
