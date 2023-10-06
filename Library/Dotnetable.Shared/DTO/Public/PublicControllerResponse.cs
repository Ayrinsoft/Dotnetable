namespace Dotnetable.Shared.DTO.Public;

public class PublicControllerResponse
{
    public string LogID { get; set; } = Guid.NewGuid().ToString().ToUpper();
    public bool Success { get; set; } = false;
    public object ResponseData { get; set; }
    public string ResponseTime { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff");
    public ErrorExceptionResponse ErrorException { get; set; } = null;
}
