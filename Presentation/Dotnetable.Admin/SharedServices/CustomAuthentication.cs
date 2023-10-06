using Blazored.LocalStorage;
using Dotnetable.Shared.DTO.Authentication;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text;

namespace Dotnetable.Admin.SharedServices;

public class CustomAuthentication : AuthenticationStateProvider
{
    private ILocalStorageService _localStorage { get; set; }
    private IConfiguration _config { get; set; }

    public CustomAuthentication(ILocalStorageService localStorage, IConfiguration config)
    {
        _localStorage = localStorage;
        _config = config;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();
        try
        {
            if (await _localStorage.ContainKeyAsync("MemberAuthorized") && await _localStorage.ContainKeyAsync("HashedAuthorize"))
            {
                var jsonIdentity = await _localStorage.GetItemAsync<UserLoginResponse>("MemberAuthorized");
                string hashedItem = await _localStorage.GetItemAsStringAsync("HashedAuthorize");
                if (hashedItem == HashAuthorize(jsonIdentity.ToJsonString()) && Convert.ToDateTime(jsonIdentity.TokenDetail.ExpireTime) >= DateTime.Now)
                {
                    identity = GetClaimsIdentity(jsonIdentity);
                }
                else
                {
                    await MarkUserAsLoggedOut();
                }
            }
        }
        catch (Exception)
        {
            await _localStorage.RemoveItemAsync("MemberAuthorized");
            await _localStorage.RemoveItemAsync("JToken");
            //await localStorage.RemoveItemAsync("SELECTEDLANGUAGE");
        }

        var user = new ClaimsPrincipal(identity);
        return await Task.FromResult(new AuthenticationState(user));
    }

    public async Task MarkUserAsAuthenticated(UserLoginResponse user)
    {
        var identity = GetClaimsIdentity(user);
        await _localStorage.SetItemAsync("MemberAuthorized", user);
        await _localStorage.SetItemAsStringAsync("HashedAuthorize", HashAuthorize(user.ToJsonString()));
        await _localStorage.SetItemAsync("JToken", user.TokenDetail);

        var claimsPrincipal = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _localStorage.RemoveItemAsync("MemberAuthorized");
        await _localStorage.RemoveItemAsync("HashedAuthorize");
        await _localStorage.RemoveItemAsync("JToken");

        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    private ClaimsIdentity GetClaimsIdentity(UserLoginResponse user)
    {
        if (user is null || user.Email is null) return new ClaimsIdentity();

        var claimList = new List<Claim>();
        foreach (var j in user.Roles) claimList.Add(new Claim(ClaimTypes.Role, j));
        claimList.Add(new Claim(ClaimTypes.Name, $"{user.Givenname} {user.Surname}"));
        claimList.Add(new Claim(ClaimTypes.MobilePhone, user.CellphoneNumber));
        claimList.Add(new Claim(ClaimTypes.Email, user.Email));
        claimList.Add(new Claim(ClaimTypes.Hash, user.AvatarID?.ToString() ?? ""));
        claimList.Add(new Claim(ClaimTypes.Gender, (user.Gender ?? true) ? "male" : "female"));

        return new ClaimsIdentity(claimList, "apiauth_type");
    }

    internal string HashAuthorize(string authorizeJson)
    {
        byte[] bytesToHash = System.Security.Cryptography.SHA384.HashData(Encoding.UTF8.GetBytes($".net@b31ar0|{authorizeJson}|^20ak{_config["AdminPanelSettings:ClientHash"]}jh32@23"));
        StringBuilder resultString = new();
        foreach (byte b in bytesToHash) resultString.Append(b.ToString("x2"));
        return resultString.ToString();
    }

}
