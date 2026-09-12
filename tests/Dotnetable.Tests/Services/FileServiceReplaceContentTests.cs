using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Dotnetable.Tests.Services;

/// <summary>A fake storage backend that mimics the real providers' contract: the returned
/// path/URL are deterministic functions of the storage key, so re-uploading under the same key
/// (what a "replace file" must do) reproduces the exact same path/URL.</summary>
internal sealed class FakeStorageProvider : IFileStorageProvider
{
    public readonly Dictionary<string, byte[]> Blobs = new();

    public StorageProviderType Provider => StorageProviderType.LocalHost;

    public Task<StorageUploadResult> UploadAsync(StorageSettingContext ctx, Stream data, string storedName,
        string mimeType, CancellationToken ct = default)
    {
        using var ms = new MemoryStream();
        data.CopyTo(ms);
        Blobs[storedName] = ms.ToArray();

        return Task.FromResult(new StorageUploadResult
        {
            StoragePath = $"{ctx.WebsiteID}/{storedName}",
            CdnUrl = $"https://cdn.test/{ctx.WebsiteID}/{storedName}",
            CdnFileCode = storedName,
        });
    }

    public Task DeleteAsync(StorageSettingContext ctx, string storedName, CancellationToken ct = default)
    {
        Blobs.Remove(storedName);
        return Task.CompletedTask;
    }

    public Task<StorageQuota> GetQuotaAsync(StorageSettingContext ctx, CancellationToken ct = default) =>
        Task.FromResult(new StorageQuota());

    public Task<bool> TestConnectionAsync(StorageSettingContext ctx, CancellationToken ct = default) =>
        Task.FromResult(true);
}

public class FileServiceReplaceContentTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly FileService _service;
    private readonly FakeStorageProvider _provider = new();
    private readonly Website _website;
    private readonly WebsiteStorageSetting _setting;

    public FileServiceReplaceContentTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        var factory = new TestDbContextFactory(opts);

        var registry = new Mock<IFileStorageProviderRegistry>();
        registry.Setup(r => r.Get(It.IsAny<StorageProviderType>())).Returns(_provider);

        _service = new FileService(factory, registry.Object, Mock.Of<IHttpClientFactory>());

        _website = new Website
        {
            TradeName = "Test", WebsiteAddress = "test.com", AuthCode = Guid.NewGuid(),
            Active = true, Manager = "Mgr", Mobile = "123", Email = "admin@test.com",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today), DefaultLanguageCode = "en",
            DefaultCurrencyCode = "USD", BrandName = "Test",
        };
        _context.Websites.Add(_website);
        _context.SaveChanges();

        _setting = new WebsiteStorageSetting
        {
            WebsiteID = _website.WebsiteID,
            StorageProvider = (short)StorageProviderType.LocalHost,
            StorageSettingsJSON = "{}",
            Active = true,
            MaxFileSizeKB = 0,
            AllowedExtensions = "jpg,jpeg,png,txt",
            AutoGenerateThumbnails = false,
        };
        _context.WebsiteStorageSettings.Add(_setting);
        _context.SaveChanges();
    }

    private static Stream Bytes(string content) => new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

    [Fact]
    public async Task ReplaceContentAsync_Keeps_Same_Id_And_Url()
    {
        var uploaded = await _service.UploadAsync(new FileUploadRequest
        {
            WebsiteID = _website.WebsiteID,
            StorageSettingID = _setting.WebsiteStorageSettingsID,
            Content = Bytes("hello world"),
            OriginalFileName = "notes.txt",
            MimeType = "text/plain",
        });

        var originalId = uploaded.FileRecordID;
        var originalStoragePath = uploaded.StoragePath;
        var originalCdnUrl = uploaded.CNDUrl;
        var originalStoredFileName = uploaded.StoredFileName;

        var replaced = await _service.ReplaceContentAsync(
            originalId, Bytes("brand new content"), "new file name.txt", "text/plain");

        replaced.FileRecordID.Should().Be(originalId);
        replaced.StoredFileName.Should().Be(originalStoredFileName);
        replaced.StoragePath.Should().Be(originalStoragePath);
        replaced.CNDUrl.Should().Be(originalCdnUrl);

        _provider.Blobs.Should().ContainKey(originalStoredFileName);
        System.Text.Encoding.UTF8.GetString(_provider.Blobs[originalStoredFileName]).Should().Be("brand new content");
    }

    [Fact]
    public async Task ReplaceContentAsync_Sanitizes_The_New_File_Name()
    {
        var uploaded = await _service.UploadAsync(new FileUploadRequest
        {
            WebsiteID = _website.WebsiteID,
            StorageSettingID = _setting.WebsiteStorageSettingsID,
            Content = Bytes("hello"),
            OriginalFileName = "notes.txt",
            MimeType = "text/plain",
        });

        var replaced = await _service.ReplaceContentAsync(
            uploaded.FileRecordID, Bytes("v2"), "my new report v2.txt", "text/plain");

        replaced.OriginalFileName.Should().NotContain(" ");
        replaced.OriginalFileName.Should().Be("my-new-report-v2.txt");
    }

    [Fact]
    public async Task ReplaceContentAsync_Updates_Size_And_MimeType()
    {
        var uploaded = await _service.UploadAsync(new FileUploadRequest
        {
            WebsiteID = _website.WebsiteID,
            StorageSettingID = _setting.WebsiteStorageSettingsID,
            Content = Bytes("x"),
            OriginalFileName = "notes.txt",
            MimeType = "text/plain",
        });

        var bigContent = new string('a', 5000);
        var replaced = await _service.ReplaceContentAsync(
            uploaded.FileRecordID, Bytes(bigContent), "notes.txt", "text/plain");

        replaced.FileSizeKB.Should().BeGreaterThan(uploaded.FileSizeKB);
    }

    [Fact]
    public async Task UploadAsync_Sanitizes_OriginalFileName_With_Spaces()
    {
        var uploaded = await _service.UploadAsync(new FileUploadRequest
        {
            WebsiteID = _website.WebsiteID,
            StorageSettingID = _setting.WebsiteStorageSettingsID,
            Content = Bytes("hello"),
            OriginalFileName = "my report final.txt",
            MimeType = "text/plain",
        });

        uploaded.OriginalFileName.Should().Be("my-report-final.txt");
    }

    public void Dispose() => _context.Dispose();
}
