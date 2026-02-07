using Shouldly;
using Tomeshelf.Paissa.Domain.ValueObjects;

namespace Tomeshelf.Paissa.Domain.Tests.PurchaseSystemTests;

public class ToEligibility
{
    [Fact]
    public void None_ReturnsUnknown()
    {
        var eligibility = PurchaseSystem.None.ToEligibility();

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
        var eligibility = system.ToEligibility();

        eligibility.AllowsPersonal.ShouldBe(allowsPersonal);
        eligibility.AllowsFreeCompany.ShouldBe(allowsFreeCompany);
    }
}