using Microsoft.Extensions.Caching.Memory;

namespace PollingService.Services
{
    public class DataService : IDataService
    {
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IBackgroundTaskQueue _queue;

        public DataService(IMemoryCache cache, IHttpClientFactory clientFactory, IBackgroundTaskQueue queue)
        {
            _cache = cache;
            _clientFactory = clientFactory;
            _queue = queue;
        }

        public Task<string?> GetResultAsync(string requestId)
        {
            _cache.TryGetValue($"result:{requestId}", out string? result);
            return Task.FromResult(result);
        }

        public Task<string> StartProcessingAsync(string clientId)
        {
            if (_cache.TryGetValue($"client:{clientId}", out string cachedResult))
            {
                return Task.FromResult(cachedResult);
            }

            var requestId = Guid.NewGuid().ToString();

            _queue.QueueBackgroundWorkItem(async token =>
            {
                var client = _clientFactory.CreateClient();
                var response = await client.PostAsync($"http://localhost:5188/api/task/{clientId}", null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();

                    _cache.Set($"client:{clientId}", result, TimeSpan.FromMinutes(5));
                    _cache.Set($"result:{requestId}", result, TimeSpan.FromMinutes(10));
                }
            });

            return Task.FromResult(requestId);
        }
    }
}