using Dotnetable.Data.DataAccess;
using Dotnetable.Shared.DTO.Logs;

namespace Dotnetable.Service;
public class LogsService
{

    public async Task<LogsCheckIPActionValidResponse> CheckIPActions(LogsCheckIPActionValidRequest requestModel)
    {
        try
        {
            var dbCheck = await LogsDataAccess.CheckIPActions(requestModel.IPAddress);
            return new()
            {
                TryCount = dbCheck.TryCount,
                ValidForNewRequest = dbCheck.TryCount <= 6
            };
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }
    }


}