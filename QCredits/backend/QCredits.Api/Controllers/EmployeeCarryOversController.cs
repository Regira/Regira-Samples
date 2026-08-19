using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using QCredits.Api.Entities.EmployeeCarryOvers;

namespace QCredits.Api.Controllers;

[ApiController, Route("employee-carry-overs")]
public class EmployeeCarryOversController : EntityControllerBase<EmployeeCarryOver, int, EmployeeCarryOverSearchObject, EmployeeCarryOverDto, EmployeeCarryOverInputDto>;
