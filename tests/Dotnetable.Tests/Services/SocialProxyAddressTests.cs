using System.Net;
using Dotnetable.Application.Instagram;
using Dotnetable.Infrastructure.Social;
using FluentAssertions;
using Xunit;

namespace Dotnetable.Tests.Services;

public class SocialProxyAddressTests
{
    [Fact]
    public void Wrap_PutsTheRealUrlInOnePathSegment()
    {
        var wrapped = SocialProxyAddress.Wrap(
            "https://worker-blue-v2.xapi.workers.dev/",
            new Uri("https://api.x.com/2/users/me?user.fields=username"));

        wrapped.Host.Should().Be("worker-blue-v2.xapi.workers.dev");
        wrapped.Query.Should().BeEmpty();
        wrapped.AbsolutePath.Should().StartWith("/proxy/b64/");
        Decode(wrapped.AbsolutePath["/proxy/b64/".Length..])
            .Should().Be("https://api.x.com/2/users/me?user.fields=username");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Normalize_Empty_IsDirect(string value) =>
        SocialProxyAddress.NormalizeBase(value).Should().BeNull();

    [Fact]
    public void Normalize_KeepsOnlyTheHttpsOrigin() =>
        SocialProxyAddress.NormalizeBase("https://worker-blue-v2.xapi.workers.dev/proxy/ignored")
            .Should().Be("https://worker-blue-v2.xapi.workers.dev");

    [Fact]
    public void Normalize_RejectsHttp()
    {
        var act = () => SocialProxyAddress.NormalizeBase("http://worker-blue-v2.xapi.workers.dev/");
        act.Should().Throw<SocialPublishException>();
    }

    [Fact]
    public async Task Handler_RewritesUri_AndKeepsAuthorization()
    {
        var inner = new CaptureHandler();
        var handler = new SocialProxyHandler { InnerHandler = inner };
        using var client = new HttpClient(handler);
        var previous = SocialProxyAmbient.Current;
        SocialProxyAmbient.Set(new SocialProxyAmbient.State("https://worker-blue-v2.xapi.workers.dev", "secret"));
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.x.com/2/tweets");
            request.Headers.TryAddWithoutValidation("Authorization", "OAuth oauth_signature=\"abc\"");
            request.Content = new StringContent("{\"text\":\"hi\"}");
            await client.SendAsync(request);
        }
        finally
        {
            SocialProxyAmbient.Set(previous);
        }

        inner.Uri!.Host.Should().Be("worker-blue-v2.xapi.workers.dev");
        inner.Authorization.Should().Be("OAuth oauth_signature=\"abc\"");
        inner.ProxyKey.Should().Be("secret");
        Decode(inner.Uri.AbsolutePath["/proxy/b64/".Length..]).Should().Be("https://api.x.com/2/tweets");
    }

    [Fact]
    public async Task Handler_WithoutScope_CallsTheOfficialHost()
    {
        var inner = new CaptureHandler();
        var handler = new SocialProxyHandler { InnerHandler = inner };
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://graph.facebook.com/v21.0/") };
        var previous = SocialProxyAmbient.Current;
        SocialProxyAmbient.Set(null);
        try
        {
            await client.GetAsync("123?fields=username&access_token=abc");
        }
        finally
        {
            SocialProxyAmbient.Set(previous);
        }

        inner.Uri!.Host.Should().Be("graph.facebook.com");
        inner.Uri.PathAndQuery.Should().Contain("access_token=abc");
    }

    private static string Decode(string segment)
    {
        var pad = (4 - (segment.Length % 4)) % 4;
        var b64 = segment.Replace('-', '+').Replace('_', '/') + new string('=', pad);
        return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(b64));
    }

    private sealed class CaptureHandler : HttpMessageHandler
    {
        public Uri? Uri { get; private set; }
        public string? Authorization { get; private set; }
        public string? ProxyKey { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Uri = request.RequestUri;
            Authorization = request.Headers.TryGetValues("Authorization", out var auth) ? auth.Single() : null;
            ProxyKey = request.Headers.TryGetValues(SocialProxyAddress.KeyHeader, out var key) ? key.Single() : null;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });
        }
    }
}
