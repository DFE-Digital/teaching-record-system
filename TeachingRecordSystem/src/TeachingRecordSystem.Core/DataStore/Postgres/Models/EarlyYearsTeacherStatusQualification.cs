namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class EarlyYearsTeacherStatusQualification : Qualification
{
    public required DateOnly AwardedDate { get; set; }

    public Guid? DqtQtsRegistrationId { get; set; }
}
