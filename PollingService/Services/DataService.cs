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
            data = string.Empty;
            if (!cache.TryGetValue(clientId, out string result))
                return false;

            data = result;
            return true;
        }

        public string StartFetchAsync(string clientId)
        {
            var existing = _pending.FirstOrDefault(x => x.Value.AsyncState?.ToString() == clientId);
            if (!string.IsNullOrEmpty(existing.Key))
                return existing.Key;

            var requestId = Guid.NewGuid().ToString();

            var fetchTask = Task.Run(async () =>
            {
                var data = await externalApi.GetDataAsync(clientId);
                cache.Set(clientId, data, _defaultTimeout);
                _pending.TryRemove(requestId, out _);
                return data;
            });

            _pending[requestId] = fetchTask;
            return requestId;
        }

        public bool TryGetResult(string requestId, out string? result, out bool completed)
        {
            if (_pending.TryGetValue(requestId, out var task))
            {
                if (task.IsCompletedSuccessfully)
                {
                    result = task.Result;
                    completed = true;
                    return true;
                }

                result = null;
                completed = false;
                return true;
            }

            result = null;
            completed = false;
            return false;
        }
    }
}