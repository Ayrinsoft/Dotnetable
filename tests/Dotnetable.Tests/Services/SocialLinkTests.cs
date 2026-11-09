using Dotnetable.Application.Instagram;
using Dotnetable.Application.Social;
using Dotnetable.Infrastructure.Social;
using FluentAssertions;
using Xunit;

namespace Dotnetable.Tests.Services;

public class SocialLinkTests
{
    [Fact]
    public void Join_PutsTheAddressOnTheNextLine()
    {
        SocialCaption.Join("A short note", "https://tvlm.ir/Blog/Post/cafe")
            .Should().Be("A short note\n\nhttps://tvlm.ir/Blog/Post/cafe");
    }

    [Fact]
    public void Join_DoesNotRepeatAnAddressAlreadyAtTheEnd()
    {
        SocialCaption.Join("Note\nhttps://tvlm.ir/p/cafe", "https://tvlm.ir/p/cafe")
            .Should().Be("Note\n\nhttps://tvlm.ir/p/cafe");
    }

    [Fact]
    public void Fit_KeepsTheAddressWhenTheTextIsTooLong()
    {
        var url = "https://tvlm.ir/p/cafe";
        var text = new string('ا', 300);
        var fitted = SocialCaption.Fit(text, url, 280);
        fitted.Length.Should().BeLessThanOrEqualTo(280);
        fitted.Should().EndWith("\n" + url);
    }

    [Fact]
    public void Parts_NeverCutsTheAddress()
    {
        var url = "https://tvlm.ir/p/cafe";
        var parts = SocialCaption.Parts(new string('a', 50), url, 40);
        parts.Should().HaveCountGreaterThan(1);
        parts[^1].Should().EndWith(url);
        parts.Take(parts.Count - 1).Should().OnlyContain(part => !part.Contains(url, StringComparison.Ordinal));
        parts.Should().OnlyContain(part => part.Length <= 40);
    }

    [Fact]
    public void Build_UsesTheWebsiteAddressAndTheRealPostRoute()
    {
        SocialLinkPaths.Build("tvlm.ir", null, null, SocialLinkPaths.DefaultPost, "my post")
            .Should().Be("https://tvlm.ir/Blog/Post/my%20post");
    }

    [Fact]
    public void Build_UsesAShortPathAndAnOverrideAddress()
    {
        SocialLinkPaths.Build("old.example", "https://tvlm.ir/fa", "p/{slug}", SocialLinkPaths.DefaultPost, "cafe")
            .Should().Be("https://tvlm.ir/fa/p/cafe");
    }

    [Theory]
    [InlineData("{slug}")]
    [InlineData("p/{slug}/extra")]
    [InlineData("../p/{slug}")]
    [InlineData("p/{slug}?x=1")]
    public void NormalizePath_RejectsAPathThatCannotBeRouted(string raw)
    {
        var act = () => SocialLinkPaths.NormalizePath(raw);
        act.Should().Throw<SocialPublishException>();
    }

    [Fact]
    public void Redirect_SendsAShortPostPathToTheStorefrontRoute()
    {
        var map = new SocialLinkMap { PostPath = "p/{slug}" };
        SocialLinkPaths.Redirect("/p/cafe", map).Should().Be("/Blog/Post/cafe");
    }

    [Fact]
    public void Hashtags_TurnCommaSeparatedKeywordsIntoTags()
    {
        SocialHashtags.Split("آیتم 1، آیتم 2").Should().Equal("آیتم 1", "آیتم 2");
        SocialHashtags.Line(["آیتم 1", "آیتم 2"]).Should().Be("#آیتم_1 #آیتم_2");
    }

    [Fact]
    public void Join_PutsHashtagsAfterTheLink()
    {
        SocialCaption.Join("note", "https://tvlm.ir/p/cafe", "#آیتم_1 #آیتم_2")
            .Should().Be("note\n\nhttps://tvlm.ir/p/cafe\n\n#آیتم_1 #آیتم_2");
    }

    [Theory]
    [InlineData("/page/about", "https://tvlm.ir/page/about")]
    [InlineData("Blog/Post/cafe", "https://tvlm.ir/Blog/Post/cafe")]
    [InlineData("/Shop/Product/mug?ref=1", "https://tvlm.ir/Shop/Product/mug?ref=1")]
    [InlineData("https://other.example/cook/1", "https://other.example/cook/1")]
    public void RequireTarget_AcceptsAnyPageOrAnyHttpAddress(string raw, string expected)
    {
        ShortLinkService.RequireTarget(raw, "https://tvlm.ir").Should().Be(expected);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("//evil.example/phish")]
    public void RequireTarget_RejectsANonHttpAddress(string raw)
    {
        var act = () => ShortLinkService.RequireTarget(raw, "https://tvlm.ir");
        act.Should().Throw<SocialPublishException>();
    }

    [Fact]
    public void TelegramHtml_TurnsTheAddressIntoTheWordHere()
    {
        var plain = SocialCaption.Join("note", "https://tvlm.ir/l/LK2873H32", "#cafe");
        SocialCaption.TelegramHtml(plain, "https://tvlm.ir/l/LK2873H32")
            .Should().Be("note\n\nبرای اطلاعات بیشتر <a href=\"https://tvlm.ir/l/LK2873H32\">اینجا</a> کلیک کنید\n\n#cafe");
    }

    [Fact]
    public void TelegramHtml_EscapesCaptionMarkup()
    {
        SocialCaption.TelegramHtml("a < b & c", null)
            .Should().Be("a &lt; b &amp; c");
    }

    [Fact]
    public void Redirect_LeavesTheRealRouteAlone()
    {
        var map = new SocialLinkMap();
        SocialLinkPaths.Redirect("/Blog/Post/cafe", map).Should().BeNull();
        SocialLinkPaths.Redirect("/page/about", map).Should().BeNull();
        SocialLinkPaths.Redirect("/Shop/Product/mug", map).Should().BeNull();
    }
}
