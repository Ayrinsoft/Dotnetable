using Dotnetable.Application.DTOs;
using Dotnetable.Application.VisitCards;
using FluentAssertions;
using Xunit;

namespace Dotnetable.Tests.VisitCards;

public class VisitCardRulesTests
{
    [Theory]
    [InlineData("aliu", "aliu")]
    [InlineData(" Ali-U ", "ali-u")]
    [InlineData("A1", "a1")]
    public void Slug_accepts_a_short_name(string raw, string expected)
    {
        VisitCardRules.TryNormalizeSlug(raw, out var slug, out var error).Should().BeTrue();
        error.Should().BeNull();
        slug.Should().Be(expected);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("1ali")]
    [InlineData("ali u")]
    [InlineData("api")]
    public void Slug_rejects_short_reserved_or_spaced_names(string raw)
    {
        VisitCardRules.TryNormalizeSlug(raw, out var slug, out var error).Should().BeFalse();
        slug.Should().BeNull();
        error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Empty_slug_means_clear_the_link()
    {
        VisitCardRules.TryNormalizeSlug("  ", out var slug, out var error).Should().BeTrue();
        slug.Should().BeNull();
        error.Should().BeNull();
    }

    [Fact]
    public void Iranian_mobiles_share_one_key()
    {
        var key = VisitCardRules.MobileKey("۰۹۱۲۳۴۵۶۷۸۹");
        VisitCardRules.MobileKey("+989123456789").Should().Be(key);
        VisitCardRules.MobileKey("9123456789").Should().Be(key);
    }

    [Fact]
    public void Contact_requires_a_phone_or_an_email()
    {
        VisitCardRules.ValidateContact("Ali", "", "", "hello", out _, out _, out _, out _).Should().Be("contact");
        VisitCardRules.ValidateContact("Ali", "09120000000", "", "hello", out _, out var mobile, out _, out _)
            .Should().BeNull();
        mobile.Should().Be("09120000000");
        VisitCardRules.ValidateContact("Ali", "", "a@b.co", "hello", out _, out _, out var email, out _)
            .Should().BeNull();
        email.Should().Be("a@b.co");
    }

    [Fact]
    public void Sheba_check_rejects_a_short_value_and_accepts_a_valid_one()
    {
        VisitCardRules.IsValidIranSheba("IR123").Should().BeFalse();
        var card = new AuthorVisitCardEdit { Bank1Sheba = "820540102680020817909002", Bank1HolderName = "Ali" };
        VisitCardRules.PrepareBanks(card).Should().BeNull();
        card.Bank1Sheba.Should().Be("IR820540102680020817909002");
    }

    [Fact]
    public void Bank_without_a_holder_is_rejected()
    {
        var card = new AuthorVisitCardEdit { Bank1CardNumber = "6037991199501234" };
        VisitCardRules.PrepareBanks(card).Should().NotBeNull();
    }

    [Fact]
    public void Empty_translation_is_dropped()
    {
        var json = VisitCardRules.SerializeTranslations(
        [
            new VisitCardTranslation { Lang = "en" },
            new VisitCardTranslation { Lang = "de", FullName = "Ali" },
            new VisitCardTranslation { Lang = "xx", FullName = "Nope" },
        ]);
        var rows = VisitCardRules.ParseTranslations(json);
        rows.Should().ContainSingle();
        rows[0].Lang.Should().Be("de");
        rows[0].FullName.Should().Be("Ali");
    }

    [Fact]
    public void Vcf_contains_the_name_and_both_ways_to_reach_the_person()
    {
        var vcf = VisitCardRules.BuildVcf("Ali, A", null, "Developer", "09120000000", null, "a@b.co", null, null, null);
        vcf.Should().Contain("BEGIN:VCARD");
        vcf.Should().Contain("FN:Ali\\, A");
        vcf.Should().Contain("TEL;TYPE=CELL:09120000000");
        vcf.Should().Contain("EMAIL:a@b.co");
    }

    [Fact]
    public void Repeated_reason_is_not_doubled()
    {
        VisitCardRules.AppendReason("call me", "call me").Should().Be("call me");
        VisitCardRules.AppendReason("call me", "about the site").Should().Be("call me\nabout the site");
    }

    [Fact]
    public void Email_signature_seed_points_at_the_card_and_whatsapp()
    {
        var json = EmailSignatureSeed.ToJson(new AuthorVisitCardEdit
        {
            Slug = "aliu",
            FullName = "Ali",
            WhatsApp = "09120000000",
            Website = "https://example.com",
            CardColor = "not-a-color",
            SocialLinks = [new AuthorSocialLink { Network = "instagram", Url = "https://instagram.com/ali" }],
        }, "https://cdn.example/logo.png", "example.com");

        json.Should().Contain("https://example.com/c/aliu");
        json.Should().Contain("https://wa.me/989120000000");
        json.Should().Contain("https://instagram.com/ali");
        json.Should().Contain("https://cdn.example/logo.png");
        json.Should().Contain("#5F41FB");
        json.Should().NotContain("<");
    }
}
