using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class AlertTypeMapping : IEntityTypeConfiguration<AlertType>
{
    public void Configure(EntityTypeBuilder<AlertType> builder)
    {
        builder.ToTable("alert_types");
        builder.HasKey(x => x.AlertTypeId);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(AlertType.NameMaxLength).UseCollation("case_insensitive");
        builder.Property(x => x.DqtSanctionCode).HasMaxLength(AlertType.DqtSanctionCodeMaxLength).UseCollation("case_insensitive");
        builder.Property(x => x.ProhibitionLevel).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Ignore(x => x.IsDbsAlertType);
        builder.HasIndex(x => x.AlertCategoryId).HasDatabaseName(AlertType.AlertCategoryIdIndexName);
        builder.HasIndex(x => new { x.AlertCategoryId, x.DisplayOrder }).HasDatabaseName(AlertType.DisplayOrderIndexName).IsUnique().HasFilter("display_order is not null and is_active = true");
        builder.HasOne<AlertCategory>(x => x.AlertCategory).WithMany(c => c.AlertTypes).HasForeignKey(x => x.AlertCategoryId).HasConstraintName(AlertType.AlertCategoryForeignKeyName);
        builder.HasData(
            new AlertType { AlertTypeId = new Guid("2ca98658-1d5b-49d5-b05f-cc08c8b8502c"), AlertCategoryId = new Guid("ee78d44d-abf8-44a9-b22b-87a821f8d3c9"), Name = "Teacher sanctioned in other EEA member state", DqtSanctionCode = "T8", ProhibitionLevel = ProhibitionLevel.Notify, InternalOnly = true, IsActive = true, DisplayOrder = 1 },
            new AlertType { AlertTypeId = new Guid("9fafaa80-f9f8-44a0-b7b3-cffedcbe0298"), AlertCategoryId = new Guid("0ae0707b-1503-477d-bc0f-1505ed95dbdf"), Name = "Failed induction", DqtSanctionCode = "C2", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = false, IsActive = true, DisplayOrder = 1 },
            new AlertType { AlertTypeId = new Guid("651e1f56-3135-4961-bd7e-3f7b2c75cb04"), AlertCategoryId = new Guid("0ae0707b-1503-477d-bc0f-1505ed95dbdf"), Name = "Prohibited by the Secretary of State - failed probation", DqtSanctionCode = "C1", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = false, IsActive = false },
            new AlertType { AlertTypeId = new Guid("5ea8bb68-4774-4ad8-b635-213a0cdda4c3"), AlertCategoryId = new Guid("0ae0707b-1503-477d-bc0f-1505ed95dbdf"), Name = "Restricted by the Secretary of State - failed probation - permitted to carry out specified work for a period equal in length to a statutory induction period only", DqtSanctionCode = "C3", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = false, IsActive = false },
            new AlertType { AlertTypeId = new Guid("ae3e385d-03f8-4f12-9ce2-006afe827d23"), AlertCategoryId = new Guid("768c9eb4-355b-4491-bb20-67eb59a97579"), Name = "FOR INTERNAL INFORMATION ONLY - see alert details", DqtSanctionCode = "T9", ProhibitionLevel = ProhibitionLevel.None, InternalOnly = true, IsActive = true, DisplayOrder = 1 },
            new AlertType { AlertTypeId = new Guid("12435c00-88cb-406b-b2b8-7400c1ced7b8"), AlertCategoryId = new Guid("768c9eb4-355b-4491-bb20-67eb59a97579"), Name = "FOR INTERNAL USER ONLY – known duplicate record", DqtSanctionCode = "T10", ProhibitionLevel = ProhibitionLevel.None, InternalOnly = true, IsActive = true, DisplayOrder = 2 },
            new AlertType { AlertTypeId = new Guid("a6fc9f2e-8923-4163-978e-93bd901d146f"), AlertCategoryId = new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), Name = "Conditional Registration Order - conviction of a relevant offence", DqtSanctionCode = "A18", ProhibitionLevel = ProhibitionLevel.Restrict, InternalOnly = false, IsActive = false },
            new AlertType { AlertTypeId = new Guid("1ebd1620-293d-4169-ba78-0b41a6413ad9"), AlertCategoryId = new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), Name = "Conditional Registration Order - serious professional incompetence", DqtSanctionCode = "A7", ProhibitionLevel = ProhibitionLevel.Restrict, InternalOnly = false, IsActive = false },
            new AlertType { AlertTypeId = new Guid("3499860a-a0fb-43e3-878e-c226d14150b0"), AlertCategoryId = new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), Name = "Conditional Registration Order - unacceptable professional conduct", DqtSanctionCode = "A3", ProhibitionLevel = ProhibitionLevel.Restrict, InternalOnly = false, IsActive = false },
            new AlertType { AlertTypeId = new Guid("552ee226-a3a9-4dc3-8d04-0b7e4f641b51"), AlertCategoryId = new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), Name = "For internal information only - historic GTC finding of unsuitable for registration", DqtSanctionCode = "A15", ProhibitionLevel = ProhibitionLevel.None, InternalOnly = true, IsActive = false },
            new AlertType { AlertTypeId = new Guid("33e00e46-6513-4136-adfd-1352cf34d8ec"), AlertCategoryId = new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), Name = "No Sanction - breach of condition(s)", DqtSanctionCode = "A22", ProhibitionLevel = ProhibitionLevel.None, InternalOnly = true, IsActive = false },
            new AlertType { AlertTypeId = new Guid("0740f9eb-ece3-4394-a230-453da224d337"), AlertCategoryId = new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), Name = "No Sanction - conviction for a relevant offence", DqtSanctionCode = "A16", ProhibitionLevel = ProhibitionLevel.None, InternalOnly = true, IsActive = false },
            new AlertType { AlertTypeId = new Guid("b6c8d8f1-723e-49a5-9551-25805e3e29b9"), AlertCategoryId = new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), Name = "No Sanction - serious professional incompetence", DqtSanctionCode = "A12", ProhibitionLevel = ProhibitionLevel.None, InternalOnly = true, IsActive = false },
            new AlertType { AlertTypeId = new Guid("78f88de2-9ec1-41b8-948a-33bdff223206"), AlertCategoryId = new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), Name = "No Sanction - unacceptable professional conduct", DqtSanctionCode = "A11", ProhibitionLevel = ProhibitionLevel.None, InternalOnly = true, IsActive = false },
            new AlertType { AlertTypeId = new Guid("fcff87d6-88f5-4fc5-ac81-5350b4fdd9e1"), AlertCategoryId = new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), Name = "Reprimand - conviction of a relevant offence", DqtSanctionCode = "A17", ProhibitionLevel = ProhibitionLevel.None, InternalOnly = true, IsActive = false },
            new AlertType { AlertTypeId = new Guid("3f7de5fd-05a8-404f-a97c-428f54e81322"), AlertCategoryId = new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), Name = "Reprimand - serious professional incompetence", DqtSanctionCode = "A8", ProhibitionLevel = ProhibitionLevel.None, InternalOnly = true, IsActive = false },
            new AlertType { AlertTypeId = new Guid("0ae8d4b6-ec9b-47ca-9338-6dae9192afe5"), AlertCategoryId = new Guid("06d98708-b52d-496a-aaa7-c1d7d2ca8b24"), Name = "Reprimand - unacceptable professional conduct", DqtSanctionCode = "A4", ProhibitionLevel = ProhibitionLevel.None, InternalOnly = true, IsActive = false },
            new AlertType { AlertTypeId = new Guid("72e48b6a-e781-4bf3-910b-91f2d28f2eaa"), AlertCategoryId = new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), Name = "Prohibition Order - conviction of a relevant offence - eligible to reapply after specified time", DqtSanctionCode = "A21B", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = false, IsActive = false },
            new AlertType { AlertTypeId = new Guid("950d3eed-bef5-448a-b0f0-bf9c54f2103b"), AlertCategoryId = new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), Name = "Prohibition Order - conviction of a relevant offence - ineligible to reapply", DqtSanctionCode = "A21A", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = false, IsActive = false },
            new AlertType { AlertTypeId = new Guid("cac68337-3f95-4475-97cf-1381e6b74700"), AlertCategoryId = new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), Name = "Prohibition Order - serious professional incompetence - Eligible to reapply after specified time", DqtSanctionCode = "A5B", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = false, IsActive = false },
            new AlertType { AlertTypeId = new Guid("c02bdc3a-7a19-4034-aa23-3a23c54e1d34"), AlertCategoryId = new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), Name = "Prohibition Order - serious professional incompetence - Ineligible to reapply", DqtSanctionCode = "A5A", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = false, IsActive = false },
            new AlertType { AlertTypeId = new Guid("e3658a61-bee2-4df1-9a26-e010681ee310"), AlertCategoryId = new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), Name = "Prohibition Order - unacceptable professional conduct - Eligible to reapply after specified time", DqtSanctionCode = "A1B", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = false, IsActive = false },
            new AlertType { AlertTypeId = new Guid("fa6bd220-61b0-41fc-9066-421b3b9d7885"), AlertCategoryId = new Guid("70b7d473-2ec8-4643-bfd4-d4ab9a9a0988"), Name = "Prohibition Order - unacceptable professional conduct - Ineligible to reapply", DqtSanctionCode = "A1A", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = false, IsActive = false },
            new AlertType { AlertTypeId = new Guid("d372fcfa-1c4a-4fed-84c8-4c7885575681"), AlertCategoryId = new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), Name = "Suspension order - conviction of a relevant offence - with conditions", DqtSanctionCode = "A20", ProhibitionLevel = ProhibitionLevel.Restrict, InternalOnly = false, IsActive = false },
            new AlertType { AlertTypeId = new Guid("af65c236-47a6-427b-8e4b-930de6d256f0"), AlertCategoryId = new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), Name = "Suspension order - conviction of a relevant offence - without conditions", DqtSanctionCode = "A19", ProhibitionLevel = ProhibitionLevel.Restrict, InternalOnly = false, IsActive = false },
            new AlertType { AlertTypeId = new Guid("50508749-7a6b-4175-8538-9a1e55692efd"), AlertCategoryId = new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), Name = "Suspension order - serious professional incompetence - with conditions", DqtSanctionCode = "A14", ProhibitionLevel = ProhibitionLevel.Restrict, InternalOnly = false, IsActive = false },
            new AlertType { AlertTypeId = new Guid("a6f51ccc-a19c-4dc2-ba80-ffb7a95ff2ee"), AlertCategoryId = new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), Name = "Suspension order - serious professional incompetence - without conditions", DqtSanctionCode = "A6", ProhibitionLevel = ProhibitionLevel.Restrict, InternalOnly = false, IsActive = false },
            new AlertType { AlertTypeId = new Guid("1a2b06ae-7e9f-4761-b95d-397ca5da4b13"), AlertCategoryId = new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), Name = "Suspension order - unacceptable professional conduct - with conditions", DqtSanctionCode = "A13", ProhibitionLevel = ProhibitionLevel.Restrict, InternalOnly = false, IsActive = false },
            new AlertType { AlertTypeId = new Guid("872d7700-aa6f-435e-b5f9-821fb087962a"), AlertCategoryId = new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), Name = "Suspension order - unacceptable professional conduct - without conditions", DqtSanctionCode = "A2", ProhibitionLevel = ProhibitionLevel.Restrict, InternalOnly = false, IsActive = false },
            new AlertType { AlertTypeId = new Guid("17b4fe26-7468-4702-92e5-785b861cf0fa"), AlertCategoryId = new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), Name = "Suspension order - with conditions - (arising from breach of previous condition(s))", DqtSanctionCode = "A24", ProhibitionLevel = ProhibitionLevel.Restrict, InternalOnly = false, IsActive = false },
            new AlertType { AlertTypeId = new Guid("3c5fc83b-10e1-4a15-83e6-794fce3e0b45"), AlertCategoryId = new Guid("790410c1-b884-4cdd-8db9-64a042ab54ae"), Name = "Suspension order - without conditions - (arising from breach of previous condition(s))", DqtSanctionCode = "A23", ProhibitionLevel = ProhibitionLevel.Restrict, InternalOnly = false, IsActive = false },
            new AlertType { AlertTypeId = new Guid("eab8b66d-68d0-4cb9-8e4d-bbd245648fb6"), AlertCategoryId = new Guid("b2b19019-b165-47a3-8745-3297ff152581"), Name = "Barring by the Secretary of State", DqtSanctionCode = "B1", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = true, IsActive = false },
            new AlertType { AlertTypeId = new Guid("2c496e3f-00d3-4f0d-81f3-21458fe707b3"), AlertCategoryId = new Guid("b2b19019-b165-47a3-8745-3297ff152581"), Name = "Formerly barred by the Independent Safeguarding Authority", DqtSanctionCode = "G2", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = true, IsActive = false },
            new AlertType { AlertTypeId = new Guid("993daa42-96cb-4621-bd9e-d4b195076bbe"), AlertCategoryId = new Guid("b2b19019-b165-47a3-8745-3297ff152581"), Name = "Formerly on List 99", DqtSanctionCode = "B6", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = true, IsActive = false },
            new AlertType { AlertTypeId = new Guid("a414283f-7d5b-4587-83bf-f6da8c05b8d5"), AlertCategoryId = new Guid("b2b19019-b165-47a3-8745-3297ff152581"), Name = "Interim prohibition by the Secretary of State", DqtSanctionCode = "T2", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = false, IsActive = true, DisplayOrder = 1 },
            new AlertType { AlertTypeId = new Guid("ed0cd700-3fb2-4db0-9403-ba57126090ed"), AlertCategoryId = new Guid("b2b19019-b165-47a3-8745-3297ff152581"), Name = "Prohibition by the Secretary of State - misconduct", DqtSanctionCode = "T1", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = false, IsActive = true, DisplayOrder = 2 },
            new AlertType { AlertTypeId = new Guid("8ef92c14-4b1f-4530-9189-779ad9f3cefd"), AlertCategoryId = new Guid("b2b19019-b165-47a3-8745-3297ff152581"), Name = "Prohibited by an Independent Schools Tribunal or Secretary of State", DqtSanctionCode = "B3", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = false, IsActive = false },
            new AlertType { AlertTypeId = new Guid("50feafbc-5124-4189-b06c-6463c7ebb8a8"), AlertCategoryId = new Guid("b2b19019-b165-47a3-8745-3297ff152581"), Name = "Prohibition by the Secretary of State - deregistered by GTC Scotland", DqtSanctionCode = "T3", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = false, IsActive = true, DisplayOrder = 3 },
            new AlertType { AlertTypeId = new Guid("5aa21b8f-2069-43c9-8afd-05b34b02505f"), AlertCategoryId = new Guid("b2b19019-b165-47a3-8745-3297ff152581"), Name = "Prohibition by the Secretary of State - refer to GTC Northern Ireland", DqtSanctionCode = "T5", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = false, IsActive = true, DisplayOrder = 4 },
            new AlertType { AlertTypeId = new Guid("a5bd4352-2cec-4417-87a1-4b6b79d033c2"), AlertCategoryId = new Guid("b2b19019-b165-47a3-8745-3297ff152581"), Name = "Prohibition by the Secretary of State - refer to the Education Workforce Council, Wales", DqtSanctionCode = "T4", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = false, IsActive = true, DisplayOrder = 5 },
            new AlertType { AlertTypeId = AlertType.DbsAlertTypeId, AlertCategoryId = new Guid("e8a9ee91-bf7f-4f70-bc66-a644d522384e"), Name = "A possible matching record was found. Please contact the DBS before employing this person", DqtSanctionCode = "G1", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = true, IsActive = true, DisplayOrder = 1 },
            new AlertType { AlertTypeId = new Guid("38db7946-2dbf-408e-bc48-1625829e7dfe"), AlertCategoryId = new Guid("cbf7633f-3904-407d-8371-42a473fa641f"), Name = "Restricted by the Secretary of State - Not Permitted to work as teacher", DqtSanctionCode = "B2B", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = true, IsActive = false },
            new AlertType { AlertTypeId = new Guid("18e04dcb-fb86-4b05-8d5d-ff9c5da738dd"), AlertCategoryId = new Guid("cbf7633f-3904-407d-8371-42a473fa641f"), Name = "Restricted by the Secretary of State - Permitted to work as teacher", DqtSanctionCode = "B2A", ProhibitionLevel = ProhibitionLevel.Teaching, InternalOnly = true, IsActive = false },
            new AlertType { AlertTypeId = new Guid("241eeb78-fac7-4c77-8059-c12e93dc2fae"), AlertCategoryId = new Guid("38df5a00-94ab-486f-8905-d5b2eac04000"), Name = "Section 128 barring direction", DqtSanctionCode = "T7", ProhibitionLevel = ProhibitionLevel.LeadershipPositions, InternalOnly = false, IsActive = true, DisplayOrder = 1 },
            new AlertType { AlertTypeId = new Guid("7924fe90-483c-49f8-84fc-674feddba848"), AlertCategoryId = new Guid("227b75e5-bb98-496c-8860-1baea37aa5c6"), Name = "Secretary of State decision - no prohibition", DqtSanctionCode = "T6", ProhibitionLevel = ProhibitionLevel.None, InternalOnly = false, IsActive = true, DisplayOrder = 1 });
    }
}
