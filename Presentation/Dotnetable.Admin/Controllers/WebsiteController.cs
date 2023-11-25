using Asp.Versioning;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.DTO.Website;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Admin.Controllers
{

    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("Api/[controller]")]
    public class WebsiteController : ControllerBase
    {
        #region CTOR
        private readonly WebsiteService _website;
        private readonly IConfiguration _config;
        public WebsiteController(WebsiteService website, IConfiguration config)
        {
            _website = website;
            _config = config;
        }
        #endregion

        [AllowAnonymous]
        [HttpGet("ImplementDB")]
        public async Task<ActionResult<PublicControllerResponse>> ImplementDB()
        {
            if (_config["InsertDataMode"] != "CREATE_DB")
                return NotFound();

            var appSettingsBody = (System.IO.File.ReadAllText("appsettings.json")).JsonToObject<AdminPanelAppSettingsResponse>();

            var insertResponse = await _website.ImplementDB(appSettingsBody.AdminPanelSettings.DefaultLanguageCode);

            if (insertResponse.SuccessAction)
            {
                appSettingsBody.InsertDataMode = "INSERT_DATA";
                _config.GetSection("AppSettings").Bind(appSettingsBody);
                System.IO.File.WriteAllText("appsettings.json", appSettingsBody.ToJsonString());
            }

            string responseExeption = insertResponse.ErrorException is null ? string.Empty : insertResponse.ErrorException.ToJsonString();
            insertResponse.ErrorException = null;
            return new PublicControllerResponse()
            {
                ResponseData = insertResponse,
                Success = insertResponse is not null && string.IsNullOrEmpty(responseExeption),
                ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
            };
        }

        [AllowAnonymous]
        [HttpPost("InsertFirstData")]
        public async Task<ActionResult<PublicControllerResponse>> InsertFirstData(AdminPanelFirstDataRequest requestModel)
        {
            if (_config["InsertDataMode"] != "INSERT_DATA")
                return NotFound();

            var insertResponse = await _website.InsertFirstData(requestModel);

            if (insertResponse.SuccessAction)
            {
                var appSettingsBody = (System.IO.File.ReadAllText("appsettings.json")).JsonToObject<AdminPanelAppSettingsResponse>();
                appSettingsBody.InsertDataMode = "COMPLETE";
                _config.GetSection("AppSettings").Bind(appSettingsBody);
                System.IO.File.WriteAllText("appsettings.json", appSettingsBody.ToJsonString());
            }

            string responseExeption = insertResponse.ErrorException is null ? string.Empty : insertResponse.ErrorException.ToJsonString();
            insertResponse.ErrorException = null;
            return new PublicControllerResponse()
            {
                ResponseData = insertResponse,
                Success = insertResponse is not null && string.IsNullOrEmpty(responseExeption),
                ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
            };
        }



    }
}
