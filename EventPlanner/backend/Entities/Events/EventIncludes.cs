namespace EventPlanner.Api.Entities.Events;

// Location + EventCategory are to-one navigations shown on every list row, so they are eager-loaded
// unconditionally by the query builder. Sessions is a collection — gated behind the flag.
[Flags]
public enum EventIncludes
{
    Default = 0,
    Sessions = 1 << 0,
    All = Sessions
}
