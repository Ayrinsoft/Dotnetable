namespace Dotnetable.Admin.Models;

public class UserModel
{
    public string Avatar { get; set; } = "/images/avatar-m.jpg";
    public string DisplayName { get; set; } = "AyrinSoft";
    public string Email { get; set; } = "members@ayrinsoft.com";
    public string Role { get; set; } = "Admin";
    public string LanguageCode { get; set; } = "EN";
    public bool Gender { get; set; } = true;
}