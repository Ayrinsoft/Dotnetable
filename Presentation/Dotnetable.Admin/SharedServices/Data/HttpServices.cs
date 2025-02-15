using Blazored.LocalStorage;
using Dotnetable.Shared.DTO.Authentication;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components.Authorization;

namespace Dotnetable.Admin.SharedServices.Data;

public class HttpServices : IHttpServices
{
    private HttpClient _httpClient { get; }
    private AuthenticationStateProvider _authenticationStateProvider { get; }
    private ILocalStorageService _localStorage { get; set; }
    public HttpServices(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider, ILocalStorageService localStorage, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _authenticationStateProvider = authenticationStateProvider;
        _localStorage = localStorage;

        HttpContext context = httpContextAccessor.HttpContext;
        _httpClient.BaseAddress = new Uri($"{context.Request.Scheme}://{context.Request.Host}");
    }

    public async Task<PublicControllerResponse> CallServiceObjAsync(HttpMethod method, bool hasAutentication, string urlAddress, string requestBody = "")
    {
        urlAddress = $"/api/{urlAddress}";
        HttpRequestMessage requestor = new(method, urlAddress) { Content = new StringContent(requestBody) };
        requestor.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        if (hasAutentication)
        {
            if (!await _localStorage.ContainKeyAsync("JToken"))
                await ((CustomAuthentication)_authenticationStateProvider).MarkUserAsLoggedOut();

            var FetchToken = await _localStorage.GetItemAsync<UserLoginResponse.TokenItems>("JToken");
            if (Convert.ToDateTime(FetchToken.ExpireTime) < DateTime.Now)
                await ((CustomAuthentication)_authenticationStateProvider).MarkUserAsLoggedOut();

            requestor.Headers.Add("Authorization", $"Bearer {FetchToken.Token}");
        }

        PublicControllerResponse responseObj;
        try
        {
            var responseAPI = await _httpClient.SendAsync(requestor);
            if (responseAPI.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseString = await responseAPI.Content.ReadAsStringAsync();
                responseObj = responseString.JsonToObject<PublicControllerResponse>();
            }
            else
            {
                responseObj = new() { Success = false, ErrorException = new() { ErrorCode = "SX", Message = $"Server return StatusCode: {responseAPI.StatusCode}" } };
            }
        }
        catch (Exception ex)
        {
            responseObj = new() { Success = false, ErrorException = new() { ErrorCode = "SX", Message = ex.Message } };
        }
        return responseObj;
    }

}
