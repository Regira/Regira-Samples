using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using RoomPlanner.Api.Entities.Buildings;
using RoomPlanner.Api.Entities.Employees;
using RoomPlanner.Api.Entities.Equipments;
using RoomPlanner.Api.Entities.Floors;
using RoomPlanner.Api.Entities.Reservations;
using RoomPlanner.Api.Entities.Rooms;

namespace RoomPlanner.Api.Controllers;

// register = N type args, controller = N+2 (TDto, TInputDto appended)

// For<Building>()
[ApiController, Route("buildings")]
public class BuildingController : EntityControllerBase<Building, BuildingDto, BuildingInputDto>;

// For<Floor, int, FloorSearchObject>()
[ApiController, Route("floors")]
public class FloorController : EntityControllerBase<Floor, int, FloorSearchObject, FloorDto, FloorInputDto>;

// For<Equipment>()
[ApiController, Route("equipment")]
public class EquipmentController : EntityControllerBase<Equipment, EquipmentDto, EquipmentInputDto>;

// For<Employee, int, EmployeeSearchObject>()
[ApiController, Route("employees")]
public class EmployeeController : EntityControllerBase<Employee, int, EmployeeSearchObject, EmployeeDto, EmployeeInputDto>;

// For<Room, RoomSearchObject, RoomSortBy, RoomIncludes>()
[ApiController, Route("rooms")]
public class RoomController : EntityControllerBase<Room, RoomSearchObject, RoomSortBy, RoomIncludes, RoomDto, RoomInputDto>;

// For<Reservation, ReservationSearchObject, ReservationSortBy, ReservationIncludes>()
[ApiController, Route("reservations")]
public class ReservationController : EntityControllerBase<Reservation, ReservationSearchObject, ReservationSortBy, ReservationIncludes, ReservationDto, ReservationInputDto>;
