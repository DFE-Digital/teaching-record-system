using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus;

public static class TestDataHelper
{
    public static async Task<RouteToProfessionalStatus> GetRouteWhereAllFieldsApplyASync(ReferenceDataCache referenceDataCache)
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

    public static ProfessionalStatusStatus GetStatusWhereAllFieldsApply()
    {
        return ProfessionalStatusStatusRegistry.All
            .Where(s => s.AgeRange != FieldRequirement.NotRequired
                && s.AwardDate != FieldRequirement.NotRequired
                && s.Country != FieldRequirement.NotRequired
                && s.DegreeType != FieldRequirement.NotRequired
                && s.EndDate != FieldRequirement.NotRequired
                && s.InductionExemption != FieldRequirement.NotRequired
                && s.TrainingProvider != FieldRequirement.NotRequired
                && s.StartDate != FieldRequirement.NotRequired
                && s.Subjects != FieldRequirement.NotRequired)
            .RandomOne()
            .Value;
    }

    public static Func<RouteToProfessionalStatus, bool> PropertyHasFieldRequirement<T>(string propertyName, FieldRequirement expectedValue)
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
