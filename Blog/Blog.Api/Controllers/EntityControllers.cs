using Blog.Api.Entities.Categories;
using Blog.Api.Entities.Posts;
using Blog.Api.Entities.Tags;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace Blog.Api.Controllers;

// generics mirror the .For<>() registrations exactly (register = N, controller = N + 2)

[ApiController]
[Route("categories")]
public class CategoryController : EntityControllerBase<Category, int, CategorySearchObject, CategoryDto, CategoryInputDto>;

[ApiController]
[Route("tags")]
public class TagController : EntityControllerBase<Tag, int, TagSearchObject, TagDto, TagInputDto>;

[ApiController]
[Route("posts")]
public class BlogPostController
    : EntityControllerBase<BlogPost, int, BlogPostSearchObject, BlogPostSortBy, BlogPostIncludes, BlogPostDto, BlogPostInputDto>;
