using Dotnetable.Shared.DTO.Public;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Admin.SharedServices;

public class ModelStateValidator
{
    public static IActionResult ValidateModelState(ActionContext context)
    {
        var validateErrors = context.ModelState.Where(i => i.Value.Errors.Count > 0).ToList();
        List<ErrorValidateModelStateParamsResponse> ErrorsList = new();
        foreach (var j in validateErrors)
            ErrorsList.Add(new()
            {
                ParamName = j.Key,
                ParamErrors = j.Value.Errors.Select(i => i.ErrorMessage).ToList()
            });

        return new OkObjectResult(new PublicControllerResponse()
        {
            ErrorException = new()
            {
                ErrorCode = "C0",
                ModelStateValidation = ErrorsList
            }
        });
    }
}
