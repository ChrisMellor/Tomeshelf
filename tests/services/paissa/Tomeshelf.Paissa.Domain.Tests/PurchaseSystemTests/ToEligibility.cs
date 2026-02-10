using Shouldly;
using Tomeshelf.Paissa.Domain.ValueObjects;

namespace Tomeshelf.Paissa.Domain.Tests.PurchaseSystemTests;

public class ToEligibility
{
    /// <summary>
    ///     Returns unknown when the value is a none.
    /// </summary>
    [Fact]
    public void None_ReturnsUnknown()
    {
        // Arrange
        // Act
        var eligibility = PurchaseSystem.None.ToEligibility();

        // Assert
        eligibility.AllowsPersonal.ShouldBeFalse();
        eligibility.AllowsFreeCompany.ShouldBeFalse();
        eligibility.IsUnknown.ShouldBeTrue();
    }

    /// <summary>
    ///     Returns the expected flags.
    /// </summary>
    /// <param name="system">The system.</param>
    /// <param name="allowsPersonal">The allows personal.</param>
    /// <param name="allowsFreeCompany">The allows free company.</param>
    [Theory]
    [InlineData(PurchaseSystem.Personal, true, false)]
    [InlineData(PurchaseSystem.FreeCompany, false, true)]
    [InlineData(PurchaseSystem.Personal | PurchaseSystem.FreeCompany, true, true)]
    public void ReturnsExpectedFlags(PurchaseSystem system, bool allowsPersonal, bool allowsFreeCompany)
    {
        // Arrange
        // Act
        var eligibility = system.ToEligibility();

        // Assert
        eligibility.AllowsPersonal.ShouldBe(allowsPersonal);
        eligibility.AllowsFreeCompany.ShouldBe(allowsFreeCompany);
    }
}