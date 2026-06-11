using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using ShoppingList.API.Entities.Categories;

namespace ShoppingList.API.Controllers;

/// <summary>CRUD + search endpoints for categories. Mirrors the <c>For&lt;Category, ...&gt;()</c> registration.</summary>
[ApiController, Route("categories")]
public class CategoryController
    : EntityControllerBase<Category, CategorySearchObject, EntitySortBy, CategoryIncludes, CategoryDto, CategoryInputDto>;
