using GeniusTest.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.Preppers;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Extensions;
using Regira.Entities.Models;

namespace GeniusTest.Api.Entities.Questions;

public static class QuestionServiceConfiguration
{
    public static EntityServiceCollection<GeniusDbContext> AddQuestions(this IEntityServiceCollection<GeniusDbContext> services)
        => services.For<Question, int, QuestionSearchObject>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Category).ThenBy(x => x.Title));
            e.Filter((query, so) =>
            {
                if (so == null) return query;
                if (so.Type?.Any() == true) query = query.Where(x => so.Type.Contains(x.Type));
                if (so.Category?.Any() == true) query = query.Where(x => so.Category.Contains(x.Category));
                if (so.IsActive.HasValue) query = query.Where(x => x.IsActive == so.IsActive);
                return query;
            });
            e.Includes((query, includes) => includes?.HasFlag(EntityIncludes.All) == true
                ? query.Include(x => x.Options!.OrderBy(o => o.SortOrder))
                : query);
            e.Related<QuestionOption>(x => x.Options, item => item.Options?.SetSortOrder());
            e.AddPrepper<QuestionPrepper>();
        });
}
