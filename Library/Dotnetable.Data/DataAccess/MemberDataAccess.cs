using Dotnetable.Data.DBContext;
using Dotnetable.Shared.DTO.Member;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Data.DataAccess;
public class MemberDataAccess
{


    #region Insert

    public static async Task<PublicActionResponse> Register(MemberInsertRequest requestModel)
    {
        using DotnetableEntity db = new();

        var fetchUser = await (from i in db.TB_Members where i.Username == requestModel.Username || i.Email == requestModel.Email || (i.CountryCode == requestModel.CountryCode && i.CellphoneNumber == requestModel.CellphoneNumber) select new { i.Username, i.Email, i.CellphoneNumber }).FirstOrDefaultAsync();

        if (fetchUser != null)
        {
            if (requestModel.Username == fetchUser.Username)
                return new() { ErrorException = new() { ErrorCode = "C4" } };

            if (requestModel.Email == fetchUser.Email)
                return new() { ErrorException = new() { ErrorCode = "C5" } };

            if (requestModel.CellphoneNumber == fetchUser.CellphoneNumber)
                return new() { ErrorException = new() { ErrorCode = "C6" } };
        }

        var memberObject = new TB_Member()
        {
            Activate = requestModel.ActivateMember ?? false,
            Active = true,
            CellphoneNumber = requestModel.CellphoneNumber,
            Gender = requestModel.Gender,
            HashKey = Guid.NewGuid(),
            CountryCode = requestModel.CountryCode,
            Email = requestModel.Email,
            Givenname = requestModel.GivenName,
            Password = requestModel.Password,
            Username = requestModel.Username.ToLower(),
            RegisterDate = DateTime.Now,
            Surname = requestModel.Surname,
            PostalCode = requestModel.PostalCode,
            PolicyID = 2,
            CityID = requestModel.CityID
        };
        db.TB_Members.Add(memberObject);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new()
        {
            SuccessAction = true,
            ObjectID = memberObject.MemberID.ToString()
        };
    }

    public static async Task<PublicActionResponse> ContactInsert(MemberContactRequest requestModel)
    {
        using DotnetableEntity db = new();
        int memberID = requestModel.CurrentMemberID ?? 0;
        var fetchRegisteredContacts = await (from i in db.TB_Member_Contacts where i.MemberID == memberID && i.Active select new { i.Address, i.CellphoneNumber, i.PhoneNumber }).ToListAsync();
        if (fetchRegisteredContacts.Count >= 10)
            return new() { ErrorException = new() { ErrorCode = "D3" } };


        if (fetchRegisteredContacts.Any(i => (!string.IsNullOrEmpty(requestModel.Address) && i.Address == requestModel.Address) || (!string.IsNullOrEmpty(requestModel.CellphoneNumber) && i.CellphoneNumber == requestModel.CellphoneNumber) || (!string.IsNullOrEmpty(requestModel.PhoneNumber) && i.PhoneNumber == requestModel.PhoneNumber)) || (string.IsNullOrEmpty(requestModel.Address) && string.IsNullOrEmpty(requestModel.CellphoneNumber) && string.IsNullOrEmpty(requestModel.PhoneNumber)))
            return new() { ErrorException = new() { ErrorCode = "C7" } };


        var memberContactObject = new TB_Member_Contact()
        {
            Address = requestModel.Address,
            CellphoneNumber = requestModel.CellphoneNumber,
            LanguageCode = requestModel.LanguageCode ?? "FA",
            MemberID = requestModel.CurrentMemberID.Value,
            HomeOwnerName = requestModel.HomeOwnerName,
            PhoneNumber = requestModel.PhoneNumber,
            CityID = requestModel.CityID,
            PointLatitude = requestModel.PointLatitude,
            PointLongitude = requestModel.PointLongitude,
            Active = true,
            DefaultContact = requestModel.DefaultContact,
            PostalCode = requestModel.PostalCode
        };
        db.TB_Member_Contacts.Add(memberContactObject);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        if (requestModel.DefaultContact)
        {
            await db.TB_Member_Contacts.Where(i => i.MemberID == memberContactObject.MemberID && i.MemberContactID != memberContactObject.MemberContactID)
                 .ExecuteUpdateAsync(i => i.SetProperty(x => x.DefaultContact, x => false));

            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception x)
            {
                return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
            }

        }

        return new()
        {
            SuccessAction = true,
            ObjectID = memberContactObject.MemberContactID.ToString()
        };
    }

    public static async Task<PublicActionResponse> AvatarInsert(MemberAvatarInsertRequest requestModel)
    {
        using DotnetableEntity db = new();

        db.TBM_Member_Files.Add(new TBM_Member_File()
        {
            FileID = requestModel.FileID.Value,
            MemberID = requestModel.CurrentMemberID.Value
        });

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true };
    }

    public static async Task<PublicActionResponse> ActivateLinkInsert(int memberID)
    {
        using DotnetableEntity db = new();
        Guid activateCode = Guid.NewGuid();
        db.TB_Member_Activate_Logs.Add(new TB_Member_Activate_Log()
        {
            ActivateCode = activateCode,
            ExpireDate = DateTime.Now.AddDays(1),
            MemberID = memberID
        });

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true, ObjectID = activateCode.ToString() };
    }


    #endregion

    #region Update
    public static async Task<PublicActionResponse> ChangeSelfPassword(MemberChangePasswordRequest requestModel)
    {
        using DotnetableEntity db = new();
        int memberID = requestModel.CurrentMemberID.Value;
        var fetchMember = await (from i in db.TB_Members where i.MemberID == memberID select i).FirstOrDefaultAsync();
        if (fetchMember is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        if (fetchMember.Password != requestModel.OldPassword)
            return new() { ErrorException = new() { ErrorCode = "C8" } };

        fetchMember.Password = requestModel.NewPassword;
        try
        {
            db.Entry(fetchMember).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = ex.Message } };
        }

        return new() { SuccessAction = true };
    }

    public static async Task<PublicActionResponse> ChangeUserPassword(MemberChangePasswordAdminRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchMember = await (from i in db.TB_Members where i.MemberID == requestModel.MemberID select i).FirstOrDefaultAsync();
        if (fetchMember is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        fetchMember.Password = requestModel.NewPassword;
        try
        {
            db.Entry(fetchMember).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = ex.Message } };
        }

        return new()
        {
            SuccessAction = true,
            ObjectID = fetchMember.Email
        };
    }

    public static async Task<PublicActionResponse> Edit(MemberEditRequest requestModel)
    {
        using DotnetableEntity db = new();
        int memberID = requestModel.MemberID.Value;
        var fetchMember = await (from i in db.TB_Members where i.MemberID == memberID select i).FirstOrDefaultAsync();
        if (fetchMember is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        fetchMember.Username = requestModel.Username;
        fetchMember.Email = requestModel.Email;
        fetchMember.CellphoneNumber = requestModel.CellphoneNumber;
        fetchMember.CountryCode = requestModel.CountryCode;
        fetchMember.Givenname = requestModel.Givenname;
        fetchMember.Surname = requestModel.Surname;
        if (requestModel.CityID.HasValue) fetchMember.CityID = requestModel.CityID.Value;
        fetchMember.Gender = requestModel.Gender;
        fetchMember.PostalCode = requestModel.PostalCode;
        if (requestModel.CurrentMemberID.HasValue && requestModel.PolicyID.HasValue && requestModel.PolicyID != fetchMember.PolicyID) fetchMember.PolicyID = requestModel.PolicyID.Value;

        try
        {
            db.Entry(fetchMember).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = ex.Message } };
        }

        return new() { SuccessAction = true, ObjectID = fetchMember.MemberID.ToString() };
    }

    public static async Task<PublicActionResponse> ActivateAdmin(MemberActivateAdminRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchMember = await (from i in db.TB_Members where i.MemberID == requestModel.MemberID && i.Active && !i.Activate select i).FirstOrDefaultAsync();
        if (fetchMember is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        fetchMember.Activate = true;

        try
        {
            db.Entry(fetchMember).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true };
    }

    public static async Task<PublicActionResponse> ContactUpdate(MemberContactRequest requestModel)
    {
        using DotnetableEntity db = new();
        int memberID = requestModel.CurrentMemberID.Value;
        var fetchMemberContact = await (from i in db.TB_Member_Contacts where i.MemberID == memberID && i.MemberContactID == requestModel.MemberContactID select i).FirstOrDefaultAsync();
        if (fetchMemberContact is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        var FetchRegisteredContacts = await (from i in db.TB_Member_Contacts where i.MemberID == memberID && i.Active & i.MemberContactID != requestModel.MemberContactID select new { i.Address, i.CellphoneNumber, i.PhoneNumber, i.MemberContactID }).ToListAsync();
        if (FetchRegisteredContacts.Any(i => (!string.IsNullOrEmpty(fetchMemberContact.Address) && i.Address == fetchMemberContact.Address && i.MemberContactID != fetchMemberContact.MemberContactID) || (!string.IsNullOrEmpty(fetchMemberContact.CellphoneNumber) && i.CellphoneNumber == fetchMemberContact.CellphoneNumber && i.MemberContactID != fetchMemberContact.MemberContactID) || (!string.IsNullOrEmpty(fetchMemberContact.PhoneNumber) && i.PhoneNumber == fetchMemberContact.PhoneNumber && i.MemberContactID != fetchMemberContact.MemberContactID)) || (string.IsNullOrEmpty(fetchMemberContact.Address) && string.IsNullOrEmpty(fetchMemberContact.CellphoneNumber) && string.IsNullOrEmpty(fetchMemberContact.PhoneNumber)))
            return new() { ErrorException = new() { ErrorCode = "C7" } };

        fetchMemberContact.Address = requestModel.Address;
        fetchMemberContact.CellphoneNumber = requestModel.CellphoneNumber;
        fetchMemberContact.HomeOwnerName = requestModel.HomeOwnerName;
        fetchMemberContact.PhoneNumber = requestModel.PhoneNumber;
        fetchMemberContact.PointLatitude = requestModel.PointLatitude;
        fetchMemberContact.PointLongitude = requestModel.PointLongitude;
        fetchMemberContact.LanguageCode = requestModel.LanguageCode;
        fetchMemberContact.CityID = requestModel.CityID;
        fetchMemberContact.DefaultContact = requestModel.DefaultContact;
        fetchMemberContact.PostalCode = requestModel.PostalCode;

        if (requestModel.DefaultContact)
            await db.TB_Member_Contacts.Where(i => i.MemberID == requestModel.CurrentMemberID && i.MemberContactID != requestModel.MemberContactID)
                 .ExecuteUpdateAsync(i => i.SetProperty(x => x.DefaultContact, x => false));

        try
        {
            db.Entry(fetchMemberContact).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = ex.Message } };
        }

        return new() { SuccessAction = true, ObjectID = fetchMemberContact.MemberContactID.ToString() };
    }

    public static async Task<PublicActionResponse> AvatarDefault(MemberAvatarDefaultRequest requestModel)
    {
        using DotnetableEntity db = new();
        int memberID = requestModel.CurrentMemberID.Value;
        var fetchFile = await (from i in db.TB_Files where i.FileCode == requestModel.FileCode select new { i.FileID }).FirstOrDefaultAsync();
        if (fetchFile is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        var memberDetail = await (from i in db.TB_Members where i.MemberID == memberID select i).FirstOrDefaultAsync();
        if (memberDetail is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        memberDetail.AvatarID = requestModel.FileCode;
        try
        {
            db.Entry(memberDetail).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = ex.Message } };
        }

        return new() { SuccessAction = true };
    }

    public static async Task<PublicActionResponse> ActivateLinkCheck(Guid activateCode)
    {
        using DotnetableEntity db = new();
        var fetchActivate = await (from i in db.TB_Member_Activate_Logs where i.ActivateCode == activateCode select new { i.MemberID, i.ExpireDate }).FirstOrDefaultAsync();
        if (fetchActivate is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        if (fetchActivate.ExpireDate < DateTime.Now)
            return new() { ErrorException = new() { ErrorCode = "C9" } };

        var fetchMember = await (from i in db.TB_Members where i.MemberID == fetchActivate.MemberID select i).FirstOrDefaultAsync();
        fetchMember.Activate = true;

        try
        {
            db.Entry(fetchMember).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true };
    }


    #endregion

    #region Delete
    public static async Task<PublicActionResponse> ChangeStatus(MemberChangeStatusRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchMember = await (from i in db.TB_Members where i.MemberID == requestModel.MemberID select i).FirstOrDefaultAsync();
        if (fetchMember is null) return new() { ErrorException = new() { ErrorCode = "D0" } };

        fetchMember.Active = !fetchMember.Active;
        try
        {
            db.Entry(fetchMember).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = ex.Message } };
        }

        return new() { SuccessAction = true };
    }

    public static async Task<PublicActionResponse> ContactDelete(MemberContactDeleteRequest deleteRequest)
    {
        using DotnetableEntity db = new();
        int memberID = deleteRequest.CurrentMemberID.Value;
        var fetchMemberContact = await (from i in db.TB_Member_Contacts where i.MemberID == memberID && i.Active && i.MemberContactID == deleteRequest.MemberContactID select i).FirstOrDefaultAsync();
        if (fetchMemberContact is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        fetchMemberContact.Active = false;
        try
        {
            db.Entry(fetchMemberContact).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = ex.Message } };
        }

        return new() { SuccessAction = true };
    }

    public static async Task<MemberAvatarDeleteResponse> AvatarDelete(MemberAvatarDeleteRequest deleteRequest)
    {
        using DotnetableEntity db = new();
        int memberID = deleteRequest.CurrentMemberID.Value;
        var fetchFile = await (from i in db.TB_Files where i.FileCode == deleteRequest.AvatarCode select i).FirstOrDefaultAsync();
        if (fetchFile is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        MemberAvatarDeleteResponse responseObject = new() { SuccessAction = true, FileCategoryID = fetchFile.FileCategoryID, FilePath = fetchFile.FilePath };

        var fetchMemberFile = await (from i in db.TBM_Member_Files where i.MemberID == memberID && i.FileID == fetchFile.FileID select i).FirstOrDefaultAsync();
        if (fetchMemberFile is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        var fetchMember = await (from i in db.TB_Members where i.MemberID == memberID select i).FirstOrDefaultAsync();
        if (fetchMember.AvatarID == deleteRequest.AvatarCode) fetchMember.AvatarID = null;

        db.TBM_Member_Files.Remove(fetchMemberFile);
        try
        {
            db.Entry(fetchMember).State = EntityState.Modified;
            db.Entry(fetchMemberFile).State = EntityState.Deleted;
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = ex.Message } };
        }

        db.TB_Files.Remove(fetchFile);
        try
        {
            db.Entry(fetchFile).State = EntityState.Deleted;
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = ex.Message } };
        }

        return responseObject;
    }


    #endregion

    #region Read

    public static async Task<MemberListFinalResponse> MemberList(MemberListRequest requestModel)
    {
        using DotnetableEntity db = new();

        var reportQuery = db.TB_Members.AsQueryable();

        if (!string.IsNullOrEmpty(requestModel.Username) && requestModel.Username != "")
            reportQuery = reportQuery.Where(i => i.Username.Contains(requestModel.Username));

        if (!string.IsNullOrEmpty(requestModel.Email) && requestModel.Email != "")
            reportQuery = reportQuery.Where(i => i.Email.Contains(requestModel.Email));

        if (!string.IsNullOrEmpty(requestModel.CellphoneNumber) && requestModel.CellphoneNumber != "")
            reportQuery = reportQuery.Where(i => i.CellphoneNumber.Contains(requestModel.CellphoneNumber));

        if (!string.IsNullOrEmpty(requestModel.Givenname) && requestModel.Givenname != "")
            reportQuery = reportQuery.Where(i => i.Givenname.Contains(requestModel.Givenname));

        if (!string.IsNullOrEmpty(requestModel.Surname) && requestModel.Surname != "")
            reportQuery = reportQuery.Where(i => i.Surname.Contains(requestModel.Surname));

        int dbCount = await reportQuery.CountAsync();


        if (string.IsNullOrEmpty(requestModel.OrderbyParams) || requestModel.OrderbyParams == "")
            reportQuery = reportQuery.OrderBy(i => i.MemberID);
        else
            reportQuery = reportQuery.OrderUsingSortExpression(requestModel.OrderbyParams);


        var finalQuery = reportQuery.Join(db.TB_Cities, m => m.CityID, c => c.CityID, (m, c) => new { m.MemberID, m.Username, m.Email, m.CellphoneNumber, m.CountryCode, m.Givenname, m.Surname, m.CityID, m.Gender, m.Active, m.Activate, m.RegisterDate, m.PostalCode, m.PolicyID, CityName = c.Title, c.CountryID });

        var fetchList = await finalQuery.Skip(requestModel.SkipCount).Take(requestModel.TakeCount).Select(i => new MemberListFinalResponse.MemberDetail { MemberID = i.MemberID, Surname = i.Surname, Givenname = i.Givenname, CellphoneNumber = i.CellphoneNumber, Email = i.Email, Activate = i.Activate, Active = i.Active, CityID = i.CityID, CityName = i.CityName, CountryCode = i.CountryCode, CountryID = i.CountryID, Gender = i.Gender, PolicyID = i.PolicyID, PostalCode = i.PostalCode, RegisterDate = i.RegisterDate, Username = i.Username }).ToListAsync();

        return new() { Members = fetchList, DatabaseRecords = dbCount };
    }

    public static async Task<int> FetchMemberIDByHashKey(Guid userHashKey)
    {
        using DotnetableEntity db = new();
        return await (from i in db.TB_Members where i.HashKey == userHashKey select i.MemberID).FirstOrDefaultAsync();
    }

    public static async Task<MemberDetailResponse> MemberDetail(MemberDetailRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchMember = await (from i in db.TB_Members join j in db.TB_Cities on i.CityID equals j.CityID where i.MemberID == requestModel.CurrentMemberID && i.Active select new MemberDetailResponse { Email = i.Email, CellphoneNumber = i.CellphoneNumber, RegisterDate = i.RegisterDate, Givenname = i.Givenname, Surname = i.Surname, MemberID = i.MemberID, CountryCode = i.CountryCode, Username = i.Username, CityID = i.CityID, Gender = i.Gender, AvatarID = i.AvatarID, PostalCode = i.PostalCode, CityName = j.Title, CountryID = j.CountryID, Addresses = new List<MemberContactRequest>() }).FirstOrDefaultAsync();

        if (fetchMember is null) return new() { ErrorException = new() { ErrorCode = "D0" } };

        var fetchMemberAddresses = await (from i in db.TB_Member_Contacts
                                          join j in db.TB_Cities on i.CityID equals j.CityID
                                          where i.MemberID == requestModel.CurrentMemberID && i.Active
                                          select new MemberContactRequest { Address = i.Address, CellphoneNumber = i.CellphoneNumber, HomeOwnerName = i.HomeOwnerName, LanguageCode = i.LanguageCode, MemberContactID = i.MemberContactID, CityID = i.CityID, PhoneNumber = i.PhoneNumber, PointLatitude = i.PointLatitude, PointLongitude = i.PointLongitude, PostalCode = i.PostalCode, DefaultContact = i.DefaultContact, CurrentMemberID = i.MemberID, CityName = j.Title, CountryID = j.CountryID }).ToListAsync();
        if (fetchMemberAddresses != null)
            fetchMember.Addresses = fetchMemberAddresses;

        return fetchMember;
    }

    public static async Task<MemberContactListResponse> ContactList(MemberContactListRequest listRequest)
    {
        using DotnetableEntity db = new();
        var contacts = await (from m in db.TB_Members
                              join mc in db.TB_Member_Contacts on m.MemberID equals mc.MemberID
                              join c in db.TB_Cities on m.CityID equals c.CityID
                              where m.Active && mc.Active && m.MemberID == listRequest.CurrentMemberID
                              select new MemberContactListResponse.ContactDetail { MemberContactID = mc.MemberContactID, MemberID = m.MemberID, Address = mc.Address, CellphoneNumber = mc.CellphoneNumber, HomeOwnerName = mc.HomeOwnerName, LanguageCode = mc.LanguageCode, PhoneNumber = mc.PhoneNumber, CityID = mc.CityID, CityName = c.Title, CountryID = c.CountryID, PointLatitude = mc.PointLatitude, PointLongitude = mc.PointLongitude, PostalCode = mc.PostalCode }).ToListAsync();

        return new() { Contacts = contacts };
    }

    public static async Task<MemberAvatarListResponse> AvatarList(MemberAvatarListRequest requestModel)
    {
        using DotnetableEntity db = new();

        var reportQuery = db.TBM_Member_Files.Join(db.TB_Files, mf => mf.FileID, f => f.FileID, (mf, f) => new { mf.MemberID, f.FileCode, f.FileName, f.FileID, f.FilePath, f.FileCategoryID }).Where(i => i.MemberID == requestModel.CurrentMemberID).AsQueryable();


        int dbCount = await reportQuery.CountAsync();


        if (string.IsNullOrEmpty(requestModel.OrderbyParams) || requestModel.OrderbyParams == "")
            reportQuery = reportQuery.OrderBy(i => i.MemberID);
        else
            reportQuery = reportQuery.OrderUsingSortExpression(requestModel.OrderbyParams);


        var fetchList = await reportQuery.Skip(requestModel.SkipCount).Take(requestModel.TakeCount).Select(i => new MemberAvatarListResponse.AvatarDetail { FileCategoryID = i.FileCategoryID, FileID = i.FileID, FileCode = i.FileCode, FileName = i.FileName, FilePath = i.FilePath }).ToListAsync();

        return new() { Avatars = fetchList, DatabaseRecords = dbCount };
    }


    #endregion

    #region RecoveryPassword

    public static async Task<PublicActionResponse> GetRecoveryPasswordCode(MemberForgetPasswordRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchMember = await (from i in db.TB_Members where i.Active && i.Activate && i.Username == requestModel.Username select new { i.Email, i.MemberID }).ToListAsync();
        if (fetchMember is null) return new() { ErrorException = new() { ErrorCode = "D0" } };
        if (fetchMember.Count > 1) return new() { ErrorException = new() { ErrorCode = "D2" } };
        db.TB_Member_Forget_Passwords.Add(new()
        {
            ForgetKey = requestModel.ForgetKey,
            LogTime = DateTime.Now,
            MemberID = fetchMember[0].MemberID
        });

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true, ObjectID = fetchMember[0].Email };
    }

    public static async Task<PublicActionResponse> SetRecoveryPasswordCode(MemberForgetPasswordSetRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchMember = await (from i in db.TB_Members where i.Active && i.Activate && i.Username == requestModel.Username select new { i.Email, i.MemberID }).ToListAsync();
        if (fetchMember is null) return new() { ErrorException = new() { ErrorCode = "D0" } };
        if (fetchMember.Count > 1) return new() { ErrorException = new() { ErrorCode = "D2" } };

        DateTime expireTime = DateTime.Now.AddMinutes(-5);
        var fetchForgetKey = await (from i in db.TB_Member_Forget_Passwords where i.MemberID == fetchMember[0].MemberID && i.ForgetKey == requestModel.ForgetKey && i.LogTime > expireTime select new { i.MemberID }).FirstOrDefaultAsync();
        if (fetchForgetKey is null) return new() { ErrorException = new() { ErrorCode = "D0" } };

        return new() { SuccessAction = true, ObjectID = fetchMember[0].MemberID.ToString() };
    }

    #endregion

    #region Subscribe

    public static async Task<PublicActionResponse> EmailSubscribeRegister(MemberEmailSubscribeRegisterRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchSubscribe = await (from i in db.TB_Email_Subscribes where i.Email == requestModel.EmailAddress && i.Active select new { i.EmailSubscribeID }).FirstOrDefaultAsync();
        if (fetchSubscribe is not null)
            return new() { SuccessAction = true, ObjectID = fetchSubscribe.EmailSubscribeID.ToString() };

        TB_Email_Subscribe subscribeObj = new()
        {
            Active = true,
            Email = requestModel.EmailAddress,
            MemberID = requestModel.CurrentMemberID,
            LogTime = DateTime.Now,
            Approved = false
        };
        db.TB_Email_Subscribes.Add(subscribeObj);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true, ObjectID = subscribeObj.EmailSubscribeID.ToString() };
    }

    public static async Task<PublicActionResponse> EmailSubscribeApprove(MemberEmailSubscribeApproveRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchSubscribe = await (from i in db.TB_Email_Subscribes where i.Email == requestModel.EmailAddress && i.Active select i).FirstOrDefaultAsync();
        if (fetchSubscribe is null) return new() { ErrorException = new() { ErrorCode = "D0" } };

        fetchSubscribe.Approved = true;

        try
        {
            db.Entry(fetchSubscribe).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true };
    }

    public static async Task<PublicActionResponse> CheckSubscribeEmailOnDB(string emailAddress)
    {
        using DotnetableEntity db = new();
        var fetchEmail = await (from i in db.TB_Email_Subscribes where i.Email == emailAddress && i.Active select new { i.EmailSubscribeID }).FirstOrDefaultAsync();
        if (fetchEmail is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };
        else
            return new() { SuccessAction = true, ObjectID = fetchEmail.EmailSubscribeID.ToString() };
    }

    public static async Task<PublicActionResponse> EmailSubscribeRemove(MemberEmailSubscribeRemoveRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchSubscribe = await (from i in db.TB_Email_Subscribes where i.Email == requestModel.EmailAddress select i).FirstOrDefaultAsync();
        if (fetchSubscribe is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        fetchSubscribe.Active = false;

        try
        {
            db.Entry(fetchSubscribe).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true };
    }


    public static async Task<SubscribeListResponse> MemberSubscribedList(SubscribeListRequest requestModel)
    {
        using DotnetableEntity db = new();

        var reportQuery = db.TB_Email_Subscribes.AsQueryable();

        if (!string.IsNullOrEmpty(requestModel.Email) && requestModel.Email != "")
            reportQuery = reportQuery.Where(i => i.Email.Contains(requestModel.Email));

        if (requestModel.MemberID.HasValue)
            reportQuery = reportQuery.Where(i => i.MemberID == requestModel.MemberID.Value);

        if (requestModel.Active.HasValue)
            reportQuery = reportQuery.Where(i => i.Active == requestModel.Active.Value);


        int dbCount = await reportQuery.CountAsync();


        if (string.IsNullOrEmpty(requestModel.OrderbyParams) || requestModel.OrderbyParams == "")
            reportQuery = reportQuery.OrderBy(i => i.MemberID);
        else
            reportQuery = reportQuery.OrderUsingSortExpression(requestModel.OrderbyParams);


        var fetchList = await reportQuery.Skip(requestModel.SkipCount).Take(requestModel.TakeCount).Select(i => new SubscribeListResponse.SubscribeDetail { MemberID = i.MemberID, Email = i.Email, Active = i.Active, EmailSubscribeID = i.EmailSubscribeID, LogTime = i.LogTime }).ToListAsync();

        return new() { SubscribedList = fetchList, DatabaseRecords = dbCount };
    }

    public static async Task<PublicActionResponse> MemberSubscribedChangeStatus(SubscribedChangeStatusRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchSubscribe = await (from i in db.TB_Email_Subscribes where i.EmailSubscribeID == requestModel.EmailSubscribeID select i).FirstOrDefaultAsync();
        if (fetchSubscribe is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        fetchSubscribe.Active = !fetchSubscribe.Active;

        try
        {
            db.Entry(fetchSubscribe).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true };
    }


    #endregion

    #region Policy & Role
    public static async Task<RoleListResponse> RoleList(RoleListRequest requestModel)
    {
        using DotnetableEntity db = new();

        var roleList = await (from m in db.TB_Members
                              join pr in db.TBM_Policy_Roles on m.PolicyID equals pr.PolicyID
                              join r in db.TB_Roles on pr.RoleID equals r.RoleID
                              where r.Active && pr.Active && m.Active && m.MemberID == requestModel.CurrentMemberID
                              select new RoleListResponse.RoleDetail { RoleID = r.RoleID, RoleKey = r.RoleKey, Description = r.Description }).ToListAsync();
        return new() { Roles = roleList };
    }

    public static async Task<List<PolicyListOnInsertMemberResponse>> PolicyListOnMemberManage(int memberID)
    {
        using DotnetableEntity db = new();

        var fetchCurrentMemberRoles = (from m in db.TB_Members
                                       join r in db.TBM_Policy_Roles on m.PolicyID equals r.PolicyID
                                       where m.MemberID == memberID && r.Active
                                       select new { r.RoleID });

        var allPolicies = await (from pr in db.TBM_Policy_Roles
                                 join p in db.TB_Policies on pr.PolicyID equals p.PolicyID
                                 join m in fetchCurrentMemberRoles on pr.RoleID equals m.RoleID into ps
                                 from m in ps.DefaultIfEmpty()
                                 where p.Active && pr.Active
                                 select new { p.PolicyID, p.Title, ExistsMember = m != null }).Distinct().ToListAsync();

        allPolicies = allPolicies.Distinct().ToList();

        var fetchNotAcceptedItems = (from i in allPolicies where !i.ExistsMember select i.PolicyID).ToList();
        return (from i in allPolicies where i.ExistsMember && !fetchNotAcceptedItems.Any(j => j == i.PolicyID) select new PolicyListOnInsertMemberResponse { PolicyID = i.PolicyID, Title = i.Title }).Distinct().ToList();
    }

    public static async Task<List<RoleListOnPolicyManageResponse>> RoleListOnPolicyManage(int memberID)
    {
        using DotnetableEntity db = new();

        return await (from p in db.TB_Roles
                      join prs in db.TBM_Policy_Roles on p.RoleID equals prs.RoleID
                      join m in db.TB_Members on prs.PolicyID equals m.PolicyID
                      where m.MemberID == memberID && m.Active && p.Active && prs.Active
                      select new RoleListOnPolicyManageResponse { RoleID = p.RoleID, RoleKey = p.RoleKey }).Distinct().ToListAsync();
    }

    public static async Task<PolicyListResponse> PolicyList(PolicyListRequest requestModel)
    {
        using DotnetableEntity db = new();

        var reportQuery = db.TB_Policies.AsQueryable();

        if (!string.IsNullOrEmpty(requestModel.Title) && requestModel.Title != "")
            reportQuery = reportQuery.Where(i => i.Title.Contains(requestModel.Title));


        int dbCount = await reportQuery.CountAsync();


        if (string.IsNullOrEmpty(requestModel.OrderbyParams) || requestModel.OrderbyParams == "")
            reportQuery = reportQuery.OrderBy(i => i.PolicyID);
        else
            reportQuery = reportQuery.OrderUsingSortExpression(requestModel.OrderbyParams);

        var fetchList = await reportQuery.Skip(requestModel.SkipCount).Take(requestModel.TakeCount).Select(i => new PolicyListResponse.PolicyDetail { Active = i.Active, PolicyID = i.PolicyID, Title = i.Title }).ToListAsync();

        return new() { Policies = fetchList, DatabaseRecords = dbCount };
    }

    public static async Task<PolicyRoleListResponse> PolicyRolesList(PolicyRoleListRequest requestModel)
    {
        using DotnetableEntity db = new();

        var reportQuery = db.TBM_Policy_Roles.Join(db.TB_Roles, pr => pr.RoleID, r => r.RoleID, (pr, r) => new { PolicyRoleActive = pr.Active, r.RoleID, r.RoleKey, pr.PolicyRoleID, RoleActive = r.Active, pr.PolicyID }).Where(i => i.PolicyID == requestModel.PolicyID).AsQueryable();

        if (requestModel.ActiveRelation.HasValue)
            reportQuery = reportQuery.Where(i => i.RoleActive == requestModel.ActiveRelation.Value);

        int dbCount = await reportQuery.CountAsync();

        if (string.IsNullOrEmpty(requestModel.OrderbyParams) || requestModel.OrderbyParams == "")
            reportQuery = reportQuery.OrderBy(i => i.PolicyRoleID);
        else
            reportQuery = reportQuery.OrderUsingSortExpression(requestModel.OrderbyParams);

        var fetchList = await reportQuery.Skip(requestModel.SkipCount).Take(requestModel.TakeCount).Select(i => new PolicyRoleListResponse.RoleDetail { PolicyRoleID = i.PolicyRoleID, RoleID = i.RoleID, RoleKey = i.RoleKey }).ToListAsync();

        return new() { Roles = fetchList, DatabaseRecords = dbCount };
    }

    public static async Task<PublicActionResponse> PolicyInsert(PolicyInsertRequest requestModel)
    {
        using DotnetableEntity db = new();
        if (await (from i in db.TB_Policies where i.Title == requestModel.Title select i.PolicyID).AnyAsync())
            return new() { ErrorException = new() { ErrorCode = "D2" } };

        TB_Policy policyObj = new()
        {
            Active = true,
            Title = requestModel.Title
        };
        db.TB_Policies.Add(policyObj);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true, ObjectID = policyObj.PolicyID.ToString() };
    }

    public static async Task<PolicyDetailResponse> PolicyDetail(PolicyDetailRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchPolicyDetail = await (from i in db.TB_Policies where i.PolicyID == requestModel.PolicyID select new PolicyDetailResponse { Active = i.Active, PolicyID = i.PolicyID, Title = i.Title }).FirstOrDefaultAsync();
        if (fetchPolicyDetail is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        return fetchPolicyDetail;
    }

    public static async Task<PublicActionResponse> PolicyUpdate(PolicyUpdateRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchPolicy = await (from i in db.TB_Policies where i.PolicyID == requestModel.PolicyID select i).FirstOrDefaultAsync();
        if (fetchPolicy is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        if (fetchPolicy.Title == requestModel.Title)
            return new() { SuccessAction = true };

        if (await (from i in db.TB_Policies where i.Title == requestModel.Title select i.PolicyID).AnyAsync())
            return new() { ErrorException = new() { ErrorCode = "D2" } };

        fetchPolicy.Title = requestModel.Title;

        try
        {
            db.Entry(fetchPolicy).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true };
    }

    public static async Task<PublicActionResponse> PolicyChangeStatus(PolicyChangeStatusRequest requestModel)
    {
        using DotnetableEntity db = new();
        int memberID = requestModel.CurrentMemberID.Value;
        if (await (from i in db.TB_Members where i.MemberID == memberID && i.PolicyID == requestModel.PolicyID select i.MemberID).AnyAsync())
            return new() { ErrorException = new() { ErrorCode = "C18" } };

        var fetchPolicy = await (from i in db.TB_Policies where i.PolicyID == requestModel.PolicyID select i).FirstOrDefaultAsync();
        if (fetchPolicy is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        fetchPolicy.Active = !fetchPolicy.Active;

        try
        {
            db.Entry(fetchPolicy).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true };
    }

    public static async Task<PublicActionResponse> PolicyRoleRemove(PolicyRoleRemoveRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchPolicyRole = await (from i in db.TBM_Policy_Roles where i.PolicyRoleID == requestModel.PolicyRoleID select i).FirstOrDefaultAsync();
        if (fetchPolicyRole is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        int memberID = requestModel.CurrentMemberID.Value;
        if (await (from i in db.TB_Members where i.MemberID == memberID && i.PolicyID == fetchPolicyRole.PolicyID select i.MemberID).AnyAsync())
            return new() { ErrorException = new() { ErrorCode = "C18" } };


        db.TBM_Policy_Roles.Remove(fetchPolicyRole);

        try
        {
            db.Entry(fetchPolicyRole).State = EntityState.Deleted;
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true };
    }

    public static async Task<PublicActionResponse> MemberRoleAppend(PolicyRoleAppendRequest requestModel)
    {
        using DotnetableEntity db = new();
        int memberID = requestModel.CurrentMemberID.Value;
        var fetchMemberPolicy = await (from i in db.TB_Members where i.MemberID == memberID select new { i.PolicyID }).FirstOrDefaultAsync();
        if (fetchMemberPolicy is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        if (requestModel.RoleID is null)
        {
            var FetchRoleID = await (from i in db.TB_Roles where i.RoleKey == requestModel.RoleKey select new { i.RoleID }).FirstOrDefaultAsync();
            if (FetchRoleID is null)
                return new() { ErrorException = new() { ErrorCode = "D0" } };

            requestModel.RoleID = FetchRoleID.RoleID;
        }

        short RoleID = requestModel.RoleID.Value;

        if (fetchMemberPolicy.PolicyID == requestModel.PolicyID)
            return new() { ErrorException = new() { ErrorCode = "C18" } };

        if (!await (from i in db.TBM_Policy_Roles where i.PolicyID == fetchMemberPolicy.PolicyID && i.RoleID == RoleID select i.PolicyRoleID).AnyAsync())
            return new() { ErrorException = new() { ErrorCode = "C19" } };

        if (await (from i in db.TBM_Policy_Roles where i.PolicyID == requestModel.PolicyID && i.RoleID == RoleID select i.PolicyRoleID).AnyAsync())
            return new() { ErrorException = new() { ErrorCode = "D2" } };

        TBM_Policy_Role policyRoleObj = new()
        {
            Active = true,
            PolicyID = requestModel.PolicyID,
            RoleID = RoleID
        };
        db.TBM_Policy_Roles.Add(policyRoleObj);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true, ObjectID = policyRoleObj.PolicyRoleID.ToString() };
    }

    #endregion

}