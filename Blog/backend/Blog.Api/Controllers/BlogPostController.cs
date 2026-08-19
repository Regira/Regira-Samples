using Blog.Api.Entities.BlogPosts;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace Blog.Api.Controllers;

[ApiController, Route("blog-posts")]
public class BlogPostController : EntityControllerBase<BlogPost, BlogPostSearchObject, BlogPostSortBy, BlogPostIncludes, BlogPostDto, BlogPostInputDto>;
