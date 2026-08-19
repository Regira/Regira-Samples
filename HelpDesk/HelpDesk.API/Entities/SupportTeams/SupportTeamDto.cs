namespace HelpDesk.API.Entities.SupportTeams;

public class SupportTeamCoreDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
}

public class SupportTeamDto : SupportTeamCoreDto
{
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public int? MemberCount { get; set; }
    public ICollection<HelpDesk.API.Entities.People.PersonCoreDto>? Members { get; set; }
}
