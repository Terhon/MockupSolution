namespace PollingService.Services;

public class ExternalApiClient(IHttpClientFactory clientFactory): IExternalApiClient
{
    private const string BaseUrl = "http://localhost:5188/api/task";
    private const int BaseDelay = 500;
    
    public async Task<string> GetDataAsync(string clientId)
    {
        var requestId = await StartTask(clientId);

        while (true)
        {
            var (done, result) = await GetResult(requestId);
            if (done)
                return result!;

            await Task.Delay(BaseDelay);
        }
    }
    
    private async Task<string> StartTask(string clientId)
    {
        var client = clientFactory.CreateClient();
        var response = await client.GetAsync($"{BaseUrl}/{clientId}");
        
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Start failed with status code {response.StatusCode} and message {await response.Content.ReadAsStringAsync()}");

        var json = await response.Content.ReadFromJsonAsync<RequestId>();
        return json?.Id ?? throw new InvalidOperationException("Invalid response from ProcessingService");
    }

    private async Task<(bool completed, string? data)> GetResult(string requestId)
    {
        var client = clientFactory.CreateClient();

        var response = await client.GetAsync($"{BaseUrl}/result/{requestId}");

        if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
        {
            var status = await response.Content.ReadFromJsonAsync<RequestStatus>();
            if (status?.Status == "Processing")
                return (false, null);
        }

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<RequestResult>();
            return (true, result?.Result);
        }

        var error = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Result check failed: {response.StatusCode} — {error}");
    }

    private record RequestId(string Id);
    private record RequestStatus(string Status);
    private record RequestResult(string Result);
}