using Dotnetable.Service;
using Dotnetable.Shared.Tools;
using Dotnetable.Shared.DTO.Authentication;
using Dotnetable.Shared.DTO.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Dotnetable.Admin.SharedServices.Authorization;

namespace Dotnetable.Admin.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("Api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly AuthenticationService _auth;
    private readonly IConfiguration _config;
    public AuthenticationController(AuthenticationService auth, IConfiguration config)
    {
        _auth = auth;
        _config = config;
    }   

    [HttpGet("Login")]
    public ActionResult Login()
    {
        return Content("<html><header><title>Please Login</title></header><body><h1>Login Please</h1></body></html>", "text/html", Encoding.UTF8);
    }

    [HttpPost("Login")]
    public async Task<ActionResult<PublicControllerResponse>> Login(UserLoginRequest requestModel)
    {
        var responseData = await _auth.LoginUser(requestModel, LocalSecret.TokenHashKey(_config["AdminPanelSettings:ClientHash"]));
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [Authorize]
    [HttpGet("CheckAuthorize")]
    public ActionResult<PublicControllerResponse> CheckAuthorize()
    {
        return new PublicControllerResponse() { Success = true };
    }


}
