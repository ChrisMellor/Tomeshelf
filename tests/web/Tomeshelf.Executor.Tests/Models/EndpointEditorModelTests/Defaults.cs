using Shouldly;
using Tomeshelf.Executor.Models;

namespace Tomeshelf.Executor.Tests.Models.EndpointEditorModelTests;

public class Defaults
{
    [Fact]
    public void AreExpected()
    {
        var model = new EndpointEditorModel();

        var method = model.Method;
        var enabled = model.Enabled;

        method.ShouldBe("POST");
        enabled.ShouldBeTrue();
    }
}