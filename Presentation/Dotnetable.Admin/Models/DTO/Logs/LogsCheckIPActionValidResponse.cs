using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Admin.Models.DTO.Logs;

public class LogsCheckIPActionValidResponse
{
    public int TryCount { get; set; }
    public bool ValidForNewRequest { get; set; }

    public ErrorExceptionResponse ErrorException { get; set; }
}
