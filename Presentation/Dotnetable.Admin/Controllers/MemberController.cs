using Asp.Versioning;
using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Member;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Admin.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("Api/[controller]")]
public class MemberController : ControllerBase
{

    #region CTOR
    private readonly MemberService _member;
    private IConfiguration _config;
    public MemberController(MemberService member, IConfiguration config)
    {
        _member = member;
        _config = config;
    }
    #endregion


    #region Admin

    //[Authorize(nameof(MemberRole.MemberManager))]
    //[HttpPost("MemberList")]
    //public async Task<ActionResult<PublicControllerResponse>> MemberList(MemberListRequest ListRequest)
    //{
    //    var responseData = await _member.MemberList(ListRequest);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    [Authorize(nameof(MemberRole.MemberManager))]
    [HttpPost("ChangeUserPassword")]
    public async Task<ActionResult<PublicControllerResponse>> ChangeUserPassword(MemberChangePasswordAdminRequest ChangeRequest)
    {
        var responseData = await _member.ChangeUserPassword(ChangeRequest);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    //[Authorize(nameof(MemberRole.MemberManager))]
    //[HttpPost("MemberDetailAdmin")]
    //public async Task<ActionResult<PublicControllerResponse>> MemberDetailAdmin(MemberDetailRequest DetailRequest)
    //{
    //    var responseData = await _member.MemberDetail(DetailRequest);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    //[Authorize(nameof(MemberRole.MemberManager))]
    //[HttpPost("RegisterAdmin")]
    //public async Task<ActionResult<PublicControllerResponse>> RegisterAdmin(MemberInsertRequest requestModel)
    //{
    //    requestModel.ActivateMember = true;
    //    var responseData = await _member.Register(requestModel);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    //[Authorize(nameof(MemberRole.MemberManager))]
    //[HttpPost("ChangeStatusAdmin")]
    //public async Task<ActionResult<PublicControllerResponse>> ChangeStatusAdmin(MemberChangeStatusRequest ChangeRequest)
    //{
    //    int CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);
    //    if (CurrentMemberID == ChangeRequest.MemberID)
    //        return new PublicControllerResponse() { ErrorException = new() { ErrorCode = "C16" } };

    //    var responseData = await _member.ChangeStatus(ChangeRequest);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    //[Authorize(nameof(MemberRole.MemberManager))]
    //[HttpPost("EditAdmin")]
    //public async Task<ActionResult<PublicControllerResponse>> EditAdmin(MemberEditRequest EditRequest)
    //{
    //    EditRequest.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);
    //    var responseData = await _member.Edit(EditRequest);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    //[Authorize(nameof(MemberRole.MemberManager))]
    //[HttpPost("ContactListAdmin")]
    //public async Task<ActionResult<PublicControllerResponse>> ContactListAdmin(MemberContactListRequest ListRequest)
    //{
    //    var responseData = await _member.ContactList(ListRequest);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    //[Authorize(nameof(MemberRole.MemberManager))]
    //[HttpPost("ContactUpdateAdmin")]
    //public async Task<ActionResult<PublicControllerResponse>> ContactUpdateAdmin(MemberContactRequest ChangeRequest)
    //{
    //    var responseData = await _member.ContactUpdate(ChangeRequest);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    //[Authorize(nameof(MemberRole.MemberManager))]
    //[HttpPost("ContactDeleteAdmin")]
    //public async Task<ActionResult<PublicControllerResponse>> ContactDeleteAdmin(MemberContactDeleteRequest DeleteRequest)
    //{
    //    var responseData = await _member.ContactDelete(DeleteRequest);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    //[Authorize(nameof(MemberRole.MemberManager))]
    //[HttpPost("ContactInsertAdmin")]
    //public async Task<ActionResult<PublicControllerResponse>> ContactInsertAdmin(MemberContactRequest InsertRequest)
    //{
    //    var responseData = await _member.ContactInsert(InsertRequest);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    //[Authorize(nameof(MemberRole.MemberManager))]
    //[HttpPost("AvatarListAdmin")]
    //public async Task<ActionResult<PublicControllerResponse>> AvatarListAdmin(MemberAvatarListRequest ListRequest)
    //{
    //    var responseData = await _member.AvatarList(ListRequest);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    //[Authorize(nameof(MemberRole.MemberManager))]
    //[HttpPost("AvatarDeleteAdmin")]
    //public async Task<ActionResult<PublicControllerResponse>> AvatarDeleteAdmin(MemberAvatarDeleteRequest DeleteRequest)
    //{
    //    var responseData = await _member.AvatarDelete(DeleteRequest);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    //[Authorize(nameof(MemberRole.MemberManager))]
    //[RequestSizeLimit(3_000_000)]
    //[HttpPost("AvatarInsertAdmin")]
    //public async Task<ActionResult<PublicControllerResponse>> AvatarInsertAdmin(MemberAvatarInsertRequest InsertRequest)
    //{
    //    InsertRequest.UploaderMemberID = await Tools.GetRequesterMemberID(Request, _config);
    //    var responseData = await _member.AvatarInsert(InsertRequest);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    //[Authorize(nameof(MemberRole.MemberManager))]
    //[HttpPost("AvatarDefaultAdmin")]
    //public async Task<ActionResult<PublicControllerResponse>> AvatarDefaultAdmin(MemberAvatarDefaultRequest ChangeRequest)
    //{
    //    ChangeRequest.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);
    //    var responseData = await _member.AvatarDefault(ChangeRequest);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    //[Authorize(nameof(MemberRole.MemberManager))]
    //[HttpPost("ActivateAdmin")]
    //public async Task<ActionResult<PublicControllerResponse>> ActivateAdmin(MemberActivateAdminRequest requestModel)
    //{
    //    var responseData = await _member.ActivateAdmin(requestModel);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    //[Authorize(nameof(MemberRole.MemberManager))]
    //[HttpPost("SendActivateLinkAdmin")]
    //public async Task<ActionResult<PublicControllerResponse>> SendActivateLinkAdmin(MemberActivateSendLinkRequest SendRequest)
    //{
    //    var responseData = await _member.SendActivateLink(SendRequest);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}


    #endregion

    #region Site

    [AllowAnonymous]
    [HttpPost("Register")]
    public async Task<ActionResult<PublicControllerResponse>> Register(MemberWebsiteRegisterRequest RegisterRequest)
    {
        RegisterRequest.ActivateMember = false;
        var responseData = await _member.RegisterWebsite(RegisterRequest);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [HttpPost("ChangeSelfPassword")]
    public async Task<ActionResult<PublicControllerResponse>> ChangeSelfPassword(MemberChangePasswordRequest ChangeRequest)
    {
        ChangeRequest.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);
        var responseData = await _member.ChangeSelfPassword(ChangeRequest);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [HttpGet("MemberDetail")]
    public async Task<ActionResult<PublicControllerResponse>> MemberDetail()
    {
        var responseData = await _member.MemberDetail(new MemberDetailRequest() { CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config) });
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [HttpGet("ChangeStatus")]
    public async Task<ActionResult<PublicControllerResponse>> ChangeStatus()
    {
        int MemberID = await Tools.GetRequesterMemberID(Request, _config);
        var responseData = await _member.ChangeStatus(new() { MemberID = MemberID });
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [HttpPost("Edit")]
    public async Task<ActionResult<PublicControllerResponse>> Edit(MemberEditRequest EditRequest)
    {
        EditRequest.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);
        var responseData = await _member.Edit(EditRequest);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [HttpGet("ContactList")]
    public async Task<ActionResult<PublicControllerResponse>> ContactList()
    {
        var responseData = await _member.ContactList(new() { CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config) });
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [HttpPost("ContactUpdate")]
    public async Task<ActionResult<PublicControllerResponse>> ContactUpdate(MemberContactRequest ChangeRequest)
    {
        ChangeRequest.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);
        var responseData = await _member.ContactUpdate(ChangeRequest);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [HttpPost("ContactDelete")]
    public async Task<ActionResult<PublicControllerResponse>> ContactDelete(MemberContactDeleteRequest DeleteRequest)
    {
        DeleteRequest.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);
        var responseData = await _member.ContactDelete(DeleteRequest);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [HttpPost("ContactInsert")]
    public async Task<ActionResult<PublicControllerResponse>> ContactInsert(MemberContactRequest InsertRequest)
    {
        InsertRequest.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);
        var responseData = await _member.ContactInsert(InsertRequest);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [HttpPost("AvatarList")]
    public async Task<ActionResult<PublicControllerResponse>> AvatarList(MemberAvatarListRequest ListRequest)
    {
        ListRequest.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);
        var responseData = await _member.AvatarList(ListRequest);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [HttpPost("AvatarDelete")]
    public async Task<ActionResult<PublicControllerResponse>> AvatarDelete(MemberAvatarDeleteRequest DeleteRequest)
    {
        DeleteRequest.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);
        var responseData = await _member.AvatarDelete(DeleteRequest);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [RequestSizeLimit(3_000_000)]
    [HttpPost("AvatarInsert")]
    public async Task<ActionResult<PublicControllerResponse>> AvatarInsert(MemberAvatarInsertRequest InsertRequest)
    {
        InsertRequest.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);
        InsertRequest.UploaderMemberID = InsertRequest.CurrentMemberID;
        var responseData = await _member.AvatarInsert(InsertRequest);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [HttpPost("AvatarDefault")]
    public async Task<ActionResult<PublicControllerResponse>> AvatarDefault(MemberAvatarDefaultRequest ChangeRequest)
    {
        ChangeRequest.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);
        var responseData = await _member.AvatarDefault(ChangeRequest);
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [HttpGet("SendActivateLink")]
    public async Task<ActionResult<PublicControllerResponse>> SendActivateLink(MemberActivateSendLinkRequest SendRequest)
    {
        SendRequest.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);
        var responseData = await _member.SendActivateLink(SendRequest);
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
    [HttpGet("Activate/{ActivateCode}")]
    public async Task<ActionResult<PublicControllerResponse>> Activate(string ActivateCode)
    {
        var responseData = await _member.ActivateLinkCheck(new Guid(ActivateCode));
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

    #region ForgetPass
    [AllowAnonymous]
    [HttpPost("ForgetPasswordGetCode")]
    public async Task<ActionResult<PublicControllerResponse>> ForgetPasswordGetCode(MemberForgetPasswordRequest requestModel)
    {
        var responseData = await _member.ForgetPasswordGetCode(requestModel);
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
    [HttpPost("ForgetPasswordSetCode")]
    public async Task<ActionResult<PublicControllerResponse>> ForgetPasswordSetCode(MemberForgetPasswordSetRequest requestModel)
    {
        var responseData = await _member.ForgetPasswordSetCode(requestModel);
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

    #region Subscribe

    [AllowAnonymous]
    [HttpPost("EmailSubscribeRegister")]
    public async Task<ActionResult<PublicControllerResponse>> EmailSubscribeRegister(MemberEmailSubscribeRegisterRequest requestModel)
    {
        requestModel.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);
        requestModel.RequestURL = $"{(Request.IsHttps ? "https://" : "http://")}{Request.Host}{Request.Path.Value.Replace("EmailSubscribeRegister", "EmailSubscribeApprove")}";
        var responseData = await _member.EmailSubscribeRegister(requestModel);
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
    [HttpGet("EmailSubscribeApprove/{EmailAddress}/{CheckHash}")]
    public async Task<ActionResult> EmailSubscribeApprove(string EmailAddress, string CheckHash)
    {
        int CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);
        var CheckResponse = await _member.EmailSubscribeApprove(new() { CheckHash = CheckHash, EmailAddress = EmailAddress, MemberID = CurrentMemberID > 0 ? CurrentMemberID : null });

        string FinalHost = "";

        var FetchHostName = Request.Host.Host.Split('.');
        if (FetchHostName.Length == 1) FinalHost = FetchHostName[0];
        if (FetchHostName.Length == 2) FinalHost = $"{FetchHostName[0]}.{FetchHostName[1]}";
        if (FetchHostName.Length == 3) FinalHost = $"{FetchHostName[1]}.{FetchHostName[1]}";

        return Redirect(FinalHost);
    }

    [AllowAnonymous]
    [HttpPost("EmailSubscribeRemoveSendCode")]
    public async Task<ActionResult<PublicControllerResponse>> EmailSubscribeRemoveSendCode(MemberEmailSubscribeRemoveSendCodeRequest requestModel)
    {
        requestModel.RequestURL = $"{(Request.IsHttps ? "https://" : "http://")}{Request.Host}{Request.Path.Value.Replace("EmailSubscribeRemoveSendCode", "EmailSubscribeRemove")}";

        var responseData = await _member.EmailSubscribeRemoveSendCode(requestModel);
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
    [HttpGet("EmailSubscribeRemove/{EmailAddress}/{CheckHash}")]
    public async Task<ActionResult<PublicControllerResponse>> EmailSubscribeRemove(string EmailAddress, string CheckHash)
    {
        MemberEmailSubscribeRemoveRequest requestModel = new() { CheckHash = CheckHash, EmailAddress = EmailAddress };
        requestModel.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);

        var CheckResponse = await _member.EmailSubscribeRemove(requestModel);

        string FinalHost = "";

        var FetchHostName = Request.Host.Host.Split('.');
        if (FetchHostName.Length == 1) FinalHost = FetchHostName[0];
        if (FetchHostName.Length == 2) FinalHost = $"{FetchHostName[0]}.{FetchHostName[1]}";
        if (FetchHostName.Length == 3) FinalHost = $"{FetchHostName[1]}.{FetchHostName[1]}";

        return Redirect(FinalHost);
    }


    //[Authorize(nameof(MemberRole.MemberManager))]
    //[HttpPost("MemberSubscribedList")]
    //public async Task<ActionResult<PublicControllerResponse>> MemberSubscribedList(SubscribeListRequest requestModel)
    //{
    //    var responseData = await _member.MemberSubscribedList(requestModel);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}


    //[Authorize(nameof(MemberRole.MemberManager))]
    //[HttpPost("MemberSubscribedChangeStatus")]
    //public async Task<ActionResult<PublicControllerResponse>> MemberSubscribedChangeStatus(SubscribedChangeStatusRequest requestModel)
    //{
    //    var responseData = await _member.MemberSubscribedChangeStatus(requestModel);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}



    #endregion

    //#region Policy&Role
    //[Authorize(nameof(MemberRole.PolicyManager))]
    //[HttpGet("RoleList")]
    //public async Task<ActionResult<PublicControllerResponse>> RoleList()
    //{
    //    int MemberID = await Tools.GetRequesterMemberID(Request, _config);

    //    RoleListRequest requestModel = new() { CurrentMemberID = MemberID };
    //    var responseData = await _member.RoleList(requestModel);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    //[Authorize(nameof(MemberRole.MemberManager))]
    //[HttpGet("PolicyListOnMemberManage")]
    //public async Task<ActionResult<PublicControllerResponse>> PolicyListOnMemberManage()
    //{
    //    var responseData = await _member.PolicyListOnMemberManage(await Tools.GetRequesterMemberID(Request, _config));
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null
    //    };
    //}

    //[Authorize(nameof(MemberRole.MemberManager))]
    //[HttpGet("RoleListOnPolicyManage")]
    //public async Task<ActionResult<PublicControllerResponse>> RoleListOnPolicyManage()
    //{
    //    var responseData = await _member.RoleListOnPolicyManage(await Tools.GetRequesterMemberID(Request, _config));
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null
    //    };
    //}

    //[Authorize(nameof(MemberRole.PolicyManager))]
    //[HttpPost("PolicyList")]
    //public async Task<ActionResult<PublicControllerResponse>> PolicyList(PolicyListRequest requestModel)
    //{
    //    requestModel.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);

    //    var responseData = await _member.PolicyList(requestModel);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    //[Authorize(nameof(MemberRole.PolicyManager))]
    //[HttpPost("PolicyRolesList")]
    //public async Task<ActionResult<PublicControllerResponse>> PolicyRolesList(PolicyRoleListRequest requestModel)
    //{
    //    var responseData = await _member.PolicyRolesList(requestModel);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}


    //[Authorize(nameof(MemberRole.PolicyManager))]
    //[HttpPost("PolicyInsert")]
    //public async Task<ActionResult<PublicControllerResponse>> PolicyInsert(PolicyInsertRequest requestModel)
    //{
    //    var responseData = await _member.PolicyInsert(requestModel);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    //[Authorize(nameof(MemberRole.PolicyManager))]
    //[HttpPost("PolicyDetail")]
    //public async Task<ActionResult<PublicControllerResponse>> PolicyDetail(PolicyDetailRequest requestModel)
    //{
    //    var responseData = await _member.PolicyDetail(requestModel);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    //[Authorize(nameof(MemberRole.PolicyManager))]
    //[HttpPost("PolicyUpdate")]
    //public async Task<ActionResult<PublicControllerResponse>> PolicyUpdate(PolicyUpdateRequest requestModel)
    //{
    //    var responseData = await _member.PolicyUpdate(requestModel);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    //[Authorize(nameof(MemberRole.PolicyManager))]
    //[HttpPost("PolicyChangeStatus")]
    //public async Task<ActionResult<PublicControllerResponse>> PolicyChangeStatus(PolicyChangeStatusRequest requestModel)
    //{
    //    requestModel.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);

    //    var responseData = await _member.PolicyChangeStatus(requestModel);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    //[Authorize(nameof(MemberRole.PolicyManager))]
    //[HttpPost("PolicyRoleRemove")]
    //public async Task<ActionResult<PublicControllerResponse>> PolicyRoleRemove(PolicyRoleRemoveRequest requestModel)
    //{
    //    requestModel.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);

    //    var responseData = await _member.PolicyRoleRemove(requestModel);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}

    //[Authorize(nameof(MemberRole.PolicyManager))]
    //[HttpPost("MemberRoleAppend")]
    //public async Task<ActionResult<PublicControllerResponse>> MemberRoleAppend(PolicyRoleAppendRequest requestModel)
    //{
    //    requestModel.CurrentMemberID = await Tools.GetRequesterMemberID(Request, _config);

    //    var responseData = await _member.MemberRoleAppend(requestModel);
    //    string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
    //        ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
    //    };
    //}







    //#endregion

}
