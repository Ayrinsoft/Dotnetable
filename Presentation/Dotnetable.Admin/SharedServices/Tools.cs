using Dotnetable.Admin.SharedServices.Authorization;
using Dotnetable.Service;

namespace Dotnetable.Admin.SharedServices;

internal class Tools
{
    internal static string GetIpAddressFromHttpRequest(HttpRequest httprequest)
    {
        string ipAddressString = string.Empty;
        if (httprequest is null) return Guid.NewGuid().ToString();

        if (httprequest.Headers != null && httprequest.Headers.Count > 0)
        {
            if (httprequest.Headers.ContainsKey("X-Forwarded-For") && !string.IsNullOrEmpty(httprequest.Headers["X-Forwarded-For"]))
                ipAddressString = httprequest.Headers["X-Forwarded-For"];
            else if (httprequest.Headers.ContainsKey("Http-x-Forwarded-For") && !string.IsNullOrEmpty(httprequest.Headers["Http-x-Forwarded-For"]))
                ipAddressString = httprequest.Headers["Http-x-Forwarded-For"];
        }

        if (string.IsNullOrEmpty(ipAddressString) || ipAddressString == "") ipAddressString = httprequest.HttpContext.Connection.RemoteIpAddress.ToString();
        ipAddressString = ipAddressString.Contains(",") ? ipAddressString.Split(',').LastOrDefault().Trim() : ipAddressString.Trim();
        if (ipAddressString == "127.0.0.1") ipAddressString = "::1";
        if (string.IsNullOrEmpty(ipAddressString) || ipAddressString == "") return Guid.NewGuid().ToString();

        return ipAddressString;
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
