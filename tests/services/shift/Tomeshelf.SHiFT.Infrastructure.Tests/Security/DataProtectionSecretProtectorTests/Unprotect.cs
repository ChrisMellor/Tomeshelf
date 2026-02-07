using Microsoft.AspNetCore.DataProtection;
using Tomeshelf.SHiFT.Infrastructure.Security;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Security.DataProtectionSecretProtectorTests;

public class Unprotect
{
    [Fact]
    public void ReturnsOriginalValue()
    {
        // Arrange
        var provider = new EphemeralDataProtectionProvider();
        var protector = new DataProtectionSecretProtector(provider);
        var protectedValue = protector.Protect("secret-value");

        // Act
        var roundTrip = protector.Unprotect(protectedValue);

        // Assert
        roundTrip.ShouldBe("secret-value");
    }
}
