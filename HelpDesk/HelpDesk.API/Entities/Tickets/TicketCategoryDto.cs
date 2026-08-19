using HelpDesk.API.Entities.Categories;

namespace HelpDesk.API.Entities.Tickets;

public class TicketCategoryDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int CategoryId { get; set; }
    public CategoryCoreDto? Category { get; set; }
}

public class TicketCategoryInputDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int CategoryId { get; set; }
}
