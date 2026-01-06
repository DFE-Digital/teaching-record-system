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
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.QualifiedBefore7May2000Id, Name = "They qualified before 07 May 2000", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.QualifiedBetween7May1999And1April2003FirstPostInWalesId, Name = "They qualified between 7 May 1999 and 1 April 2003 and first taught in Wales for at least 2 terms", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.QualifiedThroughFurtherEducationRouteBetween1Sep2001And1Sep2004Id, Name = "They qualified through a further education route between 1 September 2001 and 1 September 2004", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.PassedInductionInGuernseyId, Name = "They passed induction in Guernsey", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.PassedInductionInIsleOfManId, Name = "They passed induction in the Isle of Man", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.PassedInductionInJerseyId, Name = "They passed induction in Jersey", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.PassedInductionInNorthernIrelandId, Name = "They passed induction in Northern Ireland", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.PassedInductionInServiceChildrensEducationSchoolsInGermanyOrCyprusId, Name = "They passed induction in Service Children’s Education schools in Germany or Cyprus", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.PassedInWalesId, Name = "They passed induction in Wales", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.PassedProbationaryPeriodInGibraltarId, Name = "They passed their probationary period in Gibraltar", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.ExemptId, Name = "Exempt", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.ExemptDataLossOrErrorCriteriaId, Name = "Exempt due to data loss or system error", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.HasOrIsEligibleForFullRegistrationInScotlandId, Name = "They have or are eligible for full registration in Scotland", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.OverseasTrainedTeacherId, Name = "Overseas Trained Teacher", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = true },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.QualifiedThroughEeaMutualRecognitionRouteId, Name = "They qualified through a European Economic Area (EEA) mutual recognition route", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.RegisteredTeacherWithAtLeast2YearsFullTimeTeachingExperienceId, Name = "They’re a registered teacher with at least 2 years’ full-time teaching experience", IsActive = true, RouteImplicitExemption = false, RouteOnlyExemption = false },
            new InductionExemptionReason { InductionExemptionReasonId = InductionExemptionReason.QtlsId, Name = "Exempt through QTLS status provided they maintain membership of The Society of Education and Training", IsActive = true, RouteImplicitExemption = true, RouteOnlyExemption = true });
    }
}
