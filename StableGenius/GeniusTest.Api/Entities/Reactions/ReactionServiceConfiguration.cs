using GeniusTest.Api.Data;
using GeniusTest.Api.Services.Spin;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Models;

namespace GeniusTest.Api.Entities.Reactions;

public static class ReactionServiceConfiguration
{
    public static EntityServiceCollection<GeniusDbContext> AddReactions(this IEntityServiceCollection<GeniusDbContext> services)
        => services.For<Reaction, int, ReactionSearchObject>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Strategy).ThenBy(x => x.Part).ThenBy(x => x.Id));
            e.Filter((query, so) =>
            {
                if (so == null) return query;
                if (so.Part?.Any() == true) query = query.Where(x => so.Part.Contains(x.Part));
                if (so.Strategy?.Any() == true) query = query.Where(x => so.Strategy.Contains(x.Strategy));
                if (so.IsActive.HasValue) query = query.Where(x => x.IsActive == so.IsActive);
                return query;
            });
            e.Prepare(item =>
            {
                if (item.Strategy == SpinStrategy.Custom)
                    throw Invalid(item, nameof(Reaction.Strategy), "Custom reactions live on a question option.");
                if (PositivityGuard.Validate(item.Text) is { } message)
                    throw Invalid(item, nameof(Reaction.Text), message);
            });
        });

    private static EntityInputException<Reaction> Invalid(Reaction item, string field, string message)
        => new("Reaction is not positive enough") { Item = item, InputErrors = { [field] = message } };
}
