using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests;

// Route types that only exist for tests. These are seeded into the template database rather than added by a
// startup task, so every test's database has them; a startup task would only populate whichever database
// happened to be leased when the host started.
public static class AddTestRouteTypes
{
    private static readonly RouteToProfessionalStatusType[] _testRoutes =
    [
        new()
        {
            RouteToProfessionalStatusTypeId = new Guid("b7f3a1c2-5d64-4e18-9c3b-1a2f4e6d8b90"),
            Name = "Test Route With NotApplicable Country",
            ProfessionalStatusType = ProfessionalStatusType.QualifiedTeacherStatus,
            IsActive = true,
            TrainingStartDateRequired = FieldRequirement.Optional,
            TrainingEndDateRequired = FieldRequirement.Optional,
            HoldsFromRequired = FieldRequirement.Optional,
            InductionExemptionRequired = FieldRequirement.Optional,
            TrainingProviderRequired = FieldRequirement.Optional,
            DegreeTypeRequired = FieldRequirement.Optional,
            TrainingCountryRequired = FieldRequirement.NotApplicable,
            TrainingAgeSpecialismTypeRequired = FieldRequirement.Optional,
            TrainingSubjectsRequired = FieldRequirement.Optional,
            InductionExemptionReasonId = null
        },
        new()
        {
            RouteToProfessionalStatusTypeId = new Guid("c9e5d2b4-7a81-4f26-8d05-3b7c9e1f4a52"),
            Name = "Test Route With Mandatory Start/End Dates",
            ProfessionalStatusType = ProfessionalStatusType.QualifiedTeacherStatus,
            IsActive = true,
            TrainingStartDateRequired = FieldRequirement.Mandatory,
            TrainingEndDateRequired = FieldRequirement.Mandatory,
            HoldsFromRequired = FieldRequirement.Optional,
            InductionExemptionRequired = FieldRequirement.NotApplicable,
            TrainingProviderRequired = FieldRequirement.Optional,
            DegreeTypeRequired = FieldRequirement.Optional,
            TrainingCountryRequired = FieldRequirement.Optional,
            TrainingAgeSpecialismTypeRequired = FieldRequirement.Optional,
            TrainingSubjectsRequired = FieldRequirement.Optional,
            InductionExemptionReasonId = null
        }
    ];

    public static async Task SeedAsync(TrsDbContext dbContext)
    {
        dbContext.RouteToProfessionalStatusTypes.AddRange(_testRoutes);
        await dbContext.SaveChangesAsync();
    }
}
