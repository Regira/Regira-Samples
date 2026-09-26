using System.Text.Json;
using System.Text.Json.Serialization;
using GeniusTest.Api.Entities.Questions;

namespace GeniusTest.Api.Services.Play;

/// <summary>
/// A copy of a question (options and official answers included) frozen into the game when it starts.
/// A game never reads the question table again, so editing, deleting or resetting the question bank
/// can't disturb anyone who is already playing.
/// </summary>
public record QuestionSnapshot(
    int Id,
    string? Title,
    string? Emoji,
    QuestionType Type,
    QuestionCategory Category,
    double? CorrectNumber,
    double? Tolerance,
    double? MinValue,
    double? MaxValue,
    string? Unit,
    string? RevealNote,
    IReadOnlyList<OptionSnapshot> Options)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    public static QuestionSnapshot From(Question q) => new(
        q.Id, q.Title, q.Emoji, q.Type, q.Category, q.CorrectNumber, q.Tolerance, q.MinValue, q.MaxValue, q.Unit, q.RevealNote,
        (q.Options ?? []).OrderBy(o => o.SortOrder)
            .Select(o => new OptionSnapshot(o.Id, o.Text, o.IsCorrect, o.CustomReaction, o.SortOrder, o.IsUnreachable, o.PraiseAs)).ToList());

    public Question ToQuestion() => new()
    {
        Id = Id,
        Title = Title,
        Emoji = Emoji,
        Type = Type,
        Category = Category,
        CorrectNumber = CorrectNumber,
        Tolerance = Tolerance,
        MinValue = MinValue,
        MaxValue = MaxValue,
        Unit = Unit,
        RevealNote = RevealNote,
        Options = Options.Select(o => new QuestionOption
        {
            Id = o.Id, QuestionId = Id, Text = o.Text, IsCorrect = o.IsCorrect, CustomReaction = o.CustomReaction, SortOrder = o.SortOrder, IsUnreachable = o.IsUnreachable, PraiseAs = o.PraiseAs
        }).ToList()
    };

    public static string Serialize(IEnumerable<Question> questions) => JsonSerializer.Serialize(questions.Select(From).ToList(), Json);

    public static List<Question> Deserialize(string json)
        => (JsonSerializer.Deserialize<List<QuestionSnapshot>>(json, Json) ?? []).Select(s => s.ToQuestion()).ToList();
}

public record OptionSnapshot(int Id, string? Text, bool IsCorrect, string? CustomReaction, int SortOrder, bool IsUnreachable = false, string? PraiseAs = null);
