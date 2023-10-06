namespace Dotnetable.Shared.DTO.Authentication;

public class UserLoginResponse 
{
    public int MemberID { get; set; }
    public string Givenname { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string CellphoneNumber { get; set; }
    public Guid HashKey { get; set; }
    public DateTime RegisterDate { get; set; }
    public bool? Gender { get; set; }
    public Guid? AvatarID { get; set; }
    public string PolicyName { get; set; }
    public string LanguageCode { get; set; }

    public TokenItems TokenDetail { get; set; }
    public List<string> Roles { get; set; }

    public class TokenItems
    {
        public string Token { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public string ExpireTime { get; set; }
    }

    public Public.ErrorExceptionResponse ErrorException { get; set; }
}
