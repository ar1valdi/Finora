using Finora.Messages.System;
using Finora.Messages.Wrappers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Finora.Handlers.System;

public class HealthCheckHandler(ILogger<HealthCheckHandler> _logger) : IRequestHandler<HealthCheckRequest, RabbitResponse<object>>
{
    public async Task<RabbitResponse<object>> Handle(HealthCheckRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Health check");
        
        return new RabbitResponse<object>
        {
            Data = new { 
                status = "healthy"
            },
            StatusCode = 200,
            Errors = []
        };
    }
}
