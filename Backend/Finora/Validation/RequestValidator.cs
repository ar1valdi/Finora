using Microsoft.Extensions.Logging;

namespace Finora.Validation;

public class RequestValidator : IRequestValidator
{
    private readonly ILogger<RequestValidator> _logger;

    public RequestValidator(ILogger<RequestValidator> logger)
    {
        _logger = logger;
    }

    public async Task<ValidationResult> ValidateAsync<T>(T request, CancellationToken cancellationToken = default) where T : class
    {
        if (request == null)
        {
            return ValidationResult.Failure("Request cannot be null");
        }

        var errors = new List<string>();
        var type = typeof(T);
        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(request);
            var propertyType = property.PropertyType;
            
            // Skip nullable properties (they end with ?)
            if (propertyType.Name.EndsWith("Nullable`1") || property.Name.Contains("?"))
                continue;

            // Check strings - must not be null or empty
            if (propertyType == typeof(string))
            {
                if (string.IsNullOrWhiteSpace(value?.ToString()))
                {
                    errors.Add($"{property.Name} cannot be empty");
                }
            }
            // Check Guids - must not be empty
            else if (propertyType == typeof(Guid))
            {
                if (value == null || (Guid)value == Guid.Empty)
                {
                    errors.Add($"{property.Name} cannot be empty");
                }
            }
            // Check other value types - just not null
            else if (propertyType.IsValueType)
            {
                if (value == null)
                {
                    errors.Add($"{property.Name} is required");
                }
            }
        }

        var result = errors.Any() ? ValidationResult.Failure(errors.ToArray()) : ValidationResult.Success();
        
        if (!result.IsValid)
        {
            _logger.LogWarning("Validation failed for {RequestType}: {Errors}", 
                type.Name, string.Join(", ", errors));
        }

        return result;
    }
}
