namespace PollingService.Services
{
    public interface IDataService
    {
        public string StartFetchAsync(string clientId);
        public bool TryGetResult(string requestId, out string? result, out bool completed);
        public bool TryGetCached(string requestId, out string data);
    }
}