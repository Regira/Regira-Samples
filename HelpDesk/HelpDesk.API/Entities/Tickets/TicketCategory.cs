using Regira.Entities.Models.Abstractions;
using HelpDesk.API.Entities.Categories;

namespace HelpDesk.API.Entities.Tickets;

/// <summary>Many-to-many join between Ticket and Category - owned via Ticket's e.Related(), no own slot</summary>
public class TicketCategory : IEntityWithSerial
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
