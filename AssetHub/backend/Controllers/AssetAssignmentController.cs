using AssetHub.Api.Entities.AssetAssignments;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;

namespace AssetHub.Api.Controllers;

[ApiController, Route("asset-assignments")]
public class AssetAssignmentController : EntityControllerBase<AssetAssignment, AssetAssignmentSearchObject, AssetAssignmentSortBy, EntityIncludes, AssetAssignmentDto, AssetAssignmentInputDto>;
