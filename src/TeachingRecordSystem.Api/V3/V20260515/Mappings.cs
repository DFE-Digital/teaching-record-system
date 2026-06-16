using TeachingRecordSystem.Core.ApiSchema.V3.V20260515.Dtos;
using Common = TeachingRecordSystem.Api.V3.Operations.Common;
using Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20260515.Dtos;

namespace TeachingRecordSystem.Api.V3.V20260515;

public static class TrnRequestInfoMappingExtensions
{
    extension(Dtos.TrnRequestInfo)
    {
        public static Dtos.TrnRequestInfo Create(Common.TrnRequestInfo source) => new()
        {
            RequestId = source.RequestId,
            OneLoginUserSubject = source.OneLoginUserSubject,
            Status = Dtos.TrnRequestStatus.Create(source.Status),
            Trn = source.Trn,
            PotentialDuplicate = source.PotentialDuplicate,
            AccessYourTeachingQualificationsLink = source.AccessYourTeachingQualificationsLink
        };
    }
}
