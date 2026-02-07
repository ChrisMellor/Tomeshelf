using Shouldly;
using Tomeshelf.SHiFT.Infrastructure.Exceptions;

namespace Tomeshelf.SHiFT.Infrastructure.Tests.Exceptions.MissingConnectionStringExceptionTests;

public class Message
{
    [Fact]
    public void IncludesConnectionString()
    {
        var exception = new MissingConnectionStringException("missing-conn");

        var message = exception.Message;

        message.ShouldBe("The connection string: missing-conn is invalid");
    }
}