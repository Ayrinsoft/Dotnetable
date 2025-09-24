namespace Dotnetable.SharedDTO.p.Public;

public class PublicActionResponse
{
    public string ObjectID { get; set; }
    public bool SuccessAction { get; set; } = false;
    public ErrorExceptionResponse ErrorException { get; set; }
}
