using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using Xunit;
using PollingService.Controllers;
using PollingService.Services;

namespace PollingService.Tests;

public class PollingControllerTests
{
    private readonly Mock<IDataService> _mockCache = new();

    [Fact]
    public void ReturnsOk_WhenDataIsCached()
    {
        // Arrange
        var id = "test1";
        var cachedValue = "cached value";
        _mockCache
            .Setup(c => c.TryGetCached(id, out cachedValue))
            .Returns(true);
        var controller = new PollingController(_mockCache.Object);

        // Act
        var result = controller.Get(id);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var value = ok.Value.Should().BeAssignableTo<Result>().Subject;
        value.data.Should().Be(cachedValue);
    }

}