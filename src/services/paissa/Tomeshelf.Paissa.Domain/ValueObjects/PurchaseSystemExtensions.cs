namespace Tomeshelf.Paissa.Domain.ValueObjects;

public static class PurchaseSystemExtensions
{
    /// <summary>
    ///     Tos the eligibility.
    /// </summary>
    /// <param name="system">The system.</param>
    /// <returns>The result of the operation.</returns>
    public static PurchaseEligibility ToEligibility(this PurchaseSystem system)
    {
        var allowsFreeCompany = system.HasFlag(PurchaseSystem.FreeCompany);
        var allowsPersonal = system.HasFlag(PurchaseSystem.Personal);

        return new PurchaseEligibility(allowsPersonal, allowsFreeCompany);
    }
}