using Regira.Entities.Models;

namespace QCredits.Api.Entities.GroupTrainings;

public record GroupTrainingSearchObject : SearchObject
{
    public ICollection<string>? Department { get; set; }
    public DateTime? MinTrainingDate { get; set; }
    public DateTime? MaxTrainingDate { get; set; }
}
