using System;
using System.Collections.Generic;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Domain.Entities;

public partial class WebsiteClient
{
    public int WebsiteClientID { get; set; }

    public int WebsiteID { get; set; }

    public int? AvatarID { get; set; }

    public string? Email { get; set; }

    public string? Cellphone { get; set; }

    public string? CountryCode { get; set; }

    public string? Password { get; set; }

    public bool Active { get; set; }

    public DateOnly RegisterDate { get; set; }

    public bool? Gender { get; set; }

    public string? Givenname { get; set; }

    public string? Surname { get; set; }

    public Guid HashKey { get; set; }

    public ClientLevel ClientLevel { get; set; }

    public virtual FileRecord? Avatar { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual ICollection<WebsiteClientForgetPassword> WebsiteClientForgetPasswords { get; set; } = new List<WebsiteClientForgetPassword>();
}
