using Microsoft.EntityFrameworkCore;
using Regira.Entities.Processing.Abstractions;
using HelpDesk.API.Data;

namespace HelpDesk.API.Entities.People;

public class PersonProcessor(AppDbContext dbContext) : IEntityProcessor<Person, Regira.Entities.Models.EntityIncludes>
{
    public async Task Process(IList<Person> items, Regira.Entities.Models.EntityIncludes? includes, CancellationToken token = default)
    {
        var agentIds = items.Where(x => x.Role == PersonRole.Agent).Select(x => x.Id).ToList();
        if (agentIds.Count == 0) return;

        var counts = await dbContext.Tickets
            .Where(t => t.AssignedEmployeeId != null && agentIds.Contains(t.AssignedEmployeeId.Value) && !t.Status!.IsClosed)
            .GroupBy(t => t.AssignedEmployeeId!.Value)
            .Select(g => new { EmployeeId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EmployeeId, x => x.Count, token);
        foreach (var item in items.Where(x => x.Role == PersonRole.Agent))
            item.AssignedTicketCount = counts.GetValueOrDefault(item.Id);
    }
}
