﻿using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Message;

public class EmailPanelChangeStatusRequest
{
    public int CurrentMemberID { get; set; }

    [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_EmailSettingID_Required))]
    [Range(1, int.MaxValue, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Public_Int_Length))]
    public int EmailSettingID { get; set; }
}
