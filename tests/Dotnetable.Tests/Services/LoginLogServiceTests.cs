using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class LoginLogServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly LoginLogService _service;

    public LoginLogServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        _service = new LoginLogService(_context);
    }

    [Fact]
    public async Task RecordAsync_SavesLoginTry()
    {
        await _service.RecordAsync("alice", websiteId: 1, success: true, ip: "127.0.0.1");

        var stored = _context.LoginTries.Single();
        stored.Username.Should().Be("alice");
        stored.WebsiteID.Should().Be(1);
        stored.IsSuccess.Should().BeTrue();
        stored.TryIP.Should().Be("127.0.0.1");
    }

    [Fact]
    public async Task RecordAsync_TruncatesUsernameLongerThan64Chars()
    {
        var longName = new string('x', 80);

        await _service.RecordAsync(longName, websiteId: 1, success: false, ip: "1.1.1.1");

        _context.LoginTries.Single().Username.Should().HaveLength(64);
    }

    [Fact]
    public async Task RecordAsync_DoesNotTruncateUsernameAt64Chars()
    {
        var exactName = new string('a', 64);

        await _service.RecordAsync(exactName, websiteId: 1, success: false, ip: "1.1.1.1");

        _context.LoginTries.Single().Username.Should().HaveLength(64);
    }

    [Fact]
    public async Task RecordAsync_TruncatesIpLongerThan15Chars()
    {
        var longIp = "192.168.100.200.300"; // 19 chars

        await _service.RecordAsync("user", websiteId: 1, success: true, ip: longIp);

        _context.LoginTries.Single().TryIP.Should().HaveLength(15);
    }

    [Fact]
    public async Task RecordAsync_DoesNotTruncateShortIp()
    {
        var shortIp = "192.168.1.1"; // 11 chars

        await _service.RecordAsync("user", websiteId: 1, success: true, ip: shortIp);

        _context.LoginTries.Single().TryIP.Should().Be(shortIp);
    }

    [Fact]
    public async Task RecordAsync_SetsLogTimeToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);

        await _service.RecordAsync("user", websiteId: 1, success: true, ip: "1.1.1.1");

        var stored = _context.LoginTries.Single();
        stored.LogTime.Should().BeAfter(before).And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetPagedAsync_UsernameFilter_FiltersCorrectly()
    {
        await _service.RecordAsync("alice", 1, true, "1.1.1.1");
        await _service.RecordAsync("bob", 1, true, "2.2.2.2");

        var q = new GridQuery(); q.Search["Username"] = "ali";
        var result = await _service.GetPagedAsync(null, q);

        result.Items.Should().ContainSingle(l => l.Username == "alice");
    }

    [Fact]
    public async Task GetPagedAsync_IpFilter_FiltersCorrectly()
    {
        await _service.RecordAsync("alice", 1, true, "10.0.0.1");
        await _service.RecordAsync("bob", 1, true, "192.168.1.1");

        var q = new GridQuery(); q.Search["TryIP"] = "10.0";
        var result = await _service.GetPagedAsync(null, q);

        result.Items.Should().ContainSingle(l => l.Username == "alice");
    }

    [Fact]
    public async Task GetPagedAsync_SuccessFilter_FiltersCorrectly()
    {
        await _service.RecordAsync("alice", 1, success: true, ip: "1.1.1.1");
        await _service.RecordAsync("bob", 1, success: false, ip: "2.2.2.2");

        var q = new GridQuery(); q.Search["IsSuccess"] = "false";
        var result = await _service.GetPagedAsync(null, q);

        result.Items.Should().ContainSingle(l => l.Username == "bob");
    }

    [Fact]
    public async Task GetPagedAsync_WebsiteFilter_ReturnsOnlyMatchingSite()
    {
        await _service.RecordAsync("alice", websiteId: 1, success: true, ip: "1.1.1.1");
        await _service.RecordAsync("bob", websiteId: 2, success: true, ip: "2.2.2.2");

        var result = await _service.GetPagedAsync(websiteId: 1, new GridQuery());

        result.Items.Should().OnlyContain(l => l.WebsiteID == 1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsCorrectTotalCount()
    {
        for (int i = 0; i < 8; i++)
            await _service.RecordAsync($"user{i}", 1, true, "1.1.1.1");

        var q = new GridQuery { PageSize = 3 };
        var result = await _service.GetPagedAsync(null, q);

        result.TotalCount.Should().Be(8);
        result.Items.Should().HaveCount(3);
    }

    public void Dispose() => _context.Dispose();
}
