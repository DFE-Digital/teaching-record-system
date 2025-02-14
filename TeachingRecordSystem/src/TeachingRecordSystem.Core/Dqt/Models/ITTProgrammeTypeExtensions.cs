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

    public static dfeta_ITTProgrammeType? ConvertFromTrsRouteType(this Guid routeTypeId) => routeTypeId switch
    {
        var guid when guid == Guid.Parse("6F27BDEB-D00A-4EF9-B0EA-26498CE64713") => null, // Apply for QTS
        var guid when guid == Guid.Parse("6987240E-966E-485F-B300-23B54937FB3A") => dfeta_ITTProgrammeType.Apprenticeship,
        var guid when guid == Guid.Parse("57B86CEF-98E2-4962-A74A-D47C7A34B838") => dfeta_ITTProgrammeType.AssessmentOnlyRoute,
        var guid when guid == Guid.Parse("4B6FC697-BE67-43D3-9021-CC662C4A559F") => null, // Authorised Teacher Programme
        var guid when guid == Guid.Parse("4163C2FB-6163-409F-85FD-56E7C70A54DD") => dfeta_ITTProgrammeType.Core,
        var guid when guid == Guid.Parse("4BD7A9F0-28CA-4977-A044-A7B7828D469B") => dfeta_ITTProgrammeType.CoreFlexible,
        var guid when guid == Guid.Parse("5748D41D-7B53-4EE6-833A-83080A3BD8EF") => null, // CTC or CCTA
        var guid when guid == Guid.Parse("D9EEF3F8-FDE6-4A3F-A361-F6655A42FA1E") => dfeta_ITTProgrammeType.EYITTAssessmentOnly,
        var guid when guid == Guid.Parse("4477E45D-C531-4C63-9F4B-E157766366FB") => dfeta_ITTProgrammeType.EYITTGraduateEmploymentBased,
        var guid when guid == Guid.Parse("DBC4125B-9235-41E4-ABD2-BAABBF63F829") => dfeta_ITTProgrammeType.EYITTGraduateEntry,
        var guid when guid == Guid.Parse("7F09002C-5DAD-4839-9693-5E030D037AE9") => dfeta_ITTProgrammeType.EYITTSchoolDirect_EarlyYears,
        var guid when guid == Guid.Parse("C97C0FD2-FD84-4949-97C7-B0E2422FB3C8") => dfeta_ITTProgrammeType.EYITTUndergraduate,
        var guid when guid == Guid.Parse("F4DA123B-5C37-4060-AB00-52DE4BD3599E") => null, // EC directive
        var guid when guid == Guid.Parse("2B106B9D-BA39-4E2D-A42E-0CE827FDC324") => null, // European Recognition
        var guid when guid == Guid.Parse("EC95C276-25D9-491F-99A2-8D92F10E1E94") => null, // European Recognition - PQTS
        var guid when guid == Guid.Parse("8F5C0431-D006-4EDA-9336-16DFC6A26A78") => null, // EYPS
        var guid when guid == Guid.Parse("EBA0B7AE-CBCE-44D5-A56F-988D69B03001") => null, // EYPS ITT Migrated
        var guid when guid == Guid.Parse("5B7D1C4E-FB2B-479C-BDEE-5818DAAA8A07") => null, // EYTS ITT Migrated
        var guid when guid == Guid.Parse("45C93B5B-B4DC-4D0F-B0DE-D612521E0A13") => null, // FE Recognition 2000-2004
        var guid when guid == Guid.Parse("700EC96F-6BBF-4080-87BD-94EF65A6A879") => null, // Flexible ITT
        var guid when guid == Guid.Parse("F85962C9-CF0C-415D-9DE5-A397F95AE261") => dfeta_ITTProgrammeType.FutureTeachingScholars,
        var guid when guid == Guid.Parse("A6431D4B-E4CD-4E59-886B-358221237E75") => null, // Graduate non-trained
        var guid when guid == Guid.Parse("34222549-ED59-4C4A-811D-C0894E78D4C3") => dfeta_ITTProgrammeType.GraduateTeacherProgramme,
        var guid when guid == Guid.Parse("10078157-E8C3-42F7-A050-D8B802E83F7B") => dfeta_ITTProgrammeType.HEI,
        var guid when guid == Guid.Parse("32017D68-9DA4-43B2-AE91-4F24C68F6F78") => null, // HEI - Historic
        var guid when guid == Guid.Parse("BFEF20B2-5AC4-486D-9493-E5A4538E1BE9") => dfeta_ITTProgrammeType.HighpotentialITT,
        var guid when guid == Guid.Parse("D0B60864-AB1C-4D49-A5C2-FF4BD9872EE1") => dfeta_ITTProgrammeType.Internationalqualifiedteacherstatus,
        var guid when guid == Guid.Parse("4514EC65-20B0-4465-B66F-4718963C5B80") => null, // Legacy ITT
        var guid when guid == Guid.Parse("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") => null, // Legacy Migration
        var guid when guid == Guid.Parse("2B4862CA-BD30-4A3A-BFCE-52B57C2946C7") => dfeta_ITTProgrammeType.LicensedTeacherProgramme,
        var guid when guid == Guid.Parse("FC16290C-AC1E-4830-B7E9-35708F1BDED3") => null, // Licensed Teacher Programme - Armed Forces
        var guid when guid == Guid.Parse("D5EB09CC-C64F-45DF-A46D-08277A25DE7A") => null, // Licensed Teacher Programme - FE
        var guid when guid == Guid.Parse("64C28594-4B63-42B3-8B47-E3F140879E66") => null, // Licensed Teacher Programme - Independent School
        var guid when guid == Guid.Parse("E5C198FA-35F0-4A13-9D07-8B0239B4957A") => null, // Licensed Teacher Programme - Maintained School
        var guid when guid == Guid.Parse("779BD3C6-6B3A-4204-9489-1BBB381B52BF") => null, // Licensed Teacher Programme - OTT
        var guid when guid == Guid.Parse("AA1EFD16-D59C-4E18-A496-16E39609B389") => null, // Long Service
        var guid when guid == Guid.Parse("3604EF30-8F11-4494-8B52-A2F9C5371E03") => null, // NI R
        var guid when guid == Guid.Parse("88867B43-897B-49B5-97CC-F4F81A1D5D44") => null, // Other Qualifications non ITT
        var guid when guid == Guid.Parse("51756384-CFEA-4F63-80E5-F193686E0F71") => dfeta_ITTProgrammeType.OverseasTrainedTeacherProgramme, // Overseas Trained Teacher Programme
        var guid when guid == Guid.Parse("CE61056E-E681-471E-AF48-5FFBF2653500") => null, // Overseas Trained Teacher Recognition
        var guid when guid == Guid.Parse("F5390BE5-8336-4951-B97B-5B45D00B7A76") => null, // PGATC ITT
        var guid when guid == Guid.Parse("1C626BE0-5A64-47EC-8349-75008F52BC2C") => null, // PGATD ITT
        var guid when guid == Guid.Parse("02A2135C-AC34-4481-A293-8A00AAB7EE69") => null, // PGCE ITT
        var guid when guid == Guid.Parse("7721655F-165F-4737-97D4-17FC6991C18C") => null, // PGDE ITT
        var guid when guid == Guid.Parse("EF46FF51-8DC0-481E-B158-61CCEA9943D9") => dfeta_ITTProgrammeType.Primaryandsecondarypostgraduatefeefunded,
        var guid when guid == Guid.Parse("321D5F9A-9581-4936-9F63-CFDDD2A95FE2") => dfeta_ITTProgrammeType.Primaryandsecondaryundergraduatefeefunded,
        var guid when guid == Guid.Parse("002F7C96-F6AE-4E67-8F8B-D2F1C1317273") => null, // ProfGCE ITT
        var guid when guid == Guid.Parse("9A6F368F-06E7-4A74-B269-6886C48A49DA") => null, // ProfGDE ITT
        var guid when guid == Guid.Parse("97497716-5AC5-49AA-A444-27FA3E2C152A") => dfeta_ITTProgrammeType.Providerled_postgrad,
        var guid when guid == Guid.Parse("53A7FBDA-25FD-4482-9881-5CF65053888D") => dfeta_ITTProgrammeType.Providerled_undergrad,
        var guid when guid == Guid.Parse("BE6EAF8C-92DD-4EFF-AAD3-1C89C4BEC18C") => null, // QTLS and SET Membership
        var guid when guid == Guid.Parse("70368FF2-8D2B-467E-AD23-EFE7F79995D7") => dfeta_ITTProgrammeType.RegisteredTeacherProgramme,
        var guid when guid == Guid.Parse("ABCB0A14-0C21-4598-A42C-A007D4B048AC") => null, // School Centered ITT
        var guid when guid == Guid.Parse("D9490E58-ACDC-4A38-B13E-5A5C21417737") => dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme,
        var guid when guid == Guid.Parse("12A742C3-1CD4-43B7-A2FA-1000BD4CC373") => dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Salaried,
        var guid when guid == Guid.Parse("97E1811B-D46C-483E-AEC3-4A2DD51A55FE") => dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Selffunded,
        var guid when guid == Guid.Parse("52835B1F-1F2E-4665-ABC6-7FB1EF0A80BB") => null, // Scotland R
        var guid when guid == Guid.Parse("BED14B00-5D08-4580-83B5-86D71A4F1A24") => null, // TC ITT
        var guid when guid == Guid.Parse("82AA14D3-EF6A-4B46-A10C-DC850DDCEF5F") => null, // TCMH        
        var guid when guid == Guid.Parse("5B7F5E90-1CA6-4529-BAA0-DFBA68E698B8") => dfeta_ITTProgrammeType.TeachFirstProgramme,
        // TODO Seem to be missing dfeta_ITTProgrammeType.TeachFirstProgramme_CC ?
        var guid when guid == Guid.Parse("50D18F17-EE26-4DAD-86CA-1AAE3F956BFC") => null, // Troops to Teach
        var guid when guid == Guid.Parse("7C04865F-FA39-458A-BC39-07DD46B88154") => null, // UGMT ITT
        var guid when guid == Guid.Parse("20F67E38-F117-4B42-BBFC-5812AA717B94") => dfeta_ITTProgrammeType.UndergraduateOptIn,
        var guid when guid == Guid.Parse("877BA701-FE26-4951-9F15-171F3755D50D") => null, // Welsh R
        _ => throw new ArgumentException($"{routeTypeId} cannot be converted to {nameof(dfeta_ITTProgrammeType)}.", nameof(routeTypeId))
    };
}
