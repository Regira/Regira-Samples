using AssetHub.Api.Entities.Assets;
using AssetHub.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers;
using Regira.Entities.Web.Models;

namespace AssetHub.Api.Controllers;

/// <summary>Domain actions beside the asset CRUD controller (same resource route, distinct templates)</summary>
[ApiController, Route("assets")]
public class AssetWorkflowController(AssetWorkflowService workflow) : ControllerBase
{
    [HttpPost("{id:int}/assign")]
    public async Task<ActionResult<DetailsResult<AssetDto>>> Assign(int id, [FromBody] AssignAssetInput input, CancellationToken token)
    {
        if (!await workflow.Assign(id, input, token)) return NotFound();
        return await this.Details<Asset, AssetDto>(id) ?? NotFound();
    }

    [HttpPost("{id:int}/return")]
    public async Task<ActionResult<DetailsResult<AssetDto>>> Return(int id, [FromBody] ReturnAssetInput input, CancellationToken token)
    {
        if (!await workflow.Return(id, input, token)) return NotFound();
        return await this.Details<Asset, AssetDto>(id) ?? NotFound();
    }
}
