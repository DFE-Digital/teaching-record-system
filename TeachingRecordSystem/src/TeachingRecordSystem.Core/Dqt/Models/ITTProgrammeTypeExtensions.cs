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

    public static (dfeta_ITTProgrammeType?, RecognitionRouteType?) ConvertFromTrsRouteType(this Guid routeTypeId)
    {
        if (routeTypeId.TryConvertFromTrsRouteType(out var result))
        {
            throw new FormatException($"Unknown {typeof(Guid).Name}: '{routeTypeId}'.");
        }

        return result;
    }

    public static bool TryConvertFromTrsRouteType(this Guid routeTypeId, out (dfeta_ITTProgrammeType?, RecognitionRouteType?) result)
    {
        var mapped = routeTypeId switch
        {
            var guid when guid == Guid.Parse("6F27BDEB-D00A-4EF9-B0EA-26498CE64713") => (null, RecognitionRouteType.OverseasTrainedTeachers), // Apply for QTS
            var guid when guid == Guid.Parse("6987240E-966E-485F-B300-23B54937FB3A") => (dfeta_ITTProgrammeType.Apprenticeship, null),
            var guid when guid == Guid.Parse("57B86CEF-98E2-4962-A74A-D47C7A34B838") => (dfeta_ITTProgrammeType.AssessmentOnlyRoute, null),
            var guid when guid == Guid.Parse("4163C2FB-6163-409F-85FD-56E7C70A54DD") => (dfeta_ITTProgrammeType.Core, null),
            var guid when guid == Guid.Parse("4BD7A9F0-28CA-4977-A044-A7B7828D469B") => (dfeta_ITTProgrammeType.CoreFlexible, null),
            var guid when guid == Guid.Parse("D9EEF3F8-FDE6-4A3F-A361-F6655A42FA1E") => (dfeta_ITTProgrammeType.EYITTAssessmentOnly, null),
            var guid when guid == Guid.Parse("4477E45D-C531-4C63-9F4B-E157766366FB") => (dfeta_ITTProgrammeType.EYITTGraduateEmploymentBased, null),
            var guid when guid == Guid.Parse("DBC4125B-9235-41E4-ABD2-BAABBF63F829") => (dfeta_ITTProgrammeType.EYITTGraduateEntry, null),
            var guid when guid == Guid.Parse("7F09002C-5DAD-4839-9693-5E030D037AE9") => (dfeta_ITTProgrammeType.EYITTSchoolDirect_EarlyYears, null),
            var guid when guid == Guid.Parse("C97C0FD2-FD84-4949-97C7-B0E2422FB3C8") => (dfeta_ITTProgrammeType.EYITTUndergraduate, null),
            var guid when guid == Guid.Parse("F4DA123B-5C37-4060-AB00-52DE4BD3599E") => (null, RecognitionRouteType.EuropeanEconomicArea), // EC directive
            var guid when guid == Guid.Parse("2B106B9D-BA39-4E2D-A42E-0CE827FDC324") => (null, RecognitionRouteType.EuropeanEconomicArea), // European Recognition
            var guid when guid == Guid.Parse("EC95C276-25D9-491F-99A2-8D92F10E1E94") => (null, RecognitionRouteType.EuropeanEconomicArea), // European Recognition - PQTS
            var guid when guid == Guid.Parse("F85962C9-CF0C-415D-9DE5-A397F95AE261") => (dfeta_ITTProgrammeType.FutureTeachingScholars, null),
            var guid when guid == Guid.Parse("34222549-ED59-4C4A-811D-C0894E78D4C3") => (dfeta_ITTProgrammeType.GraduateTeacherProgramme, null),
            var guid when guid == Guid.Parse("10078157-E8C3-42F7-A050-D8B802E83F7B") => (dfeta_ITTProgrammeType.HEI, null),
            var guid when guid == Guid.Parse("BFEF20B2-5AC4-486D-9493-E5A4538E1BE9") => (dfeta_ITTProgrammeType.HighpotentialITT, null),
            var guid when guid == Guid.Parse("D0B60864-AB1C-4D49-A5C2-FF4BD9872EE1") => (dfeta_ITTProgrammeType.Internationalqualifiedteacherstatus, null),
            var guid when guid == Guid.Parse("2B4862CA-BD30-4A3A-BFCE-52B57C2946C7") => (dfeta_ITTProgrammeType.LicensedTeacherProgramme, null),
            var guid when guid == Guid.Parse("3604EF30-8F11-4494-8B52-A2F9C5371E03") => (null, RecognitionRouteType.NorthernIreland), // NI R
            var guid when guid == Guid.Parse("51756384-CFEA-4F63-80E5-F193686E0F71") => (dfeta_ITTProgrammeType.OverseasTrainedTeacherProgramme, null),
            var guid when guid == Guid.Parse("CE61056E-E681-471E-AF48-5FFBF2653500") => (null, RecognitionRouteType.OverseasTrainedTeachers), // Overseas Trained Teacher Recognition
            var guid when guid == Guid.Parse("EF46FF51-8DC0-481E-B158-61CCEA9943D9") => (dfeta_ITTProgrammeType.Primaryandsecondarypostgraduatefeefunded, null),
            var guid when guid == Guid.Parse("321D5F9A-9581-4936-9F63-CFDDD2A95FE2") => (dfeta_ITTProgrammeType.Primaryandsecondaryundergraduatefeefunded, null),
            var guid when guid == Guid.Parse("97497716-5AC5-49AA-A444-27FA3E2C152A") => (dfeta_ITTProgrammeType.Providerled_postgrad, null),
            var guid when guid == Guid.Parse("53A7FBDA-25FD-4482-9881-5CF65053888D") => (dfeta_ITTProgrammeType.Providerled_undergrad, null),
            var guid when guid == Guid.Parse("70368FF2-8D2B-467E-AD23-EFE7F79995D7") => (dfeta_ITTProgrammeType.RegisteredTeacherProgramme, null),
            var guid when guid == Guid.Parse("D9490E58-ACDC-4A38-B13E-5A5C21417737") => (dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme, null),
            var guid when guid == Guid.Parse("12A742C3-1CD4-43B7-A2FA-1000BD4CC373") => (dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Salaried, null),
            var guid when guid == Guid.Parse("97E1811B-D46C-483E-AEC3-4A2DD51A55FE") => (dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Selffunded, null),
            var guid when guid == Guid.Parse("52835B1F-1F2E-4665-ABC6-7FB1EF0A80BB") => (null, RecognitionRouteType.Scotland), // Scotland R
            var guid when guid == Guid.Parse("5B7F5E90-1CA6-4529-BAA0-DFBA68E698B8") => (dfeta_ITTProgrammeType.TeachFirstProgramme, null),
            var guid when guid == Guid.Parse("20F67E38-F117-4B42-BBFC-5812AA717B94") => (dfeta_ITTProgrammeType.UndergraduateOptIn, null),
            _ => ((dfeta_ITTProgrammeType?, RecognitionRouteType?)?)null
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
