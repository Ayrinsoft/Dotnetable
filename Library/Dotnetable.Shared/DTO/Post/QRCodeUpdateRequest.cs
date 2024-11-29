namespace Dotnetable.Shared.DTO.Post;

public class QRCodeUpdateRequest
{
    public int CurrentMemberID { get; set; }
    public PostPublicPageDetailUpdateRequest PublicPostDetail { get; set; }
    public StaticPageDetailQRCodeResponse QRCodeDetail { get; set; }
}
