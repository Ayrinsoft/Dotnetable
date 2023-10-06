using Dotnetable.Data.DataAccess;
using Dotnetable.Service.PrivateService;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.DTO.Website;

namespace Dotnetable.Service;

public class WebsiteService
{
    public async Task<PublicActionResponse> ImplementDB(string languageCode)
    {
        return await WebsiteDataAccess.ImplementDB(languageCode);
    }

    public async Task<PublicActionResponse> InsertFirstData(AdminPanelFirstDataRequest requestModel)
    {
        requestModel.Username = requestModel.Username.ToLower();
        requestModel.Password = requestModel.Password.HashLogin();
        return await WebsiteDataAccess.InsertFirstData(requestModel);
    }









}
