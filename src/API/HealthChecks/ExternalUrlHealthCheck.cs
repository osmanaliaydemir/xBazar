using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace API.HealthChecks;

public class ExternalUrlHealthCheck : IHealthCheck
{
    private readonly Uri _url;
    private readonly IHttpClientFactory _httpClientFactory;
    public ExternalUrlHealthCheck(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        var url = configuration["Payment:BaseUrl"] ?? "https://sandbox-api.iyzipay.com";
        _url = new Uri(url);
        _httpClientFactory = httpClientFactory;
    }
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            using var resp = await client.GetAsync(_url, cancellationToken);
            return resp.IsSuccessStatusCode ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy($"Status {(int)resp.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
