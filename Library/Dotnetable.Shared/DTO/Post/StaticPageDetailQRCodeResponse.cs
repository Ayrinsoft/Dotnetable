namespace Dotnetable.Shared.DTO.Post;

public class StaticPageDetailQRCodeResponse
{
    public string HTMLBody { get; set; }
    public Dictionary<string, string> OtherHtmlPart { get; set; }
    public string RedirectURL { get; set; }
}
