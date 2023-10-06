namespace Dotnetable.Shared.DTO.Public;

public class ErrorValidateModelStateParamsResponse
{
    public string ParamName { get; set; }
    public List<string> ParamErrors { get; set; }
}
