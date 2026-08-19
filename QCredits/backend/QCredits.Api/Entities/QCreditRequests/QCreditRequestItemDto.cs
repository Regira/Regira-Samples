namespace QCredits.Api.Entities.QCreditRequests;

public class QCreditRequestItemDto
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public string Description { get; set; } = null!;
    public CreditActivityType Type { get; set; }
    public decimal Credits { get; set; }
    public DateTime ActivityDate { get; set; }
    public decimal? Cost { get; set; }
    public string? Provider { get; set; }
}
