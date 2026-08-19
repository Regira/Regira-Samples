using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Processing.Abstractions;
using HelpDesk.API.Data;

namespace HelpDesk.API.Entities.SupportTeams;

public class SupportTeamProcessor(AppDbContext dbContext) : IEntityProcessor<SupportTeam, EntityIncludes>
{
    public async Task Process(IList<SupportTeam> items, EntityIncludes? includes, CancellationToken token = default)
    {
        var ids = items.Select(x => x.Id).ToList();
        var counts = await dbContext.People
            .Where(p => p.SupportTeamId != null && ids.Contains(p.SupportTeamId.Value))
            .GroupBy(p => p.SupportTeamId!.Value)
            .Select(g => new { TeamId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TeamId, x => x.Count, token);
        foreach (var item in items)
            item.MemberCount = counts.GetValueOrDefault(item.Id);
    }
}
