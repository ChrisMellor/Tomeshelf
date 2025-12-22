using System;

namespace Tomeshelf.Domain.Entities.Mcm;

public class GuestEntity
{
    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }

    public Guid GuestInfoId { get; set; }

    public GuestInfoEntity Information { get; set; }

    public Guid EventId { get; set; }

    public EventEntity Event { get; set; }
}