using System.Collections.Generic;
using Bogus;
using Shouldly;
using Tomeshelf.Web.Models.Fitness;

namespace Tomeshelf.Web.Tests.Models.Fitness.FitnessMetricSeriesViewModelTests;

public class HasData
{
    [Fact]
    public void WhenAllValuesNull_ReturnsFalse()
    {
        // Arrange
        var faker = new Faker();
        // Act
        var model = new FitnessMetricSeriesViewModel
        {
            Key = faker.Random.Word(),
            Title = faker.Lorem.Word(),
            Unit = faker.Random.Word(),
            Values = new List<double?>
            {
                null,
                null
            }
        };

        // Assert
        var hasData = model.HasData;

        hasData.ShouldBeFalse();
    }

    [Fact]
    public void WhenAnyValuePresent_ReturnsTrue()
    {
        // Arrange
        var faker = new Faker();
        // Act
        var model = new FitnessMetricSeriesViewModel
        {
            Key = faker.Random.Word(),
            Title = faker.Lorem.Word(),
            Unit = faker.Random.Word(),
            Values = new List<double?>
            {
                null,
                10,
                null
            }
        };

        // Assert
        var hasData = model.HasData;

        hasData.ShouldBeTrue();
    }

    [Fact]
    public void WhenValuesNull_ReturnsFalse()
    {
        // Arrange
        var faker = new Faker();
        // Act
        var model = new FitnessMetricSeriesViewModel
        {
            Key = faker.Random.Word(),
            Title = faker.Lorem.Word(),
            Unit = faker.Random.Word(),
            Values = null
        };

        // Assert
        var hasData = model.HasData;

        hasData.ShouldBeFalse();
    }
}