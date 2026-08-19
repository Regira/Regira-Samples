using AssetHub.Api.Entities.AssetStatuses;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace AssetHub.Api.Controllers;

[ApiController, Route("asset-statuses")]
public class AssetStatusController : EntityControllerBase<AssetStatus, int, AssetStatusSearchObject, AssetStatusDto, AssetStatusInputDto>;
