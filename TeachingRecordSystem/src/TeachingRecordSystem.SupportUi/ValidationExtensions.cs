using TeachingRecordSystem.SupportUi.Pages;

namespace TeachingRecordSystem.SupportUi;

public static class ValidationExtensions
{
    private static readonly string[] _evidenceFileExtensions =
        [".bmp", ".csv", ".doc", ".docx", ".eml", ".jpeg", ".jpg", ".mbox", ".msg", ".ods", ".odt", ".pdf", ".png", ".tif", ".txt", ".xls", ".xlsx"];

    public static IRuleBuilderOptions<T, IFormFile?> EvidenceFile<T>(this IRuleBuilder<T, IFormFile?> ruleBuilder)
    {
        return ruleBuilder
            .Must(f => f is null || _evidenceFileExtensions.Any(e => f.FileName.EndsWith(e, StringComparison.OrdinalIgnoreCase)))
            .WithMessage("The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX")
            .Must(f => f is null || f.Length <= UiDefaults.MaxFileUploadSizeMb * 1024 * 1024)
            .WithMessage("The selected file must be smaller than 50MB");
    }
}
