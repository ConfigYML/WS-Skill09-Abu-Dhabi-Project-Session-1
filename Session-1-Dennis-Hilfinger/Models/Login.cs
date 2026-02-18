using System;
using System.Collections.Generic;

namespace Session_1_Dennis_Hilfinger;

public partial class Login
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime LoginTime { get; set; }

    public DateTime? LogoutTime { get; set; }

    public string? ErrorMessage { get; set; }

    public virtual User User { get; set; } = null!;
}
