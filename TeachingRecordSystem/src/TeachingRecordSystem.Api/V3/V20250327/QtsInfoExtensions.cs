using CoreQtsInfo = TeachingRecordSystem.Api.V3.Implementation.Dtos.QtsInfo;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250327.Dtos;

public static class QtsInfoExtensions
{
    public static QtsInfo? FromModel(this CoreQtsInfo? model) =>
        model is null ? null : new()
        {
            Awarded = model.HoldsFrom,
            StatusDescription = model.StatusDescription,
            AwardedOrApprovedCount = model.AwardedOrApprovedCount
        };
}
