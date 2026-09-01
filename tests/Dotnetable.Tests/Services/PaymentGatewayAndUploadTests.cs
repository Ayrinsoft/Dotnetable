using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Security;
using Dotnetable.Infrastructure.Payments;
using FluentAssertions;
using Xunit;

namespace Dotnetable.Tests.Services;

/// <summary>
/// The two places where a small mistake is expensive: what a gateway is actually asked to charge,
/// and what a customer is allowed to upload.
/// </summary>
public class PaymentGatewayAndUploadTests
{
    private static readonly IHttpClientFactory HttpClients = new StubHttpClientFactory();

    // ── Amount denomination ─────────────────────────────────────────

    [Fact]
    public void IranianAggregators_DeclareTheUnitTheyActuallyBillIn()
    {
        // The shop prices in Toman. Most Iranian aggregators bill in Rial (×10) and PayPing does not,
        // so the unit is declared per provider — assuming one for all of them charges a customer ten
        // times too much or too little, and both directions are unrecoverable in practice.
        new ZarinpalGatewayProvider(HttpClients).AmountUnit.Should().Be(GatewayAmountUnit.Rial);
        new ZibalGatewayProvider(HttpClients).AmountUnit.Should().Be(GatewayAmountUnit.Rial);
        new IdPayGatewayProvider(HttpClients).AmountUnit.Should().Be(GatewayAmountUnit.Rial);
        new NextPayGatewayProvider(HttpClients).AmountUnit.Should().Be(GatewayAmountUnit.Rial);
        new PayIrGatewayProvider(HttpClients).AmountUnit.Should().Be(GatewayAmountUnit.Rial);

        new PayPingGatewayProvider(HttpClients).AmountUnit.Should().Be(GatewayAmountUnit.Major);
    }

    [Fact]
    public void InternationalGateways_DeclareTheirOwnUnits()
    {
        new StripeGatewayProvider(HttpClients).AmountUnit.Should().Be(GatewayAmountUnit.Minor);
        new PayPalGatewayProvider(HttpClients).AmountUnit.Should().Be(GatewayAmountUnit.Major);
    }

    [Fact]
    public void EveryProviderKey_IsUniqueSoTheRegistryCanResolveIt()
    {
        var providers = AllProviders();

        providers.Select(p => p.Key).Should().OnlyHaveUniqueItems();

        var registry = new PaymentGatewayProviderRegistry(providers);
        registry.Find("zarinpal").Should().BeOfType<ZarinpalGatewayProvider>("lookup is case-insensitive");
        registry.Find("NotAGateway").Should().BeNull();
        registry.Find(null).Should().BeNull();
    }

    [Fact]
    public void AGatewayWithNoCredentials_IsNeverConsideredConfigured()
    {
        var empty = new PaymentGatewayContext { Provider = "Zarinpal", SettingsJson = "{}" };

        // GetAvailableAsync filters on this, so an unconfigured gateway is never offered at checkout —
        // picking one would send the customer to a dead end mid-purchase.
        foreach (var provider in AllProviders().Where(p => p.Key != "PayIr"))
            provider.IsConfigured(empty).Should().BeFalse($"{provider.Key} has no credentials");
    }

    [Fact]
    public void MalformedSettingsJson_DoesNotThrow()
    {
        var broken = new PaymentGatewayContext { Provider = "Zarinpal", SettingsJson = "{ not json" };

        // A corrupt settings row must degrade to "not configured", not take the checkout page down.
        new ZarinpalGatewayProvider(HttpClients).IsConfigured(broken).Should().BeFalse();
    }

    // ── Content sanitisation ────────────────────────────────────────

    [Theory]
    [InlineData("<p>Safe <b>text</b></p>")]
    [InlineData("<a href=\"https://example.com\">link</a>")]
    [InlineData("<img src=\"https://example.com/a.png\" alt=\"a\" />")]
    public void Sanitizer_KeepsOrdinaryProductMarkup(string html) =>
        ContentSanitizer.Sanitize(html).Should().NotBeNullOrWhiteSpace();

    [Theory]
    [InlineData("<script>steal()</script>")]
    [InlineData("<img src=x onerror=\"steal()\">")]
    [InlineData("<a href=\"javascript:steal()\">click</a>")]
    [InlineData("<iframe src=\"https://evil.test/login\"></iframe>")]
    public void Sanitizer_RemovesAnythingExecutable(string html)
    {
        var sanitized = ContentSanitizer.Sanitize(html) ?? "";

        sanitized.Should().NotContain("steal");
        sanitized.Should().NotContain("javascript:");
        sanitized.Should().NotContain("evil.test");
    }

    [Fact]
    public void Sanitizer_KeepsAVideoEmbedFromAKnownHost()
    {
        var sanitized = ContentSanitizer.Sanitize(
            "<iframe src=\"https://www.youtube.com/embed/abc123\"></iframe>") ?? "";

        // Shops paste these constantly; dropping them would be a visible regression.
        sanitized.Should().Contain("youtube.com/embed/abc123");
    }

    [Fact]
    public void Sanitizer_PassesNullAndBlankThrough()
    {
        ContentSanitizer.Sanitize(null).Should().BeNull();
        ContentSanitizer.Sanitize("   ").Should().Be("   ");
    }

    // ── Page-size ceiling ───────────────────────────────────────────

    [Fact]
    public void GridQuery_ClampsAnOversizedPageSize()
    {
        // ?pageSize=10000000 on a public endpoint is otherwise an unauthenticated request to
        // materialise an entire table.
        new GridQuery { PageSize = 10_000_000 }.Take.Should().Be(GridQuery.MaxPageSize);
        new GridQuery { PageSize = 0 }.Take.Should().Be(10);
        new GridQuery { PageSize = 25 }.Take.Should().Be(25);

        GridQuery.ClampPageSize(10_000_000).Should().Be(GridQuery.MaxPageSize);
        GridQuery.ClampPageSize(-1, fallback: 8).Should().Be(8);
        GridQuery.ClampPageSize(12).Should().Be(12);
    }

    private static List<IPaymentGatewayProvider> AllProviders() =>
    [
        new ZarinpalGatewayProvider(HttpClients),
        new ZibalGatewayProvider(HttpClients),
        new IdPayGatewayProvider(HttpClients),
        new NextPayGatewayProvider(HttpClients),
        new PayIrGatewayProvider(HttpClients),
        new PayPingGatewayProvider(HttpClients),
        new StripeGatewayProvider(HttpClients),
        new PayPalGatewayProvider(HttpClients),
        new GenericRedirectGatewayProvider(HttpClients),
    ];

    /// <summary>These tests never make a request; the providers only need something to construct.</summary>
    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }
}
