using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace AccountService.HealthChecks
{
    public class MemoryHealthCheck : IHealthCheck
    {
        private readonly IOptions<MemoryCheckOptions> _options;

        public MemoryHealthCheck(IOptions<MemoryCheckOptions> options)
        {
            _options = options;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var allocated = GC.GetTotalMemory(forceFullCollection: false);
            var threshold = _options.Value.ThresholdBytes;

            var status = allocated < threshold ?
                HealthStatus.Healthy :
                HealthStatus.Unhealthy;

            return Task.FromResult(new HealthCheckResult(
                status,
                description: $"»спользовано пам€ти: {allocated} байт (порог: {threshold} байт)",
                data: new Dictionary<string, object>
                {
                    {"allocated_bytes", allocated},
                    {"threshold_bytes", threshold}
                }));
        }
    }

    public class MemoryCheckOptions
    {
        public long ThresholdBytes { get; set; } = 100 * 1024 * 1024; // 100 MB по умолчанию
    }
}