using System;
using System.Collections.Generic;

namespace Dotnetable.Data.DBContext;

public partial class TB_Member
{
    public int MemberID { get; set; }

    public bool Active { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public string Email { get; set; }

    public string CellphoneNumber { get; set; }

    public string CountryCode { get; set; }

    public bool Activate { get; set; }

    public DateTime RegisterDate { get; set; }

    public string Givenname { get; set; }

    public string Surname { get; set; }

    public Guid? AvatarID { get; set; }

    public Guid HashKey { get; set; }

    public int PolicyID { get; set; }

    public bool? Gender { get; set; }

    public string PostalCode { get; set; }

    public int CityID { get; set; }

    public virtual TB_City City { get; set; }

    public virtual TB_Policy Policy { get; set; }

    public virtual ICollection<TBM_Member_File> TBM_Member_Files { get; set; } = new List<TBM_Member_File>();

    public virtual ICollection<TB_Email_Subscribe> TB_Email_Subscribes { get; set; } = new List<TB_Email_Subscribe>();

    public virtual ICollection<TB_File> TB_Files { get; set; } = new List<TB_File>();

    public virtual ICollection<TB_Login_Token> TB_Login_Tokens { get; set; } = new List<TB_Login_Token>();

    public virtual ICollection<TB_Member_Activate_Log> TB_Member_Activate_Logs { get; set; } = new List<TB_Member_Activate_Log>();

    public virtual ICollection<TB_Member_Contact> TB_Member_Contacts { get; set; } = new List<TB_Member_Contact>();

    public virtual ICollection<TB_Member_Forget_Password> TB_Member_Forget_Passwords { get; set; } = new List<TB_Member_Forget_Password>();

    public virtual ICollection<TB_Post_Comment> TB_Post_Comments { get; set; } = new List<TB_Post_Comment>();

    public virtual ICollection<TB_Post> TB_Posts { get; set; } = new List<TB_Post>();
}
