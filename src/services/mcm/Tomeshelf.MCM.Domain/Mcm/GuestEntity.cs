using System;

namespace Tomeshelf.MCM.Domain.Mcm;

public class GuestEntity
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GuestEntity" /> class.
    /// </summary>
    public GuestEntity()
    {
        Id = Guid.NewGuid();
        AddedAt = DateTimeOffset.UtcNow;
        IsDeleted = false;
        Information = new GuestInfoEntity();
        GuestInfoId = Information.Id; 
    }

    public Guid Id { get; set; }

    public DateTimeOffset AddedAt { get; set; }

    public DateTimeOffset? RemovedAt { get; set; }

    public bool IsDeleted { get; set; }

    public Guid GuestInfoId { get; set; }

    public GuestInfoEntity? Information { get; set; }

    public string? EventId { get; set; }

    public EventEntity? Event { get; set; }
}
