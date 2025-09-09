namespace Finora.Validation;

public interface IRequestValidator
{
    ValidationResult Validate<T>(T request, CancellationToken cancellationToken = default) where T : class;
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
