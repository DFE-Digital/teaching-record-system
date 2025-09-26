using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TeachingRecordSystem.WebCommon.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class FileExtensionsAttribute : ValidationAttribute
{
    private List<string> AllowedExtensions { get; set; }

    public FileExtensionsAttribute(params string[] fileExtensions)
    {
        AllowedExtensions = fileExtensions.ToList();
    }

    public override bool IsValid(object? value)
    {
        if (value is IFormFile file)
        {
            var fileName = file.FileName;

            return AllowedExtensions.Any(extension => fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
        }

        return true;
    }
}
