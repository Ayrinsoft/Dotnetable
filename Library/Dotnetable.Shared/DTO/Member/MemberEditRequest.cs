using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Member;

public class MemberEditRequest
{
    public int? MemberID { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Authentication_Username_Required))]
    [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_64))]
    [MinLength(4, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_4))]
    public string Username { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Member_Email_Required))]
    [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_64))]
    [MinLength(8, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_8))]
    public string Email { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Member_Cellphone_Required))]
    [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_64))]
    [MinLength(9, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_9))]
    public string CellphoneNumber { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Member_CountryCode_Required))]
    [StringLength(3, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_3))]
    [MinLength(1, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_1))]
    public string CountryCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Member_Givenname_Required))]
    [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_64))]
    [MinLength(2, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_2))]
    public string Givenname { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Member_Surname_Required))]
    [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_64))]
    [MinLength(2, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_2))]
    public string Surname { get; set; }

    public int? CityID { get; set; }
    public string CityName { get; set; }
    public byte? CountryID { get; set; }
    public bool Gender { get; set; }
    public string PostalCode { get; set; }

    public int? PolicyID { get; set; }
    public int? CurrentMemberID { get; set; }
}
