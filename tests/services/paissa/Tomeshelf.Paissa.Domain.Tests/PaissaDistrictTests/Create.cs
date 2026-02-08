using Shouldly;
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
        var exception = Should.Throw<ArgumentException>(() => PaissaDistrict.Create(1, string.Empty, plots));

        // Assert
        exception.ShouldNotBeNull();
    }

    [Fact]
    public void InvalidId_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var plots = new List<PaissaPlot>();

        // Act
        var exception = Should.Throw<ArgumentOutOfRangeException>(() => PaissaDistrict.Create(0, "Mist", plots));

        // Assert
        exception.ShouldNotBeNull();
    }

    [Fact]
    public void NullName_ThrowsArgumentException()
    {
        // Arrange
        var plots = new List<PaissaPlot>();

        // Act
        var exception = Should.Throw<ArgumentException>(() => PaissaDistrict.Create(1, null!, plots));

        // Assert
        exception.ShouldNotBeNull();
    }

    [Fact]
    public void NullPlots_ThrowsArgumentNullException()
    {
        // Arrange
        // Act
        var exception = Should.Throw<ArgumentNullException>(() => PaissaDistrict.Create(1, "Mist", null!));

        // Assert
        exception.ShouldNotBeNull();
    }

    [Fact]
    public void PlotsWithNullEntry_ThrowsArgumentException()
    {
        // Arrange
        var plots = new List<PaissaPlot> { null! };

        // Act
        var exception = Should.Throw<ArgumentException>(() => PaissaDistrict.Create(1, "Mist", plots));

        // Assert
        exception.ShouldNotBeNull();
    }

    [Fact]
    public void ValidParameters_ReturnsPaissaDistrict()
    {
        // Arrange
        var plots = new List<PaissaPlot>();

        // Act
        var district = PaissaDistrict.Create(1, "Mist", plots);

        // Assert
        district.ShouldNotBeNull();
        district.Id.ShouldBe(1);
        district.Name.ShouldBe("Mist");
        district.OpenPlots.ShouldBeEmpty();
    }
}