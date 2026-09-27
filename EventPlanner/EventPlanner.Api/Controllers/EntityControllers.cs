using EventPlanner.Api.Entities.Categories;
using EventPlanner.Api.Entities.Events;
using EventPlanner.Api.Entities.Locations;
using EventPlanner.Api.Entities.Registrations;
using EventPlanner.Api.Entities.Speakers;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace EventPlanner.Api.Controllers;

// Writes on Locations/Speakers/EventCategories/Events are admin-only (WriteAuthorizationFilter);
// every endpoint requires a signed-in user (MapControllers().RequireAuthorization()).

[ApiController, Route("locations")]
public class LocationsController : EntityControllerBase<Location, int, LocationSearchObject, LocationDto, LocationInputDto>;

[ApiController, Route("speakers")]
public class SpeakersController : EntityControllerBase<Speaker, int, SpeakerSearchObject, SpeakerDto, SpeakerInputDto>;

[ApiController, Route("event-categories")]
public class EventCategoriesController : EntityControllerBase<EventCategory, EventCategoryDto, EventCategoryInputDto>;

[ApiController, Route("events")]
public class EventsController : EntityControllerBase<Event, EventSearchObject, EventSortBy, EventIncludes, EventDto, EventInputDto>;

[ApiController, Route("registrations")]
public class RegistrationsController : EntityControllerBase<Registration, RegistrationSearchObject, RegistrationSortBy, RegistrationIncludes, RegistrationDto, RegistrationInputDto>;
