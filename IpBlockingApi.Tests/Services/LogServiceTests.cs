using FluentAssertions;
using IpBlockingApi.Models;
using IpBlockingApi.Repositories.Interfaces;
using IpBlockingApi.Services.Implementations;
using Moq;
using Xunit;

namespace IpBlockingApi.Tests.Services;

public sealed class LogServiceTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static LogService CreateService(ILogRepository repo) => new(repo);

    private static BlockedAttemptLog MakeLog(
        string ip          = "1.2.3.4",
        string countryCode = "US",
        bool   isBlocked   = false,
        string userAgent   = "TestAgent",
        DateTime? timestamp = null) => new()
    {
        IpAddress   = ip,
        CountryCode = countryCode,
        IsBlocked   = isBlocked,
        UserAgent   = userAgent,
        Timestamp   = timestamp ?? DateTime.UtcNow
    };

    private static Mock<ILogRepository> RepoWith(params BlockedAttemptLog[] logs)
    {
        var mock = new Mock<ILogRepository>();
        mock.Setup(r => r.GetAllLogs()).Returns(logs.AsEnumerable());
        return mock;
    }

    // ── Empty store ────────────────────────────────────────────────────────────

    [Fact]
    public void GetBlockedAttempts_WithNoLogs_ReturnsEmptyItems()
    {
        // Arrange
        var sut = CreateService(RepoWith().Object);

        // Act
        var result = sut.GetBlockedAttempts(page: 1, pageSize: 10);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    // ── Page clamping ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-99)]
    public void GetBlockedAttempts_PageBelowOne_ClampsToPageOne(int badPage)
    {
        // Arrange
        var logs = Enumerable.Range(1, 5).Select(i => MakeLog(ip: $"1.1.1.{i}")).ToArray();
        var sut  = CreateService(RepoWith(logs).Object);

        // Act
        var result = sut.GetBlockedAttempts(page: badPage, pageSize: 5);

        // Assert
        result.Page.Should().Be(1);
        result.Items.Should().HaveCount(5);
    }

    // ── PageSize clamping ─────────────────────────────────────────────────────

    [Fact]
    public void GetBlockedAttempts_PageSizeAbove100_ClampsTo100()
    {
        // Arrange
        var logs = Enumerable.Range(1, 150).Select(i => MakeLog(ip: $"10.0.0.{i % 254 + 1}")).ToArray();
        var sut  = CreateService(RepoWith(logs).Object);

        // Act
        var result = sut.GetBlockedAttempts(page: 1, pageSize: 999);

        // Assert
        result.PageSize.Should().Be(100);
        result.Items.Should().HaveCount(100);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void GetBlockedAttempts_PageSizeBelowOne_ClampsToOne(int badSize)
    {
        // Arrange
        var logs = new[] { MakeLog(), MakeLog(ip: "5.5.5.5") };
        var sut  = CreateService(RepoWith(logs).Object);

        // Act
        var result = sut.GetBlockedAttempts(page: 1, pageSize: badSize);

        // Assert
        result.PageSize.Should().Be(1);
        result.Items.Should().HaveCount(1);
    }

    // ── Pagination math ───────────────────────────────────────────────────────

    [Fact]
    public void GetBlockedAttempts_SecondPage_ReturnsCorrectSlice()
    {
        // Arrange — 7 logs, pageSize 3 → page 2 should have items 4-6 (indices 3-5)
        var logs = Enumerable.Range(1, 7)
            .Select(i => MakeLog(ip: $"10.0.0.{i}"))
            .ToArray();
        var sut = CreateService(RepoWith(logs).Object);

        // Act
        var result = sut.GetBlockedAttempts(page: 2, pageSize: 3);

        // Assert
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(3);
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(7);
        result.TotalPages.Should().Be(3);   // ceil(7/3) = 3
    }

    [Fact]
    public void GetBlockedAttempts_LastPage_ReturnsRemainingItems()
    {
        // Arrange — 7 logs, pageSize 3 → page 3 should have only 1 item
        var logs = Enumerable.Range(1, 7)
            .Select(i => MakeLog(ip: $"10.0.0.{i}"))
            .ToArray();
        var sut = CreateService(RepoWith(logs).Object);

        // Act
        var result = sut.GetBlockedAttempts(page: 3, pageSize: 3);

        // Assert
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public void GetBlockedAttempts_PageBeyondLastPage_ReturnsEmptyItems()
    {
        // Arrange — only 2 logs, requesting page 10
        var logs = new[] { MakeLog(), MakeLog(ip: "2.2.2.2") };
        var sut  = CreateService(RepoWith(logs).Object);

        // Act
        var result = sut.GetBlockedAttempts(page: 10, pageSize: 10);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(2);
    }

    // ── Field mapping ─────────────────────────────────────────────────────────

    [Fact]
    public void GetBlockedAttempts_MapsAllFieldsFromLogToResponse()
    {
        // Arrange
        var ts  = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var log = MakeLog(
            ip:          "203.0.113.5",
            countryCode: "EG",
            isBlocked:   true,
            userAgent:   "Mozilla/5.0",
            timestamp:   ts);

        var sut = CreateService(RepoWith(log).Object);

        // Act
        var result = sut.GetBlockedAttempts(page: 1, pageSize: 10);

        // Assert
        var item = result.Items.Should().ContainSingle().Subject;
        item.IpAddress.Should().Be("203.0.113.5");
        item.CountryCode.Should().Be("EG");
        item.IsBlocked.Should().BeTrue();
        item.UserAgent.Should().Be("Mozilla/5.0");
        item.Timestamp.Should().Be(ts);
    }

    // ── Repository interaction ────────────────────────────────────────────────

    [Fact]
    public void GetBlockedAttempts_CallsGetAllLogsExactlyOnce()
    {
        // Arrange
        var repoMock = RepoWith(MakeLog());
        var sut      = CreateService(repoMock.Object);

        // Act
        sut.GetBlockedAttempts(page: 1, pageSize: 10);

        // Assert
        repoMock.Verify(r => r.GetAllLogs(), Times.Once);
    }

    // ── TotalPages edge cases ─────────────────────────────────────────────────

    [Fact]
    public void GetBlockedAttempts_ExactMultipleOfPageSize_TotalPagesIsCorrect()
    {
        // Arrange — 9 logs, pageSize 3 → exactly 3 pages, no remainder
        var logs = Enumerable.Range(1, 9).Select(i => MakeLog(ip: $"10.0.0.{i}")).ToArray();
        var sut  = CreateService(RepoWith(logs).Object);

        // Act
        var result = sut.GetBlockedAttempts(page: 1, pageSize: 3);

        // Assert
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public void GetBlockedAttempts_SingleItem_TotalPagesIsOne()
    {
        // Arrange
        var sut = CreateService(RepoWith(MakeLog()).Object);

        // Act
        var result = sut.GetBlockedAttempts(page: 1, pageSize: 10);

        // Assert
        result.TotalPages.Should().Be(1);
    }
}