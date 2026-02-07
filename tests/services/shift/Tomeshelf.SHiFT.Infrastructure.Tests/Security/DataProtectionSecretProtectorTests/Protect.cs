using Microsoft.AspNetCore.DataProtection;
using Shouldly;
using Tomeshelf.SHiFT.Infrastructure.Security;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Security.DataProtectionSecretProtectorTests;

public class Protect
{
    [Fact]
    public void ReturnsProtectedValue()
    {
        var provider = new EphemeralDataProtectionProvider();
        var protector = new DataProtectionSecretProtector(provider);

        var protectedValue = protector.Protect("secret-value");

        string.IsNullOrWhiteSpace(protectedValue)
              .ShouldBeFalse();
        protectedValue.ShouldNotBe("secret-value");
    }
}