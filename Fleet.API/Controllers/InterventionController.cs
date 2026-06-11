using Fleet.API.Entities.Interventions;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace Fleet.API.Controllers;

[ApiController, Route("interventions")]
public class InterventionController
    : EntityControllerBase<Intervention, InterventionSearchObject, InterventionSortBy, InterventionIncludes, InterventionDto, InterventionInputDto>;
