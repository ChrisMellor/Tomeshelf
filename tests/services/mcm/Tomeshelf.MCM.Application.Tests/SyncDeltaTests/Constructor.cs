using Shouldly;
using Tomeshelf.MCM.Application.Records;

namespace Tomeshelf.MCM.Application.Tests.SyncDeltaTests;

public class Constructor
{
    [Fact]
    public void AssignsProperties()
    {
        var added = 5;
        var updated = 3;
        var removed = 1;
        var total = 10;

        var syncDelta = new SyncDelta(added, updated, removed, total);

        syncDelta.Added.ShouldBe(added);
        syncDelta.Updated.ShouldBe(updated);
        syncDelta.Removed.ShouldBe(removed);
        syncDelta.Total.ShouldBe(total);
    }
}