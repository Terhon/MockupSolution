using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using PollingService.Services;

namespace PollingService.Tests;

public class DataServiceTests
{
    private readonly IMemoryCache _cache;
    private readonly IExternalApiClient _externalApiMock;
    private readonly DataService _service;

    public DataServiceTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _externalApiMock = Substitute.For<IExternalApiClient>();
        _service = new DataService(_cache, _externalApiMock);
    }

    [Fact]
    public void TryGetCached_ReturnsFalse_WhenNotInCache()
    {
        // Act
        var success = _service.TryGetCached("client1", out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNullOrEmpty();
    }

    [Fact]
    public void TryGetCached_ReturnsTrue_WhenCached()
    {
        // Arrange
        var clientId = "client1";
        var cachedValue = "cached-value";
        _cache.Set(clientId, cachedValue);

        // Act
        var success = _service.TryGetCached(clientId, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(cachedValue);
    }
    
    [Fact]
    public async Task StartFetchAsync_ShouldStartBackgroundFetch_AndCacheResult()
    {
        // Arrange
        var clientId = "client1";
        var fetchedData = "fetched-data";
        _externalApiMock.GetDataAsync(clientId).Returns(Task.FromResult(fetchedData));

        // Act
        await _service.StartFetch(clientId);
        await Task.Delay(50);

        // Assert cache filled
        _service.TryGetCached(clientId, out var cachedData).Should().BeTrue();
        cachedData.Should().Be(fetchedData);
    }
}