using Dotnetable.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace Dotnetable.Tests.Services;

public class ProductAgentParserTests
{
    [Fact]
    public void Parse_reads_variants_and_treats_omitted_keys_as_absent()
    {
        var doc = ProductAgentParser.Parse("""
            {
              "title": "کفش",
              "slug": "shoe",
              "variants": [
                {
                  "sku": "RED-42",
                  "title": "قرمز",
                  "price": 10,
                  "options": { "Color": "Red", "Size": "42" }
                }
              ]
            }
            """);

        doc.Has("title").Should().BeTrue();
        doc.Has("brand").Should().BeFalse();
        doc.Variants.Should().ContainSingle();
        doc.Variants![0].Sku.Should().Be("RED-42");
        doc.Variants[0].Price.Should().Be(10);
        doc.Variants[0].Options.Select(o => o.Attribute).Should().BeEquivalentTo(new[] { "Color", "Size" });
    }

    [Fact]
    public void Parse_rejects_empty_root()
    {
        var act = () => ProductAgentParser.Parse("{}");
        act.Should().Throw<ProductAgentException>();
    }
}
