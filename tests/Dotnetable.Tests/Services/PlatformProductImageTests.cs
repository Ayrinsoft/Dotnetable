using System.Net;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Catalog;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SkiaSharp;
using Xunit;

namespace Dotnetable.Tests.Services;

public class PlatformProductImageTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly FileService _files;
    private readonly PlatformProductImageRenderer _renderer;
    private readonly FakeStorageProvider _provider = new();
    private readonly Website _master;
    private readonly WebsiteStorageSetting _setting;

    public PlatformProductImageTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        var factory = new TestDbContextFactory(opts);

        var registry = new Mock<IFileStorageProviderRegistry>();
        registry.Setup(r => r.Get(It.IsAny<StorageProviderType>())).Returns(_provider);
        var http = new Mock<IHttpClientFactory>();
        http.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(new NamingBlobHandler(_provider), disposeHandler: false));

        _files = new FileService(factory, registry.Object, http.Object);
        _renderer = new PlatformProductImageRenderer(factory, http.Object);

        _master = new Website
        {
            TradeName = "Master", WebsiteAddress = "master.test", AuthCode = Guid.NewGuid(),
            Active = true, Manager = "Mgr", Mobile = "123", Email = "master@test.com",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today), DefaultLanguageCode = "en",
            DefaultCurrencyCode = "USD", BrandName = "Master", IsPlatform = true,
        };
        _context.Websites.Add(_master);
        _context.SaveChanges();

        _setting = new WebsiteStorageSetting
        {
            WebsiteID = _master.WebsiteID,
            StorageProvider = (short)StorageProviderType.LocalHost,
            StorageSettingsJSON = "{}",
            Active = true,
            MaxFileSizeKB = 0,
            AllowedExtensions = "jpg,jpeg,png,webp",
            AutoGenerateThumbnails = true,
        };
        _context.WebsiteStorageSettings.Add(_setting);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Platform_Upload_Stores_Clean_Sibling_When_Watermark_Is_On()
    {
        _master.WebsiteID.Should().Be(1);
        var mark = await UploadPng("mark.png");
        _context.WebsiteWatermarkSettings.Add(new WebsiteWatermarkSetting
        {
            WebsiteID = _master.WebsiteID,
            WatermarkFileID = mark.FileRecordID,
            Enabled = true,
            Position = (byte)WatermarkPosition.BottomRight,
            SizePercent = 40,
            Opacity = 100,
        });
        await _context.SaveChangesAsync();

        var photo = await UploadPng("photo.png");

        photo.RawCdnUrl.Should().NotBeNullOrWhiteSpace();
        photo.CNDUrl.Should().NotBe(photo.RawCdnUrl);
        photo.RawCdnUrl.Should().Contain("clean-");
        photo.CNDUrl.Should().NotContain("clean-");
        photo.RawThumbnailCDN.Should().Contain("_thumb.webp");

        var rawName = NameOf(photo.RawCdnUrl!);
        var burnedName = NameOf(photo.CNDUrl!);
        _provider.Blobs[rawName].Should().NotEqual(_provider.Blobs[burnedName]);
    }

    [Fact]
    public async Task Platform_Upload_Without_Watermark_Aliases_Raw_To_The_Public_File()
    {
        var photo = await UploadPng("plain.png");

        photo.RawCdnUrl.Should().Be(photo.CNDUrl);
        photo.RawStoragePath.Should().Be(photo.StoragePath);
        photo.RawThumbnailCDN.Should().Be(photo.ThumbnailCDN);
        _provider.Blobs.Keys.Should().NotContain(k => k.StartsWith("clean-", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Non_Platform_Upload_Does_Not_Store_A_Clean_Copy()
    {
        _master.IsPlatform = false;
        await _context.SaveChangesAsync();

        var photo = await UploadPng("plain.png");

        photo.RawCdnUrl.Should().BeNull();
        photo.RawStoragePath.Should().BeNull();
    }

    [Fact]
    public void Viewer_Url_Hides_Clean_Cdn_When_The_Child_Has_A_Watermark()
    {
        var file = new FileRecord
        {
            FileRecordID = 42,
            WebsiteID = 1,
            CNDUrl = "https://cdn.test/1/burned.webp",
            ThumbnailCDN = "https://cdn.test/1/burned_thumb.webp",
            RawCdnUrl = "https://cdn.test/1/clean-abc.webp",
            RawThumbnailCDN = "https://cdn.test/1/clean-abc_thumb.webp",
        };
        var stamped = new PlatformImageAudience(true, "shop.example.com", true, "7.2.30.80");
        var plain = new PlatformImageAudience(true, "shop.example.com", false, "0");

        var hidden = PlatformProductImage.ForViewer(file, stamped, thumbnail: true);
        hidden.Should().Be("https://shop.example.com/media/i/42?s=t&v=7.2.30.80");
        hidden.Should().NotContain("clean-abc");
        hidden.Should().NotContain(file.CNDUrl);

        PlatformProductImage.ForViewer(file, plain, thumbnail: false).Should().Be(file.RawCdnUrl);
        PlatformProductImage.ForViewer(file, PlatformImageAudience.Direct, thumbnail: false).Should().Be(file.CNDUrl);

        var legacy = new FileRecord
        {
            FileRecordID = 9,
            WebsiteID = 1,
            CNDUrl = "https://cdn.test/1/old.webp",
            ThumbnailCDN = "https://cdn.test/1/old_thumb.webp",
        };
        PlatformProductImage.ForViewer(legacy, stamped, thumbnail: true).Should().Be(legacy.ThumbnailCDN);
    }

    [Fact]
    public async Task Renderer_Stamps_Clean_Bytes_And_Leaves_Legacy_Files_Unchanged()
    {
        var mark = await UploadPng("mark.png");
        _context.WebsiteWatermarkSettings.Add(new WebsiteWatermarkSetting
        {
            WebsiteID = _master.WebsiteID,
            WatermarkFileID = mark.FileRecordID,
            Enabled = true,
            Position = (byte)WatermarkPosition.Center,
            SizePercent = 50,
            Opacity = 100,
        });
        await _context.SaveChangesAsync();
        var photo = await UploadPng("photo.png");

        var child = new Website
        {
            TradeName = "Child", WebsiteAddress = "child.test", AuthCode = Guid.NewGuid(),
            Active = true, Manager = "Mgr", Mobile = "555", Email = "child@test.com",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today), DefaultLanguageCode = "en",
            DefaultCurrencyCode = "USD", BrandName = "Child",
        };
        _context.Websites.Add(child);
        await _context.SaveChangesAsync();
        _context.WebsiteWatermarkSettings.Add(new WebsiteWatermarkSetting
        {
            WebsiteID = child.WebsiteID,
            WatermarkFileID = mark.FileRecordID,
            Enabled = true,
            Position = (byte)WatermarkPosition.Center,
            SizePercent = 60,
            Opacity = 100,
        });
        await _context.SaveChangesAsync();

        var raw = _provider.Blobs[NameOf(photo.RawCdnUrl!)];
        var stamped = await _renderer.RenderAsync(child.WebsiteID, photo.FileRecordID, thumbnail: false);
        stamped.Should().NotBeNull();
        stamped!.ContentType.Should().Be("image/webp");
        stamped.Content.Should().NotEqual(raw);

        var childMark = await _context.WebsiteWatermarkSettings.SingleAsync(s => s.WebsiteID == child.WebsiteID);
        childMark.Enabled = false;
        await _context.SaveChangesAsync();
        var clean = await _renderer.RenderAsync(child.WebsiteID, photo.FileRecordID, thumbnail: false);
        clean!.Content.Should().Equal(raw);

        var legacyName = "legacy.webp";
        _provider.Blobs[legacyName] = raw;
        var legacy = new FileRecord
        {
            WebsiteID = _master.WebsiteID,
            WebsiteStorageSettingsID = _setting.WebsiteStorageSettingsID,
            StorageProvider = _setting.StorageProvider,
            StoredFileName = "legacy.webp",
            OriginalFileName = "legacy.webp",
            MimeType = "image/webp",
            FileSizeKB = 1,
            FileCategory = (byte)FileCategory.Image,
            CNDUrl = "https://cdn.test/1/legacy.webp",
            UploadDate = DateTime.UtcNow,
        };
        _context.FileRecords.Add(legacy);
        await _context.SaveChangesAsync();
        childMark.Enabled = true;
        await _context.SaveChangesAsync();

        var untouched = await _renderer.RenderAsync(child.WebsiteID, legacy.FileRecordID, thumbnail: false);
        untouched!.Content.Should().Equal(raw);
    }

    public void Dispose() => _context.Dispose();

    private async Task<FileRecord> UploadPng(string name)
    {
        await using var content = new MemoryStream(SolidPng(96, 64));
        return await _files.UploadAsync(new FileUploadRequest
        {
            WebsiteID = _master.WebsiteID,
            StorageSettingID = _setting.WebsiteStorageSettingsID,
            Content = content,
            OriginalFileName = name,
            MimeType = "image/png",
        });
    }

    private static string NameOf(string url) => url.TrimEnd('/').Split('/').Last();

    private static byte[] SolidPng(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Orange);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    /// <summary>Serves a blob by the last segment of the request URL.</summary>
    private sealed class NamingBlobHandler : HttpMessageHandler
    {
        private readonly FakeStorageProvider _provider;
        public NamingBlobHandler(FakeStorageProvider provider) => _provider = provider;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var name = request.RequestUri?.Segments.LastOrDefault()?.Trim('/');
            if (name is null || !_provider.Blobs.TryGetValue(name, out var bytes))
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes),
            });
        }
    }
}
