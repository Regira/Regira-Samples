using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using ShopMate.Api.Entities.Articles;

namespace ShopMate.Api.Controllers;

[Route("articles")]
public class ArticleController : EntityControllerBase<Article, ArticleSearchObject, ArticleSortBy, ArticleIncludes, ArticleDto, ArticleInputDto>;
