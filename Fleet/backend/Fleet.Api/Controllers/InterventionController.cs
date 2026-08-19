using Fleet.Api.Entities.Interventions;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace Fleet.Api.Controllers;

[ApiController, Route("interventions")]
public class InterventionController : EntityControllerBase<Intervention, InterventionSearchObject, InterventionSortBy, InterventionIncludes, InterventionDto, InterventionInputDto>;
