using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Shared.DTO.Message;

public class EmailPanelListRequest : GridviewRequest
{
    public string EmailName { get; set; }
}
