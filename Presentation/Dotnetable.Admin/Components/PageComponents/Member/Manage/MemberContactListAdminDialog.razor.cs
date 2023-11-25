using Dotnetable.Shared.DTO.Member;
using Microsoft.AspNetCore.Components;

namespace Dotnetable.Admin.Components.PageComponents.Member.Manage;

public partial class MemberContactListAdminDialog
{
    [Parameter] public List<MemberContactRequest> Addresses { get; set; }
}
