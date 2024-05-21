namespace TeachingRecordSystem.AuthorizeAccess.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class EvidenceFileAttribute : FileExtensionsAttribute
{
    public EvidenceFileAttribute()
        : base(".bmp", ".csv", ".doc", ".docx", ".eml", ".jpeg", ".jpg", ".mbox", ".msg", ".ods", ".odt", ".pdf", ".png", ".tif", ".txt", ".xls", ".xlsx")
    {
        if (ErrorMessage == null)
        {
            ErrorMessage = "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX";
        }
    }
}
