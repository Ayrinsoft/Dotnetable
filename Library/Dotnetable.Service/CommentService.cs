﻿using Dotnetable.Data.DataAccess;
using Dotnetable.Shared.DTO.Comment;
using Dotnetable.Shared.DTO.Post;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;

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
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(requestModel.CurrentMemberID, nameof(MemberRole.CommentManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        requestModel.OrderbyParams = requestModel.OrderbyParams.CheckForInjection(new() { "PostCommentID", "LogTime" });
        return await CommentDataAccess.PostCommentAdminList(requestModel);
    }

    public async Task<PublicActionResponse> AdminApproveComment(AdminApproveCommentRequest requestModel)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(requestModel.CurrentMemberID, nameof(MemberRole.CommentManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        return requestModel.CommentCategoryID switch
        {
            CommentCategory.Post => await CommentDataAccess.AdminApprovePostComment(requestModel),
            _ => await Task.FromResult(new PublicActionResponse() { ErrorException = new() { ErrorCode = "D0" } })
        };
    }

}