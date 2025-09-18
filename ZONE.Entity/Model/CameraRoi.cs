using System;
using System.Collections.Generic;

namespace ZONE.Entity.Model;

public partial class CameraRoi
{
    public int Id { get; set; }

    public int CameraId { get; set; }

    public int RoiStartPercentageHeight { get; set; }

    public int RoiEndPercentageHeight { get; set; }

    public int RoiStartPercentageWidth { get; set; }

    public int RoiEndPercentageWidth { get; set; }
}
