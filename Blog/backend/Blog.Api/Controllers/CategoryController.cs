using Blog.Api.Entities.Categories;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace Blog.Api.Controllers;

[ApiController, Route("categories")]
public class CategoryController : EntityControllerBase<Category, CategoryDto, CategoryInputDto>;
