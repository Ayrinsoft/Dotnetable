using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Dotnetable.Admin.Components.Shared;

public partial class CommandPalette
{
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private NavigationManager _navigation { get; set; }

    private static Dictionary<string, string> _pages = new();
    private Dictionary<string, string> _pagesFiltered = new();
    private string _search;

    protected override void OnInitialized()
    {
        _pages = new()
        {
            { _loc["_Home"],"/"  },
            { _loc["_Member_Profile"],"/Member/Profile"  },
            { _loc["_Member_Manage"],"/Member/Manage"  },
            { _loc["_ChangePassword"],"/Member/ChangePassword"  },
            { _loc["_Subscribe_Management"],"/Member/SubscribeManage"  },
            { _loc["_View_Roles"],"/Member/RoleList"  },
            { _loc["_Policy_Manage"],"/Member/PolicyManage"  },
            { _loc["_ContactUs_Message_Manage"],"/Message/ContactUsMessages"  },
            { _loc["_Email_Setting_Manage"],"/Message/EmailSettingManage"  },
            { _loc["_Post_Manage"],"/Post/Manage"  },
            { _loc["_AboutUsManage"],"/Post/AboutUsManage"  },
            { _loc["_ContactUsManage"],"/Post/ContactUsManage"  },
            { _loc["_QRCodeManage"],"/Post/QRCodeManage"  },
            { _loc["_PostCategory_Manage"],"/PostCategory/Manage"  },
            { _loc["_Post_Comment_Manage"],"/Post/Comment/Manage"  },
            { _loc["_SlideShow_Manage"],"/SlideShow/Manage"  }
        };

        _pagesFiltered = _pages;
    }

    private void SearchPages(string value)
    {
        _pagesFiltered = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(value))
            _pagesFiltered = _pages
                .Where(x => x.Key
                    .Contains(value, StringComparison.InvariantCultureIgnoreCase))
                .ToDictionary(x => x.Key, x => x.Value);
        else
            _pagesFiltered = _pages;
    }
}