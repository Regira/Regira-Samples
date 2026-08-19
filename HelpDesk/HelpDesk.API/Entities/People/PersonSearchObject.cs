using Regira.Entities.Models;

namespace HelpDesk.API.Entities.People;

public record PersonSearchObject : SearchObject
{
    public ICollection<PersonRole>? Role { get; set; }
    public ICollection<int>? SupportTeamId { get; set; }
    public bool? IsActive { get; set; }
}
