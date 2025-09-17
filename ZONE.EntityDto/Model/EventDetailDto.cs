using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZONE.EntityDto.Model
{
    public class EventDetailDto
    {
        public int EventId { get; set; }

        public string? Img { get; set; }

        public DateTime EventTime { get; set; }

        public string? ObjectType { get; set; }

        public int? CameraId { get; set; }

        public string CameraName { get; set; } = null!;

        public string? Link { get; set; }
    }

    public class EventDetailView
    {
        public int EventId { get; set; }

        public string? Img { get; set; }

        public DateTime EventTime { get; set; }

        public string? ObjectType { get; set; }

        public int? CameraId { get; set; }

        public string CameraName { get; set; } = null!;

        public string? Link { get; set; }
    }
}
