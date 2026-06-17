using TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos;
using Common = TeachingRecordSystem.Api.V3.Operations.Common;
using DtoInductionStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.InductionStatus;
using Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos;
using ModelsRouteToProfessionalStatusStatus = TeachingRecordSystem.Core.Models.RouteToProfessionalStatusStatus;

namespace TeachingRecordSystem.Api.V3.V20250627;

public static class InductionInfoMappingExtensions
{
    extension(Dtos.InductionInfo)
    {
        public static Dtos.InductionInfo Create(Common.InductionInfo source) => new()
        {
            Status = DtoInductionStatus.Create(source.Status),
            StartDate = source.StartDate,
            CompletedDate = source.CompletedDate,
            ExemptionReasons = source.ExemptionReasons.Select(r => Dtos.InductionExemptionReason.Create(r)).AsReadOnly()
        };
    }
}

public static class QtsInfoMappingExtensions
{
    extension(Dtos.QtsInfo)
    {
        public static Dtos.QtsInfo Create(Common.QtsInfo source) => new()
        {
            HoldsFrom = source.HoldsFrom,
            Routes = source.Routes.Select(r => Dtos.QtsInfoRoute.Create(r)).AsReadOnly()
        };
    }
}

public static class QtsInfoRouteMappingExtensions
{
    extension(Dtos.QtsInfoRoute)
    {
        public static Dtos.QtsInfoRoute Create(Common.QtsInfoRoute source) => new()
        {
            RouteToProfessionalStatusType = Dtos.RouteToProfessionalStatusType.Create(source.RouteToProfessionalStatusType)
        };
    }
}

public static class EytsInfoMappingExtensions
{
    extension(Dtos.EytsInfo)
    {
        public static Dtos.EytsInfo Create(Common.EytsInfo source) => new()
        {
            HoldsFrom = source.HoldsFrom,
            Routes = source.Routes.Select(r => Dtos.EytsInfoRoute.Create(r)).AsReadOnly()
        };
    }
}

public static class EytsInfoRouteMappingExtensions
{
    extension(Dtos.EytsInfoRoute)
    {
        public static Dtos.EytsInfoRoute Create(Common.EytsInfoRoute source) => new()
        {
            RouteToProfessionalStatusType = Dtos.RouteToProfessionalStatusType.Create(source.RouteToProfessionalStatusType)
        };
    }
}

public static class RouteToProfessionalStatusStatusModelMappingExtensions
{
    extension(ModelsRouteToProfessionalStatusStatus)
    {
        public static ModelsRouteToProfessionalStatusStatus Create(Dtos.RouteToProfessionalStatusStatus source) => source switch
        {
            Dtos.RouteToProfessionalStatusStatus.InTraining => ModelsRouteToProfessionalStatusStatus.InTraining,
            Dtos.RouteToProfessionalStatusStatus.Holds => ModelsRouteToProfessionalStatusStatus.Holds,
            Dtos.RouteToProfessionalStatusStatus.Deferred => ModelsRouteToProfessionalStatusStatus.Deferred,
            Dtos.RouteToProfessionalStatusStatus.DeferredForSkillsTest => ModelsRouteToProfessionalStatusStatus.DeferredForSkillsTest,
            Dtos.RouteToProfessionalStatusStatus.Failed => ModelsRouteToProfessionalStatusStatus.Failed,
            Dtos.RouteToProfessionalStatusStatus.Withdrawn => ModelsRouteToProfessionalStatusStatus.Withdrawn,
            Dtos.RouteToProfessionalStatusStatus.UnderAssessment => ModelsRouteToProfessionalStatusStatus.UnderAssessment,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }
}
