using Shouldly;
using Tomeshelf.MCM.Application.Mcm;

namespace Tomeshelf.MCM.Application.Tests.CategoryDtoTests;

public class Properties
{
    [Fact]
    public void CanSetAndGetValues()
    {
        var id = "category-id-1";
        var name = "Category Name";
        var color = "#FFFFFF";

        var dto = new CategoryDto
        {
            Id = id,
            Name = name,
            Color = color
        };

        dto.Id.ShouldBe(id);
        dto.Name.ShouldBe(name);
        dto.Color.ShouldBe(color);
    }

    [Fact]
    public void DefaultsAreNull()
    {
        var dto = new CategoryDto();

        var id = dto.Id;
        var name = dto.Name;
        var color = dto.Color;

        id.ShouldBeNull();
        name.ShouldBeNull();
        color.ShouldBeNull();
    }
}