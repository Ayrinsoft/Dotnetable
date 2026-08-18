using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Asp.Versioning;
using Dotnetable.API.Versioning;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dotnetable.Tests.Api;

/// <summary>
/// Isolated host that uses the production versioning setup. Dummy controllers prove that
/// v1 and v2 share a path and that adding v2 does not take v1 offline.
/// </summary>
public sealed class ApiVersioningTests : IAsyncLifetime
{
    private WebApplication? _app;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(ApiVersioningTests).Assembly);
        builder.Services.AddDotnetableApiVersioning();

        _app = builder.Build();
        _app.UseDotnetableApiVersionHeaders();
        _app.MapControllers();
        await _app.StartAsync();
        _client = new HttpClient { BaseAddress = new Uri(_app.Urls.Single()) };
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        if (_app is not null)
            await _app.DisposeAsync();
    }

    [Fact]
    public async Task Missing_header_uses_v1()
    {
        var response = await _client.GetAsync("api/versioning-probe");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadVersionAsync(response)).Should().Be("1.0");
        response.Headers.GetValues("X-Api-Version").Should().Contain("1.0");
    }

    [Fact]
    public async Task Header_1_0_hits_v1_contract()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/versioning-probe");
        request.Headers.TryAddWithoutValidation("X-Api-Version", "1.0");

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadVersionAsync(response)).Should().Be("1.0");
    }

    [Fact]
    public async Task Header_2_0_hits_v2_on_the_same_path()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/versioning-probe");
        request.Headers.TryAddWithoutValidation("X-Api-Version", "2.0");

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadVersionAsync(response)).Should().Be("2.0");
    }

    [Fact]
    public async Task Adding_v2_does_not_break_v1()
    {
        using var v2 = new HttpRequestMessage(HttpMethod.Get, "api/versioning-probe");
        v2.Headers.TryAddWithoutValidation("X-Api-Version", "2.0");
        (await ReadVersionAsync(await _client.SendAsync(v2))).Should().Be("2.0");

        using var v1 = new HttpRequestMessage(HttpMethod.Get, "api/versioning-probe");
        v1.Headers.TryAddWithoutValidation("X-Api-Version", "1.0");
        var v1Response = await _client.SendAsync(v1);
        v1Response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadVersionAsync(v1Response)).Should().Be("1.0");
        v1Response.Headers.TryGetValues("api-supported-versions", out var supported).Should().BeTrue();
        string.Join(",", supported!).Should().Contain("1.0").And.Contain("2.0");
    }

    [Fact]
    public async Task Unknown_version_is_rejected_and_v1_still_works()
    {
        using var unknown = new HttpRequestMessage(HttpMethod.Get, "api/versioning-probe");
        unknown.Headers.TryAddWithoutValidation("X-Api-Version", "9.0");
        var rejected = await _client.SendAsync(unknown);
        rejected.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var v1 = new HttpRequestMessage(HttpMethod.Get, "api/versioning-probe");
        v1.Headers.TryAddWithoutValidation("X-Api-Version", "1.0");
        (await _client.SendAsync(v1)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("1.0")]
    [InlineData("2.0")]
    public async Task Version_neutral_route_works_without_or_with_a_supported_version(string? version)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/versioning-neutral");
        if (version is not null)
            request.Headers.TryAddWithoutValidation("X-Api-Version", version);

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadVersionAsync(response)).Should().Be("neutral");
    }

    private static async Task<string> ReadVersionAsync(HttpResponseMessage response)
    {
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("version").GetString() ?? "";
    }
}

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/versioning-probe")]
public sealed class VersioningProbeController : ControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public IActionResult GetV1() => Ok(new { version = "1.0" });

    [HttpGet]
    [MapToApiVersion("2.0")]
    public IActionResult GetV2() => Ok(new { version = "2.0" });
}

[ApiController]
[ApiVersionNeutral]
[Route("api/versioning-neutral")]
public sealed class VersioningNeutralController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { version = "neutral" });
}
