using System.ComponentModel.DataAnnotations;

namespace Finora.Validation;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class RequiredNotEmptyAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null)
            return false;

        if (value is string str)
            return !string.IsNullOrWhiteSpace(str);

        if (value is Guid guid)
            return guid != Guid.Empty;

        return true;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"The field {name} is required and cannot be empty.";
    }
}
