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
    public void Get_ReturnsOk_WhenDataIsCached()
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
        value.Data.Should().Be(cachedValue);
    }

    [Fact]
    public void Get_ReturnsAcceptedAtAction_WhenDataNotCached()
    {
        // Arrange
        var id = "test2";
        var cachedValue = "cached value";
        var requestId = "req123";
        _mockCache
            .Setup(c => c.TryGetCached(id, out cachedValue))
            .Returns(false);
        
        _mockCache
            .Setup(c => c.StartFetchAsync(id))
            .Returns(requestId);
        
        var controller = new PollingController(_mockCache.Object);

        // Act
        var result = controller.Get(id);

        // Assert
        var accepted = result.Should().BeOfType<AcceptedAtActionResult>().Subject;
        accepted.ActionName.Should().Be(nameof(PollingController.GetResult));
        
        var value = accepted.Value.Should().BeAssignableTo<RequestData>().Subject;
        (value.RequestId).Should().Be(requestId);
    }
 [Fact]
        public void GetResult_ReturnsNotFound_WhenRequestMissing()
        {
            // Arrange
            var requestId = "missing";
            string? resultValue;
            bool completed;
            _mockCache.Setup(s => s.TryGetResult(requestId, out resultValue, out completed))
                            .Returns(false);

            // Act
            var controller = new PollingController(_mockCache.Object);
            var result = controller.GetResult(requestId);

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
            _mockCache.Setup(s => s.TryGetResult(requestId, out resultValue, out completed))
                            .Returns(true);

            // Act
            var controller = new PollingController(_mockCache.Object);
            var result = controller.GetResult(requestId);

            // Assert
            var accepted = result.Should().BeOfType<AcceptedResult>().Subject;
            accepted.Value.Should().BeOfType<ProcessingResponse>();
        }

        [Fact]
        public void GetResult_ReturnsOk_WhenCompleted()
        {
            // Arrange
            var requestId = "req-123";
            var resultValue = "final-result";
            bool completed = true;
            _mockCache.Setup(s => s.TryGetResult(requestId, out resultValue, out completed))
                            .Returns(true);

            // Act
            var controller = new PollingController(_mockCache.Object);
            var result = controller.GetResult(requestId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var payload = okResult.Value.Should().BeOfType<Result>().Subject;
            payload.Data.Should().Be("final-result");
        }
}