using System.Text.Json;

namespace Dotnetable.Infrastructure.Social;

/// <summary>Turns a Telegram or worker response into the text shown on Check and save.</summary>
internal static class TelegramApiError
{
    public static string Describe(int status, string? payload, string botToken)
    {
        var body = payload ?? "";
        if (!string.IsNullOrEmpty(botToken))
            body = body.Replace(botToken, "…", StringComparison.Ordinal);

        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
            var root = doc.RootElement;
            if (root.TryGetProperty("description", out var description)
                && description.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(description.GetString()))
            {
                var text = description.GetString()!;
                return text.Contains("chat not found", StringComparison.OrdinalIgnoreCase)
                    ? "Telegram could not find that chat. Use @name or t.me/name for a public channel, or the numeric id that starts with -100. The bot must be an admin of the channel."
                    : text;
            }

            if (root.TryGetProperty("error", out var error)
                && error.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(error.GetString()))
                return "The proxy could not call Telegram: " + error.GetString();
        }
        catch (JsonException)
        {
        }

        var plain = body.Trim();
        if (status == 401 || plain.Equals("Unauthorized", StringComparison.OrdinalIgnoreCase))
            return "The proxy rejected the key. Use the same text in the worker SECRET and in the proxy key.";
        if (status == 404 || plain.Equals("Not found", StringComparison.OrdinalIgnoreCase))
            return "The worker does not accept /proxy/b64/. Paste the script from the Outbound proxy tab and deploy it again.";
        if (plain.Length is > 0 and <= 180 && !plain.StartsWith('{'))
            return plain;
        return status is >= 200 and < 300
            ? "Telegram refused the request. Confirm the bot is an admin of that chat."
            : "Telegram did not return a readable answer (" + status.ToString(System.Globalization.CultureInfo.InvariantCulture) + ").";
    }
}
