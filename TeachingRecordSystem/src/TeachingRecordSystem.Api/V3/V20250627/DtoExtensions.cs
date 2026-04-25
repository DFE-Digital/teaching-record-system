using CoreEytsInfo = TeachingRecordSystem.Api.V3.Implementation.Dtos.EytsInfo;
using CoreInductionInfo = TeachingRecordSystem.Api.V3.Implementation.Dtos.InductionInfo;
using CoreQtsInfo = TeachingRecordSystem.Api.V3.Implementation.Dtos.QtsInfo;
using CoreTrainingAgeSpecialism = TeachingRecordSystem.Api.V3.Implementation.Dtos.TrainingAgeSpecialism;
using CoreTrainingCountry = TeachingRecordSystem.Api.V3.Implementation.Dtos.TrainingCountry;
using DtoTrainingAgeSpecialismType = TeachingRecordSystem.Core.ApiSchema.V3.V20250425.Dtos.TrainingAgeSpecialismType;
using InductionStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.InductionStatus;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos;

public static class RouteToProfessionalStatusTypeExtensions
{
    public static RouteToProfessionalStatusType FromModel(this PostgresModels.RouteToProfessionalStatusType m) => new()
    {
        RouteToProfessionalStatusTypeId = m.RouteToProfessionalStatusTypeId,
        Name = m.Name,
        ProfessionalStatusType = (ProfessionalStatusType)(int)m.ProfessionalStatusType
    };
}

public static class InductionExemptionReasonExtensions
{
    public static InductionExemptionReason FromModel(this PostgresModels.InductionExemptionReason m) => new()
    {
        InductionExemptionReasonId = m.InductionExemptionReasonId,
        Name = m.Name
    };
}

public static class TrainingSubjectExtensions
{
    public static TrainingSubject FromModel(this PostgresModels.TrainingSubject m) => new()
    {
        Reference = m.Reference,
        Name = m.Name
    };
}

public static class TrainingProviderExtensions
{
    public static TrainingProvider? FromModel(this PostgresModels.TrainingProvider? m) =>
        m is null ? null : new() { Ukprn = m.Ukprn ?? string.Empty, Name = m.Name };
}

public static class DegreeTypeExtensions
{
    public static DegreeType? FromModel(this PostgresModels.DegreeType? m) =>
        m is null ? null : new() { DegreeTypeId = m.DegreeTypeId, Name = m.Name };
}

public static class TrainingCountryExtensions
{
    public static TrainingCountry? FromModel(this CoreTrainingCountry? m) =>
        m is null ? null : new() { Reference = m.Reference, Name = m.Name };
}

public static class TrainingAgeSpecialismExtensions
{
    public static TrainingAgeSpecialism? FromModel(this CoreTrainingAgeSpecialism? m) =>
        m is null || m.Type is null ? null : new()
        {
            Type = (DtoTrainingAgeSpecialismType)(int)m.Type.Value,
            From = m.From,
            To = m.To
        };
}

public static class QtsInfoExtensions
{
    public static QtsInfo? FromModel(this CoreQtsInfo? m) =>
        m is null ? null : new()
        {
            HoldsFrom = m.HoldsFrom,
            Routes = m.Routes.Select(r => new QtsInfoRoute { RouteToProfessionalStatusType = r.RouteToProfessionalStatusType.FromModel() }).ToArray()
        };
}

public static class EytsInfoExtensions
{
    public static EytsInfo? FromModel(this CoreEytsInfo? m) =>
        m is null ? null : new()
        {
            HoldsFrom = m.HoldsFrom,
            Routes = m.Routes.Select(r => new EytsInfoRoute { RouteToProfessionalStatusType = r.RouteToProfessionalStatusType.FromModel() }).ToArray()
        };
}

public static class InductionInfoExtensions
{
    public static InductionInfo? FromModel(this CoreInductionInfo? m) =>
        m is null ? null : new()
        {
            Status = (InductionStatus)(int)m.Status,
            StartDate = m.StartDate,
            CompletedDate = m.CompletedDate,
            ExemptionReasons = m.ExemptionReasons.Select(r => r.FromModel()).ToArray()
        };
}
