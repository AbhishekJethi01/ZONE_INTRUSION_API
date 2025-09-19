using System;
using System.Collections.Generic;

namespace ZONE.Entity.Model;

public partial class CameraRoi
{
    public int Id { get; set; }

    public int CameraId { get; set; }

    public string Points { get; set; } = null!;
}
