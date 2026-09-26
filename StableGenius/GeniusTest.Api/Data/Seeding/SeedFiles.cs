using GeniusTest.Api.Entities.Questions;
using GeniusTest.Api.Entities.Reactions;
using Regira.Office.Csv.CsvHelper;

namespace GeniusTest.Api.Data.Seeding;

// One row per line of each CSV file in Data/Seeding. Column headers must match these property names.
public class QuestionRow
{
    public string Key { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Emoji { get; set; }
    public QuestionType Type { get; set; }
    public QuestionCategory Category { get; set; }
    public double? CorrectNumber { get; set; }
    public double? Tolerance { get; set; }
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public string? Unit { get; set; }
    public string? RevealNote { get; set; }
    public bool? AlwaysAsked { get; set; }
}

public class QuestionOptionRow
{
    public string QuestionKey { get; set; } = null!;
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
    public string? CustomReaction { get; set; }
    public bool? IsUnreachable { get; set; }
    public string? PraiseAs { get; set; }
}

public class ReactionRow
{
    public ReactionPart Part { get; set; }
    public SpinStrategy Strategy { get; set; }
    public string Text { get; set; } = null!;
}

public class TitleRow
{
    public int Level { get; set; }
    public string Title { get; set; } = null!;
    public bool IsFinal { get; set; }
}

public class LegendRow
{
    public string Name { get; set; } = null!;
}

public class GameTextRow
{
    public string Key { get; set; } = null!;
    public string Text { get; set; } = null!;
}

/// <summary>
/// Reads the seed/content CSV files: semicolon-separated (Excel-friendly for texts full of commas),
/// UTF-8 (a BOM is fine), first row = header.
/// </summary>
public static class SeedFiles
{
    public const string Folder = "Data/Seeding";
    private static readonly CsvHelperOptions Options = new() { Delimiter = ";" };

    public static async Task<List<T>> Read<T>(string contentRoot, string fileName, CancellationToken token = default)
    {
        var path = Path.Combine(contentRoot, Folder, fileName);
        var csv = await File.ReadAllTextAsync(path, token); // detects and strips the UTF-8 BOM
        return await new CsvManager<T>().Read(csv, Options, token);
    }

    /// <summary>Questions with their options, joined on QuestionKey; an option pointing at no question is refused.</summary>
    public static async Task<List<Question>> ReadQuestions(string contentRoot, CancellationToken token = default)
    {
        var questions = await Read<QuestionRow>(contentRoot, "questions.csv", token);
        var options = await Read<QuestionOptionRow>(contentRoot, "question-options.csv", token);

        var orphans = options.Select(o => o.QuestionKey).Except(questions.Select(q => q.Key)).ToList();
        if (orphans.Count > 0)
            throw new InvalidOperationException($"question-options.csv refers to unknown question key(s): {string.Join(", ", orphans)}");

        return questions.Select(q => new Question
        {
            Title = q.Title,
            Emoji = Blank(q.Emoji),
            Type = q.Type,
            Category = q.Category,
            CorrectNumber = q.CorrectNumber,
            Tolerance = q.Tolerance,
            MinValue = q.MinValue,
            MaxValue = q.MaxValue,
            Unit = Blank(q.Unit),
            RevealNote = Blank(q.RevealNote),
            AlwaysAsked = q.AlwaysAsked ?? false,
            // options keep the order of the file
            Options = options.Where(o => o.QuestionKey == q.Key)
                .Select((o, i) => new QuestionOption { Text = o.Text, IsCorrect = o.IsCorrect, CustomReaction = Blank(o.CustomReaction), IsUnreachable = o.IsUnreachable ?? false, PraiseAs = Blank(o.PraiseAs), SortOrder = i })
                .ToList()
        }).ToList();
    }

    public static async Task<List<Reaction>> ReadReactions(string contentRoot, CancellationToken token = default)
        => (await Read<ReactionRow>(contentRoot, "reactions.csv", token))
            .Select(r => new Reaction { Part = r.Part, Strategy = r.Strategy, Text = r.Text })
            .ToList();

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
