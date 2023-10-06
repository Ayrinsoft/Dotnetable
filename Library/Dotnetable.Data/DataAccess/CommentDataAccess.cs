using Dotnetable.Data.DBContext;
using Dotnetable.Shared.DTO.Comment;
using Dotnetable.Shared.DTO.Post;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Data.DataAccess;
public class CommentDataAccess
{
    public static async Task<PublicActionResponse> Insert(CommentInsertRequest requestModel)
    {
        using DotnetableEntity db = new();
        DateTime lastChanceInsert = DateTime.Now.AddMinutes(-1);
        bool insertedBefore = requestModel.CommentCategory switch
        {
            CommentCategory.Post => await (from i in db.TB_Post_Comments where i.IPAddress == requestModel.IPAddress && i.LogTime > lastChanceInsert select i.PostCommentID).AnyAsync(),
            _ => false
        };
        if (insertedBefore) return new() { ErrorException = new() { ErrorCode = "D2" } };

        TB_Post_Comment postCommentObj = new()
        {
            Active = null,
            MemberID = requestModel.CurrentMemberID.Value,
            LogTime = DateTime.Now,
            PostID = requestModel.ObjectID,
            IPAddress = requestModel.IPAddress,
            ReplyPostCommentID = requestModel.ReplyID,
            CommentTypeID = (byte)requestModel.CommentType,
            LanguageCode = requestModel.LanguageCode,
            CommentBody = requestModel.CommentBody
        };
        db.TB_Post_Comments.Add(postCommentObj);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true, ObjectID = postCommentObj.PostCommentID.ToString() };
    }

    public static async Task<CommentListResponse> PostCommentList(PostCommentListRequest requestModel)
    {
        using DotnetableEntity db = new();

        var reportQuery = db.TB_Post_Comments.Join(db.TB_Members, c => c.MemberID, m => m.MemberID, (c, m) => new { c.PostCommentID, c.ReplyPostCommentID, c.LogTime, c.CommentBody, c.LanguageCode, m.Gender, m.Surname, m.Active, CommentActive = c.Active, c.CommentTypeID, c.PostID }).Where(i => i.Active && i.CommentActive.HasValue && i.CommentActive == true && i.CommentTypeID == 1 && i.PostID == requestModel.PostID).AsQueryable();

        if (requestModel.ReplyID.HasValue && requestModel.ReplyID.Value > 0)
            reportQuery = reportQuery.Where(i => i.ReplyPostCommentID == requestModel.ReplyID);


        int dbCount = await reportQuery.CountAsync();


        if (string.IsNullOrEmpty(requestModel.OrderbyParams) || requestModel.OrderbyParams == "")
            reportQuery = reportQuery.OrderBy(i => i.PostCommentID);
        else
            reportQuery = reportQuery.OrderUsingSortExpression(requestModel.OrderbyParams);


        var fetchList = await reportQuery.Skip(requestModel.SkipCount).Take(requestModel.TakeCount).Select(i => new CommentListResponse.CommentDetail { Surname = i.Surname, CommentBody = i.CommentBody, Gender = i.Gender ?? true, LanguageCode = i.LanguageCode, LogTime = i.LogTime, ItemID = i.PostCommentID, ReplyID = i.ReplyPostCommentID }).ToListAsync();

        return new() { Comments = fetchList, DatabaseRecords = dbCount };
    }

    public static async Task<PostCommentListAdminResponse> PostCommentAdminList(PostCommentListAdminRequest requestModel)
    {
        using DotnetableEntity db = new();

        var reportQuery = db.TB_Post_Comments.Join(db.TB_Members, c => c.MemberID, m => m.MemberID, (c, m) => new { c.PostCommentID, c.ReplyPostCommentID, c.PostID, c.LogTime, c.CommentBody, c.LanguageCode, c.CommentTypeID, c.IPAddress, m.Gender, m.Surname, m.Email, m.CellphoneNumber, m.MemberID, m.Active, CommentActive = c.Active }).Where(i => i.Active && i.CommentActive == null).AsQueryable();

        if (requestModel.ReplyID.HasValue && requestModel.ReplyID.Value > 0)
            reportQuery = reportQuery.Where(i => i.ReplyPostCommentID == requestModel.ReplyID);

        if (requestModel.PostID.HasValue && requestModel.PostID.Value > 0)
            reportQuery = reportQuery.Where(i => i.PostID == requestModel.PostID);

        if (requestModel.CommentTypeID.HasValue && requestModel.CommentTypeID.Value > 0)
            reportQuery = reportQuery.Where(i => i.CommentTypeID == requestModel.CommentTypeID);


        int dbCount = await reportQuery.CountAsync();


        if (string.IsNullOrEmpty(requestModel.OrderbyParams) || requestModel.OrderbyParams == "")
            reportQuery = reportQuery.OrderBy(i => i.PostCommentID);
        else
            reportQuery = reportQuery.OrderUsingSortExpression(requestModel.OrderbyParams);


        var fetchList = await reportQuery.Skip(requestModel.SkipCount).Take(requestModel.TakeCount).Select(i => new PostCommentListAdminResponse.CommentDetail { MemberID = i.MemberID, Surname = i.Surname, CellphoneNumber = i.CellphoneNumber, PostCommentID = i.PostCommentID, CommentBody = i.CommentBody, CommentTypeID = i.CommentTypeID, Email = i.Email, Gender = i.Gender ?? true, IPAddress = i.IPAddress, LanguageCode = i.LanguageCode, LogTime = i.LogTime, PostID = i.PostID, ReplyPostCommentID = i.ReplyPostCommentID }).ToListAsync();

        return new() { Comments = fetchList, DatabaseRecords = dbCount };
    }

    public static async Task<PublicActionResponse> AdminApprovePostComment(AdminApproveCommentRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchCommentItem = await (from i in db.TB_Post_Comments where i.PostCommentID == requestModel.ItemID && i.Active == null select i).FirstOrDefaultAsync();
        if (fetchCommentItem is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        fetchCommentItem.Active = requestModel.Approve;

        try
        {
            db.Entry(fetchCommentItem).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true };
    }





}