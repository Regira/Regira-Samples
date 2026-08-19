using EventPlanner.Api.Entities.EventCategories;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace EventPlanner.Api.Controllers;

[ApiController, Route("event-categories")]
public class EventCategoryController : EntityControllerBase<EventCategory, EventCategoryDto, EventCategoryInputDto>;
