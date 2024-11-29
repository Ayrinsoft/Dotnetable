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
        if (changeRequest.CurrentMemberID is null)
            return new() { ErrorException = new() { ErrorCode = "C0" } };

        if (changeRequest.OldPassword == changeRequest.NewPassword)
            return new() { ErrorException = new() { ErrorCode = "C15" } };
        changeRequest.OldPassword = changeRequest.OldPassword.HashLogin();
        changeRequest.NewPassword = changeRequest.NewPassword.HashLogin();

        return await MemberDataAccess.ChangeSelfPassword(changeRequest);
    }

    public async Task<PublicActionResponse> ChangeUserPassword(MemberChangePasswordAdminRequest changeRequest)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(changeRequest.CurrentMemberID, nameof(MemberRole.MemberManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

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
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(listRequest.CurrentMemberID, nameof(MemberRole.MemberManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        listRequest.OrderbyParams = listRequest.OrderbyParams.CheckForInjection(new List<string>() { "MemberID", "Username", "Email", "CellphoneNumber", "Gender", "Givenname", "Surname", });
        return await MemberDataAccess.MemberList(listRequest);
    }

    public async Task<MemberDetailResponse> MemberDetail(MemberDetailRequest detailRequest)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(detailRequest.CurrentMemberID, nameof(MemberRole.MemberManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        return await MemberDataAccess.MemberDetail(detailRequest);
    }

    public async Task<PublicActionResponse> Register(MemberInsertRequest requestModel)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(requestModel.CurrentMemberID, nameof(MemberRole.MemberManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        //if (requestModel.Password != requestModel.ConfirmPassword)
        //    return new() { ErrorException = new() { ErrorCode = "C10" } };
        requestModel.Username = requestModel.Username.ToLower();
        requestModel.Password = requestModel.Password.HashLogin();
        return await MemberDataAccess.Register(requestModel);
    }

    public async Task<PublicActionResponse> RegisterWebsite(MemberWebsiteRegisterRequest requestModel)
    {
        return await Register(requestModel);
    }

    public async Task<PublicActionResponse> ChangeStatus(MemberChangeStatusRequest changeRequest)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(changeRequest.CurrentMemberID, nameof(MemberRole.MemberManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        return await MemberDataAccess.ChangeStatus(changeRequest);
    }

    public async Task<PublicActionResponse> Edit(MemberEditRequest editRequest)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(editRequest.CurrentMemberID, nameof(MemberRole.MemberManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

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

        if (editRequest.CurrentMemberID == editRequest.MemberID.Value)
            return new() { ErrorException = new() { ErrorCode = "C16" } };

        if (!editRequest.MemberID.HasValue || editRequest.MemberID.Value < 1)
            return new() { ErrorException = new() { ErrorCode = "C12" } };
        editRequest.Username = editRequest.Username.ToLower();

        return await MemberDataAccess.Edit(editRequest);
    }

    public async Task<PublicActionResponse> ActivateAdmin(MemberActivateAdminRequest requestModel)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(requestModel.CurrentMemberID, nameof(MemberRole.MemberManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        return await MemberDataAccess.ActivateAdmin(requestModel);
    }

    public async Task<MemberContactListResponse> ContactList(MemberContactListRequest listRequest)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(listRequest.CurrentMemberID, nameof(MemberRole.MemberManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        return await MemberDataAccess.ContactList(listRequest);
    }

    public async Task<PublicActionResponse> ContactUpdate(MemberContactRequest changeRequest)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(changeRequest.CurrentMemberID.Value, nameof(MemberRole.MemberManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        if (changeRequest.CurrentMemberID is null) return null;

        if (string.IsNullOrEmpty(changeRequest.Address) && string.IsNullOrEmpty(changeRequest.PhoneNumber) && string.IsNullOrEmpty(changeRequest.CellphoneNumber))
            return new() { ErrorException = new() { ErrorCode = "C11" } };
        return await MemberDataAccess.ContactUpdate(changeRequest);
    }

    public async Task<PublicActionResponse> ContactDelete(MemberContactDeleteRequest deleteRequest)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(deleteRequest.CurrentMemberID.Value, nameof(MemberRole.MemberManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        if (deleteRequest.CurrentMemberID is null) return null;

        return await MemberDataAccess.ContactDelete(deleteRequest);
    }

    public async Task<PublicActionResponse> ContactInsert(MemberContactRequest insertRequest)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(insertRequest.CurrentMemberID.Value, nameof(MemberRole.MemberManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        if (insertRequest.CurrentMemberID is null || insertRequest.CurrentMemberID < 1)
            return new() { ErrorException = new() { ErrorCode = "C12" } };

        if (string.IsNullOrEmpty(insertRequest.Address) && string.IsNullOrEmpty(insertRequest.PhoneNumber) && string.IsNullOrEmpty(insertRequest.CellphoneNumber))
            return new() { ErrorException = new() { ErrorCode = "C11" } };

        return await MemberDataAccess.ContactInsert(insertRequest);
    }

    public async Task<MemberAvatarListFinalResponse> AvatarList(MemberAvatarListRequest listRequest)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(listRequest.CurrentMemberID, nameof(MemberRole.MemberManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        listRequest.MemberID ??= listRequest.CurrentMemberID;
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
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(deleteRequest.CurrentMemberID.Value, nameof(MemberRole.MemberManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        deleteRequest.MemberID ??= deleteRequest.CurrentMemberID;

        var removeResponse = await MemberDataAccess.AvatarDelete(deleteRequest);

        if (removeResponse.SuccessAction && removeResponse.ErrorException is null)
        {
            return await FileService.Remove(new() { FileCategoryID = removeResponse.FileCategoryID, FileCode = deleteRequest.AvatarCode.ToString(), FilePath = removeResponse.FilePath });
        }

        return new() { SuccessAction = removeResponse.SuccessAction, ErrorException = removeResponse.ErrorException };
    }

    public async Task<PublicActionResponse> AvatarInsert(MemberAvatarInsertRequest insertRequest)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(insertRequest.CurrentMemberID.Value, nameof(MemberRole.MemberManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        insertRequest.MemberID ??= insertRequest.CurrentMemberID;

        Guid fileCode = Guid.NewGuid();
        var insertFile = await _files.Insert(new Shared.DTO.File.FileInsertRequest() { FileCategoryID = 1, FileCode = fileCode.ToString(), FileName = insertRequest.FileName, FilePath = insertRequest.CurrentMemberID.ToString(), FileStream = insertRequest.FileStream, UploaderID = insertRequest.UploaderMemberID });


        if (insertFile is null || !insertFile.SuccessAction || insertFile.ErrorException != null) return insertFile;

        insertRequest.FileID = Convert.ToInt32(insertFile.ObjectID);

        var responseItem = await MemberDataAccess.AvatarInsert(insertRequest);
        if (responseItem.SuccessAction) responseItem.ObjectID = fileCode.ToString();
        if (responseItem.SuccessAction && insertRequest.SetAsDefault) await AvatarDefault(new() { FileCode = fileCode, CurrentMemberID = insertRequest.CurrentMemberID });

        return responseItem;
    }

    public async Task<PublicActionResponse> AvatarDefault(MemberAvatarDefaultRequest changeRequest)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(changeRequest.CurrentMemberID.Value, nameof(MemberRole.MemberManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        changeRequest.MemberID ??= changeRequest.CurrentMemberID;

        return await MemberDataAccess.AvatarDefault(changeRequest);
    }

    public async Task<PublicActionResponse> SendActivateLink(MemberActivateSendLinkRequest sendRequest)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(sendRequest.CurrentMemberID.Value, nameof(MemberRole.MemberManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        sendRequest.MemberID ??= sendRequest.CurrentMemberID;

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
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(requestModel.CurrentMemberID, nameof(MemberRole.MemberManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        requestModel.OrderbyParams = requestModel.OrderbyParams.CheckForInjection(new List<string>() { "EmailSubscribeID", "Email", "LogTime" });
        return await MemberDataAccess.MemberSubscribedList(requestModel);
    }

    public async Task<PublicActionResponse> MemberSubscribedChangeStatus(SubscribedChangeStatusRequest requestModel)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(requestModel.CurrentMemberID, nameof(MemberRole.MemberManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        return await MemberDataAccess.MemberSubscribedChangeStatus(requestModel);
    }

    #endregion

    #region Policy & Role
    public async Task<RoleListResponse> RoleList(RoleListRequest requestModel)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(requestModel.CurrentMemberID, nameof(MemberRole.PolicyManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        return await MemberDataAccess.RoleList(requestModel);
    }

    public async Task<List<PolicyListOnInsertMemberResponse>> PolicyListOnMemberManage(int memberID)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(memberID, nameof(MemberRole.PolicyManager)))
            return null;

        return await MemberDataAccess.PolicyListOnMemberManage(memberID);
    }

    public async Task<List<RoleListOnPolicyManageResponse>> RoleListOnPolicyManage(int memberID)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(memberID, nameof(MemberRole.PolicyManager)))
            return null;

        return await MemberDataAccess.RoleListOnPolicyManage(memberID);
    }

    public async Task<PolicyListResponse> PolicyList(PolicyListRequest requestModel)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(requestModel.CurrentMemberID, nameof(MemberRole.PolicyManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        return await MemberDataAccess.PolicyList(requestModel);
    }

    public async Task<PolicyRoleListResponse> PolicyRolesList(PolicyRoleListRequest requestModel)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(requestModel.CurrentMemberID, nameof(MemberRole.PolicyManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        return await MemberDataAccess.PolicyRolesList(requestModel);
    }

    public async Task<PublicActionResponse> PolicyInsert(PolicyInsertRequest requestModel)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(requestModel.CurrentMemberID, nameof(MemberRole.PolicyManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        return await MemberDataAccess.PolicyInsert(requestModel);
    }

    public async Task<PolicyDetailResponse> PolicyDetail(PolicyDetailRequest requestModel)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(requestModel.CurrentMemberID, nameof(MemberRole.PolicyManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        return await MemberDataAccess.PolicyDetail(requestModel);
    }

    public async Task<PublicActionResponse> PolicyUpdate(PolicyUpdateRequest requestModel)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(requestModel.CurrentMemberID, nameof(MemberRole.PolicyManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        return await MemberDataAccess.PolicyUpdate(requestModel);
    }

    public async Task<PublicActionResponse> PolicyChangeStatus(PolicyChangeStatusRequest requestModel)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(requestModel.CurrentMemberID, nameof(MemberRole.PolicyManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        return await MemberDataAccess.PolicyChangeStatus(requestModel);
    }

    public async Task<PublicActionResponse> PolicyRoleRemove(PolicyRoleRemoveRequest requestModel)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(requestModel.CurrentMemberID, nameof(MemberRole.PolicyManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        return await MemberDataAccess.PolicyRoleRemove(requestModel);
    }

    public async Task<PublicActionResponse> MemberRoleAppend(PolicyRoleAppendRequest requestModel)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(requestModel.CurrentMemberID, nameof(MemberRole.PolicyManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        return await MemberDataAccess.MemberRoleAppend(requestModel);
    }



    #endregion

}
