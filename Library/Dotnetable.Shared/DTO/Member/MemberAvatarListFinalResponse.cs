using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Shared.DTO.Member;

public class MemberAvatarListFinalResponse
{
    public int DatabaseRecords { get; set; }
    public List<AvatarDetail> Avatars { get; set; }
    public ErrorExceptionResponse ErrorException { get; set; }

    public class AvatarDetail
    {
        public Guid FileCode { get; set; }
        public string FileName { get; set; }
        public string FileURL { get; set; }
        public string Thumbnail { get; set; }
    }

}
