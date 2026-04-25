using CoreDqtInductionStatus = TeachingRecordSystem.Api.V3.Implementation.Dtos.DqtInductionStatus;
using CoreDqtInductionStatusInfo = TeachingRecordSystem.Api.V3.Implementation.Dtos.DqtInductionStatusInfo;
using CoreEytsInfo = TeachingRecordSystem.Api.V3.Implementation.Dtos.EytsInfo;
using CoreQtsInfo = TeachingRecordSystem.Api.V3.Implementation.Dtos.QtsInfo;
using DqtInductionStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos;

public static class QtsInfoExtensions
{
    public static QtsInfo? FromModel(this CoreQtsInfo? model) =>
        model is null ? null : new() { Awarded = model.HoldsFrom, StatusDescription = model.StatusDescription };
}

public static class EytsInfoExtensions
{
    public static EytsInfo? FromModel(this CoreEytsInfo? model) =>
        model is null ? null : new() { Awarded = model.HoldsFrom, StatusDescription = model.StatusDescription };
}

public static class DqtInductionStatusInfoExtensions
{
    public static DqtInductionStatusInfo? FromModel(this CoreDqtInductionStatusInfo? model) =>
        model is null ? null : new()
        {
            Status = model.Status switch
            {
                CoreDqtInductionStatus.Exempt => DqtInductionStatus.Exempt,
                CoreDqtInductionStatus.Fail => DqtInductionStatus.Fail,
                CoreDqtInductionStatus.FailedInWales => DqtInductionStatus.FailedinWales,
                CoreDqtInductionStatus.InductionExtended => DqtInductionStatus.InductionExtended,
                CoreDqtInductionStatus.InProgress => DqtInductionStatus.InProgress,
                CoreDqtInductionStatus.NotYetCompleted => DqtInductionStatus.NotYetCompleted,
                CoreDqtInductionStatus.Pass => DqtInductionStatus.Pass,
                CoreDqtInductionStatus.PassedInWales => DqtInductionStatus.PassedinWales,
                CoreDqtInductionStatus.RequiredToComplete => DqtInductionStatus.RequiredtoComplete,
                _ => throw new ArgumentOutOfRangeException(nameof(model.Status))
            },
            StatusDescription = model.StatusDescription
        };
}
