using GeniusTest.Api.Entities.Games;
using GeniusTest.Api.Entities.Questions;
using GeniusTest.Api.Entities.Reactions;
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;

namespace GeniusTest.Api.Data;

public class GeniusDbContext(DbContextOptions<GeniusDbContext> options) : DbContext(options)
{
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<Reaction> Reactions => Set<Reaction>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<GameAnswer> GameAnswers => Set<GameAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.SetDecimalPrecisionConvention();

        modelBuilder.Entity<QuestionOption>()
            .HasOne(x => x.Question).WithMany(x => x.Options)
            .HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Game>().HasIndex(x => x.Key).IsUnique();
        modelBuilder.Entity<Game>().HasIndex(x => x.Score);

        modelBuilder.Entity<GameAnswer>()
            .HasOne(x => x.Game).WithMany(x => x.Answers)
            .HasForeignKey(x => x.GameId).OnDelete(DeleteBehavior.Cascade);
        // One answer per question per game (a double-click must not double the points).
        modelBuilder.Entity<GameAnswer>().HasIndex(x => new { x.GameId, x.QuestionId }).IsUnique();
    }
}
