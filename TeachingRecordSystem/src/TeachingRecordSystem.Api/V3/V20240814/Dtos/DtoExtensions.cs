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
                Implementation.Dtos.DqtInductionStatus.Exempt => DqtInductionStatus.Exempt,
                Implementation.Dtos.DqtInductionStatus.Fail => DqtInductionStatus.Fail,
                Implementation.Dtos.DqtInductionStatus.FailedInWales => DqtInductionStatus.FailedinWales,
                Implementation.Dtos.DqtInductionStatus.InductionExtended => DqtInductionStatus.InductionExtended,
                Implementation.Dtos.DqtInductionStatus.InProgress => DqtInductionStatus.InProgress,
                Implementation.Dtos.DqtInductionStatus.NotYetCompleted => DqtInductionStatus.NotYetCompleted,
                Implementation.Dtos.DqtInductionStatus.Pass => DqtInductionStatus.Pass,
                Implementation.Dtos.DqtInductionStatus.PassedInWales => DqtInductionStatus.PassedinWales,
                Implementation.Dtos.DqtInductionStatus.RequiredToComplete => DqtInductionStatus.RequiredtoComplete,
                _ => throw new ArgumentOutOfRangeException(nameof(model.Status))
            },
            StatusDescription = model.StatusDescription
        };
}
