using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public sealed class ElasticsearchHealthCheck(ElasticsearchClient client) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken ct = default)
    {
        try
        {
            var response = await client.PingAsync(ct);

            if (response.IsValidResponse)
            {
                return HealthCheckResult.Healthy("Elastic Search is responding");
            }

            return HealthCheckResult.Unhealthy(
                "Elastic Search Ping Failed"
            );
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Elasticsearch is unreachable.", ex
            );
        }
    }
}

