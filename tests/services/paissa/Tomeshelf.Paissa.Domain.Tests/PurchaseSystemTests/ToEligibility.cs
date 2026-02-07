using Tomeshelf.Paissa.Domain.ValueObjects;

namespace Tomeshelf.Paissa.Domain.Tests.PurchaseSystemTests;

public class ToEligibility
{
    [Fact]
    public void None_ReturnsUnknown()
    {
        // Act
        var eligibility = PurchaseSystem.None.ToEligibility();

        // Assert
        eligibility.AllowsPersonal.ShouldBeFalse();
        eligibility.AllowsFreeCompany.ShouldBeFalse();
        eligibility.IsUnknown.ShouldBeTrue();
    }

    [Theory]
    [InlineData(PurchaseSystem.Personal, true, false)]
    [InlineData(PurchaseSystem.FreeCompany, false, true)]
    [InlineData(PurchaseSystem.Personal | PurchaseSystem.FreeCompany, true, true)]
    public void ReturnsExpectedFlags(PurchaseSystem system, bool allowsPersonal, bool allowsFreeCompany)
    {
        // Act
        var eligibility = system.ToEligibility();

        // Assert
        eligibility.AllowsPersonal.ShouldBe(allowsPersonal);
        eligibility.AllowsFreeCompany.ShouldBe(allowsFreeCompany);
    }
}
