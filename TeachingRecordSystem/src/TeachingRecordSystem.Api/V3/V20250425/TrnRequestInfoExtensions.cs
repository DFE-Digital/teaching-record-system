using CoreTrnRequestInfo = TeachingRecordSystem.Api.V3.Implementation.Dtos.TrnRequestInfo;
using TrnRequestStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20240606.Dtos.TrnRequestStatus;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250425.Dtos;

public static class TrnRequestInfoExtensions
{
    public static TrnRequestInfo FromModel(this CoreTrnRequestInfo model) => new()
    {
        RequestId = model.RequestId,
        Status = (TrnRequestStatus)(int)model.Status,
        Trn = model.Trn,
        PotentialDuplicate = model.PotentialDuplicate,
        AccessYourTeachingQualificationsLink = model.AccessYourTeachingQualificationsLink
    };
}
