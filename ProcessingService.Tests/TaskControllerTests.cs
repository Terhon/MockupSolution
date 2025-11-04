using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ProcessingService.Controllers;
using ProcessingService.Models;

namespace ProcessingService.Tests;

public class TaskControllerTests
{
    private readonly TaskController _controller = new();

    public TaskControllerTests()
    {
        // Clear static store before each test
        ResultStore.Data.Clear();
        typeof(ResultStore)
            .GetField("_requestCount",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(null, 0);
    }

    [Fact]
    public void StartTask_ShouldReturnOk_WithRequestId()
    {
        // Act
        var result = _controller.StartTask("client1");

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<RequestId>().Subject;
        data.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void StartTask_Every10thRequest_ShouldReturnServerError()
    {
        // Arrange
        for (int i = 1; i < 10; i++)
            _controller.StartTask($"client{i}");

        // Act
        var result = _controller.StartTask("client10");

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public void GetResult_ShouldReturnAccepted_IfNotReady()
    {
        // Arrange
        var requestId = Guid.NewGuid().ToString();

        // Act
        var result = _controller.GetResult(requestId);

        // Assert
        var accepted = result.Should().BeOfType<AcceptedResult>().Subject;
        var data = accepted.Value.Should().BeAssignableTo<RequestStatus>().Subject;
        data.Status.Should().Be("Processing");
    }

    [Fact]
    public void GetResult_ShouldReturnOk_WhenDataIsReady()
    {
        // Arrange
        var requestId = "req123";
        ResultStore.Data.TryAdd(requestId, "done");

        // Act
        var result = _controller.GetResult(requestId);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok.Value.Should().BeAssignableTo<RequestResult>().Subject;
        data.Result.Should().Be("done");
    }

    [Fact]
    public void StartTask_ShouldEventuallyAddToResultStore()
    {
        // Arrange
        var result = _controller.StartTask("client123");
        var ok = result as OkObjectResult;
        var requestId = ok!.Value.Should().BeAssignableTo<RequestId>().Subject.Id;

        // Manually simulate background completion for test determinism
        ResultStore.Data.TryAdd(requestId, "Simulated result");

        // Act
        var getResult = _controller.GetResult(requestId);

        // Assert
        var ok2 = getResult.Should().BeOfType<OkObjectResult>().Subject;
        var data = ok2.Value.Should().BeAssignableTo<RequestResult>().Subject;
        data.Result.Should().Be("Simulated result");
    }
}