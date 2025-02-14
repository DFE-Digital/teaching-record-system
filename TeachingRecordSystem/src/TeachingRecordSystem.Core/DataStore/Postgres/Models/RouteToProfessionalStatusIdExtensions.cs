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
        return routeToProfessionalStatusId == Guid.Parse("6F27BDEB-D00A-4EF9-B0EA-26498CE64713") // Apply for QTS
            || routeToProfessionalStatusId == Guid.Parse("2B106B9D-BA39-4E2D-A42E-0CE827FDC324") // European Recognition
            || routeToProfessionalStatusId == Guid.Parse("CE61056E-E681-471E-AF48-5FFBF2653500") // Overseas Trained Teacher Recognition
            || routeToProfessionalStatusId == Guid.Parse("3604EF30-8F11-4494-8B52-A2F9C5371E03") // NI R
            || routeToProfessionalStatusId == Guid.Parse("52835B1F-1F2E-4665-ABC6-7FB1EF0A80BB"); // Scotland R
    }

    public static bool CanBeExemptFromInduction(this Guid routeToProfessionalStatusId)
    {
        return routeToProfessionalStatusId == Guid.Parse("6F27BDEB-D00A-4EF9-B0EA-26498CE64713") // Apply for QTS
            || routeToProfessionalStatusId == Guid.Parse("3604EF30-8F11-4494-8B52-A2F9C5371E03") // NI R
            || routeToProfessionalStatusId == Guid.Parse("52835B1F-1F2E-4665-ABC6-7FB1EF0A80BB") // Scotland R
            || routeToProfessionalStatusId == Guid.Parse("BE6EAF8C-92DD-4EFF-AAD3-1C89C4BEC18C"); // QTLS and SET Membership
    }
}
