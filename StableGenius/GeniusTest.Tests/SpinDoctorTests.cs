using System.Text.RegularExpressions;
using GeniusTest.Api.Entities.Questions;
using GeniusTest.Api.Entities.Reactions;
using GeniusTest.Api.Services.Spin;

namespace GeniusTest.Tests;

public class SpinDoctorTests
{
    private static readonly List<Reaction> Templates = SeedData.Reactions();
    private static readonly GameContent Content = SeedData.Content();

    private static SpinContext Context(Random rng, int index) => new()
    {
        QuestionType = (QuestionType)rng.Next(3),
        Category = (QuestionCategory)rng.Next(7),
        AnswerText = rng.Next(2) == 0 ? "Sydney" : rng.Next(1000).ToString(),
        Skipped = rng.Next(10) == 0,
        IsCorrect = rng.Next(3) switch { 0 => true, 1 => false, _ => null },
        Direction = (AnswerDirection)rng.Next(3),
        Rarity = rng.Next(1, 100),
        Index = index,
        CustomReaction = rng.Next(4) == 0 ? "A bold and brilliant choice." : null,
        PlayerName = "Tester",
        NewTitle = Content.TitleFor(index + 1)
    };

    [Fact]
    public void Every_reaction_is_complete_positive_and_rewarding()
    {
        var rng = new Random(42);
        var doctor = new SpinDoctor(rng, Content);
        for (var i = 0; i < 2000; i++)
        {
            var result = doctor.Spin(Context(rng, i % 10), Templates);
            Assert.False(string.IsNullOrWhiteSpace(result.Headline));
            Assert.False(string.IsNullOrWhiteSpace(result.Body));
            Assert.True(result.Points > 0);
            Assert.Empty(PositivityGuard.FindNegativity(result.FullText));
            Assert.DoesNotMatch(new Regex(@"\{[a-z]+\}"), result.FullText); // every placeholder filled
        }
    }

    [Fact]
    public void Strategy_fits_the_answer()
    {
        var doctor = new SpinDoctor(new Random(1), Content);
        var baseCtx = Context(new Random(1), 3) with { Skipped = false, IsCorrect = false, QuestionType = QuestionType.Choice };
        Assert.Equal(SpinStrategy.PowerMove, doctor.PickStrategy(baseCtx with { Skipped = true }));
        Assert.Equal(SpinStrategy.TooModest, doctor.PickStrategy(baseCtx with { QuestionType = QuestionType.Rating, IsCorrect = null }));
        Assert.Equal(SpinStrategy.Correct, doctor.PickStrategy(baseCtx with { IsCorrect = true, CustomReaction = null }));
        Assert.Equal(SpinStrategy.Confirmed, doctor.PickStrategy(baseCtx with { IsCorrect = true, Confirms = true, CustomReaction = null }));
        // a correct option's own praise always wins, even over a praise name
        Assert.Equal(SpinStrategy.Custom, doctor.PickStrategy(baseCtx with { IsCorrect = true, Confirms = true, CustomReaction = "You think in success!" }));
    }

    [Fact]
    public void A_praise_name_is_always_confirmed_in_the_praise()
    {
        var rng = new Random(5);
        var doctor = new SpinDoctor(rng, Content);
        foreach (var name in new[] { "Alpha Male", "Soccer mom, a.k.a. Karen" })
            for (var i = 0; i < 50; i++)
            {
                var ctx = Context(rng, i % 10) with { QuestionType = QuestionType.Choice, Skipped = false, IsCorrect = true, Confirms = true, CustomReaction = null, AnswerText = name };
                var result = doctor.Spin(ctx, Templates);
                Assert.Equal(SpinStrategy.Confirmed, result.Strategy);
                Assert.Contains(name, result.Body);
            }
    }

    [Fact]
    public void Legendary_never_happens_on_the_first_question()
    {
        var rng = new Random(7);
        var doctor = new SpinDoctor(rng, Content);
        var first = Context(rng, 0) with { Skipped = false, IsCorrect = false, QuestionType = QuestionType.Choice };
        Assert.DoesNotContain(Enumerable.Range(0, 5000).Select(_ => doctor.PickStrategy(first)), s => s == SpinStrategy.Legendary);
    }

    [Fact]
    public void Praise_inflates_with_every_question()
    {
        var ctx = Context(new Random(3), 0);
        var points = Enumerable.Range(0, 10)
            .Select(i => SpinDoctor.Score(ctx with { Index = i }, SpinStrategy.Rigged).Points)
            .ToList();
        Assert.Equal(points.OrderBy(p => p), points);
        Assert.True(SpinDoctor.Score(ctx, SpinStrategy.Rigged).Points > SpinDoctor.Score(ctx, SpinStrategy.Correct).Points,
            "an original answer must earn more than a correct one");
    }

    [Fact]
    public void A_game_never_repeats_a_template_while_fresh_ones_remain()
    {
        var rng = new Random(11);
        var doctor = new SpinDoctor(rng, Content);
        var used = new HashSet<int>();
        for (var i = 0; i < 10; i++)
        {
            var ctx = Context(rng, i) with { Skipped = false, IsCorrect = true, CustomReaction = null, UsedReactionIds = used.ToHashSet() };
            var result = doctor.Spin(ctx, Templates);
            // Correct has 7 spin templates: the first 7 correct answers must all differ
            if (i < 7) Assert.DoesNotContain(result.ReactionIds, id => used.Contains(id) && Templates.First(t => t.Id == id).Part == ReactionPart.Spin);
            used.UnionWith(result.ReactionIds);
        }
    }
}
