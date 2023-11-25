using Asp.Versioning;
using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
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

public class PostController : ControllerBase
{
    #region CTOR
    private readonly PostService _post;
    private IConfiguration _config;
    public PostController(PostService post, IConfiguration config)
    {
        _post = post;
        _config = config;
    }
    #endregion


    #region Admin
    #region PostCategory

    [Authorize(nameof(MemberRole.PostCategoryManager))]
    [HttpGet("PostCategoryList")]
    public async Task<ActionResult<PublicControllerResponse>> PostCategoryList()
    {
        var responseData = await _post.PostCategoryList();
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.PostCategoryManager))]
    [HttpPost("PostCategoryInsert")]
    public async Task<ActionResult<PublicControllerResponse>> PostCategoryInsert(PostCategoryInsertRequest requestModel)
    {
        var responseData = await _post.PostCategoryInsert(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.PostCategoryManager))]
    [HttpPost("PostCategoryDetail")]
    public async Task<ActionResult<PublicControllerResponse>> PostCategoryDetail(PostCategoryDetailRequest requestModel)
    {
        var responseData = await _post.PostCategoryDetail(requestModel);
        string responseExeption = responseData?.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.PostCategoryManager))]
    [HttpPost("PostCategoryUpdate")]
    public async Task<ActionResult<PublicControllerResponse>> PostCategoryUpdate(PostCategoryUpdateRequest requestModel)
    {
        var responseData = await _post.PostCategoryUpdate(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.PostCategoryManager))]
    [HttpPost("PostCategoryChangeStatus")]
    public async Task<ActionResult<PublicControllerResponse>> PostCategoryChangeStatus(PostCategoryChangeStatusRequest requestModel)
    {
        var responseData = await _post.PostCategoryChangeStatus(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.PostCategoryManager))]
    [HttpPost("PostCategoryUpdatePriorityAndParent")]
    public async Task<ActionResult<PublicControllerResponse>> PostCategoryUpdatePriorityAndParent(List<PostCategoryUpdatePriorityAndParentRequest> requestModel)
    {
        var responseData = await _post.PostCategoryUpdatePriorityAndParent(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }


    [Authorize(nameof(MemberRole.PostCategoryManager))]
    [HttpPost("PostCategoryUpdateOtherLanguage")]
    public async Task<ActionResult<PublicControllerResponse>> PostCategoryUpdateOtherLanguage(PostCategoryUpdateOtherLanguageRequest requestModel)
    {
        var responseData = await _post.PostCategoryUpdateOtherLanguage(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }


    #endregion

    #region Post
    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("AdminPostList")]
    public async Task<ActionResult<PublicControllerResponse>> AdminPostList(PostListFetchRequest ListRequest)
    {
        var responseData = await _post.AdminPostList(ListRequest);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("Insert")]
    public async Task<ActionResult<PublicControllerResponse>> Insert(PostInsertRequest requestModel)
    {
        requestModel.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);
        var responseData = await _post.Insert(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("AdminDetail")]
    public async Task<ActionResult<PublicControllerResponse>> AdminDetail(PostDetailRequest requestModel)
    {
        var responseData = await _post.AdminDetail(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("Update")]
    public async Task<ActionResult<PublicControllerResponse>> Update(PostUpdateRequest requestModel)
    {
        requestModel.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);
        var responseData = await _post.Update(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }


    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("RemovePostFile")]
    public async Task<ActionResult<PublicControllerResponse>> RemovePostFile(PostFileRemoveRequest requestModel)
    {
        var responseData = await _post.RemovePostFile(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("ChangeStatus")]
    public async Task<ActionResult<PublicControllerResponse>> ChangeStatus(PostChangeStatusRequest requestModel)
    {
        var responseData = await _post.ChangeStatus(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }


    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("PostAddLanguage")]
    public async Task<ActionResult<PublicControllerResponse>> PostAddLanguage(PostUpdateRequest requestModel)
    {
        requestModel.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);
        var responseData = await _post.PostAddLanguage(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("PostLanguageDetail")]
    public async Task<ActionResult<PublicControllerResponse>> PostLanguageDetail(PostLanguageDetailRequest requestModel)
    {
        var responseData = await _post.PostLanguageDetail(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("PostDeleteLangauge")]
    public async Task<ActionResult<PublicControllerResponse>> PostDeleteLangauge(PostLanguageDeleteRequest requestModel)
    {
        var responseData = await _post.PostDeleteLangauge(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }


    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("ContactusUpdate")]
    public async Task<ActionResult<PublicControllerResponse>> ContactusUpdate(ContactUsUpdateRequest requestModel)
    {
        var responseData = await _post.ContactusUpdate(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }


    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("AboutusUpdate")]
    public async Task<ActionResult<PublicControllerResponse>> AboutusUpdate(AboutUsUpdateRequest requestModel)
    {
        var responseData = await _post.AboutusUpdate(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }


    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("QRCodeUpdate")]
    public async Task<ActionResult<PublicControllerResponse>> QRCodeUpdate(QRCodeUpdateRequest requestModel)
    {
        var responseData = await _post.QRCodeUpdate(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }





    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("AdminCommentList")]
    public ActionResult<PublicControllerResponse> AdminCommentList(PostListFetchRequest requestModel)
    {
        ///TODO: Complete this section
        return null;
    }





    #endregion


    #endregion

    #region Website

    [AllowAnonymous]
    [HttpGet("PublicPostCategoryList")]
    public async Task<ActionResult<PublicControllerResponse>> PublicPostCategoryList()
    {
        var responseData = await _post.PublicPostCategoryList();
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
    [HttpPost("PublicPostCategoryDetail")]
    public async Task<ActionResult<PublicControllerResponse>> PublicPostCategoryDetail(PostCategoryPublicDetailRequest requestModel)
    {
        var responseData = await _post.PublicPostCategoryDetail(requestModel);
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
    [HttpPost("PublicPostList")]
    public async Task<ActionResult<PublicControllerResponse>> PublicPostList(PostListPublicRequest requestModel)
    {
        var responseData = await _post.PublicPostList(requestModel);
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
    [HttpPost("PublicPostDetail")]
    public async Task<ActionResult<PublicControllerResponse>> PublicPostDetail(PostDetailPublicRequest requestModel)
    {
        var responseData = await _post.PublicPostDetail(requestModel);
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
    [HttpPost("PublicPostAddVisitCount")]
    public async Task<ActionResult<PublicControllerResponse>> PublicPostAddVisitCount(PostDetailPublicRequest requestModel)
    {
        var responseData = await _post.PublicPostAddVisitCount(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    #endregion

    #region SlideShow

    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("SlideShowInsert")]
    public async Task<ActionResult<PublicControllerResponse>> SlideShowInsert(SlideShowInsertRequest requestModel)
    {
        var responseData = await _post.SlideShowInsert(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("SlideShowInsertLanguage")]
    public async Task<ActionResult<PublicControllerResponse>> SlideShowInsertLanguage(SlideShowInsertLanguageRequest requestModel)
    {
        var responseData = await _post.SlideShowInsertLanguage(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("SlideShowRemoveLanguage")]
    public async Task<ActionResult<PublicControllerResponse>> SlideShowRemoveLanguage(SlideShowRemoveLanguageRequest requestModel)
    {
        var responseData = await _post.SlideShowRemoveLanguage(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("SlideShowLanguageDetail")]
    public async Task<ActionResult<PublicControllerResponse>> SlideShowLanguageDetail(SlideShowLanguageDetailRequest requestModel)
    {
        var responseData = await _post.SlideShowLanguageDetail(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("SlideShowLanguageList")]
    public async Task<ActionResult<PublicControllerResponse>> SlideShowLanguageList(SlideShowLanguageListRequest requestModel)
    {
        var responseData = await _post.SlideShowLanguageList(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("SlideShowUpdate")]
    public async Task<ActionResult<PublicControllerResponse>> SlideShowUpdate(SlideShowUpdateRequest requestModel)
    {
        var responseData = await _post.SlideShowUpdate(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("SlideShowChangeStatus")]
    public async Task<ActionResult<PublicControllerResponse>> SlideShowChangeStatus(SlideShowChangeStatusRequest requestModel)
    {
        var responseData = await _post.SlideShowChangeStatus(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("SlideShowDetail")]
    public async Task<ActionResult<PublicControllerResponse>> SlideShowDetail(SlideShowDetailRequest requestModel)
    {
        var responseData = await _post.SlideShowDetail(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("SlideShowList")]
    public async Task<ActionResult<PublicControllerResponse>> SlideShowList(SlideShowListRequest requestModel)
    {
        var responseData = await _post.SlideShowList(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize(nameof(MemberRole.PostManager))]
    [HttpPost("RemoveSlideShowFile")]
    public async Task<ActionResult<PublicControllerResponse>> RemoveSlideShowFile(SlideShowRemoveFileRequest requestModel)
    {
        var responseData = await _post.RemoveSlideShowFile(requestModel);
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
    [HttpPost("SlideShowSite")]
    public async Task<ActionResult<PublicControllerResponse>> SlideShowSite(SlideShowWebsiteShowRequest requestModel)
    {
        var responseData = await _post.SlideShowSite(requestModel);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }


    #endregion



    //PostManager
    //PostAuthor

}
