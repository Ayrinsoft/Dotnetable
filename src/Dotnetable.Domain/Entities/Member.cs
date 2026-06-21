using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Member
{
    public int MemberID { get; set; }

    public bool Active { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string CellphoneNumber { get; set; } = null!;

    public string CountryCode { get; set; } = null!;

    public DateOnly RegisterDate { get; set; }

    public string Givenname { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public Guid? AvatarID { get; set; }

    public Guid HashKey { get; set; }

    public int PolicyID { get; set; }

    public bool? Gender { get; set; }

    public int WebsiteID { get; set; }

    public virtual ICollection<EmailSubscribe> EmailSubscribes { get; set; } = new List<EmailSubscribe>();

    public virtual ICollection<MemberForgetPassword> MemberForgetPasswords { get; set; } = new List<MemberForgetPassword>();

    public virtual Policy Policy { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;
}
