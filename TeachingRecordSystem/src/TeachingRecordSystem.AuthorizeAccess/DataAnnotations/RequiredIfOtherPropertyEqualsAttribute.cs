using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.AuthorizeAccess.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class RequiredIfOtherPropertyEqualsAttribute : RequiredAttribute
{
    private readonly string _propertyName;
    private readonly string? _targetValue;

    public RequiredIfOtherPropertyEqualsAttribute(string propertyName, string? targetValue = null)
    {
        _propertyName = propertyName;
        _targetValue = targetValue;
    }

    protected override ValidationResult IsValid(object? value, ValidationContext context)
    {
        object instance = context.ObjectInstance;
        Type type = instance.GetType();

        var property = type.GetProperty(_propertyName);
        if (property == null)
        {
            throw new ArgumentException($"Property {_propertyName} not found");
        }

        var propertyValue = property.GetValue(instance)?.ToString();
        bool isRequired;

        if (_targetValue is null)
        {
            bool.TryParse(propertyValue, out isRequired);
        }
        else
        {
            isRequired = propertyValue == _targetValue;
        }

        return isRequired && !base.IsValid(value) ? new ValidationResult(ErrorMessage) : ValidationResult.Success!;
    }
}
