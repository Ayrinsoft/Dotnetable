using Dotnetable.Application.Text;
using FluentAssertions;
using Xunit;

namespace Dotnetable.Tests.Services;

public class FileNameSanitizerTests
{
    [Fact]
    public void Normalize_Replaces_Spaces_With_Dashes_And_Keeps_Extension()
    {
        FileNameSanitizer.Normalize("my report final.txt").Should().Be("my-report-final.txt");
    }

    [Fact]
    public void Normalize_Lowercases_The_Extension_Only()
    {
        FileNameSanitizer.Normalize("Photo.JPG").Should().Be("Photo.jpg");
    }

    [Fact]
    public void Normalize_Keeps_NonLatin_Letters_As_Is()
    {
        FileNameSanitizer.Normalize("گزارش فروش.pdf").Should().Be("گزارش-فروش.pdf");
    }

    [Fact]
    public void Normalize_Collapses_Punctuation_Runs()
    {
        FileNameSanitizer.Normalize("a###b***c.png").Should().Be("a-b-c.png");
    }

    [Fact]
    public void Normalize_Strips_Path_Separators_Rather_Than_Preserving_Them_As_Segments()
    {
        // Path.GetFileNameWithoutExtension treats "/" as a directory separator, so anything before
        // the last one is dropped entirely — a defense against a filename smuggling path components
        // (e.g. "../../etc/passwd") through this field, not something callers should rely on for
        // an ordinary uploaded filename (browsers never put "/" in one).
        FileNameSanitizer.Normalize("a/b/c.png").Should().Be("c.png");
    }

    [Fact]
    public void Normalize_Handles_No_Extension()
    {
        FileNameSanitizer.Normalize("README").Should().Be("README");
    }

    [Fact]
    public void Normalize_Returns_Fallback_For_Empty_Name()
    {
        FileNameSanitizer.Normalize("   ").Should().Be("file");
    }

    [Fact]
    public void Normalize_Never_Exceeds_MaxLength()
    {
        var longName = new string('a', 500) + ".jpg";
        FileNameSanitizer.Normalize(longName, maxLength: 50).Length.Should().BeLessThanOrEqualTo(50);
    }

    [Fact]
    public void Normalize_Is_Idempotent()
    {
        var once = FileNameSanitizer.Normalize("My File v2.final.jpg");
        FileNameSanitizer.Normalize(once).Should().Be(once);
    }
}
