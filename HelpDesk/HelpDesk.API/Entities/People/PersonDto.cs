using HelpDesk.API.Entities.SupportTeams;

namespace HelpDesk.API.Entities.People;

public class PersonCoreDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public PersonRole Role { get; set; }
    public bool IsActive { get; set; }
}

public class PersonDto : PersonCoreDto
{
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public int? SupportTeamId { get; set; }
    public SupportTeamCoreDto? SupportTeam { get; set; }
    public int? AssignedTicketCount { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
