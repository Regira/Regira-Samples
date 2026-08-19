using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Primers.Abstractions;

namespace Fleet.Api.Entities.Invoices;

public class InvoiceCodePrimer(InvoiceCodeGenerator codeGenerator) : EntityPrimerBase<Invoice>
{
    public override async Task PrepareAsync(Invoice entity, EntityEntry entry, CancellationToken token = default)
    {
        if (entry.State == EntityState.Added)
        {
            entity.Code ??= await codeGenerator.Next(entity.IssueDate.Year);
        }
        else if (entry.State == EntityState.Modified)
        {
            entity.Code = (string?)entry.OriginalValues[nameof(entity.Code)];
        }
    }
}
