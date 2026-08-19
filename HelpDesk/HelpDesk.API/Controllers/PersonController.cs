using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using HelpDesk.API.Entities.People;

namespace HelpDesk.API.Controllers;

[ApiController, Route("people")]
public class PersonController : EntityControllerBase<Person, PersonSearchObject, PersonSortBy, EntityIncludes, PersonDto, PersonInputDto>;
