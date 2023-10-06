using Dotnetable.Admin.Models.Nestable;
using Microsoft.AspNetCore.Components;

namespace Dotnetable.Admin.Sahred.Nestable
{
    public partial class NestableItem
    {
        [Parameter] public List<NestableStandardRequest> Model { get; set; }
        [Parameter] public string ParentID { get; set; }
        [Parameter] public string MenuID { get; set; }

    }
}
