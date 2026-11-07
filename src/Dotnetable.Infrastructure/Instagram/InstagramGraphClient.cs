using System.Net.Http.Headers;
using System.Text.Json;
using Dotnetable.Application.Instagram;
using Dotnetable.Infrastructure.Social;

namespace Dotnetable.Infrastructure.Instagram;

public sealed class InstagramGraphClient : IInstagramGraph
{
    public const string Version = "v21.0";

    private readonly HttpClient _http;

    public InstagramGraphClient(HttpClient http)
    {
        _http = http;
        if (_http.BaseAddress is null)
            _http.BaseAddress = new Uri($"https://graph.facebook.com/{Version}/");
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<string> GetUsernameAsync(string igUserId, string accessToken, CancellationToken ct = default)
    {
        using var doc = await GetAsync($"{Uri.EscapeDataString(igUserId)}?fields=username&access_token={Uri.EscapeDataString(accessToken)}", ct);
        return doc.RootElement.TryGetProperty("username", out var name) ? name.GetString() ?? "" : "";
    }

    public Task<string> CreateImageAsync(string igUserId, string accessToken, string imageUrl, string? caption, bool carouselItem, CancellationToken ct = default)
    {
        var fields = new Dictionary<string, string>
        {
            ["image_url"] = imageUrl,
            ["access_token"] = accessToken,
        };
        if (carouselItem) fields["is_carousel_item"] = "true";
        else if (!string.IsNullOrEmpty(caption)) fields["caption"] = caption;
        return CreateContainerAsync(igUserId, fields, ct);
    }

    public Task<string> CreateVideoAsync(string igUserId, string accessToken, string videoUrl, string? caption, bool carouselItem, string? coverUrl, CancellationToken ct = default)
    {
        var fields = new Dictionary<string, string>
        {
            ["video_url"] = videoUrl,
            ["media_type"] = carouselItem ? "VIDEO" : "REELS",
            ["access_token"] = accessToken,
        };
        if (carouselItem) fields["is_carousel_item"] = "true";
        else
        {
            fields["share_to_feed"] = "true";
            if (!string.IsNullOrEmpty(caption)) fields["caption"] = caption;
            if (!string.IsNullOrWhiteSpace(coverUrl)) fields["cover_url"] = coverUrl;
        }
        return CreateContainerAsync(igUserId, fields, ct);
    }

    public Task<string> CreateCarouselAsync(string igUserId, string accessToken, IReadOnlyList<string> children, string caption, CancellationToken ct = default)
    {
        var fields = new Dictionary<string, string>
        {
            ["media_type"] = "CAROUSEL",
            ["children"] = string.Join(',', children),
            ["caption"] = caption,
            ["access_token"] = accessToken,
        };
        return CreateContainerAsync(igUserId, fields, ct);
    }

    public async Task WaitUntilReadyAsync(string containerId, string accessToken, bool video, CancellationToken ct = default)
    {
        var attempts = video ? 90 : 30;
        for (var i = 0; i < attempts; i++)
        {
            using var doc = await GetAsync($"{Uri.EscapeDataString(containerId)}?fields=status_code&access_token={Uri.EscapeDataString(accessToken)}", ct);
            var status = doc.RootElement.TryGetProperty("status_code", out var code) ? code.GetString() : null;
            if (string.Equals(status, "FINISHED", StringComparison.OrdinalIgnoreCase)) return;
            if (string.Equals(status, "ERROR", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "EXPIRED", StringComparison.OrdinalIgnoreCase))
                throw new InstagramException("Instagram could not prepare the media. Check the file type, size, and that the address is a public https link.");
            await Task.Delay(TimeSpan.FromSeconds(2), ct);
        }

        throw new InstagramException("Instagram did not finish preparing the media in time.");
    }

    public async Task<string> PublishAsync(string igUserId, string accessToken, string creationId, CancellationToken ct = default)
    {
        var doc = await PostAsync($"{Uri.EscapeDataString(igUserId)}/media_publish", new Dictionary<string, string>
        {
            ["creation_id"] = creationId,
            ["access_token"] = accessToken,
        }, ct);
        return RequiredId(doc);
    }

    public async Task<string?> GetPermalinkAsync(string mediaId, string accessToken, CancellationToken ct = default)
    {
        try
        {
            using var doc = await GetAsync($"{Uri.EscapeDataString(mediaId)}?fields=permalink&access_token={Uri.EscapeDataString(accessToken)}", ct);
            return doc.RootElement.TryGetProperty("permalink", out var link) ? link.GetString() : null;
        }
        catch (InstagramException)
        {
            return null;
        }
    }

    private async Task<string> CreateContainerAsync(string igUserId, Dictionary<string, string> fields, CancellationToken ct)
    {
        var doc = await PostAsync($"{Uri.EscapeDataString(igUserId)}/media", fields, ct);
        return RequiredId(doc);
    }

    private async Task<JsonDocument> GetAsync(string relative, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(_http.BaseAddress!, relative));
        SocialProxyAddress.Apply(request);
        using var response = await _http.SendAsync(request, ct);
        return await ReadAsync(response, ct);
    }

    private async Task<JsonDocument> PostAsync(string relative, Dictionary<string, string> fields, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(_http.BaseAddress!, relative))
        {
            Content = new FormUrlEncodedContent(fields),
        };
        SocialProxyAddress.Apply(request);
        using var response = await _http.SendAsync(request, ct);
        return await ReadAsync(response, ct);
    }

    private static async Task<JsonDocument> ReadAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct);
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
        }
        catch (JsonException)
        {
            throw new InstagramException("Instagram returned a response that could not be read.");
        }

        doc.RootElement.TryGetProperty("error", out var error);
        if (!response.IsSuccessStatusCode || error.ValueKind == JsonValueKind.Object)
        {
            var message = error.ValueKind == JsonValueKind.Object && error.TryGetProperty("message", out var text)
                ? text.GetString()
                : null;
            doc.Dispose();
            throw new InstagramException(string.IsNullOrWhiteSpace(message) ? "Instagram refused the request." : message);
        }

        return doc;
    }

    private static string RequiredId(JsonDocument doc)
    {
        using (doc)
        {
            var id = doc.RootElement.TryGetProperty("id", out var value) ? value.GetString() : null;
            if (string.IsNullOrWhiteSpace(id))
                throw new InstagramException("Instagram did not return a media id.");
            return id;
        }
    }
}
