using Shouldly;
using Tomeshelf.SHiFT.Infrastructure.Exceptions;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Exceptions.MissingConnectionStringExceptionTests;

public class Message
{
    [Fact]
    public void IncludesConnectionString()
    {
        // Arrange
        var exception = new MissingConnectionStringException("missing-conn");

        // Act
        var message = exception.Message;

        // Assert
        message.ShouldBe("The connection string: missing-conn is invalid");
    }
}