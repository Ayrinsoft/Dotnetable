using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace Dotnetable.Tests.Services;

public class HumanVerificationServiceTests : IDisposable
{
    private readonly IMemoryCache _cache;
    private readonly Mock<IAppSettingsStore> _settingsMock;
    private readonly HumanVerificationService _service;

    public HumanVerificationServiceTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());

        _settingsMock = new Mock<IAppSettingsStore>();
        _settingsMock.Setup(s => s.Security).Returns(new SecuritySettings());

        _service = new HumanVerificationService(_settingsMock.Object, _cache);
    }

    // ── TurnstileEnabled / TurnstileSiteKey ────────────────────────────────────

    [Fact]
    public void TurnstileEnabled_NoKeysConfigured_ReturnsFalse()
    {
        _settingsMock.Setup(s => s.Security).Returns(new SecuritySettings());

        _service.TurnstileEnabled.Should().BeFalse();
    }

    [Fact]
    public void TurnstileEnabled_BothKeysConfigured_ReturnsTrue()
    {
        _settingsMock.Setup(s => s.Security).Returns(new SecuritySettings
        {
            TurnstileSiteKey = "site-key",
            TurnstileSecretKey = "secret-key",
            CaptchaMode = CaptchaMode.Auto
        });

        _service.TurnstileEnabled.Should().BeTrue();
    }

    [Fact]
    public void TurnstileEnabled_MathModeForced_ReturnsFalse()
    {
        _settingsMock.Setup(s => s.Security).Returns(new SecuritySettings
        {
            TurnstileSiteKey = "site-key",
            TurnstileSecretKey = "secret-key",
            CaptchaMode = CaptchaMode.Math
        });

        _service.TurnstileEnabled.Should().BeFalse();
    }

    [Fact]
    public void TurnstileSiteKey_WhenDisabled_ReturnsNull()
    {
        _settingsMock.Setup(s => s.Security).Returns(new SecuritySettings()); // no keys

        _service.TurnstileSiteKey.Should().BeNull();
    }

    [Fact]
    public void TurnstileSiteKey_WhenEnabled_ReturnsSiteKey()
    {
        _settingsMock.Setup(s => s.Security).Returns(new SecuritySettings
        {
            TurnstileSiteKey = "my-site-key",
            TurnstileSecretKey = "my-secret",
            CaptchaMode = CaptchaMode.Auto
        });

        _service.TurnstileSiteKey.Should().Be("my-site-key");
    }

    // ── CreateMathChallenge ────────────────────────────────────────────────────

    [Fact]
    public void CreateMathChallenge_ReturnsNonEmptyTokenAndSvg()
    {
        var challenge = _service.CreateMathChallenge();

        challenge.Token.Should().NotBeNullOrWhiteSpace();
        challenge.Svg.Should().NotBeNullOrWhiteSpace();
        challenge.Svg.Should().StartWith("<svg");
    }

    [Fact]
    public void CreateMathChallenge_EachCallReturnsUniqueToken()
    {
        var t1 = _service.CreateMathChallenge().Token;
        var t2 = _service.CreateMathChallenge().Token;

        t1.Should().NotBe(t2);
    }

    [Fact]
    public void CreateMathChallenge_StoresAnswerInCache()
    {
        var challenge = _service.CreateMathChallenge();

        _cache.TryGetValue($"mathcaptcha:{challenge.Token}", out int answer).Should().BeTrue();
        answer.Should().BeInRange(0, 18); // max: 9+9=18
    }

    // ── ValidateMath ───────────────────────────────────────────────────────────

    [Fact]
    public void ValidateMath_CorrectAnswer_ReturnsTrue()
    {
        var challenge = _service.CreateMathChallenge();
        _cache.TryGetValue($"mathcaptcha:{challenge.Token}", out int expected);

        _service.ValidateMath(challenge.Token, expected.ToString()).Should().BeTrue();
    }

    [Fact]
    public void ValidateMath_WrongAnswer_ReturnsFalse()
    {
        var challenge = _service.CreateMathChallenge();
        _cache.TryGetValue($"mathcaptcha:{challenge.Token}", out int expected);
        var wrong = (expected + 7).ToString(); // guaranteed different

        _service.ValidateMath(challenge.Token, wrong).Should().BeFalse();
    }

    [Fact]
    public void ValidateMath_ConsumedToken_SecondAttemptFails()
    {
        var challenge = _service.CreateMathChallenge();
        _cache.TryGetValue($"mathcaptcha:{challenge.Token}", out int expected);

        _service.ValidateMath(challenge.Token, expected.ToString()); // consumes token
        _service.ValidateMath(challenge.Token, expected.ToString()).Should().BeFalse(); // second attempt
    }

    [Fact]
    public void ValidateMath_NullToken_ReturnsFalse()
    {
        _service.ValidateMath(null, "5").Should().BeFalse();
    }

    [Fact]
    public void ValidateMath_NullAnswer_ReturnsFalse()
    {
        var challenge = _service.CreateMathChallenge();
        _service.ValidateMath(challenge.Token, null).Should().BeFalse();
    }

    [Fact]
    public void ValidateMath_WhitespaceToken_ReturnsFalse()
    {
        _service.ValidateMath("   ", "5").Should().BeFalse();
    }

    [Fact]
    public void ValidateMath_NonNumericAnswer_ReturnsFalse()
    {
        var challenge = _service.CreateMathChallenge();
        _service.ValidateMath(challenge.Token, "abc").Should().BeFalse();
    }

    [Fact]
    public void ValidateMath_AnswerWithLeadingWhitespace_TrimsAndValidates()
    {
        var challenge = _service.CreateMathChallenge();
        _cache.TryGetValue($"mathcaptcha:{challenge.Token}", out int expected);

        _service.ValidateMath(challenge.Token, $"  {expected}  ").Should().BeTrue();
    }

    public void Dispose() => _cache.Dispose();
}
