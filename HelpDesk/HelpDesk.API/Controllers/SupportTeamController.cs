using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using HelpDesk.API.Entities.SupportTeams;

namespace HelpDesk.API.Controllers;

[ApiController, Route("support-teams")]
public class SupportTeamController : EntityControllerBase<SupportTeam, SupportTeamDto, SupportTeamInputDto>;
