using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Admin.SharedServices.Data;

public interface IHttpServices
{
    Task<PublicControllerResponse> CallServiceObjAsync(HttpMethod method, bool hasAutentication, string urlAddress, string requestBody = "");
}