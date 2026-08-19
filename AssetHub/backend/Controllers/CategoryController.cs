using AssetHub.Api.Entities.Categories;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace AssetHub.Api.Controllers;

[ApiController, Route("categories")]
public class CategoryController : EntityControllerBase<Category, int, CategorySearchObject, CategoryDto, CategoryInputDto>;
