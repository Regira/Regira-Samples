using System.ComponentModel.DataAnnotations;
using QCredits.Api.Entities.Employees;
using Regira.Entities.Attributes;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace QCredits.Api.Entities.CreditRequests;

public enum RequestStatus { Draft = 0, Submitted = 1, Approved = 2, Rejected = 3, Cancelled = 4 }

public enum ActivityType { Course = 0, Book = 1, OnlineSubscription = 2, SelfStudy = 3, Conference = 4, Certification = 5, Other = 6 }

/// <summary>
/// A QCredit request: one or more purchases / activities an employee wants to spend credits on.
/// Only approved requests deduct credits from the balance.
/// </summary>
public class CreditRequest : IEntityWithSerial, IHasTitle, IHasDescription, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [ServerOwned] public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    [Range(2000, 2100)] public int Year { get; set; }
    [Required, MaxLength(150)] public string? Title { get; set; }
    /// <summary>Motivation: why the employee wants this training.</summary>
    [MaxLength(2000)] public string? Description { get; set; }

    // workflow fields: only the workflow actions write these (see CreditRequestGuard)
    public RequestStatus Status { get; set; } = RequestStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
    [MaxLength(256)] public string? DecidedBy { get; set; }
    [MaxLength(1000)] public string? DecisionComment { get; set; }

    /// <summary>Sum of the item credits (computed server-side).</summary>
    public double TotalCredits { get; set; }
    /// <summary>Sum of the item costs in EUR (computed server-side, informational).</summary>
    public decimal TotalCost { get; set; }

    public ICollection<CreditRequestItem>? Items { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }
}

/// <summary>A single purchase or activity inside a request (owned by the request).</summary>
public class CreditRequestItem : IEntityWithSerial, ISortable
{
    public int Id { get; set; }
    public int CreditRequestId { get; set; }
    public CreditRequest? CreditRequest { get; set; }
    public ActivityType Type { get; set; }
    [Required, MaxLength(200)] public string? Description { get; set; }
    [MaxLength(150)] public string? Provider { get; set; }
    public DateOnly? ActivityDate { get; set; }
    /// <summary>1 QCredit = half a working day or EUR 250.</summary>
    [Range(0.5, 40)] public double Credits { get; set; }
    /// <summary>Estimated cost in EUR (informational).</summary>
    [Range(0, 100000)] public decimal Cost { get; set; }
    [MaxLength(500)] public string? Url { get; set; }
    public int SortOrder { get; set; }
}

public record CreditRequestSearchObject : SearchObject
{
    public ICollection<int>? EmployeeId { get; set; }
    public ICollection<int>? DepartmentId { get; set; }
    public int? Year { get; set; }
    public ICollection<RequestStatus>? Status { get; set; }
    public ICollection<ActivityType>? ActivityType { get; set; }
}

public enum CreditRequestSortBy
{
    Default = 0,
    Newest,
    Oldest,
    SubmittedAt,
    SubmittedAtDesc,
    CreditsDesc,
    Credits,
    Employee,
    Title,
    Status
}

[Flags]
public enum CreditRequestIncludes
{
    Default = 0,
    Items = 1 << 0,
    All = Items
}

public class CreditRequestDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public EmployeeDto? Employee { get; set; }
    public int Year { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public RequestStatus Status { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? DecidedBy { get; set; }
    public string? DecisionComment { get; set; }
    public double TotalCredits { get; set; }
    public decimal TotalCost { get; set; }
    public ICollection<CreditRequestItemDto>? Items { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class CreditRequestItemDto
{
    public int Id { get; set; }
    public int CreditRequestId { get; set; }
    public ActivityType Type { get; set; }
    public string? Description { get; set; }
    public string? Provider { get; set; }
    public DateOnly? ActivityDate { get; set; }
    public double Credits { get; set; }
    public decimal Cost { get; set; }
    public string? Url { get; set; }
    public int SortOrder { get; set; }
}

public class CreditRequestInputDto
{
    public int Id { get; set; }
    /// <summary>Only administrators may choose the employee; for others it is taken from the signed-in user.</summary>
    public int EmployeeId { get; set; }
    [Range(2000, 2100)] public int Year { get; set; }
    [Required, MaxLength(150)] public string? Title { get; set; }
    [MaxLength(2000)] public string? Description { get; set; }
    public ICollection<CreditRequestItemInputDto>? Items { get; set; }
}

public class CreditRequestItemInputDto
{
    public int Id { get; set; }
    public ActivityType Type { get; set; }
    [Required, MaxLength(200)] public string? Description { get; set; }
    [MaxLength(150)] public string? Provider { get; set; }
    public DateOnly? ActivityDate { get; set; }
    [Range(0.5, 40)] public double Credits { get; set; }
    [Range(0, 100000)] public decimal Cost { get; set; }
    [MaxLength(500)] public string? Url { get; set; }
    public int SortOrder { get; set; }
}
