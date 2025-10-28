using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace PollingService.Services
{
    public class DataService(IMemoryCache cache, IHttpClientFactory clientFactory, IBackgroundTaskQueue queue)
        : IDataService
    {
        public bool TryGetResult(string requestId, out string data)
        {
            data = string.Empty;
            if (!cache.TryGetValue(requestId, out string result)) 
                return false;
            
            data = result;
            return true;
        }

        public async Task<string> StartProcessingAsync(string clientId)
        {
            if (cache.TryGetValue($"client:{clientId}", out string cachedResult))
                return cachedResult;

            Console.WriteLine($"client id {clientId}");

            var client = clientFactory.CreateClient();
            var response = await client.PostAsync($"http://localhost:5188/api/task/{clientId}", null);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Start failed with status code {response.StatusCode} and message {response.Content}");

            var json = await response.Content.ReadAsStringAsync();
            var requestId = JsonDocument.Parse(json).RootElement.GetProperty("requestId").GetString();
            Console.WriteLine($"request id {requestId}");

            queue.QueueBackgroundWorkItem(async token =>
            {
                while (!token.IsCancellationRequested)
                {
                    var resultResponse = await client.GetAsync($"http://localhost:5188/api/task/result/{requestId}", token);

                    if (resultResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var json = await resultResponse.Content.ReadAsStringAsync(token);
                        var result = JsonDocument.Parse(json).RootElement.GetProperty("result").GetString();
                        cache.Set(clientId, result, TimeSpan.FromMinutes(5));

                        Console.WriteLine($"{result} for {clientId}");
                        return;
                    }
                    else if (resultResponse.StatusCode == System.Net.HttpStatusCode.Accepted)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), token);
                    }
                    else
                    {
                        throw new HttpRequestException($"Unexpected status code: {resultResponse.StatusCode}");
                    }
                }

                throw new OperationCanceledException();
            });

            return requestId;
        }
    }
}