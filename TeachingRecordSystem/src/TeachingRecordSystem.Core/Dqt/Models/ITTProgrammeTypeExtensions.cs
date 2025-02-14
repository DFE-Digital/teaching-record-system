namespace TeachingRecordSystem.Core.Dqt.Models;

public static class ITTProgrammeTypeExtensions
{
    public static bool IsEarlyYears(this dfeta_ITTProgrammeType programmeType) => programmeType switch
    {
        dfeta_ITTProgrammeType.EYITTAssessmentOnly => true,
        dfeta_ITTProgrammeType.EYITTGraduateEmploymentBased => true,
        dfeta_ITTProgrammeType.EYITTGraduateEntry => true,
        dfeta_ITTProgrammeType.EYITTSchoolDirect_EarlyYears => true,
        dfeta_ITTProgrammeType.EYITTUndergraduate => true,
        _ => false
    };

    public static dfeta_ITTProgrammeType? ConvertFromTrsRouteType(this Guid routeTypeId)
    {
        if (routeTypeId.TryConvertFromTrsRouteType(out var result))
        {
            throw new FormatException($"Unknown {typeof(Guid).Name}: '{routeTypeId}'.");
        }

        return result;
    }

    public static bool TryConvertFromTrsRouteType(this Guid routeTypeId, out dfeta_ITTProgrammeType? result)
    {
        var mapped = routeTypeId switch
        {
            var guid when guid == Guid.Parse("6987240E-966E-485F-B300-23B54937FB3A") => dfeta_ITTProgrammeType.Apprenticeship,
            var guid when guid == Guid.Parse("57B86CEF-98E2-4962-A74A-D47C7A34B838") => dfeta_ITTProgrammeType.AssessmentOnlyRoute,
            var guid when guid == Guid.Parse("4163C2FB-6163-409F-85FD-56E7C70A54DD") => dfeta_ITTProgrammeType.Core,
            var guid when guid == Guid.Parse("4BD7A9F0-28CA-4977-A044-A7B7828D469B") => dfeta_ITTProgrammeType.CoreFlexible,
            var guid when guid == Guid.Parse("D9EEF3F8-FDE6-4A3F-A361-F6655A42FA1E") => dfeta_ITTProgrammeType.EYITTAssessmentOnly,
            var guid when guid == Guid.Parse("4477E45D-C531-4C63-9F4B-E157766366FB") => dfeta_ITTProgrammeType.EYITTGraduateEmploymentBased,
            var guid when guid == Guid.Parse("DBC4125B-9235-41E4-ABD2-BAABBF63F829") => dfeta_ITTProgrammeType.EYITTGraduateEntry,
            var guid when guid == Guid.Parse("7F09002C-5DAD-4839-9693-5E030D037AE9") => dfeta_ITTProgrammeType.EYITTSchoolDirect_EarlyYears,
            var guid when guid == Guid.Parse("C97C0FD2-FD84-4949-97C7-B0E2422FB3C8") => dfeta_ITTProgrammeType.EYITTUndergraduate,
            var guid when guid == Guid.Parse("F85962C9-CF0C-415D-9DE5-A397F95AE261") => dfeta_ITTProgrammeType.FutureTeachingScholars,
            var guid when guid == Guid.Parse("34222549-ED59-4C4A-811D-C0894E78D4C3") => dfeta_ITTProgrammeType.GraduateTeacherProgramme,
            var guid when guid == Guid.Parse("10078157-E8C3-42F7-A050-D8B802E83F7B") => dfeta_ITTProgrammeType.HEI,
            var guid when guid == Guid.Parse("BFEF20B2-5AC4-486D-9493-E5A4538E1BE9") => dfeta_ITTProgrammeType.HighpotentialITT,
            var guid when guid == Guid.Parse("D0B60864-AB1C-4D49-A5C2-FF4BD9872EE1") => dfeta_ITTProgrammeType.Internationalqualifiedteacherstatus,
            var guid when guid == Guid.Parse("2B4862CA-BD30-4A3A-BFCE-52B57C2946C7") => dfeta_ITTProgrammeType.LicensedTeacherProgramme,
            var guid when guid == Guid.Parse("51756384-CFEA-4F63-80E5-F193686E0F71") => dfeta_ITTProgrammeType.OverseasTrainedTeacherProgramme,
            var guid when guid == Guid.Parse("EF46FF51-8DC0-481E-B158-61CCEA9943D9") => dfeta_ITTProgrammeType.Primaryandsecondarypostgraduatefeefunded,
            var guid when guid == Guid.Parse("321D5F9A-9581-4936-9F63-CFDDD2A95FE2") => dfeta_ITTProgrammeType.Primaryandsecondaryundergraduatefeefunded,
            var guid when guid == Guid.Parse("97497716-5AC5-49AA-A444-27FA3E2C152A") => dfeta_ITTProgrammeType.Providerled_postgrad,
            var guid when guid == Guid.Parse("53A7FBDA-25FD-4482-9881-5CF65053888D") => dfeta_ITTProgrammeType.Providerled_undergrad,
            var guid when guid == Guid.Parse("70368FF2-8D2B-467E-AD23-EFE7F79995D7") => dfeta_ITTProgrammeType.RegisteredTeacherProgramme,
            var guid when guid == Guid.Parse("D9490E58-ACDC-4A38-B13E-5A5C21417737") => dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme,
            var guid when guid == Guid.Parse("12A742C3-1CD4-43B7-A2FA-1000BD4CC373") => dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Salaried,
            var guid when guid == Guid.Parse("97E1811B-D46C-483E-AEC3-4A2DD51A55FE") => dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Selffunded,
            var guid when guid == Guid.Parse("5B7F5E90-1CA6-4529-BAA0-DFBA68E698B8") => dfeta_ITTProgrammeType.TeachFirstProgramme,
            var guid when guid == Guid.Parse("20F67E38-F117-4B42-BBFC-5812AA717B94") => dfeta_ITTProgrammeType.UndergraduateOptIn,
            _ => (dfeta_ITTProgrammeType?)null
        };

        if (mapped.HasValue)
        {
            result = mapped.Value;
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }
}
