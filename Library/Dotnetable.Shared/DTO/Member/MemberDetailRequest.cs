﻿using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Member;

public class MemberDetailRequest
{
    public int CurrentMemberID { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Public_Int_Length))]        
    public int MemberID { get; set; }
}
