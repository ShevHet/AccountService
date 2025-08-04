using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Http.Headers;
using System.Net;

namespace AccountService.HealthChecks
{
    public class KeycloakHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        private readonly string _keycloakAuthority;

        public KeycloakHealthCheck(HttpClient httpClient, string keycloakAuthority)
        {
            _httpClient = httpClient;
            _keycloakAuthority = keycloakAuthority;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{_keycloakAuthority}/.well-known/openid-configuration",
                    cancellationToken);

                return response.StatusCode == HttpStatusCode.OK
                    ? HealthCheckResult.Healthy("Keycloak доступен")
                    : HealthCheckResult.Unhealthy($"Keycloak недоступен. Код: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Ошибка подключения к Keycloak", ex);
            }
        }
    }
}