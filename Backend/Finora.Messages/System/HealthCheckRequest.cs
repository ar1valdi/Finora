using Finora.Messages.Interfaces;

namespace Finora.Messages.System;

public class HealthCheckRequest : IQuery
{
    public string Component { get; set; } = "system";
}
