namespace PollingService.Services
{
    public interface IDataService
    {
        Task<string> StartProcessingAsync(string clientId);
        Task<string?> GetResultAsync(string requestId);
    }
}