using Dotnetable.Data.DataAccess;
using Dotnetable.Service.PrivateService;
using Dotnetable.Shared.DTO.Member;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;

namespace Dotnetable.Service;
public class MemberService
{
    #region CTOR

    //TODO: Hashkey in config file
    private readonly MessageService _msg;
    private readonly FileService _files;
    public MemberService(MessageService msg, FileService files)
    {
        _msg = msg;
        _files = files;
    }
    #endregion

    #region Admin&User
    public static async Task<int> FetchMemberIDByHashKey(Guid userHashKey)
    {
        return await MemberDataAccess.FetchMemberIDByHashKey(userHashKey);
    }

    public async Task<PublicActionResponse> ChangeSelfPassword(MemberChangePasswordRequest changeRequest)
    {
        if (changeRequest.OldPassword == changeRequest.NewPassword)
            return new() { ErrorException = new() { ErrorCode = "C15" } };
        changeRequest.OldPassword = changeRequest.OldPassword.HashLogin();
        changeRequest.NewPassword = changeRequest.NewPassword.HashLogin();

        return await MemberDataAccess.ChangeSelfPassword(changeRequest);
    }

    public async Task<PublicActionResponse> ChangeUserPassword(MemberChangePasswordAdminRequest changeRequest)
    {
        string clearPassword = changeRequest.NewPassword;
        changeRequest.NewPassword = changeRequest.NewPassword.HashLogin();
        var responseItem = await MemberDataAccess.ChangeUserPassword(changeRequest);
        if (changeRequest.SendMailForUser && responseItem.SuccessAction)
        {
            var emailSentResponse = await _msg.EmailSend(new Shared.DTO.Message.EmailSendRequest() { EmailAddress = responseItem.ObjectID, Title = "Your Password has changed", Body = $"your new password is: {clearPassword}" }).ConfigureAwait(false);
            if (emailSentResponse is null || emailSentResponse.ErrorException is not null) return new() { ErrorException = emailSentResponse.ErrorException };
        }
        return new() { SuccessAction = true };
    }

    public async Task<MemberListFinalResponse> MemberList(MemberListRequest listRequest)
    {
        listRequest.OrderbyParams = listRequest.OrderbyParams.CheckForInjection(new List<string>() { "MemberID", "Username", "Email", "CellphoneNumber", "Gender", "Givenname", "Surname", });
        return await MemberDataAccess.MemberList(listRequest);
    }

    public async Task<MemberDetailResponse> MemberDetail(MemberDetailRequest detailRequest)
    {
        return await MemberDataAccess.MemberDetail(detailRequest);
    }

    public async Task<PublicActionResponse> Register(MemberInsertRequest requestModel)
    {
        //if (requestModel.Password != requestModel.ConfirmPassword)
        //    return new() { ErrorException = new() { ErrorCode = "C10" } };
        requestModel.Username = requestModel.Username.ToLower();
        requestModel.Password = requestModel.Password.HashLogin();
        return await MemberDataAccess.Register(requestModel);
    }

    public async Task<PublicActionResponse> RegisterWebsite(MemberWebsiteRegisterRequest requestModel)
    {
        return await this.Register(requestModel);
    }

    public async Task<PublicActionResponse> ChangeStatus(MemberChangeStatusRequest changeRequest)
    {
        return await MemberDataAccess.ChangeStatus(changeRequest);
    }

    public async Task<PublicActionResponse> Edit(MemberEditRequest editRequest)
    {
        //if (!(EditRequest.UsernameChange.HasValue && EditRequest.UsernameChange.Value) &&
        //    !(EditRequest.EmailChange.HasValue && EditRequest.EmailChange.Value) &&
        //    !(EditRequest.CellphoneChange.HasValue && EditRequest.CellphoneChange.Value) &&
        //    !(EditRequest.CountryCodeChange.HasValue && EditRequest.CountryCodeChange.Value) &&
        //    !(EditRequest.GivennameChange.HasValue && EditRequest.GivennameChange.Value) &&
        //    !(EditRequest.SurnameChange.HasValue && EditRequest.SurnameChange.Value) &&
        //    !(EditRequest.LanguageIDChange.HasValue && EditRequest.LanguageIDChange.Value) &&
        //    !(EditRequest.PlaceIDChange.HasValue && EditRequest.PlaceIDChange.Value))
        //{
        //    return new()
        //    {
        //        ErrorException = new()
        //        {
        //            ErrorCode = "C11"
        //        }
        //    };
        //}

        if (editRequest.CurrentMemberID.HasValue && editRequest.CurrentMemberID == editRequest.MemberID)
            return new() { ErrorException = new() { ErrorCode = "C16" } };

        if (!editRequest.MemberID.HasValue || editRequest.MemberID.Value < 1)
            return new() { ErrorException = new() { ErrorCode = "C12" } };
        editRequest.Username = editRequest.Username.ToLower();

        return await MemberDataAccess.Edit(editRequest);
    }

    public async Task<PublicActionResponse> ActivateAdmin(MemberActivateAdminRequest requestModel)
    {
        return await MemberDataAccess.ActivateAdmin(requestModel);
    }

    public async Task<MemberContactListResponse> ContactList(MemberContactListRequest listRequest)
    {
        return await MemberDataAccess.ContactList(listRequest);
    }

    public async Task<PublicActionResponse> ContactUpdate(MemberContactRequest changeRequest)
    {
        if (changeRequest.CurrentMemberID is null || changeRequest.CurrentMemberID < 1)
            return new() { ErrorException = new() { ErrorCode = "C12" } };
        if (string.IsNullOrEmpty(changeRequest.Address) && string.IsNullOrEmpty(changeRequest.PhoneNumber) && string.IsNullOrEmpty(changeRequest.CellphoneNumber))
            return new() { ErrorException = new() { ErrorCode = "C11" } };
        return await MemberDataAccess.ContactUpdate(changeRequest);
    }

    public async Task<PublicActionResponse> ContactDelete(MemberContactDeleteRequest deleteRequest)
    {
        if (deleteRequest.CurrentMemberID is null || deleteRequest.CurrentMemberID < 1)
            return new() { ErrorException = new() { ErrorCode = "C12" } };

        return await MemberDataAccess.ContactDelete(deleteRequest);
    }

    public async Task<PublicActionResponse> ContactInsert(MemberContactRequest insertRequest)
    {
        if (insertRequest.CurrentMemberID is null || insertRequest.CurrentMemberID < 1)
            return new() { ErrorException = new() { ErrorCode = "C12" } };

        if (string.IsNullOrEmpty(insertRequest.Address) && string.IsNullOrEmpty(insertRequest.PhoneNumber) && string.IsNullOrEmpty(insertRequest.CellphoneNumber))
            return new() { ErrorException = new() { ErrorCode = "C11" } };

        return await MemberDataAccess.ContactInsert(insertRequest);
    }

    public async Task<MemberAvatarListFinalResponse> AvatarList(MemberAvatarListRequest listRequest)
    {
        var fetchAvatars = await MemberDataAccess.AvatarList(listRequest);
        if (fetchAvatars.ErrorException is not null)
            return new() { ErrorException = fetchAvatars.ErrorException };

        MemberAvatarListFinalResponse ResponseItem = new() { DatabaseRecords = fetchAvatars.DatabaseRecords, Avatars = new() };
        if (fetchAvatars.Avatars != null && fetchAvatars.Avatars.Count > 0)
        {
            foreach (var j in fetchAvatars.Avatars)
            {
                ResponseItem.Avatars.Add(new()
                {
                    FileCode = j.FileCode,
                    FileName = j.FileName,
                    FileURL = $"/File/Receive/original/{j.FileCode}/{j.FileName}",
                    Thumbnail = $"/File/Receive/75X75/{j.FileCode}/{j.FileName}"
                });
            }
        }
        return ResponseItem;
    }

    public async Task<PublicActionResponse> AvatarDelete(MemberAvatarDeleteRequest deleteRequest)
    {
        if (deleteRequest.CurrentMemberID is null || deleteRequest.CurrentMemberID < 1)
            return new() { ErrorException = new() { ErrorCode = "C12" } };


        var removeResponse = await MemberDataAccess.AvatarDelete(deleteRequest);

        if (removeResponse.SuccessAction && removeResponse.ErrorException is null)
        {
            return FileService.Remove(new() { FileCategoryID = removeResponse.FileCategoryID, FileCode = deleteRequest.AvatarCode.ToString(), FilePath = removeResponse.FilePath });
        }

        return new() { SuccessAction = removeResponse.SuccessAction, ErrorException = removeResponse.ErrorException };
    }

    public async Task<PublicActionResponse> AvatarInsert(MemberAvatarInsertRequest insertRequest)
    {
        if (insertRequest.CurrentMemberID is null || insertRequest.CurrentMemberID < 1)
            return new() { ErrorException = new() { ErrorCode = "C12" } };

        Guid fileCode = Guid.NewGuid();
        var insertFile = await _files.Insert(new Shared.DTO.File.FileInsertRequest() { FileCategoryID = 1, FileCode = fileCode.ToString(), FileName = insertRequest.FileName, FilePath = insertRequest.CurrentMemberID.ToString(), FileStream = insertRequest.FileStream, UploaderID = insertRequest.UploaderMemberID });


        if (insertFile is null || !insertFile.SuccessAction || insertFile.ErrorException != null) return insertFile;

        insertRequest.FileID = Convert.ToInt32(insertFile.ObjectID);

        var responseItem = await MemberDataAccess.AvatarInsert(insertRequest);
        if (responseItem.SuccessAction) responseItem.ObjectID = fileCode.ToString();
        if (responseItem.SuccessAction && insertRequest.SetAsDefault) await this.AvatarDefault(new MemberAvatarDefaultRequest() { FileCode = fileCode, CurrentMemberID = insertRequest.CurrentMemberID });

        return responseItem;
    }

    public async Task<PublicActionResponse> AvatarDefault(MemberAvatarDefaultRequest changeRequest)
    {
        if (changeRequest.CurrentMemberID is null || changeRequest.CurrentMemberID < 1)
            return new() { ErrorException = new() { ErrorCode = "C12" } };

        return await MemberDataAccess.AvatarDefault(changeRequest);
    }

    public async Task<PublicActionResponse> SendActivateLink(MemberActivateSendLinkRequest sendRequest)
    {
        if (sendRequest.CurrentMemberID is null || sendRequest.CurrentMemberID < 1)
            return new() { ErrorException = new() { ErrorCode = "C12" } };

        return await MemberDataAccess.ActivateLinkInsert(sendRequest.CurrentMemberID.Value);
    }

    public async Task<PublicActionResponse> ActivateLinkCheck(Guid activateCode)
    {
        return await MemberDataAccess.ActivateLinkCheck(activateCode);
    }


    public async Task<PublicActionResponse> ForgetPasswordGetCode(MemberForgetPasswordRequest requestModel)
    {
        requestModel.Username = requestModel.Username.Trim();
        requestModel.ForgetKey = GeneralEvents.GenerateRandomCode(8);
        var passwordCode = await MemberDataAccess.GetRecoveryPasswordCode(requestModel);
        if (passwordCode.ErrorException is not null) return new() { ErrorException = passwordCode.ErrorException };

        var mailSentDetail = await _msg.EmailSend(new Shared.DTO.Message.EmailSendRequest() { Title = "Forget password key", Body = $"<p>Your new forget password key is: {requestModel.ForgetKey} </p><p> It will be expire at 5 minutes later</p>", EmailAddress = passwordCode.ObjectID });
        if (mailSentDetail.ErrorException is null)
            return new() { SuccessAction = true };
        else
            return new() { SuccessAction = false, ErrorException = mailSentDetail.ErrorException };
    }


    public async Task<PublicActionResponse> ForgetPasswordSetCode(MemberForgetPasswordSetRequest requestModel)
    {
        requestModel.ForgetKey = requestModel.ForgetKey.Trim();
        var passwordCode = await MemberDataAccess.SetRecoveryPasswordCode(requestModel);
        if (passwordCode.ErrorException is not null) return new() { ErrorException = passwordCode.ErrorException };

        var changePasswordResponse = await this.ChangeUserPassword(new MemberChangePasswordAdminRequest() { MemberID = Convert.ToInt32(passwordCode.ObjectID), NewPassword = requestModel.Password, SendMailForUser = true });

        if (changePasswordResponse.ErrorException is null)
            return new() { SuccessAction = true };
        else
            return new() { SuccessAction = false, ErrorException = changePasswordResponse.ErrorException };
    }


    #endregion

    #region Subscribe
    public async Task<PublicActionResponse> EmailSubscribeRegister(MemberEmailSubscribeRegisterRequest requestModel)
    {
        var responseModel = await MemberDataAccess.EmailSubscribeRegister(requestModel);
        if (responseModel.SuccessAction)
        {
            var checkSubscribeEmail = await MemberDataAccess.CheckSubscribeEmailOnDB(requestModel.EmailAddress ?? "");
            if (checkSubscribeEmail.ErrorException is not null)
                return new() { ErrorException = checkSubscribeEmail.ErrorException };

            string hashCheck = $"{requestModel.EmailAddress.ToUpper().HashLogin()}-{checkSubscribeEmail.ObjectID.ToUpper()}".HashLogin();
            string approveAddress = $"{requestModel.RequestURL}/{requestModel.EmailAddress}/{hashCheck}";
            var emailSentResponse = await _msg.EmailSend(new() { EmailAddress = requestModel.EmailAddress, Title = "Approve your Email", Body = $"<p>Your Email address has been Approve in subscribe.</p> <p>Use this link to do unsubscribe: <a href=\"{approveAddress}\" target=\"_blank\">{approveAddress}</a></p>" }).ConfigureAwait(false);
            if (emailSentResponse is null || emailSentResponse.ErrorException is not null) return new() { ErrorException = emailSentResponse?.ErrorException ?? new() { ErrorCode = "SX" } };
        }

        return responseModel;
    }

    public async Task<PublicActionResponse> EmailSubscribeApprove(MemberEmailSubscribeApproveRequest requestModel)
    {
        var checkSubscribeEmail = await MemberDataAccess.CheckSubscribeEmailOnDB(requestModel.EmailAddress ?? "");
        if (checkSubscribeEmail.ErrorException is not null)
            return new() { ErrorException = checkSubscribeEmail.ErrorException };

        if (requestModel.CheckHash.ToUpper() != $"{requestModel.EmailAddress.ToUpper().HashLogin()}-{checkSubscribeEmail.ObjectID.ToUpper()}".HashLogin().ToUpper()) return new() { ErrorException = new() { ErrorCode = "D0" } };

        return await MemberDataAccess.EmailSubscribeApprove(requestModel);
    }

    public async Task<PublicActionResponse> EmailSubscribeRemoveSendCode(MemberEmailSubscribeRemoveSendCodeRequest requestModel)
    {
        var checkSubscribeEmail = await MemberDataAccess.CheckSubscribeEmailOnDB(requestModel.EmailAddress);
        if (checkSubscribeEmail.ErrorException is not null)
            return new() { ErrorException = checkSubscribeEmail.ErrorException };

        string hashCheck = $"{requestModel.EmailAddress.ToUpper().HashLogin()}-{checkSubscribeEmail.ObjectID.ToUpper()}".HashLogin();
        string unsubscribeAddress = $"{requestModel.RequestURL}/{requestModel.EmailAddress}/{hashCheck}";
        var emailSentResponse = await _msg.EmailSend(new Shared.DTO.Message.EmailSendRequest() { EmailAddress = requestModel.EmailAddress, Title = "Unsubscribe Email", Body = $"<p>Your Email address has been remove from subscribe.</p> <p>Use this link to do unsubscribe: <a href=\"{unsubscribeAddress}\" target=\"_blank\">{unsubscribeAddress}</a></p>" }).ConfigureAwait(false);
        if (emailSentResponse is null || emailSentResponse.ErrorException is not null) return new() { ErrorException = emailSentResponse.ErrorException };
        return new() { SuccessAction = true };
    }

    public async Task<PublicActionResponse> EmailSubscribeRemove(MemberEmailSubscribeRemoveRequest requestModel)
    {
        var checkSubscribeEmail = await MemberDataAccess.CheckSubscribeEmailOnDB(requestModel.EmailAddress);
        if (checkSubscribeEmail.ErrorException is not null)
            return new() { ErrorException = checkSubscribeEmail.ErrorException };

        if ($"{requestModel.EmailAddress.ToUpper().HashLogin()}-{checkSubscribeEmail.ObjectID.ToUpper()}".HashLogin().ToUpper() != requestModel.CheckHash.ToUpper())
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        return await MemberDataAccess.EmailSubscribeRemove(requestModel);
    }

    public async Task<SubscribeListResponse> MemberSubscribedList(SubscribeListRequest requestModel)
    {
        requestModel.OrderbyParams = requestModel.OrderbyParams.CheckForInjection(new List<string>() { "EmailSubscribeID", "Email", "LogTime" });
        return await MemberDataAccess.MemberSubscribedList(requestModel);
    }

    public async Task<PublicActionResponse> MemberSubscribedChangeStatus(SubscribedChangeStatusRequest requestModel)
    {
        return await MemberDataAccess.MemberSubscribedChangeStatus(requestModel);
    }

    #endregion

    #region Policy & Role
    public async Task<RoleListResponse> RoleList(RoleListRequest requestModel)
    {
        return await MemberDataAccess.RoleList(requestModel);
    }

    public async Task<List<PolicyListOnInsertMemberResponse>> PolicyListOnMemberManage(int memberID)
    {
        return await MemberDataAccess.PolicyListOnMemberManage(memberID);
    }

    public async Task<List<RoleListOnPolicyManageResponse>> RoleListOnPolicyManage(int memberID)
    {
        return await MemberDataAccess.RoleListOnPolicyManage(memberID);
    }

    public async Task<PolicyListResponse> PolicyList(PolicyListRequest requestModel)
    {
        return await MemberDataAccess.PolicyList(requestModel);
    }

    public async Task<PolicyRoleListResponse> PolicyRolesList(PolicyRoleListRequest requestModel)
    {
        return await MemberDataAccess.PolicyRolesList(requestModel);
    }

    public async Task<PublicActionResponse> PolicyInsert(PolicyInsertRequest requestModel)
    {
        return await MemberDataAccess.PolicyInsert(requestModel);
    }

    public async Task<PolicyDetailResponse> PolicyDetail(PolicyDetailRequest requestModel)
    {
        return await MemberDataAccess.PolicyDetail(requestModel);
    }

    public async Task<PublicActionResponse> PolicyUpdate(PolicyUpdateRequest requestModel)
    {
        return await MemberDataAccess.PolicyUpdate(requestModel);
    }

    public async Task<PublicActionResponse> PolicyChangeStatus(PolicyChangeStatusRequest requestModel)
    {
        return await MemberDataAccess.PolicyChangeStatus(requestModel);
    }

    public async Task<PublicActionResponse> PolicyRoleRemove(PolicyRoleRemoveRequest requestModel)
    {
        return await MemberDataAccess.PolicyRoleRemove(requestModel);
    }

    public async Task<PublicActionResponse> MemberRoleAppend(PolicyRoleAppendRequest requestModel)
    {
        return await MemberDataAccess.MemberRoleAppend(requestModel);
    }



    #endregion

}
