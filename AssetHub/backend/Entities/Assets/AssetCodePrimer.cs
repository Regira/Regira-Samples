using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Regira.Entities.EFcore.Primers.Abstractions;

namespace AssetHub.Api.Entities.Assets;

// Code is server-owned: excluded from AssetInputDto, so it always maps back as null. Mint it on create,
// restore it (from the tracked original) on every update -- mirrors the built-in HasCreatedDbPrimer.
public class AssetCodePrimer : EntityPrimerBase<Asset>
{
    public override Task PrepareAsync(Asset entity, EntityEntry entry, CancellationToken token = default)
    {
        if (entry.State == EntityState.Added)
        {
            entity.Code ??= $"AST-{Guid.NewGuid():N}".ToUpperInvariant()[..12];
        }
        else if (entry.State == EntityState.Modified)
        {
            entity.Code = (string?)entry.OriginalValues[nameof(Asset.Code)];
        }

        return Task.CompletedTask;
    }
}
