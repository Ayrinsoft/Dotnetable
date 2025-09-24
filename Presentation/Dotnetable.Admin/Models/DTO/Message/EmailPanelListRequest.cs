using Dotnetable.SharedDTO.p.Public;

namespace Dotnetable.Admin.Models.Charts.DTO.Message;

public class EmailPanelListRequest : GridviewRequest
{
    public string EmailName { get; set; }
}
