using Dotnetable.Service;
using Dotnetable.Shared.Tools;
using Dotnetable.Shared.DTO.Logs;
using Dotnetable.Shared.DTO.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace Dotnetable.Admin.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("Api/[controller]")]
public class LogsController : ControllerBase
{

    #region CTOR
    private readonly LogsService _log;
    public LogsController(LogsService log)
    {
        _log = log;
    }
    #endregion

    [AllowAnonymous]
    [HttpPost("CheckIPActions")]
    public async Task<ActionResult<PublicControllerResponse>> CheckIPActions(LogsCheckIPActionValidRequest requestModel)
    {
        var responseData = await _log.CheckIPActions(requestModel);
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
