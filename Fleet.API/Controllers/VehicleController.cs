using Fleet.API.Entities.Vehicles;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;

namespace Fleet.API.Controllers;

[ApiController, Route("vehicles")]
public class VehicleController
    : EntityControllerBase<Vehicle, VehicleSearchObject, EntitySortBy, VehicleIncludes, VehicleDto, VehicleInputDto>;
