namespace Dotnetable.Admin.Models.Nestable;

public class NestableResponse
{
    public int id { get; set; }
    public List<NestableResponse> children { get; set; }
}
