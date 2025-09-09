namespace Finora.Validation;

public interface IRequestValidator
{
    Task<ValidationResult> ValidateAsync<T>(T request, CancellationToken cancellationToken = default) where T : class;
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    
    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Failure(params string[] errors) => new() 
    { 
        IsValid = false, 
        Errors = errors.ToList() 
    };
}
