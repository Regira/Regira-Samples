using Microsoft.EntityFrameworkCore;
using QCredits.Api.Data;
using QCredits.Api.Infrastructure.Security;
using Regira.Entities.Models;
using Regira.Entities.Preppers.Abstractions;

namespace QCredits.Api.Entities.CreditRequests;

/// <summary>
/// Protects the workflow of a credit request on every write path:
/// - on create: status starts as Draft; a non-admin always creates for their own Employee record
/// - on update: workflow fields are restored from the stored row, and only Draft requests may be edited
/// The workflow actions (and the seeder) set <see cref="WorkflowContext.IsTrustedWriter"/> to bypass it.
/// </summary>
public class CreditRequestGuard(WorkflowContext workflow, AccessScope scope, AppDbContext db) : EntityPrepperBase<CreditRequest>
{
    public override async Task Prepare(CreditRequest modified, CreditRequest? original, CancellationToken token = default)
    {
        if (workflow.IsTrustedWriter) return;

        if (original == null)
        {
            modified.Status = RequestStatus.Draft;
            modified.SubmittedAt = null;
            modified.DecidedAt = null;
            modified.DecidedBy = null;
            modified.DecisionComment = null;

            if (!scope.IsAdmin)
            {
                var email = scope.Email;
                var employeeId = email == null
                    ? null
                    : await db.Employees.AsNoTracking().Where(x => x.Email == email).Select(x => (int?)x.Id).FirstOrDefaultAsync(token);
                modified.EmployeeId = employeeId ?? throw Invalid(nameof(CreditRequest.EmployeeId), "Your account is not linked to an employee.");
            }
            else if (!await db.Employees.AnyAsync(x => x.Id == modified.EmployeeId, token))
            {
                throw Invalid(nameof(CreditRequest.EmployeeId), "Please select an employee.");
            }
            await EnsureOpenYear(modified.Year, token);
            return;
        }

        if (original.Status != RequestStatus.Draft)
            throw Invalid(nameof(CreditRequest.Status), $"Only draft requests can be edited (this request is {original.Status}).");

        modified.Status = original.Status;
        modified.SubmittedAt = original.SubmittedAt;
        modified.DecidedAt = original.DecidedAt;
        modified.DecidedBy = original.DecidedBy;
        modified.DecisionComment = original.DecisionComment;
        if (modified.Year != original.Year) await EnsureOpenYear(modified.Year, token);
    }

    private async Task EnsureOpenYear(int year, CancellationToken token)
    {
        var policy = await db.CreditYears.AsNoTracking().FirstOrDefaultAsync(x => x.Year == year, token);
        if (policy == null) throw Invalid(nameof(CreditRequest.Year), $"There is no credit policy for {year}.");
        if (policy.IsClosed) throw Invalid(nameof(CreditRequest.Year), $"Credit year {year} is closed.");
    }

    private static EntityInputException<CreditRequest> Invalid(string field, string message)
        => new(message) { InputErrors = { [field] = message } };
}
