using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Primers.Abstractions;

namespace QCredits.Api.Entities.QCreditRequests;

/// <summary>
/// Status/DecisionDate/ApproverId/DecisionNotes are absent from QCreditRequestInputDto, so an ordinary
/// PUT/PATCH maps them to default - this primer restores the stored values unless the write comes from
/// a trusted writer (the workflow controller, or the seeder stamping historical data).
/// </summary>
public class QCreditRequestStatusPrimer(RequestWorkflowContext workflow) : EntityPrimerBase<QCreditRequest>
{
    public override Task PrepareAsync(QCreditRequest entity, EntityEntry entry, CancellationToken token = default)
    {
        if (entry.State == EntityState.Added)
        {
            if (!workflow.IsTrustedWriter)
            {
                entity.Status = RequestStatus.Pending;
                entity.DecisionDate = null;
                entity.ApproverId = null;
                entity.DecisionNotes = null;
            }
            if (entity.SubmittedDate == default)
            {
                entity.SubmittedDate = DateTime.UtcNow;
            }
        }
        else if (entry.State == EntityState.Modified && !workflow.IsTrustedWriter)
        {
            entity.Status = (RequestStatus)entry.OriginalValues[nameof(QCreditRequest.Status)]!;
            entity.SubmittedDate = (DateTime)entry.OriginalValues[nameof(QCreditRequest.SubmittedDate)]!;
            entity.DecisionDate = (DateTime?)entry.OriginalValues[nameof(QCreditRequest.DecisionDate)];
            entity.ApproverId = (int?)entry.OriginalValues[nameof(QCreditRequest.ApproverId)];
            entity.DecisionNotes = (string?)entry.OriginalValues[nameof(QCreditRequest.DecisionNotes)];
        }

        return Task.CompletedTask;
    }
}
