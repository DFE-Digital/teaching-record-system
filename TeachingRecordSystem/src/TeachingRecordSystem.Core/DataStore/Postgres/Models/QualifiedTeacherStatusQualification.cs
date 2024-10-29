namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class QualifiedTeacherStatusQualification : Qualification
{
    public required DateOnly AwardedDate { get; set; }

    public Guid? DqtQtsRegistrationId { get; set; }
}
