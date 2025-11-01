namespace PollingService.Services
{
    public interface IDataService
    {
        public void StartFetch(string clientId);
        public bool TryGetCached(string clientId, out string data);
    }
}