using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZONE.EntityDto.Model
{
    public class CameraRoiDto
    {
        public int Id { get; set; }

        public int CameraId { get; set; }

        public int RoiStartPercentageHeight { get; set; }

        public int RoiEndPercentageHeight { get; set; }

        public int RoiStartPercentageWidth { get; set; }

        public int RoiEndPercentageWidth { get; set; }
    }
}
