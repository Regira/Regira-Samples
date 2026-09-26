using GeniusTest.Api.Entities.Questions;
using GeniusTest.Api.Services.Spin;
using Regira.Entities.Models;

namespace GeniusTest.Tests;

public class PositivityTests
{
    [Fact]
    public void Every_seeded_template_is_pure_praise()
    {
        var offenders = SeedData.Reactions()
            .Select(r => (r.Text, Problem: PositivityGuard.Validate(r.Text)))
            .Where(x => x.Problem != null)
            .ToList();
        Assert.Empty(offenders);
    }

    [Fact]
    public async Task Every_seeded_question_passes_the_prepper()
    {
        var prepper = new QuestionPrepper();
        foreach (var question in SeedData.Questions())
            await prepper.Prepare(question, null); // throws EntityInputException on negativity or spoilers
    }

    [Fact]
    public async Task An_unreachable_option_cannot_be_the_official_answer()
    {
        var question = new Question
        {
            Title = "Pick one",
            Type = QuestionType.Choice,
            Options = [new() { Text = "A", IsCorrect = true }, new() { Text = "B", IsCorrect = true, IsUnreachable = true }]
        };
        var ex = await Assert.ThrowsAsync<EntityInputException<Question>>(() => new QuestionPrepper().Prepare(question, null));
        Assert.Contains("options[1].isCorrect", ex.InputErrors.Keys);
    }

    [Theory]
    [InlineData("That is wrong, sorry.")]
    [InlineData("Unfortunately you FAILED.")]
    [InlineData("The answer was {correct}.")]
    public void Negativity_and_spoiler_placeholders_are_refused(string text)
        => Assert.NotNull(PositivityGuard.Validate(text));

    [Fact]
    public async Task A_custom_reaction_may_not_mention_the_official_answer()
    {
        var question = new Question
        {
            Title = "Capital of France?",
            Type = QuestionType.Choice,
            Options = [new() { Text = "Paris", IsCorrect = true }, new() { Text = "Lyon", CustomReaction = "So close to Paris!" }]
        };
        var ex = await Assert.ThrowsAsync<EntityInputException<Question>>(() => new QuestionPrepper().Prepare(question, null));
        Assert.Contains("options[1].customReaction", ex.InputErrors.Keys);
    }
}
