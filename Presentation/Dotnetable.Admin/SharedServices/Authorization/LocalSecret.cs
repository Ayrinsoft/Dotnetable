namespace Dotnetable.Admin.SharedServices.Authorization;

internal static class LocalSecret
{
    internal static string TokenHashKey(string configHash) => $"<Current|Hash - the default key to use for show in code> + your config hash key {configHash} appended here <Change_it to new one>"; //hash string for more security
}
