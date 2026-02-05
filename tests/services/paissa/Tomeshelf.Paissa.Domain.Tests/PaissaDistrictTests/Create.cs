using FluentAssertions;
using Tomeshelf.Paissa.Domain.Entities;

namespace Tomeshelf.Paissa.Domain.Tests.PaissaDistrictTests;

public class Create
{
    [Fact]
    public void EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var plots = new List<PaissaPlot>();

        // Act
        Action act = () => PaissaDistrict.Create(1, string.Empty, plots);

        // Assert
        act.Should()
           .Throw<ArgumentException>();
    }

    [Fact]
    public void InvalidId_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var plots = new List<PaissaPlot>();

        // Act
        Action act = () => PaissaDistrict.Create(0, "Mist", plots);

        // Assert
        act.Should()
           .Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void NullName_ThrowsArgumentException()
    {
        // Arrange
        var plots = new List<PaissaPlot>();

        // Act
        Action act = () => PaissaDistrict.Create(1, null!, plots);

        // Assert
        act.Should()
           .Throw<ArgumentException>();
    }

    [Fact]
    public void NullPlots_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => PaissaDistrict.Create(1, "Mist", null!);

        // Assert
        act.Should()
           .Throw<ArgumentNullException>();
    }

    [Fact]
    public void PlotsWithNullEntry_ThrowsArgumentException()
    {
        // Arrange
        var plots = new List<PaissaPlot> { null! };

        // Act
        Action act = () => PaissaDistrict.Create(1, "Mist", plots);

        // Assert
        act.Should()
           .Throw<ArgumentException>();
    }

    [Fact]
    public void ValidParameters_ReturnsPaissaDistrict()
    {
        // Arrange
        var plots = new List<PaissaPlot>();

        // Act
        var district = PaissaDistrict.Create(1, "Mist", plots);

        // Assert
        district.Should()
                .NotBeNull();
        district.Id
                .Should()
                .Be(1);
        district.Name
                .Should()
                .Be("Mist");
        district.OpenPlots
                .Should()
                .BeEmpty();
    }
}