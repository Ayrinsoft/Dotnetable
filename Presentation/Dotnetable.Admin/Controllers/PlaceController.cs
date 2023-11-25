using Dotnetable.Service;
using Dotnetable.Shared.Tools;
using Dotnetable.Shared.DTO.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace Dotnetable.Admin.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("Api/[controller]")]

public class PlaceController : ControllerBase
{
    #region CTOR
    private readonly PlaceService _place;
    public PlaceController(PlaceService place)
    {
        _place = place;
    }
    #endregion

    [ResponseCache(Duration = 2592000, Location = ResponseCacheLocation.Any, VaryByHeader = "User-Agent", NoStore = false)]
    [AllowAnonymous]
    [HttpGet("CountryList")]
    public async Task<ActionResult<PublicControllerResponse>> CountryList()
    {
        var responseData = await _place.CountryList();
        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }

    [ResponseCache(Duration = 2592000, Location = ResponseCacheLocation.Any, VaryByHeader = "User-Agent", NoStore = false)]
    [AllowAnonymous]
    [HttpGet("CityList")]
    public async Task<ActionResult<PublicControllerResponse>> CityList()
    {
        var responseData = await _place.CityList();
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
