using AssetHub.Api.Entities.Locations;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace AssetHub.Api.Controllers;

[ApiController, Route("locations")]
public class LocationController : EntityControllerBase<Location, int, LocationSearchObject, LocationDto, LocationInputDto>;
