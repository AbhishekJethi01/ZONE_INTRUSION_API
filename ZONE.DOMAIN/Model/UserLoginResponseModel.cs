using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZONE.DOMAIN.Model
{
    public class UserLoginResponseModel
    {
        //public int UserId { get; set; }

        public string UserName { get; set; } = null!;

        public string EmailId { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;
        public string? Token { get; set; }
    }
}
