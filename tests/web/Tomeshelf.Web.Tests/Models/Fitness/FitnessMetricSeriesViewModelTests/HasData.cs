using System.Collections.Generic;
using Bogus;
using FluentAssertions;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.FitnessMetricSeriesViewModelTests;

public class HasData
{
    [Fact]
    public void WhenAnyValuePresent_ReturnsTrue()
    {
        // Arrange
        var faker = new Faker();
        var model = new FitnessMetricSeriesViewModel
        {
            Key = faker.Random.Word(),
            Title = faker.Lorem.Word(),
            Unit = faker.Random.Word(),
            Values = new List<double?> { null, 10, null }
        };

        // Act
        var hasData = model.HasData;

        // Assert
        hasData.Should().BeTrue();
    }

    [Fact]
    public void WhenAllValuesNull_ReturnsFalse()
    {
        // Arrange
        var faker = new Faker();
        var model = new FitnessMetricSeriesViewModel
        {
            Key = faker.Random.Word(),
            Title = faker.Lorem.Word(),
            Unit = faker.Random.Word(),
            Values = new List<double?> { null, null }
        };

        // Act
        var hasData = model.HasData;

        // Assert
        hasData.Should().BeFalse();
    }

    [Fact]
    public void WhenValuesNull_ReturnsFalse()
    {
        // Arrange
        var faker = new Faker();
        var model = new FitnessMetricSeriesViewModel
        {
            Key = faker.Random.Word(),
            Title = faker.Lorem.Word(),
            Unit = faker.Random.Word(),
            Values = null
        };

        // Act
        var hasData = model.HasData;

        // Assert
        hasData.Should().BeFalse();
    }
}
