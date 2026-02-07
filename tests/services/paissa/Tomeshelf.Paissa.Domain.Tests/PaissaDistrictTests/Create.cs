using Shouldly;
using Tomeshelf.Paissa.Domain.Entities;

namespace Tomeshelf.Paissa.Domain.Tests.PaissaDistrictTests;

public class Create
{
    [Fact]
    public void EmptyName_ThrowsArgumentException()
    {
        var plots = new List<PaissaPlot>();

        var exception = Should.Throw<ArgumentException>(() => PaissaDistrict.Create(1, string.Empty, plots));

        exception.ShouldNotBeNull();
    }

    [Fact]
    public void InvalidId_ThrowsArgumentOutOfRangeException()
    {
        var plots = new List<PaissaPlot>();

        var exception = Should.Throw<ArgumentOutOfRangeException>(() => PaissaDistrict.Create(0, "Mist", plots));

        exception.ShouldNotBeNull();
    }

    [Fact]
    public void NullName_ThrowsArgumentException()
    {
        var plots = new List<PaissaPlot>();

        var exception = Should.Throw<ArgumentException>(() => PaissaDistrict.Create(1, null!, plots));

        exception.ShouldNotBeNull();
    }

    [Fact]
    public void NullPlots_ThrowsArgumentNullException()
    {
        var exception = Should.Throw<ArgumentNullException>(() => PaissaDistrict.Create(1, "Mist", null!));

        exception.ShouldNotBeNull();
    }

    [Fact]
    public void PlotsWithNullEntry_ThrowsArgumentException()
    {
        var plots = new List<PaissaPlot> { null! };

        var exception = Should.Throw<ArgumentException>(() => PaissaDistrict.Create(1, "Mist", plots));

        exception.ShouldNotBeNull();
    }

    [Fact]
    public void ValidParameters_ReturnsPaissaDistrict()
    {
        var plots = new List<PaissaPlot>();

        var district = PaissaDistrict.Create(1, "Mist", plots);

        district.ShouldNotBeNull();
        district.Id.ShouldBe(1);
        district.Name.ShouldBe("Mist");
        district.OpenPlots.ShouldBeEmpty();
    }
}