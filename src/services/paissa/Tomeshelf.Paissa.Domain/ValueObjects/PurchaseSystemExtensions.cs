namespace Tomeshelf.Paissa.Domain.ValueObjects;

public static class PurchaseSystemExtensions
{
    public static PurchaseEligibility ToEligibility(this PurchaseSystem system)
    {
        var allowsFreeCompany = system.HasFlag(PurchaseSystem.FreeCompany);
        var allowsPersonal = system.HasFlag(PurchaseSystem.Personal);

        return new PurchaseEligibility(allowsPersonal, allowsFreeCompany);
    }
}