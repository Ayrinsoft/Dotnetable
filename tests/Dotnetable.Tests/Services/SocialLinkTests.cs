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
            .Should().Be("A short note\nhttps://tvlm.ir/Blog/Post/cafe");
    }

    [Fact]
    public void Join_DoesNotRepeatAnAddressAlreadyAtTheEnd()
    {
        SocialCaption.Join("Note\nhttps://tvlm.ir/p/cafe", "https://tvlm.ir/p/cafe")
            .Should().Be("Note\nhttps://tvlm.ir/p/cafe");
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
            .Should().Be("note\nhttps://tvlm.ir/p/cafe\n#آیتم_1 #آیتم_2");
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
