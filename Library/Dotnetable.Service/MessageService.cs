using Dotnetable.Data.DataAccess;
using Dotnetable.Shared.DTO.Message;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Dotnetable.Service;
public class MessageService
{
    private static bool AcceptAllCertifications(object sender, X509Certificate certification, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }


    public async Task<PublicActionResponse> EmailSend(EmailSendRequest mailRequest)
    {
        var emailSetting = await MessageDataAccess.EmailSetting(mailRequest.EmailType);

        if (emailSetting is null)
            return new() { ErrorException = new() { ErrorCode = "C13" } };

        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);

        var mailMsg = new MailMessage(new MailAddress(emailSetting.EmailAddress, emailSetting.MailName), new MailAddress(mailRequest.EmailAddress, null))
        {
            Subject = mailRequest.Title,
            Body = mailRequest.Body,
            IsBodyHtml = true,
            BodyEncoding = new UTF8Encoding()
        };

        var smtp = new SmtpClient()
        {
            Port = Convert.ToInt32(emailSetting.PortNumber),
            Host = emailSetting.MailServer,
            EnableSsl = emailSetting.EnableSSL,
            Credentials = new NetworkCredential(emailSetting.EmailAddress, emailSetting.Password)
        };

        try
        {
            await smtp.SendMailAsync(mailMsg);
        }
        catch (Exception ex)
        {
            return new() { ErrorException = new() { ErrorCode = "C14", Message = ex.Message } };
        }

        return new() { SuccessAction = true };
    }

    public async Task<PublicActionResponse> EmailSettingInsert(EmailPanelInsertRequest requestModel)
    {
        return await MessageDataAccess.EmailSettingInsert(requestModel);
    }

    public async Task<PublicActionResponse> EmailSettingUpdate(EmailPanelUpdateRequest requestModel)
    {
        return await MessageDataAccess.EmailSettingUpdate(requestModel);
    }

    public async Task<PublicActionResponse> EmailSettingChangeStatus(EmailPanelChangeStatusRequest requestModel)
    {
        return await MessageDataAccess.EmailSettingChangeStatus(requestModel);
    }

    public async Task<EmailPanelListResponse> EmailSettingList(EmailPanelListRequest requestModel)
    {
        requestModel.OrderbyParams = requestModel.OrderbyParams.CheckForInjection(new List<string>() { "i.EmailSettingID" });
        requestModel.EmailName ??= "";
        return await MessageDataAccess.EmailSettingList(requestModel);
    }




    public async Task<PublicActionResponse> ContactUsMessageInsert(MessageBoxOnContactUsRequest requestModel)
    {
        var responseModel = await MessageDataAccess.ContactUsMessageInsert(requestModel);

        //TODO: Collect Setting keys
        var fetchManagerInbox = await WebsiteDataAccess.FetchSettingByKey("MANAGER_EMAIL_ADDRESS");
        if (!string.IsNullOrEmpty(fetchManagerInbox) && fetchManagerInbox != "")
        {
            try
            {
                _ = await EmailSend(new() { EmailAddress = fetchManagerInbox, Body = $"<p>Message from website: </p><p>{requestModel.MessageBody}</p>", Title = $"Send message from site: {requestModel.SenderName} - {DateTime.Now:yyyy-MM-dd HH:mm:ss}" });
            }
            catch (Exception) { }
        }

        return responseModel;
    }

    public async Task<MessageContactUsListResponse> ContactUsMessageList(MessageContactUsListRequest requestModel)
    {
        requestModel.OrderbyParams = requestModel.OrderbyParams.CheckForInjection(new List<string>() { "ContactUsMessagesID", "SenderName", "EmailAddress", "CellphoneNumber", "LogTime", "SenderIPAddress" });
        return await MessageDataAccess.MessageContactUsList(requestModel);
    }

    public async Task<PublicActionResponse> ContactUsMessageArchive(MessageContactUsChangesRequest requestModel)
    {
        return await MessageDataAccess.ContactUsMessageArchive(requestModel);
    }

    public async Task<PublicActionResponse> ContactUsMessageDelete(MessageContactUsChangesRequest requestModel)
    {
        return await MessageDataAccess.ContactUsMessageDelete(requestModel);
    }


}