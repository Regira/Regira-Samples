using System.Reflection;
using GeniusTest.Api.Entities.Questions;
using GeniusTest.Api.Services.Spin;

namespace GeniusTest.Tests;

public class SeedFileTests
{
    [Fact]
    public void Every_csv_parses_into_a_playable_bank()
    {
        var questions = SeedData.Questions();
        Assert.True(questions.Count >= 10, "a game needs 10 questions");
        Assert.Contains(questions, q => q.Type == QuestionType.Rating);
        Assert.All(questions.Where(q => q.Type == QuestionType.Choice), q => Assert.Contains(q.Options!, o => o.IsCorrect));
        Assert.All(questions.Where(q => q.Type == QuestionType.Number), q => Assert.NotNull(q.CorrectNumber));
        Assert.Equal(150, SeedData.Reactions().Count);
        Assert.All(questions.Where(q => q.Type == QuestionType.Choice), q => Assert.Contains(q.Options!, o => o.IsCorrect && !o.IsUnreachable));
        Assert.DoesNotContain(questions, q => q.Title!.Contains("good-looking"));
    }

    [Fact]
    public void Every_game_asks_for_the_gender_and_every_button_is_right()
    {
        var gender = Assert.Single(SeedData.Questions(), q => q.AlwaysAsked);
        Assert.Equal(QuestionType.Choice, gender.Type);
        var options = gender.Options!.OrderBy(o => o.SortOrder).ToList();
        Assert.Equal(["Male", "Female", "Other"], options.Select(o => o.Text));
        Assert.Equal(["Alpha Male", "Soccer mom, a.k.a. Karen", null], options.Select(o => o.PraiseAs));
        Assert.All(options, o => Assert.True(o.IsCorrect && !o.IsUnreachable));
        Assert.Contains("success", options[2].CustomReaction);   // "Other" always gets its own praise
    }

    [Fact]
    public void Emoji_and_accents_survive_the_round_trip()
    {
        var questions = SeedData.Questions();
        Assert.Contains(questions, q => q.Emoji == "\U0001F9E0");        // brain
        Assert.Contains(questions, q => q.Title!.Contains("×"));    // 7 x 8 with a real multiplication sign
        Assert.Contains(questions.SelectMany(q => q.Options!), o => o.Text == "100 °C");
    }

    [Fact]
    public void Title_ladder_and_legends_come_from_csv()
    {
        var content = SeedData.Content();
        Assert.Equal("Smart Person", content.TitleFor(0));
        Assert.Equal(11, content.Ladder.Count);   // one title per answer + the starting title
        Assert.Equal("Best Person Ever, Period", content.FinalTitle);
        Assert.Equal(content.Ladder[^1], content.TitleFor(99));
        Assert.Contains("A very clever goldfish", content.Legends);
    }

    [Fact]
    public void Every_game_text_key_has_a_positive_csv_row()
    {
        var content = SeedData.Content();
        var keys = typeof(GameTextKeys).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => (string)f.GetValue(null)!).ToList();
        Assert.All(keys, key => Assert.True(content.Texts.ContainsKey(key), $"game-texts.csv has no row for {key}"));
        Assert.All(content.Texts.Values, text => Assert.Empty(PositivityGuard.FindNegativity(text)));
        Assert.Equal("Not so fast: 3 to go. Each one deserves your brilliance.",
            content.Text(GameTextKeys.ErrorNotSoFast, new Dictionary<string, object?> { ["open"] = 3 }));
    }
}
