using TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos;
using Common = TeachingRecordSystem.Api.V3.Operations.Common;
using Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos;

namespace TeachingRecordSystem.Api.V3.V20250203;

public static class GenderModelMappingExtensions
{
    extension(Core.Models.Gender)
    {
        public static Core.Models.Gender Create(Dtos.Gender source) => source switch
        {
            Dtos.Gender.Male => Core.Models.Gender.Male,
            Dtos.Gender.Female => Core.Models.Gender.Female,
            Dtos.Gender.Other => Core.Models.Gender.Other,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }
}

public static class InductionInfoMappingExtensions
{
    extension(Dtos.InductionInfo)
    {
        public static Dtos.InductionInfo Create(Common.InductionInfo source) => new()
        {
            Status = Dtos.InductionStatus.Create(source.Status),
            StartDate = source.StartDate,
            CompletedDate = source.CompletedDate
        };
    }
}

public static class InductionStatusModelMappingExtensions
{
    extension(Core.Models.InductionStatus)
    {
        public static Core.Models.InductionStatus Create(Dtos.InductionStatus source) => source switch
        {
            Dtos.InductionStatus.None => Core.Models.InductionStatus.None,
            Dtos.InductionStatus.RequiredToComplete => Core.Models.InductionStatus.RequiredToComplete,
            Dtos.InductionStatus.Exempt => Core.Models.InductionStatus.Exempt,
            Dtos.InductionStatus.InProgress => Core.Models.InductionStatus.InProgress,
            Dtos.InductionStatus.Passed => Core.Models.InductionStatus.Passed,
            Dtos.InductionStatus.Failed => Core.Models.InductionStatus.Failed,
            Dtos.InductionStatus.FailedInWales => Core.Models.InductionStatus.FailedInWales,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }
}
