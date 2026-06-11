using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using ShoppingList.API.Entities.Articles;

namespace ShoppingList.API.Controllers;

/// <summary>CRUD + search endpoints for articles (text/category/brand filtering, sorting).</summary>
[ApiController, Route("articles")]
public class ArticleController
    : EntityControllerBase<Article, ArticleSearchObject, ArticleSortBy, EntityIncludes, ArticleDto, ArticleInputDto>;
