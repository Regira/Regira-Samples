namespace QCredits.Api.Entities.QCreditRequests;

/// <summary>
/// Scoped flag flipped by trusted writers (the approve/reject workflow controller, the seeder) right
/// before they call IEntityService.Modify/Add so QCreditRequestStatusPrimer lets their write through
/// instead of restoring the previous Status/decision fields.
/// </summary>
public class RequestWorkflowContext
{
    public bool IsTrustedWriter { get; set; }
}
