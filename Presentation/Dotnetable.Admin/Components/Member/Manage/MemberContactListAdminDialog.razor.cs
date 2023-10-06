using Dotnetable.Shared.DTO.Member;
using Microsoft.AspNetCore.Components;

namespace Dotnetable.Admin.Components.Member.Manage;

public partial class MemberContactListAdminDialog
{
    [Parameter] public List<MemberContactRequest> Addresses { get; set; }
}
