using MediatR;
using Microsoft.Extensions.Logging;

namespace Finora.Validation;

public class RequestValidator(ILogger<RequestValidator> _logger) : IRequestValidator
{
    public ValidationResult Validate<T>(T request, CancellationToken cancellationToken = default) where T : class
    {
        if (request == null)
        {
            return new ValidationResult { IsValid = false, Errors = ["Request cannot be null"] };
        }

        var errors = new List<string>();
        var type = typeof(T);
        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(request);
            var propertyType = property.PropertyType;
            
            if (propertyType.Name.EndsWith("Nullable`1") || property.Name.Contains("?"))
                continue;

            if (propertyType == typeof(string))
            {
                if (string.IsNullOrWhiteSpace(value?.ToString()))
                {
                    errors.Add($"{property.Name} cannot be empty");
                }
            }
            else if (propertyType == typeof(Guid))
            {
                if (value == null || (Guid)value == Guid.Empty)
                {
                    errors.Add($"{property.Name} cannot be empty");
                }
            }
            else if (propertyType.IsValueType)
            {
                if (value == null)
                {
                    errors.Add($"{property.Name} is required");
                }
            }
        }

        var result = new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };

        if (!result.IsValid)
        {
            _logger.LogWarning("Validation failed for {RequestType}: {Errors}", 
                type.Name, string.Join(", ", errors));
        }

        return result;
    }
}
