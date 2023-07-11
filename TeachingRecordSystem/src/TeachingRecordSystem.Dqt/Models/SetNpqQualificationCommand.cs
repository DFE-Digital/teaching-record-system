#nullable disable


namespace TeachingRecordSystem.Dqt.Models;

public class SetNpqQualificationCommand
{
    public DateTime? CompletionDate { get; set; }
    public Guid TeacherId { get; set; }
    public dfeta_qualification_dfeta_Type QualificationType { get; set; }
}
