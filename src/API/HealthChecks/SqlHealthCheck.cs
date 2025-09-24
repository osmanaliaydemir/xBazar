using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Data.SqlClient;

namespace API.HealthChecks;

public class SqlHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    public SqlHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var connString = _configuration["ConnectionStrings:DefaultConnection"];
            await using var conn = new SqlConnection(connString);
            await conn.OpenAsync(cancellationToken);
            await using var cmd = new SqlCommand("SELECT 1", conn);
            await cmd.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
