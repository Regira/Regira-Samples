namespace HelpDesk.API.Entities.Tickets;

/// <summary>
/// Customer / AssignedEmployee / Priority / Status / SupportTeam are cheap to-one references shown on
/// every list row and always eager-loaded - only the three collections are flag-gated.
/// </summary>
[Flags]
public enum TicketIncludes
{
    Default = 0,
    Categories = 1 << 0,
    Comments = 1 << 1,
    Attachments = 1 << 2,
    All = Categories | Comments | Attachments
}
