namespace HelpDesk.API.Entities.Tickets;

public enum TicketSortBy
{
    Default = 0,
    Created,
    CreatedDesc,
    Priority,
    PriorityDesc,
    Title,
    TitleDesc,
    Status
}
