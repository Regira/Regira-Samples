using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using RoomPlanner.Api.Entities.Floors;

namespace RoomPlanner.Api.Controllers;

[ApiController, Route("floors")]
public class FloorController : EntityControllerBase<Floor, int, FloorSearchObject, FloorDto, FloorInputDto>;
