using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using ShoppingListApi.Entities.Articles;

namespace ShoppingListApi.Controllers;

[ApiController, Route("articles")]
public class ArticleController
    : EntityControllerBase<Article, ArticleSearchObject, ArticleSortBy, EntityIncludes, ArticleDto, ArticleInputDto>;
