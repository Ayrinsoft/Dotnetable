using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Admin.Models.DTO.Message;

public class MessageContactUsListRequest: GridviewRequest
{
    public bool? Archive { get; set; }
}
