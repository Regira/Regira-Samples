namespace QCredits.Api.Entities.QCreditRequests;

public class QCreditRequestItemInputDto
{
    public int Id { get; set; }
    public string Description { get; set; } = null!;
    public CreditActivityType Type { get; set; }
    public decimal Credits { get; set; }
    public DateTime ActivityDate { get; set; }
    public decimal? Cost { get; set; }
    public string? Provider { get; set; }
}
