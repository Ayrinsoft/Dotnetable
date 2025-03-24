using Dotnetable.Data.DBContext;
using Dotnetable.Shared.DTO.Message;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Data.DataAccess;
public class MessageDataAccess
{

    public static async Task<EmailSettingResponse> EmailSetting(EmailType emailType)
    {
        using DotnetableEntity db = new();
        byte emailTypeID = (byte)emailType;
        var fetchEmailSetting = await (from i in db.TB_Email_Settings where i.EmailTypeID == emailTypeID select new EmailSettingResponse { EmailAddress = i.EmailAddress, EnableSSL = i.EnableSSL, MailName = i.MailName, MailServer = i.MailServer, Password = i.Password, PortNumber = i.SMTPPort }).FirstOrDefaultAsync();

        if (fetchEmailSetting is null)
        {
            emailTypeID = (byte)EmailType.PUBLIC;
            fetchEmailSetting = await (from i in db.TB_Email_Settings where i.EmailTypeID == emailTypeID select new EmailSettingResponse { EmailAddress = i.EmailAddress, EnableSSL = i.EnableSSL, MailName = i.MailName, MailServer = i.MailServer, Password = i.Password, PortNumber = i.SMTPPort }).FirstOrDefaultAsync();
        }

        return fetchEmailSetting ?? new();
    }

    public static async Task<PublicActionResponse> EmailSettingInsert(EmailPanelInsertRequest requestModel)
    {
        using DotnetableEntity db = new();
        if (await (from i in db.TB_Email_Settings where i.EmailAddress.ToLower() == requestModel.EmailAddress.ToLower() select i).AnyAsync())
            return new() { ErrorException = new() { ErrorCode = "D2" } };

        if (requestModel.DefaultEmail)
        {
            await db.TB_Email_Settings.ExecuteUpdateAsync(i => i.SetProperty(x => x.DefaultEMail, x => false));
            try
            {
                db.Entry(db.TB_Email_Settings).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            catch (Exception) { }
        }

        TB_Email_Setting emailSettingObject = new()
        {
            DefaultEMail = requestModel.DefaultEmail,
            EmailAddress = requestModel.EmailAddress,
            EmailTypeID = (byte)requestModel.EMailType,
            EnableSSL = requestModel.EnableSSL,
            MailName = requestModel.EmailName,
            MailServer = requestModel.MailServer,
            Password = requestModel.EmailPassword,
            SMTPPort = requestModel.SMTPPort,
            Active = true
        };
        db.TB_Email_Settings.Add(emailSettingObject);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true, ObjectID = emailSettingObject.EmailSettingID.ToString() };
    }

    public static async Task<PublicActionResponse> EmailSettingUpdate(EmailPanelUpdateRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchPanel = await (from i in db.TB_Email_Settings where i.EmailSettingID == requestModel.EmailSettingID select i).FirstOrDefaultAsync();
        if (fetchPanel is null) return new() { ErrorException = new() { ErrorCode = "D0" } };

        if (!fetchPanel.DefaultEMail && requestModel.DefaultEmail.HasValue && requestModel.DefaultEmail.Value)
        {
            await db.TB_Email_Settings.ExecuteUpdateAsync(i => i.SetProperty(x => x.DefaultEMail, x => false));
            try
            {
                db.Entry(db.TB_Email_Settings).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            catch (Exception x)
            {
                return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
            }

            fetchPanel.DefaultEMail = true;
        }


        if (requestModel.EnableSSL.HasValue) fetchPanel.EnableSSL = requestModel.EnableSSL.Value;
        if (requestModel.SMTPPort.HasValue) fetchPanel.SMTPPort = requestModel.SMTPPort.Value;
        if (requestModel.EMailType.HasValue) fetchPanel.EmailTypeID = (byte)requestModel.EMailType.Value;
        if (!string.IsNullOrEmpty(requestModel.EmailName) && requestModel.EmailName != "") fetchPanel.MailName = requestModel.EmailName;
        if (!string.IsNullOrEmpty(requestModel.EmailPassword) && requestModel.EmailPassword != "") fetchPanel.Password = requestModel.EmailPassword;
        if (!string.IsNullOrEmpty(requestModel.EmailAddress) && requestModel.EmailAddress != "") fetchPanel.EmailAddress = requestModel.EmailAddress;
        if (!string.IsNullOrEmpty(requestModel.MailServer) && requestModel.MailServer != "") fetchPanel.MailServer = requestModel.MailServer;

        db.Entry(fetchPanel).State = EntityState.Modified;

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

    public static async Task<PublicActionResponse> EmailSettingChangeStatus(EmailPanelChangeStatusRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchPanel = await (from i in db.TB_Email_Settings where i.EmailSettingID == requestModel.EmailSettingID select i).FirstOrDefaultAsync();
        if (fetchPanel is null) return new() { ErrorException = new() { ErrorCode = "D0" } };

        fetchPanel.Active = !fetchPanel.Active;

        db.Entry(fetchPanel).State = EntityState.Modified;

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

    public static async Task<EmailPanelListResponse> EmailSettingList(EmailPanelListRequest requestModel)
    {
        using DotnetableEntity db = new();
        var dbCount = await (from i in db.TB_Email_Settings where requestModel.EmailName == "" || i.MailName == requestModel.EmailName select i).CountAsync();
        if (dbCount == 0)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        var fetchSetitngs = await (from i in db.TB_Email_Settings where requestModel.EmailName == "" || i.MailName == requestModel.EmailName orderby i.EmailSettingID select new EmailPanelListResponse.EmailSettingDetail { EmailAddress = i.EmailAddress, EmailSettingID = i.EmailSettingID, EMailType = i.EmailTypeID, EmailName = i.MailName, MailServer = i.MailServer, SMTPPort = i.SMTPPort, Active = i.Active, DefaultEmail = i.DefaultEMail, EmailPassword = i.Password, EnableSSL = i.EnableSSL }).Skip(requestModel.SkipCount).Take(requestModel.TakeCount).ToListAsync();
        return new() { DatabaseRecords = dbCount, EmailSettings = fetchSetitngs };
    }


    public static async Task<PublicActionResponse> ContactUsMessageInsert(MessageBoxOnContactUsRequest requestModel)
    {
        using DotnetableEntity db = new();
        DateTime checkTime = DateTime.Now.AddMinutes(-5);
        if (await (from i in db.TB_ContactUs_Messages where i.SenderIPAddress == requestModel.IPAddress && i.LogTime > checkTime select i).AnyAsync())
            return new() { ErrorException = new() { ErrorCode = "D2", Message = "Duplicate send message" } };

        TB_ContactUs_Message contactMsgObj = new()
        {
            SenderName = requestModel.SenderName,
            Archive = false,
            LogTime = DateTime.Now,
            CellphoneNumber = requestModel.CellphoneNumber,
            EmailAddress = requestModel.EmailAddress,
            MessageBody = requestModel.MessageBody,
            MessageSubject = requestModel.MessageSubject,
            SenderIPAddress = requestModel.IPAddress
        };

        db.TB_ContactUs_Messages.Add(contactMsgObj);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true, ObjectID = contactMsgObj.ContactUsMessagesID.ToString() };
    }

    public static async Task<MessageContactUsListResponse> MessageContactUsList(MessageContactUsListRequest requestModel)
    {
        using DotnetableEntity db = new();

        var contactusMsgs = db.TB_ContactUs_Messages.AsQueryable();

        if (requestModel.Archive is not null)
            contactusMsgs = contactusMsgs.Where(i => i.Archive == requestModel.Archive.Value);


        int dbCount = await contactusMsgs.CountAsync();

        if (string.IsNullOrEmpty(requestModel.OrderbyParams) || requestModel.OrderbyParams == "")
            contactusMsgs = contactusMsgs.OrderBy(i => i.ContactUsMessagesID);
        else
            contactusMsgs = contactusMsgs.OrderUsingSortExpression(requestModel.OrderbyParams);

        var fetchList = await contactusMsgs.Skip(requestModel.SkipCount).Take(requestModel.TakeCount).Select(i => new MessageContactUsListResponse.ContactMessage { Archive = i.Archive, CellphoneNumber = i.CellphoneNumber, ContactUsMessagesID = i.ContactUsMessagesID, EmailAddress = i.EmailAddress, LogTime = i.LogTime, MessageBody = i.MessageBody, MessageSubject = i.MessageSubject, SenderIPAddress = i.SenderIPAddress, SenderName = i.SenderName }).ToListAsync();

        return new() { ContactUsMessages = fetchList, DatabaseRecords = dbCount };
    }

    public static async Task<PublicActionResponse> ContactUsMessageArchive(MessageContactUsChangesRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchItem = await (from i in db.TB_ContactUs_Messages where i.ContactUsMessagesID == requestModel.ContactUsMessageID select i).FirstOrDefaultAsync();
        if (fetchItem is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        fetchItem.Archive = !fetchItem.Archive;

        try
        {
            db.Entry(fetchItem).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception x) { return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } }; }

        return new() { SuccessAction = true };
    }

    public static async Task<PublicActionResponse> ContactUsMessageDelete(MessageContactUsChangesRequest requestModel)
    {
        using DotnetableEntity db = new();

        var fetchItem = await (from i in db.TB_ContactUs_Messages where i.ContactUsMessagesID == requestModel.ContactUsMessageID select i).FirstOrDefaultAsync();
        if (fetchItem is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        db.TB_ContactUs_Messages.Remove(fetchItem);

        try
        {
            db.Entry(fetchItem).State = EntityState.Deleted;
            await db.SaveChangesAsync();
        }
        catch (Exception x) { return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } }; }

        return new() { SuccessAction = true };
    }


}