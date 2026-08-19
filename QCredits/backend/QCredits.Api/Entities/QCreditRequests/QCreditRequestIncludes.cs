namespace QCredits.Api.Entities.QCreditRequests;

[Flags]
public enum QCreditRequestIncludes
{
    Default = 0,
    Items = 1 << 0,
    All = Items
}
