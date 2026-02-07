using Microsoft.AspNetCore.DataProtection;
using Shouldly;
using Tomeshelf.SHiFT.Infrastructure.Security;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Security.DataProtectionSecretProtectorTests;

public class Unprotect
{
    [Fact]
    public void ReturnsOriginalValue()
    {
        var provider = new EphemeralDataProtectionProvider();
        var protector = new DataProtectionSecretProtector(provider);
        var protectedValue = protector.Protect("secret-value");

        var roundTrip = protector.Unprotect(protectedValue);

        roundTrip.ShouldBe("secret-value");
    }
}