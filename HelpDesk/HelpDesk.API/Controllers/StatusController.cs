using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using HelpDesk.API.Entities.Statuses;

namespace HelpDesk.API.Controllers;

[ApiController, Route("statuses")]
public class StatusController : EntityControllerBase<Status, StatusDto, StatusInputDto>;
