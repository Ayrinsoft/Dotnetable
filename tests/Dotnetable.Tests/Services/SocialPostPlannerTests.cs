using Dotnetable.Application.DTOs;
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
    }
}
