using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using NSubstitute;
using PollingService.Controllers;
using PollingService.Services;

namespace PollingService.Tests;

public class PollingControllerTests
{
    private readonly IDataService _mockCache = Substitute.For<IDataService>();
    private readonly PollingController _controller;

    public PollingControllerTests()
    {
        _controller = new PollingController(_mockCache);
    }

    [Fact]
    public void Get_ReturnsOk_WhenDataIsCached()
    {
        // Arrange
        var id = "test1";
        var cachedValue = "cached value";

        _mockCache.TryGetCached(id, out Arg.Any<string>())
            .Returns(x =>
                {
                    x[1] = cachedValue;
                    return true;
                }
            );

        // Act
        var result = _controller.Get(id);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = ok.Value.Should().BeAssignableTo<Result>().Subject;
        value.Data.Should().Be(cachedValue);
    }

    [Fact]
    public void Get_ReturnsAcceptedAtAction_WhenDataNotCached()
    {
        // Arrange
        var id = "test2";
        var cachedValue = "cached value";
        var requestId = "req123";
        
        _mockCache.TryGetCached(id, out Arg.Any<string>())
            .Returns(x =>
            {
                x[1] = cachedValue;
                return false;
            });

        _mockCache.StartFetchAsync(id).Returns(requestId);

        // Act
        var result = _controller.Get(id);

        // Assert
        var accepted = result.Should().BeOfType<AcceptedAtActionResult>().Subject;
        accepted.ActionName.Should().Be(nameof(PollingController.GetResult));

        var value = accepted.Value.Should().BeAssignableTo<RequestData>().Subject;
        value.RequestId.Should().Be(requestId);
    }

    [Fact]
    public void GetResult_ReturnsNotFound_WhenRequestMissing()
    {
        // Arrange
        var requestId = "missing";
        _mockCache.TryGetResult(requestId, out Arg.Any<string?>(), out Arg.Any<bool>())
            .Returns(x =>
            {
                x[1] = null;
                x[2] = false;
                return false;
            });

        // Act
        var result = _controller.GetResult(requestId);

        // Assert
        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var payload = notFound.Value.Should().BeOfType<ErrorResponse>().Subject;
        payload.Message.Should().Be("Request not found");
    }

    [Fact]
    public void GetResult_ReturnsAccepted_WhenProcessing()
    {
        // Arrange
        var requestId = "req-123";
        var resultValue = "intermediate";
        bool completed = false;
        _mockCache.TryGetResult(requestId, out Arg.Any<string?>(), out Arg.Any<bool>())
            .Returns(x =>
            {
                x[1] = resultValue;
                x[2] = completed;
                return true;
            });

        // Act
        var result = _controller.GetResult(requestId);

        // Assert
        var accepted = result.Should().BeOfType<AcceptedResult>().Subject;
        accepted.Value.Should().BeOfType<ProcessingResponse>();
    }

    [Fact]
    public void GetResult_ReturnsOk_WhenCompleted()
    {
        // Arrange
        var requestId = "req-123";
        
        _mockCache.TryGetResult(requestId, out Arg.Any<string?>(), out Arg.Any<bool>())
            .Returns(x =>
            {
                x[1] = "final-result";
                x[2] = true;
                return true;
            });

        // Act
        var result = _controller.GetResult(requestId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = okResult.Value.Should().BeOfType<Result>().Subject;
        payload.Data.Should().Be("final-result");
    }
}