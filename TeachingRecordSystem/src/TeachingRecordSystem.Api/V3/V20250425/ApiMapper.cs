using Riok.Mapperly.Abstractions;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using V20240606Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20240606.Dtos;
using V20250203Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos;
using V20250425Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20250425.Dtos;

namespace TeachingRecordSystem.Api.V3.V20250425;

[Mapper]
public partial class ApiMapper
{
    public V20250425Dtos.TrnRequestInfo MapTrnRequestInfo(TrnRequestInfo source) =>
        new()
        {
            RequestId = source.RequestId,
            Status = (V20240606Dtos.TrnRequestStatus)(int)source.Status,
            Trn = source.Trn,
            PotentialDuplicate = source.PotentialDuplicate,
            AccessYourTeachingQualificationsLink = source.AccessYourTeachingQualificationsLink
        };

    public Core.Models.Gender MapGender(V20250203Dtos.Gender source) =>
        (Core.Models.Gender)(int)source;
}
