using Dotnetable.Data.DBContext;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.DTO.Website;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Data.DataAccess;

public class WebsiteDataAccess
{

    public static async Task<string> FetchSettingByKey(string settingKey)
    {
        using DotnetableEntity db = new();
        return await (from i in db.TB_Settings where i.SettingKey == settingKey select i.SettingValue).FirstOrDefaultAsync();
    }

    public static async Task<PublicActionResponse> ImplementDB(string languageCode)
    {
        try
        {
            using DotnetableEntity db = new();
            await db.Database.EnsureCreatedAsync();

            await db.SaveChangesAsync();

            foreach (var j in Enum.GetValues<CommentType>())
                db.TB_Comment_Types.Add(new() { CommentTypeID = (byte)j, Title = j.ToString() });

            foreach (var j in Enum.GetValues<EmailType>())
                db.TB_Email_Types.Add(new() { EmailTypeID = (byte)j, Title = j.ToString() });


            //Files
            db.TB_File_Types.Add(new() { FileTypeName = "", FileExtention = "png", MIMEType = "image/jpg" });
            db.TB_File_Types.Add(new() { FileTypeName = "", FileExtention = "svc", MIMEType = "application/vnd.dvb.service" });
            db.TB_File_Types.Add(new() { FileTypeName = "", FileExtention = "gif", MIMEType = "image/gif" });
            db.TB_File_Types.Add(new() { FileTypeName = "", FileExtention = "jpeg", MIMEType = "image/jpeg" });
            db.TB_File_Types.Add(new() { FileTypeName = "", FileExtention = "jpg", MIMEType = "image/jpeg" });

            db.TB_File_Categories.Add(new() { FileCategoryID = 0, Tite = "Temporary" });
            db.TB_File_Categories.Add(new() { FileCategoryID = 1, Tite = "Member Gallery" });
            db.TB_File_Categories.Add(new() { FileCategoryID = 2, Tite = "Post" });
            db.TB_File_Categories.Add(new() { FileCategoryID = 3, Tite = "Post Category" });
            db.TB_File_Categories.Add(new() { FileCategoryID = 4, Tite = "Brand" });
            db.TB_File_Categories.Add(new() { FileCategoryID = 5, Tite = "SlideShow" });

            db.TB_Settings.Add(new() { SettingKey = "DEFAULT_LANGUAGE_CODE", SettingValue = languageCode });

            TB_Policy policyObj = new() { Active = true, Title = "ADMIN" };
            db.TB_Policies.Add(policyObj);

            foreach (var j in Enum.GetValues<MemberRole>())
                db.TB_Roles.Add(new() { Active = true, Description = "", RoleKey = j.ToString() });

            await db.SaveChangesAsync();

            var fetchAllRoles = await (from i in db.TB_Roles select i.RoleID).ToListAsync();
            foreach (var j in fetchAllRoles)
                db.TBM_Policy_Roles.Add(new() { RoleID = j, Active = true, PolicyID = policyObj.PolicyID });

            TB_Post_Category PostcategoryObj = new() { ParentID = null, MenuView = false, Title = "Websiteobjects", Tags = null, MetaDescription = null, MetaKeywords = null, Priority = 0, Active = true, FooterView = false, Description = "", FileCode = null, LanguageCode = languageCode };
            db.TB_Post_Categories.Add(PostcategoryObj);

            await db.SaveChangesAsync();

            return new() { SuccessAction = true };
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = $"{x.Message} -|- {x.InnerException?.Message}" } };
        }
    }

    public static async Task<PublicActionResponse> InsertFirstData(AdminPanelFirstDataRequest requestModel)
    {
        using DotnetableEntity db = new();
        try
        {
            var fetchPostCategory = await (from i in db.TB_Post_Categories where i.Title == "Websiteobjects" select new { i.LanguageCode, i.PostCategoryID }).FirstOrDefaultAsync();

            TB_Country countryObj = new()
            {
                CountryCode = requestModel.CountryCode,
                Title = requestModel.CountryName,
                LanguageCode = fetchPostCategory.LanguageCode,
                PhonePerfix = ""
            };

            db.TB_Countries.Add(countryObj);
            await db.SaveChangesAsync();


            TB_City cityObj = new()
            {
                LanguageCode = fetchPostCategory.LanguageCode,
                Active = true,
                CountryID = countryObj.CountryID,
                Title = requestModel.CityName
            };

            db.TB_Cities.Add(cityObj);

            db.TB_Settings.Add(new() { SettingKey = "MANAGER_EMAIL_ADDRESS", SettingValue = requestModel.Email });

            await db.SaveChangesAsync();

            var fetchPolicy = await (from i in db.TB_Policies select i.PolicyID).FirstOrDefaultAsync();

            TB_Member memberObj = new()
            {
                Activate = true,
                Active = true,
                AvatarID = null,
                CellphoneNumber = requestModel.CellphoneNumber,
                CityID = cityObj.CityID,
                Email = requestModel.Email,
                CountryCode = requestModel.PhoneCountryCode,
                Gender = requestModel.Gender,
                PolicyID = fetchPolicy,
                HashKey = Guid.NewGuid(),
                Givenname = requestModel.GivenName,
                Surname = requestModel.Surname,
                Password = requestModel.Password,
                PostalCode = "",
                RegisterDate = DateTime.Now,
                Username = requestModel.Username
            };
            db.TB_Members.Add(memberObj);

            foreach (var j in requestModel.AvailableLanguages)
                db.TB_Languages.Add(new() { ItemPriority = null, LanguageCode = j, RTLDesign = false, Title = j, LanguageISOCode = j, LocalizedTitle = "" });

            await db.SaveChangesAsync();

            db.TB_Posts.Add(new() { Title = "Contact us", Summary = "Contact us", PostCode = "ContactUs", Active = true, Body = "", FileCode = null, LanguageCode = fetchPostCategory.LanguageCode, LogTime = DateTime.Now, MemberID = memberObj.MemberID, PostCategoryID = fetchPostCategory.PostCategoryID, MetaDescription = "", MetaKeywords = "", Tags = "", NormalBody = false, VisitCount = 0 });

            db.TB_Posts.Add(new() { Title = "About us", Summary = "About us", PostCode = "AboutUs", Active = true, Body = "", FileCode = null, LanguageCode = fetchPostCategory.LanguageCode, LogTime = DateTime.Now, MemberID = memberObj.MemberID, PostCategoryID = fetchPostCategory.PostCategoryID, MetaDescription = "", MetaKeywords = "", Tags = "", NormalBody = false, VisitCount = 0 });

            db.TB_Posts.Add(new() { Title = "QR Code", Summary = "QR Code", PostCode = "QRCode", Active = true, Body = "", FileCode = null, LanguageCode = fetchPostCategory.LanguageCode, LogTime = DateTime.Now, MemberID = memberObj.MemberID, PostCategoryID = fetchPostCategory.PostCategoryID, MetaDescription = "", MetaKeywords = "", Tags = "", NormalBody = false, VisitCount = 0 });

            await db.SaveChangesAsync();

            return new() { SuccessAction = true };
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = $"{x.Message} -|- {x.InnerException?.Message}" } };
        }
    }


    //update db
    //await db.Database.MigrateAsync();
}
