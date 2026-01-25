using System;

namespace Tomeshelf.Paissa.Domain.ValueObjects;

[Flags]
public enum PurchaseSystem
{
    None = 0,
    FreeCompany = 2,
    Personal = 4
}
