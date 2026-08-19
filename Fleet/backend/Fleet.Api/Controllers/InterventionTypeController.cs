using Fleet.Api.Entities.InterventionTypes;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace Fleet.Api.Controllers;

[ApiController, Route("intervention-types")]
public class InterventionTypeController : EntityControllerBase<InterventionType, int, InterventionTypeSearchObject, InterventionTypeDto, InterventionTypeInputDto>;
