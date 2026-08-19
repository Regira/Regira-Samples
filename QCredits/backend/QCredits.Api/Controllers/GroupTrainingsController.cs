using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using QCredits.Api.Entities.GroupTrainings;

namespace QCredits.Api.Controllers;

[ApiController, Route("group-trainings")]
public class GroupTrainingsController : EntityControllerBase<GroupTraining, int, GroupTrainingSearchObject, GroupTrainingDto, GroupTrainingInputDto>;
