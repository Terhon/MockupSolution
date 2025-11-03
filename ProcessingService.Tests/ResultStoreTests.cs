using FluentAssertions;
using ProcessingService.Models;

namespace ProcessingService.Tests;

public class ResultStoreTests
{
    [Fact]
    public void RequestCount_ShouldIncrementEachTime()
    {
        // Arrange
        var startCount = ResultStore.RequestCount;

        // Act
        var nextCount = ResultStore.RequestCount;

        // Assert
        nextCount.Should().BeGreaterThan(startCount);
    }

    [Fact]
    public void Data_ShouldAllowAddingAndRetrievingValues()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();
        var value = "test result";

        // Act
        var added = ResultStore.Data.TryAdd(key, value);

        // Assert
        added.Should().BeTrue();
        ResultStore.Data[key].Should().Be(value);
    }

    [Fact]
    public void Data_ShouldBeThreadSafe()
    {
        // Arrange
        var items = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid().ToString()).ToList();

        // Act
        Parallel.ForEach(items, id =>
        {
            ResultStore.Data.TryAdd(id, "val");
        });

        // Assert
        ResultStore.Data.Keys.Should().Contain(items);
    }
}