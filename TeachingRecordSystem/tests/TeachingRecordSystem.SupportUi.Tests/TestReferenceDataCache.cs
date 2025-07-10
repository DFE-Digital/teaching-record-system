using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.SupportUi.Tests;

public class TestReferenceDataCache(ICrmQueryDispatcher crmQueryDispatcher, IDbContextFactory<TrsDbContext> dbContextFactory)
    : ReferenceDataCache(crmQueryDispatcher, dbContextFactory)
{
    private RouteToProfessionalStatusType[] _testRoutes = [
        new() {
            RouteToProfessionalStatusTypeId = Guid.NewGuid(),
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
        new() {
            RouteToProfessionalStatusTypeId = Guid.NewGuid(),
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
        },
        new() {
            RouteToProfessionalStatusTypeId = Guid.NewGuid(),
            Name = "Test Route With Optional HoldsFrom Date",
            ProfessionalStatusType = ProfessionalStatusType.QualifiedTeacherStatus,
            IsActive = true,
            TrainingStartDateRequired = FieldRequirement.Optional,
            TrainingEndDateRequired = FieldRequirement.Optional,
            HoldsFromRequired = FieldRequirement.Optional,
            InductionExemptionRequired = FieldRequirement.Optional,
            TrainingProviderRequired = FieldRequirement.Optional,
            DegreeTypeRequired = FieldRequirement.Optional,
            TrainingCountryRequired = FieldRequirement.Optional,
            TrainingAgeSpecialismTypeRequired = FieldRequirement.Optional,
            TrainingSubjectsRequired = FieldRequirement.Optional,
            InductionExemptionReasonId = null
        }
    ];

    protected override async Task<RouteToProfessionalStatusType[]> InitializeRouteToProfessionalStatusTypesAsync()
    {
        using var dbContext = DbContextFactory.CreateDbContext();

        var testRouteNames = _testRoutes.Select(tr => tr.Name).ToArray();

        var existingTestRoutes = await dbContext.RouteToProfessionalStatusTypes
            .Where(r => testRouteNames.Contains(r.Name))
            .ToArrayAsync();

        foreach (var testRoute in _testRoutes)
        {
            var existingRoute = _testRoutes.SingleOrDefault(r => r.Name == testRoute.Name);
            if (existingRoute is null)
            {
                dbContext.RouteToProfessionalStatusTypes.Add(testRoute);
            }
            else if (existingRoute.ProfessionalStatusType != testRoute.ProfessionalStatusType ||
                existingRoute.IsActive != testRoute.IsActive ||
                existingRoute.TrainingStartDateRequired != testRoute.TrainingStartDateRequired ||
                existingRoute.TrainingEndDateRequired != testRoute.TrainingEndDateRequired ||
                existingRoute.HoldsFromRequired != testRoute.HoldsFromRequired ||
                existingRoute.InductionExemptionRequired != testRoute.InductionExemptionRequired ||
                existingRoute.TrainingProviderRequired != testRoute.TrainingProviderRequired ||
                existingRoute.DegreeTypeRequired != testRoute.DegreeTypeRequired ||
                existingRoute.TrainingCountryRequired != testRoute.TrainingCountryRequired ||
                existingRoute.TrainingAgeSpecialismTypeRequired != testRoute.TrainingAgeSpecialismTypeRequired ||
                existingRoute.TrainingSubjectsRequired != testRoute.TrainingSubjectsRequired ||
                existingRoute.InductionExemptionReasonId != testRoute.InductionExemptionReasonId)
            {
                dbContext.RouteToProfessionalStatusTypes.Remove(existingRoute);
                dbContext.RouteToProfessionalStatusTypes.Add(testRoute);
            }
        }

        await dbContext.SaveChangesAsync();

        return await dbContext.RouteToProfessionalStatusTypes.AsNoTracking().ToArrayAsync();
    }
}
