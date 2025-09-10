using System.Threading;

namespace Finora.Services;

public enum CircuitState
{
    Closed,
    Open,
    HalfOpen
}

public interface ICircuitBreakerStateHolder
{
    CircuitState Get();
    void Set(CircuitState newState);
}

public class CircuitBreakerStateHolder : ICircuitBreakerStateHolder, IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private CircuitState _state = CircuitState.Closed;

    public CircuitState Get()
    {
        _semaphore.Wait();
        try
        {
            return _state;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Set(CircuitState newState)
    {
        _semaphore.Wait();
        try
        {
            _state = newState;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}


