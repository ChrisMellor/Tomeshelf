using System.Collections.Generic;
using Tomeshelf.Web.Models.Fitness;
using Xunit;

namespace Tomeshelf.Web.Tests.Models.Fitness;

public class FitnessMetricSeriesViewModelTests
{
    [Fact]
    public void HasData_WhenAnyValuePresent_ReturnsTrue()
    {
        var model = new FitnessMetricSeriesViewModel
        {
            Key = "steps",
            Title = "Steps",
            Unit = "count",
            Values = new List<double?> { null, 10, null }
        };

        Assert.True(model.HasData);
    }

    [Fact]
    public void HasData_WhenAllValuesNull_ReturnsFalse()
    {
        var model = new FitnessMetricSeriesViewModel
        {
            Key = "steps",
            Title = "Steps",
            Unit = "count",
            Values = new List<double?> { null, null }
        };

        Assert.False(model.HasData);
    }
}
