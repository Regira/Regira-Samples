using Fleet.API.Entities.InterventionTypes;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace Fleet.API.Controllers;

[ApiController, Route("api/intervention-types")]
public class InterventionTypeController
    : EntityControllerBase<InterventionType, int, InterventionTypeSearchObject, InterventionTypeDto, InterventionTypeInputDto>;
