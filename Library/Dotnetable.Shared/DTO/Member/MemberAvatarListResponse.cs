using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Shared.DTO.Member;

public class MemberAvatarListResponse
{
    public int DatabaseRecords { get; set; }
    public List<AvatarDetail> Avatars { get; set; }
    public ErrorExceptionResponse ErrorException { get; set; }

    public class AvatarDetail
    {
        public Guid FileCode { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int FileID { get; set; }
        public byte FileCategoryID { get; set; }
    }
}
