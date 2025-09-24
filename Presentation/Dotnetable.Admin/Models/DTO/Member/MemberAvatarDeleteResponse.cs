using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Admin.Models.DTO.Member;

public class MemberAvatarDeleteResponse
{
    public bool SuccessAction { get; set; } = false;
    public byte FileCategoryID { get; set; }
    public string FilePath { get; set; }
    public ErrorExceptionResponse ErrorException { get; set; }
}
