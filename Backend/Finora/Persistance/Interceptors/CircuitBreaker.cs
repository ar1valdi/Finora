namespace Finora.Persistance.Interceptors;

using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

public class DatabaseCircuitBreakerInterceptor(
    ILogger<DatabaseCircuitBreakerInterceptor> _logger,
    IConfiguration _configuration
) : DbCommandInterceptor, IDbConnectionInterceptor, IDbTransactionInterceptor, IDisposable
{
    private enum CircuitState
    {
        Closed,
        Open,
        HalfOpen
    }

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly SemaphoreSlim _halfOpenSemaphore = new(3, 3);

    private CircuitState _circuitState = CircuitState.Closed;
    private int _consecutiveFailures = 0;
    private int _halfOpenSuccessCount = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;

    private readonly int _failureThreshold = _configuration.GetValue<int>("DatabaseCircuitBreaker:FailureThreshold");
    private readonly int _retryTimeout = _configuration.GetValue<int>("DatabaseCircuitBreaker:OpenTime");
    private readonly int _halfOpenSuccessThreshold = _configuration.GetValue<int>("DatabaseCircuitBreaker:HalfOpenSuccessThreshold");
    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        CheckCircuitBreaker();
        return await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CheckCircuitBreaker();
        return await base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        CheckCircuitBreaker();
        return await base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        HandleSuccess();
        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override async ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        HandleSuccess();
        return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override async ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        HandleSuccess();
        return await base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override async Task CommandFailedAsync(
        DbCommand command,
        CommandErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Command failed: {Error}", eventData.Exception?.Message);
        HandleFailure();
        await base.CommandFailedAsync(command, eventData, cancellationToken);
    }

    // conn interceptors
    public InterceptionResult ConnectionOpening(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result)
    {
        CheckCircuitBreaker();
        return result;
    }

    public ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        CheckCircuitBreaker();
        return ValueTask.FromResult(result);
    }

    public void ConnectionFailed(
        DbConnection connection,
        ConnectionErrorEventData eventData)
    {
        _logger.LogWarning("Connection failed: {Error}", eventData.Exception?.Message);
        HandleFailure();
    }

    public Task ConnectionFailedAsync(
        DbConnection connection,
        ConnectionErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Connection failed async: {Error}", eventData.Exception?.Message);
        HandleFailure();
        return Task.CompletedTask;
    }

    // trans interceptors
    public InterceptionResult<DbTransaction> TransactionStarting(
        DbConnection connection,
        TransactionStartingEventData eventData,
        InterceptionResult<DbTransaction> result)
    {
        CheckCircuitBreaker();
        return result;
    }

    public ValueTask<InterceptionResult<DbTransaction>> TransactionStartingAsync(
        DbConnection connection,
        TransactionStartingEventData eventData,
        InterceptionResult<DbTransaction> result,
        CancellationToken cancellationToken = default)
    {
        CheckCircuitBreaker();
        return ValueTask.FromResult(result);
    }

    public void TransactionFailed(
        DbTransaction transaction,
        TransactionErrorEventData eventData)
    {
        _logger.LogWarning("Transaction failed: {Error}", eventData.Exception?.Message);
        HandleFailure();
    }

    public Task TransactionFailedAsync(
        DbTransaction transaction,
        TransactionErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Transaction failed async: {Error}", eventData.Exception?.Message);
        HandleFailure();
        return Task.CompletedTask;
    }

    public InterceptionResult TransactionCommitting(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result)
    {
        CheckCircuitBreaker();
        return result;
    }

    public ValueTask<InterceptionResult> TransactionCommittingAsync(
        DbTransaction transaction,
        TransactionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        CheckCircuitBreaker();
        return ValueTask.FromResult(result);
    }

    public void TransactionCommitted(
        DbTransaction transaction,
        TransactionEndEventData eventData)
    {
        HandleSuccess();
    }

    public Task TransactionCommittedAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        HandleSuccess();
        return Task.CompletedTask;
    }

    private void CheckCircuitBreaker()
    {
        switch (_circuitState)
        {
            case CircuitState.Closed:
                break;

            case CircuitState.Open:
                if (DateTime.UtcNow - _lastFailureTime > TimeSpan.FromSeconds(_retryTimeout))
                {
                    _circuitState = CircuitState.HalfOpen;
                    _halfOpenSuccessCount = 0;
                    _logger.LogInformation("Circuit breaker moved to HALF-OPEN - testing database recovery");
                }
                else
                {
                    throw new Exception("Database is currently unavailable");
                }
                break;
                
            case CircuitState.HalfOpen:
                if (!_halfOpenSemaphore.Wait(100))
                {
                    throw new Exception("Database is in recovery mode - too many concurrent requests");
                }
                break;
        }
    }

    private void HandleSuccess()
    {
        _semaphore.Wait();
        try
        {
            if (_circuitState == CircuitState.HalfOpen)
            {
                _halfOpenSuccessCount++;
                _logger.LogInformation("Half-Open success count: {SuccessCount}", _halfOpenSuccessCount);
                
                if (_halfOpenSuccessCount >= _halfOpenSuccessThreshold)
                {
                    _circuitState = CircuitState.Closed;
                    _consecutiveFailures = 0;
                    _halfOpenSuccessCount = 0;
                    _logger.LogInformation("Circuit breaker moved to CLOSED - database recovered");
                }
            }
            else
            {
                _consecutiveFailures = 0;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void HandleFailure()
    {
        _semaphore.Wait();
        try
        {
            _consecutiveFailures++;
            _lastFailureTime = DateTime.UtcNow;

            if (_circuitState == CircuitState.HalfOpen)
            {
                _circuitState = CircuitState.Open;
                _halfOpenSuccessCount = 0;
                _logger.LogError("Circuit breaker moved back to OPEN from Half-Open after failure");
            }
            else if (_consecutiveFailures >= _failureThreshold)
            {
                _circuitState = CircuitState.Open;
                _logger.LogError("Database circuit breaker opened after {FailureCount} consecutive failures", _consecutiveFailures);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        _semaphore?.Dispose();
        _halfOpenSemaphore?.Dispose();
    }
}