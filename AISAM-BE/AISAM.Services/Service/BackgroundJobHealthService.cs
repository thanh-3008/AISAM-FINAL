using System.Collections.Concurrent;
using AISAM.Services.IServices;

namespace AISAM.Services.Service
{
    public class ServiceStatus
    {
        public string Name { get; set; } = "";
        public string Status { get; set; } = "Unknown";
        public DateTime LastHeartbeat { get; set; }
        public int _successCount;
        public int _failureCount;
        public string? LastError { get; set; }
        public DateTime? LastErrorTime { get; set; }
    }

    public class BackgroundJobHealthService : IBackgroundJobHealthService
    {
        private readonly ConcurrentDictionary<string, ServiceStatus> _services = new();

        public void ReportHeartbeat(string serviceName)
        {
            var status = _services.GetOrAdd(serviceName, _ => new ServiceStatus { Name = serviceName });
            status.LastHeartbeat = DateTime.UtcNow;
            status.Status = "Running";
        }

        public void ReportSuccess(string serviceName)
        {
            var status = _services.GetOrAdd(serviceName, _ => new ServiceStatus { Name = serviceName });
            Interlocked.Increment(ref status._successCount);
            status.LastHeartbeat = DateTime.UtcNow;
            status.Status = "Running";
        }

        public void ReportFailure(string serviceName, string error)
        {
            var status = _services.GetOrAdd(serviceName, _ => new ServiceStatus { Name = serviceName });
            Interlocked.Increment(ref status._failureCount);
            status.LastError = error;
            status.LastErrorTime = DateTime.UtcNow;
            status.Status = "Degraded";
        }

        public Task<object> GetStatusAsync()
        {
            var services = _services.Values.Select(s => new
            {
                s.Name,
                s.Status,
                LastHeartbeat = s.LastHeartbeat,
                SuccessCount = s._successCount,
                s._failureCount,
                s.LastError,
                s.LastErrorTime,
                IsStale = (DateTime.UtcNow - s.LastHeartbeat).TotalMinutes > 5
            }).ToList();

            var knownServices = new[] {
                "ScheduledPosting", "VideoGeneration", "VideoPolling",
                "AutomationGeneration", "AutomationOperations"
            };
            foreach (var name in knownServices)
            {
                if (!services.Any(s => s.Name == name))
                {
                    services.Add(new
                    {
                        Name = name,
                        Status = "Not Started",
                        LastHeartbeat = DateTime.MinValue,
                        SuccessCount = 0,
                        _failureCount = 0,
                        LastError = (string?)null,
                        LastErrorTime = (DateTime?)null,
                        IsStale = true
                    });
                }
            }

            return Task.FromResult<object>(new
            {
                Services = services,
                OverallStatus = services.Any(s => s.Status == "Degraded") ? "Degraded"
                    : services.Any(s => s.IsStale) ? "Warning"
                    : services.All(s => s.Status == "Running") ? "Healthy"
                    : "Unknown"
            });
        }
    }
}
