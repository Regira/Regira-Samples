using System.ComponentModel.DataAnnotations;
using EventPlanner.Api.Entities.Events;
using EventPlanner.Api.Entities.Users;
using Regira.Entities.Attributes;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;

namespace EventPlanner.Api.Entities.Registrations;

public enum RegistrationStatus { Confirmed = 0, Waitlisted = 1, Cancelled = 2 }

/// <summary>An employee's registration for an event, optionally with a selection of its sessions.</summary>
public class Registration : IEntityWithSerial, IHasTimestamps
{
    public int Id { get; set; }

    /// <summary>Immutable once created (a registration never moves to another event).</summary>
    [ServerOwned] public int EventId { get; set; }
    public Event? Event { get; set; }

    /// <summary>The employee. Stamped from the token for employees, chosen by administrators; immutable afterwards.</summary>
    [ServerOwned, Required, MaxLength(450)] public string? UserId { get; set; }
    public AppUser? User { get; set; }

    public RegistrationStatus Status { get; set; } = RegistrationStatus.Confirmed;
    [MaxLength(1000)] public string? Notes { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<RegistrationSession>? Sessions { get; set; }
}

/// <summary>Owned join row Registration x Session (the sessions the employee picked).</summary>
public class RegistrationSession : IEntityWithSerial
{
    public int Id { get; set; }
    public int RegistrationId { get; set; }
    public Registration? Registration { get; set; }
    public int SessionId { get; set; }
    public Session? Session { get; set; }
}

public record RegistrationSearchObject : SearchObject
{
    public ICollection<int>? EventId { get; set; }
    public ICollection<string>? UserId { get; set; }
    public ICollection<RegistrationStatus>? Status { get; set; }
    public ICollection<int>? SessionId { get; set; }
    /// <summary>true = the event has not ended yet.</summary>
    public bool? Upcoming { get; set; }
}

public enum RegistrationSortBy { Created = 0, CreatedAsc, EventDate, EventDateDesc, Employee }

[Flags] public enum RegistrationIncludes { Default = 0, Sessions = 1 << 0, All = Sessions }

public class RegistrationDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public RegistrationEventDto? Event { get; set; }
    public string? UserId { get; set; }
    public EmployeeDto? User { get; set; }
    public RegistrationStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<RegistrationSessionDto>? Sessions { get; set; }
}

/// <summary>Event summary shown on a registration row (no sessions, no heavy text).</summary>
public class RegistrationEventDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public EventStatus Status { get; set; }
    public string? BannerUrl { get; set; }
    public int CategoryId { get; set; }
    public int LocationId { get; set; }
}

public class RegistrationSessionDto
{
    public int Id { get; set; }
    public int RegistrationId { get; set; }
    public int SessionId { get; set; }
    public RegistrationSessionInfoDto? Session { get; set; }
}

public class RegistrationSessionInfoDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Room { get; set; }
}

public class RegistrationInputDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    /// <summary>Only honoured for administrators (on create); employees always register themselves.</summary>
    public string? UserId { get; set; }
    public RegistrationStatus Status { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }
    public ICollection<RegistrationSessionInputDto>? Sessions { get; set; }
}

public class RegistrationSessionInputDto
{
    public int Id { get; set; }
    public int RegistrationId { get; set; }
    public int SessionId { get; set; }
}
