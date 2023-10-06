namespace Dotnetable.Shared.DTO.Post;

public class StaticPageDetailAboutUsResponse
{
    public string HTMLBody { get; set; }
    public Dictionary<string, string> RelatedCompanies { get; set; }
    public Dictionary<string, string> OtherHtmlPart { get; set; }
}
