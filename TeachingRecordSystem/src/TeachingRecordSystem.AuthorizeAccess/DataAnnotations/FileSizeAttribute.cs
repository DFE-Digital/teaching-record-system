using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.AuthorizeAccess.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class FileSizeAttribute : ValidationAttribute
{
    private readonly int _maxFileSize;

    public FileSizeAttribute(int maxFileSize)
    {
        _maxFileSize = maxFileSize;
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        var file = value as IFormFile;
        if (file is null)
        {
            throw new NotSupportedException("FileSizeAttribute can only be used on property of type IFormFile");

        }

        if (file.Length > _maxFileSize)
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success!;
    }
}
