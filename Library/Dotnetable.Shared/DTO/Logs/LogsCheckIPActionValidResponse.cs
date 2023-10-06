namespace Dotnetable.Shared.DTO.Logs;

public class LogsCheckIPActionValidResponse
{
    public int TryCount { get; set; }
    public bool ValidForNewRequest { get; set; }

    public Public.ErrorExceptionResponse ErrorException { get; set; }
}
