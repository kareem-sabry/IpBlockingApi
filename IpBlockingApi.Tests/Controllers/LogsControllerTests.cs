using FluentAssertions;
using IpBlockingApi.Common;
using IpBlockingApi.Controllers;
using IpBlockingApi.DTOs.Responses;
using IpBlockingApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace IpBlockingApi.Tests.Controllers;

public sealed class LogsControllerTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static LogsController CreateController(ILogService logService)
        => new(logService, NullLogger<LogsController>.Instance);

    private static PagedResponse<BlockedAttemptLogResponse> MakePagedResponse(
        int page = 1, int pageSize = 10, int totalCount = 0)
        => new()
        {
            Items      = Enumerable.Empty<BlockedAttemptLogResponse>(),
            Page       = page,
            PageSize   = pageSize,
            TotalCount = totalCount
        };

    // ── Status code ───────────────────────────────────────────────────────────

    [Fact]
    public void GetBlockedAttempts_Always_Returns200Ok()
    {
        // Arrange
        var serviceMock = new Mock<ILogService>();
        serviceMock
            .Setup(s => s.GetBlockedAttempts(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(MakePagedResponse());

        var sut = CreateController(serviceMock.Object);

        // Act
        var actionResult = sut.GetBlockedAttempts();

        // Assert
        actionResult.Should().BeOfType<OkObjectResult>()
            .Which.StatusCode.Should().Be(200);
    }

    // ── Response envelope ─────────────────────────────────────────────────────

    [Fact]
    public void GetBlockedAttempts_Always_WrapsResultInApiResponseEnvelope()
    {
        // Arrange
        var serviceMock = new Mock<ILogService>();
        serviceMock
            .Setup(s => s.GetBlockedAttempts(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(MakePagedResponse(totalCount: 3));

        var sut = CreateController(serviceMock.Object);

        // Act
        var okResult = sut.GetBlockedAttempts() as OkObjectResult;
        var envelope = okResult!.Value as ApiResponse<PagedResponse<BlockedAttemptLogResponse>>;

        // Assert
        envelope.Should().NotBeNull();
        envelope!.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.TotalCount.Should().Be(3);
    }

    // ── Delegation — correct arguments forwarded to service ───────────────────

    [Fact]
    public void GetBlockedAttempts_PassesSuppliedPageAndPageSizeToService()
    {
        // Arrange
        var serviceMock = new Mock<ILogService>();
        serviceMock
            .Setup(s => s.GetBlockedAttempts(3, 25))
            .Returns(MakePagedResponse(page: 3, pageSize: 25))
            .Verifiable();

        var sut = CreateController(serviceMock.Object);

        // Act
        sut.GetBlockedAttempts(page: 3, pageSize: 25);

        // Assert
        serviceMock.Verify(s => s.GetBlockedAttempts(3, 25), Times.Once);
    }

    [Fact]
    public void GetBlockedAttempts_WithNoArguments_PassesDefaultsToService()
    {
        // Arrange — defaults are page=1, pageSize=10 as declared in the action signature
        var serviceMock = new Mock<ILogService>();
        serviceMock
            .Setup(s => s.GetBlockedAttempts(1, 10))
            .Returns(MakePagedResponse())
            .Verifiable();

        var sut = CreateController(serviceMock.Object);

        // Act
        sut.GetBlockedAttempts();

        // Assert
        serviceMock.Verify(s => s.GetBlockedAttempts(1, 10), Times.Once);
    }

    // ── Service called exactly once ───────────────────────────────────────────

    [Fact]
    public void GetBlockedAttempts_CallsServiceExactlyOnce()
    {
        // Arrange
        var serviceMock = new Mock<ILogService>();
        serviceMock
            .Setup(s => s.GetBlockedAttempts(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(MakePagedResponse());

        var sut = CreateController(serviceMock.Object);

        // Act
        sut.GetBlockedAttempts(page: 2, pageSize: 5);

        // Assert
        serviceMock.Verify(
            s => s.GetBlockedAttempts(It.IsAny<int>(), It.IsAny<int>()),
            Times.Once);
    }

    // ── Data flows from service to response unchanged ─────────────────────────

    [Fact]
    public void GetBlockedAttempts_ReturnsExactDataFromService()
    {
        // Arrange
        var ts = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var item = new BlockedAttemptLogResponse
        {
            IpAddress   = "8.8.8.8",
            CountryCode = "US",
            IsBlocked   = true,
            UserAgent   = "curl/7.0",
            Timestamp   = ts
        };

        var pagedData = new PagedResponse<BlockedAttemptLogResponse>
        {
            Items      = new[] { item },
            Page       = 1,
            PageSize   = 10,
            TotalCount = 1
        };

        var serviceMock = new Mock<ILogService>();
        serviceMock
            .Setup(s => s.GetBlockedAttempts(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(pagedData);

        var sut = CreateController(serviceMock.Object);

        // Act
        var okResult = sut.GetBlockedAttempts() as OkObjectResult;
        var envelope = okResult!.Value as ApiResponse<PagedResponse<BlockedAttemptLogResponse>>;

        // Assert
        var returnedItem = envelope!.Data!.Items.Should().ContainSingle().Subject;
        returnedItem.IpAddress.Should().Be("8.8.8.8");
        returnedItem.CountryCode.Should().Be("US");
        returnedItem.IsBlocked.Should().BeTrue();
        returnedItem.UserAgent.Should().Be("curl/7.0");
        returnedItem.Timestamp.Should().Be(ts);
    }
}