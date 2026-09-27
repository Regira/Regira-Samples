using Regira.Entities.Attachments.Models;

namespace AssetHub.Api.Entities.Assets;

public class AssetAttachment : EntityAttachment
{
    public AssetAttachment() => ObjectType = nameof(Asset);
}
