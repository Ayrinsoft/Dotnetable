using Dotnetable.Data.DBContext;
using Dotnetable.Shared.DTO.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Data.DataAccess;
public class AuthenticationDataAccess
{

    public static async Task<UserLoginResponse> LoginUser(UserLoginRequest requestModel)
    {
        using DotnetableEntity db = new();
        if (!await (from i in db.TB_Members where i.Username == requestModel.Username && i.Password == requestModel.Password && i.Activate && i.Active select i).AnyAsync())
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        db.TB_Login_Tries.Add(new() { IsSuccess = true, LogTime = DateTime.Now, TryIP = requestModel.ClientIP, Username = requestModel.Username });

        var responseModel = await (from i in db.TB_Members
                                   join p in db.TB_Policies on i.PolicyID equals p.PolicyID
                                   where i.Username == requestModel.Username && i.Password == requestModel.Password && i.Activate && i.Active && p.Active
                                   select new UserLoginResponse { AvatarID = i.AvatarID, CellphoneNumber = i.CellphoneNumber, Email = i.Email, Gender = i.Gender, Givenname = i.Givenname, HashKey = i.HashKey, MemberID = i.MemberID, PolicyName = p.Title, RegisterDate = i.RegisterDate, Surname = i.Surname }).FirstOrDefaultAsync();

        responseModel.Roles = await (from m in db.TB_Members
                                     join pr in db.TBM_Policy_Roles on m.PolicyID equals pr.PolicyID
                                     join r in db.TB_Roles on pr.RoleID equals r.RoleID
                                     where r.Active && pr.Active && m.Active && m.MemberID == responseModel.MemberID
                                     select r.RoleKey).ToListAsync();

        responseModel.LanguageCode = (await WebsiteDataAccess.FetchSettingByKey("DEFAULT_LANGUAGE_CODE")) ?? "EN";

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return responseModel;
    }

    public static async Task<bool> RefreshTokenInsert(RefreshTokenResponse refreshToken)
    {
        using DotnetableEntity db = new();

        db.TB_Login_Tokens.Add(new()
        {
            MemberID = refreshToken.MemberID,
            ExpireTime = refreshToken.ExpireTime,
            RefreshToken = refreshToken.Token
        });
        await db.SaveChangesAsync();

        return true;
    }

    public static RefreshTokenValidateResponse RefreshTokenValidation(RefreshTokenValidateRequest refreshTokenRequest)
    {
        return null;
    }

    public static async Task<bool> UserValidatePolicy(UserValidatePolicyRequest validateRequest)
    {
        using DotnetableEntity db = new();

        return await (from m in db.TB_Members
                      join pr in db.TBM_Policy_Roles on m.PolicyID equals pr.PolicyID
                      join r in db.TB_Roles on pr.RoleID equals r.RoleID
                      where r.Active && pr.Active && m.Active && m.HashKey == validateRequest.UserHashKey && validateRequest.RoleNames.Any(i => i == r.RoleKey)
                      select m).AnyAsync();
    }


}