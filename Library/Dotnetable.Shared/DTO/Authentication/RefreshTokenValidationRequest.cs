using System.ComponentModel.DataAnnotations;

namespace Dotnetable.Shared.DTO.Authentication
{
    public class RefreshTokenValidationRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Authentication_Token_Required))]
        [StringLength(1024, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MaxLength_1024))]
        [MinLength(4, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_MinLength_4))]
        public string Token { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = nameof(Resources.Resource._Err_Authentication_RefreshToken_Required))]
        public Guid RefreshToken { get; set; }
        public string ClientIP { get; set; }
    }
}
