using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace API.HealthChecks;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    public RedisHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var connString = _configuration["ConnectionStrings:Redis"] ?? string.Empty;
            var mux = await ConnectionMultiplexer.ConnectAsync(connString);
            var db = mux.GetDatabase();
            var pong = await db.PingAsync();
            return HealthCheckResult.Healthy($"Ping: {pong.TotalMilliseconds} ms");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
