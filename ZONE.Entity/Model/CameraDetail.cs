using System;
using System.Collections.Generic;

namespace ZONE.Entity.Model;

public partial class CameraDetail
{
    public int CameraId { get; set; }

    public string CameraName { get; set; } = null!;

    public string Url { get; set; } = null!;

    public int RoistartPercentageHeight { get; set; }

    public int RoiendPercentageHeight { get; set; }

    public string? Roicoordinates { get; set; }

    public int RoistartPercentageWidth { get; set; }

    public int RoiendPercentageWidth { get; set; }

    public string? CameraUserName { get; set; }

    public string? CameraIpAddress { get; set; }

    public string? CameraPassword { get; set; }

    public string? CameraPort { get; set; }

    public string? CameraSubstream { get; set; }
}
