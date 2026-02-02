using FluentAssertions;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Tests.Services.FitbitBackendUnavailableExceptionTests;

public class Message
{
    [Fact]
    public void WhenMessageIsEmpty_ReturnsDefault()
    {
        // Act
        var exception = new FitbitBackendUnavailableException(" ");

        // Assert
        exception.Message.Should().Be("Fitbit service is unavailable. Please try again in a moment.");
    }

    [Fact]
    public void WhenMessageIsHtml_ReturnsDefault()
    {
        // Act
        var exception = new FitbitBackendUnavailableException("<html>bad gateway</html>");

        // Assert
        exception.Message.Should().Be("Fitbit service is unavailable. Please try again in a moment.");
    }

    [Fact]
    public void WhenMessageIsJsonObject_ExtractsMessage()
    {
        // Act
        var exception = new FitbitBackendUnavailableException("{\"message\":\"Oops\"}");

        // Assert
        exception.Message.Should().Be("Oops");
    }

    [Fact]
    public void WhenMessageIsJsonString_ParsesString()
    {
        // Act
        var exception = new FitbitBackendUnavailableException("\"Too busy\"");

        // Assert
        exception.Message.Should().Be("Too busy");
    }
}
