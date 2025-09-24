using Dotnetable.Admin.Models.Charts.DTO.Comment;
using Dotnetable.Admin.Models.Charts.DTO.Post;
using Dotnetable.Data.DataAccess;
using Dotnetable.Shared.Tools;
using Dotnetable.SharedDTO.p.Public;

namespace Dotnetable.Service;
public class CommentService
{

    public async Task<PublicActionResponse> Insert(CommentInsertRequest requestModel)
    {
        return await CommentDataAccess.Insert(requestModel);
    }

    public async Task<CommentListResponse> PostCommentList(PostCommentListRequest requestModel)
    {
        requestModel.OrderbyParams = requestModel.OrderbyParams.CheckForInjection(new() { "PostCommentID", "LogTime" });
        return await CommentDataAccess.PostCommentList(requestModel);
    }

    public async Task<PostCommentListAdminResponse> PostCommentAdminList(PostCommentListAdminRequest requestModel)
    {
        requestModel.OrderbyParams = requestModel.OrderbyParams.CheckForInjection(new() { "PostCommentID", "LogTime" });
        return await CommentDataAccess.PostCommentAdminList(requestModel);
    }

    public async Task<PublicActionResponse> AdminApproveComment(AdminApproveCommentRequest requestModel)
    {
        return requestModel.CommentCategoryID switch
        {
            CommentCategory.Post => await CommentDataAccess.AdminApprovePostComment(requestModel),
            _ => await Task.FromResult(new PublicActionResponse() { ErrorException = new() { ErrorCode = "D0" } })
        };
    }

}