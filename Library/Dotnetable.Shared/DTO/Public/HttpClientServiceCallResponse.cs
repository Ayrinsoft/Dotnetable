namespace Dotnetable.Shared.DTO.Public;

public class HttpClientServiceCallResponse
{
    public string ResponseBody { get; set; }
    public bool IsSuccess { get; set; }
    public ErrorExceptionResponse ErrorException { get; set; }
}
