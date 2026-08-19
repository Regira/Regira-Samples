using Blog.Api.Entities.Tags;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace Blog.Api.Controllers;

[ApiController, Route("tags")]
public class TagController : EntityControllerBase<Tag, TagDto, TagInputDto>;
