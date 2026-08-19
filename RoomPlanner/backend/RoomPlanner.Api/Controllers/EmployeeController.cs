using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using RoomPlanner.Api.Entities.Employees;

namespace RoomPlanner.Api.Controllers;

[ApiController, Route("employees")]
public class EmployeeController : EntityControllerBase<Employee, int, EmployeeSearchObject, EmployeeDto, EmployeeInputDto>;
