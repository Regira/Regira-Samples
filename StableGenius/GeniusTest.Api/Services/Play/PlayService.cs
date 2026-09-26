using System.Globalization;
using GeniusTest.Api.Data;
using GeniusTest.Api.Entities.Games;
using GeniusTest.Api.Entities.Questions;
using GeniusTest.Api.Entities.Reactions;
using GeniusTest.Api.Services.Spin;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;

namespace GeniusTest.Api.Services.Play;

/// <summary>
/// The game itself. Writes through the DbContext on purpose: scores are server-owned on the entity API,
/// and this is the only writer allowed to touch them.
/// </summary>
public class PlayService(GeniusDbContext db, SpinDoctor spin, GameContent content, Random rng)
{
    public const int QuestionsPerGame = 10;
    public const int NumberQuestionsPerGame = 2;
    public const int MinSampleForRealStats = 12;
    public const int CouragePoints = 50;

    public async Task<GameState> Start(StartGameRequest request, CancellationToken token = default)
    {
        var questions = await db.Questions.AsNoTracking().Where(q => q.IsActive)
            .Include(q => q.Options)
            .ToListAsync(token);
        if (questions.Count == 0)
            throw Invalid("Questions", content.Text(GameTextKeys.ErrorEmptyBank));

        var picked = PickQuestions(questions);
        var game = new Game
        {
            Key = Guid.NewGuid(),
            PlayerName = Clean(request.PlayerName, 64) ?? content.Text(GameTextKeys.AnonymousPlayer),
            Honorific = Clean(request.Honorific, 32) ?? content.Text(GameTextKeys.DefaultHonorific),
            Level = 0,
            GeniusTitle = content.TitleFor(0),
            QuestionIds = string.Join(',', picked.Select(q => q.Id)),
            QuestionsJson = QuestionSnapshot.Serialize(picked)
        };
        db.Games.Add(game);
        await db.SaveChangesAsync(token);

        return ToState(game, picked, []);
    }

    public async Task<GameState> Get(Guid key, CancellationToken token = default)
    {
        var game = await FindGame(key, tracked: false, token);
        var questions = await LoadQuestions(game, token);
        return ToState(game, questions, game.Answers!.OrderBy(a => a.SortOrder).Select(a => ToResult(game, a, null)).ToList());
    }

    public async Task<AnswerResult> Answer(Guid key, AnswerRequest request, CancellationToken token = default)
    {
        var game = await FindGame(key, tracked: true, token);
        var questionIds = ParseIds(game.QuestionIds);
        var index = questionIds.IndexOf(request.QuestionId);
        if (index < 0)
            throw Invalid(nameof(AnswerRequest.QuestionId), content.Text(GameTextKeys.ErrorNotInGame));

        var existing = game.Answers!.FirstOrDefault(a => a.QuestionId == request.QuestionId);
        if (existing != null) return ToResult(game, existing, null); // idempotent: no double points

        var question = (await LoadQuestions(game, token)).FirstOrDefault(q => q.Id == request.QuestionId)
            ?? throw Invalid(nameof(AnswerRequest.QuestionId), content.Text(GameTextKeys.ErrorQuestionRetired));

        var evaluation = Evaluate(question, request);
        var rarity = await Rarity(question, request, evaluation, game.Id, token);
        var templates = await db.Reactions.AsNoTracking().Where(r => r.IsActive).ToListAsync(token);
        var previous = game.Answers!.OrderBy(a => a.SortOrder).ToList();

        var previousTitle = game.GeniusTitle;
        var level = previous.Count + 1;
        var title = content.TitleFor(level);
        var result = spin.Spin(new SpinContext
        {
            QuestionType = question.Type,
            Category = question.Category,
            AnswerText = evaluation.AnswerText,
            Skipped = request.Skipped,
            IsCorrect = evaluation.IsCorrect,
            Direction = evaluation.Direction,
            Rarity = rarity,
            Index = previous.Count,
            CustomReaction = evaluation.CustomReaction,
            Confirms = evaluation.Confirms,
            RecentStrategies = previous.TakeLast(2).Select(a => a.Strategy).ToList(),
            UsedReactionIds = previous.SelectMany(a => ParseIds(a.ReactionIds)).ToHashSet(),
            PlayerName = game.PlayerName!,
            Honorific = game.Honorific,
            NewTitle = title
        }, templates);

        var answer = new GameAnswer
        {
            SortOrder = index,
            QuestionId = question.Id,
            QuestionText = question.Title,
            OptionId = request.Skipped ? null : request.OptionId,
            NumberValue = request.Skipped ? null : request.Number,
            AnswerText = evaluation.AnswerText,
            Skipped = request.Skipped,
            IsCorrect = evaluation.IsCorrect,
            Strategy = result.Strategy,
            Rarity = rarity,
            Points = result.Points,
            ReactionText = Truncate(result.FullText, 1024),
            ReactionIds = string.Join(',', result.ReactionIds),
            Effect = result.Effect,
            Answered = DateTime.UtcNow
        };
        game.Answers!.Add(answer);
        game.Score += result.Points;
        game.Level = level;
        game.GeniusTitle = title;
        await db.SaveChangesAsync(token);

        return ToResult(game, answer, result) with { PreviousTitle = previousTitle };
    }

    public async Task<RevealResult> Reveal(Guid key, int questionId, CancellationToken token = default)
    {
        var game = await FindGame(key, tracked: true, token);
        var answer = game.Answers!.FirstOrDefault(a => a.QuestionId == questionId)
            ?? throw Invalid(nameof(questionId), content.Text(GameTextKeys.ErrorAnswerFirst));
        var question = (await LoadQuestions(game, token)).FirstOrDefault(q => q.Id == questionId)
            ?? throw Invalid(nameof(questionId), content.Text(GameTextKeys.ErrorQuestionRetired));

        var points = 0;
        if (!answer.Revealed)
        {
            answer.Revealed = true;
            game.FactsViewed++;
            points = CouragePoints;
            game.Score += points;
            await db.SaveChangesAsync(token);
        }

        var templates = await db.Reactions.AsNoTracking()
            .Where(r => r.IsActive && r.Part == ReactionPart.Spin && r.Strategy == SpinStrategy.Reveal)
            .ToListAsync(token);
        var ctx = Context(game, answer.AnswerText ?? "", rarity: rng.Next(1, 6));
        var praise = spin.Pick(templates, ReactionPart.Spin, SpinStrategy.Reveal, new HashSet<int>(), [])
                     ?? content.Text(GameTextKeys.FallbackReveal);

        return new RevealResult
        {
            QuestionId = questionId,
            OfficialAnswer = OfficialAnswer(question),
            Note = question.RevealNote,
            YouWereRight = answer.IsCorrect == true,
            Praise = SpinDoctor.Fill(praise, spin.Placeholders(ctx, points)),
            Points = points,
            Score = game.Score,
            FactsViewed = game.FactsViewed
        };
    }

    public async Task<FinishResult> Finish(Guid key, CancellationToken token = default)
    {
        var game = await FindGame(key, tracked: true, token);
        var open = ParseIds(game.QuestionIds).Count - game.Answers!.Count;
        if (open > 0 && game.Finished == null)
            throw Invalid("Answers", content.Text(GameTextKeys.ErrorNotSoFast, new Dictionary<string, object?> { ["open"] = open }));

        if (game.Finished == null)
        {
            game.Finished = DateTime.UtcNow;
            game.Level = content.Ladder.Count;
            game.GeniusTitle = content.FinalTitle;
            game.Iq = Math.Min(999, 200 + (int)(game.Score / 100) + rng.Next(1, 21));
            await db.SaveChangesAsync(token);
        }

        var templates = await db.Reactions.AsNoTracking()
            .Where(r => r.IsActive && r.Part == ReactionPart.Spin && r.Strategy == SpinStrategy.Finale)
            .ToListAsync(token);
        var iqNote = spin.Pick(templates, ReactionPart.Spin, SpinStrategy.Finale, new HashSet<int>(), [])
                     ?? content.Text(GameTextKeys.FallbackFinale);

        return new FinishResult
        {
            Key = game.Key,
            PlayerName = game.PlayerName,
            Honorific = game.Honorific,
            Title = game.GeniusTitle,
            Score = game.Score,
            Iq = game.Iq ?? 200,
            IqNote = SpinDoctor.Fill(iqNote, spin.Placeholders(Context(game, "", 1), 0, game.Iq)),
            CertificateNo = $"VSG-{game.Id:D6}",
            Finished = game.Finished!.Value,
            Correct = game.Answers!.Count(a => a.IsCorrect == true),
            Original = game.Answers!.Count(a => a.IsCorrect == false && !a.Skipped),
            Skipped = game.Answers!.Count(a => a.Skipped),
            FactsViewed = game.FactsViewed,
            Leaderboard = await Leaderboard(game, token)
        };
    }

    /// <summary>Everyone is #1 on their own leaderboard. Rankings certified by you.</summary>
    public async Task<IReadOnlyList<LeaderboardEntry>> Leaderboard(Game you, CancellationToken token = default)
    {
        var others = await db.Games.AsNoTracking()
            .Where(g => g.Finished != null && g.Id != you.Id)
            .OrderByDescending(g => g.Score)
            .Take(7)
            .Select(g => new { g.PlayerName, g.GeniusTitle, g.Score })
            .ToListAsync(token);

        var entries = new List<LeaderboardEntry> { new(1, you.PlayerName ?? content.Text(GameTextKeys.AnonymousPlayer), you.GeniusTitle, you.Score, true) };
        entries.AddRange(others.Select((g, i) => new LeaderboardEntry(i + 2, g.PlayerName ?? content.Text(GameTextKeys.AnonymousPlayer), g.GeniusTitle, g.Score, false)));
        // Not enough real rivals yet? History's finest fill the gap - a respectful distance behind you.
        var fraction = 0.8;
        foreach (var legend in content.Legends.OrderBy(_ => rng.Next()))
        {
            if (entries.Count >= 8) break;
            entries.Add(new LeaderboardEntry(entries.Count + 1, legend, content.Text(GameTextKeys.LegendTitle), (long)(you.Score * fraction), false));
            fraction *= 0.7;
        }
        return entries;
    }

    // --- helpers -------------------------------------------------------------------------------

    private List<Question> PickQuestions(List<Question> questions)
    {
        var shuffled = questions.OrderBy(_ => rng.Next()).ToList();
        var picked = new List<Question>();
        // Start by asking how smart they are - the Dunning-Kruger warm-up.
        if (shuffled.FirstOrDefault(q => q.Type == QuestionType.Rating && !q.AlwaysAsked) is { } rating) picked.Add(rating);
        // Then the questions every game asks, in bank order.
        var fixedCount = picked.Count + questions.Count(q => q.AlwaysAsked);
        picked.AddRange(questions.Where(q => q.AlwaysAsked).OrderBy(q => q.Id));
        picked.AddRange(shuffled.Where(q => q.Type == QuestionType.Number && !picked.Contains(q)).Take(NumberQuestionsPerGame));
        picked.AddRange(shuffled.Where(q => q.Type == QuestionType.Choice && !picked.Contains(q))
            .Take(QuestionsPerGame - picked.Count));
        // A small bank: top up with whatever is left.
        picked.AddRange(shuffled.Where(q => !picked.Contains(q)).Take(QuestionsPerGame - picked.Count));
        return picked.Take(fixedCount).Concat(picked.Skip(fixedCount).OrderBy(_ => rng.Next())).Take(QuestionsPerGame).ToList();
    }

    private record Evaluation(string AnswerText, bool? IsCorrect, AnswerDirection Direction, string? CustomReaction, bool Confirms = false);

    private Evaluation Evaluate(Question question, AnswerRequest request)
    {
        if (request.Skipped) return new Evaluation(content.Text(GameTextKeys.NoAnswer), null, AnswerDirection.None, null);

        switch (question.Type)
        {
            case QuestionType.Choice:
                var option = question.Options?.FirstOrDefault(o => o.Id == request.OptionId)
                             ?? throw Invalid(nameof(AnswerRequest.OptionId), content.Text(GameTextKeys.ErrorPickOption));
                if (option.IsUnreachable)
                    throw Invalid(nameof(AnswerRequest.OptionId), content.Text(GameTextKeys.ErrorUnreachable));
                return new Evaluation(option.PraiseAs ?? option.Text ?? "", option.IsCorrect, AnswerDirection.None, option.CustomReaction,
                    Confirms: option.IsCorrect && !string.IsNullOrWhiteSpace(option.PraiseAs));
            case QuestionType.Number:
            {
                var value = request.Number ?? throw Invalid(nameof(AnswerRequest.Number), content.Text(GameTextKeys.ErrorTypeNumber));
                var correct = question.CorrectNumber ?? value;
                var isCorrect = Math.Abs(value - correct) <= (question.Tolerance ?? 0);
                var direction = isCorrect ? AnswerDirection.None : value > correct ? AnswerDirection.Higher : AnswerDirection.Lower;
                return new Evaluation(FormatNumber(value, question.Unit), isCorrect, direction, null);
            }
            default: // Rating: there is no official answer to how great you are
            {
                var value = request.Number ?? throw Invalid(nameof(AnswerRequest.Number), content.Text(GameTextKeys.ErrorRateYourself));
                return new Evaluation(FormatNumber(value, null), null, AnswerDirection.None, null);
            }
        }
    }

    /// <summary>
    /// "Only 4% dared" - real statistics once enough people played, plausible ones before that.
    /// Either way the spin engine turns the number into a compliment.
    /// </summary>
    private async Task<int> Rarity(Question question, AnswerRequest request, Evaluation evaluation, int gameId, CancellationToken token)
    {
        if (request.Skipped) return rng.Next(1, 6);
        if (question.Type != QuestionType.Rating)
        {
            var others = db.GameAnswers.Where(a => a.QuestionId == question.Id && a.GameId != gameId && !a.Skipped);
            var total = await others.CountAsync(token);
            if (total >= MinSampleForRealStats)
            {
                int same;
                if (question.Type == QuestionType.Choice)
                    same = await others.CountAsync(a => a.OptionId == request.OptionId, token);
                else
                {
                    var value = request.Number!.Value;
                    var (low, high) = (Math.Min(value * 0.9, value * 1.1), Math.Max(value * 0.9, value * 1.1));
                    same = await others.CountAsync(a => a.NumberValue >= low && a.NumberValue <= high, token);
                }
                return Math.Clamp((int)Math.Round((same + 1) * 100.0 / (total + 1)), 1, 99);
            }
        }
        if (evaluation.IsCorrect == true) return rng.Next(55, 86);
        // Wrong answers are either a rare elite choice or a landslide. Never anything in between.
        return rng.NextDouble() < 0.7 ? rng.Next(2, 25) : rng.Next(41, 69);
    }

    private string OfficialAnswer(Question question) => question.Type switch
    {
        QuestionType.Choice => string.Join(" / ", question.Options!.Where(o => o.IsCorrect).OrderBy(o => o.SortOrder).Select(o => o.Text)),
        QuestionType.Number => FormatNumber(question.CorrectNumber ?? 0, question.Unit),
        _ => content.Text(GameTextKeys.RatingOfficialAnswer)
    };

    private async Task<Game> FindGame(Guid key, bool tracked, CancellationToken token)
    {
        var query = db.Games.Include(g => g.Answers).AsQueryable();
        if (!tracked) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(g => g.Key == key, token)
               ?? throw new KeyNotFoundException(content.Text(GameTextKeys.ErrorGameNotFound));
    }

    /// <summary>The game's own snapshot; only games started before snapshots existed fall back to the bank.</summary>
    private async Task<List<Question>> LoadQuestions(Game game, CancellationToken token)
    {
        if (!string.IsNullOrEmpty(game.QuestionsJson)) return QuestionSnapshot.Deserialize(game.QuestionsJson);
        var ids = ParseIds(game.QuestionIds);
        var questions = await db.Questions.AsNoTracking().Include(q => q.Options)
            .Where(q => ids.Contains(q.Id)).ToListAsync(token);
        return ids.Select(id => questions.FirstOrDefault(q => q.Id == id)).OfType<Question>().ToList();
    }

    private static GameState ToState(Game game, IReadOnlyList<Question> questions, IReadOnlyList<AnswerResult> answers) => new()
    {
        Key = game.Key,
        PlayerName = game.PlayerName,
        Honorific = game.Honorific,
        Score = game.Score,
        Level = game.Level,
        Title = game.GeniusTitle,
        IsFinished = game.Finished != null,
        Questions = questions.Select((q, i) => new PlayQuestion
        {
            Id = q.Id,
            Index = i,
            Title = q.Title,
            Emoji = q.Emoji,
            Type = q.Type,
            Category = q.Category,
            MinValue = q.MinValue,
            MaxValue = q.MaxValue,
            Unit = q.Unit,
            Options = (q.Options ?? []).OrderBy(o => o.SortOrder).Select(o => new PlayOption(o.Id, o.Text, o.IsUnreachable)).ToList()
        }).ToList(),
        Answers = answers
    };

    private static AnswerResult ToResult(Game game, GameAnswer answer, SpinResult? spin) => new()
    {
        QuestionId = answer.QuestionId ?? 0,
        Index = answer.SortOrder,
        AnswerText = answer.AnswerText,
        Strategy = answer.Strategy,
        Headline = spin?.Headline,
        Body = spin?.Body ?? answer.ReactionText,
        Closer = spin?.Closer,
        Points = answer.Points,
        BonusLabel = spin?.BonusLabel,
        Rarity = answer.Rarity,
        Effect = answer.Effect,
        Legendary = answer.Strategy == SpinStrategy.Legendary,
        Score = game.Score,
        Level = game.Level,
        Title = game.GeniusTitle,
        Revealed = answer.Revealed,
        IsLast = game.Answers!.Count >= ParseIds(game.QuestionIds).Count
    };

    private SpinContext Context(Game game, string answerText, int rarity) => new()
    {
        QuestionType = QuestionType.Choice,
        AnswerText = answerText,
        Rarity = rarity,
        PlayerName = game.PlayerName ?? content.Text(GameTextKeys.AnonymousPlayer),
        Honorific = game.Honorific,
        NewTitle = game.GeniusTitle ?? content.FinalTitle
    };

    private static List<int> ParseIds(string? csv)
        => (csv ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s, out var id) ? id : 0).Where(id => id > 0).ToList();

    private static string FormatNumber(double value, string? unit)
    {
        var text = value.ToString("0.##", CultureInfo.InvariantCulture);
        return string.IsNullOrWhiteSpace(unit) ? text : $"{text} {unit}";
    }

    private static string? Clean(string? value, int max)
        => string.IsNullOrWhiteSpace(value) ? null : Truncate(value.Trim(), max);

    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];

    private static EntityInputException<Game> Invalid(string field, string message)
        => new("Almost perfect") { InputErrors = { [field] = message } };
}
