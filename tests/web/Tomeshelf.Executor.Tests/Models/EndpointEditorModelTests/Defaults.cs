using Shouldly;
using Tomeshelf.Executor.Models;

namespace Tomeshelf.Executor.Tests.Models.EndpointEditorModelTests;

public class Defaults
{
    [Fact]
    public void AreExpected()
    {
        // Arrange
        var model = new EndpointEditorModel();

        var method = model.Method;
        // Act
        var enabled = model.Enabled;

        // Assert
        method.ShouldBe("POST");
        enabled.ShouldBeTrue();
    }
}