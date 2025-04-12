using Microsoft.Extensions.Options;
using T9Backend.Configuration;

namespace T9Backend.Services
{
    public class InMemoryRateLimitService : IRateLimitService
    {
        private readonly RateLimitSettings _settings;
        private readonly ILogger<InMemoryRateLimitService> _logger;
        private static readonly Dictionary<string, Queue<DateTime>> _requestLog = new Dictionary<string, Queue<DateTime>>();
        private static readonly object _lock = new object();

        public InMemoryRateLimitService(IOptions<RateLimitSettings> options, ILogger<InMemoryRateLimitService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public bool IsRateLimited(string clientIdentifier)
        {
            if (!_settings.Enabled || string.IsNullOrEmpty(clientIdentifier))
            {
                return false;
            }

            lock (_lock)
            {
                CleanupOldRequests();
                
                // Create queue for new clients
                if (!_requestLog.ContainsKey(clientIdentifier))
                {
                    _requestLog[clientIdentifier] = new Queue<DateTime>();
                }

                var clientQueue = _requestLog[clientIdentifier];
                var now = DateTime.UtcNow;

                // If queue is at limit, client is rate limited
                if (clientQueue.Count >= _settings.RequestsPerWindow)
                {
                    _logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientIdentifier);
                    return true;
                }

                // Add request to queue
                clientQueue.Enqueue(now);
                return false;
            }
        }

        private void CleanupOldRequests()
        {
            var cutoff = DateTime.UtcNow.AddSeconds(-_settings.WindowSizeInSeconds);
            
            foreach (var clientId in _requestLog.Keys.ToList())
            {
                var queue = _requestLog[clientId];
                
                // Remove timestamps older than the window
                while (queue.Count > 0 && queue.Peek() < cutoff)
                {
                    queue.Dequeue();
                }
                
                // Remove empty queues to prevent memory leaks
                if (queue.Count == 0)
                {
                    _requestLog.Remove(clientId);
                }
            }
        }
    }
}