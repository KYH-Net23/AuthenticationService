using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace TokenService.Extensions;

public static class RateLimiterExtensions
{
    public static IServiceCollection AddCustomRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    await context.HttpContext.Response.WriteAsync($"Too many requests. Retry after {retryAfter.TotalMinutes} minutes.", cancellationToken: token);
                }
                else
                {
                    await context.HttpContext.Response.WriteAsync($"Too many requests. Retry after {retryAfter.TotalMinutes} minutes.", cancellationToken: token);
                }
            };

            options.AddTokenBucketLimiter("token", tokenOptions =>
            {
                tokenOptions.TokenLimit = 10_000;
                tokenOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                tokenOptions.QueueLimit = 10;
                tokenOptions.ReplenishmentPeriod = TimeSpan.FromDays(1);
                tokenOptions.TokensPerPeriod = 10_000;
                tokenOptions.AutoReplenishment = true;
            });
        });

        return services;
    }
}