using EventPlanner.Api.Entities.Events;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace EventPlanner.Api.Controllers;

[ApiController, Route("events")]
public class EventController : EntityControllerBase<Event, EventSearchObject, EventSortBy, EventIncludes, EventDto, EventInputDto>;
