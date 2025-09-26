using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TeachingRecordSystem.WebCommon.DataAnnotations;

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
        if (value is IFormFile file && file.Length > _maxFileSize)
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success!;
    }
}
