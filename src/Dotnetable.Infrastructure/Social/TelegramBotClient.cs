using System.Net.Http.Headers;
using System.Text.Json;
using Dotnetable.Application.Instagram;

namespace Dotnetable.Infrastructure.Social;

public sealed class TelegramBotClient : ITelegramBot
{
    private readonly HttpClient _http;

    public TelegramBotClient(HttpClient http)
    {
        _http = http;
        _http.Timeout = TimeSpan.FromMinutes(3);
    }

    public async Task<string> GetUsernameAsync(string botToken, CancellationToken ct = default)
    {
        using var doc = await GetAsync(botToken, "getMe", ct);
        return doc.RootElement.TryGetProperty("result", out var result) && result.TryGetProperty("username", out var name)
            ? name.GetString() ?? ""
            : "";
    }

    public async Task EnsureChatAsync(string botToken, string chatId, CancellationToken ct = default)
    {
        using var doc = await GetAsync(botToken, "getChat?chat_id=" + Uri.EscapeDataString(chatId), ct);
    }

    public async Task<string?> SendTextAsync(string botToken, string chatId, string text, CancellationToken ct = default)
    {
        using var form = new FormUrlEncodedContent(new Dictionary<string, string> { ["chat_id"] = chatId, ["text"] = text });
        using var doc = await PostAsync(botToken, "sendMessage", form, ct);
        return MessageId(doc.RootElement);
    }

    public Task<string?> SendPhotoAsync(string botToken, string chatId, TelegramOutbound photo, string? caption, CancellationToken ct = default) =>
        SendFileAsync(botToken, chatId, "sendPhoto", "photo", photo, caption, ct);

    public Task<string?> SendVideoAsync(string botToken, string chatId, TelegramOutbound video, string? caption, CancellationToken ct = default) =>
        SendFileAsync(botToken, chatId, "sendVideo", "video", video, caption, ct);

    public async Task<string?> SendAlbumAsync(string botToken, string chatId, IReadOnlyList<TelegramOutbound> items, string? caption, CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(chatId), "chat_id");
        var media = new List<object>();
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var key = "f" + i.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var entry = new Dictionary<string, string>
            {
                ["type"] = item.IsVideo ? "video" : "photo",
                ["media"] = "attach://" + key,
            };
            if (i == 0 && !string.IsNullOrWhiteSpace(caption)) entry["caption"] = caption;
            media.Add(entry);
            var file = new ByteArrayContent(item.Bytes);
            file.Headers.ContentType = new MediaTypeHeaderValue(item.IsVideo ? (string.IsNullOrWhiteSpace(item.MimeType) ? "video/mp4" : item.MimeType) : "image/jpeg");
            form.Add(file, key, string.IsNullOrWhiteSpace(item.FileName) ? key : item.FileName);
        }

        form.Add(new StringContent(JsonSerializer.Serialize(media)), "media");
        using var doc = await PostAsync(botToken, "sendMediaGroup", form, ct);
        return MessageId(doc.RootElement);
    }

    private async Task<string?> SendFileAsync(string botToken, string chatId, string method, string field, TelegramOutbound file, string? caption, CancellationToken ct)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(chatId), "chat_id");
        if (!string.IsNullOrWhiteSpace(caption)) form.Add(new StringContent(caption), "caption");
        var bytes = new ByteArrayContent(file.Bytes);
        bytes.Headers.ContentType = new MediaTypeHeaderValue(file.IsVideo ? (string.IsNullOrWhiteSpace(file.MimeType) ? "video/mp4" : file.MimeType) : "image/jpeg");
        form.Add(bytes, field, string.IsNullOrWhiteSpace(file.FileName) ? field : file.FileName);
        using var doc = await PostAsync(botToken, method, form, ct);
        return MessageId(doc.RootElement);
    }

    private Task<JsonDocument> GetAsync(string botToken, string methodAndQuery, CancellationToken ct) =>
        SendAsync(HttpMethod.Get, botToken, methodAndQuery, null, ct);

    private Task<JsonDocument> PostAsync(string botToken, string method, HttpContent? body, CancellationToken ct) =>
        SendAsync(HttpMethod.Post, botToken, method, body, ct);

    private async Task<JsonDocument> SendAsync(HttpMethod httpMethod, string botToken, string methodAndQuery, HttpContent? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(httpMethod, "https://api.telegram.org/bot" + botToken + "/" + methodAndQuery) { Content = body };
        SocialProxyAddress.Apply(request);
        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, ct);
        }
        catch (HttpRequestException ex)
        {
            var detail = ex.Message;
            for (var inner = ex.InnerException; inner is not null; inner = inner.InnerException)
            {
                if (!string.IsNullOrWhiteSpace(inner.Message) && !detail.Contains(inner.Message, StringComparison.Ordinal))
                    detail = detail + " " + inner.Message;
            }

            detail = detail.Replace(botToken, "…", StringComparison.Ordinal);
            throw new SocialPublishException("Telegram request to " + request.RequestUri?.GetLeftPart(UriPartial.Authority) + " failed. " + detail);
        }

        using (response)
        {
            var payload = await response.Content.ReadAsStringAsync(ct);
            JsonDocument? doc = null;
            try
            {
                doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(payload) ? "{}" : payload);
                var ok = doc.RootElement.TryGetProperty("ok", out var flag) && flag.ValueKind == JsonValueKind.True;
                if (response.IsSuccessStatusCode && ok) return doc;
            }
            catch (JsonException)
            {
            }

            doc?.Dispose();
            throw new SocialPublishException(TelegramApiError.Describe((int)response.StatusCode, payload, botToken));
        }
    }

    private static string? MessageId(JsonElement root)
    {
        if (!root.TryGetProperty("result", out var result)) return null;
        if (result.ValueKind == JsonValueKind.Array && result.GetArrayLength() > 0) result = result[0];
        if (result.TryGetProperty("message_id", out var id))
            return id.ValueKind == JsonValueKind.Number ? id.GetRawText() : id.GetString();
        return null;
    }
}
