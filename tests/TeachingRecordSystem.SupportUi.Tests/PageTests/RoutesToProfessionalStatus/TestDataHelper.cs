using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus;

public static class TestDataHelper
{
    public static async Task<RouteToProfessionalStatusType> GetRouteWhereAllFieldsApplyAsync(
        this ReferenceDataCache referenceDataCache,
        ProfessionalStatusType? professionalStatusType = null)
    {
        return (await referenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingAgeSpecialismTypeRequired != FieldRequirement.NotApplicable
                && r.TrainingCountryRequired != FieldRequirement.NotApplicable
                && r.TrainingProviderRequired != FieldRequirement.NotApplicable
                && r.DegreeTypeRequired != FieldRequirement.NotApplicable
                && r.TrainingSubjectsRequired != FieldRequirement.NotApplicable
                && r.InductionExemptionRequired != FieldRequirement.NotApplicable
                && r.TrainingStartDateRequired != FieldRequirement.NotApplicable
                && r.TrainingEndDateRequired != FieldRequirement.NotApplicable
                && r.HoldsFromRequired != FieldRequirement.NotApplicable)
            .Where(r => professionalStatusType is null || professionalStatusType == r.ProfessionalStatusType)
            .SingleRandom();
    }

    public static async Task<RouteToProfessionalStatusType> GetRouteWhereAllFieldsHaveFieldRequirementAsync(this ReferenceDataCache referenceDataCache, FieldRequirement fieldRequirement)
    {
        return (await referenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingAgeSpecialismTypeRequired == fieldRequirement
                && r.TrainingCountryRequired == fieldRequirement
                && r.TrainingProviderRequired == fieldRequirement
                && r.DegreeTypeRequired == fieldRequirement
                && r.TrainingSubjectsRequired == fieldRequirement
                && r.InductionExemptionRequired == fieldRequirement
                && r.TrainingStartDateRequired == fieldRequirement
                && r.TrainingEndDateRequired == fieldRequirement)
            .SingleRandom();
    }

    public static RouteToProfessionalStatusStatus GetRouteStatusWhereAllFieldsApply()
    {
        return ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingAgeSpecialismTypeRequired != FieldRequirement.NotApplicable
                && s.HoldsFromRequired != FieldRequirement.NotApplicable
                && s.TrainingCountryRequired != FieldRequirement.NotApplicable
                && s.DegreeTypeRequired != FieldRequirement.NotApplicable
                && s.TrainingEndDateRequired != FieldRequirement.NotApplicable
                && s.InductionExemptionRequired != FieldRequirement.NotApplicable
                && s.TrainingProviderRequired != FieldRequirement.NotApplicable
                && s.TrainingStartDateRequired != FieldRequirement.NotApplicable
                && s.TrainingSubjectsRequired != FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
    }

    public static Func<T, bool> PropertyHasFieldRequirement<T>(string propertyName, FieldRequirement expectedValue)
    {
        return item =>
        {
            var property = typeof(T).GetProperty(propertyName);
            if (property is null)
            {
                throw new InvalidOperationException($"Property '{propertyName}' not found on type {typeof(T).Name}.");
            }
            var actualValue = property.GetValue(item);
            return actualValue?.Equals(expectedValue) ?? false;
        };
    }
}
