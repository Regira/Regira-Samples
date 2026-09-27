namespace AssetHub.Api.Services;

/// <summary>
/// Scoped flag flipped by trusted writers (the assign / return workflow and the seeder).
/// AssetPrepper restores the workflow-owned fields on every write that does not set it.
/// </summary>
public sealed class WorkflowContext
{
    public bool IsTrustedWriter { get; set; }
}
