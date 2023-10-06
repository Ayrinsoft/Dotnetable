namespace Dotnetable.Admin.SharedServices.Authorization;

internal static class LocalSecret
{
    internal static string TokenHashKey(string configHash) => $"<Current|Hash>{configHash}<Change_it>"; //hash string for more security
}
