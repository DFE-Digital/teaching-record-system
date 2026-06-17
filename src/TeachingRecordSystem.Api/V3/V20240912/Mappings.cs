using Common = TeachingRecordSystem.Api.V3.Operations.Common;
using Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20240912.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240912;

public static class QtlsResponseMappingExtensions
{
    extension(Dtos.QtlsResponse)
    {
        public static Dtos.QtlsResponse Create(Common.QtlsResult source) => new()
        {
            QtsDate = source.QtsDate,
            Trn = source.Trn
        };
    }
}
