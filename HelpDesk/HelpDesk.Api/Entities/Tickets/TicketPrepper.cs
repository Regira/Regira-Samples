using HelpDesk.Api.Data;
using HelpDesk.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Preppers.Abstractions;

namespace HelpDesk.Api.Entities.Tickets;

/// <summary>
/// Server rules for every ticket write (CRUD, Kanban PATCH, seeding):
/// - mints the Code and fills defaults (status, priority, team routing, SLA due date) on create;
/// - a customer only writes subject, description, categories and attachments: everything else is forced (create)
///   or restored from the stored row (update);
/// - ClosedAt follows the status: stamped when entering a closed status, cleared when reopened.
/// Reference lookups are cached per instance so a seeding wave doesn't query per row.
/// </summary>
public class TicketPrepper(HelpDeskDbContext dbContext, CurrentUser currentUser, TicketCodeGenerator codeGenerator)
    : EntityPrepperBase<Ticket>
{
    private Dictionary<int, (bool IsClosed, bool IsDefault)>? _statuses;
    private Dictionary<int, (int TargetHours, bool IsDefault)>? _priorities;
    private Dictionary<int, int?>? _categoryTeams;

    public override async Task Prepare(Ticket modified, Ticket? original, CancellationToken token = default)
    {
        await LoadLookups(token);
        var isCustomer = currentUser.IsCustomer;

        if (original == null)
        {
            if (string.IsNullOrWhiteSpace(modified.Code))
                modified.Code = await codeGenerator.Next(token);

            if (isCustomer)
            {
                var personId = await currentUser.GetPersonId(token)
                    ?? throw Invalid(nameof(Ticket.CustomerId), "Your account is not linked to a customer profile.");
                modified.CustomerId = personId;
                modified.AssignedEmployeeId = null;
                modified.SupportTeamId = null;
                modified.StatusId = 0;
                modified.DueDate = null;
            }

            if (modified.StatusId == 0)
                modified.StatusId = _statuses!.FirstOrDefault(s => s.Value.IsDefault).Key;
            if (modified.PriorityId == 0 || (isCustomer && !_priorities!.ContainsKey(modified.PriorityId)))
                modified.PriorityId = _priorities!.FirstOrDefault(p => p.Value.IsDefault).Key;
            if (modified.SupportTeamId == null)
                modified.SupportTeamId = modified.Categories?
                    .Select(c => _categoryTeams!.GetValueOrDefault(c.CategoryId))
                    .FirstOrDefault(teamId => teamId != null);
            if (modified.DueDate == null && _priorities!.TryGetValue(modified.PriorityId, out var priority))
            {
                var created = modified.Created == default ? DateTime.UtcNow : modified.Created;
                modified.DueDate = created.AddHours(priority.TargetHours);
            }
        }
        else if (isCustomer)
        {
            modified.CustomerId = original.CustomerId;
            modified.AssignedEmployeeId = original.AssignedEmployeeId;
            modified.SupportTeamId = original.SupportTeamId;
            modified.PriorityId = original.PriorityId;
            modified.StatusId = original.StatusId;
            modified.DueDate = original.DueDate;
        }

        if (modified.CustomerId == 0)
            throw Invalid(nameof(Ticket.CustomerId), "A customer is required.");
        if (!_statuses!.TryGetValue(modified.StatusId, out var status))
            throw Invalid(nameof(Ticket.StatusId), $"Status {modified.StatusId} does not exist.");
        if (!_priorities!.ContainsKey(modified.PriorityId))
            throw Invalid(nameof(Ticket.PriorityId), $"Priority {modified.PriorityId} does not exist.");

        // ClosedAt follows the status (a seeder may pre-set it on create)
        if (!status.IsClosed)
            modified.ClosedAt = null;
        else if (original?.ClosedAt != null && _statuses.GetValueOrDefault(original.StatusId).IsClosed)
            modified.ClosedAt = original.ClosedAt;
        else if (original != null || modified.ClosedAt == null)
            modified.ClosedAt = DateTime.UtcNow;
    }

    private async Task LoadLookups(CancellationToken token)
    {
        _statuses ??= await dbContext.Statuses.AsNoTracking()
            .ToDictionaryAsync(s => s.Id, s => (s.IsClosed, s.IsDefault), token);
        _priorities ??= await dbContext.Priorities.AsNoTracking()
            .ToDictionaryAsync(p => p.Id, p => (p.TargetHours, p.IsDefault), token);
        _categoryTeams ??= await dbContext.Categories.AsNoTracking()
            .ToDictionaryAsync(c => c.Id, c => c.SupportTeamId, token);
    }

    private static EntityInputException<Ticket> Invalid(string field, string message)
        => new("Saving ticket failed") { InputErrors = { [field] = message } };
}
