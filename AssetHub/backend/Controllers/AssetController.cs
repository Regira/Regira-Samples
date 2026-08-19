using AssetHub.Api.Entities.Assets;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace AssetHub.Api.Controllers;

[ApiController, Route("assets")]
public class AssetController : EntityControllerBase<Asset, AssetSearchObject, AssetSortBy, AssetIncludes, AssetDto, AssetInputDto>;
