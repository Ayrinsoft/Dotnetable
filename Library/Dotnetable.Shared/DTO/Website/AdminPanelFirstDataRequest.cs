using Dotnetable.Shared.Tools;
using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Website;

public class AdminPanelFirstDataRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_AvailableLanguages_Required))]
    [NotEmpty]
    public List<string> AvailableLanguages { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_CountryName_Required))]
    [StringLength(42, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_42))]
    [MinLength(2, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_2))]
    public string CountryName { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_CountryCode_Required))]
    [StringLength(2, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_2))]
    [MinLength(2, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_2))]
    public string CountryCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_CityName_Required))]
    [StringLength(48, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_48))]
    [MinLength(2, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_2))]
    public string CityName { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_CityCode_Required))]
    [StringLength(3, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_3))]
    [MinLength(3, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_3))]
    public string CityCode { get; set; }



    //User
    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Authentication_Username_Required))]
    [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_64))]
    [MinLength(4, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_4))]
    public string Username { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Authentication_Password_Required)), DataType(DataType.Password)]
    [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_64))]
    [MinLength(4, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_8))]
    public string Password { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Member_Email_Required)), DataType(DataType.EmailAddress)]
    [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_64))]
    [MinLength(8, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_8))]
    public string Email { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Member_Cellphone_Required))]
    [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_64))]
    [MinLength(9, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_9))]
    public string CellphoneNumber { get; set; }

    public bool? Gender { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Member_CountryCode_Required))]
    [StringLength(3, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_3))]
    [MinLength(1, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_1))]
    public string PhoneCountryCode { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Member_Givenname_Required))]
    [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_64))]
    [MinLength(2, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_2))]
    public string GivenName { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Member_Surname_Required))]
    [StringLength(64, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_64))]
    [MinLength(2, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_2))]
    public string Surname { get; set; }
}
