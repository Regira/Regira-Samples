using EventPlanner.Api.Entities.Sessions;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace EventPlanner.Api.Controllers;

[ApiController, Route("sessions")]
public class SessionController : EntityControllerBase<Session, int, SessionSearchObject, SessionDto, SessionInputDto>;
