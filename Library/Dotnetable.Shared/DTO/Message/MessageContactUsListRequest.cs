using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Shared.DTO.Message;

public class MessageContactUsListRequest: GridviewRequest
{
    public bool? Archive { get; set; }
}
