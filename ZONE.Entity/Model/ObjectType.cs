using System;
using System.Collections.Generic;

namespace ZONE.Entity.Model;

public partial class ObjectType
{
    public string Object { get; set; } = null!;

    public string? Type { get; set; }

    public bool Active { get; set; }
}
