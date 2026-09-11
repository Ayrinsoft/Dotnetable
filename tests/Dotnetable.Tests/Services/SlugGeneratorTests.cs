using Dotnetable.Application.Text;
using FluentAssertions;
using Xunit;

namespace Dotnetable.Tests.Services;

public class SlugGeneratorTests
{
    [Fact]
    public void Normalize_Collapses_Spaces_Into_Single_Dash()
    {
        SlugGenerator.Normalize("تجهیز و تعمیر").Should().Be("تجهیز-و-تعمیر");
    }

    [Fact]
    public void Normalize_Keeps_Persian_Letters_And_Digits_As_Is()
    {
        SlugGenerator.Normalize("تسمه نقاله ۱۲۳").Should().Be("تسمه-نقاله-۱۲۳");
    }

    [Fact]
    public void Normalize_Lowercases_Latin_Letters()
    {
        SlugGenerator.Normalize("Hello World").Should().Be("hello-world");
    }

    [Fact]
    public void Normalize_Collapses_Punctuation_Runs()
    {
        SlugGenerator.Normalize("A / B -- C!!").Should().Be("a-b-c");
    }

    [Fact]
    public void Normalize_Trims_Leading_And_Trailing_Dashes()
    {
        SlugGenerator.Normalize("  -Hello-  ").Should().Be("hello");
    }

    [Fact]
    public void Normalize_Returns_Fallback_For_AllPunctuation_Title()
    {
        SlugGenerator.Normalize("!!!").Should().Be("item");
    }

    [Fact]
    public void Normalize_Never_Exceeds_MaxLength()
    {
        var longTitle = string.Concat(Enumerable.Repeat("word ", 100));
        SlugGenerator.Normalize(longTitle, maxLength: 50).Length.Should().BeLessThanOrEqualTo(50);
    }

    [Fact]
    public void Normalize_Is_Idempotent()
    {
        var once = SlugGenerator.Normalize("Hello World");
        SlugGenerator.Normalize(once).Should().Be(once);
    }

    [Fact]
    public void MakeUnique_Returns_BaseSlug_When_No_Collision()
    {
        var used = new HashSet<string>();
        SlugGenerator.MakeUnique("news", used).Should().Be("news");
    }

    [Fact]
    public void MakeUnique_Appends_Incrementing_Suffix_On_Collision()
    {
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "news", "news-2" };
        SlugGenerator.MakeUnique("news", used).Should().Be("news-3");
    }

    [Fact]
    public void MakeUnique_Is_CaseInsensitive()
    {
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "News" };
        SlugGenerator.MakeUnique("news", used).Should().Be("news-2");
    }
}
