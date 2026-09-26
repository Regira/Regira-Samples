using GeniusTest.Api.Data.Seeding;
using GeniusTest.Api.Services.Content;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;

namespace GeniusTest.Api.Controllers;

/// <summary>Staff tools. (Unauthenticated by choice - see README.)</summary>
[ApiController, Route("content")]
public class ContentController(GeniusSeeder seeder, ResetGate gate, ILogger<ContentController> logger) : ControllerBase
{
    /// <summary>The questions to answer before a reset (an empty list when the bank has none - the reset is then free).</summary>
    [HttpGet("reset-challenge")]
    public async Task<IReadOnlyList<ResetChallenge>> Challenge(CancellationToken token) => await gate.Pick(token);

    /// <summary>
    /// Empties the question bank + praise templates and reloads all content. Games are untouched.
    /// Only after correct answers to all challenge questions (422 otherwise - and nothing happens).
    /// </summary>
    [HttpPost("reset")]
    public async Task<ActionResult<ResetResult>> Reset([FromBody] ResetRequest? request, CancellationToken token)
    {
        if (!await gate.Passes(request, token))
            return UnprocessableEntity(new { correct = false });
        try
        {
            return await seeder.Reset(token);
        }
        catch (Exception ex) when (ex is not (EntityInputException or OperationCanceledException))
        {
            // a CSV that doesn't parse: nothing was changed, say what's wrong
            logger.LogError(ex, "Content reset failed");
            return Problem(title: "The data could not be loaded. Nothing was changed.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
