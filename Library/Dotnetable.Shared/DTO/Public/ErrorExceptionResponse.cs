namespace Dotnetable.Shared.DTO.Public;

public class ErrorExceptionResponse
{
    public string ErrorCode { get; set; }
    public string Message { get; set; }
    //public string ErrorID { get; set; }
    //public string HelpLink { get; set; }
    public List<ErrorValidateModelStateParamsResponse> ModelStateValidation { get; set; }
}
