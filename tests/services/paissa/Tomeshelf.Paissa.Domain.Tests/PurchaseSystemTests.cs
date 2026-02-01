using Tomeshelf.Paissa.Domain.ValueObjects;
using Xunit;

namespace Tomeshelf.Paissa.Domain.Tests;

public class PurchaseSystemTests
{
    [Fact]
    public void ToEligibility_None_ReturnsUnknown()
    {
        // Act
        var eligibility = PurchaseSystem.None.ToEligibility();

        // Assert
        Assert.False(eligibility.AllowsPersonal);
        Assert.False(eligibility.AllowsFreeCompany);
        Assert.True(eligibility.IsUnknown);
    }

    [Theory]
    [InlineData(PurchaseSystem.Personal, true, false)]
    [InlineData(PurchaseSystem.FreeCompany, false, true)]
    [InlineData(PurchaseSystem.Personal | PurchaseSystem.FreeCompany, true, true)]
    public void ToEligibility_ReturnsExpectedFlags(PurchaseSystem system, bool allowsPersonal, bool allowsFreeCompany)
    {
        // Act
        var eligibility = system.ToEligibility();

        // Assert
        Assert.Equal(allowsPersonal, eligibility.AllowsPersonal);
        Assert.Equal(allowsFreeCompany, eligibility.AllowsFreeCompany);
    }
}
