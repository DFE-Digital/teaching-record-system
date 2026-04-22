using CoreQtlsResult = TeachingRecordSystem.Api.V3.Implementation.Dtos.QtlsResult;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20240912.Dtos;

public static class QtlsResponseExtensions
{
    public static QtlsResponse FromModel(this CoreQtlsResult model) => new()
    {
        QtsDate = model.QtsDate,
        Trn = model.Trn
    };
}
