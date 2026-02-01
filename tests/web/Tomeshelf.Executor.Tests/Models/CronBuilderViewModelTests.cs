using System;
using Tomeshelf.Executor.Models;
using Xunit;

namespace Tomeshelf.Executor.Tests.Models;

public class CronBuilderViewModelTests
{
    [Fact]
    public void Constructor_WhenNullInputId_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CronBuilderViewModel(null!, "name", "* * * * *"));
    }

    [Fact]
    public void Constructor_WhenNullInputName_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CronBuilderViewModel("id", null!, "* * * * *"));
    }

    [Fact]
    public void Constructor_WhenInitialValueNull_UsesEmptyString()
    {
        var model = new CronBuilderViewModel("id", "name", null);

        Assert.Equal(string.Empty, model.InitialValue);
        Assert.Equal("id", model.InputId);
        Assert.Equal("name", model.InputName);
    }
}
