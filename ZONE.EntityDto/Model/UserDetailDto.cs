using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZONE.EntityDto.Model
{
    public class UserDetailDto
    {
        public int? UserId { get; set; }

        public string? UserName { get; set; } = null!;

        public string? EmailId { get; set; }

        public string? PhoneNumber { get; set; } = null!;

        public string? Password { get; set; } = null!;
    }

    public class UserDetailView
    {
        public int? UserId { get; set; }

        public string? UserName { get; set; } = null!;

        public string? EmailId { get; set; } = null!;

        public string? PhoneNumber { get; set; } = null!;
    }
}
