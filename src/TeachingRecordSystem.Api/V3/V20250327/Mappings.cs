using Common = TeachingRecordSystem.Api.V3.Operations.Common;
using Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20250327.Dtos;

namespace TeachingRecordSystem.Api.V3.V20250327;

public static class QtsInfoMappingExtensions
{
    extension(Dtos.QtsInfo)
    {
        public static Dtos.QtsInfo Create(Common.QtsInfo source) => new()
        {
            Awarded = source.HoldsFrom,
            StatusDescription = source.StatusDescription,
            AwardedOrApprovedCount = source.AwardedOrApprovedCount
        };
    }
}
