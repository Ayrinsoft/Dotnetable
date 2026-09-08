using System.Text.Json;
using System.Xml.Linq;
using Dotnetable.Application.DTOs;
using Dotnetable.Infrastructure.Marketplace;
using FluentAssertions;
using Xunit;

namespace Dotnetable.Tests.Services;

/// <summary>
/// The feed providers are the half of the marketplace module that can be wrong silently: an engine
/// rejects a malformed document without telling the shop, so the shape is asserted here rather than
/// discovered in a seller panel.
/// </summary>
public class MarketplaceProviderTests
{
    private static readonly XNamespace G = "http://base.google.com/ns/1.0";

    private static MarketplaceChannelContext Context(string provider, object? settings = null) => new()
    {
        MarketplaceChannelID = 1,
        WebsiteID = 1,
        Provider = provider,
        Title = "Test Shop",
        SettingsJson = settings is null ? "{}" : JsonSerializer.Serialize(settings),
        SiteBaseUrl = "https://shop.example.com",
        CurrencyCode = "IRR",
    };

    private static MarketplaceProductItem Item(bool inStock = true, decimal price = 250000,
        decimal? compareAt = null) => new()
    {
        ProductID = 7,
        ProductVariantID = 9,
        Id = "SKU-1",
        Sku = "SKU-1",
        Barcode = "6260100110011",
        Title = "Blue Shirt",
        Description = "A shirt",
        Url = "https://shop.example.com/product/blue-shirt",
        ImageUrl = "https://cdn.example.com/blue.jpg",
        Price = price,
        CompareAtPrice = compareAt,
        InStock = inStock,
        AvailableQuantity = inStock ? 4 : 0,
        Brand = "Acme",
        CategoryName = "Shirts",
        RemoteCategoryID = "212",
        RemoteCategoryPath = "Apparel > Shirts",
    };

    [Fact]
    public void GoogleFeed_Emits_The_Required_Namespaced_Elements()
    {
        var provider = new GoogleShoppingFeedProvider();

        var feed = provider.BuildFeed(Context("GoogleShopping"), [Item()]);
        var item = XDocument.Parse(feed.Content).Descendants("item").Single();

        feed.ItemCount.Should().Be(1);
        item.Element(G + "id")!.Value.Should().Be("SKU-1");
        item.Element(G + "title")!.Value.Should().Be("Blue Shirt");
        item.Element(G + "link")!.Value.Should().Be("https://shop.example.com/product/blue-shirt");
        item.Element(G + "image_link")!.Value.Should().Be("https://cdn.example.com/blue.jpg");
        item.Element(G + "price")!.Value.Should().Be("250000 IRR");
        item.Element(G + "availability")!.Value.Should().Be("in stock");
        item.Element(G + "condition")!.Value.Should().Be("new");
        item.Element(G + "gtin")!.Value.Should().Be("6260100110011");
        item.Element(G + "google_product_category")!.Value.Should().Be("212");
    }

    [Fact]
    public void GoogleFeed_Sends_The_List_Price_And_The_Discount_Separately()
    {
        var provider = new GoogleShoppingFeedProvider();

        var feed = provider.BuildFeed(Context("GoogleShopping"),
            [Item(price: 200000, compareAt: 300000)]);
        var item = XDocument.Parse(feed.Content).Descendants("item").Single();

        // Google reads g:price as the pre-discount price and g:sale_price as what the customer pays.
        item.Element(G + "price")!.Value.Should().Be("300000 IRR");
        item.Element(G + "sale_price")!.Value.Should().Be("200000 IRR");
    }

    [Fact]
    public void GoogleFeed_Can_Drop_Out_Of_Stock_Products()
    {
        var provider = new GoogleShoppingFeedProvider();
        var settings = new GoogleShoppingFeedSettings { IncludeOutOfStock = false };

        var feed = provider.BuildFeed(Context("GoogleShopping", settings), [Item(inStock: false)]);

        feed.ItemCount.Should().Be(0);
        XDocument.Parse(feed.Content).Descendants("item").Should().BeEmpty();
    }

    [Fact]
    public void GenericFeed_Uses_The_Configured_Tag_Names_And_Availability_Words()
    {
        var provider = new GenericFeedProvider();
        var settings = new GenericFeedSettings
        {
            RootElement = "products",
            ItemElement = "product",
            InStockValue = "instock",
            OutOfStockValue = "outofstock",
            FieldMap = new Dictionary<string, string>
            {
                ["product_id"] = "{id}",
                ["title"] = "{title}",
                ["page_url"] = "{url}",
                ["price"] = "{price}",
                ["availability"] = "{availability}",
            },
        };

        var feed = provider.BuildFeed(Context("GenericFeed", settings), [Item(inStock: false)]);
        var product = XDocument.Parse(feed.Content).Root!.Elements("product").Single();

        product.Element("product_id")!.Value.Should().Be("SKU-1");
        product.Element("page_url")!.Value.Should().Be("https://shop.example.com/product/blue-shirt");
        product.Element("availability")!.Value.Should().Be("outofstock");
        product.Element("title")!.Value.Should().Be("Blue Shirt");
    }

    [Fact]
    public void GenericFeed_Divides_The_Price_For_Engines_Billing_In_Another_Unit()
    {
        var provider = new GenericFeedProvider();
        var settings = new GenericFeedSettings
        {
            PriceDivisor = 10,
            FieldMap = new Dictionary<string, string> { ["price"] = "{price}" },
        };

        var feed = provider.BuildFeed(Context("GenericFeed", settings), [Item(price: 250000)]);
        var product = XDocument.Parse(feed.Content).Root!.Elements().Single();

        // The shop prices in Rial; a Toman-based engine must not be sent a ten-times figure.
        product.Element("price")!.Value.Should().Be("25000");
    }

    [Fact]
    public void GenericFeed_Rejects_Nothing_When_An_Admin_Types_An_Illegal_Tag_Name()
    {
        var provider = new GenericFeedProvider();
        var settings = new GenericFeedSettings
        {
            FieldMap = new Dictionary<string, string> { ["product id"] = "{id}" },
        };

        var feed = provider.BuildFeed(Context("GenericFeed", settings), [Item()]);

        // A space would make the document unparseable, so the name is sanitised rather than emitted raw.
        var act = () => XDocument.Parse(feed.Content);
        act.Should().NotThrow();
        XDocument.Parse(feed.Content).Root!.Elements().Single().Elements().Single().Name.LocalName
            .Should().Be("product_id");
    }

    [Fact]
    public void GenericFeed_Can_Emit_Json_For_Engines_That_Ask_For_It()
    {
        var provider = new GenericFeedProvider();
        var settings = new GenericFeedSettings
        {
            Format = "Json",
            FieldMap = new Dictionary<string, string> { ["product_id"] = "{id}", ["title"] = "{title}" },
        };

        var feed = provider.BuildFeed(Context("GenericFeed", settings), [Item()]);

        feed.ContentType.Should().StartWith("application/json");
        var rows = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(feed.Content)!;
        rows.Single()["product_id"].Should().Be("SKU-1");
        rows.Single()["title"].Should().Be("Blue Shirt");
    }

    [Fact]
    public void ApiTemplate_Escapes_Values_So_A_Quote_In_A_Title_Cannot_Break_The_Body()
    {
        var item = new MarketplaceProductItem { Id = "SKU-1", Title = """A 15" shirt""" };

        var body = MarketplaceTemplate.Fill("""{"title":"{title}"}""", item, "IRR", "true", "false", 1,
            MarketplaceTemplate.JsonEscape);

        var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(body)!;
        parsed["title"].Should().Be("""A 15" shirt""");
    }
}
