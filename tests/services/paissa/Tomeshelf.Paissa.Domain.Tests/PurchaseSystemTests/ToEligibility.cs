using FluentAssertions;
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
        eligibility.AllowsPersonal
                   .Should()
                   .BeFalse();
        eligibility.AllowsFreeCompany
                   .Should()
                   .BeFalse();
        eligibility.IsUnknown
                   .Should()
                   .BeTrue();
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
        eligibility.AllowsPersonal
                   .Should()
                   .Be(allowsPersonal);
        eligibility.AllowsFreeCompany
                   .Should()
                   .Be(allowsFreeCompany);
    }
}