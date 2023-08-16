#nullable disable


namespace TeachingRecordSystem.Core.Dqt.Models;

public class SetNpqQualificationCommand
{
    public DateTime? CompletionDate { get; set; }
    public Guid TeacherId { get; set; }
    public dfeta_qualification_dfeta_Type QualificationType { get; set; }
}
