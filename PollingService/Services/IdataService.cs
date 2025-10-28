namespace PollingService.Services
{
    public interface IDataService
    {
        Task<string> StartProcessingAsync(string clientId);
        bool TryGetResult(string requestId, out string data);
    }
}