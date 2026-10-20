using Dotnetable.Web.Services;
using FluentAssertions;
using Xunit;

namespace Dotnetable.Tests.Services;

public class BotMediaTests
{
    private const string Photo = "https://cdn.example.com/0123456789abcdef0123456789abcdef.webp";
    private const string Thumb = "https://cdn.example.com/0123456789abcdef0123456789abcdef_thumb.webp";
    private const string Icon = "https://cdn.example.com/aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.png";

    [Theory]
    [InlineData("Mozilla/5.0 (Linux; Android 6.0.1; Nexus 5X) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Mobile Safari/537.36 (compatible; GoogleOther)")]
    [InlineData("Mozilla/5.0 (compatible; Google-InspectionTool/1.0)")]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36 (compatible; meta-externalagent/1.1)")]
    [InlineData("Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)")]
    [InlineData("")]
    public void Crawlers_Are_Not_Treated_As_People(string userAgent)
    {
        BotDetector.Classify(userAgent).Should().Be(BotKind.Crawler);
    }

    [Fact]
    public void A_Normal_Browser_Is_Not_A_Bot()
    {
        const string chrome = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36";
        BotDetector.Classify(chrome).Should().Be(BotKind.None);
    }

    [Fact]
    public void WhatsApp_In_App_Browser_Is_A_Person()
    {
        const string ua = "Mozilla/5.0 (Linux; Android 10; SM-G973F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Mobile Safari/537.36 WhatsApp/2.21.16.20";
        BotDetector.Classify(ua).Should().Be(BotKind.None);
    }

    [Fact]
    public void WhatsApp_Preview_Fetcher_Keeps_The_Share_Card_Path()
    {
        BotDetector.Classify("WhatsApp/2.23.20.0").Should().Be(BotKind.LinkPreview);
    }

    [Fact]
    public void Crawler_Html_Has_No_Address_Of_An_Uploaded_Image()
    {
        var html = $$"""
            <!DOCTYPE html>
            <html><head>
              <link rel="icon" href="{{Icon}}" />
              <link rel="stylesheet" href="/css/site.css" />
              <link rel="preload" as="image" href="{{Thumb}}" />
              <meta property="og:image" content="{{Photo}}" />
              <style>.hero { background-image: url('{{Photo}}'); }</style>
            </head><body>
              <img src="{{MediaHtml.Placeholder}}" data-src="{{Thumb}}" data-full="{{Photo}}" alt="Cover" />
              <div data-bg="{{Thumb}}"></div>
              <img src="https://images.example.com/not-ours.jpg" alt="Partner" />
              <script>var cover = "{{Photo}}"; var ok = 1 && 2;</script>
            </body></html>
            """;

        var stripped = MediaHtml.StripForBots(html, keepShareImage: false);

        stripped.Should().NotContain(Photo);
        stripped.Should().NotContain(Thumb);
        stripped.Should().NotContain("0123456789abcdef0123456789abcdef");
        stripped.Should().Contain(Icon);
        stripped.Should().Contain("not-ours.jpg");
        stripped.Should().Contain("/css/site.css");
        stripped.Should().Contain("1 && 2");
        stripped.Should().NotContain("data-src");
    }

    [Fact]
    public void Link_Preview_Keeps_Only_The_Share_Image()
    {
        var html = $"""
            <!DOCTYPE html>
            <html><head><meta property="og:image" content="{Photo}" /></head>
            <body><img src="{Thumb}" alt="Cover" /></body></html>
            """;

        var stripped = MediaHtml.StripForBots(html, keepShareImage: true);

        stripped.Should().Contain(Photo);
        stripped.Should().NotContain(Thumb);
        stripped.Should().NotContain("<img");
    }
}
