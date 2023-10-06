using Dotnetable.Data.DBContext;
using Dotnetable.Shared.DTO.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Data.DataAccess;
public class LogsDataAccess
{

    public static async Task<CheckIPActionResponse> CheckIPActions(string ipAddress)
    {
        using DotnetableEntity db = new();
        DateTime checkTime = DateTime.Now.AddMinutes(-5);
        int fetchLogsCount = await (from i in db.TB_IP_Address_Actions where i.TryIP == ipAddress && i.LogTime > checkTime select i).CountAsync();

        return new() { TryCount = fetchLogsCount };
    }


}
