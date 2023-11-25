using Dotnetable.Data.DataAccess;
using Dotnetable.Service.PrivateService;
using Dotnetable.Shared.DTO.Authentication;
using Dotnetable.Shared.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Dotnetable.Service;
public class AuthenticationService
{

    private static IConfiguration _config;
    public AuthenticationService(IConfiguration config)
    {
            _config = config;
    }

    private const string JWTIssuer = "https://ayrinsoft.com";
    public async Task<UserLoginResponse> LoginUser(UserLoginRequest requestModel, string tokenHashKey)
    {
        requestModel.Username = requestModel.Username.ToLower();
        requestModel.Password = requestModel.Password.HashLogin();

        var loginDetail = await AuthenticationDataAccess.LoginUser(requestModel);

        if (loginDetail is null || loginDetail.ErrorException is not null)
            return new() { ErrorException = loginDetail?.ErrorException ?? new() { ErrorCode = "D0" } };


        loginDetail.TokenDetail = new UserLoginResponse.TokenItems()
        {
            ExpireTime = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"),
            Token = GenerateAccessToken(new() { HashKey = loginDetail.HashKey.ToString(), MemberID = loginDetail.MemberID, LogHashKey = Guid.NewGuid().ToString(), TokenHashKey = tokenHashKey }),
            RefreshToken = (await GenerateRefreshToken(new RefreshTokenRequest() { MemberID = loginDetail.MemberID, ClientIP = requestModel.ClientIP }))
        };

        return loginDetail;
    }

    public UserLoginResponse RefreshToken(RefreshTokenValidationRequest refreshTokenRequest, string tokenHashKey)
    {
        string userHashKey = GetUserHashKeyFromAccessToken(refreshTokenRequest.Token, tokenHashKey);
        if (string.IsNullOrEmpty(userHashKey)) return null;

        var refreshTokenResponse = AuthenticationDataAccess.RefreshTokenValidation(new() { RefreshToken = refreshTokenRequest.RefreshToken, UserHashKey = new Guid(userHashKey) });
        UserLoginResponse userDetail = refreshTokenResponse.CastModel<UserLoginResponse>();

        if (userDetail != null)
        {
            userDetail.TokenDetail = new()
            {
                Token = GenerateAccessToken(new() { HashKey = userHashKey, LogHashKey = Guid.NewGuid().ToString(), MemberID = userDetail.MemberID }),
                RefreshToken = refreshTokenRequest.RefreshToken.ToString(),
                ExpireTime = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss")
            };
        }

        return userDetail;
    }

    public static string GetUserHashKeyFromAccessToken(string accessToken, string tokenHashKey)
    {
        string userHashKey = "";
        try
        {
            JwtSecurityTokenHandler tokenHandler = new();
            TokenValidationParameters tokenValidationParameters = new()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenHashKey)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
            var Principle = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is JwtSecurityToken jwtSecurityToken && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha384, StringComparison.InvariantCultureIgnoreCase))
                userHashKey = Principle.FindFirst(ClaimTypes.Name)?.Value;
        }
        catch (Exception) { }

        return userHashKey;
    }

    private async Task<string> GenerateRefreshToken(RefreshTokenRequest requestModel)
    {
        if (requestModel.MemberID < 1) return "";
        RefreshTokenResponse refreshToken = new()
        {
            Token = Guid.NewGuid(),
            ExpireTime = DateTime.Now.AddDays(10),
            MemberID = requestModel.MemberID,
            ClientIP = requestModel.ClientIP
        };

        await AuthenticationDataAccess.RefreshTokenInsert(refreshToken);
        return refreshToken.Token.ToString();
    }

    private string GenerateAccessToken(JWTGenerateRequest requestModel)
    {
        JwtSecurityTokenHandler tokenHandler = new();
        SecurityTokenDescriptor TokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new(ClaimTypes.Name, requestModel.HashKey.ToLower()),
                new(ClaimTypes.Hash, CreateJWTHash(new(){ LogHashKey = requestModel.LogHashKey, UserHashKey = requestModel.HashKey, MemberID = requestModel.MemberID }, _config)),
                new(ClaimTypes.AuthorizationDecision, requestModel.LogHashKey.ToLower()),
                new(ClaimTypes.Spn, requestModel.MemberID.ToString())
            }),
            Expires = DateTime.Now.AddDays(1),
            Issuer = JWTIssuer,
            Audience = JWTIssuer,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(requestModel.TokenHashKey)), SecurityAlgorithms.HmacSha384)
        };
        var token = tokenHandler.CreateToken(TokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<bool> UserValidatePolicy(UserValidatePolicyRequest validateRequest)
    {
        return await AuthenticationDataAccess.UserValidatePolicy(validateRequest);
    }

    public static bool CheckJWTHash(JWTCheckHashRequest requestModel, IConfiguration config)
    {
        return string.Equals(requestModel.JWTHashKey, CreateJWTHash(new() { UserHashKey = requestModel.UserHashKey, LogHashKey = requestModel.LogHashKey, MemberID = requestModel.MemberID }, config), StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateJWTHash(JWTCreateHashRequest requestModel, IConfiguration config)
    {
        return HashJWT($"{requestModel.UserHashKey.Substring(2, 21)}&{requestModel.MemberID}!{requestModel.LogHashKey}{requestModel.UserHashKey}{requestModel.UserHashKey[..15]}{config["AdminPanelSettings:ClientHash"]}".ToUpper(), config);
    }

    public static bool ValidateCurrentToken(string Token, string tokenHashKey)
    {
        JwtSecurityTokenHandler tokenHandler = new();
        try
        {
            tokenHandler.ValidateToken(Token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = JWTIssuer,
                ValidAudience = JWTIssuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenHashKey)),
            }, out SecurityToken _);
        }
        catch
        {
            return false;
        }
        return true;
    }

    private static string HashJWT(string stringToHash, IConfiguration config)
    {
        byte[] bytesToHash = Encoding.UTF8.GetBytes($"C#udP@soh!8^x8*&@%{stringToHash}#@dAS$s838^{config["AdminPanelSettings:ClientHash"]}#uph!@%^!k@");
        bytesToHash = System.Security.Cryptography.SHA384.HashData(bytesToHash);
        var hashedResult = new StringBuilder();
        foreach (byte b in bytesToHash) hashedResult.Append(b.ToString("x2"));
        return hashedResult.ToString();
    }




}