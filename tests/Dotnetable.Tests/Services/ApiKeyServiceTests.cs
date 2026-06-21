using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class ApiKeyServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ApiKeyService _service;

    public ApiKeyServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _service = new ApiKeyService(_context);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnApiKeyWithUniqueKey()
    {
        var key = await _service.CreateAsync(websiteId: 1, description: "Test", allowedIps: null);

        key.Should().NotBeNull();
        key.Key.Should().NotBeNullOrWhiteSpace();
        key.WebsiteId.Should().Be(1);
        key.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithValidKey_ShouldReturnApiKey()
    {
        var created = await _service.CreateAsync(1, null, null);

        var result = await _service.ValidateAsync(created.Key, clientIp: null);

        result.Should().NotBeNull();
        result!.WebsiteId.Should().Be(1);
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidKey_ShouldReturnNull()
    {
        var result = await _service.ValidateAsync("invalid-key", clientIp: null);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_WithBlockedIp_ShouldReturnNull()
    {
        var created = await _service.CreateAsync(1, null, ["192.168.1.1"]);

        var result = await _service.ValidateAsync(created.Key, clientIp: "10.0.0.1");

        result.Should().BeNull();
    }

    [Fact]
    public async Task RevokeAsync_ShouldDeactivateKey()
    {
        var created = await _service.CreateAsync(1, null, null);

        await _service.RevokeAsync(created.Id);

        var result = await _service.ValidateAsync(created.Key, clientIp: null);
        result.Should().BeNull();
    }

    public void Dispose() => _context.Dispose();
}
