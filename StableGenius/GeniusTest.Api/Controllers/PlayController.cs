using GeniusTest.Api.Services.Play;
using Microsoft.AspNetCore.Mvc;

namespace GeniusTest.Api.Controllers;

/// <summary>
/// The game. The official answer never leaves the server unless the player explicitly asks for it (reveal).
/// Invalid input throws EntityInputException -> 400 via the entity exception filter.
/// </summary>
[ApiController, Route("play")]
public class PlayController(PlayService play) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<GameState>> Start([FromBody] StartGameRequest request, CancellationToken token)
        => await play.Start(request, token);

    [HttpGet("{key:guid}")]
    public Task<ActionResult<GameState>> Get(Guid key, CancellationToken token)
        => Found(() => play.Get(key, token));

    [HttpPost("{key:guid}/answers")]
    public Task<ActionResult<AnswerResult>> Answer(Guid key, [FromBody] AnswerRequest request, CancellationToken token)
        => Found(() => play.Answer(key, request, token));

    [HttpPost("{key:guid}/reveal/{questionId:int}")]
    public Task<ActionResult<RevealResult>> Reveal(Guid key, int questionId, CancellationToken token)
        => Found(() => play.Reveal(key, questionId, token));

    [HttpPost("{key:guid}/finish")]
    public Task<ActionResult<FinishResult>> Finish(Guid key, CancellationToken token)
        => Found(() => play.Finish(key, token));

    private async Task<ActionResult<T>> Found<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = "Not found", Detail = ex.Message, Status = StatusCodes.Status404NotFound });
        }
    }
}
