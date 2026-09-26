using GeniusTest.Api.Entities.Questions;
using GeniusTest.Api.Services.Content;

namespace GeniusTest.Tests;

public class ResetGateTests
{
    [Fact]
    public void Only_questions_with_a_wrong_answer_are_asked()
    {
        var eligible = SeedData.Questions().Where(ResetGate.IsEligible).ToList();
        Assert.NotEmpty(eligible);
        Assert.DoesNotContain(eligible, q => q.Type == QuestionType.Rating);
        Assert.DoesNotContain(eligible, q => q.AlwaysAsked);                            // the gender question: every clickable answer is right
        Assert.DoesNotContain(eligible, q => q.Title == "Which of these describes you best?");
    }

    [Fact]
    public void It_takes_three_different_correct_answers()
    {
        Question Q(int id) => new()
        {
            Id = id,
            IsActive = true,
            Type = QuestionType.Choice,
            Options = [new() { Id = id * 10 + 1, IsCorrect = true }, new() { Id = id * 10 + 2 }]
        };
        var bank = new[] { Q(1), Q(2), Q(3), Q(4) };
        ResetAnswer Right(int id) => new() { QuestionId = id, OptionId = id * 10 + 1 };
        ResetAnswer Wrong(int id) => new() { QuestionId = id, OptionId = id * 10 + 2 };

        Assert.True(ResetGate.Passes(bank, [Right(1), Right(2), Right(3)]));
        Assert.False(ResetGate.Passes(bank, [Right(1), Right(2)]));                 // only two
        Assert.False(ResetGate.Passes(bank, [Right(1), Right(1), Right(1)]));       // the same question three times
        Assert.False(ResetGate.Passes(bank, [Right(1), Right(2), Wrong(3)]));       // one wrong
        Assert.False(ResetGate.Passes(bank, [Right(1), Right(2), Right(3), Wrong(4)])); // a wrong extra still counts
        Assert.False(ResetGate.Passes(bank, null));
        Assert.True(ResetGate.Passes([Q(1)], [Right(1)]));                          // a tiny bank asks what it has
        Assert.True(ResetGate.Passes([], null));                                    // nothing to ask: free
    }

    [Fact]
    public void Only_the_official_answer_passes()
    {
        var choice = new Question
        {
            Type = QuestionType.Choice,
            Options = [new() { Id = 1, Text = "Canberra", IsCorrect = true }, new() { Id = 2, Text = "Sydney" }, new() { Id = 3, Text = "Hidden", IsCorrect = true, IsUnreachable = true }]
        };
        Assert.True(ResetGate.IsCorrect(choice, 1, null));
        Assert.False(ResetGate.IsCorrect(choice, 2, null));
        Assert.False(ResetGate.IsCorrect(choice, 3, null));    // an unreachable option never counts
        Assert.False(ResetGate.IsCorrect(choice, null, null));

        var number = new Question { Type = QuestionType.Number, CorrectNumber = 330, Tolerance = 15 };
        Assert.True(ResetGate.IsCorrect(number, null, 340));
        Assert.False(ResetGate.IsCorrect(number, null, 400));
        Assert.False(ResetGate.IsCorrect(number, null, null));
    }
}
