namespace Fleet.API.Entities.Interventions;

public enum InterventionSortBy
{
    Default = 0,
    ScheduledDate,
    ScheduledDateDesc,
    CompletedDate,
    CompletedDateDesc,
    Cost,
    CostDesc,
    Status
}
