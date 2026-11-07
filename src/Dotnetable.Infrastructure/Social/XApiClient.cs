using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Dotnetable.Application.Instagram;

namespace Dotnetable.Infrastructure.Social;

public sealed class XApiClient : IXApi
{
    private const string Root = "https://api.x.com/2";
    private readonly HttpClient _http;

    public XApiClient(HttpClient http)
    {
        _http = http;
        _http.Timeout = TimeSpan.FromMinutes(3);
    }

    public async Task<string> GetUsernameAsync(XCredentials credentials, CancellationToken ct = default)
    {
        var query = new[] { new KeyValuePair<string, string>("user.fields", "username") };
        using var doc = await SendAsync(HttpMethod.Get, Root + "/users/me", credentials, query, null, ct);
        return doc.RootElement.TryGetProperty("data", out var data) && data.TryGetProperty("username", out var name)
            ? name.GetString() ?? ""
            : "";
    }

    public Task<string> UploadImageAsync(XCredentials credentials, byte[] jpeg, CancellationToken ct = default) =>
        UploadAsync(credentials, jpeg, "image/jpeg", "photo.jpg", "tweet_image", ct);

    public Task<string> UploadVideoAsync(XCredentials credentials, byte[] content, string mime, CancellationToken ct = default) =>
        UploadAsync(credentials, content, string.IsNullOrWhiteSpace(mime) ? "video/mp4" : mime, "clip.mp4", "tweet_video", ct);

    public async Task<string> TweetAsync(XCredentials credentials, string? text, IReadOnlyList<string> mediaIds, string? replyTo, CancellationToken ct = default)
    {
        var body = new Dictionary<string, object>();
        if (!string.IsNullOrWhiteSpace(text)) body["text"] = text;
        if (mediaIds.Count > 0) body["media"] = new Dictionary<string, object> { ["media_ids"] = mediaIds };
        if (!string.IsNullOrWhiteSpace(replyTo))
            body["reply"] = new Dictionary<string, string> { ["in_reply_to_tweet_id"] = replyTo };

        using var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        using var doc = await SendAsync(HttpMethod.Post, Root + "/tweets", credentials, null, content, ct);
        if (doc.RootElement.TryGetProperty("data", out var data) && data.TryGetProperty("id", out var id))
            return id.ValueKind == JsonValueKind.String ? id.GetString() ?? "" : id.GetRawText().Trim('"');
        throw new SocialPublishException("X did not return a post id.");
    }

    private async Task<string> UploadAsync(XCredentials credentials, byte[] content, string mime, string fileName, string category, CancellationToken ct)
    {
        var initBody = JsonSerializer.Serialize(new
        {
            media_type = mime,
            total_bytes = content.Length,
            media_category = category,
        });
        using var init = new StringContent(initBody, Encoding.UTF8, "application/json");
        using var started = await SendAsync(HttpMethod.Post, Root + "/media/upload/initialize", credentials, null, init, ct);
        var mediaId = ReadId(started.RootElement);
        const int chunk = 4 * 1024 * 1024;
        var segment = 0;
        for (var offset = 0; offset < content.Length; offset += chunk)
        {
            var length = Math.Min(chunk, content.Length - offset);
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(segment.ToString(System.Globalization.CultureInfo.InvariantCulture)), "segment_index");
            var file = new ByteArrayContent(content, offset, length);
            file.Headers.ContentType = new MediaTypeHeaderValue(mime);
            form.Add(file, "media", fileName);
            using var appended = await SendAsync(HttpMethod.Post, Root + "/media/upload/" + Uri.EscapeDataString(mediaId) + "/append", credentials, null, form, ct);
            segment++;
        }

        using var finalized = await SendAsync(HttpMethod.Post, Root + "/media/upload/" + Uri.EscapeDataString(mediaId) + "/finalize", credentials, null, null, ct);
        await WaitAsync(credentials, mediaId, finalized.RootElement, ct);
        return mediaId;
    }

    private async Task WaitAsync(XCredentials credentials, string mediaId, JsonElement finalized, CancellationToken ct)
    {
        if (!NeedsWait(finalized)) return;
        var wait = WaitSeconds(finalized);
        var query = new[]
        {
            new KeyValuePair<string, string>("command", "STATUS"),
            new KeyValuePair<string, string>("media_id", mediaId),
        };
        for (var attempt = 0; attempt < 30; attempt++)
        {
            await Task.Delay(TimeSpan.FromSeconds(wait), ct);
            using var status = await SendAsync(HttpMethod.Get, Root + "/media/upload", credentials, query, null, ct);
            var state = StateOf(status.RootElement);
            if (string.Equals(state, "succeeded", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(state)) return;
            if (string.Equals(state, "failed", StringComparison.OrdinalIgnoreCase))
                throw new SocialPublishException("X could not process that file.");
            wait = WaitSeconds(status.RootElement);
        }

        throw new SocialPublishException("X is still processing the file. Try again in a moment.");
    }

    private static int WaitSeconds(JsonElement root)
    {
        if (root.TryGetProperty("data", out var data)
            && data.TryGetProperty("processing_info", out var info)
            && info.TryGetProperty("check_after_secs", out var seconds)
            && seconds.TryGetInt32(out var parsed))
            return Math.Clamp(parsed, 1, 10);
        return 2;
    }

    private static bool NeedsWait(JsonElement root) =>
        !string.IsNullOrEmpty(StateOf(root)) && !string.Equals(StateOf(root), "succeeded", StringComparison.OrdinalIgnoreCase);

    private static string? StateOf(JsonElement root) =>
        root.TryGetProperty("data", out var data)
        && data.TryGetProperty("processing_info", out var info)
        && info.TryGetProperty("state", out var state)
            ? state.GetString()
            : null;

    private static string ReadId(JsonElement root)
    {
        if (root.TryGetProperty("data", out var data) && data.TryGetProperty("id", out var id))
            return id.ValueKind == JsonValueKind.String ? id.GetString() ?? "" : id.GetRawText().Trim('"');
        throw new SocialPublishException("X did not accept the file.");
    }

    private async Task<JsonDocument> SendAsync(
        HttpMethod method, string url, XCredentials credentials, IReadOnlyList<KeyValuePair<string, string>>? query, HttpContent? body, CancellationToken ct)
    {
        var target = url;
        if (query is { Count: > 0 })
            target += "?" + string.Join("&", query.Select(pair => Uri.EscapeDataString(pair.Key) + "=" + Uri.EscapeDataString(pair.Value)));

        using var request = new HttpRequestMessage(method, target);
        request.Headers.TryAddWithoutValidation("Authorization", XOAuth1.Header(method.Method, url, credentials, query));
        request.Content = body;
        SocialProxyAddress.Apply(request);
        using var response = await _http.SendAsync(request, ct);
        var payload = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new SocialPublishException(ReadError(payload) ?? "X refused the request. Check the app has Read and Write, and that the access token was regenerated after that change.");
        try
        {
            return JsonDocument.Parse(string.IsNullOrWhiteSpace(payload) ? "{}" : payload);
        }
        catch (JsonException)
        {
            throw new SocialPublishException("X did not return a readable answer.");
        }
    }

    private static string? ReadError(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;
            if (root.TryGetProperty("detail", out var detail) && detail.GetString() is { Length: > 0 } text) return text;
            if (root.TryGetProperty("title", out var title) && title.GetString() is { Length: > 0 } named) return named;
            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array && errors.GetArrayLength() > 0)
            {
                var first = errors[0];
                if (first.TryGetProperty("detail", out var item) && item.GetString() is { Length: > 0 } message) return message;
                if (first.TryGetProperty("message", out var older) && older.GetString() is { Length: > 0 } old) return old;
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }
}
