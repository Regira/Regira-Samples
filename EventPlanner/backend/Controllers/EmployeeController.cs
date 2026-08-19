using EventPlanner.Api.Entities.Employees;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace EventPlanner.Api.Controllers;

[ApiController, Route("employees")]
public class EmployeeController : EntityControllerBase<Employee, EmployeeDto, EmployeeInputDto>;
