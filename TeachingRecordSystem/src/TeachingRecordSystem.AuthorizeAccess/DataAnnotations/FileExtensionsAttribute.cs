using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.AuthorizeAccess.DataAnnotations;

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
        var file = value as IFormFile;
        if (file is null)
        {
            throw new NotSupportedException("FileExtensionsAttribute can only be used on property of type IFormFile");
        }

        var fileName = file.FileName;
        return AllowedExtensions.Any(extension => fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
    }
}
