namespace PollingService.Services;

public class ExternalApiClient(IHttpClientFactory clientFactory): IExternalApiClient
{
    public async Task<string> GetDataAsync(string clientId)
    {
        var client = clientFactory.CreateClient();
        var response = await client.GetAsync($"http://localhost:5188/api/task/{clientId}");
        
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Start failed with status code {response.StatusCode} and message {response.Content}");

        var json = await response.Content.ReadAsStringAsync();
        return json;
    }
}