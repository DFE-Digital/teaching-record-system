using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus;

public static class TestDataHelper
{
    public static async Task<RouteToProfessionalStatus> GetRouteWhereAllFieldsApplyAsync(this ReferenceDataCache referenceDataCache)
    {
        return (await referenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingAgeSpecialismTypeRequired != FieldRequirement.NotRequired
                && r.TrainingCountryRequired != FieldRequirement.NotRequired
                && r.TrainingProviderRequired != FieldRequirement.NotRequired
                && r.DegreeTypeRequired != FieldRequirement.NotRequired
                && r.TrainingSubjectsRequired != FieldRequirement.NotRequired
                && r.InductionExemptionRequired != FieldRequirement.NotRequired
                && r.TrainingStartDateRequired != FieldRequirement.NotRequired
                && r.TrainingEndDateRequired != FieldRequirement.NotRequired)
            .RandomOne();
    }

    public static async Task<RouteToProfessionalStatus> GetRouteWhereAllFieldsHaveFieldRequirementAsync(this ReferenceDataCache referenceDataCache, FieldRequirement fieldRequirement)
    {
        return (await referenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingAgeSpecialismTypeRequired == fieldRequirement
                && r.TrainingCountryRequired == fieldRequirement
                && r.TrainingProviderRequired == fieldRequirement
                && r.DegreeTypeRequired == fieldRequirement
                && r.TrainingSubjectsRequired == fieldRequirement
                && r.InductionExemptionRequired == fieldRequirement
                && r.TrainingStartDateRequired == fieldRequirement
                && r.TrainingEndDateRequired == fieldRequirement)
            .RandomOne();
    }

    public static ProfessionalStatusStatus GetRouteStatusWhereAllFieldsApply(this ReferenceDataCache referenceDataCache)
    {
        return ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingAgeSpecialismTypeRequired != FieldRequirement.NotRequired
                && s.AwardDateRequired != FieldRequirement.NotRequired
                && s.TrainingCountryRequired != FieldRequirement.NotRequired
                && s.DegreeTypeRequired != FieldRequirement.NotRequired
                && s.TrainingEndDateRequired != FieldRequirement.NotRequired
                && s.InductionExemptionRequired != FieldRequirement.NotRequired
                && s.TrainingProviderRequired != FieldRequirement.NotRequired
                && s.TrainingStartDateRequired != FieldRequirement.NotRequired
                && s.TrainingSubjectsRequired != FieldRequirement.NotRequired)
            .RandomOne()
            .Value;
    }

    public static Func<T, bool> PropertyHasFieldRequirement<T>(string propertyName, FieldRequirement expectedValue)
    {
        return item =>
        {
            var property = typeof(T).GetProperty(propertyName);
            if (property is null)
            {
                throw new InvalidOperationException($"Property {propertyName} not found on type RouteToProfessionalStatus");
            }
            var actualValue = property.GetValue(item);
            return actualValue?.Equals(expectedValue) ?? false;
        };
    }
}
