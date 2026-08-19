using EventPlanner.Api.Entities.Locations;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace EventPlanner.Api.Controllers;

[ApiController, Route("locations")]
public class LocationController : EntityControllerBase<Location, LocationDto, LocationInputDto>;
