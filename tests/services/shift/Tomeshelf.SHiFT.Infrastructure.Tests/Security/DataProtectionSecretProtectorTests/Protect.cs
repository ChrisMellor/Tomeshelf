using Microsoft.AspNetCore.DataProtection;
using Shouldly;
using Tomeshelf.SHiFT.Infrastructure.Security;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Security.DataProtectionSecretProtectorTests;

public class Protect
{
    [Fact]
    public void ReturnsProtectedValue()
    {
        // Arrange
        var provider = new EphemeralDataProtectionProvider();
        var protector = new DataProtectionSecretProtector(provider);

        // Act
        var protectedValue = protector.Protect("secret-value");

        // Assert
        string.IsNullOrWhiteSpace(protectedValue)
              .ShouldBeFalse();
        protectedValue.ShouldNotBe("secret-value");
    }
}