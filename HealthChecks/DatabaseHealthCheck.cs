using Microsoft.Extensions.Diagnostics.HealthChecks;
using AccountService.Services;

namespace AccountService.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly IAccountRepository _repository;

        public DatabaseHealthCheck(IAccountRepository repository)
        {
            _repository = repository;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var isHealthy = await _repository.CheckHealthAsync();
                return isHealthy
                    ? HealthCheckResult.Healthy("База данных доступна")
                    : HealthCheckResult.Unhealthy("Ошибка подключения к базе данных");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Ошибка подключения к базе данных", ex);
            }
        }
    }
}