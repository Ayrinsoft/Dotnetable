using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Admin.Models.DTO.Message;

public class EmailPanelListRequest : GridviewRequest
{
    public string EmailName { get; set; }
}
