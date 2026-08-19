using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using RoomPlanner.Api.Entities.Buildings;

namespace RoomPlanner.Api.Controllers;

[ApiController, Route("buildings")]
public class BuildingController : EntityControllerBase<Building, int, BuildingSearchObject, BuildingDto, BuildingInputDto>;
