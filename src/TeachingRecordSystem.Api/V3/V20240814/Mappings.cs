using TeachingRecordSystem.Api.V3.V20240101;
using Common = TeachingRecordSystem.Api.V3.Operations.Common;
using Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos;
using V20240101Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240814;

public static class QtsInfoMappingExtensions
{
    extension(Dtos.QtsInfo)
    {
        public static Dtos.QtsInfo Create(Common.QtsInfo source) => new()
        {
            Awarded = source.HoldsFrom,
            StatusDescription = source.StatusDescription
        };
    }
}

public static class EytsInfoMappingExtensions
{
    extension(Dtos.EytsInfo)
    {
        public static Dtos.EytsInfo Create(Common.EytsInfo source) => new()
        {
            Awarded = source.HoldsFrom,
            StatusDescription = source.StatusDescription
        };
    }
}

public static class DqtInductionStatusInfoMappingExtensions
{
    extension(Dtos.DqtInductionStatusInfo)
    {
        public static Dtos.DqtInductionStatusInfo Create(Common.DqtInductionStatusInfo source) => new()
        {
            Status = V20240101Dtos.DqtInductionStatus.Create(source.Status),
            StatusDescription = source.StatusDescription
        };
    }
}
