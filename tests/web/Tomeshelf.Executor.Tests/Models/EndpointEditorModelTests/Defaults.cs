using Shouldly;
using Tomeshelf.Executor.Models;

namespace Tomeshelf.Executor.Tests.Models.EndpointEditorModelTests;

public class Defaults
{
    /// <summary>
    ///     Determines whether the current instance is an expected.
    /// </summary>
    [Fact]
    public void AreExpected()
    {
        // Arrange
        var model = new EndpointEditorModel();

        // Act
        var method = model.Method;
        var enabled = model.Enabled;

        // Assert
        method.ShouldBe("POST");
        enabled.ShouldBeTrue();
    }
}