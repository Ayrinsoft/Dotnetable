using Dotnetable.Application.Instagram;
using Dotnetable.Infrastructure.Social;
using FluentAssertions;
using Xunit;

namespace Dotnetable.Tests.Services;

public class TelegramChatIdTests
{
    [Theory]
    [InlineData("@trvlmg", "@trvlmg")]
    [InlineData("trvlmg", "@trvlmg")]
    [InlineData("t.me/trvlmg", "@trvlmg")]
    [InlineData("https://t.me/trvlmg/", "@trvlmg")]
    [InlineData("https://t.me/s/trvlmg", "@trvlmg")]
    [InlineData("https://t.me/trvlmg/12", "@trvlmg")]
    [InlineData("-1001234567890", "-1001234567890")]
    public void Normalize_PublicChannel(string raw, string expected) =>
        TelegramChatId.Normalize(raw).Should().Be(expected);

    [Fact]
    public void Normalize_InviteLink_IsRejected()
    {
        var act = () => TelegramChatId.Normalize("https://t.me/+AbCdEf");
        act.Should().Throw<SocialPublishException>();
    }

    [Fact]
    public void ProxyUnauthorized_NamesTheKey() =>
        TelegramApiError.Describe(401, "Unauthorized", "123:ABC").Should().Contain("proxy key");

    [Fact]
    public void ProxyFailure_NamesTheWorkerError() =>
        TelegramApiError.Describe(502, """{"error":"duplex"}""", "123:ABC").Should().Contain("duplex");

    [Fact]
    public void TelegramDescription_HidesTheToken() =>
        TelegramApiError.Describe(400, """{"ok":false,"description":"bad 123:ABC"}""", "123:ABC")
            .Should().Be("bad …");
}
