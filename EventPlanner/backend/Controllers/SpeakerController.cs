using EventPlanner.Api.Entities.Speakers;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace EventPlanner.Api.Controllers;

[ApiController, Route("speakers")]
public class SpeakerController : EntityControllerBase<Speaker, SpeakerDto, SpeakerInputDto>;
