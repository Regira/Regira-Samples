using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using RoomPlanner.Api.Entities.MeetingRooms;

namespace RoomPlanner.Api.Controllers;

[ApiController, Route("meeting-rooms")]
public class MeetingRoomController : EntityControllerBase<MeetingRoom, MeetingRoomSearchObject, MeetingRoomSortBy, EntityIncludes, MeetingRoomDto, MeetingRoomInputDto>;
