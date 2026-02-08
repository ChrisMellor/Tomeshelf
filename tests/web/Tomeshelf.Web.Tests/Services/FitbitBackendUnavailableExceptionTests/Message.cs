using Shouldly;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web.Tests.Services.FitbitBackendUnavailableExceptionTests;

public class Message
{
    [Fact]
    public void WhenMessageIsEmpty_ReturnsDefault()
    {
        // Arrange
        // Act
        var exception = new FitbitBackendUnavailableException(" ");

        // Assert
        exception.Message.ShouldBe("Fitbit service is unavailable. Please try again in a moment.");
    }

    [Fact]
    public void WhenMessageIsHtml_ReturnsDefault()
    {
        // Arrange
        // Act
        var exception = new FitbitBackendUnavailableException("<html>bad gateway</html>");

        // Assert
        exception.Message.ShouldBe("Fitbit service is unavailable. Please try again in a moment.");
    }

    [Fact]
    public void WhenMessageIsJsonObject_ExtractsMessage()
    {
        // Arrange
        // Act
        var exception = new FitbitBackendUnavailableException("{\"message\":\"Oops\"}");

        // Assert
        exception.Message.ShouldBe("Oops");
    }

    [Fact]
    public void WhenMessageIsJsonString_ParsesString()
    {
        // Arrange
        // Act
        var exception = new FitbitBackendUnavailableException("\"Too busy\"");

        // Assert
        exception.Message.ShouldBe("Too busy");
    }
}