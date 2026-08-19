namespace EventPlanner.Api.Entities.Registrations;

// Employee + Event are to-one navigations shown on every list row: unconditional.
// SelectedSessions is a collection: gated behind the flag.
[Flags]
public enum RegistrationIncludes
{
    Default = 0,
    SelectedSessions = 1 << 0,
    All = SelectedSessions
}
