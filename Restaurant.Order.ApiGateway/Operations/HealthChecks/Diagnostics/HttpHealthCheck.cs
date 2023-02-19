using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Restaurant.Order.ApiGateway.Operations.HealthChecks.Diagnostics;

public class HttpHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy());
    }
}
