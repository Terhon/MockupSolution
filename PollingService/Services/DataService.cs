using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace PollingService.Services
{
    public class DataService(IMemoryCache cache, IExternalApiClient externalApi)
        : IDataService
    {
        private readonly ConcurrentDictionary<string, Task<string>> _pending = new();
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(5);
        
        public bool TryGetCached(string clientId, out string data)
        {
            return cache.TryGetValue(clientId, out data!);
        }

        public async Task StartFetch(string clientId)
        {
            await _pending.GetOrAdd(clientId, async _ =>
            {
                try
                {
                    var data = await externalApi.GetDataAsync(clientId);
                    cache.Set(clientId, data, _defaultTimeout);
                    return data;
                }
                finally
                {
                    _pending.TryRemove(clientId, out var obj);
                }
            });
        }
    }
}