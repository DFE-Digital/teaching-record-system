using System.Diagnostics;
using System.Reactive.Subjects;
using System.ServiceModel;
using AngleSharp.Common;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using MoreLinq.Extensions;
using Npgsql;
using NpgsqlTypes;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.Files;
using DqtSystemUser = TeachingRecordSystem.Core.Dqt.Models.SystemUser;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.Core.Services.TrsDataSync;

#pragma warning disable CS9113 // Parameter is unread.
public class TrsDataSyncHelper(
    NpgsqlDataSource trsDbDataSource,
    [FromKeyedServices(TrsDataSyncService.CrmClientName)] IOrganizationServiceAsync2 organizationService,
    ReferenceDataCache referenceDataCache,
    IClock clock,
    IAuditRepository auditRepository,
    ILogger<TrsDataSyncHelper> logger,
    IFileService fileService,
    IConfiguration configuration)
#pragma warning restore CS9113 // Parameter is unread.
{
    private delegate Task SyncEntitiesHandler(IReadOnlyCollection<Entity> entities, bool ignoreInvalid, bool dryRun, CancellationToken cancellationToken);

    private const string NowParameterName = "@now";
    private const string IdsParameterName = "@ids";
    private const int MaxAuditRequestsPerBatch = 10;

    private IReadOnlyDictionary<string, ModelTypeSyncInfo> GetModelTypeSyncInfo() =>
        new Dictionary<string, ModelTypeSyncInfo>()
        {
            { ModelTypes.Person, GetModelTypeSyncInfoForPerson() },
            { ModelTypes.Event, GetModelTypeSyncInfoForEvent() },
            { ModelTypes.Induction, GetModelTypeSyncInfoForInduction() },
            { ModelTypes.DqtNote, GetModelTypeSyncInfoForNotes() },
            { ModelTypes.Route, GetModelTypeSyncInfoForRoute() },
            { ModelTypes.PreviousName, GetModelTypeSyncInfoForPreviousName() }
        };

    private IReadOnlyDictionary<string, ModelTypeSyncInfo> AllModelTypeSyncInfo => GetModelTypeSyncInfo();

    private static IReadOnlyDictionary<dfeta_ITTProgrammeType, Guid> _programmeTypeRouteMapping = new Dictionary<dfeta_ITTProgrammeType, Guid>()
    {
        { dfeta_ITTProgrammeType.Apprenticeship, new("6987240E-966E-485F-B300-23B54937FB3A") },
        { dfeta_ITTProgrammeType.AssessmentOnlyRoute, new("57B86CEF-98E2-4962-A74A-D47C7A34B838") },
        { dfeta_ITTProgrammeType.Core, new("4163C2FB-6163-409F-85FD-56E7C70A54DD") },
        { dfeta_ITTProgrammeType.CoreFlexible, new("4BD7A9F0-28CA-4977-A044-A7B7828D469B") },
        { dfeta_ITTProgrammeType.EYITTAssessmentOnly, new("D9EEF3F8-FDE6-4A3F-A361-F6655A42FA1E") },
        { dfeta_ITTProgrammeType.EYITTGraduateEmploymentBased, new("4477E45D-C531-4C63-9F4B-E157766366FB") },
        { dfeta_ITTProgrammeType.EYITTGraduateEntry, new("DBC4125B-9235-41E4-ABD2-BAABBF63F829") },
        { dfeta_ITTProgrammeType.EYITTSchoolDirect_EarlyYears, new("7F09002C-5DAD-4839-9693-5E030D037AE9") },
        { dfeta_ITTProgrammeType.EYITTUndergraduate, new("C97C0FD2-FD84-4949-97C7-B0E2422FB3C8") },
        { dfeta_ITTProgrammeType.FutureTeachingScholars, new("F85962C9-CF0C-415D-9DE5-A397F95AE261") },
        { dfeta_ITTProgrammeType.HEI, new("10078157-E8C3-42F7-A050-D8B802E83F7B") },
        { dfeta_ITTProgrammeType.HighpotentialITT, new("BFEF20B2-5AC4-486D-9493-E5A4538E1BE9") },
        { dfeta_ITTProgrammeType.Internationalqualifiedteacherstatus, new("D0B60864-AB1C-4D49-A5C2-FF4BD9872EE1") },
        { dfeta_ITTProgrammeType.LicensedTeacherProgramme, new("2B4862CA-BD30-4A3A-BFCE-52B57C2946C7") },
        { dfeta_ITTProgrammeType.OverseasTrainedTeacherProgramme, new("51756384-CFEA-4F63-80E5-F193686E0F71") },
        { dfeta_ITTProgrammeType.Primaryandsecondarypostgraduatefeefunded, new("EF46FF51-8DC0-481E-B158-61CCEA9943D9") },
        { dfeta_ITTProgrammeType.Primaryandsecondaryundergraduatefeefunded, new("321D5F9A-9581-4936-9F63-CFDDD2A95FE2") },
        { dfeta_ITTProgrammeType.Providerled_postgrad, new("97497716-5AC5-49AA-A444-27FA3E2C152A") },
        { dfeta_ITTProgrammeType.Providerled_undergrad, new("53A7FBDA-25FD-4482-9881-5CF65053888D") },
        { dfeta_ITTProgrammeType.RegisteredTeacherProgramme, new("70368FF2-8D2B-467E-AD23-EFE7F79995D7") },
        { dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme, new("D9490E58-ACDC-4A38-B13E-5A5C21417737") },
        { dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Salaried, new("12A742C3-1CD4-43B7-A2FA-1000BD4CC373") },
        { dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Selffunded, new("97E1811B-D46C-483E-AEC3-4A2DD51A55FE") },
        { dfeta_ITTProgrammeType.TeachFirstProgramme, new("5B7F5E90-1CA6-4529-BAA0-DFBA68E698B8") },
        { dfeta_ITTProgrammeType.TeachFirstProgramme_CC, new("5B7F5E90-1CA6-4529-BAA0-DFBA68E698B8") },
        { dfeta_ITTProgrammeType.UndergraduateOptIn, new("20f67e38-f117-4b42-bbfc-5812aa717b94") },
    };

    private static IReadOnlyDictionary<string, Guid?> _statusRouteIdMapping = new Dictionary<string, Guid?>()
    {
        { "71", null },
        { "49", new("34222549-ED59-4C4A-811D-C0894E78D4C3") },
        { "67", new("F4DA123B-5C37-4060-AB00-52DE4BD3599E") },
        { "72", new("88867B43-897B-49B5-97CC-F4F81A1D5D44") },
        { "213", new("877ba701-fe26-4951-9f15-171f3755d50d") },
        { "211", null },
        { "77", new("ABCB0A14-0C21-4598-A42C-A007D4B048AC") },
        { "79", new("88867B43-897B-49B5-97CC-F4F81A1D5D44") },
        { "103", new("CE61056E-E681-471E-AF48-5FFBF2653500") },
        { "100", new("57B86CEF-98E2-4962-A74A-D47C7A34B838") },
        { "222", new("8F5C0431-D006-4EDA-9336-16DFC6A26A78") },
        { "80", new("700EC96F-6BBF-4080-87BD-94EF65A6A879") },
        { "68", new("52835B1F-1F2E-4665-ABC6-7FB1EF0A80BB") },
        { "52", new("51756384-CFEA-4F63-80E5-F193686E0F71") },
        { "221", null },
        { "104", new("6F27BDEB-D00A-4EF9-B0EA-26498CE64713") },
        { "45", new("5B7F5E90-1CA6-4529-BAA0-DFBA68E698B8") },
        { "51", new("51756384-CFEA-4F63-80E5-F193686E0F71") },
        { "69", new("3604EF30-8F11-4494-8B52-A2F9C5371E03") },
        { "47", new("70368FF2-8D2B-467E-AD23-EFE7F79995D7") },
        { "223", new("2B106B9D-BA39-4E2D-A42E-0CE827FDC324") },
        { "62", new("2B4862CA-BD30-4A3A-BFCE-52B57C2946C7") },
        { "76", new("4B6FC697-BE67-43D3-9021-CC662C4A559F") },
        { "63", new("779BD3C6-6B3A-4204-9489-1BBB381B52BF") },
        { "220", null },
        { "90", new("D0B60864-AB1C-4D49-A5C2-FF4BD9872EE1") },
        { "74", new("E5C198FA-35F0-4A13-9D07-8B0239B4957A") },
        { "65", new("D5EB09CC-C64F-45DF-A46D-08277A25DE7A") },
        { "214", new("EC95C276-25D9-491F-99A2-8D92F10E1E94") },
        { "212", new("57B86CEF-98E2-4962-A74A-D47C7A34B838") },
        { "64", new("64C28594-4B63-42B3-8B47-E3F140879E66") },
        { "101", new("5B7F5E90-1CA6-4529-BAA0-DFBA68E698B8") },
        { "53", new("45C93B5B-B4DC-4D0F-B0DE-D612521E0A13") },
        { "89", new("88867B43-897B-49B5-97CC-F4F81A1D5D44") },
        { "82", new("88867B43-897B-49B5-97CC-F4F81A1D5D44") },
        { "92", new("88867B43-897B-49B5-97CC-F4F81A1D5D44") },
        { "73", new("AA1EFD16-D59C-4E18-A496-16E39609B389") },
        { "66", new("FC16290C-AC1E-4830-B7E9-35708F1BDED3") },
        { "99", new("88867B43-897B-49B5-97CC-F4F81A1D5D44") },
        { "58", new("82AA14D3-EF6A-4B46-A10C-DC850DDCEF5F") },
        { "87", new("F4DA123B-5C37-4060-AB00-52DE4BD3599E") },
        { "78", new("5748D41D-7B53-4EE6-833A-83080A3BD8EF") },
        { "48", new("34222549-ED59-4C4A-811D-C0894E78D4C3") },
        { "97", new("F4DA123B-5C37-4060-AB00-52DE4BD3599E") },
        { "28", new("52835B1F-1F2E-4665-ABC6-7FB1EF0A80BB") },
        { "83", new("AA1EFD16-D59C-4E18-A496-16E39609B389") },
        { "24", new("64C28594-4B63-42B3-8B47-E3F140879E66") },
        { "102", new("50D18F17-EE26-4DAD-86CA-1AAE3F956BFC") },
    };

    private static IReadOnlyDictionary<string, Guid?> _ittQualificationRouteIdMapping = new Dictionary<string, Guid?>()
    {
        { "051", new("57B86CEF-98E2-4962-A74A-D47C7A34B838") },
        { "007", new("32017D68-9DA4-43B2-AE91-4F24C68F6F78") },
        { "008", new("32017D68-9DA4-43B2-AE91-4F24C68F6F78") },
        { "010", new("32017D68-9DA4-43B2-AE91-4F24C68F6F78") },
        { "014", new("32017D68-9DA4-43B2-AE91-4F24C68F6F78") },
        { "009", new("32017D68-9DA4-43B2-AE91-4F24C68F6F78") },
        { "018", new("32017D68-9DA4-43B2-AE91-4F24C68F6F78") },
        { "011", new("32017D68-9DA4-43B2-AE91-4F24C68F6F78") },
        { "001", new("32017D68-9DA4-43B2-AE91-4F24C68F6F78") },
        { "002", new("32017D68-9DA4-43B2-AE91-4F24C68F6F78") },
        { "016", new("32017D68-9DA4-43B2-AE91-4F24C68F6F78") },
        { "003", new("32017D68-9DA4-43B2-AE91-4F24C68F6F78") },
        { "004", new("32017D68-9DA4-43B2-AE91-4F24C68F6F78") },
        { "013", new("32017D68-9DA4-43B2-AE91-4F24C68F6F78") },
        { "017", new("32017D68-9DA4-43B2-AE91-4F24C68F6F78") },
        { "015", new("32017D68-9DA4-43B2-AE91-4F24C68F6F78") },
        { "006", new("32017D68-9DA4-43B2-AE91-4F24C68F6F78") },
        { "005", new("32017D68-9DA4-43B2-AE91-4F24C68F6F78") },
        { "030", new("4514EC65-20B0-4465-B66F-4718963C5B80") },
        { "040", null },
        { "400", new("10078157-E8C3-42F7-A050-D8B802E83F7B") },
        { "402", null },
        { "105", null },
        { "116", null },
        { "114", null },
        { "115", null },
        { "111", new("700EC96F-6BBF-4080-87BD-94EF65A6A879") },
        { "110", new("700EC96F-6BBF-4080-87BD-94EF65A6A879") },
        { "113", new("700EC96F-6BBF-4080-87BD-94EF65A6A879") },
        { "033", new("4514EC65-20B0-4465-B66F-4718963C5B80") },
        { "025", new("4514EC65-20B0-4465-B66F-4718963C5B80") },
        { "024", new("4514EC65-20B0-4465-B66F-4718963C5B80") },
        { "997", null }, // there is only one of these in prod and it is an inactive ITT qual
        { "100", new("34222549-ED59-4C4A-811D-C0894E78D4C3") },
        { "401", null },
        { "055", new("D0B60864-AB1C-4D49-A5C2-FF4BD9872EE1") },
        { "112", new("2B4862CA-BD30-4A3A-BFCE-52B57C2946C7") },
        { "032", null },
        { "998", null },
        { "107", new("3604EF30-8F11-4494-8B52-A2F9C5371E03") },
        { "102", null },
        { "103", null },
        { "054", new("CE61056E-E681-471E-AF48-5FFBF2653500") },
        { "026", new("4514EC65-20B0-4465-B66F-4718963C5B80") },
        { "022", new("F5390BE5-8336-4951-B97B-5B45D00B7A76") },
        { "023", new("1C626BE0-5A64-47EC-8349-75008F52BC2C") },
        { "020", new("02A2135C-AC34-4481-A293-8A00AAB7EE69") },
        { "019", new("700EC96F-6BBF-4080-87BD-94EF65A6A879") },
        { "041", null },
        { "021", new("7721655F-165F-4737-97D4-17FC6991C18C") },
        { "031", new("002F7C96-F6AE-4E67-8F8B-D2F1C1317273") },
        { "050", new("9A6F368F-06E7-4A74-B269-6886C48A49DA") },
        { "029", null },
        { "027", null },
        { "049", null },
        { "101", new("70368FF2-8D2B-467E-AD23-EFE7F79995D7") },
        { "106", null },
        { "104", null },
        { "052", null },
        { "043", new("BED14B00-5D08-4580-83B5-86D71A4F1A24") },
        { "042", null },
        { "028", new("7C04865F-FA39-458A-BC39-07DD46B88154") },
        { "999", null }
    };

    private static IReadOnlyDictionary<(string? TeacherStatus, dfeta_ITTProgrammeType? IttProgrammeType, string? IttQualificationValue), Guid?> _hardcodedRouteIdMapping
        = new Dictionary<(string? TeacherStatusValue, dfeta_ITTProgrammeType? IttProgrammeType, string? IttQualificationValue), Guid?>()
    {
        { ("71", null, "029"), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // Legacy Migration
        { ("71", null, "999"), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // Legacy Migration
        { ("71", null, null), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // Legacy Migration
        { ("71", null, "042"), new("88867B43-897B-49B5-97CC-F4F81A1D5D44") }, // Other Qualifications non ITT
        { ("71", null, "040"), new("88867B43-897B-49B5-97CC-F4F81A1D5D44") }, // Other Qualifications non ITT
        { ("71", null, "049"), new("88867B43-897B-49B5-97CC-F4F81A1D5D44") }, // Other Qualifications non ITT
        { ("71", null, "041"), new("88867B43-897B-49B5-97CC-F4F81A1D5D44") }, // Other Qualifications non ITT
        { ("85", null, "999"), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // Legacy Migration
        { ("91", null, "999"), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // Legacy Migration
        { ("81", null, "999"), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // Legacy Migration
        { ("91", null, null), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // Legacy Migration
        { ("81", null, null), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // Legacy Migration
        { ("85", null, "042"), new("88867B43-897B-49B5-97CC-F4F81A1D5D44") }, // Other Qualifications non ITT
        { ("91", null, "042"), new("88867B43-897B-49B5-97CC-F4F81A1D5D44") }, // Other Qualifications non ITT
        { ("81", null, "042"), new("88867B43-897B-49B5-97CC-F4F81A1D5D44") }, // Other Qualifications non
        { ("221", null, null), new("5B7D1C4E-FB2B-479C-BDEE-5818DAAA8A07") }, // EYTS ITT Migrated
    };

    private static IReadOnlyDictionary<(string? TeacherStatus, dfeta_ITTProgrammeType? IttProgrammeType, string? IttQualificationValue, dfeta_ITTResult? IttResult), Guid?> _hardcodedIncludingResultRouteIdMapping
        = new Dictionary<(string? TeacherStatusValue, dfeta_ITTProgrammeType? IttProgrammeType, string? IttQualificationValue, dfeta_ITTResult? IttResult), Guid?>()
    {
        { ("221", null, "114", dfeta_ITTResult.Pass), new("5B7D1C4E-FB2B-479C-BDEE-5818DAAA8A07") }, // Early Years Teacher Status | EYTS Only | Pass -> EYTS ITT Migrated
        { ("211", null, "029", dfeta_ITTResult.Deferred), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // Trainee Teacher | QTS Assessment only | Deferred -> Legacy Migration
        { ("211", null, "029", dfeta_ITTResult.InTraining), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // Trainee Teacher | QTS Assessment only | InTraining -> Legacy Migration
        { ("211", null, "027", dfeta_ITTResult.Deferred), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // Trainee Teacher | QTS Award | Deferred -> Legacy Migration
        { (null, null, "040", dfeta_ITTResult.Fail), new("88867B43-897B-49B5-97CC-F4F81A1D5D44") }, // NULL | Certificate in Education (Further Education) | Fail -> Other Qualifications non ITT
        { (null, null, "040", dfeta_ITTResult.Withdrawn), new("88867B43-897B-49B5-97CC-F4F81A1D5D44") }, // NULL | Certificate in Education (Further Education) | Withdrawn -> Other Qualifications non ITT
        { (null, null, "102", dfeta_ITTResult.DeferredforSkillsTests), new("51756384-CFEA-4F63-80E5-F193686E0F71") }, // NULL | OTT | DeferredforSkillsTests -> Overseas Trained Teacher Programme
        { (null, null, "102", dfeta_ITTResult.Fail), new("51756384-CFEA-4F63-80E5-F193686E0F71") }, // NULL | OTT | Fail -> Overseas Trained Teacher Programme
        { (null, null, "102", dfeta_ITTResult.Withdrawn), new("51756384-CFEA-4F63-80E5-F193686E0F71") }, // NULL | OTT | Withdrawn -> Overseas Trained Teacher Programme
        { (null, null, "029", dfeta_ITTResult.Deferred), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // NULL | QTS Assessment only | Deferred -> Legacy Migration
        { (null, null, "029", dfeta_ITTResult.DeferredforSkillsTests), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // NULL | QTS Assessment only | DeferredforSkillsTests -> Legacy Migration
        { (null, null, "029", dfeta_ITTResult.Fail), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // NULL | QTS Assessment only | Fail -> Legacy Migration
        { (null, null, "029", dfeta_ITTResult.Withdrawn), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // NULL | QTS Assessment only | Withdrawn -> Legacy Migration
        { (null, null, "027", dfeta_ITTResult.Fail), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // NULL | QTS Award | Fail -> Legacy Migration
        { (null, null, "027", dfeta_ITTResult.Withdrawn), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // NULL | QTS Award | Withdrawn -> Legacy Migration
        { (null, null, "999", dfeta_ITTResult.Deferred), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // NULL | Unknown | Deferred -> Legacy Migration
        { (null, null, "999", dfeta_ITTResult.DeferredforSkillsTests), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // NULL | Unknown | DeferredforSkillsTests -> Legacy Migration
        { (null, null, "999", dfeta_ITTResult.Fail), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // NULL | Unknown | Fail -> Legacy Migration
        { (null, null, "999", dfeta_ITTResult.Withdrawn), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // NULL | Unknown | Withdrawn -> Legacy Migration
        { (null, null, null, dfeta_ITTResult.Withdrawn), new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8") }, // NULL | Unknown | Withdrawn -> Legacy Migration
    };

    private static IReadOnlyDictionary<(string? TeacherStatus, dfeta_ITTProgrammeType? IttProgrammeType, string? IttQualificationValue), RouteMappingPrecedence> _manualRouteMappingPrecedence
        = new Dictionary<(string? TeacherStatusValue, dfeta_ITTProgrammeType? IttProgrammeType, string? IttQualificationValue), RouteMappingPrecedence>()
    {
       { ("77", null, "020"), RouteMappingPrecedence.TeachingStatus },
       { ("49", dfeta_ITTProgrammeType.AssessmentOnlyRoute, "100"), RouteMappingPrecedence.TeachingStatus },
       { ("77", null, "031"), RouteMappingPrecedence.TeachingStatus },
       { ("80", null, "020"), RouteMappingPrecedence.TeachingStatus },
       { ("80", dfeta_ITTProgrammeType.HEI, "110"), RouteMappingPrecedence.TeachingStatus },
       { ("80", dfeta_ITTProgrammeType.HEI, "113"), RouteMappingPrecedence.TeachingStatus },
       { ("72", null, "020"), RouteMappingPrecedence.IttQualification },
       { ("80", null, "031"), RouteMappingPrecedence.TeachingStatus },
       { ("80", dfeta_ITTProgrammeType.HEI, "020"), RouteMappingPrecedence.TeachingStatus },
       { ("80", dfeta_ITTProgrammeType.Core, "113"), RouteMappingPrecedence.TeachingStatus },
       { ("80", dfeta_ITTProgrammeType.HEI, "031"), RouteMappingPrecedence.TeachingStatus },
       { ("67", null, "030"), RouteMappingPrecedence.TeachingStatus },
       { ("79", null, "020"), RouteMappingPrecedence.TeachingStatus },
       { ("79", null, "030"), RouteMappingPrecedence.TeachingStatus },
       { ("77", null, "113"), RouteMappingPrecedence.TeachingStatus },
       { ("67", null, "020"), RouteMappingPrecedence.TeachingStatus },
       { ("80", dfeta_ITTProgrammeType.HEI, "111"), RouteMappingPrecedence.TeachingStatus },
       { ("67", null, "400"), RouteMappingPrecedence.TeachingStatus },
       { ("67", null, "001"), RouteMappingPrecedence.TeachingStatus },
       { ("77", null, "030"), RouteMappingPrecedence.TeachingStatus },
       { ("63", dfeta_ITTProgrammeType.LicensedTeacherProgramme, "112"), RouteMappingPrecedence.TeachingStatus },
       { ("49", null, "020"), RouteMappingPrecedence.TeachingStatus },
       { ("67", null, "002"), RouteMappingPrecedence.TeachingStatus },
       { ("80", dfeta_ITTProgrammeType.Core, "110"), RouteMappingPrecedence.TeachingStatus },
       { ("80", dfeta_ITTProgrammeType.Core, "111"), RouteMappingPrecedence.TeachingStatus },
       { ("67", null, "07"), RouteMappingPrecedence.TeachingStatus },
       { ("72", null, "030"), RouteMappingPrecedence.IttQualification },
       { ("80", null, "008"), RouteMappingPrecedence.TeachingStatus },
       { ("77", null, "002"), RouteMappingPrecedence.TeachingStatus },
       { ("62", null, "020"), RouteMappingPrecedence.TeachingStatus },
       { ("53", null, "030"), RouteMappingPrecedence.TeachingStatus },
       { ("223", null, "054"), RouteMappingPrecedence.TeachingStatus },
       { ("67", null, "010"), RouteMappingPrecedence.TeachingStatus },
       { ("67", null, "004"), RouteMappingPrecedence.TeachingStatus },
       { ("74", null, "020"), RouteMappingPrecedence.TeachingStatus },
       { ("77", null, "008"), RouteMappingPrecedence.TeachingStatus },
       { ("63", dfeta_ITTProgrammeType.OverseasTrainedTeacherProgramme, "102"), RouteMappingPrecedence.TeachingStatus },
       { ("53", null, "020"), RouteMappingPrecedence.TeachingStatus },
       { ("80", dfeta_ITTProgrammeType.GraduateTeacherProgramme, "100"), RouteMappingPrecedence.TeachingStatus },
       { ("74", dfeta_ITTProgrammeType.LicensedTeacherProgramme, "112"), RouteMappingPrecedence.TeachingStatus },
       { ("52", dfeta_ITTProgrammeType.GraduateTeacherProgramme, "100"), RouteMappingPrecedence.TeachingStatus },
       { ("77", dfeta_ITTProgrammeType.HEI, "020"), RouteMappingPrecedence.TeachingStatus },
       { ("45", dfeta_ITTProgrammeType.GraduateTeacherProgramme, "100"), RouteMappingPrecedence.TeachingStatus },
       { ("67", dfeta_ITTProgrammeType.HEI, "105"), RouteMappingPrecedence.TeachingStatus },
       { ("80", null, "022"), RouteMappingPrecedence.TeachingStatus },
       { ("51", dfeta_ITTProgrammeType.GraduateTeacherProgramme, "100"), RouteMappingPrecedence.TeachingStatus },
       { ("67", null, "008"), RouteMappingPrecedence.TeachingStatus },
       { ("67", null, "003"), RouteMappingPrecedence.TeachingStatus },
       { ("67", null, "054"), RouteMappingPrecedence.TeachingStatus },
       { ("103", dfeta_ITTProgrammeType.OverseasTrainedTeacherProgramme, null), RouteMappingPrecedence.TeachingStatus },
       { ("80", null, "030"), RouteMappingPrecedence.TeachingStatus },
       { ("80", null, "002"), RouteMappingPrecedence.TeachingStatus },
       { ("77", dfeta_ITTProgrammeType.HEI, "008"), RouteMappingPrecedence.TeachingStatus },
       { ("58", null, "020"), RouteMappingPrecedence.TeachingStatus },
       { ("65", null, "030"), RouteMappingPrecedence.TeachingStatus },
       { ("65", null, "020"), RouteMappingPrecedence.TeachingStatus },
       { ("74", null, "002"), RouteMappingPrecedence.TeachingStatus },
       { ("77", null, "026"), RouteMappingPrecedence.TeachingStatus },
       { ("77", null, "001"), RouteMappingPrecedence.TeachingStatus },
       { ("45", dfeta_ITTProgrammeType.AssessmentOnlyRoute, "100"), RouteMappingPrecedence.TeachingStatus },
       { ("101", null, "054"), RouteMappingPrecedence.TeachingStatus },
       { ("72", null, "110"), RouteMappingPrecedence.IttQualification },
       { ("76", null, "020"), RouteMappingPrecedence.TeachingStatus },
       { ("76", null, "008"), RouteMappingPrecedence.TeachingStatus },
       { ("76", dfeta_ITTProgrammeType.LicensedTeacherProgramme, "112"), RouteMappingPrecedence.ProgrammeType },
       { ("78", null, "020"), RouteMappingPrecedence.TeachingStatus },
       { ("67", dfeta_ITTProgrammeType.TeachFirstProgramme, "105"), RouteMappingPrecedence.TeachingStatus },
       { ("223", dfeta_ITTProgrammeType.Providerled_postgrad, "020"), RouteMappingPrecedence.TeachingStatus },
       { ("53", null, "043"), RouteMappingPrecedence.TeachingStatus },
       { ("80", null, "023"), RouteMappingPrecedence.TeachingStatus },
       { ("80", dfeta_ITTProgrammeType.AssessmentOnlyRoute, "100"), RouteMappingPrecedence.TeachingStatus },
       { ("80", dfeta_ITTProgrammeType.HEI, "008"), RouteMappingPrecedence.TeachingStatus },
       { ("49", null, "008"), RouteMappingPrecedence.TeachingStatus },
       { ("49", dfeta_ITTProgrammeType.Core, "0029"), RouteMappingPrecedence.TeachingStatus },
       { ("49", dfeta_ITTProgrammeType.TeachFirstProgramme, "104"), RouteMappingPrecedence.ProgrammeType },
       { ("62", null, "030"), RouteMappingPrecedence.TeachingStatus },
       { ("62", null, "043"), RouteMappingPrecedence.TeachingStatus },
       { ("62", null, "008"), RouteMappingPrecedence.TeachingStatus },
       { ("24", dfeta_ITTProgrammeType.LicensedTeacherProgramme, "112"), RouteMappingPrecedence.TeachingStatus },
       { ("51", null, "002"), RouteMappingPrecedence.TeachingStatus },
       { ("103", dfeta_ITTProgrammeType.HEI, "054"), RouteMappingPrecedence.TeachingStatus },
       { ("103", dfeta_ITTProgrammeType.OverseasTrainedTeacherProgramme, "103"), RouteMappingPrecedence.TeachingStatus },
       { ("47", null, "008"), RouteMappingPrecedence.TeachingStatus },
       { ("77", dfeta_ITTProgrammeType.HEI, "007"), RouteMappingPrecedence.TeachingStatus },
       { ("77", null, "022"), RouteMappingPrecedence.TeachingStatus },
       { ("102", null, "020"), RouteMappingPrecedence.TeachingStatus },
       { ("67", null, "025"), RouteMappingPrecedence.TeachingStatus },
       { ("67", null, "015"), RouteMappingPrecedence.TeachingStatus },
       { ("53", null, "024"), RouteMappingPrecedence.TeachingStatus },
       { ("80", null, "400"), RouteMappingPrecedence.TeachingStatus },
       { ("80", null, "004"), RouteMappingPrecedence.TeachingStatus },
       { ("49", null, "002"), RouteMappingPrecedence.TeachingStatus },
       { ("74", null, "001"), RouteMappingPrecedence.TeachingStatus },
       { ("63", null, "112"), RouteMappingPrecedence.TeachingStatus },
       { ("72", null, "400"), RouteMappingPrecedence.TeachingStatus },
       { ("72", null, "026"), RouteMappingPrecedence.IttQualification },
       { ("104", null, "054"), RouteMappingPrecedence.IttQualification },
       { ("49", null, "054"), RouteMappingPrecedence.TeachingStatus },
       { ("49", null, "030"), RouteMappingPrecedence.TeachingStatus },
       { ("49", null, "400"), RouteMappingPrecedence.TeachingStatus },
       { ("71", dfeta_ITTProgrammeType.HEI, "020"), RouteMappingPrecedence.ProgrammeType },
       { ("71", dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme, "020"), RouteMappingPrecedence.ProgrammeType },
       { ("71", dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Salaried, "020"), RouteMappingPrecedence.ProgrammeType },
       { ("71", dfeta_ITTProgrammeType.Providerled_postgrad, "020"), RouteMappingPrecedence.ProgrammeType },
       { ("71", dfeta_ITTProgrammeType.Apprenticeship, "020"), RouteMappingPrecedence.ProgrammeType },
       { ("71", dfeta_ITTProgrammeType.Apprenticeship, "031"), RouteMappingPrecedence.ProgrammeType },
       { ("211", dfeta_ITTProgrammeType.HEI, "020"), RouteMappingPrecedence.ProgrammeType },
       { ("71", dfeta_ITTProgrammeType.Core, "020"), RouteMappingPrecedence.ProgrammeType },
       { ("71", dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme, "031"), RouteMappingPrecedence.ProgrammeType },
       { ("71", dfeta_ITTProgrammeType.HEI, "031"), RouteMappingPrecedence.ProgrammeType },
       { ("211", dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme, "020"), RouteMappingPrecedence.ProgrammeType },
       { ("211", dfeta_ITTProgrammeType.Providerled_postgrad, "020"), RouteMappingPrecedence.ProgrammeType },
       { ("211", dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Salaried, "020"), RouteMappingPrecedence.ProgrammeType },
       { ("211", dfeta_ITTProgrammeType.Primaryandsecondarypostgraduatefeefunded, "020"), RouteMappingPrecedence.ProgrammeType },
       { ("211", dfeta_ITTProgrammeType.Core, "020"), RouteMappingPrecedence.ProgrammeType },
       { ("211", dfeta_ITTProgrammeType.Apprenticeship, "020"), RouteMappingPrecedence.ProgrammeType },
       { ("71", dfeta_ITTProgrammeType.HEI, "008"), RouteMappingPrecedence.ProgrammeType },
       { ("71", dfeta_ITTProgrammeType.HEI, "004"), RouteMappingPrecedence.ProgrammeType },
       { ("71", dfeta_ITTProgrammeType.HEI, "013"), RouteMappingPrecedence.ProgrammeType },
       { ("71", dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Salaried, "031"), RouteMappingPrecedence.ProgrammeType },
       { ("71", dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Selffunded, "031"), RouteMappingPrecedence.ProgrammeType }
    };

    private static IReadOnlyDictionary<Guid, (Guid QtsIttId, Guid EyIttId)> HardcodedQtsEyMappings = new Dictionary<Guid, (Guid QtsIttId, Guid EyIttId)>
    {
        { new("2cbbe3f2-05af-e311-b8ed-005056822391"), (new("3f5b2a42-35b0-e311-b8ed-005056822391"), new("69d3722c-bb91-e911-a958-000d3a2aa275")) },
        { new("C13E84ED-CAAE-E311-B8ED-005056822391"), (new("04ECD95D-DEAF-E311-B8ED-005056822391"), new("03C8B2AE-3789-E911-A958-000D3A2AA275")) },
        { new("DE813D0C-DAAE-E311-B8ED-005056822391"), (new("4FD469D2-AEAF-E311-B8ED-005056822391"), new("7302FCBA-3589-E911-A958-000D3A2AA275")) },
        { new("2962393F-1BAF-E311-B8ED-005056822391"), (new("DACA45E8-474B-F011-877A-7C1E52203527"), new("F729CBBE-3989-E911-A958-000D3A2AA275")) },
    };

    private static IReadOnlyList<Guid> WelshIttProviderIds = [
        new("EAC72DD3-C7AE-E311-B8ED-005056822391"), // Aberystwyth University
        new("5EF135C7-C7AE-E311-B8ED-005056822391"), // Bangor University
        new("EB993DC1-C7AE-E311-B8ED-005056822391"), // Cardiff Institute Of Higher Educatio
        new("157135CD-C7AE-E311-B8ED-005056822391"), // Glyndwr University
        new("039A3DC1-C7AE-E311-B8ED-005056822391"), // Glyndwr University Wrexham ITT
        new("2C7135CD-C7AE-E311-B8ED-005056822391"), // Swansea Institute of Higher Educatio
        new("099A3DC1-C7AE-E311-B8ED-005056822391"), // Swansea Metropolitan University ITT
        new("C8993DC1-C7AE-E311-B8ED-005056822391"), // University College Cardiff
        new("D6F135C7-C7AE-E311-B8ED-005056822391"), // University College of North Wales
        new("0F7135CD-C7AE-E311-B8ED-005056822391"), // University of Wales College, Newport
        new("1D9A3DC1-C7AE-E311-B8ED-005056822391"), // University of Wales Institute, Cardi
        new("4D056786-C8AE-E311-B8ED-005056822391"), // University Of Wales Institute, Cardi
        new("9C993DC1-C7AE-E311-B8ED-005056822391"), // University Of Wales ITT
        new("119A3DC1-C7AE-E311-B8ED-005056822391"), // University of Wales Newport
        new("706F35CD-C7AE-E311-B8ED-005056822391"), // University of Wales Trinity Saint Da
        new("52F135C7-C7AE-E311-B8ED-005056822391"), // University of Wales, Aberystwyth ITT
        new("C5993DC1-C7AE-E311-B8ED-005056822391"), // University Of Wales, Cardiff ITT
        new("6AF135C7-C7AE-E311-B8ED-005056822391"), // University of Wales, Swansea ITT
    ];

    private static IReadOnlyList<dfeta_ITTResult> IttResultsToIgnore = [
        dfeta_ITTResult.ApplicationReceived,
        dfeta_ITTResult.ApplicationUnsuccessful,
        dfeta_ITTResult.NoResultSubmitted
    ];

    private readonly ISubject<object[]> _syncedEntitiesSubject = new Subject<object[]>();

    public IObservable<object[]> GetSyncedEntitiesObservable() => _syncedEntitiesSubject;

    private bool IsFakeXrm { get; } = organizationService.GetType().FullName == "Castle.Proxies.ObjectProxy_2";

    public (string EntityLogicalName, string[] AttributeNames) GetEntityInfoForModelType(string modelType)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo(modelType);
        return (modelTypeSyncInfo.EntityLogicalName, modelTypeSyncInfo.AttributeNames);
    }

    public bool TryMapDqtInductionExemptionReason(dfeta_InductionExemptionReason dqtInductionExemptionReason, out Guid inductionExemptionReasonId)
    {
        Guid? id = dqtInductionExemptionReason switch
        {
            dfeta_InductionExemptionReason.Exempt => new("a5faff9f-29ce-4a6b-a7b8-0c1f57f15920"),
            dfeta_InductionExemptionReason.ExemptDataLossErrorCriteria => new("204f86eb-0383-40eb-b793-6fccb76ecee2"),
            dfeta_InductionExemptionReason.HasoriseligibleforfullregistrationinScotland => new("a112e691-1694-46a7-8f33-5ec5b845c181"),
            dfeta_InductionExemptionReason.OverseasTrainedTeacher => new("4c97e211-10d2-4c63-8da9-b0fcebe7f2f9"),
            dfeta_InductionExemptionReason.Qualifiedbefore07May1999 => new("5a80cee8-98a8-426b-8422-b0e81cb49b36"),
            dfeta_InductionExemptionReason.Qualifiedbetween07May1999and01April2003FirstpostwasinWalesandlastedaminimumoftwoterms => new("15014084-2d8d-4f51-9198-b0e1881f8896"),
            dfeta_InductionExemptionReason.QualifiedthroughEEAmutualrecognitionroute => new("e7118bab-c2b1-4fe8-ad3f-4095d73f5b85"),
            dfeta_InductionExemptionReason.QualifiedthroughFEroutebetween01Sep2001and01Sep2004 => new("0997ab13-7412-4560-8191-e51ed4d58d2a"),
            dfeta_InductionExemptionReason.RegisteredTeacher_havingatleasttwoyearsfulltimeteachingexperience => new("42bb7bbc-a92c-4886-b319-3c1a5eac319a"),
            dfeta_InductionExemptionReason.SuccessfullycompletedinductioninGuernsey => new("fea2db23-93e0-49af-96fd-83c815c17c0b"),
            dfeta_InductionExemptionReason.SuccessfullycompletedinductioninIsleOfMan => new("e5c3847d-8fb6-4b31-8726-812392da8c5c"),
            dfeta_InductionExemptionReason.SuccessfullycompletedinductioninJersey => new("243b21a8-0be4-4af5-8874-85944357e7f8"),
            dfeta_InductionExemptionReason.SuccessfullycompletedinductioninNorthernIreland => new("3471ab35-e6e4-4fa9-a72b-b8bd113df591"),
            dfeta_InductionExemptionReason.SuccessfullycompletedinductioninServiceChildrensEducationschoolsinGermanyorCyprus => new("7d17d904-c1c6-451b-9e09-031314bd35f7"),
            dfeta_InductionExemptionReason.SuccessfullycompletedinductioninWales => InductionExemptionReason.PassedInWalesId,
            dfeta_InductionExemptionReason.SuccessfullycompletedprobationaryperiodinGibraltar => new("a751494a-7e7a-4836-96cb-00b9ed6e1b5f"),
            dfeta_InductionExemptionReason.TeacherhasbeenawardedQTLSandisexemptprovidedtheymaintaintheirmembershipwiththeSocietyforEducationandTraining => new("35caa6a3-49f2-4a63-bd5a-2ba5fa9dc5db"),
            _ => null
        };

        if (id is null)
        {
            inductionExemptionReasonId = default;
            return false;
        }

        inductionExemptionReasonId = id.Value;
        return true;
    }

    public async Task SyncAuditAsync(
        string entityLogicalName,
        IEnumerable<Guid> ids,
        bool skipIfExists,
        CancellationToken cancellationToken = default)
    {
        var idsToSync = ids.ToList();

        if (skipIfExists)
        {
            var existingAudits = await Task.WhenAll(
                idsToSync.Select(async id => (Id: id, HaveAudit: await auditRepository.HaveAuditDetailAsync(entityLogicalName, id))));

            foreach (var (id, _) in existingAudits.Where(t => t.HaveAudit))
            {
                idsToSync.Remove(id);
            }
        }

        if (idsToSync.Count == 0)
        {
            return;
        }

        var audits = await GetAuditRecordsAsync(entityLogicalName, idsToSync, cancellationToken);
        await Task.WhenAll(audits.Select(async kvp => await auditRepository.SetAuditDetailAsync(entityLogicalName, kvp.Key, kvp.Value)));
    }

    private static List<string> GetIgnoreNotesContainingTerms() => new List<string>
    {
        ".",
        "itt result updated to pass or approved as a part of tq data query exercise. the result was one of 102668 results corrected by a data fix run on 09 january 2009",
        "manpay1205 letter suppressed",
        "name amended in error by tp update, name corrected to previous entry held by gtc",
        "\u00A3", //£
        "0 day letter",
        "anomoly",
        "apology as previous letter had an incorrect details due to file creation error",
        "autoregd nqt reminder",
        "bulk dereg",
        "cancel fee",
        "cancellation of fees",
        "card details taken",
        "cc details taken",
        "cc received",
        "contacted regarding requirements for otts and instructors to be provisionally registered",
        "currfees only) letter issued",
        "curryrfees",
        "dd cancelled",
        "dd claim",
        "dd conf",
        "dd dconfirmation",
        "dd details",
        "dd end",
        "dd failed",
        "dd failure",
        "dd fee notice",
        "dd incorrect",
        "dd letter",
        "dd mandate",
        "dd notification",
        "dd pilot",
        "dd rec`d",
        "dd rejected",
        "dd ret",
        "dd returned",
        "dd run",
        "dereg action",
        "dereg form",
        "de - reg form",
        "dereg, action",
        "dereg,action",
        "dereg.",
        "deregistered",
        "deregistration confirmation",
        "de - registration date manually changed",
        "de - regn",
        "deregrem",
        "details extracted on",
        "did not claim the teacher",
        "direct debit",
        "dual registered - info from wales",
        "due to an administrative error this unregistered teacher did not have their deregistration date changed",
        "edc 2005 / 06 teachers working at relationship updated",
        "edc put out of service",
        "email address hard bounce",
        "email address irresolvable hard bounce back",
        "employer data collection",
        "employment at school",
        "employment update letter and cod form sent",
        "employment updated",
        "fee cancellation",
        "fee chase",
        "fee notice",
        "fee notification",
        "fee notin",
        "fee paid",
        "fee receipt",
        "fee reciept",
        "fee remind",
        "fee waiver",
        "fee year",
        "finance",
        "ftf - achieve - 20060606 - raceequalityandyourschool - leeds",
        "ftf - achieve - 20060607 - raceequalityandyourschool - leeds",
        "future dereg request",
        "general query - role & remit info sheet sent in response",
        "gtc / ioe conference",
        "in service(is) updated from edc",
        "inappropriate contact address removed and site usages for school address recalculated",
        "intention to dereg",
        "intention to register updated following receipt of a suitability declaration form - itt exit",
        "invalid email address removed",
        "la address removed as no mail should be sent to la",
        "letter sent to update employment details",
        "loaded home contact address as active post migration",
        "manually reregistered",
        "more info for deregistration",
        "more info to dereg",
        "more information for deregistration requested",
        "more information requested for deregistration",
        "new card",
        "newregcard",
        "no address email sent to",
        "not enough info to dereg teacher so letter and dereg form sent",
        "not registered",
        "nqtfee",
        "paid by dd",
        "part yr fees",
        "paydereg",
        "payment",
        "possible dereg",
        "provisional registration",
        "pryrfees",
        "re deregistration",
        "re scaled fees",
        "record has been temporarily inactivated, consult data governance team before amending these records",
        "refer to payer",
        "refund issued",
        "refund request",
        "regable45s letter issued and attached",
        "registration has now been removed",
        "registration status manually amended from ineligible for full registration to ineligible",
        "replacement card",
        "replacement registration card",
        "replacementcard",
        "request refund",
        "requested more information to deregister",
        "school address in home address field removed by reg",
        "sd fees",
        "sent application to register",
        "supply(nonla) employment closed",
        "teacher not registered.sent a reminder that in error refers to them as being registered",
        "teacher put out of service and then back in service at the same school to correct site usage",
        "teacher removed from service at organisation",
        "teacher taken out of service at welsh school",
        "teacher was recorded either as teacher supply agency or unattached supply teaching and is now recorded as supply (non la)",
        "teacher was sent a confirmation letter confirming that they were provisionally? registered. the letter incorrectly referred to them as an ott and they will receive another letter",
        "this record was inactive with an active address.the the address has been inactivated  with an end date of the day it was closed",
        "total amount due",
        "total to be paid",
        "unregistered teacher had an incorrect deregistration date",
        "updated employment",
        "updated from edc",
        "withpryrfees) letter issued to"
    }
    .Select(s => s.ToLowerInvariant())
    .ToList();

    private async Task<DqtNoteInfo?> MapNoteFromDqtAnnotationAsync(
        Annotation annotation)
    {
        var ignoredTerms = GetIgnoreNotesContainingTerms();
        var lowerInput = await annotation.GetNoteTextWithoutHtmlAsync() ?? string.Empty;
        if (ignoredTerms.Any(term => lowerInput.ToLower().Contains(term)) &&
            (!string.IsNullOrEmpty(annotation.Subject) && annotation.Subject.Contains("Entered by REG", StringComparison.InvariantCultureIgnoreCase)))
        {
            return null;
        }

        return new DqtNoteInfo()
        {
            Id = annotation.Id,
            PersonId = annotation!.ObjectId.Id,
            ContentHtml = annotation.NoteText,
            CreatedByDqtUserId = annotation.CreatedBy.Id,
            CreatedByDqtUserName = annotation.CreatedBy.Name,
            UpdatedOn = annotation.ModifiedOn,
            CreatedOn = annotation.CreatedOn!.Value,
            UpdatedByDqtUserId = annotation.ModifiedBy?.Id,
            UpdatedByDqtUserName = annotation.ModifiedBy?.Name,
            FileName = null,
            AttachmentBytes = string.IsNullOrEmpty(annotation.FileName) ? null : Convert.FromBase64String(annotation.DocumentBody),
            OriginalFileName = annotation.FileName,
            MimeType = annotation.MimeType,
        };
    }

    private InductionInfo? MapInductionInfoFromDqtInduction(
        dfeta_induction? induction,
        Contact contact,
        bool ignoreInvalid)
    {
        // Double check that contact record induction status matches the induction record (if there is one) induction status (which should have been set via CRM plugin)
        var hasQtls = contact.dfeta_qtlsdate is not null;
        if (induction is not null && induction.dfeta_InductionStatus != contact.dfeta_InductionStatus)
        {
            var errorMessage = $"Induction status {contact.dfeta_InductionStatus} for contact {contact.ContactId} does not match induction status {induction.dfeta_InductionStatus} for induction {induction!.dfeta_inductionId}.";
            if (ignoreInvalid)
            {
                logger.LogWarning(errorMessage);
                return null;
            }

            throw new InvalidOperationException(errorMessage);
        }
        // Person with QTLS should be exempt from induction
        else if (hasQtls && contact.dfeta_InductionStatus != dfeta_InductionStatus.Exempt)
        {
            var errorMessage = $"Induction status for contact {contact.ContactId} with QTLS should be {dfeta_InductionStatus.Exempt} but is {contact.dfeta_InductionStatus}.";
            if (ignoreInvalid)
            {
                logger.LogWarning(errorMessage);
                return null;
            }

            throw new InvalidOperationException(errorMessage);
        }

        Guid[] exemptionReasonIds = [];
        if (induction?.dfeta_InductionExemptionReason is not null)
        {
            if (!TryMapDqtInductionExemptionReason(induction.dfeta_InductionExemptionReason!.Value, out var exemptionReasonId))
            {
                var errorMessage = $"Failed mapping DQT Induction Exemption Reason '{induction.dfeta_InductionExemptionReason}' for contact {contact.ContactId}.";
                if (ignoreInvalid)
                {
                    logger.LogWarning(errorMessage);
                    return null;
                }

                throw new InvalidOperationException(errorMessage);
            }

            exemptionReasonIds = [exemptionReasonId];
        }

        return new InductionInfo()
        {
            PersonId = contact.ContactId!.Value,
            InductionId = induction?.dfeta_inductionId,
            InductionStatus = contact.dfeta_InductionStatus.ToInductionStatus(),
            InductionStartDate = induction?.dfeta_StartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            InductionCompletedDate = induction?.dfeta_CompletionDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            InductionExemptionReasonIds = exemptionReasonIds,
            DqtModifiedOn = induction?.ModifiedOn,
            InductionExemptWithoutReason = contact.dfeta_InductionStatus.ToInductionStatus() is InductionStatus.Exempt && exemptionReasonIds.Length == 0
        };
    }

    public async Task DeleteRecordsAsync(string modelType, IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return;
        }

        var modelTypeSyncInfo = GetModelTypeSyncInfo(modelType);

        if (modelTypeSyncInfo.DeleteStatement is null)
        {
            if (!modelTypeSyncInfo.IgnoreDeletions)
            {
                throw new NotSupportedException($"Cannot delete a {modelType}.");
            }
            else
            {
                logger.LogWarning($"Ignoring deletion of {ids.Count} {modelType} records.");
                return;
            }
        }

        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = modelTypeSyncInfo.DeleteStatement;
            cmd.Parameters.Add(new NpgsqlParameter(IdsParameterName, ids.ToArray()));
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task<DateTime?> GetLastModifiedOnForModelTypeAsync(string modelType)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo(modelType);

        if (modelTypeSyncInfo.GetLastModifiedOnStatement is null)
        {
            return null;
        }

        await using var connection = await trsDbDataSource.OpenConnectionAsync();

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = modelTypeSyncInfo.GetLastModifiedOnStatement;
            await using var reader = await cmd.ExecuteReaderAsync();
            var read = reader.Read();
            Debug.Assert(read);
            return reader.IsDBNull(0) ? null : reader.GetDateTime(0);
        }
    }

    public Task SyncRecordsAsync(string modelType, IReadOnlyCollection<Entity> entities, bool ignoreInvalid, bool dryRun, CancellationToken cancellationToken = default)
    {
        if (entities.Count == 0)
        {
            return Task.CompletedTask;
        }

        var modelTypeSyncInfo = GetModelTypeSyncInfo(modelType);
        return modelTypeSyncInfo.GetSyncHandler(this)(entities, ignoreInvalid, dryRun, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> SyncPersonsAsync(IReadOnlyCollection<Guid> contactIds, bool syncAudit, bool ignoreInvalid = false, bool dryRun = false, CancellationToken cancellationToken = default)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo(ModelTypes.Person);

        var contacts = await GetEntitiesAsync<Contact>(
            Contact.EntityLogicalName,
            Contact.PrimaryIdAttribute,
            contactIds,
            modelTypeSyncInfo.AttributeNames,
            activeOnly: false,
            cancellationToken);

        return await SyncPersonsAsync(contacts, syncAudit, ignoreInvalid, dryRun, cancellationToken);
    }

    public async Task<bool> SyncPersonAsync(Guid contactId, bool syncAudit, bool ignoreInvalid = false, bool dryRun = false, CancellationToken cancellationToken = default) =>
        (await SyncPersonsAsync([contactId], syncAudit, ignoreInvalid, dryRun, cancellationToken)).Count() == 1;

    public async Task<bool> SyncPersonAsync(Contact entity, bool syncAudit, bool ignoreInvalid, bool dryRun = false, CancellationToken cancellationToken = default) =>
        (await SyncPersonsAsync(new[] { entity }, syncAudit, ignoreInvalid, dryRun, cancellationToken)).Count() == 1;

    public async Task<IReadOnlyCollection<Guid>> SyncPersonsAsync(
        IReadOnlyCollection<Contact> entities,
        bool syncAudit,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        if (syncAudit)
        {
            await SyncAuditAsync(Contact.EntityLogicalName, entities.Select(q => q.ContactId!.Value), skipIfExists: false, cancellationToken);
        }

        var auditDetails = await GetAuditRecordsFromAuditRepositoryAsync(Contact.EntityLogicalName, Contact.PrimaryIdAttribute, entities.Select(q => q.ContactId!.Value), cancellationToken);
        return await SyncPersonsAsync(entities, auditDetails, ignoreInvalid, dryRun, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> SyncPersonsAsync(
        IReadOnlyCollection<Contact> entities,
        IReadOnlyDictionary<Guid, AuditDetailCollection> auditDetails,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        var (persons, events) = MapPersonsAndAudits(entities, auditDetails, ignoreInvalid);
        return await SyncPersonsAsync(persons, events, ignoreInvalid, dryRun, cancellationToken);
    }

    private async Task<IReadOnlyCollection<Guid>> SyncPersonsAsync(
        IReadOnlyCollection<PersonInfo> persons,
        IReadOnlyCollection<EventBase> events,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        var toSync = persons.ToList();

        if (ignoreInvalid)
        {
            // Some bad data in the build environment has a TRN that's longer than 7 digits
            toSync = toSync.Where(p => string.IsNullOrEmpty(p.Trn) || p.Trn.Length == 7).ToList();
        }

        if (!toSync.Any())
        {
            return [];
        }

        var modelTypeSyncInfo = GetModelTypeSyncInfo<PersonInfo>(ModelTypes.Person);

        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);
        using var txn = await connection.BeginTransactionAsync(cancellationToken);

        using (var createTempTableCommand = connection.CreateCommand())
        {
            createTempTableCommand.CommandText = modelTypeSyncInfo.CreateTempTableStatement;
            createTempTableCommand.Transaction = txn;
            await createTempTableCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        using var writer = await connection.BeginBinaryImportAsync(modelTypeSyncInfo.CopyStatement!, cancellationToken);

        foreach (var person in toSync)
        {
            writer.StartRow();
            modelTypeSyncInfo.WriteRecord!(writer, person);
        }

        await writer.CompleteAsync(cancellationToken);
        await writer.CloseAsync(cancellationToken);

        try
        {
            using (var mergeCommand = connection.CreateCommand())
            {
                mergeCommand.CommandText = modelTypeSyncInfo.UpsertStatement;
                mergeCommand.Parameters.Add(new NpgsqlParameter(NowParameterName, clock.UtcNow));
                mergeCommand.Transaction = txn;
                await mergeCommand.ExecuteNonQueryAsync();
            }
        }
        catch (PostgresException ex) when (ex.SqlState == "23503" && ex.ConstraintName == "fk_persons_persons_merged_with_person_id")
        {
            // Trying to insert a record that has been merged with another person record which itself has not been synced yet!!
            // ex.Detail will be something like "Key (merged_with_person_id)=(5f0d1146-1836-eb11-a813-000d3a2287a4) is not present in table "persons"."
            var unsyncedPersonId = Guid.Parse(ex.Detail!.Substring("Key (merged_with_person_id)=(".Length, 36));
            await txn.DisposeAsync();
            await connection.DisposeAsync();

            var personSynced = await SyncPersonAsync(unsyncedPersonId, syncAudit: true, ignoreInvalid, dryRun: false, cancellationToken);

            if (personSynced)
            {
                // Retry syncing the original person record
                return await SyncPersonsAsync(toSync, events, ignoreInvalid, dryRun, cancellationToken);
            }
            else
            {
                // If we failed to sync the person record, we cannot continue.
                throw new InvalidOperationException($"Failed to sync person with ID {unsyncedPersonId} which is required for syncing the original person record.");
            }
        }
        catch (PostgresException ex) when (ignoreInvalid && ex.SqlState == "23505" && ex.ConstraintName == "ix_persons_trn")
        {
            // Record already exists with TRN.
            // Remove the record from the collection and try again.

            // ex.Detail will be something like "Key (trn)=(1000336) already exists."
            var trn = ex.Detail!.Substring("Key (trn)=(".Length, 7);

            if (trn.Length != 7 || !trn.All(char.IsAsciiDigit))
            {
                Debug.Fail("Failed parsing TRN from exception message.");
                throw;
            }

            var personsExceptFailedOne = persons.Where(e => e.Trn != trn).ToArray();
            var eventsExceptFailedOne = events.Where(e => e is not IEventWithPersonId || ((IEventWithPersonId)e).PersonId != personsExceptFailedOne[0].PersonId).ToArray();

            // Be extra sure we've actually removed a record (otherwise we'll go in an endless loop and stack overflow)
            if (!(personsExceptFailedOne.Length < persons.Count))
            {
                Debug.Fail("No persons removed from collection.");
                throw;
            }

            await txn.DisposeAsync();
            await connection.DisposeAsync();

            return await SyncPersonsAsync(personsExceptFailedOne, eventsExceptFailedOne, ignoreInvalid, dryRun, cancellationToken);
        }

        await txn.SaveEventsAsync(events, "events_person_import", clock, cancellationToken, timeoutSeconds: 120);

        if (!dryRun)
        {
            await txn.CommitAsync(cancellationToken);
        }

        _syncedEntitiesSubject.OnNext([.. toSync, .. events]);
        return toSync.Select(p => p.PersonId).ToArray();
    }

    public async Task<int> SyncInductionsAsync(
        IReadOnlyCollection<dfeta_induction> inductions,
        bool syncAudit,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var contactAttributeNames = GetModelTypeSyncInfo(ModelTypes.Person).AttributeNames;

        var contacts = await GetEntitiesAsync<Contact>(
            Contact.EntityLogicalName,
            Contact.PrimaryIdAttribute,
            inductions.Select(e => e.dfeta_PersonId.Id),
            contactAttributeNames,
            activeOnly: false,
            cancellationToken);

        return await SyncInductionsAsync(contacts, inductions, syncAudit, ignoreInvalid, dryRun, cancellationToken);
    }

    public async Task<int> SyncInductionsAsync(
        IReadOnlyCollection<Contact> contacts,
        bool syncAudit,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo(ModelTypes.Induction);

        var inductions = await GetEntitiesAsync<dfeta_induction>(
            dfeta_induction.EntityLogicalName,
            dfeta_induction.Fields.dfeta_PersonId,
            contacts.Select(c => c.ContactId!.Value),
            modelTypeSyncInfo.AttributeNames,
            true,
            cancellationToken);

        return await SyncInductionsAsync(contacts, inductions, syncAudit, ignoreInvalid, dryRun, cancellationToken);
    }

    public async Task<int> SyncAnnotationsAsync(
        IReadOnlyCollection<Annotation> annotations,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo<DqtNoteInfo>(ModelTypes.DqtNote);
        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);
        var toSync = new List<DqtNoteInfo>();
        foreach (var ann in annotations)
        {
            var dqtNote = await MapNoteFromDqtAnnotationAsync(ann);
            if (dqtNote is not null)
            {
                toSync.Add(dqtNote);
            }
        }

        //upload attachments new or remove attachment
        foreach (var noteAttachment in toSync)
        {
            var fileId = noteAttachment!.Id;

            //if note does not have an attachment or length is 0, attempt delete
            if (noteAttachment!.AttachmentBytes is null || noteAttachment.AttachmentBytes!.Length == 0)
            {
                //not interested if file exists or not
                await fileService.DeleteFileAsync(fileId!.Value);
                noteAttachment.FileName = null;
                noteAttachment.OriginalFileName = null;
            }

            //upload new attachment
            if (noteAttachment!.AttachmentBytes != null)
            {
                //incoming note attachment filename is the annotation id
                var bytes = noteAttachment.AttachmentBytes;
                using (var stream = new MemoryStream(bytes))
                {
                    await fileService.UploadFileAsync(stream, noteAttachment.MimeType, noteAttachment.Id);
                    noteAttachment.OriginalFileName = noteAttachment.OriginalFileName;
                    noteAttachment.FileName = fileId.ToString();
                }
            }
        }

        using var txn = await connection.BeginTransactionAsync(cancellationToken);

        using (var createTempTableCommand = connection.CreateCommand())
        {
            createTempTableCommand.CommandText = modelTypeSyncInfo.CreateTempTableStatement;
            createTempTableCommand.Transaction = txn;
            await createTempTableCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        using var writer = await connection.BeginBinaryImportAsync(modelTypeSyncInfo.CopyStatement!, cancellationToken);

        foreach (var i in toSync)
        {
            writer.StartRow();
            modelTypeSyncInfo.WriteRecord!(writer, i!);
        }

        await writer.CompleteAsync(cancellationToken);
        await writer.CloseAsync(cancellationToken);

        using (var mergeCommand = connection.CreateCommand())
        {
            mergeCommand.CommandText = modelTypeSyncInfo.UpsertStatement;
            mergeCommand.Parameters.Add(new NpgsqlParameter(NowParameterName, clock.UtcNow));
            mergeCommand.Transaction = txn;
            await mergeCommand.ExecuteNonQueryAsync();
        }

        if (!dryRun)
        {
            await txn.CommitAsync(cancellationToken);
        }

        _syncedEntitiesSubject.OnNext([.. toSync]);
        return toSync.Count;
    }

    public async Task<int> SyncInductionsAsync(
        IReadOnlyCollection<Contact> contacts,
        IReadOnlyCollection<dfeta_induction> entities,
        bool syncAudit,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        if (syncAudit)
        {
            await SyncAuditAsync(dfeta_induction.EntityLogicalName, entities.Select(q => q.Id), skipIfExists: false, cancellationToken);
        }

        var auditDetails = await GetAuditRecordsFromAuditRepositoryAsync(dfeta_induction.EntityLogicalName, dfeta_induction.PrimaryIdAttribute, entities.Select(q => q.Id), cancellationToken);
        return await SyncInductionsAsync(contacts, entities, auditDetails, ignoreInvalid, dryRun, cancellationToken);
    }

    public async Task<int> SyncInductionsAsync(
        IReadOnlyCollection<Contact> contacts,
        IReadOnlyCollection<dfeta_induction> entities,
        IReadOnlyDictionary<Guid, AuditDetailCollection> auditDetails,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var (inductions, events) = MapInductionsAndAudits(contacts, entities, auditDetails, ignoreInvalid);
        return await SyncInductionsAsync(inductions, events, ignoreInvalid, dryRun, cancellationToken);
    }

    private async Task<int> SyncInductionsAsync(
        IReadOnlyCollection<InductionInfo> inductions,
        IReadOnlyCollection<EventBase> events,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo<InductionInfo>(ModelTypes.Induction);

        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);

        var toSync = inductions.ToList();

        do
        {
            using var txn = await connection.BeginTransactionAsync(cancellationToken);

            using (var createTempTableCommand = connection.CreateCommand())
            {
                createTempTableCommand.CommandText = modelTypeSyncInfo.CreateTempTableStatement;
                createTempTableCommand.Transaction = txn;
                await createTempTableCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            using var writer = await connection.BeginBinaryImportAsync(modelTypeSyncInfo.CopyStatement!, cancellationToken);

            foreach (var i in toSync)
            {
                writer.StartRow();
                modelTypeSyncInfo.WriteRecord!(writer, i);
            }

            await writer.CompleteAsync(cancellationToken);
            await writer.CloseAsync(cancellationToken);

            var syncedInductionPersonIds = new List<Guid>();

            using (var mergeCommand = connection.CreateCommand())
            {
                mergeCommand.CommandText = modelTypeSyncInfo.UpsertStatement;
                mergeCommand.Parameters.Add(new NpgsqlParameter(NowParameterName, clock.UtcNow));
                mergeCommand.Transaction = txn;
                using var reader = await mergeCommand.ExecuteReaderAsync();
                while (await reader.ReadAsync(cancellationToken))
                {
                    syncedInductionPersonIds.Add(reader.GetGuid(0));
                }
            }

            var unsyncedContactIds = toSync
                .Where(i => !syncedInductionPersonIds.Contains(i.PersonId))
                .Select(i => i.PersonId)
                .ToArray();

            if (unsyncedContactIds.Length > 0)
            {
                var personsSynced = await SyncPersonsAsync(unsyncedContactIds, syncAudit: true, ignoreInvalid, dryRun: false, cancellationToken);
                var unableToSyncContactIds = unsyncedContactIds.Where(id => !personsSynced.Contains(id)).ToArray();
                if (unableToSyncContactIds.Length > 0)
                {
                    var errorMessage = $"Attempted to sync Induction for persons but the Contact records with IDs [{string.Join(", ", unableToSyncContactIds)}] do not meet the sync criteria.";
                    if (ignoreInvalid)
                    {
                        logger.LogWarning(errorMessage);
                        toSync.RemoveAll(i => unableToSyncContactIds.Contains(i.PersonId));
                        if (toSync.Count == 0)
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException(errorMessage);
                    }
                }

                continue;
            }

            var eventsForSyncedContacts = events
                .Where(e => e is IEventWithPersonId && !unsyncedContactIds.Any(c => c == ((IEventWithPersonId)e).PersonId))
                .ToArray();

            await txn.SaveEventsAsync(eventsForSyncedContacts, "events_induction_import", clock, cancellationToken, timeoutSeconds: 120);

            if (!dryRun)
            {
                await txn.CommitAsync(cancellationToken);
            }

            break;
        }
        while (true);

        _syncedEntitiesSubject.OnNext([.. toSync, .. events]);
        return toSync.Count;
    }

    public async Task<int> MigrateInductionsAsync(
        IReadOnlyCollection<Contact> contacts,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo(ModelTypes.Induction);

        var inductions = await GetEntitiesAsync<dfeta_induction>(
            dfeta_induction.EntityLogicalName,
            dfeta_induction.Fields.dfeta_PersonId,
            contacts.Select(c => c.ContactId!.Value),
            modelTypeSyncInfo.AttributeNames,
            true,
            cancellationToken);

        var inductionLookup = inductions
            .GroupBy(i => i.dfeta_PersonId.Id)
            .ToDictionary(g => g.Key, g => g.ToArray());

        var events = new List<EventBase>();

        foreach (var contact in contacts)
        {
            dfeta_induction? induction = null;
            if (inductionLookup.TryGetValue(contact.ContactId!.Value, out var personInductions))
            {
                // We shouldn't have multiple induction records for the same person in prod at all but we might in other environments
                // so we'll just take the most recently modified one.
                induction = personInductions.OrderByDescending(i => i.ModifiedOn).First();
                if (personInductions.Length > 1)
                {
                    var errorMessage = $"Contact '{contact.ContactId!.Value}' has multiple induction records.";
                    if (ignoreInvalid)
                    {
                        logger.LogWarning(errorMessage);
                    }
                    else
                    {
                        throw new InvalidOperationException(errorMessage);
                    }
                }
            }

            if (induction is null && contact.dfeta_InductionStatus is null)
            {
                continue;
            }

            var mapped = MapInductionInfoFromDqtInduction(induction, contact, ignoreInvalid);
            if (mapped is null)
            {
                continue;
            }

            var migratedEvent = MapMigratedEvent(contact, induction, mapped);
            events.Add(migratedEvent);
        }

        if (events.Count == 0)
        {
            return 0;
        }

        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);
        using var txn = await connection.BeginTransactionAsync(cancellationToken);
        await txn.SaveEventsAsync(events, "events_induction_migration", clock, cancellationToken);

        if (!dryRun)
        {
            await txn.CommitAsync();
        }

        return events.Count;

        EventBase MapMigratedEvent(Contact contact, dfeta_induction? dqtInduction, InductionInfo mappedInduction)
        {
            var dqtInductionStatus = (contact.dfeta_InductionStatus?.GetMetadata().Name ??
                dqtInduction?.dfeta_InductionStatus?.GetMetadata().Name)!;

            return new InductionMigratedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{dqtInduction?.Id ?? contact.Id}-Migrated",
                CreatedUtc = new DateTime(2025, 02, 18, 10, 17, 05, DateTimeKind.Utc),
                RaisedBy = EventModels.RaisedByUserInfo.FromUserId(Core.DataStore.Postgres.Models.SystemUser.SystemUserId),
                PersonId = contact.Id,
                InductionStartDate = mappedInduction.InductionStartDate,
                InductionCompletedDate = mappedInduction.InductionCompletedDate,
                InductionStatus = mappedInduction.InductionStatus,
                InductionExemptionReasonId = mappedInduction.InductionExemptionReasonIds switch
                {
                    [var id] => id,
                    _ => null
                },
                DqtInduction = dqtInduction is not null ? GetEventDqtInduction(dqtInduction) : null,
                DqtInductionStatus = dqtInductionStatus
            };
        }
    }

    public async Task<int> SyncPreviousNamesForContactsAsync(
        IReadOnlyCollection<Guid> contactIds,
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo<PreviousName>(ModelTypes.PreviousName);

        var contacts = (await GetEntitiesAsync<Contact>(
                Contact.EntityLogicalName,
                Contact.PrimaryIdAttribute,
                contactIds,
                [Contact.Fields.FirstName, Contact.Fields.MiddleName, Contact.Fields.LastName, Contact.Fields.ModifiedOn],
                activeOnly: false,
                cancellationToken))
            .ToDictionary(c => c.ContactId!.Value, c => c);

        var previousNamesByContactId = await GetPreviousNamesByContactIdAsync(contactIds, cancellationToken);

        var mapped = contactIds.SelectMany(id => MapPreviousNames(contacts[id], previousNamesByContactId[id])).ToArray();

        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);
        using var txn = await connection.BeginTransactionAsync(cancellationToken);

        using (var createTempTableCommand = connection.CreateCommand())
        {
            createTempTableCommand.CommandText = modelTypeSyncInfo.CreateTempTableStatement;
            createTempTableCommand.Transaction = txn;
            await createTempTableCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        using var writer = await connection.BeginBinaryImportAsync(modelTypeSyncInfo.CopyStatement!, cancellationToken);

        foreach (var i in mapped)
        {
            writer.StartRow();
            modelTypeSyncInfo.WriteRecord!(writer, i);
        }

        await writer.CompleteAsync(cancellationToken);
        await writer.CloseAsync(cancellationToken);

        using (var mergeCommand = connection.CreateCommand())
        {
            mergeCommand.CommandTimeout = 0;
            mergeCommand.CommandText = modelTypeSyncInfo.UpsertStatement;
            mergeCommand.Parameters.Add(new NpgsqlParameter(NowParameterName, clock.UtcNow));
            mergeCommand.Transaction = txn;
            await mergeCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        if (!dryRun)
        {
            await txn.CommitAsync(cancellationToken);
        }

        return mapped.Length;
    }

    private async Task<IReadOnlyDictionary<Guid, dfeta_previousname[]>> GetPreviousNamesByContactIdAsync(
        IReadOnlyCollection<Guid> contactIds,
        CancellationToken cancellationToken = default)
    {
        if (contactIds.Count == 0)
        {
            return new Dictionary<Guid, dfeta_previousname[]>();
        }

        var modelTypeSyncInfo = GetModelTypeSyncInfo(ModelTypes.PreviousName);

        var queryExpression = new QueryExpression
        {
            EntityName = modelTypeSyncInfo.EntityLogicalName,
            ColumnSet = new(modelTypeSyncInfo.AttributeNames),
            Criteria = new FilterExpression()
            {
                Conditions =
                {
                    new ConditionExpression(dfeta_previousname.Fields.dfeta_PersonId, ConditionOperator.In, contactIds.ToArray())
                }
            }
        };

        var results = await organizationService.RetrieveMultipleAsync(queryExpression, cancellationToken);

        return results.Entities
            .Select(e => e.ToEntity<dfeta_previousname>())
            .GroupBy(r => r.GetAttributeValue<EntityReference>(dfeta_previousname.Fields.dfeta_PersonId).Id)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }

    public async Task<int> SyncEventsAsync(IReadOnlyCollection<dfeta_TRSEvent> events, bool dryRun, CancellationToken cancellationToken = default)
    {
        var modelTypeSyncInfo = GetModelTypeSyncInfo<EventInfo>(ModelTypes.Event);

        var mapped = events.Select(e => EventInfo.Deserialize(e.dfeta_Payload).Event).ToArray();

        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);

        using var txn = await connection.BeginTransactionAsync(cancellationToken);
        await txn.SaveEventsAsync(mapped, tempTableSuffix: "events_import", clock, cancellationToken);

        if (!dryRun)
        {
            await txn.CommitAsync();
        }

        _syncedEntitiesSubject.OnNext(events.ToArray());
        return events.Count;
    }

    private async Task<Guid[]> EnsurePersonsSyncedAsync(Guid[] personIds, bool ignoreInvalid = false, bool dryRun = false, CancellationToken cancellationToken = default)
    {
        using var dbContext = TrsDbContext.Create(trsDbDataSource);
        var alreadySyncedPersonIds = await dbContext.Persons
            .Where(p => personIds.Contains(p.PersonId))
            .Select(p => p.PersonId)
            .ToListAsync(cancellationToken);
        var unsyncedPersonIds = personIds.Except(alreadySyncedPersonIds).ToArray();
        if (!unsyncedPersonIds.Any())
        {
            return personIds;
        }

        var syncedPersonIds = await SyncPersonsAsync(unsyncedPersonIds, syncAudit: true, ignoreInvalid, dryRun, cancellationToken);
        var unsyncablePersonIds = unsyncedPersonIds.Where(id => !syncedPersonIds.Contains(id)).ToArray();
        if (unsyncablePersonIds.Length > 0)
        {
            return personIds.Except(unsyncablePersonIds).ToArray();
        }

        return personIds;
    }

    private async Task<int> MigrateRoutesAsync(
        IReadOnlyCollection<ContactIttQtsMapResult> mappings,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        if (!mappings.Any())
        {
            return 0;
        }

        // Ensure we sync any Persons that are not already synced
        var personIds = mappings.Where(m => m.MappedResults.Any(r => r.Success)).Select(m => m.ContactId).Distinct().ToArray();
        var syncedPersonIds = await EnsurePersonsSyncedAsync(personIds, ignoreInvalid, dryRun: false, cancellationToken);
        var unsyncedPersonIds = personIds.Except(syncedPersonIds).ToArray();
        var mappingsToProcess = mappings.ToArray();
        if (ignoreInvalid && unsyncedPersonIds.Length > 0)
        {
            // Remove mappings for persons that could not be synced - although I guess we could still report on them in non-prod environments
            mappingsToProcess = mappingsToProcess.Where(m => !unsyncedPersonIds.Contains(m.ContactId)).ToArray();
        }

        var personIdsToSync = mappingsToProcess
            .Where(m => m.MappedResults.Any(r => r.Success)).Select(m => m.ContactId)
            .ToArray();

        var modelTypeSyncInfo = GetModelTypeSyncInfo<RouteToProfessionalStatus>(ModelTypes.Route);
        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);

        using var txn = await connection.BeginTransactionAsync(cancellationToken);

        using var dbContext = TrsDbContext.Create(connection);
        dbContext.Database.UseTransaction(txn);

        var persons = await dbContext.Persons
            .FromSql(
            $"""
            SELECT
            	*
            FROM
            	persons p
            WHERE
            	person_id = ANY({personIdsToSync})
            FOR UPDATE
            """)
            .IgnoreQueryFilters()
            .ToListAsync();

        var toSync = new List<RouteToProfessionalStatus>();
        var events = new List<EventBase>();
        foreach (var mapping in mappingsToProcess.Where(m => personIdsToSync.Contains(m.ContactId)))
        {
            var person = persons.Single(p => p.PersonId == mapping.ContactId);
            var inductionExemptionReasonIdsToRemove = new HashSet<Guid>();
            foreach (var result in mapping.MappedResults.Where(m => m.Success))
            {
                var inductionExemptionReasonIdsMovedFromPerson = new List<Guid>();
                var inductionExemptionReasonId = result.ProfessionalStatusInfo!.RouteToProfessionalStatusType!.InductionExemptionReasonId;
                if (inductionExemptionReasonId is not null)
                {
                    if (person.InductionExemptionReasonIds.Any(id => id == inductionExemptionReasonId))
                    {
                        inductionExemptionReasonIdsToRemove.Add(inductionExemptionReasonId.Value);
                        inductionExemptionReasonIdsMovedFromPerson.Add(inductionExemptionReasonId.Value);
                        result.ProfessionalStatusInfo!.ProfessionalStatus!.ExemptFromInduction = true;
                    }
                    else if (inductionExemptionReasonId == InductionExemptionReason.QtlsId)
                    {
                        // if for some reason the exemption reasons have got out of whack on Person for QTLS, still set it to true
                        result.ProfessionalStatusInfo!.ProfessionalStatus!.ExemptFromInduction = true;
                    }
                    else
                    {

                        result.ProfessionalStatusInfo!.ProfessionalStatus!.ExemptFromInduction = false;
                    }
                }

                if (result.ProfessionalStatusInfo!.RouteToProfessionalStatusType!.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus &&
                    result.ProfessionalStatusInfo!.ProfessionalStatus!.Status == RouteToProfessionalStatusStatus.Holds &&
                    result.ProfessionalStatusInfo!.ProfessionalStatus.HoldsFrom < new DateOnly(2000, 05, 07) &&
                    person.InductionExemptionReasonIds.Any(id => id == InductionExemptionReason.QualifiedBefore7May2000Id))
                {
                    inductionExemptionReasonIdsToRemove.Add(InductionExemptionReason.QualifiedBefore7May2000Id);
                    inductionExemptionReasonIdsMovedFromPerson.Add(InductionExemptionReason.QualifiedBefore7May2000Id);
                    result.ProfessionalStatusInfo!.ProfessionalStatus!.ExemptFromInductionDueToQtsDate = true;
                }

                result.InductionExemptionReasonIdsMovedFromPerson = inductionExemptionReasonIdsMovedFromPerson.ToArray();

                toSync.Add(result.ProfessionalStatusInfo!.ProfessionalStatus!);
                events.Add(MapMigratedEvent(result, person));
            }

            // Remove any induction exemption reasons that need to be moved to the route
            foreach (var inductionExemptionReasonId in inductionExemptionReasonIdsToRemove)
            {
                var oldEventInduction = EventModels.Induction.FromModel(person);
                var now = clock.UtcNow;
                if (person.UnsafeRemoveInductionExemptionReason(inductionExemptionReasonId, SystemUser.SystemUserId, now))
                {
                    events.Add(new PersonInductionUpdatedEvent()
                    {
                        EventId = Guid.NewGuid(),
                        CreatedUtc = now,
                        RaisedBy = SystemUser.SystemUserId,
                        PersonId = person.PersonId,
                        Induction = EventModels.Induction.FromModel(person),
                        OldInduction = oldEventInduction,
                        ChangeReason = "Moved to route",
                        ChangeReasonDetail = $"Moved induction exemption reason '{inductionExemptionReasonId}' to route.",
                        EvidenceFile = null,
                        Changes = PersonInductionUpdatedEventChanges.InductionExemptionReasons
                    });
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        using (var createTempTableCommand = connection.CreateCommand())
        {
            createTempTableCommand.CommandText = modelTypeSyncInfo.CreateTempTableStatement;
            createTempTableCommand.Transaction = txn;
            await createTempTableCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        using var writer = await connection.BeginBinaryImportAsync(modelTypeSyncInfo.CopyStatement!, cancellationToken);

        foreach (var route in toSync)
        {
            writer.StartRow();
            modelTypeSyncInfo.WriteRecord!(writer, route);
        }

        await writer.CompleteAsync(cancellationToken);
        await writer.CloseAsync(cancellationToken);

        using (var mergeCommand = connection.CreateCommand())
        {
            mergeCommand.CommandText = modelTypeSyncInfo.UpsertStatement;
            mergeCommand.Transaction = txn;
            await mergeCommand.ExecuteNonQueryAsync();
        }

        await txn.SaveEventsAsync(events, "events_route_migration", clock, cancellationToken, timeoutSeconds: 120);

        var reportItems = mappingsToProcess
            .SelectMany(m => m.MappedResults.Select(r => MapReportItem(r)))
            .ToList();

        await SaveMigrationReportAsync(txn, reportItems, cancellationToken);

        if (!dryRun)
        {
            await txn.CommitAsync(cancellationToken);
        }

        _syncedEntitiesSubject.OnNext([.. toSync, .. events]);
        return toSync.Count;

        EventBase MapMigratedEvent(IttQtsMapResult mapResult, Person person)
        {
            return new RouteToProfessionalStatusMigratedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = clock.UtcNow,
                RaisedBy = SystemUser.SystemUserId,
                PersonId = mapResult.ContactId,
                RouteToProfessionalStatus = EventModels.RouteToProfessionalStatus.FromModel(mapResult.ProfessionalStatusInfo!.ProfessionalStatus!),
                PersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(person),
                OldPersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(person),
                DqtInitialTeacherTraining = MapInitialTeacherTraining(mapResult),
                DqtQtsRegistration = MapQtsRegistration(mapResult),
                DqtQtlsDate = mapResult.QtlsDate,
                DqtQtlsDateHasBeenSet = mapResult.QtlsDateHasBeenSet
            };
        }

        Events.Models.DqtInitialTeacherTraining? MapInitialTeacherTraining(IttQtsMapResult mapResult)
        {
            if (mapResult.IttId is null)
            {
                return null;
            }

            return new Events.Models.DqtInitialTeacherTraining()
            {
                InitialTeacherTrainingId = mapResult.IttId,
                SlugId = mapResult.IttSlugId,
                ProgrammeType = mapResult.ProgrammeType?.ToString(),
                ProgrammeStartDate = mapResult.ProgrammeStartDate,
                ProgrammeEndDate = mapResult.ProgrammeEndDate,
                Result = mapResult.IttResult?.ToString(),
                QualificationName = mapResult.IttQualification?.dfeta_name,
                QualificationValue = mapResult.IttQualification?.dfeta_Value,
                ProviderId = mapResult.IttProvider?.Id,
                ProviderName = mapResult.IttProvider?.Name,
                ProviderUkprn = mapResult.IttProvider?.dfeta_UKPRN,
                CountryName = mapResult.IttCountry?.dfeta_name,
                CountryValue = mapResult.IttCountry?.dfeta_Value,
                Subject1Name = mapResult.IttSubject1?.dfeta_name,
                Subject1Value = mapResult.IttSubject1?.dfeta_Value,
                Subject2Name = mapResult.IttSubject2?.dfeta_name,
                Subject2Value = mapResult.IttSubject2?.dfeta_Value,
                Subject3Name = mapResult.IttSubject3?.dfeta_name,
                AgeRangeFrom = mapResult.ProfessionalStatusInfo?.ProfessionalStatus?.DqtAgeRangeFrom,
                AgeRangeTo = mapResult.ProfessionalStatusInfo?.ProfessionalStatus?.DqtAgeRangeTo
            };
        }

        EventModels.DqtQtsRegistration? MapQtsRegistration(IttQtsMapResult mapResult)
        {
            if (mapResult.QtsRegistrationId is null)
            {
                return null;
            }

            return new EventModels.DqtQtsRegistration()
            {
                QtsRegistrationId = mapResult.QtsRegistrationId,
                TeacherStatusName = mapResult.TeacherStatus?.dfeta_name,
                TeacherStatusValue = mapResult.TeacherStatus?.dfeta_Value,
                EarlyYearsStatusName = mapResult.EarlyYearsStatus?.dfeta_name,
                EarlyYearsStatusValue = mapResult.EarlyYearsStatus?.dfeta_Value,
                QtsDate = mapResult.QtsDate,
                EytsDate = mapResult.EytsDate,
                PartialRecognitionDate = mapResult.PartialRecognitionDate
            };
        }

        RouteMigrationReportItem MapReportItem(IttQtsMapResult result)
        {
            return new RouteMigrationReportItem
            {
                RouteMigrationReportItemId = Guid.NewGuid(),
                PersonId = result.ContactId,
                Migrated = result.Success,
                NotMigratedReason = result.FailedReason?.ToString(),
                DqtInitialTeacherTrainingId = result.IttId,
                DqtIttSlugId = result.IttSlugId,
                DqtIttProgrammeType = result.ProgrammeType?.ToString(),
                DqtIttProgrammeStartDate = result.ProgrammeStartDate,
                DqtIttProgrammeEndDate = result.ProgrammeEndDate,
                DqtIttResult = result.IttResult?.ToString(),
                DqtIttQualificationName = result.IttQualification?.dfeta_name,
                DqtIttQualificationValue = result.IttQualification?.dfeta_Value,
                DqtIttProviderId = result.IttProvider?.Id,
                DqtIttProviderName = result.IttProvider?.Name,
                DqtIttProviderUkprn = result.IttProvider?.dfeta_UKPRN,
                DqtIttCountryName = result.IttCountry?.dfeta_name,
                DqtIttCountryValue = result.IttCountry?.dfeta_Value,
                DqtIttSubject1Name = result.IttSubject1?.dfeta_name,
                DqtIttSubject1Value = result.IttSubject1?.dfeta_Value,
                DqtIttSubject2Name = result.IttSubject2?.dfeta_name,
                DqtIttSubject2Value = result.IttSubject2?.dfeta_Value,
                DqtIttSubject3Name = result.IttSubject3?.dfeta_name,
                DqtAgeRangeFrom = result.ProfessionalStatusInfo?.ProfessionalStatus?.DqtAgeRangeFrom,
                DqtAgeRangeTo = result.ProfessionalStatusInfo?.ProfessionalStatus?.DqtAgeRangeTo,
                DqtQtsRegistrationId = result.QtsRegistrationId,
                DqtTeacherStatusName = result.TeacherStatus?.dfeta_name,
                DqtTeacherStatusValue = result.TeacherStatus?.dfeta_Value,
                DqtEarlyYearsStatusName = result.EarlyYearsStatus?.dfeta_name,
                DqtEarlyYearsStatusValue = result.EarlyYearsStatus?.dfeta_Value,
                DqtQtsDate = result.QtsDate,
                DqtEytsDate = result.EytsDate,
                DqtPartialRecognitionDate = result.PartialRecognitionDate,
                DqtQtlsDate = result.QtlsDate,
                DqtQtlsDateHasBeenSet = result.QtlsDateHasBeenSet,
                StatusDerivedRouteToProfessionalStatusTypeId = result.StatusDerivedRoute?.RouteToProfessionalStatusTypeId,
                StatusDerivedRouteToProfessionalStatusTypeName = result.StatusDerivedRoute?.Name,
                ProgrammeTypeDerivedRouteToProfessionalStatusTypeId = result.ProgrammeTypeDerivedRoute?.RouteToProfessionalStatusTypeId,
                ProgrammeTypeDerivedRouteToProfessionalStatusTypeName = result.ProgrammeTypeDerivedRoute?.Name,
                IttQualificationDerivedRouteToProfessionalStatusTypeId = result.IttQualificationDerivedRoute?.RouteToProfessionalStatusTypeId,
                IttQualificationDerivedRouteToProfessionalStatusTypeName = result.IttQualificationDerivedRoute?.Name,
                MultiplePotentialCompatibleIttRecords = result.MultiplePotentialCompatibleIttRecords,
                InductionExemptionReasonIdsMovedFromPerson = result.InductionExemptionReasonIdsMovedFromPerson,
                ContactIttRowCount = result.ContactIttRowCount,
                ContactQtsRowCount = result.ContactQtsRowCount,
                RouteToProfessionalStatusTypeId = result.ProfessionalStatusInfo?.RouteToProfessionalStatusType?.RouteToProfessionalStatusTypeId,
                RouteToProfessionalStatusTypeName = result.ProfessionalStatusInfo?.RouteToProfessionalStatusType?.Name,
                SourceApplicationReference = result.ProfessionalStatusInfo?.ProfessionalStatus?.SourceApplicationReference,
                SourceApplicationUserId = result.ProfessionalStatusInfo?.SourceApplicationUser?.UserId,
                SourceApplicationUserShortName = result.ProfessionalStatusInfo?.SourceApplicationUser?.ShortName,
                Status = result.ProfessionalStatusInfo?.ProfessionalStatus?.Status.ToString(),
                HoldsFrom = result.ProfessionalStatusInfo?.ProfessionalStatus?.HoldsFrom,
                TrainingStartDate = result.ProfessionalStatusInfo?.ProfessionalStatus?.TrainingStartDate,
                TrainingEndDate = result.ProfessionalStatusInfo?.ProfessionalStatus?.TrainingEndDate,
                TrainingSubject1Name = result.ProfessionalStatusInfo?.TrainingSubject1?.Name,
                TrainingSubject1Reference = result.ProfessionalStatusInfo?.TrainingSubject1?.Reference,
                TrainingSubject2Name = result.ProfessionalStatusInfo?.TrainingSubject2?.Name,
                TrainingSubject2Reference = result.ProfessionalStatusInfo?.TrainingSubject2?.Reference,
                TrainingSubject3Name = result.ProfessionalStatusInfo?.TrainingSubject3?.Name,
                TrainingSubject3Reference = result.ProfessionalStatusInfo?.TrainingSubject3?.Reference,
                TrainingAgeSpecialismType = result.ProfessionalStatusInfo?.ProfessionalStatus?.TrainingAgeSpecialismType?.ToString(),
                TrainingAgeSpecialismRangeFrom = result.ProfessionalStatusInfo?.ProfessionalStatus?.TrainingAgeSpecialismRangeFrom,
                TrainingAgeSpecialismRangeTo = result.ProfessionalStatusInfo?.ProfessionalStatus?.TrainingAgeSpecialismRangeTo,
                TrainingCountryName = result.ProfessionalStatusInfo?.TrainingCountry?.Name,
                TrainingCountryId = result.ProfessionalStatusInfo?.TrainingCountry?.CountryId,
                TrainingProviderId = result.ProfessionalStatusInfo?.TrainingProvider?.TrainingProviderId,
                TrainingProviderName = result.ProfessionalStatusInfo?.TrainingProvider?.Name,
                TrainingProviderUkprn = result.ProfessionalStatusInfo?.TrainingProvider?.Ukprn,
                ExemptFromInduction = result.ProfessionalStatusInfo?.ProfessionalStatus?.ExemptFromInduction,
                ExemptFromInductionDueToQtsDate = result.ProfessionalStatusInfo?.ProfessionalStatus?.ExemptFromInductionDueToQtsDate,
                DegreeTypeId = result.ProfessionalStatusInfo?.ProfessionalStatus?.DegreeTypeId,
                DegreeTypeName = result.ProfessionalStatusInfo?.DegreeType?.Name,
                CreatedOn = clock.UtcNow,
            };
        }

        async Task<int> SaveMigrationReportAsync(
            NpgsqlTransaction transaction,
            IReadOnlyCollection<RouteMigrationReportItem> items,
            CancellationToken cancellationToken)
        {
            if (items.Count == 0)
            {
                return 0;
            }

            var columnNames = new[]
            {
                "route_migration_report_item_id",
                "person_id",
                "migrated",
                "not_migrated_reason",
                "dqt_initial_teacher_training_id",
                "dqt_itt_slug_id",
                "dqt_itt_programme_type",
                "dqt_itt_programme_start_date",
                "dqt_itt_programme_end_date",
                "dqt_itt_result",
                "dqt_itt_qualification_name",
                "dqt_itt_qualification_value",
                "dqt_itt_provider_id",
                "dqt_itt_provider_name",
                "dqt_itt_provider_ukprn",
                "dqt_itt_country_name",
                "dqt_itt_country_value",
                "dqt_itt_subject1_name",
                "dqt_itt_subject1_value",
                "dqt_itt_subject2_name",
                "dqt_itt_subject2_value",
                "dqt_itt_subject3_name",
                "dqt_itt_subject3_value",
                "dqt_age_range_from",
                "dqt_age_range_to",
                "dqt_qts_registration_id",
                "dqt_teacher_status_name",
                "dqt_teacher_status_value",
                "dqt_early_years_status_name",
                "dqt_early_years_status_value",
                "dqt_qts_date",
                "dqt_eyts_date",
                "dqt_partial_recognition_date",
                "dqt_qtls_date",
                "dqt_qtls_date_has_been_set",
                "status_derived_route_to_professional_status_type_id",
                "status_derived_route_to_professional_status_type_name",
                "programme_type_derived_route_to_professional_status_type_id",
                "programme_type_derived_route_to_professional_status_type_name",
                "itt_qualification_derived_route_to_professional_status_type_id",
                "itt_qualification_derived_route_to_professional_status_type_name",
                "multiple_potential_compatible_itt_records",
                "induction_exemption_reason_ids_moved_from_person",
                "contact_itt_row_count",
                "contact_qts_row_count",
                "route_to_professional_status_type_id",
                "route_to_professional_status_type_name",
                "source_application_reference",
                "source_application_user_id",
                "source_application_user_short_name",
                "status",
                "holds_from",
                "training_start_date",
                "training_end_date",
                "training_subject1_name",
                "training_subject1_reference",
                "training_subject2_name",
                "training_subject2_reference",
                "training_subject3_name",
                "training_subject3_reference",
                "training_age_specialism_type",
                "training_age_specialism_range_from",
                "training_age_specialism_range_to",
                "training_country_name",
                "training_country_id",
                "training_provider_id",
                "training_provider_name",
                "training_provider_ukprn",
                "exempt_from_induction",
                "exempt_from_induction_due_to_qts_date",
                "degree_type_id",
                "degree_type_name",
                "created_on"
            };

            var columnList = string.Join(", ", columnNames);
            var copyStatement = $"COPY route_migration_report_items ({columnList}) FROM STDIN (FORMAT BINARY)";

            using var writer = await transaction.Connection!.BeginBinaryImportAsync(copyStatement, cancellationToken);

            foreach (var item in items)
            {
                writer.StartRow();
                writer.WriteValueOrNull(item.RouteMigrationReportItemId, NpgsqlDbType.Uuid);
                writer.WriteValueOrNull(item.PersonId, NpgsqlDbType.Uuid);
                writer.WriteValueOrNull(item.Migrated, NpgsqlDbType.Boolean);
                writer.WriteValueOrNull(item.NotMigratedReason, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtInitialTeacherTrainingId, NpgsqlDbType.Uuid);
                writer.WriteValueOrNull(item.DqtIttSlugId, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtIttProgrammeType, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtIttProgrammeStartDate, NpgsqlDbType.Date);
                writer.WriteValueOrNull(item.DqtIttProgrammeEndDate, NpgsqlDbType.Date);
                writer.WriteValueOrNull(item.DqtIttResult, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtIttQualificationName, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtIttQualificationValue, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtIttProviderId, NpgsqlDbType.Uuid);
                writer.WriteValueOrNull(item.DqtIttProviderName, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtIttProviderUkprn, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtIttCountryName, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtIttCountryValue, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtIttSubject1Name, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtIttSubject1Value, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtIttSubject2Name, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtIttSubject2Value, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtIttSubject3Name, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtIttSubject3Value, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtAgeRangeFrom, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtAgeRangeTo, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtQtsRegistrationId, NpgsqlDbType.Uuid);
                writer.WriteValueOrNull(item.DqtTeacherStatusName, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtTeacherStatusValue, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtEarlyYearsStatusName, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtEarlyYearsStatusValue, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.DqtQtsDate, NpgsqlDbType.Date);
                writer.WriteValueOrNull(item.DqtEytsDate, NpgsqlDbType.Date);
                writer.WriteValueOrNull(item.DqtPartialRecognitionDate, NpgsqlDbType.Date);
                writer.WriteValueOrNull(item.DqtQtlsDate, NpgsqlDbType.Date);
                writer.WriteValueOrNull(item.DqtQtlsDateHasBeenSet, NpgsqlDbType.Boolean);
                writer.WriteValueOrNull(item.StatusDerivedRouteToProfessionalStatusTypeId, NpgsqlDbType.Uuid);
                writer.WriteValueOrNull(item.StatusDerivedRouteToProfessionalStatusTypeName, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.ProgrammeTypeDerivedRouteToProfessionalStatusTypeId, NpgsqlDbType.Uuid);
                writer.WriteValueOrNull(item.ProgrammeTypeDerivedRouteToProfessionalStatusTypeName, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.IttQualificationDerivedRouteToProfessionalStatusTypeId, NpgsqlDbType.Uuid);
                writer.WriteValueOrNull(item.IttQualificationDerivedRouteToProfessionalStatusTypeName, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.MultiplePotentialCompatibleIttRecords, NpgsqlDbType.Array | NpgsqlDbType.Uuid);
                writer.WriteValueOrNull(item.InductionExemptionReasonIdsMovedFromPerson, NpgsqlDbType.Array | NpgsqlDbType.Uuid);
                writer.WriteValueOrNull(item.ContactIttRowCount, NpgsqlDbType.Integer);
                writer.WriteValueOrNull(item.ContactQtsRowCount, NpgsqlDbType.Integer);
                writer.WriteValueOrNull(item.RouteToProfessionalStatusTypeId, NpgsqlDbType.Uuid);
                writer.WriteValueOrNull(item.RouteToProfessionalStatusTypeName, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.SourceApplicationReference, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.SourceApplicationUserId, NpgsqlDbType.Uuid);
                writer.WriteValueOrNull(item.SourceApplicationUserShortName, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.Status, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.HoldsFrom, NpgsqlDbType.Date);
                writer.WriteValueOrNull(item.TrainingStartDate, NpgsqlDbType.Date);
                writer.WriteValueOrNull(item.TrainingEndDate, NpgsqlDbType.Date);
                writer.WriteValueOrNull(item.TrainingSubject1Name, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.TrainingSubject1Reference, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.TrainingSubject2Name, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.TrainingSubject2Reference, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.TrainingSubject3Name, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.TrainingSubject3Reference, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.TrainingAgeSpecialismType, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.TrainingAgeSpecialismRangeFrom, NpgsqlDbType.Integer);
                writer.WriteValueOrNull(item.TrainingAgeSpecialismRangeTo, NpgsqlDbType.Integer);
                writer.WriteValueOrNull(item.TrainingCountryName, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.TrainingCountryId, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.TrainingProviderId, NpgsqlDbType.Uuid);
                writer.WriteValueOrNull(item.TrainingProviderName, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.TrainingProviderUkprn, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.ExemptFromInduction, NpgsqlDbType.Boolean);
                writer.WriteValueOrNull(item.ExemptFromInductionDueToQtsDate, NpgsqlDbType.Boolean);
                writer.WriteValueOrNull(item.DegreeTypeId, NpgsqlDbType.Uuid);
                writer.WriteValueOrNull(item.DegreeTypeName, NpgsqlDbType.Varchar);
                writer.WriteValueOrNull(item.CreatedOn, NpgsqlDbType.TimestampTz);
            }

            await writer.CompleteAsync(cancellationToken);
            await writer.CloseAsync(cancellationToken);

            return items.Count;
        }
    }

    private EntityVersionInfo<TEntity>[] GetEntityVersions<TEntity>(TEntity latest, IEnumerable<AuditDetail> auditDetails, string[] attributeNames)
        where TEntity : Entity
    {
        if (!latest.TryGetAttributeValue<DateTime?>("createdon", out var createdOn) || !createdOn.HasValue)
        {
            throw new ArgumentException($"Expected {latest.LogicalName} entity with ID {latest.Id} to have a non-null 'createdon' attribute value.", nameof(latest));
        }

        var created = createdOn.Value;
        var createdBy = latest.GetAttributeValue<EntityReference>("createdby");

        var ordered = auditDetails
            .OfType<AttributeAuditDetail>()
            .Select(a => (AuditDetail: a, AuditRecord: a.AuditRecord.ToEntity<Audit>()))
            .OrderBy(a => a.AuditRecord.CreatedOn)
            .ThenBy(a => a.AuditRecord.Action == Audit_Action.Create ? 0 : 1)
            .ToArray();

        if (ordered.Length == 0)
        {
            return [new EntityVersionInfo<TEntity>(latest.Id, latest, ChangedAttributes: [], created, createdBy.Id, createdBy.Name)];
        }

        var versions = new List<EntityVersionInfo<TEntity>>();

        var initialVersion = GetInitialVersion();
        versions.Add(new EntityVersionInfo<TEntity>(initialVersion.Id, initialVersion, ChangedAttributes: [], created, createdBy.Id, createdBy.Name));

        latest = initialVersion.ShallowClone();
        foreach (var audit in ordered)
        {
            if (audit.AuditRecord.Action == Audit_Action.Create)
            {
                if (audit != ordered[0])
                {
                    throw new InvalidOperationException($"Expected the {Audit_Action.Create} audit to be first.");
                }

                continue;
            }

            var thisVersion = latest.ShallowClone();
            var changedAttributes = new List<string>();

            foreach (var attr in audit.AuditDetail.DeletedAttributes)
            {
                if (!attributeNames.Contains(attr.Value))
                {
                    continue;
                }

                thisVersion.Attributes.Remove(attr.Value);
                changedAttributes.Add(attr.Value);
            }

            foreach (var attr in audit.AuditDetail.NewValue.Attributes)
            {
                if (!attributeNames.Contains(attr.Key))
                {
                    continue;
                }

                thisVersion.Attributes[attr.Key] = attr.Value;
                changedAttributes.Add(attr.Key);
            }

            if (changedAttributes.Count == 0)
            {
                continue;
            }

            versions.Add(new EntityVersionInfo<TEntity>(
                audit.AuditRecord.Id,
                thisVersion.SparseClone(attributeNames),
                changedAttributes.ToArray(),
                audit.AuditRecord.CreatedOn!.Value,
                audit.AuditRecord.UserId.Id,
                audit.AuditRecord.UserId.Name));

            latest = thisVersion;
        }

        return versions.ToArray();

        TEntity GetInitialVersion()
        {
            TEntity? initial;
            if (ordered[0] is { AuditRecord: { Action: Audit_Action.Create } } createAction)
            {
                initial = createAction.AuditDetail.NewValue.ToEntity<TEntity>();
                initial.Id = latest.Id;
            }
            else
            {
                // Starting with `latest`, go through each event in reverse and undo the changes it applied.
                // When we're done we end up with the initial version of the record.
                initial = latest.ShallowClone();

                foreach (var a in ordered.Reverse())
                {
                    // Check that new attributes align with what we have in `initial`;
                    // if they don't, then we've got an incomplete history
                    foreach (var attr in a.AuditDetail.NewValue.Attributes.Where(kvp => attributeNames.Contains(kvp.Key)))
                    {
                        if (!AttributeValuesEqual(attr.Value, initial.Attributes.TryGetValue(attr.Key, out var initialAttr) ? initialAttr : null))
                        {
                            throw new Exception($"Non-contiguous audit records for {initial.LogicalName} '{initial.Id}':\n" +
                                $"Expected '{attr.Key}' to be '{attr.Value ?? "<null>"}' but was '{initialAttr ?? "<null>"}'.");
                        }

                        if (!a.AuditDetail.OldValue.Attributes.Contains(attr.Key))
                        {
                            initial.Attributes.Remove(attr.Key);
                        }
                    }

                    foreach (var attr in a.AuditDetail.OldValue.Attributes.Where(kvp => attributeNames.Contains(kvp.Key)))
                    {
                        initial.Attributes[attr.Key] = attr.Value;
                    }
                }
            }

            return initial.SparseClone(attributeNames);
        }

        static bool AttributeValuesEqual(object? first, object? second)
        {
            if (first is null && second is null)
            {
                return true;
            }

            if (first is null || second is null)
            {
                return false;
            }

            if (first.GetType() != second.GetType())
            {
                return false;
            }

            return first is EntityReference firstRef && second is EntityReference secondRef ?
                firstRef.Name == secondRef.Name && firstRef.Id == secondRef.Id :
                first.Equals(second);
        }
    }

    private ModelTypeSyncInfo<TModel> GetModelTypeSyncInfo<TModel>(string modelType) =>
        (ModelTypeSyncInfo<TModel>)GetModelTypeSyncInfo(modelType);

    private ModelTypeSyncInfo GetModelTypeSyncInfo(string modelType) =>
        AllModelTypeSyncInfo.TryGetValue(modelType, out var modelTypeSyncInfo) ?
            modelTypeSyncInfo :
            throw new ArgumentException($"Unknown data type: '{modelType}.", nameof(modelType));

    private async Task<IReadOnlyDictionary<Guid, AuditDetailCollection>> GetAuditRecordsAsync(
        string entityLogicalName,
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken)
    {
        if (IsFakeXrm)
        {
            return new Dictionary<Guid, AuditDetailCollection>();
        }

        // Throttle the amount of concurrent requests
        using var requestThrottle = new SemaphoreSlim(20, 20);

        // Keep track of the last seen 'retry-after' value
        var retryDelayUpdateLock = new object();
        var retryDelay = Task.Delay(0, cancellationToken);

        void UpdateRetryDelay(TimeSpan ts)
        {
            lock (retryDelayUpdateLock)
            {
                retryDelay = Task.Delay(ts, cancellationToken);
            }
        }

        return (await Task.WhenAll(ids
            .Chunk(MaxAuditRequestsPerBatch)
            .Select(async chunk =>
            {
                var request = new ExecuteMultipleRequest()
                {
                    Requests = new(),
                    Settings = new()
                    {
                        ContinueOnError = false,
                        ReturnResponses = true
                    }
                };

                // The following is not supported by FakeXrmEasy hence the check above to allow more test coverage
                request.Requests.AddRange(chunk.Select(e => new RetrieveRecordChangeHistoryRequest() { Target = e.ToEntityReference(entityLogicalName) }));

                ExecuteMultipleResponse response;
                while (true)
                {
                    await retryDelay;
                    await requestThrottle.WaitAsync(cancellationToken);
                    try
                    {
                        response = (ExecuteMultipleResponse)await organizationService.ExecuteAsync(request, cancellationToken);
                    }
                    catch (FaultException fex) when (fex.IsCrmRateLimitException(out var retryAfter))
                    {
                        logger.LogWarning("Hit CRM service limits getting {entityLogicalName} audit records; Fault exception. Retrying after {retryAfter} seconds.", entityLogicalName, retryAfter.TotalSeconds);
                        UpdateRetryDelay(retryAfter);
                        continue;
                    }
                    finally
                    {
                        requestThrottle.Release();
                    }

                    if (response.IsFaulted)
                    {
                        var firstFault = response.Responses.First(r => r.Fault is not null).Fault;

                        if (firstFault.IsCrmRateLimitFault(out var retryAfter))
                        {
                            logger.LogWarning("Hit CRM service limits getting {entityLogicalName} audit records; CRM rate limit fault. Retrying after {retryAfter} seconds.", entityLogicalName, retryAfter.TotalSeconds);
                            UpdateRetryDelay(retryAfter);
                            continue;
                        }
                        else if (firstFault.Message.Contains("The HTTP status code of the response was not expected (429)"))
                        {
                            logger.LogWarning("Hit CRM service limits getting {entityLogicalName} audit records; 429 too many requests", entityLogicalName);
                            UpdateRetryDelay(TimeSpan.FromMinutes(2));
                            continue;
                        }

                        throw new FaultException<OrganizationServiceFault>(firstFault, new FaultReason(firstFault.Message));
                    }

                    break;
                }

                return response.Responses.Zip(
                    chunk,
                    (r, e) => (Id: e, ((RetrieveRecordChangeHistoryResponse)r.Response).AuditDetailCollection));
            })))
            .SelectMany(b => b)
            .ToDictionary(t => t.Id, t => t.AuditDetailCollection);
    }

    private async Task<IReadOnlyDictionary<Guid, AuditDetailCollection>> GetAuditRecordsFromAuditRepositoryAsync(
        string entityLogicalName,
        string primaryIdAttribute,
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken)
    {
        var auditRecords = await Task.WhenAll(
            ids.Select(async id =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                int retryCount = 0;
                AuditDetailCollection? audit = null;
                while (true)
                {
                    audit = await auditRepository.GetAuditDetailAsync(entityLogicalName, primaryIdAttribute, id);
                    if (audit is not null || retryCount > 0)
                    {
                        break;
                    }

                    // Try and sync the audit record from CRM if it doesn't exist in the audit repository
                    await SyncAuditAsync(entityLogicalName, new[] { id }, skipIfExists: false, cancellationToken);
                    retryCount++;
                }

                if (audit is null)
                {
                    throw new Exception($"No audit detail for {entityLogicalName} with id '{id}'.");
                }

                return (Id: id, Audit: audit);
            }));

        return auditRecords.ToDictionary(a => a.Id, a => a.Audit);
    }

    private async Task<TEntity[]> GetEntitiesAsync<TEntity>(
        string entityLogicalName,
        string idAttributeName,
        IEnumerable<Guid> ids,
        string[] attributeNames,
        bool activeOnly,
        CancellationToken cancellationToken)
        where TEntity : Entity
    {
        var query = new QueryExpression(entityLogicalName);
        query.ColumnSet = new(attributeNames);
        query.Criteria.AddCondition(idAttributeName, ConditionOperator.In, ids.Cast<object>().ToArray());
        if (activeOnly)
        {
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
        }

        EntityCollection response;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                response = await organizationService.RetrieveMultipleAsync(query, cancellationToken);
            }
            catch (FaultException<OrganizationServiceFault> fex) when (fex.IsCrmRateLimitException(out var retryAfter))
            {
                logger.LogWarning("Hit CRM service limits; error code: {ErrorCode}", fex.Detail.ErrorCode);
                await Task.Delay(retryAfter, cancellationToken);
                continue;
            }

            break;
        }

        return response.Entities.Select(e => e.ToEntity<TEntity>()).ToArray();
    }

    private static ModelTypeSyncInfo GetModelTypeSyncInfoForNotes()
    {
        var tempTableName = "temp_notes_import";
        var tableName = "notes";

        var columnNames = new[]
        {
            "note_id",
            "person_id",
            "content_html",
            "created_on",
            "created_by_dqt_user_id",
            "file_name",
            "original_file_name",
            "created_by_dqt_user_name",
            "updated_by_dqt_user_id",
            "updated_by_dqt_user_name",
            "updated_on"
        };

        var columnsToUpdate = new[] {
            "content_html",
            "file_name",
            "original_file_name",
            "updated_on",
            "updated_by_dqt_user_id",
            "updated_by_dqt_user_name"
        };

        var columnList = string.Join(", ", columnNames);

        var createTempTableStatement =
            $"""
            CREATE TEMP TABLE {tempTableName}
            (
                note_id uuid NOT NULL,
                person_id uuid NOT NULL,
                content_html TEXT NULL,
                created_on timestamp with time zone,
                created_by_dqt_user_id uuid NOT NULL,
                file_name TEXT NULL,
                original_file_name TEXT NULL,
                created_by_dqt_user_name TEXT,
                updated_by_dqt_user_name text NULL,
                updated_by_dqt_user_id uuid NULL,
                updated_on timestamp with time zone
            )
            """;

        var copyStatement = $"COPY {tempTableName} ({columnList}) FROM STDIN (FORMAT BINARY)";

        var insertStatement =
            $"""
            INSERT INTO {tableName} ({columnList})
            SELECT {columnList} FROM {tempTableName}
            ON CONFLICT (note_id) DO UPDATE
            SET {string.Join(", ", columnsToUpdate.Select(c => $"{c} = EXCLUDED.{c}"))}
            WHERE {tableName}.updated_on IS NULL OR {tableName}.updated_on < EXCLUDED.updated_on;
            """;

        //artitrary date of now-1 day because there are no rows in dqt_note
        var getLastModifiedOnStatement = $"SELECT COALESCE(MAX(updated_on), NOW() - INTERVAL '1 day') FROM {tableName}";

        var deleteStatement = $"DELETE FROM {tableName} WHERE note_id = ANY({IdsParameterName})";

        var attributeNames = new[]
        {
            Annotation.PrimaryIdAttribute,
            Annotation.Fields.FileName,
            Annotation.Fields.DocumentBody,
            Annotation.Fields.ObjectId,
            Annotation.Fields.ModifiedOn,
            Annotation.Fields.NoteText,
            Annotation.Fields.CreatedBy,
            Annotation.Fields.CreatedOn,
            Annotation.Fields.ModifiedBy,
            Annotation.Fields.MimeType,
        };

        Action<NpgsqlBinaryImporter, DqtNoteInfo> writeRecord = (writer, note) =>
        {
            writer.WriteValueOrNull(note.Id, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(note.PersonId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(note.ContentHtml, NpgsqlDbType.Text);
            writer.WriteValueOrNull(note.CreatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(note.CreatedByDqtUserId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(note.FileName, NpgsqlDbType.Text);
            writer.WriteValueOrNull(note.OriginalFileName, NpgsqlDbType.Text);
            writer.WriteValueOrNull(note.CreatedByDqtUserName, NpgsqlDbType.Text);
            writer.WriteValueOrNull(note.UpdatedByDqtUserId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(note.UpdatedByDqtUserName, NpgsqlDbType.Text);
            writer.WriteValueOrNull(note.UpdatedOn ?? note.CreatedOn, NpgsqlDbType.TimestampTz); //default updated on to when it was created
        };

        return new ModelTypeSyncInfo<DqtNoteInfo>()
        {
            CreateTempTableStatement = createTempTableStatement,
            CopyStatement = copyStatement,
            UpsertStatement = insertStatement,
            DeleteStatement = deleteStatement,
            IgnoreDeletions = false,
            GetLastModifiedOnStatement = getLastModifiedOnStatement,
            EntityLogicalName = Annotation.EntityLogicalName,
            AttributeNames = attributeNames,
            GetSyncHandler = helper => (entities, ignoreInvalid, dryRun, ct) =>
                helper.SyncAnnotationsAsync(entities.Select(e => e.ToEntity<Annotation>()).ToArray(), ignoreInvalid, dryRun, ct),
            WriteRecord = writeRecord
        };
    }

    private ModelTypeSyncInfo GetModelTypeSyncInfoForPerson()
    {
        var tempTableName = "temp_person_import";
        var tableName = "persons";

        var columnNames = new[]
        {
            "person_id",
            "created_on",
            "updated_on",
            "status",
            "merged_with_person_id",
            "trn",
            "first_name",
            "middle_name",
            "last_name",
            "date_of_birth",
            "email_address",
            "national_insurance_number",
            "gender",
            "dqt_contact_id",
            "dqt_state",
            "dqt_created_on",
            "dqt_modified_on",
            "dqt_first_name",
            "dqt_middle_name",
            "dqt_last_name",
            "created_by_tps"
        };

        var columnsToUpdate = columnNames.Except(new[] { "person_id", "dqt_contact_id" }).ToArray();

        var columnList = string.Join(", ", columnNames);

        var createTempTableStatement = $"CREATE TEMP TABLE {tempTableName} (LIKE {tableName} INCLUDING DEFAULTS)";

        var copyStatement = $"COPY {tempTableName} ({columnList}) FROM STDIN (FORMAT BINARY)";

        var insertStatement =
            $"""
            INSERT INTO {tableName} AS t ({columnList}, dqt_first_sync, dqt_last_sync)
            SELECT {columnList}, {NowParameterName}, {NowParameterName} FROM {tempTableName}
            ON CONFLICT (person_id) DO UPDATE
            SET dqt_last_sync = {NowParameterName}, {string.Join(", ", columnsToUpdate.Select(c => $"{c} = EXCLUDED.{c}"))}
            """;

        var deleteStatement = $"DELETE FROM {tableName} WHERE dqt_contact_id = ANY({IdsParameterName})";

        var getLastModifiedOnStatement = $"SELECT MAX(dqt_modified_on) FROM {tableName}";

        var attributeNames = new[]
        {
            Contact.PrimaryIdAttribute,
            Contact.Fields.StateCode,
            Contact.Fields.CreatedOn,
            Contact.Fields.CreatedBy,
            Contact.Fields.ModifiedOn,
            Contact.Fields.dfeta_TRN,
            Contact.Fields.FirstName,
            Contact.Fields.MiddleName,
            Contact.Fields.LastName,
            Contact.Fields.dfeta_StatedFirstName,
            Contact.Fields.dfeta_StatedMiddleName,
            Contact.Fields.dfeta_StatedLastName,
            Contact.Fields.BirthDate,
            Contact.Fields.EMailAddress1,
            Contact.Fields.dfeta_NINumber,
            Contact.Fields.MobilePhone,
            Contact.Fields.GenderCode,
            Contact.Fields.dfeta_InductionStatus,
            Contact.Fields.dfeta_MergedWith,
            Contact.Fields.dfeta_CapitaTRNChangedOn
        };

        Action<NpgsqlBinaryImporter, PersonInfo> writeRecord = (writer, person) =>
        {
            writer.WriteValueOrNull(person.PersonId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(person.CreatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(person.UpdatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull((int)person.Status, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(person.MergedWithPersonId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(person.Trn, NpgsqlDbType.Char);
            writer.WriteValueOrNull(person.FirstName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.MiddleName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.LastName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.DateOfBirth, NpgsqlDbType.Date);
            writer.WriteValueOrNull(person.EmailAddress, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.NationalInsuranceNumber, NpgsqlDbType.Char);
            writer.WriteValueOrNull((int?)person.Gender, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(person.DqtContactId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(person.DqtState, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(person.DqtCreatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(person.DqtModifiedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(person.DqtFirstName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.DqtMiddleName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.DqtLastName, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(person.CreatedByTps, NpgsqlDbType.Boolean);
        };

        return new ModelTypeSyncInfo<PersonInfo>()
        {
            CreateTempTableStatement = createTempTableStatement,
            CopyStatement = copyStatement,
            UpsertStatement = insertStatement,
            DeleteStatement = deleteStatement,
            IgnoreDeletions = false,
            GetLastModifiedOnStatement = getLastModifiedOnStatement,
            EntityLogicalName = Contact.EntityLogicalName,
            AttributeNames = attributeNames,
            GetSyncHandler = helper => (entities, ignoreInvalid, dryRun, ct) =>
                helper.SyncPersonsAsync(entities.Select(e => e.ToEntity<Contact>()).ToArray(), syncAudit: true, ignoreInvalid, dryRun, ct),
            WriteRecord = writeRecord
        };
    }

    private static ModelTypeSyncInfo GetModelTypeSyncInfoForInduction()
    {
        var tempTableName = "temp_induction_import";
        var tableName = "persons";

        var columnNames = new[]
        {
            "person_id",
            "induction_completed_date",
            "induction_exemption_reason_ids",
            "induction_start_date",
            "induction_status",
            "induction_modified_on",
            "dqt_induction_modified_on",
            "induction_exempt_without_reason"
        };

        var columnsToUpdate = columnNames.Except(new[] { "person_id" }).ToArray();

        var columnList = string.Join(", ", columnNames);

        var createTempTableStatement =
            $"""
            CREATE TEMP TABLE {tempTableName}
            (
                person_id uuid NOT NULL,
                induction_completed_date date,
                induction_exemption_reason_ids uuid[],
                induction_start_date date,
                induction_status integer,
                induction_modified_on timestamp with time zone,
                dqt_induction_modified_on timestamp with time zone,
                induction_exempt_without_reason boolean
            )
            """;

        var copyStatement = $"COPY {tempTableName} ({columnList}) FROM STDIN (FORMAT BINARY)";

        var updateStatement =
            $"""
            UPDATE {tableName} AS t
            SET dqt_induction_last_sync = {NowParameterName}, {string.Join(", ", columnsToUpdate.Select(c => $"{c} = temp.{c}"))}
            FROM {tempTableName} AS temp
            WHERE t.person_id = temp.person_id
            RETURNING t.person_id
            """;

        var getLastModifiedOnStatement = $"SELECT MAX(dqt_induction_modified_on) FROM {tableName}";

        var attributeNames = new[]
        {
            dfeta_induction.PrimaryIdAttribute,
            dfeta_induction.Fields.dfeta_PersonId,
            dfeta_induction.Fields.dfeta_CompletionDate,
            dfeta_induction.Fields.dfeta_InductionExemptionReason,
            dfeta_induction.Fields.dfeta_StartDate,
            dfeta_induction.Fields.dfeta_InductionStatus,
            dfeta_induction.Fields.CreatedOn,
            dfeta_induction.Fields.CreatedBy,
            dfeta_induction.Fields.ModifiedOn,
            dfeta_induction.Fields.StateCode
        };

        Action<NpgsqlBinaryImporter, InductionInfo> writeRecord = (writer, induction) =>
        {
            writer.WriteValueOrNull(induction.PersonId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(induction.InductionCompletedDate, NpgsqlDbType.Date);
            writer.WriteValueOrNull(induction.InductionExemptionReasonIds, NpgsqlDbType.Array | NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(induction.InductionStartDate, NpgsqlDbType.Date);
            writer.WriteValueOrNull((int?)induction.InductionStatus, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(induction.DqtModifiedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(induction.DqtModifiedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(induction.InductionExemptWithoutReason, NpgsqlDbType.Boolean);
        };

        return new ModelTypeSyncInfo<InductionInfo>()
        {
            CreateTempTableStatement = createTempTableStatement,
            CopyStatement = copyStatement,
            UpsertStatement = updateStatement,
            DeleteStatement = null,
            IgnoreDeletions = true,
            GetLastModifiedOnStatement = getLastModifiedOnStatement,
            EntityLogicalName = dfeta_induction.EntityLogicalName,
            AttributeNames = attributeNames,
            GetSyncHandler = helper => (entities, ignoreInvalid, dryRun, ct) =>
                helper.SyncInductionsAsync(entities.Select(e => e.ToEntity<dfeta_induction>()).ToArray(), syncAudit: true, ignoreInvalid, dryRun, ct),
            WriteRecord = writeRecord
        };
    }

    private static ModelTypeSyncInfo GetModelTypeSyncInfoForRoute()
    {
        var tempTableName = "temp_route_import";
        var tableName = "qualifications";

        var columnNames = new[]
        {
            "qualification_id",
            "created_on",
            "updated_on",
            "qualification_type",
            "person_id",
            "training_age_specialism_type",
            "training_age_specialism_range_from",
            "training_age_specialism_range_to",
            "training_start_date",
            "training_country_id",
            "dqt_early_years_status_name",
            "dqt_early_years_status_value",
            "dqt_initial_teacher_training_id",
            "dqt_qts_registration_id",
            "dqt_teacher_status_name",
            "dqt_teacher_status_value",
            "route_to_professional_status_type_id",
            "status",
            "training_provider_id",
            "training_subject_ids",
            "source_application_reference",
            "source_application_user_id",
            "holds_from",
            "training_end_date",
            "degree_type_id",
            "exempt_from_induction",
            "exempt_from_induction_due_to_qts_date",
            "dqt_age_range_from",
            "dqt_age_range_to"
        };

        var columnList = string.Join(", ", columnNames);

        var createTempTableStatement = $"CREATE TEMP TABLE {tempTableName} (LIKE {tableName} INCLUDING DEFAULTS)";

        var copyStatement = $"COPY {tempTableName} ({columnList}) FROM STDIN (FORMAT BINARY)";

        var insertStatement =
            $"""
            INSERT INTO {tableName} AS t ({columnList})
            SELECT {columnList} FROM {tempTableName}
            """;

        Action<NpgsqlBinaryImporter, RouteToProfessionalStatus> writeRecord = (writer, route) =>
        {
            writer.WriteValueOrNull(route.QualificationId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(route.CreatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(route.UpdatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull((int)route.QualificationType, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(route.PersonId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull((int?)route.TrainingAgeSpecialismType, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(route.TrainingAgeSpecialismRangeFrom, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(route.TrainingAgeSpecialismRangeTo, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(route.TrainingStartDate, NpgsqlDbType.Date);
            writer.WriteValueOrNull(route.TrainingCountryId, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(route.DqtEarlyYearsStatusName, NpgsqlDbType.Text);
            writer.WriteValueOrNull(route.DqtEarlyYearsStatusValue, NpgsqlDbType.Text);
            writer.WriteValueOrNull(route.DqtInitialTeacherTrainingId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(route.DqtQtsRegistrationId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(route.DqtTeacherStatusName, NpgsqlDbType.Text);
            writer.WriteValueOrNull(route.DqtTeacherStatusValue, NpgsqlDbType.Text);
            writer.WriteValueOrNull(route.RouteToProfessionalStatusTypeId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull((int)route.Status, NpgsqlDbType.Integer);
            writer.WriteValueOrNull(route.TrainingProviderId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(route.TrainingSubjectIds, NpgsqlDbType.Array | NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(route.SourceApplicationReference, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(route.SourceApplicationUserId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(route.HoldsFrom, NpgsqlDbType.Date);
            writer.WriteValueOrNull(route.TrainingEndDate, NpgsqlDbType.Date);
            writer.WriteValueOrNull(route.DegreeTypeId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(route.ExemptFromInduction, NpgsqlDbType.Boolean);
            writer.WriteValueOrNull(route.ExemptFromInductionDueToQtsDate, NpgsqlDbType.Boolean);
            writer.WriteValueOrNull(route.DqtAgeRangeFrom, NpgsqlDbType.Varchar);
            writer.WriteValueOrNull(route.DqtAgeRangeTo, NpgsqlDbType.Varchar);
        };

        return new ModelTypeSyncInfo<RouteToProfessionalStatus>()
        {
            CreateTempTableStatement = createTempTableStatement,
            CopyStatement = copyStatement,
            UpsertStatement = insertStatement,
            DeleteStatement = null,
            IgnoreDeletions = false,
            GetLastModifiedOnStatement = null,
            EntityLogicalName = string.Empty,
            AttributeNames = [],
            GetSyncHandler = helper => (entities, ignoreInvalid, dryRun, ct) => Task.CompletedTask,
            WriteRecord = writeRecord
        };
    }

    private static ModelTypeSyncInfo GetModelTypeSyncInfoForPreviousName()
    {
        var tempTableName = "temp_previous_name_import";
        var tableName = "previous_names";

        var columnNames = new[]
        {
            "previous_name_id",
            "created_on",
            "updated_on",
            "person_id",
            "first_name",
            "middle_name",
            "last_name",
            "dqt_audit_id",
            "dqt_previous_name_ids"
        };

        var columnList = string.Join(", ", columnNames);

        var createTempTableStatement = $"CREATE TEMP TABLE {tempTableName} (LIKE {tableName} INCLUDING DEFAULTS)";

        var copyStatement = $"COPY {tempTableName} ({columnList}) FROM STDIN (FORMAT BINARY)";

        var insertStatement =
            $"""
            MERGE INTO previous_names AS target
            USING (SELECT * FROM {tempTableName}) AS source
            ON target.person_id = source.person_id AND target.dqt_audit_id IS NOT DISTINCT FROM source.dqt_audit_id AND target.dqt_previous_name_ids IS NOT DISTINCT FROM source.dqt_previous_name_ids
            WHEN NOT MATCHED THEN
                INSERT (previous_name_id, created_on, updated_on, person_id, first_name, middle_name, last_name, dqt_audit_id, dqt_previous_name_ids)
                VALUES (source.previous_name_id, source.created_on, source.updated_on, source.person_id, source.first_name, source.middle_name, source.last_name, source.dqt_audit_id, source.dqt_previous_name_ids)
            WHEN MATCHED THEN
                UPDATE SET
                    created_on = source.created_on,
                    updated_on = source.updated_on,
                    first_name = source.first_name,
                    middle_name = source.middle_name,
                    last_name = source.last_name,
                    dqt_audit_id = source.dqt_audit_id,
                    dqt_previous_name_ids = source.dqt_previous_name_ids
            WHEN NOT MATCHED BY SOURCE AND EXISTS (SELECT person_id FROM {tempTableName} WHERE person_id = target.person_id) THEN
                DELETE;
            """;

        Action<NpgsqlBinaryImporter, PreviousName> writeRecord = (writer, previousName) =>
        {
            writer.WriteValueOrNull(previousName.PreviousNameId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(previousName.CreatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(previousName.UpdatedOn, NpgsqlDbType.TimestampTz);
            writer.WriteValueOrNull(previousName.PersonId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(previousName.FirstName, NpgsqlDbType.Text);
            writer.WriteValueOrNull(previousName.MiddleName, NpgsqlDbType.Text);
            writer.WriteValueOrNull(previousName.LastName, NpgsqlDbType.Text);
            writer.WriteValueOrNull(previousName.DqtAuditId, NpgsqlDbType.Uuid);
            writer.WriteValueOrNull(previousName.DqtPreviousNameIds, NpgsqlDbType.Array | NpgsqlDbType.Uuid);
        };

        var attributeNames = new[]
        {
            dfeta_previousname.Fields.dfeta_previousnameId,
            dfeta_previousname.Fields.dfeta_PersonId,
            dfeta_previousname.Fields.CreatedOn,
            "createdby",
            dfeta_previousname.Fields.StateCode,
            dfeta_previousname.Fields.dfeta_Type,
            dfeta_previousname.Fields.dfeta_name,
            dfeta_previousname.Fields.dfeta_ChangedOn
        };

        return new ModelTypeSyncInfo<PreviousName>()
        {
            CreateTempTableStatement = createTempTableStatement,
            CopyStatement = copyStatement,
            UpsertStatement = insertStatement,
            DeleteStatement = null,
            IgnoreDeletions = false,
            GetLastModifiedOnStatement = null,
            EntityLogicalName = dfeta_previousname.EntityLogicalName,
            AttributeNames = attributeNames,
            GetSyncHandler = helper => (entities, ignoreInvalid, dryRun, ct) => Task.CompletedTask,
            WriteRecord = writeRecord
        };
    }

    private static ModelTypeSyncInfo GetModelTypeSyncInfoForEvent()
    {
        var attributeNames = new[]
        {
            dfeta_TRSEvent.Fields.dfeta_TRSEventId,
            dfeta_TRSEvent.Fields.dfeta_EventName,
            dfeta_TRSEvent.Fields.dfeta_Payload,
        };

        return new ModelTypeSyncInfo<EventInfo>()
        {
            CreateTempTableStatement = null,
            CopyStatement = null,
            UpsertStatement = null,
            DeleteStatement = null,
            IgnoreDeletions = false,
            GetLastModifiedOnStatement = null,
            EntityLogicalName = dfeta_TRSEvent.EntityLogicalName,
            AttributeNames = attributeNames,
            GetSyncHandler = helper => (entities, ignoreInvalid, dryRun, ct) =>
                helper.SyncEventsAsync(entities.Select(e => e.ToEntity<dfeta_TRSEvent>()).ToArray(), dryRun, ct),
            WriteRecord = null
        };
    }

    private (List<PersonInfo> Persons, List<EventBase> Events) MapPersonsAndAudits(
        IReadOnlyCollection<Contact> contacts,
        IReadOnlyDictionary<Guid, AuditDetailCollection> auditDetails,
        bool ignoreInvalid)
    {
        var events = new List<EventBase>();
        var persons = MapPersons(contacts);

        return (persons, events);
    }

    public static List<PersonInfo> MapPersons(IEnumerable<Contact> contacts) => contacts
        .Select(c => new PersonInfo()
        {
            PersonId = c.ContactId!.Value,
            CreatedOn = c.CreatedOn!.Value,
            UpdatedOn = c.ModifiedOn!.Value,
            Status = c.StateCode == ContactState.Active ? PersonStatus.Active : PersonStatus.Deactivated,
            MergedWithPersonId = c.dfeta_MergedWith?.Id,
            Trn = c.dfeta_TRN,
            FirstName = (c.HasStatedNames() ? c.dfeta_StatedFirstName : c.FirstName) ?? string.Empty,
            MiddleName = (c.HasStatedNames() ? c.dfeta_StatedMiddleName : c.MiddleName) ?? string.Empty,
            LastName = (c.HasStatedNames() ? c.dfeta_StatedLastName : c.LastName) ?? string.Empty,
            DateOfBirth = c.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false),
            EmailAddress = c.EMailAddress1.NormalizeString(),
            NationalInsuranceNumber = c.dfeta_NINumber.NormalizeString(),
            Gender = c.GenderCode.ToGender(),
            DqtContactId = c.Id,
            DqtState = (int)c.StateCode!,
            DqtCreatedOn = c.CreatedOn!.Value,
            DqtModifiedOn = c.ModifiedOn!.Value,
            DqtFirstName = c.FirstName ?? string.Empty,
            DqtMiddleName = c.MiddleName ?? string.Empty,
            DqtLastName = c.LastName ?? string.Empty,
            CreatedByTps = c.dfeta_CapitaTRNChangedOn == null ? true : false
        })
        .ToList();

    private (List<InductionInfo> Inductions, List<EventBase> Events) MapInductionsAndAudits(
        IReadOnlyCollection<Contact> contacts,
        IEnumerable<dfeta_induction> inductionEntities,
        IReadOnlyDictionary<Guid, AuditDetailCollection> auditDetails,
        bool ignoreInvalid)
    {
        var inductionLookup = inductionEntities
            .GroupBy(i => i.dfeta_PersonId.Id)
            .ToDictionary(g => g.Key, g => g.ToArray());

        var inductions = new List<InductionInfo>();
        var events = new List<EventBase>();

        foreach (var contact in contacts)
        {
            dfeta_induction? induction = null;
            if (inductionLookup.TryGetValue(contact.ContactId!.Value, out var personInductions))
            {
                // We shouldn't have multiple induction records for the same person in prod at all but we might in other environments
                // so we'll just take the most recently modified one.
                induction = personInductions.OrderByDescending(i => i.ModifiedOn).First();
                if (personInductions.Length > 1)
                {
                    var errorMessage = $"Contact '{contact.ContactId!.Value}' has multiple induction records.";
                    if (ignoreInvalid)
                    {
                        logger.LogWarning(errorMessage);
                    }
                    else
                    {
                        throw new InvalidOperationException(errorMessage);
                    }
                }
            }

            var mapped = MapInductionInfoFromDqtInduction(induction, contact, ignoreInvalid);
            if (mapped is null)
            {
                continue;
            }

            if (induction is not null)
            {
                var inductionAttributeNames = new[]
                {
                    dfeta_induction.Fields.dfeta_CompletionDate,
                    dfeta_induction.Fields.dfeta_InductionExemptionReason,
                    dfeta_induction.Fields.dfeta_StartDate,
                    dfeta_induction.Fields.dfeta_InductionStatus,
                    dfeta_induction.Fields.StateCode
                };

                if (auditDetails.TryGetValue(induction!.Id, out var inductionAudits))
                {
                    var orderedInductionAuditDetails = inductionAudits.AuditDetails
                        .OfType<AttributeAuditDetail>()
                        .Where(a => a.AuditRecord.ToEntity<Audit>().Action != Audit_Action.Merge)
                        .Select(a =>
                        {
                            var allChangedAttributes = a.NewValue.Attributes.Keys.Union(a.OldValue.Attributes.Keys).ToArray();
                            var relevantChangedAttributes = allChangedAttributes.Where(k => inductionAttributeNames.Contains(k)).ToArray();
                            var newValue = a.NewValue.ToEntity<dfeta_induction>().SparseClone(inductionAttributeNames);
                            newValue.Id = induction.Id;
                            var oldValue = a.OldValue.ToEntity<dfeta_induction>().SparseClone(inductionAttributeNames);
                            oldValue.Id = induction.Id;

                            return new AuditInfo<dfeta_induction>
                            {
                                AllChangedAttributes = allChangedAttributes,
                                RelevantChangedAttributes = relevantChangedAttributes,
                                NewValue = newValue,
                                OldValue = oldValue,
                                AuditRecord = a.AuditRecord.ToEntity<Audit>()
                            };
                        })
                        .OrderBy(a => a.AuditRecord.CreatedOn)
                        .ThenBy(a => a.AuditRecord.Action == Audit_Action.Create ? 0 : 1)
                        .ToArray();

                    var auditRecordsToMap = orderedInductionAuditDetails.Skip(1).ToArray();
                    if (orderedInductionAuditDetails.FirstOrDefault() is { AuditRecord: { Action: Audit_Action.Create } } createAction)
                    {
                        events.Add(MapCreatedEvent(contact.ContactId!.Value, createAction.NewValue, createAction.AuditRecord));
                    }
                    else
                    {
                        auditRecordsToMap = orderedInductionAuditDetails;
                        // we may have gaps in the audit history so we can't be sure that we can rewind from the latest back to the imported version
                        var minimalInduction = new Entity(dfeta_induction.EntityLogicalName, induction.Id).ToEntity<dfeta_induction>();
                        minimalInduction.CreatedOn = induction.CreatedOn;
                        minimalInduction.CreatedBy = induction.CreatedBy;
                        events.Add(MapImportedEvent(contact.ContactId!.Value, minimalInduction));
                    }

                    foreach (var item in auditRecordsToMap)
                    {
                        if (item.AllChangedAttributes.Contains(dfeta_induction.Fields.StateCode))
                        {
                            var nonStateAttributes = item.AllChangedAttributes
                                .Where(a => !(a is dfeta_induction.Fields.StateCode or dfeta_induction.Fields.StatusCode))
                                .ToArray();

                            if (nonStateAttributes.Length > 0)
                            {
                                throw new InvalidOperationException(
                                    $"Expected state and status attributes to change in isolation but also received: {string.Join(", ", nonStateAttributes)}.");
                            }
                        }

                        var mappedEvent = MapUpdatedEvent(contact.ContactId!.Value, item.RelevantChangedAttributes, item.NewValue, item.OldValue, item.AuditRecord);
                        if (mappedEvent is not null)
                        {
                            events.Add(mappedEvent);
                        }
                    }
                }
            }

            inductions.Add(mapped);
        }

        return (inductions, events);

        EventBase MapCreatedEvent(Guid contactId, dfeta_induction induction, Audit audit)
        {
            return new DqtInductionCreatedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{induction.Id}-Created",
                CreatedUtc = audit.CreatedOn!.Value,
                RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(audit.UserId.Id, audit.UserId.Name),
                PersonId = contactId,
                Induction = GetEventDqtInduction(induction)
            };
        }

        EventBase MapImportedEvent(Guid contactId, dfeta_induction induction)
        {
            var createdBy = induction.GetAttributeValue<EntityReference>("createdby");
            return new DqtInductionImportedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{induction.Id}-Imported",
                CreatedUtc = induction.CreatedOn!.Value,
                RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(createdBy.Id, createdBy.Name),
                PersonId = contactId,
                Induction = GetEventDqtInduction(induction),
                DqtState = (int)dfeta_inductionState.Active
            };
        }

        EventBase? MapUpdatedEvent(Guid contactId, string[] changedAttributes, dfeta_induction newValue, dfeta_induction oldValue, Audit audit)
        {
            if (changedAttributes.Contains(dfeta_induction.Fields.StateCode))
            {
                if (newValue.StateCode == dfeta_inductionState.Inactive)
                {
                    return new DqtInductionDeactivatedEvent()
                    {
                        EventId = Guid.NewGuid(),
                        Key = $"{audit.Id}",  // The CRM Audit ID
                        CreatedUtc = audit.CreatedOn!.Value,
                        RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(audit.UserId.Id, audit.UserId.Name),
                        PersonId = contactId,
                        Induction = GetEventDqtInduction(newValue)
                    };
                }
                else
                {
                    return new DqtInductionReactivatedEvent()
                    {
                        EventId = Guid.NewGuid(),
                        Key = $"{audit.Id}",  // The CRM Audit ID
                        CreatedUtc = audit.CreatedOn!.Value,
                        RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(audit.UserId.Id, audit.UserId.Name),
                        PersonId = contactId,
                        Induction = GetEventDqtInduction(newValue)
                    };
                }
            }

            var changes = DqtInductionUpdatedEventChanges.None |
                (changedAttributes.Contains(dfeta_induction.Fields.dfeta_StartDate) ? DqtInductionUpdatedEventChanges.StartDate : 0) |
                (changedAttributes.Contains(dfeta_induction.Fields.dfeta_CompletionDate) ? DqtInductionUpdatedEventChanges.CompletionDate : 0) |
                (changedAttributes.Contains(dfeta_induction.Fields.dfeta_InductionStatus) ? DqtInductionUpdatedEventChanges.Status : 0) |
                (changedAttributes.Contains(dfeta_induction.Fields.dfeta_InductionExemptionReason) ? DqtInductionUpdatedEventChanges.ExemptionReason : 0);

            if (changes == DqtInductionUpdatedEventChanges.None)
            {
                return null;
            }

            return new DqtInductionUpdatedEvent()
            {
                EventId = Guid.NewGuid(),
                Key = $"{audit.Id}",  // The CRM Audit ID
                CreatedUtc = audit.CreatedOn!.Value,
                RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(audit.UserId.Id, audit.UserId.Name),
                PersonId = contactId,
                Induction = GetEventDqtInduction(newValue),
                OldInduction = GetEventDqtInduction(oldValue),
                Changes = changes
            };
        }
    }

    EventModels.DqtInduction GetEventDqtInduction(dfeta_induction induction)
    {
        Option<DateOnly?> startDateOption = induction.TryGetAttributeValue<DateTime?>(dfeta_induction.Fields.dfeta_StartDate, out var startDate)
            ? Option.Some(startDate.ToDateOnlyWithDqtBstFix(isLocalTime: true))
            : Option.None<DateOnly?>();

        Option<DateOnly?> completionDateOption = induction.TryGetAttributeValue<DateTime?>(dfeta_induction.Fields.dfeta_CompletionDate, out var completionDate)
            ? Option.Some(completionDate.ToDateOnlyWithDqtBstFix(isLocalTime: true))
            : Option.None<DateOnly?>();

        Option<string?> inductionStatusOption = induction.TryGetAttributeValue<OptionSetValue>(dfeta_induction.Fields.dfeta_InductionStatus, out var inductionStatus)
            ? Option.Some(inductionStatus is not null ? (string?)((dfeta_InductionStatus?)inductionStatus!.Value)!.Value.GetMetadata().Name : null)
            : Option.None<string?>();

        Option<string?> inductionExemptionReasonOption = induction.TryGetAttributeValue<OptionSetValue>(dfeta_induction.Fields.dfeta_InductionExemptionReason, out var inductionExemptionReason)
            ? Option.Some(inductionExemptionReason is not null ? (string?)((dfeta_InductionExemptionReason?)inductionExemptionReason!.Value)!.Value.GetMetadata().Name : null)
            : Option.None<string?>();

        return new EventModels.DqtInduction()
        {
            InductionId = induction.Id,
            StartDate = startDateOption,
            CompletionDate = completionDateOption,
            InductionStatus = inductionStatusOption,
            InductionExemptionReason = inductionExemptionReasonOption
        };
    }

    public async Task SyncQtsIttBusinessEventAuditsAsync(
        IEnumerable<dfeta_businesseventaudit> businessEventAudits,
        bool dryRun,
        DateTime crmDateFormatChangeDate,
        CancellationToken cancellationToken)
    {
        var events = MapQtsIttBusinessEventAudits(businessEventAudits, crmDateFormatChangeDate);
        await BulkSaveEventsAsync(events, "events_qts_itt_audit", dryRun, cancellationToken);
    }

    private IReadOnlyCollection<EventBase> MapQtsIttBusinessEventAudits(
        IEnumerable<dfeta_businesseventaudit> businessEventAudits,
        DateTime crmDateFormatChangeDate)
    {
        var events = new List<EventBase>();
        foreach (var businessEventAudit in businessEventAudits)
        {
            var createdByUser = businessEventAudit.Extract<DqtSystemUser>($"{DqtSystemUser.EntityLogicalName}_createdby", DqtSystemUser.PrimaryIdAttribute);
            var raisedBy = EventModels.RaisedByUserInfo.FromDqtUser(createdByUser.Id, createdByUser.FullName);
            EventBase? trsEvent = null;
            if (businessEventAudit.dfeta_changedfield == "Result")
            {
                trsEvent = MapIttBusinessEventAudit(businessEventAudit, raisedBy);
            }
            else if (businessEventAudit.dfeta_changedfield is "Early Years Teacher Status" or "EYTS Date" or "QTS Date" or "Teacher Status")
            {
                trsEvent = MapQtsBusinessEventAudit(businessEventAudit, raisedBy, crmDateFormatChangeDate);
            }

            if (trsEvent is not null)
            {
                events.Add(trsEvent);
            }
        }

        return events;

        EventBase MapIttBusinessEventAudit(dfeta_businesseventaudit businessEventAudit, EventModels.RaisedByUserInfo raisedBy) => businessEventAudit.dfeta_Event == dfeta_businesseventaudit_dfeta_Event.Create
            ? new DqtInitialTeacherTrainingCreatedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = businessEventAudit.CreatedOn!.Value,
                RaisedBy = raisedBy,
                PersonId = businessEventAudit.dfeta_Person.Id,
                InitialTeacherTraining = new EventModels.DqtInitialTeacherTraining()
                {
                    Result = businessEventAudit.dfeta_NewValue
                }
            }
            : new DqtInitialTeacherTrainingUpdatedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = businessEventAudit.CreatedOn!.Value,
                RaisedBy = raisedBy,
                PersonId = businessEventAudit.dfeta_Person.Id,
                InitialTeacherTraining = new EventModels.DqtInitialTeacherTraining()
                {
                    Result = businessEventAudit.dfeta_NewValue
                },
                OldInitialTeacherTraining = new EventModels.DqtInitialTeacherTraining()
                {
                    Result = businessEventAudit.dfeta_OldValue
                },
                Changes = DqtInitialTeacherTrainingUpdatedEventChanges.Result
            };

        EventBase MapQtsBusinessEventAudit(dfeta_businesseventaudit businessEventAudit, EventModels.RaisedByUserInfo raisedBy, DateTime crmDateFormatChangeDate)
        {
            var crmDateFormat = businessEventAudit.CreatedOn >= crmDateFormatChangeDate ? "M/d/yyyy" : "dd/MM/yyyy";

            return
                businessEventAudit.dfeta_Event == dfeta_businesseventaudit_dfeta_Event.Create
                ? new DqtQtsRegistrationCreatedEvent()
                {
                    EventId = Guid.NewGuid(),
                    CreatedUtc = businessEventAudit.CreatedOn!.Value,
                    RaisedBy = raisedBy,
                    PersonId = businessEventAudit.dfeta_Person.Id,
                    QtsRegistration = new EventModels.DqtQtsRegistration()
                    {
                        QtsDate = businessEventAudit.dfeta_changedfield == "QTS Date" ? !string.IsNullOrEmpty(businessEventAudit.dfeta_NewValue) ? DateOnly.ParseExact(businessEventAudit.dfeta_NewValue, crmDateFormat) : null : null,
                        EytsDate = businessEventAudit.dfeta_changedfield == "EYTS Date" ? !string.IsNullOrEmpty(businessEventAudit.dfeta_NewValue) ? DateOnly.ParseExact(businessEventAudit.dfeta_NewValue, crmDateFormat) : null : null,
                        TeacherStatusName = businessEventAudit.dfeta_changedfield == "Teacher Status" ? businessEventAudit.dfeta_NewValue : null,
                        EarlyYearsStatusName = businessEventAudit.dfeta_changedfield == "Early Years Teacher Status" ? businessEventAudit.dfeta_NewValue : null
                    }
                }
                : new DqtQtsRegistrationUpdatedEvent()
                {
                    EventId = Guid.NewGuid(),
                    CreatedUtc = businessEventAudit.CreatedOn!.Value,
                    RaisedBy = raisedBy,
                    PersonId = businessEventAudit.dfeta_Person.Id,
                    QtsRegistration = new EventModels.DqtQtsRegistration()
                    {
                        QtsDate = businessEventAudit.dfeta_changedfield == "QTS Date" ? !string.IsNullOrEmpty(businessEventAudit.dfeta_NewValue) ? DateOnly.ParseExact(businessEventAudit.dfeta_NewValue, crmDateFormat) : null : null,
                        EytsDate = businessEventAudit.dfeta_changedfield == "EYTS Date" ? !string.IsNullOrEmpty(businessEventAudit.dfeta_NewValue) ? DateOnly.ParseExact(businessEventAudit.dfeta_NewValue, crmDateFormat) : null : null,
                        TeacherStatusName = businessEventAudit.dfeta_changedfield == "Teacher Status" ? businessEventAudit.dfeta_NewValue : null,
                        EarlyYearsStatusName = businessEventAudit.dfeta_changedfield == "Early Years Teacher Status" ? businessEventAudit.dfeta_NewValue : null
                    },
                    OldQtsRegistration = new EventModels.DqtQtsRegistration()
                    {
                        QtsDate = businessEventAudit.dfeta_changedfield == "QTS Date" ? !string.IsNullOrEmpty(businessEventAudit.dfeta_OldValue) ? DateOnly.ParseExact(businessEventAudit.dfeta_OldValue, crmDateFormat) : null : null,
                        EytsDate = businessEventAudit.dfeta_changedfield == "EYTS Date" ? !string.IsNullOrEmpty(businessEventAudit.dfeta_OldValue) ? DateOnly.ParseExact(businessEventAudit.dfeta_OldValue, crmDateFormat) : null : null,
                        TeacherStatusName = businessEventAudit.dfeta_changedfield == "Teacher Status" ? businessEventAudit.dfeta_OldValue : null,
                        EarlyYearsStatusName = businessEventAudit.dfeta_changedfield == "Early Years Teacher Status" ? businessEventAudit.dfeta_OldValue : null
                    },
                    Changes = DqtQtsRegistrationUpdatedEventChanges.QtsDate
                };
        }
    }

    private async Task BulkSaveEventsAsync(
        IReadOnlyCollection<EventBase> events,
        string tempTableSuffix,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        await using var connection = await trsDbDataSource.OpenConnectionAsync(cancellationToken);
        using var txn = await connection.BeginTransactionAsync(cancellationToken);
        await txn.SaveEventsAsync(events, tempTableSuffix, clock, cancellationToken, timeoutSeconds: 120);
        if (!dryRun)
        {
            await txn.CommitAsync(cancellationToken);
        }
        else
        {
            await txn.RollbackAsync(cancellationToken);
        }
    }

    public async Task MigrateIttAndQtsRegistrationsAsync(
        IEnumerable<Contact> contacts,
        IEnumerable<dfeta_qtsregistration> qts,
        IEnumerable<dfeta_initialteachertraining> itt,
        bool ignoreInvalid,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var contactIttQtsMapResults = await MapIttAndQtsRegistrationsAsync(contacts, qts, itt);
        await MigrateRoutesAsync(contactIttQtsMapResults, ignoreInvalid, dryRun);
    }

    public async Task<ContactIttQtsMapResult[]> MapIttAndQtsRegistrationsAsync(
        IEnumerable<Contact> contacts,
        IEnumerable<dfeta_qtsregistration> qts,
        IEnumerable<dfeta_initialteachertraining> itt)
    {
        using var dbContext = TrsDbContext.Create(trsDbDataSource);

        var registerApplicationUser = await dbContext.ApplicationUsers.SingleAsync(u => u.ShortName == "Register");
        var afqtsApplicationUser = await dbContext.ApplicationUsers.SingleAsync(u => u.ShortName == "AfQTS");
        var allRoutes = await referenceDataCache.GetRouteToProfessionalStatusTypesAsync(activeOnly: false);
        var earlyYearsProfessionalStatus = await referenceDataCache.GetEarlyYearsStatusByValueAsync("222");
        var noQualificationRestrictedByGtc = await referenceDataCache.GetIttQualificationByValueAsync("998");
        var partiallyQualifiedTeacherStatus = await referenceDataCache.GetTeacherStatusByValueAsync("214");
        var qualifiedTeacherTrainedTeacherStatus = await referenceDataCache.GetTeacherStatusByValueAsync("71");

        var ittByContact = itt.Where(i => i.StateCode == 0).GroupBy(i => i.dfeta_PersonId.Id).ToDictionary(g => g.Key, g => g.ToArray());
        var qtsByContact = qts.Where(i => i.StateCode == 0).GroupBy(i => i.dfeta_PersonId.Id).ToDictionary(g => g.Key, g => g.ToArray());

        var result = new List<ContactIttQtsMapResult>();

        if (!contacts.Any())
        {
            return [];
        }

        foreach (var contact in contacts)
        {
            try
            {
                var succeeded = await MapContactIttAndQtsAsync(contact);
                result.Add(succeeded);
            }
            catch (IttQtsMapException ex)
            {
                result.Add(new ContactIttQtsMapResult()
                {
                    ContactId = contact.Id,
                    MappedResults = [ex.Result]
                });
            }
        }

        return result.ToArray();

        async Task<ContactIttQtsMapResult> MapContactIttAndQtsAsync(Contact contact)
        {
            var contactId = contact.Id;
            var qtlsDate = contact.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: true);
            var qtlsDateHasBeenSet = contact.dfeta_QtlsDateHasBeenSet;
            var contactItt = ittByContact.GetValueOrDefault(contactId)?.ToList() ?? [];
            var contactQts = qtsByContact.GetValueOrDefault(contactId)?.ToList() ?? [];
            var contactIttRowCount = contactItt.Count;
            var contactQtsRowCount = contactQts.Count;

            var hasMatchedIttQtsCombo = false;
            var hasWelshItt = false;
            var mapped = new List<IttQtsMapResult>();

            // QTLS is stored directly on Contact record
            if (qtlsDate is not null)
            {
                var ps = await CreateProfessionalStatusAsync(
                    registerApplicationUser,
                    afqtsApplicationUser,
                    clock,
                    referenceDataCache,
                    allRoutes,
                    contactId,
                    RouteToProfessionalStatusType.QtlsAndSetMembershipId,
                    RouteToProfessionalStatusStatus.Holds,
                    InductionExemptionReason.QtlsId,
                    qtlsDate,
                    qts: null,
                    itt: null,
                    ittQualification: null,
                    teacherStatus: null,
                    eyStatus: null);
                mapped.Add(await IttQtsMapResult.SucceededAsync(
                            referenceDataCache,
                            ps,
                            itt: null,
                            teacherStatus: null,
                            qtsDate: null,
                            earlyYearsStatus: null,
                            eytsDate: null,
                            partialRecognitionDate: null,
                            qtlsDate,
                            qtlsDateHasBeenSet,
                            ittQualification: null,
                            statusDerivedRouteId: null,
                            programmeTypeDerivedRouteId: null,
                            ittQualificationDerivedRouteId: null,
                            multiplePotentialCompatibleIttRecords: null,
                            contactIttRowCount,
                            contactQtsRowCount));
            }

            // Filter out ITT which we aren't migrating to TRS
            foreach (var itt in contactItt.Where(i => i.dfeta_Result is not null && IttResultsToIgnore.Contains(i.dfeta_Result!.Value)).ToArray())
            {
                var failedReason = IttQtsMapResultFailedReason.DoNotMigrateIttResult;
                if (itt is not null && itt.dfeta_Result is null && itt.dfeta_ITTQualificationId?.Id == noQualificationRestrictedByGtc.Id)
                {
                    failedReason = IttQtsMapResultFailedReason.DoNotMigrateNoQualificationRestrictedByGtc;
                }

                mapped.Add(
                    await IttQtsMapResult.FailedAsync(
                        referenceDataCache,
                        contactId,
                        failedReason,
                        qtsRegistrationId: null,
                        itt: itt,
                        teacherStatus: null,
                        qtsDate: null,
                        earlyYearsStatus: null,
                        eytsDate: null,
                        partialRecognitionDate: null,
                        qtlsDate,
                        qtlsDateHasBeenSet,
                        ittQualification: null,
                        statusDerivedRouteId: null,
                        programmeTypeDerivedRouteId: null,
                        ittQualificationDerivedRouteId: null,
                        multiplePotentialCompatibleIttRecords: null,
                        contactIttRowCount,
                        contactQtsRowCount));
                contactItt.Remove(itt!);
            }

            // Filter out QTS registrations which we aren't migrating to TRS
            foreach (var qts in contactQts.Where(q => q.dfeta_TeacherStatusId is null && q.dfeta_EarlyYearsStatusId is null).ToArray())
            {
                mapped.Add(
                    await IttQtsMapResult.FailedAsync(
                        referenceDataCache,
                        contactId,
                        IttQtsMapResultFailedReason.DoNotMigrateQtsRegistrationHasNoStatus,
                        qts.Id,
                        itt: null,
                        teacherStatus: null,
                        qts.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                        earlyYearsStatus: null,
                        qts.dfeta_EYTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                        qts.dfeta_DateofRecognition.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                        qtlsDate,
                        qtlsDateHasBeenSet,
                        ittQualification: null,
                        statusDerivedRouteId: null,
                        programmeTypeDerivedRouteId: null,
                        ittQualificationDerivedRouteId: null,
                        multiplePotentialCompatibleIttRecords: null,
                        contactIttRowCount,
                        contactQtsRowCount));
                contactQts.Remove(qts);
            }

            // Match QTS then Partial QTS then EYTS then EYPS
            foreach (var qts in contactQts.OrderBy(q => q.dfeta_QTSDate is not null).ThenBy(q => q.dfeta_DateofRecognition is not null).ThenBy(q => q.dfeta_EYTSDate is not null).ThenBy(q => q.dfeta_EarlyYearsStatusId?.Id == earlyYearsProfessionalStatus.Id))
            {
                // A qtsregistration can have a teacher status, EY status or both
                foreach (var isEy in new[] { false, true })
                {
                    Guid[]? mutlipleCompatibleIttIds = null;
                    var teacherStatus = !isEy && qts.dfeta_TeacherStatusId is not null
                        ? await referenceDataCache.GetTeacherStatusByIdAsync(qts.dfeta_TeacherStatusId.Id)
                        : null;

                    if (!isEy && teacherStatus is null)
                    {
                        continue;
                    }

                    var eyStatus = isEy && qts.dfeta_EarlyYearsStatusId is not null
                        ? await referenceDataCache.GetEarlyYearsStatusByIdAsync(qts.dfeta_EarlyYearsStatusId.Id)
                        : null;

                    if (isEy && eyStatus is null)
                    {
                        continue;
                    }

                    // Validate for partial recognition
                    if ((qts.dfeta_TeacherStatusId?.Id == partiallyQualifiedTeacherStatus.Id && qts.dfeta_DateofRecognition is null) ||
                        (qts.dfeta_TeacherStatusId?.Id != partiallyQualifiedTeacherStatus.Id && qts.dfeta_DateofRecognition is not null) ||
                        (qts.dfeta_DateofRecognition is not null && (qts.dfeta_QTSDate is not null || qts.dfeta_EYTSDate is not null)))
                    {
                        mapped.Add(
                            await IttQtsMapResult.FailedAsync(
                                referenceDataCache,
                                contactId,
                                IttQtsMapResultFailedReason.PartialRecognitionIsInvalid,
                                qts.Id,
                                itt: null,
                                teacherStatus,
                                qts.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                eyStatus,
                                qts.dfeta_EYTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                qts.dfeta_DateofRecognition.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                qtlsDate,
                                qtlsDateHasBeenSet,
                                ittQualification: null,
                                statusDerivedRouteId: null,
                                programmeTypeDerivedRouteId: null,
                                ittQualificationDerivedRouteId: null,
                                multiplePotentialCompatibleIttRecords: null,
                                contactIttRowCount,
                                contactQtsRowCount));
                        continue;
                    }

                    Guid? inductionExemptionReasonId = null;

                    var compatibleProgrammeTypes = Enum.GetValues<dfeta_ITTProgrammeType>()
                        .Where(i => isEy == i.IsEarlyYears())
                        .ToArray();

                    dfeta_initialteachertraining? itt = null;

                    var specifiedItt = contactItt.Where(i => i.GetAttributeValue<EntityReference>("dfeta_qtsregistration")?.Id == qts.Id && (i.dfeta_ProgrammeType is null || compatibleProgrammeTypes.Contains(i.dfeta_ProgrammeType!.Value))).ToArray();

                    var matchingItt = specifiedItt.Length > 0
                        ? specifiedItt
                        : contactItt
                            .Where(itt => (itt.dfeta_ProgrammeType is null || compatibleProgrammeTypes.Contains(itt.dfeta_ProgrammeType!.Value)))
                            .ToArray();

                    mutlipleCompatibleIttIds = matchingItt.Length > 1
                        ? matchingItt.Select(i => i.Id).ToArray()
                        : null;

                    // Very specific overrides for contacts which have multiple potential ITT records but we know which one to use
                    if (HardcodedQtsEyMappings.TryGetValue(contactId, out var hardcodedItt))
                    {
                        if (isEy)
                        {
                            itt = matchingItt
                                .Where(i => i.Id == hardcodedItt.EyIttId)
                                .FirstOrDefault();
                        }
                        else
                        {
                            itt = matchingItt
                                .Where(i => i.Id == hardcodedItt.QtsIttId)
                                .FirstOrDefault();
                        }
                    }
                    else
                    {
                        // Match ITT on ones which are associated with being awarded QTS or EYTS first, then EYPS, then ones with programme type, then createdon date (as createdon is unreliable due to it being reset from previous data migrations)
                        itt = matchingItt
                            .OrderBy(itt => (itt.dfeta_Result == dfeta_ITTResult.Pass || itt.dfeta_Result == dfeta_ITTResult.Approved) && ((!isEy && (qts.dfeta_QTSDate is not null || qts.dfeta_DateofRecognition is not null)) || (isEy && (qts.dfeta_EYTSDate is not null || eyStatus == earlyYearsProfessionalStatus))) ? 0 : 1)
                            .ThenBy(itt => isEy && (eyStatus == earlyYearsProfessionalStatus && (itt.dfeta_AgeRangeFrom == dfeta_AgeRange._00 && itt.dfeta_AgeRangeTo == dfeta_AgeRange._05) || (itt.dfeta_AgeRangeFrom is null && itt.dfeta_AgeRangeTo is null)) ? 0 : 1)
                            .ThenBy(itt => itt.dfeta_ProgrammeType is not null ? 0 : 1)
                            .ThenBy(itt => itt.GetAttributeValue<DateTime>("createdon"))
                            .FirstOrDefault();
                    }

                    // Prevent this ITT record from being mapped again
                    if (itt is not null)
                    {
                        contactItt.Remove(itt);
                    }

                    DateOnly? awardedDate = isEy
                        ? qts.dfeta_EYTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true)
                        : teacherStatus!.Id != partiallyQualifiedTeacherStatus.Id
                            ? qts.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true)
                            : qts.dfeta_DateofRecognition.ToDateOnlyWithDqtBstFix(isLocalTime: true);

                    // Do Not Migrate
                    if ((awardedDate is null && itt?.dfeta_Result is null))
                    {
                        var failedReason = IttQtsMapResultFailedReason.DoNotMigrateIttResult;
                        if (itt is not null && itt.dfeta_Result is null && itt.dfeta_ITTQualificationId?.Id == noQualificationRestrictedByGtc.Id)
                        {
                            failedReason = IttQtsMapResultFailedReason.DoNotMigrateNoQualificationRestrictedByGtc;
                        }

                        mapped.Add(
                            await IttQtsMapResult.FailedAsync(
                                referenceDataCache,
                                contactId,
                                failedReason,
                                qts.Id,
                                itt: itt,
                                teacherStatus,
                                qts.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                eyStatus,
                                qts.dfeta_EYTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                qts.dfeta_DateofRecognition.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                qtlsDate,
                                qtlsDateHasBeenSet,
                                ittQualification: null,
                                statusDerivedRouteId: null,
                                programmeTypeDerivedRouteId: null,
                                ittQualificationDerivedRouteId: null,
                                mutlipleCompatibleIttIds,
                                contactIttRowCount,
                                contactQtsRowCount));
                        continue;
                    }

                    // Re-map teachers who have Qualified teacher (trained) gained at a Welsh ITT provider -> Welsh R route
                    Guid? welshDerivedRouteId = null;
                    if (itt?.dfeta_EstablishmentId is not null && WelshIttProviderIds.Contains(itt!.dfeta_EstablishmentId!.Id) &&
                        teacherStatus?.dfeta_Value == qualifiedTeacherTrainedTeacherStatus.dfeta_Value &&
                        itt?.dfeta_Result == dfeta_ITTResult.Pass &&
                        (itt?.dfeta_ProgrammeType is null or dfeta_ITTProgrammeType.HEI) &&
                        itt?.dfeta_ITTQualificationId is not null)
                    {
                        welshDerivedRouteId = RouteToProfessionalStatusType.WelshRId;
                    }

                    var ittQualification = itt?.dfeta_ITTQualificationId is not null
                        ? await referenceDataCache.GetIttQualificationByIdAsync(itt.dfeta_ITTQualificationId.Id)
                        : null;

                    var status = (isEy && (qts.dfeta_EYTSDate is not null || eyStatus?.dfeta_Value == earlyYearsProfessionalStatus.dfeta_Value)) || (!isEy && (qts.dfeta_QTSDate is not null || qts.dfeta_DateofRecognition is not null))
                        ? RouteToProfessionalStatusStatus.Holds
                        : await MapStatusAsync(itt, qts, ittQualification, teacherStatus, eyStatus);

                    // Map ITT, QTS & ITT Qual and check they resolve to the same route type (where mapping is possible)
                    // If not, use precedence rules (see 'Manual cases' tab)
                    // hardcoded first ( in route type) then the logic in the new rules column (manual cases, then the other cases in there) then the lookups from route type column
                    var ittQualificationDerivedRouteId = ittQualification is not null
                        ? (_ittQualificationRouteIdMapping.TryGetValue(ittQualification.dfeta_Value, out var id) ? id : null)
                        : (Guid?)null;

                    var programmeTypeDerivedRouteId = itt?.dfeta_ProgrammeType is dfeta_ITTProgrammeType pt
                        ? (_programmeTypeRouteMapping.TryGetValue(pt, out var id2) ? id2 : null)
                        : (Guid?)null;

                    var statusCode = isEy ? eyStatus!.dfeta_Value : teacherStatus!.dfeta_Value;

                    var statusDerivedRouteId = _statusRouteIdMapping.GetValueOrDefault(statusCode, null);

                    Guid[] derivedRouteIds = welshDerivedRouteId is not null ? [welshDerivedRouteId!.Value] : new[] { programmeTypeDerivedRouteId, statusDerivedRouteId, ittQualificationDerivedRouteId }
                        .Where(s => s is not null)
                        .Distinct()
                        .Cast<Guid>()
                        .ToArray();
                    if (derivedRouteIds.Length != 1)
                    {
                        if (TryDeriveRouteId(
                            teacherStatus,
                            eyStatus,
                            statusDerivedRouteId,
                            itt?.dfeta_ProgrammeType,
                            programmeTypeDerivedRouteId,
                            ittQualification,
                            ittQualificationDerivedRouteId,
                            itt?.dfeta_Result,
                            hasMatchedIttQtsCombo,
                            isEy,
                            out var derivedRouteId))
                        {
                            derivedRouteIds = [derivedRouteId!.Value];
                        }
                        else
                        {
                            throw new IttQtsMapException(
                                await IttQtsMapResult.FailedAsync(
                                    referenceDataCache,
                                    contactId,
                                    IttQtsMapResultFailedReason.CannotDeriveRoute,
                                    qts.Id,
                                    itt,
                                    teacherStatus,
                                    qts.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                    eyStatus,
                                    qts.dfeta_EYTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                    qts.dfeta_DateofRecognition.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                    qtlsDate,
                                    qtlsDateHasBeenSet,
                                    ittQualification,
                                    statusDerivedRouteId,
                                    programmeTypeDerivedRouteId,
                                    ittQualificationDerivedRouteId,
                                    mutlipleCompatibleIttIds,
                                    contactIttRowCount,
                                    contactQtsRowCount));
                        }
                    }

                    var routeId = derivedRouteIds.Single();
                    if (!isEy)
                    {
                        hasMatchedIttQtsCombo = true;
                    }

                    if (routeId == RouteToProfessionalStatusType.WelshRId)
                    {
                        hasWelshItt = true;
                    }

                    var ps = await CreateProfessionalStatusAsync(
                        registerApplicationUser,
                        afqtsApplicationUser,
                        clock,
                        referenceDataCache,
                        allRoutes,
                        contactId,
                        routeId,
                        status,
                        inductionExemptionReasonId,
                        awardedDate,
                        qts,
                        itt,
                        ittQualification,
                        teacherStatus,
                        eyStatus);
                    mapped.Add(
                        await IttQtsMapResult.SucceededAsync(
                            referenceDataCache,
                            ps,
                            itt,
                            teacherStatus,
                            qts.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                            eyStatus,
                            qts.dfeta_EYTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                            qts.dfeta_DateofRecognition.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                            qtlsDate,
                            qtlsDateHasBeenSet,
                            ittQualification,
                            statusDerivedRouteId,
                            programmeTypeDerivedRouteId,
                            ittQualificationDerivedRouteId,
                            mutlipleCompatibleIttIds,
                            contactIttRowCount,
                            contactQtsRowCount));
                }
            }

            foreach (var itt in contactItt)
            {
                // If we get here, we have an ITT record which has not been matched to a QTS record

                // Do Not Migrate
                if (itt.dfeta_Result is null)
                {
                    var failedReason = IttQtsMapResultFailedReason.DoNotMigrateIttResult;
                    if (itt.dfeta_ITTQualificationId?.Id == noQualificationRestrictedByGtc.Id)
                    {
                        failedReason = IttQtsMapResultFailedReason.DoNotMigrateNoQualificationRestrictedByGtc;
                    }

                    mapped.Add(
                        await IttQtsMapResult.FailedAsync(
                            referenceDataCache,
                            contactId,
                            failedReason,
                            qtsRegistrationId: null,
                            itt: itt,
                            teacherStatus: null,
                            qtsDate: null,
                            earlyYearsStatus: null,
                            eytsDate: null,
                            partialRecognitionDate: null,
                            qtlsDate,
                            qtlsDateHasBeenSet,
                            ittQualification: null,
                            statusDerivedRouteId: null,
                            programmeTypeDerivedRouteId: null,
                            ittQualificationDerivedRouteId: null,
                            multiplePotentialCompatibleIttRecords: null,
                            contactIttRowCount,
                            contactQtsRowCount));
                    continue;
                }

                // If we have ITT records which are Pass or Approved then this might be because of duplicate records
                // i.e. if we've already matched to QTS for a contact, we shouldn't have another ITT record with a Pass or Approved result for the same thing
                if (itt.dfeta_Result is dfeta_ITTResult.Pass or dfeta_ITTResult.Approved)
                {
                    var failedReason = IttQtsMapResultFailedReason.PassResultHasNoAwardDate;
                    if (mapped.Any(r => r.IttResult == dfeta_ITTResult.Pass || r.IttResult == dfeta_ITTResult.Approved))
                    {
                        failedReason = hasWelshItt ? IttQtsMapResultFailedReason.DoNotMigrateQtsAwardedInWalesDuplicateIttRecords : IttQtsMapResultFailedReason.PotentialDuplicateIttRecords;
                    }

                    mapped.Add(
                        await IttQtsMapResult.FailedAsync(
                            referenceDataCache,
                            contactId,
                            failedReason,
                            qtsRegistrationId: null,
                            itt: itt,
                            teacherStatus: null,
                            qtsDate: null,
                            earlyYearsStatus: null,
                            eytsDate: null,
                            partialRecognitionDate: null,
                            qtlsDate,
                            qtlsDateHasBeenSet,
                            ittQualification: null,
                            statusDerivedRouteId: null,
                            programmeTypeDerivedRouteId: null,
                            ittQualificationDerivedRouteId: null,
                            multiplePotentialCompatibleIttRecords: null,
                            contactIttRowCount,
                            contactQtsRowCount));
                    continue;
                }

                var ittQualification = itt?.dfeta_ITTQualificationId is not null
                        ? await referenceDataCache.GetIttQualificationByIdAsync(itt.dfeta_ITTQualificationId.Id)
                        : null;

                var status = await MapStatusAsync(itt, qts: null, ittQualification, teacherStatus: null, earlyYearsStatus: null);

                var ittQualificationDerivedRouteId = ittQualification is not null
                        ? (_ittQualificationRouteIdMapping.TryGetValue(ittQualification.dfeta_Value, out var id) ? id : null)
                        : (Guid?)null;

                var programmeTypeDerivedRouteId = itt?.dfeta_ProgrammeType is dfeta_ITTProgrammeType pt
                    ? (_programmeTypeRouteMapping.TryGetValue(pt, out var id2) ? id2 : null)
                    : (Guid?)null;

                var isEy = itt?.dfeta_ProgrammeType?.IsEarlyYears() ?? false;

                var derivedRouteIds = new[] { programmeTypeDerivedRouteId, ittQualificationDerivedRouteId }
                        .Where(s => s is not null)
                        .Distinct()
                        .Cast<Guid>()
                        .ToArray();
                if (derivedRouteIds.Length != 1)
                {
                    if (TryDeriveRouteId(
                        null,
                        null,
                        null,
                        itt?.dfeta_ProgrammeType,
                        programmeTypeDerivedRouteId,
                        ittQualification,
                        ittQualificationDerivedRouteId,
                        itt?.dfeta_Result,
                        hasMatchedIttQtsCombo,
                        isEy,
                        out var derivedRouteId))
                    {
                        derivedRouteIds = [derivedRouteId!.Value];
                    }
                    else
                    {
                        throw new IttQtsMapException(
                            await IttQtsMapResult.FailedAsync(
                                referenceDataCache,
                                contactId,
                                IttQtsMapResultFailedReason.CannotDeriveRoute,
                                qtsRegistrationId: null,
                                itt,
                                teacherStatus: null,
                                qtsDate: null,
                                earlyYearsStatus: null,
                                eytsDate: null,
                                partialRecognitionDate: null,
                                qtlsDate,
                            qtlsDateHasBeenSet,
                                ittQualification,
                                statusDerivedRouteId: null,
                                programmeTypeDerivedRouteId,
                                ittQualificationDerivedRouteId,
                                multiplePotentialCompatibleIttRecords: null,
                                contactIttRowCount,
                                contactQtsRowCount));
                    }
                }

                var routeId = derivedRouteIds.Single();

                var ps = await CreateProfessionalStatusAsync(
                        registerApplicationUser,
                        afqtsApplicationUser,
                        clock,
                        referenceDataCache,
                        allRoutes,
                        contactId,
                        routeId,
                        status,
                        inductionExemptionReasonId: null,
                        awardedDate: null,
                        qts: null,
                        itt,
                        ittQualification,
                        teacherStatus: null,
                        eyStatus: null);
                mapped.Add(
                    await IttQtsMapResult.SucceededAsync(
                        referenceDataCache,
                        ps,
                        itt,
                        teacherStatus: null,
                        qtsDate: null,
                        earlyYearsStatus: null,
                        eytsDate: null,
                        partialRecognitionDate: null,
                        qtlsDate,
                        qtlsDateHasBeenSet,
                        ittQualification,
                        statusDerivedRouteId: null,
                        programmeTypeDerivedRouteId,
                        ittQualificationDerivedRouteId,
                        multiplePotentialCompatibleIttRecords: null,
                        contactIttRowCount,
                        contactQtsRowCount));
            }

            return new ContactIttQtsMapResult()
            {
                ContactId = contactId,
                MappedResults = mapped.ToArray()
            };

            bool TryDeriveRouteId(
                dfeta_teacherstatus? teacherStatus,
                dfeta_earlyyearsstatus? earlyYearsStatus,
                Guid? statusDerivedRouteId,
                dfeta_ITTProgrammeType? ittProgrammeType,
                Guid? programmeTypeDerivedRouteId,
                dfeta_ittqualification? ittQualification,
                Guid? ittQualificationDerivedRouteId,
                dfeta_ITTResult? ittResult,
                bool hasMatchedIttQtsCombo,
                bool isEarlyYears,
                out Guid? routeId)
            {
                // Based on rules defined in spreadsheet try and derive which routeId to map to where there are conflicts from mapping from ITT, QTS and ITT Qualification
                RouteMappingPrecedence[] defaultPrecedenceOrder = [RouteMappingPrecedence.TeachingStatus, RouteMappingPrecedence.ProgrammeType, RouteMappingPrecedence.IttQualification];
                RouteMappingPrecedence[] precedenceOrder = defaultPrecedenceOrder;

                if (_hardcodedRouteIdMapping.TryGetValue((isEarlyYears ? earlyYearsStatus?.dfeta_Value : teacherStatus?.dfeta_Value, ittProgrammeType, ittQualification?.dfeta_Value), out routeId))
                {
                    return true;
                }

                // Additional hardcoded mappings suggested by Rob
                if (_hardcodedIncludingResultRouteIdMapping.TryGetValue((isEarlyYears ? earlyYearsStatus?.dfeta_Value : teacherStatus?.dfeta_Value, ittProgrammeType, ittQualification?.dfeta_Value, ittResult), out routeId))
                {
                    return true;
                }

                if (!isEarlyYears)
                {
                    precedenceOrder = TryDeriveRouteMappingPrecedence(teacherStatus, ittProgrammeType, ittQualification, hasMatchedIttQtsCombo, out var derivedPrecendenceOrder)
                        ? derivedPrecendenceOrder
                        : defaultPrecedenceOrder;
                }

                foreach (var precedence in precedenceOrder)
                {
                    switch (precedence)
                    {
                        case RouteMappingPrecedence.TeachingStatus:
                            if (statusDerivedRouteId is not null)
                            {
                                routeId = statusDerivedRouteId;
                                return true;
                            }
                            break;
                        case RouteMappingPrecedence.ProgrammeType:
                            if (programmeTypeDerivedRouteId is not null)
                            {
                                routeId = programmeTypeDerivedRouteId;
                                return true;
                            }
                            break;
                        case RouteMappingPrecedence.IttQualification:
                            if (ittQualificationDerivedRouteId is not null)
                            {
                                routeId = ittQualificationDerivedRouteId;
                                return true;
                            }
                            break;
                        default:
                            break;
                    }
                }

                routeId = null;
                return false;
            }

            bool TryDeriveRouteMappingPrecedence(
                dfeta_teacherstatus? teacherStatus,
                dfeta_ITTProgrammeType? ittProgrammeType,
                dfeta_ittqualification? ittQualification,
                bool hasMatchedIttQtsCombo,
                out RouteMappingPrecedence[] precendenceOrder)
            {
                if (hasMatchedIttQtsCombo && ittProgrammeType is null)
                {
                    precendenceOrder = [RouteMappingPrecedence.IttQualification];
                    return true;
                }

                if (_manualRouteMappingPrecedence.TryGetValue((teacherStatus?.dfeta_Value, ittProgrammeType, ittQualification?.dfeta_Value), out var manualPrecendence))
                {
                    precendenceOrder = manualPrecendence == RouteMappingPrecedence.TeachingStatus && hasMatchedIttQtsCombo ? [RouteMappingPrecedence.ProgrammeType, RouteMappingPrecedence.IttQualification] : [manualPrecendence];
                    return true;
                }

                if (hasMatchedIttQtsCombo)
                {
                    precendenceOrder = [RouteMappingPrecedence.ProgrammeType, RouteMappingPrecedence.IttQualification];
                    return true;
                }

                if (teacherStatus?.dfeta_Value == "77" && ittProgrammeType == dfeta_ITTProgrammeType.Core)
                {
                    precendenceOrder = [RouteMappingPrecedence.ProgrammeType];
                    return true;
                }

                string[] combo1TeacherStatuses = ["72", "79", "89", "82", "92", "99"];
                string[] combo1IttQualifications = ["7", "8", "10", "14", "9", "18", "11", "1", "2", "16", "3", "4", "13", "17", "15", "6", "5"];
                string[] combo2TeacherStatuses = ["51", "52"];

                if ((combo1TeacherStatuses.Contains(teacherStatus?.dfeta_Value) && combo1IttQualifications.Contains(ittQualification?.dfeta_Value)) ||
                    (combo2TeacherStatuses.Contains(teacherStatus?.dfeta_Value) && ittQualification?.dfeta_Value == "54"))
                {
                    precendenceOrder = [RouteMappingPrecedence.IttQualification];
                    return true;
                }

                string[] combo3TeacherStatuses = ["213", "68", "69", "28"];
                if (combo3TeacherStatuses.Contains(teacherStatus?.dfeta_Value))
                {
                    precendenceOrder = [RouteMappingPrecedence.TeachingStatus];
                    return true;
                }

                if (ittProgrammeType is not null)
                {
                    precendenceOrder = [RouteMappingPrecedence.TeachingStatus, RouteMappingPrecedence.ProgrammeType];
                    return true;
                }

                precendenceOrder = [];
                return false;
            }

            async Task<RouteToProfessionalStatusStatus> MapStatusAsync(
                dfeta_initialteachertraining? itt,
                dfeta_qtsregistration? qts,
                dfeta_ittqualification? ittQualification,
                dfeta_teacherstatus? teacherStatus,
                dfeta_earlyyearsstatus? earlyYearsStatus) => itt?.dfeta_Result switch
                {
                    dfeta_ITTResult.Approved => throw new IttQtsMapException(
                        await IttQtsMapResult.FailedAsync(
                            referenceDataCache,
                            contactId,
                            IttQtsMapResultFailedReason.ApprovedResultHasNoAwardDate,
                            qts?.Id,
                            itt,
                            teacherStatus,
                            qts?.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                            earlyYearsStatus,
                            qts?.dfeta_EYTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                            qts?.dfeta_DateofRecognition.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                            qtlsDate,
                            qtlsDateHasBeenSet,
                            ittQualification,
                            statusDerivedRouteId: null,
                            programmeTypeDerivedRouteId: null,
                            ittQualificationDerivedRouteId: null,
                            multiplePotentialCompatibleIttRecords: null,
                            contactIttRowCount,
                            contactQtsRowCount)),
                    dfeta_ITTResult.Deferred => RouteToProfessionalStatusStatus.Deferred,
                    dfeta_ITTResult.DeferredforSkillsTests => RouteToProfessionalStatusStatus.DeferredForSkillsTest,
                    dfeta_ITTResult.Fail => RouteToProfessionalStatusStatus.Failed,
                    dfeta_ITTResult.InTraining => RouteToProfessionalStatusStatus.InTraining,
                    dfeta_ITTResult.Pass => throw new IttQtsMapException(
                        await IttQtsMapResult.FailedAsync(
                            referenceDataCache,
                            contactId,
                            IttQtsMapResultFailedReason.PassResultHasNoAwardDate,
                            qts?.Id,
                            itt,
                            teacherStatus,
                            qts?.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                            earlyYearsStatus,
                            qts?.dfeta_EYTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                            qts?.dfeta_DateofRecognition.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                            qtlsDate,
                            qtlsDateHasBeenSet,
                            ittQualification,
                            statusDerivedRouteId: null,
                            programmeTypeDerivedRouteId: null,
                            ittQualificationDerivedRouteId: null,
                            multiplePotentialCompatibleIttRecords: null,
                            contactIttRowCount,
                            contactQtsRowCount)),
                    dfeta_ITTResult.UnderAssessment => RouteToProfessionalStatusStatus.UnderAssessment,
                    dfeta_ITTResult.Withdrawn => RouteToProfessionalStatusStatus.Withdrawn,
                    _ => throw new IttQtsMapException(
                        await IttQtsMapResult.FailedAsync(
                            referenceDataCache,
                            contactId,
                            IttQtsMapResultFailedReason.CannotMapStatus,
                            qts?.Id,
                            itt,
                            teacherStatus,
                            qts?.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                            earlyYearsStatus,
                            qts?.dfeta_EYTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                            qts?.dfeta_DateofRecognition.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                            qtlsDate,
                            qtlsDateHasBeenSet,
                            ittQualification,
                            statusDerivedRouteId: null,
                            programmeTypeDerivedRouteId: null,
                            ittQualificationDerivedRouteId: null,
                            multiplePotentialCompatibleIttRecords: null,
                            contactIttRowCount,
                            contactQtsRowCount))
                };

            static async Task<RouteToProfessionalStatusInfo> CreateProfessionalStatusAsync(
                ApplicationUser registerApplicationUser,
                ApplicationUser afqtsApplicationsUser,
                IClock clock,
                ReferenceDataCache referenceDataCache,
                RouteToProfessionalStatusType[] allRoutes,
                Guid personId,
                Guid routeId,
                RouteToProfessionalStatusStatus status,
                Guid? inductionExemptionReasonId,
                DateOnly? awardedDate,
                dfeta_qtsregistration? qts,
                dfeta_initialteachertraining? itt,
                dfeta_ittqualification? ittQualification,
                dfeta_teacherstatus? teacherStatus,
                dfeta_earlyyearsstatus? eyStatus,
                bool ignoreInvalidData = true)
            {
                var sourceApplicationReference = itt?.dfeta_SlugId;
                ApplicationUser? sourceApplicationUser = null;
                if (sourceApplicationReference is not null)
                {
                    sourceApplicationUser = routeId.IsOverseas() ? afqtsApplicationsUser : registerApplicationUser;
                }

                var route = allRoutes.Single(r => r.RouteToProfessionalStatusTypeId == routeId);
                var trainingAgeSpecialism = AgeRange.ConvertToTrsTrainingAgeSpecialism(itt?.dfeta_AgeRangeFrom, itt?.dfeta_AgeRangeTo);
                var degreeTypeId = ittQualification?.ConvertToTrsDegreeTypeId() ?? null;
                var degreeType = degreeTypeId is not null
                    ? await referenceDataCache.GetDegreeTypeByIdAsync(degreeTypeId.Value)
                    : null;

                Country? country = null;
                if (itt?.dfeta_CountryId is not null && itt.dfeta_CountryId.Id != Guid.Empty)
                {
                    var result = await itt.dfeta_CountryId.Id.TryConvertToTrsCountryAsync(referenceDataCache);
                    if (result.IsSuccess)
                    {
                        country = result.Result;
                    }
                    else if (!ignoreInvalidData)
                    {
                        throw new IttQtsMapException(
                            await IttQtsMapResult.FailedAsync(
                                referenceDataCache,
                                personId,
                                IttQtsMapResultFailedReason.UnmappableIttCountryId,
                                qts?.Id,
                                itt,
                                teacherStatus,
                                qts?.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                eyStatus,
                                qts?.dfeta_EYTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                qts?.dfeta_DateofRecognition.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                qtlsDate: null,
                                qtlsDateHasBeenSet: null,
                                ittQualification,
                                statusDerivedRouteId: null,
                                programmeTypeDerivedRouteId: null,
                                ittQualificationDerivedRouteId: null,
                                multiplePotentialCompatibleIttRecords: null,
                                contactIttRowCount: 0,
                                contactQtsRowCount: 0));
                    }
                }

                var trainingSubjectIds = new List<Guid>();
                TrainingSubject? trainingSubject1 = null;
                if (itt?.dfeta_Subject1Id is not null)
                {
                    var result = await itt.dfeta_Subject1Id.Id.TryConvertToTrsTrainingSubjectAsync(referenceDataCache);
                    if (result.IsSuccess)
                    {
                        trainingSubjectIds.Add(result.Result!.TrainingSubjectId);
                        trainingSubject1 = result.Result;
                    }
                    else if (!ignoreInvalidData)
                    {
                        throw new IttQtsMapException(
                            await IttQtsMapResult.FailedAsync(
                                referenceDataCache,
                                personId,
                                IttQtsMapResultFailedReason.UnmappableIttSubjectId,
                                qts?.Id,
                                itt,
                                teacherStatus,
                                qts?.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                eyStatus,
                                qts?.dfeta_EYTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                qts?.dfeta_DateofRecognition.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                qtlsDate: null,
                                qtlsDateHasBeenSet: null,
                                ittQualification,
                                statusDerivedRouteId: null,
                                programmeTypeDerivedRouteId: null,
                                ittQualificationDerivedRouteId: null,
                                multiplePotentialCompatibleIttRecords: null,
                                contactIttRowCount: 0,
                                contactQtsRowCount: 0));
                    }
                }

                var trainingSubject2 = default(TrainingSubject);
                if (itt?.dfeta_Subject2Id is not null)
                {
                    var result = await itt.dfeta_Subject2Id.Id.TryConvertToTrsTrainingSubjectAsync(referenceDataCache);
                    if (result.IsSuccess)
                    {
                        trainingSubjectIds.Add(result.Result!.TrainingSubjectId);
                        trainingSubject2 = result.Result;
                    }
                    else if (!ignoreInvalidData)
                    {
                        throw new IttQtsMapException(
                            await IttQtsMapResult.FailedAsync(
                                referenceDataCache,
                                personId,
                                IttQtsMapResultFailedReason.UnmappableIttSubjectId,
                                qts?.Id,
                                itt,
                                teacherStatus,
                                qts?.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                eyStatus,
                                qts?.dfeta_EYTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                qts?.dfeta_DateofRecognition.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                qtlsDate: null,
                                qtlsDateHasBeenSet: null,
                                ittQualification,
                                statusDerivedRouteId: null,
                                programmeTypeDerivedRouteId: null,
                                ittQualificationDerivedRouteId: null,
                                multiplePotentialCompatibleIttRecords: null,
                                contactIttRowCount: 0,
                                contactQtsRowCount: 0));
                    }
                }

                var trainingSubject3 = default(TrainingSubject);
                if (itt?.dfeta_Subject3Id is not null)
                {
                    var result = await itt.dfeta_Subject3Id.Id.TryConvertToTrsTrainingSubjectAsync(referenceDataCache);
                    if (result.IsSuccess)
                    {
                        trainingSubjectIds.Add(result.Result!.TrainingSubjectId);
                        trainingSubject3 = result.Result;
                    }
                    else if (!ignoreInvalidData)
                    {
                        throw new IttQtsMapException(
                            await IttQtsMapResult.FailedAsync(
                                referenceDataCache,
                                personId,
                                IttQtsMapResultFailedReason.UnmappableIttSubjectId,
                                qts?.Id,
                                itt,
                                teacherStatus,
                                qts?.dfeta_QTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                eyStatus,
                                qts?.dfeta_EYTSDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                qts?.dfeta_DateofRecognition.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                                qtlsDate: null,
                                qtlsDateHasBeenSet: null,
                                ittQualification,
                                statusDerivedRouteId: null,
                                programmeTypeDerivedRouteId: null,
                                ittQualificationDerivedRouteId: null,
                                multiplePotentialCompatibleIttRecords: null,
                                contactIttRowCount: 0,
                                contactQtsRowCount: 0));
                    }
                }

                var trainingProvider = itt?.dfeta_EstablishmentId is not null
                    ? await itt.dfeta_EstablishmentId.Id.ConvertToTrsTrainingProviderAsync(referenceDataCache)
                    : null;

                var professionalStatus = new RouteToProfessionalStatus
                {
                    SourceApplicationReference = sourceApplicationReference,
                    SourceApplicationUserId = sourceApplicationUser?.UserId,
                    QualificationId = Guid.NewGuid(),
                    CreatedOn = clock.UtcNow,
                    UpdatedOn = clock.UtcNow,
                    DeletedOn = null,
                    PersonId = personId,
                    RouteToProfessionalStatusTypeId = routeId,
                    Status = status,
                    TrainingStartDate = itt?.dfeta_ProgrammeStartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                    TrainingEndDate = itt?.dfeta_ProgrammeEndDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                    TrainingAgeSpecialismType = trainingAgeSpecialism?.TrainingAgeSpecialismType,
                    TrainingAgeSpecialismRangeFrom = trainingAgeSpecialism?.TrainingAgeSpecialismRangeFrom,
                    TrainingAgeSpecialismRangeTo = trainingAgeSpecialism?.TrainingAgeSpecialismRangeTo,
                    HoldsFrom = awardedDate,
                    DegreeTypeId = degreeTypeId,
                    TrainingSubjectIds = trainingSubjectIds.ToArray(),
                    TrainingCountryId = country?.CountryId,
                    TrainingProviderId = trainingProvider?.TrainingProviderId,
                    ExemptFromInduction = null,
                    DqtTeacherStatusName = teacherStatus?.dfeta_name,
                    DqtTeacherStatusValue = teacherStatus?.dfeta_Value,
                    DqtEarlyYearsStatusName = eyStatus?.dfeta_name,
                    DqtEarlyYearsStatusValue = eyStatus?.dfeta_Value,
                    DqtInitialTeacherTrainingId = itt?.Id,
                    DqtQtsRegistrationId = qts?.Id,
                    DqtAgeRangeFrom = itt?.dfeta_AgeRangeFrom.ToString(),
                    DqtAgeRangeTo = itt?.dfeta_AgeRangeTo.ToString(),
                };

                return new RouteToProfessionalStatusInfo
                {
                    ProfessionalStatus = professionalStatus,
                    RouteToProfessionalStatusType = route,
                    SourceApplicationUser = sourceApplicationUser,
                    DegreeType = degreeType,
                    TrainingProvider = trainingProvider,
                    TrainingCountry = country,
                    TrainingSubject1 = trainingSubject1,
                    TrainingSubject2 = trainingSubject2,
                    TrainingSubject3 = trainingSubject3
                };
            }
        }
    }

    public IReadOnlyCollection<PreviousName> MapPreviousNames(
        Contact contact,
        IEnumerable<dfeta_previousname> dqtPreviousNames)
    {
        var nextFirstName = contact.FirstName ?? string.Empty;
        var nextMiddleName = contact.MiddleName ?? string.Empty;
        var nextLastName = contact.LastName ?? string.Empty;
        DateTime? nextNameTime = null;

        var toMap = dqtPreviousNames.ToList();

        List<PreviousName> result = new();

        var resolvedPreviousNames = new List<(dfeta_previousname DqtPreviousName, DateTime Created, string FirstName, string MiddleName, string LastName)>();

        foreach (var dqtPreviousName in toMap.OrderByDescending(pn => pn.dfeta_ChangedOn?.ToDateTimeWithDqtBstFix(true) ?? pn.CreatedOn))
        {
            if (dqtPreviousName.StateCode is not dfeta_previousnameState.Active)
            {
                continue;
            }

            var firstName = nextFirstName;
            var middleName = nextMiddleName;
            var lastName = nextLastName;

            if (dqtPreviousName.dfeta_Type is dfeta_NameType.FirstName)
            {
                firstName = dqtPreviousName.dfeta_name ?? string.Empty;
            }
            else if (dqtPreviousName.dfeta_Type is dfeta_NameType.MiddleName)
            {
                middleName = dqtPreviousName.dfeta_name ?? string.Empty;
            }
            else if (dqtPreviousName.dfeta_Type is dfeta_NameType.LastName)
            {
                lastName = dqtPreviousName.dfeta_name ?? string.Empty;
            }
            else
            {
                continue;
            }

            var changedOn = dqtPreviousName.dfeta_ChangedOn is DateTime dt
                ? DateTime.SpecifyKind(dt.ToDateTimeWithDqtBstFix(true), DateTimeKind.Utc)
                : dqtPreviousName.CreatedOn!.Value;

            resolvedPreviousNames.Add((dqtPreviousName, changedOn, firstName, middleName, lastName));

            nextFirstName = firstName;
            nextMiddleName = middleName;
            nextLastName = lastName;
        }

        // Collapse entries where nothing has actually changed
        var collapsed = resolvedPreviousNames.GroupAdjacent(pn => (pn.FirstName, pn.MiddleName, pn.LastName));

        foreach (var (dqtPreviousName, changedOn, firstName, middleName, lastName) in collapsed.Select(g => g.Last()))
        {
            if (changedOn > nextNameTime)
            {
                throw new InvalidOperationException(
                    $"Previous name from DQT '{dqtPreviousName.Id}' is after the last audited previous name for contact: '{contact.Id}'.");
            }

            var previousName = new PreviousName
            {
                PreviousNameId = Guid.NewGuid(),
                PersonId = contact.Id,
                CreatedOn = changedOn,
                UpdatedOn = changedOn,
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DqtPreviousNameIds = [dqtPreviousName.Id]
            };

            result.Add(previousName);
        }

        return result;
    }

    public sealed class RouteToProfessionalStatusInfo
    {
        public RouteToProfessionalStatus? ProfessionalStatus { get; init; }
        public RouteToProfessionalStatusType? RouteToProfessionalStatusType { get; init; }
        public ApplicationUser? SourceApplicationUser { get; init; }
        public DegreeType? DegreeType { get; init; }
        public TrainingProvider? TrainingProvider { get; init; }
        public Country? TrainingCountry { get; init; }
        public TrainingSubject? TrainingSubject1 { get; init; }
        public TrainingSubject? TrainingSubject2 { get; init; }
        public TrainingSubject? TrainingSubject3 { get; init; }
        public InductionExemptionReason? InductionExemptionReason { get; init; }
    }

    public sealed class ContactIttQtsMapResult
    {
        public required Guid ContactId { get; init; }
        public required IttQtsMapResult[] MappedResults { get; init; }
    }

    public sealed class IttQtsMapResult
    {
        private IttQtsMapResult()
        {
        }

        public bool Success { get; private set; }

        public RouteToProfessionalStatusInfo? ProfessionalStatusInfo { get; private set; }

        public Guid ContactId { get; private set; }

        public IttQtsMapResultFailedReason? FailedReason { get; private set; }

        public Guid? QtsRegistrationId { get; private set; }

        public Guid? IttId { get; private set; }

        public string? IttSlugId { get; private set; }

        public dfeta_teacherstatus? TeacherStatus { get; private set; }

        public DateOnly? QtsDate { get; private set; }

        public dfeta_earlyyearsstatus? EarlyYearsStatus { get; private set; }

        public DateOnly? EytsDate { get; private set; }

        public DateOnly? PartialRecognitionDate { get; private set; }

        public DateOnly? QtlsDate { get; private set; }

        public bool? QtlsDateHasBeenSet { get; private set; }

        public dfeta_ITTProgrammeType? ProgrammeType { get; private set; }

        public DateOnly? ProgrammeStartDate { get; private set; }

        public DateOnly? ProgrammeEndDate { get; private set; }

        public dfeta_ITTResult? IttResult { get; private set; }

        public dfeta_ittqualification? IttQualification { get; private set; }

        public Account? IttProvider { get; private set; }

        public dfeta_ittsubject? IttSubject1 { get; private set; }

        public dfeta_ittsubject? IttSubject2 { get; private set; }

        public dfeta_ittsubject? IttSubject3 { get; private set; }

        public dfeta_country? IttCountry { get; private set; }

        public RouteToProfessionalStatusType? StatusDerivedRoute { get; private set; }

        public RouteToProfessionalStatusType? ProgrammeTypeDerivedRoute { get; private set; }

        public RouteToProfessionalStatusType? IttQualificationDerivedRoute { get; private set; }

        public Guid[]? MultiplePotentialCompatibleIttRecords { get; private set; }

        public int ContactIttRowCount { get; private set; }

        public int ContactQtsRowCount { get; private set; }

        public Guid[]? InductionExemptionReasonIdsMovedFromPerson { get; set; }

        public static async Task<IttQtsMapResult> SucceededAsync(
            ReferenceDataCache referenceDataCache,
            RouteToProfessionalStatusInfo ps,
            dfeta_initialteachertraining? itt,
            dfeta_teacherstatus? teacherStatus,
            DateOnly? qtsDate,
            dfeta_earlyyearsstatus? earlyYearsStatus,
            DateOnly? eytsDate,
            DateOnly? partialRecognitionDate,
            DateOnly? qtlsDate,
            bool? qtlsDateHasBeenSet,
            dfeta_ittqualification? ittQualification,
            Guid? statusDerivedRouteId,
            Guid? programmeTypeDerivedRouteId,
            Guid? ittQualificationDerivedRouteId,
            Guid[]? multiplePotentialCompatibleIttRecords,
            int contactIttRowCount,
            int contactQtsRowCount)
        {
            var ittProvider = itt?.dfeta_EstablishmentId?.Id is not null
                ? await referenceDataCache.GetIttProviderByIdAsync(itt!.dfeta_EstablishmentId.Id)
                : null;

            var ittCountry = itt?.dfeta_CountryId?.Id is not null
                ? await referenceDataCache.GetCountryByIdAsync(itt!.dfeta_CountryId.Id)
                : null;

            var ittSubject1 = itt?.dfeta_Subject1Id?.Id is not null
                ? await referenceDataCache.GetIttSubjectBySubjectIdAsync(itt!.dfeta_Subject1Id.Id)
                : null;

            var ittSubject2 = itt?.dfeta_Subject2Id?.Id is not null
                ? await referenceDataCache.GetIttSubjectBySubjectIdAsync(itt!.dfeta_Subject2Id.Id)
                : null;

            var ittSubject3 = itt?.dfeta_Subject3Id?.Id is not null
                ? await referenceDataCache.GetIttSubjectBySubjectIdAsync(itt!.dfeta_Subject3Id.Id)
                : null;

            RouteToProfessionalStatusType? statusDerivedRoute = null;
            if (statusDerivedRouteId is not null)
            {
                statusDerivedRoute = await referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(statusDerivedRouteId.Value);
            }

            RouteToProfessionalStatusType? programmeTypeDerivedRoute = null;
            if (programmeTypeDerivedRouteId is not null)
            {
                programmeTypeDerivedRoute = await referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(programmeTypeDerivedRouteId.Value);
            }

            RouteToProfessionalStatusType? ittQualificationDerivedRoute = null;
            if (ittQualificationDerivedRouteId is not null)
            {
                ittQualificationDerivedRoute = await referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(ittQualificationDerivedRouteId.Value);
            }

            return new IttQtsMapResult()
            {
                Success = true,
                ProfessionalStatusInfo = ps,
                ContactId = ps.ProfessionalStatus!.PersonId,
                QtsRegistrationId = ps.ProfessionalStatus.DqtQtsRegistrationId,
                IttId = ps.ProfessionalStatus.DqtInitialTeacherTrainingId,
                IttSlugId = itt?.dfeta_SlugId,
                TeacherStatus = teacherStatus,
                QtsDate = qtsDate,
                EarlyYearsStatus = earlyYearsStatus,
                EytsDate = eytsDate,
                PartialRecognitionDate = partialRecognitionDate,
                QtlsDate = qtlsDate,
                QtlsDateHasBeenSet = qtlsDateHasBeenSet,
                ProgrammeType = itt?.dfeta_ProgrammeType,
                ProgrammeStartDate = itt?.dfeta_ProgrammeStartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                ProgrammeEndDate = itt?.dfeta_ProgrammeEndDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                IttResult = itt?.dfeta_Result,
                IttProvider = ittProvider,
                IttSubject1 = ittSubject1,
                IttSubject2 = ittSubject2,
                IttSubject3 = ittSubject3,
                IttCountry = ittCountry,
                IttQualification = ittQualification,
                StatusDerivedRoute = statusDerivedRoute,
                ProgrammeTypeDerivedRoute = programmeTypeDerivedRoute,
                IttQualificationDerivedRoute = ittQualificationDerivedRoute,
                MultiplePotentialCompatibleIttRecords = multiplePotentialCompatibleIttRecords,
                ContactIttRowCount = contactIttRowCount,
                ContactQtsRowCount = contactQtsRowCount
            };
        }

        public static async Task<IttQtsMapResult> FailedAsync(
            ReferenceDataCache referenceDataCache,
            Guid contactId,
            IttQtsMapResultFailedReason reason,
            Guid? qtsRegistrationId,
            dfeta_initialteachertraining? itt,
            dfeta_teacherstatus? teacherStatus,
            DateOnly? qtsDate,
            dfeta_earlyyearsstatus? earlyYearsStatus,
            DateOnly? eytsDate,
            DateOnly? partialRecognitionDate,
            DateOnly? qtlsDate,
            bool? qtlsDateHasBeenSet,
            dfeta_ittqualification? ittQualification,
            Guid? statusDerivedRouteId,
            Guid? programmeTypeDerivedRouteId,
            Guid? ittQualificationDerivedRouteId,
            Guid[]? multiplePotentialCompatibleIttRecords,
            int contactIttRowCount,
            int contactQtsRowCount)
        {
            var ittProvider = itt?.dfeta_EstablishmentId?.Id is not null
                ? await referenceDataCache.GetIttProviderByIdAsync(itt!.dfeta_EstablishmentId.Id)
                : null;

            var ittCountry = itt?.dfeta_CountryId?.Id is not null
                ? await referenceDataCache.GetCountryByIdAsync(itt!.dfeta_CountryId.Id)
                : null;

            var ittSubject1 = itt?.dfeta_Subject1Id?.Id is not null
                ? await referenceDataCache.GetIttSubjectBySubjectIdAsync(itt!.dfeta_Subject1Id.Id)
                : null;

            var ittSubject2 = itt?.dfeta_Subject2Id?.Id is not null
                ? await referenceDataCache.GetIttSubjectBySubjectIdAsync(itt!.dfeta_Subject2Id.Id)
                : null;

            var ittSubject3 = itt?.dfeta_Subject3Id?.Id is not null
                ? await referenceDataCache.GetIttSubjectBySubjectIdAsync(itt!.dfeta_Subject3Id.Id)
                : null;

            RouteToProfessionalStatusType? statusDerivedRoute = null;
            if (statusDerivedRouteId is not null)
            {
                statusDerivedRoute = await referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(statusDerivedRouteId.Value);
            }

            RouteToProfessionalStatusType? programmeTypeDerivedRoute = null;
            if (programmeTypeDerivedRouteId is not null)
            {
                programmeTypeDerivedRoute = await referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(programmeTypeDerivedRouteId.Value);
            }

            RouteToProfessionalStatusType? ittQualificationDerivedRoute = null;
            if (ittQualificationDerivedRouteId is not null)
            {
                ittQualificationDerivedRoute = await referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(ittQualificationDerivedRouteId.Value);
            }

            return new IttQtsMapResult()
            {
                Success = false,
                ContactId = contactId,
                FailedReason = reason,
                QtsRegistrationId = qtsRegistrationId,
                IttId = itt?.Id,
                IttSlugId = itt?.dfeta_SlugId,
                TeacherStatus = teacherStatus,
                QtsDate = qtsDate,
                EarlyYearsStatus = earlyYearsStatus,
                EytsDate = eytsDate,
                PartialRecognitionDate = partialRecognitionDate,
                QtlsDate = qtlsDate,
                QtlsDateHasBeenSet = qtlsDateHasBeenSet,
                ProgrammeType = itt?.dfeta_ProgrammeType,
                ProgrammeEndDate = itt?.dfeta_ProgrammeEndDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                ProgrammeStartDate = itt?.dfeta_ProgrammeStartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                IttResult = itt?.dfeta_Result,
                IttProvider = ittProvider,
                IttSubject1 = ittSubject1,
                IttSubject2 = ittSubject2,
                IttSubject3 = ittSubject3,
                IttCountry = ittCountry,
                IttQualification = ittQualification,
                StatusDerivedRoute = statusDerivedRoute,
                ProgrammeTypeDerivedRoute = programmeTypeDerivedRoute,
                IttQualificationDerivedRoute = ittQualificationDerivedRoute,
                MultiplePotentialCompatibleIttRecords = multiplePotentialCompatibleIttRecords,
                ContactIttRowCount = contactIttRowCount,
                ContactQtsRowCount = contactQtsRowCount
            };
        }
    }

    public enum IttQtsMapResultFailedReason
    {
        CannotMapStatus,
        PassResultHasNoAwardDate,
        ApprovedResultHasNoAwardDate,
        CannotDeriveRoute,
        PartialRecognitionIsInvalid,
        PotentialDuplicateIttRecords,
        DoNotMigrateIttResult,
        DoNotMigrateNoQualificationRestrictedByGtc,
        DoNotMigrateQtsRegistrationHasNoStatus,
        DoNotMigrateQtsAwardedInWalesDuplicateIttRecords,
        UnmappableIttCountryId,
        UnmappableIttSubjectId
    }

    private class IttQtsMapException(IttQtsMapResult result) : Exception
    {
        public IttQtsMapResult Result { get; } = result;
    }

    private record ModelTypeSyncInfo
    {
        public required string? CreateTempTableStatement { get; init; }
        public required string? CopyStatement { get; init; }
        public required string? UpsertStatement { get; init; }
        public required string? DeleteStatement { get; init; }
        public required bool IgnoreDeletions { get; init; }
        public required string? GetLastModifiedOnStatement { get; init; }
        public required string EntityLogicalName { get; init; }
        public required string[] AttributeNames { get; init; }
        public required Func<TrsDataSyncHelper, SyncEntitiesHandler> GetSyncHandler { get; init; }
    }

    private record ModelTypeSyncInfo<TModel> : ModelTypeSyncInfo
    {
        public required Action<NpgsqlBinaryImporter, TModel>? WriteRecord { get; init; }
    }

    private record EntityVersionInfo<TEntity>(
        Guid Id,
        TEntity Entity,
        string[] ChangedAttributes,
        DateTime Timestamp,
        Guid UserId,
        string UserName)
        where TEntity : Entity;

    public record PersonInfo
    {
        public required Guid PersonId { get; init; }
        public required DateTime? CreatedOn { get; init; }
        public required DateTime? UpdatedOn { get; init; }
        public required PersonStatus Status { get; init; }
        public required Guid? MergedWithPersonId { get; init; }
        public required string? Trn { get; init; }
        public required string FirstName { get; init; }
        public required string MiddleName { get; init; }
        public required string LastName { get; init; }
        public required DateOnly? DateOfBirth { get; init; }
        public required string? EmailAddress { get; init; }
        public required string? NationalInsuranceNumber { get; init; }
        public required Gender? Gender { get; init; }
        public required Guid? DqtContactId { get; init; }
        public required int? DqtState { get; init; }
        public required DateTime? DqtCreatedOn { get; init; }
        public required DateTime? DqtModifiedOn { get; init; }
        public required string? DqtFirstName { get; init; }
        public required string? DqtMiddleName { get; init; }
        public required string? DqtLastName { get; init; }
        public required bool CreatedByTps { get; init; }
    }

    private record InductionInfo
    {
        public required Guid PersonId { get; init; }
        public required Guid? InductionId { get; init; }
        public required DateOnly? InductionCompletedDate { get; init; }
        public required Guid[] InductionExemptionReasonIds { get; init; }
        public required DateOnly? InductionStartDate { get; init; }
        public required InductionStatus InductionStatus { get; init; }
        public required DateTime? DqtModifiedOn { get; init; }
        public required bool InductionExemptWithoutReason { get; init; }
    }

    private record DqtNoteInfo
    {
        public required Guid? Id { get; set; }
        public required Guid PersonId { get; set; }
        public required string? ContentHtml { get; set; }
        public required DateTime CreatedOn { get; set; }
        public required Guid CreatedByDqtUserId { get; set; }
        public required string? CreatedByDqtUserName { get; set; }
        public required DateTime? UpdatedOn { get; set; }
        public required Guid? UpdatedByDqtUserId { get; set; }
        public required string? UpdatedByDqtUserName { get; set; }
        public required string? FileName { get; set; }
        public required byte[]? AttachmentBytes { get; set; }
        public required string? OriginalFileName { get; set; }
        public required string? MimeType { get; set; }

    }

    private record AuditInfo<TEntity>
    {
        public required string[] AllChangedAttributes { get; init; }
        public required string[] RelevantChangedAttributes { get; init; }
        public required TEntity NewValue { get; init; }
        public required TEntity OldValue { get; init; }
        public required Audit AuditRecord { get; init; }
    }

    public static class ModelTypes
    {
        public const string Person = "Person";
        public const string Event = "Event";
        public const string Induction = "Induction";
        public const string DqtNote = "Annotation";
        public const string Route = "Route";
        public const string PreviousName = "PreviousName";
    }

    private enum RouteMappingPrecedence
    {
        TeachingStatus,
        ProgrammeType,
        IttQualification
    }
}

file static class Extensions
{
    public static TEntity ShallowClone<TEntity>(this TEntity entity) where TEntity : Entity
    {
        // N.B. This only clones Attributes

        var cloned = new Entity(entity.LogicalName, entity.Id);

        foreach (var attr in entity.Attributes)
        {
            cloned.Attributes.Add(attr.Key, attr.Value);
        }

        return cloned.ToEntity<TEntity>();
    }

    public static TEntity SparseClone<TEntity>(this TEntity entity, string[] attributeNames) where TEntity : Entity
    {
        // N.B. This only clones Attributes in the whitelist
        var cloned = new Entity(entity.LogicalName, entity.Id);

        foreach (var attr in entity.Attributes.Where(kvp => attributeNames.Contains(kvp.Key)))
        {
            cloned.Attributes.Add(attr.Key, attr.Value);
        }

        return cloned.ToEntity<TEntity>();
    }

    /// <summary>
    /// Returns <c>null</c> if <paramref name="value"/> is empty or whitespace.
    /// </summary>
    public static string? NormalizeString(this string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static readonly TimeZoneInfo _gmt = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

    public static DateTime ToDateTimeWithDqtBstFix(this DateTime dateTime, bool isLocalTime) =>
        isLocalTime ? TimeZoneInfo.ConvertTimeFromUtc(dateTime, _gmt) : dateTime;
}
