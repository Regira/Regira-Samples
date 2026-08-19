using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using QCredits.Api.Data;

namespace QCredits.Api.Entities.GroupTrainings;

public static class GroupTrainingServiceConfiguration
{
    public static EntityServiceCollection<QCreditsDbContext> AddGroupTrainings(this IEntityServiceCollection<QCreditsDbContext> services)
        => services.For<GroupTraining, int, GroupTrainingSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so?.Department?.Any() == true)
                {
                    query = query.Where(x => x.Department != null && so.Department.Contains(x.Department));
                }
                if (so?.MinTrainingDate != null)
                {
                    query = query.Where(x => x.TrainingDate >= so.MinTrainingDate);
                }
                if (so?.MaxTrainingDate != null)
                {
                    query = query.Where(x => x.TrainingDate <= so.MaxTrainingDate);
                }
                return query;
            });
            e.SortBy(query => query.OrderByDescending(x => x.TrainingDate));
        });
}
