using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZONE.EntityDto.Model;

namespace ZONE.DOMAIN.Model
{
    public class CameraLatestEventsView
    {
        public string CameraId { get; set; } = default!;
        public string Object { get; set; } = default!;
        public string CameraName { get; set; } = default!;
        public List<EventDetailDto> Events { get; set; } = new();
    }
}
