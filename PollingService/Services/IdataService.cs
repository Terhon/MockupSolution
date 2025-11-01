namespace PollingService.Services
{
    public interface IDataService
    {
        public string StartFetch(string clientId);
        public bool TryGetCached(string clientId, out string data);
    }
}