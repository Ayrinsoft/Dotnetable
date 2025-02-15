namespace Dotnetable.Shared.DTO.Post;

public class QRCodeUpdateRequest
{
    public PostPublicPageDetailUpdateRequest PublicPostDetail { get; set; }
    public StaticPageDetailQRCodeResponse QRCodeDetail { get; set; }
}
