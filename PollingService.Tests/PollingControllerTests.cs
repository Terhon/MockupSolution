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
    public void Get_ReturnsAccepted_WhenDataNotCached()
    {
        // Arrange
        var id = "test2";
        var requestId = "req123";

        _mockCache.TryGetCached(id, out Arg.Any<string>())
            .Returns(false);

        _mockCache.StartFetch(id).Returns(requestId);

        // Act
        var result = _controller.Get(id);

        // Assert
        result.Should().BeOfType<AcceptedResult>();
    }
}