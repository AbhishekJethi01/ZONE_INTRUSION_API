using System;
using System.Collections.Generic;

namespace ZONE.Entity.Model;

public partial class UserDetail
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string? EmailId { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string Password { get; set; } = null!;
}
