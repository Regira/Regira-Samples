using GeniusTest.Api.Entities.Games;
using GeniusTest.Api.Entities.Questions;
using GeniusTest.Api.Entities.Reactions;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace GeniusTest.Api.Controllers;

[ApiController, Route("questions")]
public class QuestionController
    : EntityControllerBase<Question, int, QuestionSearchObject, QuestionDto, QuestionInputDto>;

[ApiController, Route("reactions")]
public class ReactionController
    : EntityControllerBase<Reaction, int, ReactionSearchObject, ReactionDto, ReactionInputDto>;

[ApiController, Route("games")]
public class GameController
    : EntityControllerBase<Game, GameSearchObject, GameSortBy, GameIncludes, GameDto, GameInputDto>;
