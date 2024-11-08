using Asp.Versioning;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Message;
using Dotnetable.Shared.DTO.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Admin.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("Api/[controller]")]

public class MessageController : ControllerBase
{

    #region CTOR
    private readonly MessageService _msg;
    public MessageController(MessageService msg)
    {
        _msg = msg;
    }
    #endregion

    [AllowAnonymous]
    [HttpPost("ContactUsMessageInsert")]
    public async Task<ActionResult<PublicControllerResponse>> ContactUsMessageInsert(MessageBoxOnContactUsRequest requestModel)
    {
        var responseData = await _msg.ContactUsMessageInsert(requestModel);
        var responseExeption = responseData.ErrorException;
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && responseExeption is null,
            ErrorException = responseExeption
        };
    }

    //[Authorize(nameof(MemberRole.MessageManager))]
    //[HttpPost("ContactUsMessageList")]
    //public async Task<ActionResult<PublicControllerResponse>> ContactUsMessageList(MessageContactUsListRequest requestModel)
    //{
    //    var responseData = await _msg.ContactUsMessageList(requestModel);
    //    var responseExeption = responseData.ErrorException;
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && responseExeption is null,
    //        ErrorException = responseExeption
    //    };
    //}

    //[Authorize(nameof(MemberRole.MessageManager))]
    //[HttpPost("ContactUsMessageArchive")]
    //public async Task<ActionResult<PublicControllerResponse>> ContactUsMessageArchive(MessageContactUsChangesRequest requestModel)
    //{
    //    var responseData = await _msg.ContactUsMessageArchive(requestModel);
    //    var responseExeption = responseData.ErrorException;
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && responseExeption is null,
    //        ErrorException = responseExeption
    //    };
    //}

    //[Authorize(nameof(MemberRole.MessageManager))]
    //[HttpPost("ContactUsMessageDelete")]
    //public async Task<ActionResult<PublicControllerResponse>> ContactUsMessageDelete(MessageContactUsChangesRequest requestModel)
    //{
    //    var responseData = await _msg.ContactUsMessageDelete(requestModel);
    //    var responseExeption = responseData.ErrorException;
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && responseExeption is null,
    //        ErrorException = responseExeption
    //    };
    //}


    //[Authorize(nameof(MemberRole.MessageManager))]
    //[HttpPost("EmailSettingInsert")]
    //public async Task<ActionResult<PublicControllerResponse>> EmailSettingInsert(EmailPanelInsertRequest requestModel)
    //{
    //    var responseData = await _msg.EmailSettingInsert(requestModel);
    //    var responseExeption = responseData.ErrorException;
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && responseExeption is null,
    //        ErrorException = responseExeption
    //    };
    //}

    //[Authorize(nameof(MemberRole.MessageManager))]
    //[HttpPost("EmailSettingUpdate")]
    //public async Task<ActionResult<PublicControllerResponse>> EmailSettingUpdate(EmailPanelUpdateRequest requestModel)
    //{
    //    var responseData = await _msg.EmailSettingUpdate(requestModel);
    //    var responseExeption = responseData.ErrorException;
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && responseExeption is null,
    //        ErrorException = responseExeption
    //    };
    //}

    //[Authorize(nameof(MemberRole.MessageManager))]
    //[HttpPost("EmailSettingChangeStatus")]
    //public async Task<ActionResult<PublicControllerResponse>> EmailSettingChangeStatus(EmailPanelChangeStatusRequest requestModel)
    //{
    //    var responseData = await _msg.EmailSettingChangeStatus(requestModel);
    //    var responseExeption = responseData.ErrorException;
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && responseExeption is null,
    //        ErrorException = responseExeption
    //    };
    //}

    //[Authorize(nameof(MemberRole.MessageManager))]
    //[HttpPost("EmailSettingList")]
    //public async Task<ActionResult<PublicControllerResponse>> EmailSettingList(EmailPanelListRequest requestModel)
    //{
    //    var responseData = await _msg.EmailSettingList(requestModel);
    //    var responseExeption = responseData.ErrorException;
    //    responseData.ErrorException = null;
    //    return new PublicControllerResponse()
    //    {
    //        ResponseData = responseData,
    //        Success = responseData is not null && responseExeption is null,
    //        ErrorException = responseExeption
    //    };
    //}




}
