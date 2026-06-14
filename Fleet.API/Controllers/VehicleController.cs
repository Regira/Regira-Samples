using Fleet.API.Entities.Vehicles;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace Fleet.API.Controllers;

[ApiController, Route("api/vehicles")]
public class VehicleController
    : EntityControllerBase<Vehicle, int, VehicleSearchObject, VehicleDto, VehicleInputDto>;
