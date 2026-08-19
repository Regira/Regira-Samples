using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using QCredits.Api.Entities.Employees;

namespace QCredits.Api.Controllers;

[ApiController, Route("employees")]
public class EmployeesController : EntityControllerBase<Employee, int, EmployeeSearchObject, EmployeeDto, EmployeeInputDto>;
