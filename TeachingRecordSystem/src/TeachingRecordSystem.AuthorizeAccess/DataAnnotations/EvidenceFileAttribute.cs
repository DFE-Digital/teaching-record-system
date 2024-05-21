namespace TeachingRecordSystem.AuthorizeAccess.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class EvidenceFileAttribute : FileExtensionsAttribute
{
    public EvidenceFileAttribute()
        : base(".jpeg", ".jpg", ".pdf", ".png")
    {
        if (ErrorMessage == null)
        {
            ErrorMessage = "The selected file must be a JPEG, JPG, PNG or PDF";
        }
    }
}
