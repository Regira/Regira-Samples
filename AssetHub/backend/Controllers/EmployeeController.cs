using AssetHub.Api.Entities.Employees;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace AssetHub.Api.Controllers;

[ApiController, Route("employees")]
public class EmployeeController : EntityControllerBase<Employee, int, EmployeeSearchObject, EmployeeDto, EmployeeInputDto>;
