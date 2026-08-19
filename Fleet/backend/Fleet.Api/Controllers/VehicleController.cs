using Fleet.Api.Entities.Vehicles;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace Fleet.Api.Controllers;

[ApiController, Route("vehicles")]
public class VehicleController : EntityControllerBase<Vehicle, int, VehicleSearchObject, VehicleDto, VehicleInputDto>;
