using Shouldly;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Tests.Services.FitbitBackendUnavailableExceptionTests;

public class Message
{
    [Fact]
    public void WhenMessageIsEmpty_ReturnsDefault()
    {
        var exception = new FitbitBackendUnavailableException(" ");

        exception.Message.ShouldBe("Fitbit service is unavailable. Please try again in a moment.");
    }

    [Fact]
    public void WhenMessageIsHtml_ReturnsDefault()
    {
        var exception = new FitbitBackendUnavailableException("<html>bad gateway</html>");

        exception.Message.ShouldBe("Fitbit service is unavailable. Please try again in a moment.");
    }

    [Fact]
    public void WhenMessageIsJsonObject_ExtractsMessage()
    {
        var exception = new FitbitBackendUnavailableException("{\"message\":\"Oops\"}");

        exception.Message.ShouldBe("Oops");
    }

    [Fact]
    public void WhenMessageIsJsonString_ParsesString()
    {
        var exception = new FitbitBackendUnavailableException("\"Too busy\"");

        exception.Message.ShouldBe("Too busy");
    }
}