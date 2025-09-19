using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZONE.EntityDto.Model
{
    public class CameraRoiDto
    {
        public int? Id { get; set; }

        public int? CameraId { get; set; }

        public string? Points { get; set; } = null!;
    }
}
