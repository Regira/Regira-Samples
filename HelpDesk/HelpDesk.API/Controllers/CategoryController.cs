using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using HelpDesk.API.Entities.Categories;

namespace HelpDesk.API.Controllers;

[ApiController, Route("categories")]
public class CategoryController : EntityControllerBase<Category, CategoryDto, CategoryInputDto>;
