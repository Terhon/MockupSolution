using System.Collections.Concurrent;

namespace ProcessingService.Models
{
    public static class ResultStore
    {
        public static ConcurrentDictionary<string, string> Data { get; set; } = new();

        private static int _requestCount = 0;
        public static int RequestCount
        {
            get { return Interlocked.Increment(ref _requestCount); }
        }
    }
}