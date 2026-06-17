using Common = TeachingRecordSystem.Api.V3.Operations.Common;
using Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240101;

public static class NameInfoMappingExtensions
{
    extension(Dtos.NameInfo)
    {
        public static Dtos.NameInfo Create(Common.NameInfo source) => new()
        {
            FirstName = source.FirstName,
            MiddleName = source.MiddleName,
            LastName = source.LastName
        };
    }
}

public static class SanctionInfoMappingExtensions
{
    extension(Dtos.SanctionInfo)
    {
        public static Dtos.SanctionInfo Create(Common.SanctionInfo source) => new()
        {
            Code = source.Code,
            StartDate = source.StartDate
        };
    }
}

public static class DqtInductionStatusMappingExtensions
{
    extension(Dtos.DqtInductionStatus)
    {
        public static Dtos.DqtInductionStatus Create(Common.DqtInductionStatus source) => source switch
        {
            Common.DqtInductionStatus.Exempt => Dtos.DqtInductionStatus.Exempt,
            Common.DqtInductionStatus.Fail => Dtos.DqtInductionStatus.Fail,
            Common.DqtInductionStatus.FailedInWales => Dtos.DqtInductionStatus.FailedinWales,
            Common.DqtInductionStatus.InductionExtended => Dtos.DqtInductionStatus.InductionExtended,
            Common.DqtInductionStatus.InProgress => Dtos.DqtInductionStatus.InProgress,
            Common.DqtInductionStatus.NotYetCompleted => Dtos.DqtInductionStatus.NotYetCompleted,
            Common.DqtInductionStatus.Pass => Dtos.DqtInductionStatus.Pass,
            Common.DqtInductionStatus.PassedInWales => Dtos.DqtInductionStatus.PassedinWales,
            Common.DqtInductionStatus.RequiredToComplete => Dtos.DqtInductionStatus.RequiredtoComplete,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }
}
