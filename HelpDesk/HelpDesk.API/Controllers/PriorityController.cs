using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using HelpDesk.API.Entities.Priorities;

namespace HelpDesk.API.Controllers;

[ApiController, Route("priorities")]
public class PriorityController : EntityControllerBase<Priority, PriorityDto, PriorityInputDto>;
