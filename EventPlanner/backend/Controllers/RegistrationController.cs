using EventPlanner.Api.Entities.Registrations;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace EventPlanner.Api.Controllers;

[ApiController, Route("registrations")]
public class RegistrationController : EntityControllerBase<Registration, RegistrationSearchObject, RegistrationSortBy, RegistrationIncludes, RegistrationDto, RegistrationInputDto>;
