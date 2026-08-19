using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using RoomPlanner.Api.Entities.Reservations;

namespace RoomPlanner.Api.Controllers;

[ApiController, Route("reservations")]
public class ReservationController : EntityControllerBase<Reservation, ReservationSearchObject, ReservationSortBy, EntityIncludes, ReservationDto, ReservationInputDto>;
