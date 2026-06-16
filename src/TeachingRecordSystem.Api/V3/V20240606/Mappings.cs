using TeachingRecordSystem.Core.ApiSchema.V3.V20240606.Dtos;
using Common = TeachingRecordSystem.Api.V3.Operations.Common;
using Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20240606.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240606;

public static class TrnRequestInfoMappingExtensions
{
    extension(Dtos.TrnRequestInfo)
    {
        public static Dtos.TrnRequestInfo Create(Common.TrnRequestInfo source) => new()
        {
            RequestId = source.RequestId,
            Status = Dtos.TrnRequestStatus.Create(source.Status),
            Trn = source.Trn
        };
    }
}
