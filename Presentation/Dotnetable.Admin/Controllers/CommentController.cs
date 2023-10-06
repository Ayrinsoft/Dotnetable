using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Comment;
using Dotnetable.Shared.DTO.Post;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Admin.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("Api/[controller]")]
public class CommentController : ControllerBase
{

    #region CTOR
    private readonly CommentService _comm;
    private IConfiguration _config;
    public CommentController(CommentService comm, IConfiguration config)
    {
        _comm = comm;
        _config = config;
    }
    #endregion

    [AllowAnonymous]
    [HttpPost("Insert")]
    public async Task<ActionResult<PublicControllerResponse>> Insert(CommentInsertRequest requestModel)
    {
        requestModel.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);

        var responseData = await _comm.Insert(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [AllowAnonymous]
    [HttpPost("PostCommentList")]
    public async Task<ActionResult<PublicControllerResponse>> PostCommentList(PostCommentListRequest requestModel)
    {
        var responseData = await _comm.PostCommentList(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.CommentManager))]
    [HttpPost("PostCommentAdminList")]
    public async Task<ActionResult<PublicControllerResponse>> PostCommentAdminList(PostCommentListAdminRequest requestModel)
    {
        var responseData = await _comm.PostCommentAdminList(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.CommentManager))]
    [HttpPost("AdminApproveComment")]
    public async Task<ActionResult<PublicControllerResponse>> AdminApproveComment(AdminApproveCommentRequest requestModel)
    {
        var responseData = await _comm.AdminApproveComment(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

}
