namespace Dotnetable.Shared.DTO.Member;

public class MemberAvatarDeleteResponse
{
    public bool SuccessAction { get; set; } = false;
    public byte FileCategoryID { get; set; }
    public string FilePath { get; set; }
    public Public.ErrorExceptionResponse ErrorException { get; set; }
}
