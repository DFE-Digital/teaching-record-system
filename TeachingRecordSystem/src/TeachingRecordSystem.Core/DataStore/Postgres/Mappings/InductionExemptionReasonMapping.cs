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
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.QualifiedBefore07May2000Id, Name = "Qualified before 07 May 2000", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.QualifiedBetween07May1999And01April2003FirstPostInWalesId, Name = "Qualified between 07 May 1999 and 01 Apr 2003. First post was in Wales and lasted a minimum of two terms.", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.QualifiedThroughFurtherEducationRouteBetween1Sep2001And1Sep2004Id, Name = "Qualified through Further Education route between 1 Sep 2001 and 1 Sep 2004", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.PassedInductionInGuernseyId, Name = "Passed induction in Guernsey", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.PassedInductionInIsleOfManId, Name = "Passed induction in Isle of Man", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.PassedInductionInJerseyId, Name = "Passed induction in Jersey", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.PassedInductionInNorthernIrelandId, Name = "Passed induction in Northern Ireland", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.PassedInductionInServiceChildrensEducationSchoolsInGermanyOrCyprusId, Name = "Passed induction in Service Children's Education schools in Germany or Cyprus", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.PassedInWalesId, Name = "Passed induction in Wales", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.PassedProbationaryPeriodInGibraltarId, Name = "Passed probationary period in Gibraltar", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.ExemptId, Name = "Exempt", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.ExemptDataLossOrErrorCriteriaId, Name = "Exempt - Data Loss/Error Criteria", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.HasOrIsEligibleForFullRegistrationInScotlandId, Name = "Has, or is eligible for, full registration in Scotland", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.OverseasTrainedTeacherId, Name = "Overseas Trained Teacher", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = true },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.QualifiedThroughEEAMutualRecognitionRouteId, Name = "Qualified through EEA mutual recognition route", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.RegisteredTeacherWithAtLeast2YearsFullTimeTeachingExperienceneId, Name = "Registered teacher with at least 2 years full-time teaching experience", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.QtlsId, Name = "Exempt through QTLS status provided they maintain membership of The Society of Education and Training", IsActive = true, RouteImplicitExemption = true, RouteOnlyExemption = true });
    }
}
