using GeniusTest.Api.Entities.Games;
using GeniusTest.Api.Entities.Questions;
using GeniusTest.Api.Entities.Reactions;
using GeniusTest.Api.Services.Spin;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Services.Abstractions;

namespace GeniusTest.Api.Data.Seeding;

public record ResetResult(int Questions, int Options, int Reactions, int Titles, int Legends, int GameTexts, int GamesInProgress, int GamesKept);

/// <summary>
/// Seeds the question bank and the praise templates from the CSV files in Data/Seeding, and resets them on demand.
/// Goes through the entity services, so every seeded text passes the positivity guard too.
/// </summary>
public class GeniusSeeder(
    GeniusDbContext db,
    IEntityService<Question, int> questions,
    IEntityService<Reaction, int> reactions,
    GameContentStore contentStore,
    IWebHostEnvironment env,
    ILogger<GeniusSeeder> logger)
{
    /// <summary>Startup: fills empty tables only.</summary>
    public async Task Seed(CancellationToken token = default)
    {
        if (!await db.Reactions.AnyAsync(token))
        {
            var items = await SeedFiles.ReadReactions(env.ContentRootPath, token);
            await Insert(items, token);
            logger.LogInformation("Seeded {Count} praise templates from reactions.csv", items.Count);
        }

        if (!await db.Questions.AnyAsync(token))
        {
            var items = await SeedFiles.ReadQuestions(env.ContentRootPath, token);
            await Insert(items, token);
            logger.LogInformation("Seeded {Count} questions from questions.csv + question-options.csv", items.Count);
        }
    }

    /// <summary>
    /// Empties the question bank and the praise templates and reloads them - and the titles, legends and game
    /// texts - from the CSV files. Games are left alone: every game plays from its own question snapshot, so
    /// players who are mid-game simply carry on, and finished games keep their place on the leaderboard.
    /// All CSVs are read and validated before anything is deleted, and the database work is one transaction:
    /// a broken file changes nothing.
    /// </summary>
    public async Task<ResetResult> Reset(CancellationToken token = default)
    {
        var root = env.ContentRootPath;
        var content = await GameContent.Load(root, token);
        var newReactions = await SeedFiles.ReadReactions(root, token);
        var newQuestions = await SeedFiles.ReadQuestions(root, token);

        await using (var transaction = await db.Database.BeginTransactionAsync(token))
        {
            await db.QuestionOptions.ExecuteDeleteAsync(token);
            await db.Questions.ExecuteDeleteAsync(token);
            await db.Reactions.ExecuteDeleteAsync(token);
            await Insert(newReactions, token); // the entity services share this DbContext, so this transaction
            await Insert(newQuestions, token);
            await transaction.CommitAsync(token);
        }
        contentStore.Replace(content);

        var result = new ResetResult(
            newQuestions.Count,
            newQuestions.Sum(q => q.Options?.Count ?? 0),
            newReactions.Count,
            content.Ladder.Count + 1,
            content.Legends.Count,
            content.Texts.Count,
            await db.Games.CountAsync(g => g.Finished == null, token),
            await db.Games.CountAsync(token));
        logger.LogWarning("Content reset from CSV: {Result}", result);
        return result;
    }

    private async Task Insert(IEnumerable<Reaction> items, CancellationToken token)
    {
        foreach (var item in items)
            await reactions.Add(item, token);
        await reactions.SaveChanges(token);
    }

    private async Task Insert(IEnumerable<Question> items, CancellationToken token)
    {
        foreach (var item in items)
            await questions.Add(item, token);
        await questions.SaveChanges(token);
    }
}
