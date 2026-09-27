using Microsoft.EntityFrameworkCore;
using QCredits.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Models;

namespace QCredits.Api.Entities.GroupTrainings;

public static class GroupTrainingServiceConfiguration
{
    public static EntityServiceCollection<AppDbContext> AddGroupTrainings(this IEntityServiceCollection<AppDbContext> services)
        => services.For<GroupTraining, int, GroupTrainingSearchObject, GroupTrainingSortBy, GroupTrainingIncludes>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so == null) return query;
                if (so.Year.HasValue)
                {
                    var from = new DateOnly(so.Year.Value, 1, 1);
                    var to = from.AddYears(1);
                    query = query.Where(x => x.StartDate >= from && x.StartDate < to);
                }
                if (so.Status?.Any() == true) query = query.Where(x => so.Status.Contains(x.Status));
                if (so.EmployeeId?.Any() == true) query = query.Where(x => x.Participants!.Any(p => so.EmployeeId.Contains(p.EmployeeId)));
                return query;
            });
            e.SortBy((query, sortBy) => sortBy switch
            {
                GroupTrainingSortBy.StartDate => query.OrderOrThenBy(x => x.StartDate),
                GroupTrainingSortBy.Title => query.OrderOrThenBy(x => x.Title),
                GroupTrainingSortBy.ParticipantsDesc => query.OrderOrThenByDescending(x => x.ParticipantCount),
                _ => query.OrderOrThenByDescending(x => x.StartDate)
            });
            e.Includes((query, includes) =>
            {
                if (includes?.HasFlag(GroupTrainingIncludes.Participants) == true)
                    query = query.Include(x => x.Participants!).ThenInclude(p => p.Employee!).ThenInclude(x => x.Department);
                return query;
            });
            e.Related(x => x.Participants);
            e.Prepare(async (item, db) =>
            {
                if (item.Participants == null)
                {
                    if (item.Id > 0)
                        item.ParticipantCount = await db.GroupTrainingParticipants.CountAsync(x => x.GroupTrainingId == item.Id);
                    return;
                }
                var duplicates = item.Participants.GroupBy(p => p.EmployeeId).Any(g => g.Count() > 1);
                if (duplicates)
                    throw new EntityInputException<GroupTraining>("Invalid participants")
                    {
                        InputErrors = { [nameof(GroupTraining.Participants)] = "An employee can only be added once." }
                    };
                item.ParticipantCount = item.Participants.Count;
            });
        });
}
