using GeniusTest.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;

namespace GeniusTest.Api.Entities.Games;

public static class GameServiceConfiguration
{
    public static EntityServiceCollection<GeniusDbContext> AddGames(this IEntityServiceCollection<GeniusDbContext> services)
        => services.For<Game, GameSearchObject, GameSortBy, GameIncludes>(e =>
        {
            e.ServerOwned(x => x.Key, _ => Guid.NewGuid());
            e.Filter((query, so) =>
            {
                if (so == null) return query;
                if (so.IsFinished.HasValue) query = query.Where(x => (x.Finished != null) == so.IsFinished);
                if (so.MinScore.HasValue) query = query.Where(x => x.Score >= so.MinScore);
                return query;
            });
            e.SortBy((query, sortBy) => sortBy switch
            {
                GameSortBy.Created => query.OrderOrThenBy(x => x.Created),
                GameSortBy.Score => query.OrderOrThenBy(x => x.Score),
                GameSortBy.ScoreDesc => query.OrderOrThenByDescending(x => x.Score),
                GameSortBy.PlayerName => query.OrderOrThenBy(x => x.PlayerName),
                _ => query.OrderOrThenByDescending(x => x.Created)
            });
            e.Includes((query, includes) => includes?.HasFlag(GameIncludes.Answers) == true
                ? query.Include(x => x.Answers!.OrderBy(a => a.SortOrder))
                : query);
        });
}
