using Shouldly;
using Tomeshelf.MCM.Infrastructure.Responses;

namespace Tomeshelf.MCM.Infrastructure.Tests.McmEventResponseTests;

public class GlobalCategory
{
    [Fact]
    public void CanSetAndGetValues()
    {
        var id = "cat-id-1";
        var name = "Category A";
        var colour = "#FF0000";

        var globalCategory = new McmEventResponse.GlobalCategory
        {
            Id = id,
            Name = name,
            Colour = colour
        };

        globalCategory.Id.ShouldBe(id);
        globalCategory.Name.ShouldBe(name);
        globalCategory.Colour.ShouldBe(colour);
    }
}