using Dotnetable.Admin.Models;
using Dotnetable.Admin.SharedServices.Data;
using Dotnetable.Shared.DTO.Comment;
using Dotnetable.Shared.DTO.Post;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Dotnetable.Admin.Components.Pages.Posts.Comment;

public partial class PostCommentManage
{
    [Inject] private ISnackbar _snackbar { get; set; }
    [Inject] private IStringLocalizer<Dotnetable.Shared.Resources.Resource> _loc { get; set; }
    [Inject] private IHttpServices _httpService { get; set; }
    [CascadingParameter] protected ThemeManagerModel themeManager { get; set; }

    private PostCommentListAdminRequest _listRequest { get; set; }
    private PostCommentListAdminResponse _listResponse { get; set; }
    private GridViewHeaderParameters _gridHeaderParams { get; set; }

    protected async override Task OnInitializedAsync()
    {
        _gridHeaderParams = new()
        {
            HeaderItems = new()
                {
                    new() { ColumnLocalizeCode = "_PostCommentID", ColumnName = nameof(PostCommentListAdminResponse.CommentDetail.PostCommentID), HasSort = true },
                    new() { ColumnLocalizeCode = "_ReplyPostCommentID", ColumnName = nameof(PostCommentListAdminResponse.CommentDetail.ReplyPostCommentID), HasSearch = true, SearchType = SearchColumnType.Text, HasSort = true },
                    new() { ColumnLocalizeCode = "_PostID", ColumnName = nameof(PostCommentListAdminResponse.CommentDetail.PostID), HasSearch = true, SearchType = SearchColumnType.Text, HasSort = true },
                    new() { ColumnLocalizeCode = "_LogTime" },
                    new() { ColumnLocalizeCode = "_CommentBody" },
                    new() { ColumnLocalizeCode = "_LanguageCode" },
                    new() { ColumnLocalizeCode = "_CommentTypeID", ColumnName = nameof(PostCommentListAdminResponse.CommentDetail.CommentTypeID), HasSearch = true, SearchType = SearchColumnType.DropDown, OtherDropDownValues = new() { { "Normal", "1" }, { "Edit", "2" }, { "Wrong", "3" }, { "BadWords", "4" } }, HasSort = true },
                    new() { ColumnLocalizeCode = "_IPAddress" },
                    new() { ColumnLocalizeCode = "_Gender" },
                    new() { ColumnLocalizeCode = "_Surname" },
                    new() { ColumnLocalizeCode = "_Email" },
                    new() { ColumnLocalizeCode = "_CellphoneNumber" },
                    new() { ColumnLocalizeCode = "_MemberID" },
                    new() { ColumnLocalizeCode = "_Management" }
                },
            Pagination = new() { MaxLength = _listResponse?.DatabaseRecords ?? 1, ShowFirstLast = true }
        };

        RefreshRequestInput();
        await FetchGrid();
    }

    #region GRID

    private async Task OnSearchChanged(GridViewHeaderParameters changedColumns)
    {
        _gridHeaderParams = changedColumns;
        RefreshRequestInput();
        await FetchGrid();
    }

    private void RefreshRequestInput()
    {
        string commentTypeString = _gridHeaderParams.HeaderItems.FirstOrDefault(i => i.ColumnName == nameof(PostCommentListAdminResponse.CommentDetail.CommentTypeID))?.SearchText ?? "0";
        if (string.IsNullOrEmpty(commentTypeString) || commentTypeString == "") commentTypeString = "0";

        string postID = _gridHeaderParams.HeaderItems.FirstOrDefault(i => i.ColumnName == nameof(PostCommentListAdminResponse.CommentDetail.PostID))?.SearchText ?? "0";
        if (string.IsNullOrEmpty(postID) || postID == "") postID = "0";

        string replyID = _gridHeaderParams.HeaderItems.FirstOrDefault(i => i.ColumnName == nameof(PostCommentListAdminResponse.CommentDetail.ReplyPostCommentID))?.SearchText ?? "0";
        if (string.IsNullOrEmpty(replyID) || replyID == "") replyID = "0";

        _listRequest = new()
        {
            SkipCount = ((_gridHeaderParams.Pagination.PageIndex - 1) * _gridHeaderParams.Pagination.PageSize),
            TakeCount = _gridHeaderParams.Pagination.PageSize,
            OrderbyParams = _gridHeaderParams.OrderbyParams,
            PostID = Convert.ToInt32(postID),
            ReplyID = Convert.ToInt32(replyID),
            CommentTypeID = Convert.ToByte(commentTypeString)
        };
    }

    private async Task FetchGrid()
    {
        var fetchData = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Comment/PostCommentAdminList", _listRequest.ToJsonString());
        if (fetchData.Success)
        {
            _listResponse = fetchData.ResponseData.CastModel<PostCommentListAdminResponse>();
        }
        _gridHeaderParams.Pagination.MaxLength = _listResponse?.DatabaseRecords ?? 1;
        StateHasChanged();
    }
    #endregion

    private async Task ConfirmComment(int itemCommentID, bool confirmItem)
    {
        if (itemCommentID < 0) return;

        var fetchData = await _httpService.CallServiceObjAsync(HttpMethod.Post, true, "Comment/AdminApproveComment", new AdminApproveCommentRequest() { Approve = confirmItem, CommentCategoryID = CommentCategory.Post, ItemID = itemCommentID }.ToJsonString());
        if (fetchData.Success)
        {
            var parseData = fetchData.ResponseData.CastModel<PublicActionResponse>();
            if (parseData is not null && parseData.SuccessAction)
            {
                _snackbar.Add($"{_loc["_SuccessAction"]} {_loc[(confirmItem ? "_Confirm" : "_UnConfirm")]}", Severity.Success);
                return;
            }
            else
            {
                _snackbar.Add($"{_loc[$"_ERROR_{parseData?.ErrorException.ErrorCode}"]} {_loc[(confirmItem ? "_Confirm" : "_UnConfirm")]}", Severity.Error);
                return;
            }
        }
        _snackbar.Add($"{_loc["_FailedAction"]} {_loc[(confirmItem ? "_Confirm" : "_UnConfirm")]}", Severity.Error);
    }


}
