using Blazored.LocalStorage;
using Dotnetable.Admin.SharedServices.Authorization;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Authentication;

namespace Dotnetable.Admin.SharedServices;

public class Tools
{
    private ILocalStorageService _localStorage { get; set; }
    private IConfiguration _config { get; set; }
    public Tools(ILocalStorageService localStorage, IConfiguration config)
    {
        _localStorage = localStorage;
        _config = config;
    }

    internal async Task<int> GetRequesterMemberID()
    {
        int memberID = -1;
        try
        {
            if (!await _localStorage.ContainKeyAsync("JToken"))
                return -1;

            var currentToken = await _localStorage.GetItemAsync<UserLoginResponse.TokenItems>("JToken");
            if (!string.IsNullOrEmpty(currentToken.Token))
                memberID = await MemberService.FetchMemberIDByHashKey(new Guid(AuthenticationService.GetUserHashKeyFromAccessToken(currentToken.Token, LocalSecret.TokenHashKey(_config["AdminPanelSettings:ClientHash"]))));
        }
        catch (Exception) { }
        return memberID;
    }

    internal static async Task<int> GetRequesterMemberID(HttpRequest httprequest, IConfiguration config)
    {
        int memberID = -1;
        try
        {
            string jwtToken = httprequest.Headers["Authorization"][0].ToString().Split(' ')[1];
            if (!string.IsNullOrEmpty(jwtToken))
                memberID = await MemberService.FetchMemberIDByHashKey(new Guid(AuthenticationService.GetUserHashKeyFromAccessToken(jwtToken, LocalSecret.TokenHashKey(config["AdminPanelSettings:ClientHash"]))));
        }
        catch (Exception) { }
        return memberID;
    }

}
