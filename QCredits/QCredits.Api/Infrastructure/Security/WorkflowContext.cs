namespace QCredits.Api.Infrastructure.Security;

/// <summary>
/// Scoped flag for trusted writers (workflow actions, seeder, roll-over job).
/// While set, row scoping is lifted and the workflow fields of a credit request may be written.
/// </summary>
public sealed class WorkflowContext
{
    public bool IsTrustedWriter { get; set; }
}
