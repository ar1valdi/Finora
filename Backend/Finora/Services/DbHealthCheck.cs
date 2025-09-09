using Finora.Persistance.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Finora.Services;

public class DbHealthCheck : IDbHealthCheck
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DbHealthCheck> _logger;

    public DbHealthCheck(IServiceProvider serviceProvider, ILogger<DbHealthCheck> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _serviceProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<FinoraDbContext>();
            
            await context.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return false;
        }
    }
}
