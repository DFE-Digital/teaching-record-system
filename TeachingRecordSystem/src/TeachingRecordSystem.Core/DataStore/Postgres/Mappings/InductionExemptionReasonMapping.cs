using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Mappings;

public class InductionExemptionReasonMapping : IEntityTypeConfiguration<InductionExemptionReason>
{
    public void Configure(EntityTypeBuilder<InductionExemptionReason> builder)
    {
        builder.ToTable("induction_exemption_reasons");
        builder.Property(x => x.IsActive).IsRequired();

        builder.HasData(
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.QualifiedBefore7thMay2000Id, Name = "Qualified before 07 May 2000", IsActive = true, RouteImplicitExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = new("15014084-2d8d-4f51-9198-b0e1881f8896"), Name = "Qualified between 07 May 1999 and 01 Apr 2003. First post was in Wales and lasted a minimum of two terms.", IsActive = true, RouteImplicitExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = new("0997ab13-7412-4560-8191-e51ed4d58d2a"), Name = "Qualified through Further Education route between 1 Sep 2001 and 1 Sep 2004", IsActive = true, RouteImplicitExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = new("fea2db23-93e0-49af-96fd-83c815c17c0b"), Name = "Passed induction in Guernsey", IsActive = true, RouteImplicitExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = new("e5c3847d-8fb6-4b31-8726-812392da8c5c"), Name = "Passed induction in Isle of Man", IsActive = true, RouteImplicitExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = new("243b21a8-0be4-4af5-8874-85944357e7f8"), Name = "Passed induction in Jersey", IsActive = true, RouteImplicitExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = new("3471ab35-e6e4-4fa9-a72b-b8bd113df591"), Name = "Passed induction in Northern Ireland", IsActive = true, RouteImplicitExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = new("7d17d904-c1c6-451b-9e09-031314bd35f7"), Name = "Passed induction in Service Children's Education schools in Germany or Cyprus", IsActive = true, RouteImplicitExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.PassedInWalesId, Name = "Passed induction in Wales", IsActive = true, RouteImplicitExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = new("a751494a-7e7a-4836-96cb-00b9ed6e1b5f"), Name = "Passed probationary period in Gibraltar", IsActive = true, RouteImplicitExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = new("a5faff9f-29ce-4a6b-a7b8-0c1f57f15920"), Name = "Exempt", IsActive = true, RouteImplicitExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = new("204f86eb-0383-40eb-b793-6fccb76ecee2"), Name = "Exempt - Data Loss/Error Criteria", IsActive = true, RouteImplicitExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = new("a112e691-1694-46a7-8f33-5ec5b845c181"), Name = "Has, or is eligible for, full registration in Scotland", IsActive = true, RouteImplicitExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = new("4c97e211-10d2-4c63-8da9-b0fcebe7f2f9"), Name = "Overseas Trained Teacher", IsActive = true, RouteImplicitExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = new("e7118bab-c2b1-4fe8-ad3f-4095d73f5b85"), Name = "Qualified through EEA mutual recognition route", IsActive = true, RouteImplicitExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = new("42bb7bbc-a92c-4886-b319-3c1a5eac319a"), Name = "Registered teacher with at least 2 years full-time teaching experience", IsActive = true, RouteImplicitExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.QtlsId, Name = "Exempt through QTLS status provided they maintain membership of The Society of Education and Training", IsActive = true, RouteImplicitExemption = true });
    }
}
