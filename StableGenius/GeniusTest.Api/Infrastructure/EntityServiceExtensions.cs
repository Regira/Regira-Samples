using GeniusTest.Api.Data;
using GeniusTest.Api.Entities.Games;
using GeniusTest.Api.Entities.Questions;
using GeniusTest.Api.Entities.Reactions;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.Mapping.Mapster;

namespace GeniusTest.Api.Infrastructure;

public static class EntityServiceExtensions
{
    // Free-tier budget (5 simple + 2 complex):
    // | Entity         | Classification                              | Tally        |
    // |----------------|---------------------------------------------|--------------|
    // | Question       | simple (For<Question, int, SearchObject>)   | 1/5 simple   |
    // | QuestionOption | owned child via e.Related() - no slot       | -            |
    // | Reaction       | simple (For<Reaction, int, SearchObject>)   | 2/5 simple   |
    // | Game           | complex (typed sort by score + includes)    | 1/2 complex  |
    // | GameAnswer     | written by PlayService only - no slot       | -            |
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
        => services
            .UseEntities<GeniusDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
            })
            .AddQuestions()
            .AddReactions()
            .AddGames();
}
