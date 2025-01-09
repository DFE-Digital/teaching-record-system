using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class RouteToProfessionalStatusMapping : IEntityTypeConfiguration<RouteToProfessionalStatus>
{
    public void Configure(EntityTypeBuilder<RouteToProfessionalStatus> builder)
    {
        builder.ToTable("routes_to_professional_status");
        builder.Property(x => x.QualificationType).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();

        builder.HasData(
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("6F27BDEB-D00A-4EF9-B0EA-26498CE64713"), Name = "Apply for QTS", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("6987240E-966E-485F-B300-23B54937FB3A"), Name = "Apprenticeship", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("57B86CEF-98E2-4962-A74A-D47C7A34B838"), Name = "Assessment Only Route", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("4B6FC697-BE67-43D3-9021-CC662C4A559F"), Name = "Authorised Teacher Programme", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("4163C2FB-6163-409F-85FD-56E7C70A54DD"), Name = "Core - Core Programme Type", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("4BD7A9F0-28CA-4977-A044-A7B7828D469B"), Name = "Core Flexible", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("5748D41D-7B53-4EE6-833A-83080A3BD8EF"), Name = "CTC or CCTA", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("D9EEF3F8-FDE6-4A3F-A361-F6655A42FA1E"), Name = "Early Years ITT Assessment Only", QualificationType = QualificationType.EarlyYearsTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("4477E45D-C531-4C63-9F4B-E157766366FB"), Name = "Early Years ITT Graduate Employment Based", QualificationType = QualificationType.EarlyYearsTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("DBC4125B-9235-41E4-ABD2-BAABBF63F829"), Name = "Early Years ITT Graduate Entry", QualificationType = QualificationType.EarlyYearsTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("7F09002C-5DAD-4839-9693-5E030D037AE9"), Name = "Early Years ITT School Direct", QualificationType = QualificationType.EarlyYearsTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("C97C0FD2-FD84-4949-97C7-B0E2422FB3C8"), Name = "Early Years ITT Undergraduate", QualificationType = QualificationType.EarlyYearsTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("F4DA123B-5C37-4060-AB00-52DE4BD3599E"), Name = "EC directive", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("2B106B9D-BA39-4E2D-A42E-0CE827FDC324"), Name = "European Recognition", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("EC95C276-25D9-491F-99A2-8D92F10E1E94"), Name = "European Recognition - PQTS", QualificationType = QualificationType.PartialQualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("8F5C0431-D006-4EDA-9336-16DFC6A26A78"), Name = "EYPS", QualificationType = QualificationType.EarlyYearsProfessionalStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("EBA0B7AE-CBCE-44D5-A56F-988D69B03001"), Name = "EYPS ITT Migrated", QualificationType = QualificationType.EarlyYearsProfessionalStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("5B7D1C4E-FB2B-479C-BDEE-5818DAAA8A07"), Name = "EYTS ITT Migrated", QualificationType = QualificationType.EarlyYearsTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("45C93B5B-B4DC-4D0F-B0DE-D612521E0A13"), Name = "FE Recognition 2000-2004", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("700EC96F-6BBF-4080-87BD-94EF65A6A879"), Name = "Flexible ITT", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("F85962C9-CF0C-415D-9DE5-A397F95AE261"), Name = "Future Teaching Scholars", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("A6431D4B-E4CD-4E59-886B-358221237E75"), Name = "Graduate non-trained", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("34222549-ED59-4C4A-811D-C0894E78D4C3"), Name = "Graduate Teacher Programme", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("10078157-E8C3-42F7-A050-D8B802E83F7B"), Name = "HEI - HEI Programme Type", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("32017D68-9DA4-43B2-AE91-4F24C68F6F78"), Name = "HEI - Historic", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("BFEF20B2-5AC4-486D-9493-E5A4538E1BE9"), Name = "High Potential ITT", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("D0B60864-AB1C-4D49-A5C2-FF4BD9872EE1"), Name = "International Qualified Teacher Status", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("4514EC65-20B0-4465-B66F-4718963C5B80"), Name = "Legacy ITT", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("C80CB763-0D61-4CF1-A749-37C1D0AB85F8"), Name = "Legacy Migration", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("2B4862CA-BD30-4A3A-BFCE-52B57C2946C7"), Name = "Licensed Teacher Programme", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("FC16290C-AC1E-4830-B7E9-35708F1BDED3"), Name = "Licensed Teacher Programme - Armed Forces", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("D5EB09CC-C64F-45DF-A46D-08277A25DE7A"), Name = "Licensed Teacher Programme - FE", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("64C28594-4B63-42B3-8B47-E3F140879E66"), Name = "Licensed Teacher Programme - Independent School", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("E5C198FA-35F0-4A13-9D07-8B0239B4957A"), Name = "Licensed Teacher Programme - Maintained School", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("779BD3C6-6B3A-4204-9489-1BBB381B52BF"), Name = "Licensed Teacher Programme - OTT", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("AA1EFD16-D59C-4E18-A496-16E39609B389"), Name = "Long Service", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("3604EF30-8F11-4494-8B52-A2F9C5371E03"), Name = "NI R", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("88867B43-897B-49B5-97CC-F4F81A1D5D44"), Name = "Other Qualifications non ITT", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("51756384-CFEA-4F63-80E5-F193686E0F71"), Name = "Overseas Trained Teacher Programme", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("CE61056E-E681-471E-AF48-5FFBF2653500"), Name = "Overseas Trained Teacher Recognition", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("F5390BE5-8336-4951-B97B-5B45D00B7A76"), Name = "PGATC ITT", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("1C626BE0-5A64-47EC-8349-75008F52BC2C"), Name = "PGATD ITT", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("02A2135C-AC34-4481-A293-8A00AAB7EE69"), Name = "PGCE ITT", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("7721655F-165F-4737-97D4-17FC6991C18C"), Name = "PGDE ITT", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("EF46FF51-8DC0-481E-B158-61CCEA9943D9"), Name = "Primary and secondary postgraduate fee funded", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("321D5F9A-9581-4936-9F63-CFDDD2A95FE2"), Name = "Primary and secondary undergraduate fee funded", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("002F7C96-F6AE-4E67-8F8B-D2F1C1317273"), Name = "ProfGCE ITT", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("9A6F368F-06E7-4A74-B269-6886C48A49DA"), Name = "ProfGDE ITT", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("97497716-5AC5-49AA-A444-27FA3E2C152A"), Name = "Provider led Postgrad", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("53A7FBDA-25FD-4482-9881-5CF65053888D"), Name = "Provider led Undergrad", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("BE6EAF8C-92DD-4EFF-AAD3-1C89C4BEC18C"), Name = "QTLS and SET Membership", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("70368FF2-8D2B-467E-AD23-EFE7F79995D7"), Name = "Registered Teacher Programme", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("ABCB0A14-0C21-4598-A42C-A007D4B048AC"), Name = "School Centered ITT", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("D9490E58-ACDC-4A38-B13E-5A5C21417737"), Name = "School Direct Training Programme", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("12A742C3-1CD4-43B7-A2FA-1000BD4CC373"), Name = "School Direct Training Programme Salaried", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("97E1811B-D46C-483E-AEC3-4A2DD51A55FE"), Name = "School Direct Training Programme Self Funded", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("52835B1F-1F2E-4665-ABC6-7FB1EF0A80BB"), Name = "Scotland R", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("BED14B00-5D08-4580-83B5-86D71A4F1A24"), Name = "TC ITT", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("82AA14D3-EF6A-4B46-A10C-DC850DDCEF5F"), Name = "TCMH", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("5B7F5E90-1CA6-4529-BAA0-DFBA68E698B8"), Name = "Teach First Programme", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("50D18F17-EE26-4DAD-86CA-1AAE3F956BFC"), Name = "Troops to Teach", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("7C04865F-FA39-458A-BC39-07DD46B88154"), Name = "UGMT ITT", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("20F67E38-F117-4B42-BBFC-5812AA717B94"), Name = "Undergraduate Opt In", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true },
            new RouteToProfessionalStatus { RouteToProfessionalStatusId = new("877BA701-FE26-4951-9F15-171F3755D50D"), Name = "Welsh R", QualificationType = QualificationType.QualifiedTeacherStatus, IsActive = true });
    }
}
