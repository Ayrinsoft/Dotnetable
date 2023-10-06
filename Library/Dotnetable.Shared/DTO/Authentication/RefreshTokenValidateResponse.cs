namespace Dotnetable.Shared.DTO.Authentication;

public class RefreshTokenValidateResponse
{
    public int MemberID { get; set; }
    public string Givenname { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string CellphoneNumber { get; set; }
    public string HashKey { get; set; }
    public string LocalizedLanguageCode { get; set; }
    public bool? Gender { get; set; }
    public string AvatarID { get; set; }
}
