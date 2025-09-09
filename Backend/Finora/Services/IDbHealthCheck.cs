namespace Finora.Services;

public interface IDbHealthCheck
{
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}
