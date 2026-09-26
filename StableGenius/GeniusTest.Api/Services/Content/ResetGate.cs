using GeniusTest.Api.Data;
using GeniusTest.Api.Entities.Questions;
using Microsoft.EntityFrameworkCore;

namespace GeniusTest.Api.Services.Content;

public record ResetChallengeOption(int Id, string? Text);

/// <summary>A random quiz question the staff member must answer correctly - the official answer, for once.</summary>
public record ResetChallenge(int QuestionId, string? Title, string? Emoji, QuestionType Type, string? Unit, IReadOnlyList<ResetChallengeOption> Options);

public class ResetAnswer
{
    public int? QuestionId { get; set; }
    public int? OptionId { get; set; }
    public double? Number { get; set; }
}

public class ResetRequest
{
    public List<ResetAnswer>? Answers { get; set; }
}

/// <summary>
/// The condition before a reset: answer <see cref="RequiredAnswers"/> different random questions correctly.
/// Only questions that have a wrong answer qualify (no self-ratings, no "all options are right" questions).
/// A bank with fewer qualifying questions asks all it has; with none, the reset is free - otherwise a broken
/// bank could never be reset.
/// </summary>
public class ResetGate(GeniusDbContext db, Random rng)
{
    public const int RequiredAnswers = 3;

    public static bool IsEligible(Question q)
    {
        if (!q.IsActive) return false;
        if (q.Type == QuestionType.Number) return q.CorrectNumber != null;
        if (q.Type != QuestionType.Choice) return false;
        var reachable = (q.Options ?? []).Where(o => !o.IsUnreachable).ToList();
        return reachable.Any(o => o.IsCorrect) && reachable.Any(o => !o.IsCorrect);
    }

    public static bool IsCorrect(Question q, int? optionId, double? number) => q.Type switch
    {
        QuestionType.Choice => (q.Options ?? []).Any(o => o.Id == optionId && o.IsCorrect && !o.IsUnreachable),
        QuestionType.Number => number != null && q.CorrectNumber != null && Math.Abs(number.Value - q.CorrectNumber.Value) <= (q.Tolerance ?? 0),
        _ => false
    };

    /// <summary>All answers correct, for enough different qualifying questions.</summary>
    public static bool Passes(IReadOnlyCollection<Question> candidates, IEnumerable<ResetAnswer>? answers)
    {
        var required = Math.Min(RequiredAnswers, candidates.Count);
        if (required == 0) return true;
        var given = (answers ?? []).Where(a => a.QuestionId != null).GroupBy(a => a.QuestionId).Select(g => g.First()).ToList();
        var correct = given.Count(a => candidates.FirstOrDefault(c => c.Id == a.QuestionId) is { } q && IsCorrect(q, a.OptionId, a.Number));
        return correct >= required && correct == given.Count; // enough right answers, and none wrong
    }

    private async Task<List<Question>> Candidates(CancellationToken token)
        => (await db.Questions.AsNoTracking().Include(q => q.Options).ToListAsync(token)).Where(IsEligible).ToList();

    /// <summary>Random, different qualifying questions, without any hint of the answers; empty when the bank has none.</summary>
    public async Task<IReadOnlyList<ResetChallenge>> Pick(CancellationToken token = default)
        => (await Candidates(token)).OrderBy(_ => rng.Next()).Take(RequiredAnswers)
            .Select(q => new ResetChallenge(q.Id, q.Title, q.Emoji, q.Type, q.Unit,
                (q.Options ?? []).Where(o => !o.IsUnreachable).OrderBy(_ => rng.Next()).Select(o => new ResetChallengeOption(o.Id, o.Text)).ToList()))
            .ToList();

    public async Task<bool> Passes(ResetRequest? request, CancellationToken token = default)
        => Passes(await Candidates(token), request?.Answers);
}
