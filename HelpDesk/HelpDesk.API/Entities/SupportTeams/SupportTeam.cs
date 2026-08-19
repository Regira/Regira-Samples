using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Regira.Entities.Models.Abstractions;

namespace HelpDesk.API.Entities.SupportTeams;

public class SupportTeam : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription
{
    public int Id { get; set; }
    [Required, MaxLength(64)] public string Title { get; set; } = null!;
    [MaxLength(512)] public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    // Independent entity back-ref - not owned, loaded via Include() in the query builder
    public ICollection<HelpDesk.API.Entities.People.Person>? Members { get; set; }

    [NotMapped] public int? MemberCount { get; set; }
}
