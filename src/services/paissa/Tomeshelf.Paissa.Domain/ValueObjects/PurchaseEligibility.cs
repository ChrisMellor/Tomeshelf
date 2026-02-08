namespace Tomeshelf.Paissa.Domain.ValueObjects;

public sealed record PurchaseEligibility(bool AllowsPersonal, bool AllowsFreeCompany)
{
    public bool IsUnknown => !AllowsPersonal && !AllowsFreeCompany;
}