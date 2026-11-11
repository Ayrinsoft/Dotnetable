using Dotnetable.Application.DTOs;
using Dotnetable.Application.Instagram;
using Dotnetable.Application.Social;
using Dotnetable.Infrastructure.Social;
using FluentAssertions;
using Xunit;

namespace Dotnetable.Tests.Services;

public class SocialPostPlannerTests
{
    [Fact]
    public void X_GroupsFourPhotos_AndKeepsVideoAlone()
    {
        var chunks = SocialPostPlanner.PlanX([false, false, false, false, false, true, false]);
        chunks.Should().Equal(
            new XChunk(false, 0, 4),
            new XChunk(false, 4, 1),
            new XChunk(true, 5, 1),
            new XChunk(false, 6, 1));
    }

    [Fact]
    public void Telegram_SplitsAfterTen()
    {
        SocialPostPlanner.PlanTelegram(12).Should().Equal((0, 10), (10, 2));
        SocialPostPlanner.PlanTelegram(0).Should().BeEmpty();
    }

    [Fact]
    public void InstagramFilter_KeepsOnlyTickedSlides_AndMovesTheCover()
    {
        var request = new InstagramPublishRequest
        {
            CoverIndex = 2,
            InstagramAccountId = 7,
            ContentLanguageCode = "en",
            Items =
            [
                new InstagramMediaItem { Url = "a", ToInstagram = false },
                new InstagramMediaItem { Url = "b", ToInstagram = true },
                new InstagramMediaItem { Url = "c", ToInstagram = true },
            ],
        };

        var filtered = SocialPublishService.ForInstagram(request);
        filtered.Items.Select(item => item.Url).Should().Equal("b", "c");
        filtered.CoverIndex.Should().Be(1);
        filtered.InstagramAccountId.Should().Be(7);
        filtered.ContentLanguageCode.Should().Be("en");
    }

    [Fact]
    public void ChannelChoice_PrefersTheSingleLanguageMatch()
    {
        var accounts = new List<(int Id, string? LanguageCode)> { (1, "fa"), (2, "en"), (3, null) };
        SocialChannelChoice.Prefer(accounts, "en").Should().Be(2);
        SocialChannelChoice.Prefer(accounts, "de").Should().Be(1);
        SocialChannelChoice.Prefer([], "en").Should().Be(0);
    }

    [Fact]
    public void ChannelName_IsRequired()
    {
        var act = () => SocialChannelChoice.Read("  ", "fa");
        act.Should().Throw<SocialPublishException>();
        SocialChannelChoice.Read(" اخبار ", " fa ").Should().Be(("اخبار", "fa"));
        SocialChannelChoice.Format("", "@news", "fa").Should().Be("news · fa");
    }
}
