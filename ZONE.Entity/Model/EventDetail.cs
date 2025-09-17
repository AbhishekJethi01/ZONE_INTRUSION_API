using System;
using System.Collections.Generic;

namespace ZONE.Entity.Model;

public partial class EventDetail
{
    public int EventId { get; set; }

    public string? Img { get; set; }

    public DateTime EventTime { get; set; }

    public string? ObjectType { get; set; }

    public int? CameraId { get; set; }

    public string CameraName { get; set; } = null!;

    public string? Link { get; set; }
}
