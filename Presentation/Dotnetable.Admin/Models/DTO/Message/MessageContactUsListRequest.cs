using Dotnetable.SharedDTO.p.Public;

namespace Dotnetable.Admin.Models.Charts.DTO.Message;

public class MessageContactUsListRequest: GridviewRequest
{
    public bool? Archive { get; set; }
}
