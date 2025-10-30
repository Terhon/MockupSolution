namespace PollingService.Services;

public interface IExternalApiClient
{
    public Task<string> GetDataAsync(string clientId);
}