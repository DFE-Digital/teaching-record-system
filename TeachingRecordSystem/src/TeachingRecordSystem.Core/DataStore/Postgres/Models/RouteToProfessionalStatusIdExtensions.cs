namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public static class RouteToProfessionalStatusIdExtensions
{
    public static bool IsEarlyYears(this Guid routeToProfessionalStatusId)
    {
        return routeToProfessionalStatusId == Guid.Parse("D9EEF3F8-FDE6-4A3F-A361-F6655A42FA1E") // Early Years ITT Assessment Only
            || routeToProfessionalStatusId == Guid.Parse("4477E45D-C531-4C63-9F4B-E157766366FB") // Early Years ITT Graduate Employment Based
            || routeToProfessionalStatusId == Guid.Parse("DBC4125B-9235-41E4-ABD2-BAABBF63F829") // Early Years ITT Graduate Entry
            || routeToProfessionalStatusId == Guid.Parse("7F09002C-5DAD-4839-9693-5E030D037AE9") // Early Years ITT School Direct
            || routeToProfessionalStatusId == Guid.Parse("C97C0FD2-FD84-4949-97C7-B0E2422FB3C8"); // Early Years ITT Undergraduate
    }

    public static bool IsOverseas(this Guid routeToProfessionalStatusId)
    {
        return routeToProfessionalStatusId == RouteToProfessionalStatus.ApplyforQtsId
            || routeToProfessionalStatusId == RouteToProfessionalStatus.EuropeanRecognitionId
            || routeToProfessionalStatusId == RouteToProfessionalStatus.OverseasTrainedTeacherRecognitionId
            || routeToProfessionalStatusId == RouteToProfessionalStatus.NiRId
            || routeToProfessionalStatusId == RouteToProfessionalStatus.ScotlandRId;
    }

    public static bool CanBeExemptFromInduction(this Guid routeToProfessionalStatusId)
    {
        return routeToProfessionalStatusId == RouteToProfessionalStatus.ApplyforQtsId
            || routeToProfessionalStatusId == RouteToProfessionalStatus.NiRId
            || routeToProfessionalStatusId == RouteToProfessionalStatus.ScotlandRId
            || routeToProfessionalStatusId == RouteToProfessionalStatus.QtlsAndSetMembershipId;
    }
}
